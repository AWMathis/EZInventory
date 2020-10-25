using System;
using System.Collections.Generic;
using System.Windows;
using System.Net;
using EZInventory.UserControls;
using System.Net.NetworkInformation;
using System.ComponentModel;
using EZInventory.InfoClasses;
using EZInventory.CSVWriter;
using EZInventory.Windows;
using System.Reflection.Emit;
using System.IO;

/*

 * 
 * Multiple tabs for multiple computers?

Add option to exclude pointless driver names like usb hubs?

Console output kinda sucks, maybe swap back to windowed application?

finish menu buttons

add an about screen to the menu

 */

namespace EZInventory {

	public struct InputArgs {
		public string computerName;
		public string ipAddress;
		public string usbIDSPath;
		public string outputPath;
		public bool showDisconnected;
		public bool decryptSerials;
		public bool requireSerial;
		public bool excludeUSBMassStorage;
		public bool noGUI;
		public bool excludeUSBHubs;
		public string inputListPath;
		public bool db;
	}

	public partial class Info_Window : Window {

		private InfoGetter infoGetter = new InfoGetter();
		private CSVWriterClass writer = new CSVWriterClass();

		private ComputerInfo computerInfo = new ComputerInfo();
		private List<MonitorInfo> monitorInfoList = new List<MonitorInfo>();
		private List<DeviceInfo> deviceInfoList = new List<DeviceInfo>();

		private InputArgs globalArgs;

		public Info_Window() {

			InitializeComponent();
			infoGetter.ParseUSBIDs();
			StatusBarText.Text = "Ready";

			ComputerInfoUserControl.ComputerName.Focus();

			ComputerInfoUserControl.searchButtonClickEvent += SearchButton_Click;
		}
		public Info_Window(InputArgs args) {

			globalArgs = args;

			infoGetter.USBIDSPath = args.usbIDSPath;
			infoGetter.ParseUSBIDs();
			Console.WriteLine();

			if (!args.noGUI) { //If input args are specified open the window but with them applied. If a computer name or IP address is given run a search.
				InitializeComponent();
				StatusBarText.Text = "Ready";

				//apply variables from args here
				ComputerInfoUserControl.ComputerName.Text = args.computerName ?? "";
				ComputerInfoUserControl.IPAddress.Text = args.ipAddress ?? "";
				DecryptHexMenuItem.IsChecked = args.decryptSerials;
				ShowDisconnectedMenuItem.IsChecked = args.showDisconnected;
				RequireSerialMenuItem.IsChecked = args.requireSerial;

				if (ComputerInfoUserControl.ComputerName.Text != "" || ComputerInfoUserControl.IPAddress.Text != "") {
					ComputerInfoUserControl.SearchButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); //Raise an event as though the search button was clicked
				}

				ComputerInfoUserControl.ComputerName.Focus();
			}
			else { //If using the nogui argument, output everything to the console and don't show a window at all.

				Console.WriteLine("GUI is disabled, running in console only mode, current time is " + DateTime.Now + System.Environment.NewLine);

				string computerListPath = args.inputListPath;
				if (computerListPath != null) {
					if (File.Exists(computerListPath)) {
						List<string> computers = new List<string>();
						string currentLine;
						// Read the file
						System.IO.StreamReader file = new System.IO.StreamReader(computerListPath);
						while ((currentLine = file.ReadLine()) != null) {
							computers.Add(currentLine);
						}
						file.Close();

						List<CSVInfo> masterList = new List<CSVInfo>();
						foreach(string computer in computers) {
							Console.WriteLine("Querying " + computer + "...");
							if ((computer != "") && (computer != null)) {
								try {
									masterList.AddRange(No_GUI(args, computer));
								} catch {
									
								}
								

							}

						}

						if (args.db == true) {

							if (!File.Exists("db.csv")) {
								writer.WriteCSV(new List<CSVInfo>(), "db.csv");
							}

							List<CSVInfo> currentDB = writer.ReadCSV("db.csv");
							File.Delete("db.csv");
						    writer.WriteCSV(writer.MergeCSVLists(currentDB, masterList, false), "db.csv");
							List<CSVInfo> newDB = writer.ReadCSV("db.csv");
							List<CSVInfo> added = new List<CSVInfo>();
							List<CSVInfo> removed = new List<CSVInfo>();

							foreach (CSVInfo oldInfo in currentDB) {
								if (!newDB.Contains(oldInfo)) {
									removed.Add(oldInfo);
								}
							}
							foreach (CSVInfo newInfo in newDB) {
								if (!currentDB.Contains(newInfo)) {
									added.Add(newInfo);
								}
							}

							writer.WriteCSV(added, "added.csv");
							writer.WriteCSV(removed, "removed.csv");

						}

					}
					else {
						Console.WriteLine("Error! A list of computer names was supplied but the file doesn't exist or couldn't be opened! Aborting...");
						return;
					}

				} 
				else {
					No_GUI(args, null);
				}
				
			}

			Console.WriteLine();
		}

		private List<CSVInfo> No_GUI(InputArgs args, string computerNameOverride) {

			globalArgs = args;

			string computerName = computerNameOverride ?? args.computerName ?? System.Environment.MachineName;
			computerName = computerName.Trim(' ');

			Ping ping = new Ping();
			PingReply pingReply = null;

			try {
				pingReply = ping.Send(computerName, 1000);
			}
			catch (PingException pingException) {
				Console.WriteLine("Error! Bad hostname: " + computerName +". Skipping...");
				ping.Dispose();
				return null ;
			}

			if (pingReply.Status != IPStatus.Success) {
				ping.Dispose();
				return null;
			}
			ping.Dispose();

			if (!infoGetter.connectionTest(computerName)) {
				Console.WriteLine("WMI/CIM query failed on " + computerName + ". Skipping...");
				return null;
			}

			Console.WriteLine("Searching info for computer \"" + computerName + "\"..." + System.Environment.NewLine + "-----------------------------------------------------------------------------------" + System.Environment.NewLine);
			ComputerInfo computerInfo = infoGetter.GetComputerInfo(computerName);
			Console.WriteLine("-----------------------------------Computer Info-----------------------------------");
			Console.WriteLine(computerInfo.ToString());

			monitorInfoList = infoGetter.GetMonitorInfo(computerName);
			monitorInfoList = infoGetter.FilterMonitorInfo(monitorInfoList, args);
			Console.WriteLine("-----------------------------------Monitor Info------------------------------------");
			int monitorCount = 1;
			foreach (MonitorInfo m in monitorInfoList) {
				Console.WriteLine("---------------------------------Monitor " + monitorCount + "-----------------------------------");
				Console.WriteLine(m.ToString());
				monitorCount++;
			}
			
			if (!infoGetter.registryTest(computerName)) {
				Console.WriteLine("Registry query failed on " + computerName + ". Skipping devices...");
				return null;
			}

			deviceInfoList = infoGetter.GetDeviceInfoRegistry(computerName);
			deviceInfoList = infoGetter.FilterDeviceInfo(deviceInfoList, globalArgs);
			Console.WriteLine("-----------------------------------Device Info-------------------------------------");
			int deviceCount = 1;
			foreach (DeviceInfo d in deviceInfoList) {
				Console.WriteLine("---------------------------------Device " + deviceCount + "------------------------------------");
				Console.WriteLine(d.ToString());
				deviceCount++;
			}

			List<CSVInfo> listToWrite = writer.IngestData(computerInfo, monitorInfoList, deviceInfoList);

			if (args.outputPath != null) {
				writer.WriteCSV(listToWrite, args.outputPath);
			}

			return listToWrite;

		}

		private void NewInstanceMenuItem_Click(object sender, RoutedEventArgs e) {
			System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);
		}

		private void ExitMenuItem_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}

		private void ExportMenuItem_Click(object sender, RoutedEventArgs e) {
			writer.WriteCSV(DisplayComputerInfo(computerInfo), DisplayMonitorInfo(monitorInfoList), DisplayDeviceInfo(deviceInfoList));
		}

		private void ChangeDisplayOption_Click(object sender, RoutedEventArgs e) {

			globalArgs.decryptSerials = DecryptHexMenuItem.IsChecked;
			globalArgs.showDisconnected = ShowDisconnectedMenuItem.IsChecked;
			globalArgs.requireSerial = RequireSerialMenuItem.IsChecked;
			globalArgs.excludeUSBMassStorage = ExcludeMassStorageMenuItem.IsChecked;
			globalArgs.excludeUSBHubs = ExcludeUSBHubsMenuItem.IsChecked;

			DisplayMonitorInfo(monitorInfoList);
			DisplayDeviceInfo(deviceInfoList);
		}

		private void AboutMenuItem_Click(object sender, RoutedEventArgs e) {
			About_Window window = new About_Window();
			window.Title = "About";
			window.Show();
		}

		private void SearchButton_Click() {

			string computer = "";

			if (ComputerInfoUserControl.ComputerName.Text == "" && ComputerInfoUserControl.IPAddress.Text == "") {
				Console.WriteLine("No name or IP entered, searching local host...");
				computer = System.Environment.MachineName;
			}
			else if (ComputerInfoUserControl.ComputerName.Text == "" && ComputerInfoUserControl.IPAddress.Text != "") {
				computer = Dns.GetHostAddresses(ComputerInfoUserControl.IPAddress.Text)[0].ToString();
				Console.WriteLine("No name entered, searching by IP...");
			}
			else {
				computer = ComputerInfoUserControl.ComputerName.Text;
				Console.WriteLine("Searching by hostname...");
			}

			MonitorInfoStackPanel.Children.Clear();
			DeviceInfoStackPanel.Children.Clear();

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
				(sender as BackgroundWorker).ReportProgress(-1);
				return;
			}

			if (pingReply.Status == IPStatus.Success) {

				(sender as BackgroundWorker).ReportProgress(1);
				ComputerInfo computerInfo = infoGetter.GetComputerInfo(computer);

				(sender as BackgroundWorker).ReportProgress(2, computerInfo);
				List<MonitorInfo> monitorInfos = infoGetter.GetMonitorInfo(computer);

				(sender as BackgroundWorker).ReportProgress(3, monitorInfos);
				List<DeviceInfo> deviceInfos = infoGetter.GetDeviceInfoRegistry(computer);

				(sender as BackgroundWorker).ReportProgress(4, deviceInfos);

				(sender as BackgroundWorker).ReportProgress(5, deviceInfos);
			}
		}

		private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			switch (e.ProgressPercentage) {
				case 1:
					StatusBarText.Text = "Querying Computer Info...";
					break;
				case 2:
					StatusBarText.Text = "Querying Monitor Info...";
					computerInfo = (ComputerInfo)e.UserState;
					DisplayComputerInfo(computerInfo);
					break;
				case 3:
					StatusBarText.Text = "Querying Device Info... (this might take a minute)";
					monitorInfoList = (List<MonitorInfo>)e.UserState;
					DisplayMonitorInfo(monitorInfoList);
					break;
				case 4:
					StatusBarText.Text = "Populating Device Info...";
					deviceInfoList = (List<DeviceInfo>)e.UserState;
					DisplayDeviceInfo(deviceInfoList);
					break;
				case 5:
					StatusBarText.Text = "Ready";
					break;
				case -1:
					StatusBarText.Text = "Error: Unable to query info";
					break;
				default:
					StatusBarText.Text = "Error: Unknown BackgroundWorker state";
					break;
			}
		}

		private void worker_Complete(object sender, RunWorkerCompletedEventArgs e) {

			
		}

		public ComputerInfo DisplayComputerInfo(ComputerInfo info) {

			ComputerInfoUserControl.ComputerName.Text = info.ComputerName;
			ComputerInfoUserControl.IPAddress.Text = info.IPAddress;
			ComputerInfoUserControl.ComputerModel.Text = info.Model;
			ComputerInfoUserControl.SerialNumber.Text = info.SerialNumber;
			ComputerInfoUserControl.WindowsVersion.Text = info.WindowsVersion;
			ComputerInfoUserControl.CurrentUser.Text = info.Username;

			return info;
		}

		private List<MonitorInfo> DisplayMonitorInfo(List<MonitorInfo> info) {

			MonitorInfoStackPanel.Children.Clear();

			List<MonitorInfo> monitorInfoListModified = infoGetter.FilterMonitorInfo(info, globalArgs);

			int monitorCount = 1;
			foreach (MonitorInfo monitor in monitorInfoListModified) {

				MonitorInfoUserControl monitorInfo = new MonitorInfoUserControl(monitor);
				monitorInfo.Title = "Monitor " + monitorCount;
				monitorCount++;
				MonitorInfoStackPanel.Children.Add(monitorInfo);
			}

			return monitorInfoListModified;
		}

		private List<DeviceInfo> DisplayDeviceInfo(List<DeviceInfo> info) {

			DeviceInfoStackPanel.Children.Clear();
			int deviceCount = 1;
			List<DeviceInfo> deviceInfoListModified = infoGetter.FilterDeviceInfo(info, globalArgs);

			foreach (DeviceInfo device in deviceInfoListModified) {

				DeviceInfoUserControl deviceInfo = new DeviceInfoUserControl(device);
				deviceInfo.Title = "Device " + deviceCount;

				deviceCount += 1;

				DeviceInfoStackPanel.Children.Add(deviceInfo);
			}
			return deviceInfoListModified;
		}

	}
}
