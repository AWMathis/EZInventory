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
using System.IO;
using System.Reflection;


/*

Add checkboxes for things like surface docks. Maybe parse them specially and add a normal device object?
 * 
Need a way to tell/sort by what’s connected. Maybe a last connected date value too? You can use win32_pnpentity to check, disconnected devices don’t show in there. Maybe win32_usbdevice too?

pnputil /enum-devices /disconnected can kinda do it... get-pnpdevice works too... so why TF can't you get disconnected devices through win32_pnpentity?!?!?!
get-pnpdevice uses the System.Management.Automation.InvocationInfo namespace... maybe there's something there?
Registry keys have a DeviceDesc property, can we rely on that?
 * 
 * Export to csv?
 * 
 * Multiple tabs for multiple computers?

Make sure exporting follows the “show disconnected devices” and “decode serials” rules

Make UI update when options are changed

Add devices even if serial can’t be found but VID/PID are matched.

Add option to exclude “USB Mass Storage Device” driver names
 */

namespace EZInventory {

	public partial class Info_Window : Window {

		private InfoGetter infoGetter = new InfoGetter();
		private CSVWriter writer = new CSVWriter();

		private ComputerInfo computerInfo = new ComputerInfo();
		private List<MonitorInfo> monitorInfoList = new List<MonitorInfo>();
		private List<DeviceInfo> deviceInfoList = new List<DeviceInfo>();

		public Info_Window() {
			InitializeComponent();
			StatusBarText.Text = "Ready";

			ComputerName.Focus();
		}

		private void ExportMenuItem_Click(object sender, RoutedEventArgs e) {
			writer.WriteCSV(DisplayComputerInfo(), DisplayMonitorInfo(), DisplayDeviceInfo());
		}

		private void DecryptHexMenuItem_Click(object sender, RoutedEventArgs e) {
			DisplayDeviceInfo();
		}

		private void ShowDisconnectedMenuItem_Click(object sender, RoutedEventArgs e) {
			DisplayDeviceInfo();
		}

		private void SearchButton_Click(object sender, RoutedEventArgs e) {

			string computer = "";

			if (ComputerName.Text == "" && IPAddress.Text == "") {
				Console.WriteLine("No name or IP entered, searching local host...");
				computer = System.Environment.MachineName;
			}
			else if (ComputerName.Text == "" && IPAddress.Text != "") {
				computer = Dns.GetHostAddresses(IPAddress.Text)[0].ToString();
				Console.WriteLine("No name entered, searching by IP...");
			}
			else {
				computer = ComputerName.Text;
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
					DisplayComputerInfo();
					break;
				case 3:
					StatusBarText.Text = "Querying Device Info... (this might take a minute)";
					monitorInfoList = (List<MonitorInfo>)e.UserState;
					DisplayMonitorInfo();
					break;
				case 4:
					StatusBarText.Text = "Populating Device Info...";
					deviceInfoList = (List<DeviceInfo>)e.UserState;
					DisplayDeviceInfo();
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

		public ComputerInfo DisplayComputerInfo() {

			ComputerName.Text = computerInfo.ComputerName;
			IPAddress.Text = computerInfo.IPAddress;
			ComputerModel.Text = computerInfo.Model;
			SerialNumber.Text = computerInfo.SerialNumber;
			WindowsVersion.Text = computerInfo.WindowsVersion;

			return computerInfo;
		}

		private List<MonitorInfo> DisplayMonitorInfo() {

			MonitorInfoStackPanel.Children.Clear();

			List<MonitorInfo> monitorInfoListModified = new List<MonitorInfo>(monitorInfoList.Count); //Perform a deep copy to allow different display options without modifying the data
			foreach (MonitorInfo info in monitorInfoList) {
				monitorInfoListModified.Add(new MonitorInfo(info));
			}

			int monitorCount = 1;
			foreach (MonitorInfo monitor in monitorInfoList) {

				if (DecryptHexMenuItem.IsChecked) {
					monitor.SerialNumber = infoGetter.TestForHex(monitor.SerialNumber);
				}

				MonitorInfoUserControl info = new MonitorInfoUserControl(monitor);
				info.Title = "Monitor " + monitorCount;
				monitorCount++;
				MonitorInfoStackPanel.Children.Add(info);
			}

			return monitorInfoListModified;
		}

		private List<DeviceInfo> DisplayDeviceInfo() {

			DeviceInfoStackPanel.Children.Clear();
			int deviceCount = 1;

			List<DeviceInfo> deviceInfoListModified = new List<DeviceInfo>(deviceInfoList.Count); //Perform a deep copy to allow different display options without modifying the data
			foreach (DeviceInfo info in deviceInfoList) {
				deviceInfoListModified.Add(new DeviceInfo(info));
			}

			if (ShowDisconnectedMenuItem.IsChecked == false) {
				deviceInfoListModified.RemoveAll(DeviceDisconnected);
			}

			foreach (DeviceInfo device in deviceInfoListModified) {

				if (DecryptHexMenuItem.IsChecked) {
					device.SerialNumber = infoGetter.TestForHex(device.SerialNumber);
				}

				DeviceInfoUserControl info = new DeviceInfoUserControl(device);
				info.Title = "Device " + deviceCount;

				deviceCount += 1;

				DeviceInfoStackPanel.Children.Add(info);
			}

			return deviceInfoListModified;
		}
		private bool DeviceDisconnected(DeviceInfo e) {
			return !(e.Connected);
		}
	}
}
