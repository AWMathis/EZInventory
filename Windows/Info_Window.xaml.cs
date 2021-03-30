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

/*

 * 
 * Multiple tabs for multiple computers?

Add option to exclude pointless driver names like usb hubs?

Console output kinda sucks, maybe swap back to windowed application?

finish menu buttons

add an about screen to the menu

 */

namespace EZInventory {

	public partial class Info_Window : Window {

		private InfoGetter infoGetter = new InfoGetter();
		private CSVWriterClass writer = new CSVWriterClass();

		private ComputerInfo computerInfo = new ComputerInfo();
		private List<MonitorInfo> monitorInfoList = new List<MonitorInfo>();
		private List<DeviceInfo> deviceInfoList = new List<DeviceInfo>();

		private InputArgs globalArgs;

		public Info_Window(InputArgs args) {

			globalArgs = args;

			infoGetter.USBIDSPath = args.usbIDSPath;
			infoGetter.ParseUSBIDs();
			Console.WriteLine();

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

			ComputerInfoUserControl.searchButtonClickEvent += SearchButton_Click;
			
			Console.WriteLine();
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
			ComputerInfoUserControl.ComputerName.ToolTip = "Boot Time: " + info.BootTime;
			ComputerInfoUserControl.IPAddress.Text = info.IPAddress;
			ComputerInfoUserControl.ComputerModel.Text = info.Model;
			ComputerInfoUserControl.SerialNumber.Text = info.SerialNumber;
			ComputerInfoUserControl.WindowsVersion.Text = info.WindowsVersion;
			ComputerInfoUserControl.CurrentUser.Text = info.UsernameDisplayName + " (" + info.Username + ")";
			ComputerInfoUserControl.AssetTag.Text = info.AssetTag;

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
