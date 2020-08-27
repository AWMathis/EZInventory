using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Management;
using System.Net;
using EZInventory.UserControls;
using System.Net.NetworkInformation;
using System.Threading;
using System.ComponentModel;
using EZInventory.InfoClasses;
using Microsoft.Win32;


/*
 * Add enter to search

Lookup by IP only doesn’t work. When name is blank it searches with localhost still

IP address is kinda formatted weird… for exm014 it’s “172.21.57.99InterNetwork;”  for mine “fe80::21f5:21d:4a69:8bed%17InterNetworkV6; fe80::c46b:8576:5ba3:d9d2%11InterNetworkV6; fe80::91ce:e6a8:bcee:f56d%18InterNetworkV6; fe80::29b6:dfde:d75e:372b%23InterNetworkV6; 172.21.57.47InterNetwork; 192.168.215.241InterNetwork; 169.254.80.80InterNetwork; 172.16.80.1InterNetwork; fd11:deed:222:10:34ed:839:e95c:a417InterNetworkV6; fd11:deed:222:10:91ce:e6a8:bcee:f56dInterNetworkV6;”

My current method doesn’t get currently disconnected devices and seems to miss label printers because of it (maybe they’re sleeping?). It could be worth just querying the registry instead… It looks like that’s what usbdeview does anyway…

Integrate usb.ids to get user friendly names 😊

Add checkboxes for things like surface docks. Maybe parse them specially and add a normal device object?
 * 
 */

namespace EZInventory {
	/// <summary>
	/// Interaction logic for Info_Window.xaml
	/// </summary>
	public partial class Info_Window : Window {

		public Info_Window() {
			InitializeComponent();
			StatusBarText.Text = "Ready";
			ComputerName.Focus();
		}
		public void SetValues(ComputerInfo info) {
			ComputerName.Text = info.ComputerName;
			IPAddress.Text = info.IPAddress;
			ComputerModel.Text = info.Model;
			SerialNumber.Text = info.SerialNumber;
			WindowsVersion.Text = info.WindowsVersion;

		}

		private void SearchButton_Click(object sender, RoutedEventArgs e) {

			string computer = "";

			if (ComputerName.Text == "" && IPAddress.Text == "") {
				Console.WriteLine("No name or IP entered, searching local host...");
				computer = "localHost";
			}
			else if (ComputerName.Text == "" && IPAddress.Text != "") {
				computer = Dns.GetHostAddresses(IPAddress.Text)[0].ToString();
				Console.WriteLine("No name entered, searching by IP...");
			}
			else {
				computer = ComputerName.Text;
				Console.WriteLine("Searching by hostname...");
			}

			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += worker_QueryInfo;
			worker.ProgressChanged += worker_ProgressChanged;
			worker.RunWorkerCompleted += worker_Complete;
			worker.WorkerReportsProgress = true;
			worker.RunWorkerAsync(computer);

		}

		private void worker_QueryInfo(object sender, DoWorkEventArgs e) {

			string computer = (string)e.Argument;

			Console.WriteLine("Computer = " + computer);

			Ping ping = new Ping();
			PingReply pingReply = null;

			try {
				pingReply = ping.Send(computer);
			}
			catch (PingException pingException) {
				Console.WriteLine("Error! Bad hostname! Aborting...");
				return;
			}

			if (pingReply.Status == IPStatus.Success) {

				(sender as BackgroundWorker).ReportProgress(1);
				ComputerInfo computerInfo = GetComputerInfo(computer);

				(sender as BackgroundWorker).ReportProgress(2, computerInfo);
				List<MonitorInfo> monitorInfos = GetMonitorInfo(computer);

				(sender as BackgroundWorker).ReportProgress(3, monitorInfos);
				List<DeviceInfo> deviceInfos =  GetDeviceInfoRegistry(computer);

				(sender as BackgroundWorker).ReportProgress(4, deviceInfos);
			}
		}

		private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			switch (e.ProgressPercentage) {
				case 1:
					StatusBarText.Text = "Querying Computer Info...";
					break;
				case 2:
					StatusBarText.Text = "Querying Monitor Info...";
					ComputerInfo computerInfo = (ComputerInfo)e.UserState;
					SetValues(computerInfo);
					break;
				case 3:
					StatusBarText.Text = "Querying Device Info... (this might take a minute)";
					DisplayMonitorInfo((List<MonitorInfo>)e.UserState);
					break;
				case 4:
					StatusBarText.Text = "Populating Device Info...";
					DisplayDeviceInfo((List<DeviceInfo>)e.UserState);
					break;
				default:
					StatusBarText.Text = "Error: Unknown BackgroundWorker state";
					break;
			}
		}

		private void worker_Complete(object sender, RunWorkerCompletedEventArgs e) {

			//Update UI based on result
			StatusBarText.Text = "Ready";
		}

		private ComputerInfo GetComputerInfo(string computer) {

			ManagementObjectCollection computerInfoCollection = CIMQuery(computer, "Win32_ComputerSystem");
			ManagementObject computerInfo = computerInfoCollection.OfType<ManagementObject>().First();

			IPAddress[] addresses = Dns.GetHostAddresses(computer);
			string computerIP = "";
			int i = 0;
			foreach (IPAddress a in addresses) {

				computerIP += a.ToString() + a.AddressFamily;
				computerIP += "; ";
			}

			ManagementObjectCollection biosInfoCollection = CIMQuery(computer, "Win32_Bios");
			ManagementObject biosInfo = biosInfoCollection.OfType<ManagementObject>().First();

			ManagementObjectCollection windowsInfoCollection = CIMQuery(computer, "Win32_OperatingSystem");
			ManagementObject windowsInfo = windowsInfoCollection.OfType<ManagementObject>().First();

			string name = computerInfo["Name"].ToString();
			string address = computerIP;
			string model = (computerInfo["Manufacturer"].ToString()) + " " + (computerInfo["Model"].ToString());
			string serial = biosInfo["SerialNumber"].ToString();
			string version = windowsInfo["Caption"] + " " + windowsInfo["OSArchitecture"] + " (" + windowsInfo["Version"] + ")";
			ComputerInfo info = new ComputerInfo(name, address, model, serial, version);
			return info;
		}

		private List<MonitorInfo> GetMonitorInfo(string computer) {

			//Dispatcher.Invoke((Action)delegate () { MonitorInfoStackPanel.Children.Clear(); });

			List<MonitorInfo> monitorInfos = new List<MonitorInfo>();

			ManagementObjectCollection monitors = WMIQuery(computer, "WMIMonitorID");

			if (monitors != null) {
				Console.WriteLine("Monitors != null");
				int monitorCount = 0;

				foreach (ManagementObject monitor in monitors) {

					string monitorManufacturer = "";

					string monitorModel = "";
					foreach (UInt16 i in (UInt16[])monitor["UserFriendlyName"]) {
						monitorModel += (char)i;
					}

					string monitorSerial = "";
					foreach (UInt16 i in (UInt16[])monitor["SerialNumberID"]) {
						monitorSerial += (char)i;
					}

					monitorInfos.Add(new MonitorInfo(monitorManufacturer, monitorModel, monitorSerial));

				}
			}

			return monitorInfos;
		}

		private void DisplayMonitorInfo(List<MonitorInfo> monitorInfos) {

			MonitorInfoStackPanel.Children.Clear();

			int monitorCount = 1;
			foreach (MonitorInfo monitor in monitorInfos) {
				MonitorInfoUserControl info = new MonitorInfoUserControl(monitor);
				info.Title = "Monitor " + monitorCount;
				monitorCount++;
				MonitorInfoStackPanel.Children.Add(info);
			}
		}

		private List<DeviceInfo> GetDeviceInfo(string computer) {

			List<DeviceInfo> deviceInfos = new List<DeviceInfo>();

			ManagementObjectCollection devices = CIMQuery(computer, "Win32_PNPEntity");

			int i = 0;

			foreach (ManagementObject entity in devices) {

				if (entity.Properties["Description"].Value != null && entity.Properties["DeviceID"].Value != null) {

					string manufacturer = ""; //objUSBDevice["Manufacturer"].ToString(); ;
					string model = entity.Properties["Description"].Value.ToString();
					string serialNumber = entity.Properties["DeviceID"].Value.ToString().Split('\\')[2]; //.ToString().Split('\\')[2];
					string type = entity.Properties["DeviceID"].Value.ToString().Split('\\')[0];

					if (!serialNumber.Any(ch => !Char.IsLetterOrDigit(ch)) && type == "USB") {
						deviceInfos.Add(new DeviceInfo(manufacturer, model, serialNumber, ""));

					}

					
				}

				

			}

			return deviceInfos;
		}

		private List<DeviceInfo> GetDeviceInfoRegistry(string computer) {

			List<DeviceInfo> deviceInfos = new List<DeviceInfo>();
			List<string> deviceIDs = new List<string>();
			List<string> serialNumbers = new List<string>();
			List<string> driverNames = new List<string>();
			List<string> manufacturers = new List<string>();

			RegistryKey remoteReg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "AlisLuminis");

			RegistryKey regKey = remoteReg.OpenSubKey("SYSTEM").OpenSubKey("CurrentControlSet").OpenSubKey("Enum").OpenSubKey("USB");

			foreach (string keyName in regKey.GetSubKeyNames()) {

				foreach (string subKeyName in regKey.OpenSubKey(keyName).GetSubKeyNames()) {
					if (!subKeyName.Any(ch => !Char.IsLetterOrDigit(ch))) {
						deviceIDs.Add("USB\\" + keyName + "\\" + subKeyName);
						serialNumbers.Add(subKeyName);
						manufacturers.Add(regKey.OpenSubKey(keyName).OpenSubKey(subKeyName).GetValue("Mfg").ToString().Split(';')[1]);
					}
				}

			}

			ManagementObjectCollection query = CIMQuery(computer, "Win32_PnPSignedDriver");

			while (driverNames.Count < deviceIDs.Count) {
				driverNames.Add("Unknown :(");
			}

			foreach (ManagementObject driver in query) {

				string driverDevice = (string)driver.Properties["DeviceID"].Value;

				string driverDeviceType = driverDevice.Split('\\')[0];

				if (driverDeviceType == "USB") {

					int index = deviceIDs.IndexOf(driverDevice);
					if (index != -1) {
						driverNames[index] = (string)driver.Properties["DeviceName"].Value;
					}

				}

			}

			for (int i = 0; i < deviceIDs.Count; i++) {
				deviceInfos.Add(new DeviceInfo(manufacturers[i], "", TestForHex(serialNumbers[i]), driverNames[i]));
			}

			return deviceInfos;
		}

		//Apparently some serials can be encoded as hexidecimal (at least usbdeview has an option saying so). This tests the string to see if it's probably in hex and if so converts it to ASCII
		private string TestForHex(string testString) {

			char[] chars = testString.ToCharArray();

			string hexString = "";

			for (int i = 0; i < chars.Length; i += 2) {
				if ((chars[i] >= 50) && (chars[i] <= 55)) { //if ascii values could plausably be a string (no normally hidden characters) checks for 2-7
					if ((chars[i+1] >= 48) && (chars[i+1] <= 122)) { //if ascii values could plausably be a string (no normally hidden characters) checks for 0-F
						string temp = chars[i].ToString() + chars[i+1].ToString();
						ushort tempShort = System.Convert.ToUInt16(temp, 16);
						char tempChar = System.Convert.ToChar(tempShort);
						hexString += tempChar.ToString();
					}
					else {
						return testString;
					}
				}
				else {
					return testString;
				}
			}

			return hexString;
		}

		private void DisplayDeviceInfo(List<DeviceInfo> deviceInfos) {

			DeviceInfoStackPanel.Children.Clear();
			int deviceCount = 1;

			foreach (DeviceInfo device in deviceInfos) {
				DeviceInfoUserControl info = new DeviceInfoUserControl(device);
				info.Title = "Device " + deviceCount;

				deviceCount += 1;

				DeviceInfoStackPanel.Children.Add(info);
			}
		}

		private ManagementObjectCollection CIMQuery(string computer, string className) {
			ManagementScope scope = new ManagementScope("\\\\" + computer + "\\root\\cimv2");
			scope.Connect();
			ObjectQuery query = new ObjectQuery("SELECT * FROM " + className);
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

			ManagementObjectCollection queryCollection = searcher.Get();
			return queryCollection;
		}

		private ManagementObjectCollection WMIQuery(string computer, string className) {

			ManagementScope scope = new ManagementScope("\\\\" + computer + "\\root\\wmi");
			scope.Connect();
			ObjectQuery query = new ObjectQuery("SELECT * FROM " + className);
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

			ManagementObjectCollection queryCollection = searcher.Get();

			Console.WriteLine(queryCollection.ToString());

			return queryCollection;
		}
	}
}
