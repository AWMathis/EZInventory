﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Net;
using EZInventory.UserControls;
using System.Net.NetworkInformation;
using System.ComponentModel;
using EZInventory.InfoClasses;
using EZInventory.CSVWriter;
using MahApps.Metro.Controls;
using ControlzEx.Theming;
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
	}

	public partial class Info_Window : MetroWindow {

		private InfoGetter infoGetter = new InfoGetter();
		private CSVWriterClass writer = new CSVWriterClass();

		private ComputerInfo computerInfo = new ComputerInfo();
		private List<MonitorInfo> monitorInfoList = new List<MonitorInfo>();
		private List<DeviceInfo> deviceInfoList = new List<DeviceInfo>();

		bool themeChanged = false;

		public Info_Window() {

			//Sync the theme with the windows settings. e.g. Dark mode with green accents
			ThemeManager.Current.SyncThemeBaseColorWithWindowsAppModeSetting();
			ThemeManager.Current.SyncThemeColorSchemeWithWindowsAccentColor();

			InitializeComponent();
			infoGetter.ParseUSBIDs();
			StatusBarText.Text = "Ready";

			ComputerName.Focus();
		}
		public Info_Window(InputArgs args) {

			//Sync the theme with the windows settings. e.g. Dark mode with green accents
			ThemeManager.Current.SyncThemeBaseColorWithWindowsAppModeSetting();
			ThemeManager.Current.SyncThemeColorSchemeWithWindowsAccentColor();

			infoGetter.USBIDSPath = args.usbIDSPath;
			infoGetter.ParseUSBIDs();
			Console.WriteLine();

			if (!args.noGUI) { //If input args are specified open the window but with them applied. If a computer name or IP address is given run a search.
				InitializeComponent();
				StatusBarText.Text = "Ready";

				//apply variables from args here
				ComputerName.Text = args.computerName ?? "";
				IPAddress.Text = args.ipAddress ?? "";
				DecryptHexMenuItem.IsChecked = args.decryptSerials;
				ShowDisconnectedMenuItem.IsChecked = args.showDisconnected;
				RequireSerialMenuItem.IsChecked = args.requireSerial;

				if (ComputerName.Text != "" || IPAddress.Text != "") {
					SearchButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); //Raise an event as though the search button was clicked
				}

				ComputerName.Focus();
			}
			else { //If using the nogui argument, output everything to the console and don't show a window at all.

				Console.WriteLine("GUI is disabled, running in console only mode" + System.Environment.NewLine);

				string computerName = args.computerName ?? System.Environment.MachineName;
				Console.WriteLine("Searching info for computer \"" + computerName + "\"..." + System.Environment.NewLine + "-----------------------------------------------------------------------------------" + System.Environment.NewLine);
				ComputerInfo computerInfo = infoGetter.GetComputerInfo(computerName);
				Console.WriteLine("-----------------------------------Computer Info-----------------------------------");
				Console.WriteLine(computerInfo.ToString());

				monitorInfoList = infoGetter.GetMonitorInfo(computerName);
				monitorInfoList = infoGetter.FilterMonitorInfo(monitorInfoList, args.decryptSerials);
				Console.WriteLine("-----------------------------------Monitor Info------------------------------------");
				int monitorCount = 1;
				foreach (MonitorInfo m in monitorInfoList) {
					Console.WriteLine("---------------------------------Monitor " + monitorCount + "-----------------------------------");
					Console.WriteLine(m.ToString());
					monitorCount++;
				}

				deviceInfoList = infoGetter.GetDeviceInfoRegistry(computerName);
				deviceInfoList = infoGetter.FilterDeviceInfo(deviceInfoList, args.decryptSerials, args.showDisconnected, args.requireSerial, args.excludeUSBMassStorage);
				Console.WriteLine("-----------------------------------Device Info-------------------------------------");
				int deviceCount = 1;
				foreach (DeviceInfo d in deviceInfoList) {
					Console.WriteLine("---------------------------------Device " + deviceCount + "------------------------------------");
					Console.WriteLine(d.ToString());
					deviceCount++;
				}

				if (args.outputPath != null) {
					writer.WriteCSV(computerInfo, monitorInfoList, deviceInfoList, args.outputPath);
				}
			}

			Console.WriteLine();
		}

		private void NewInstanceMenuItem_Click(object sender, RoutedEventArgs e) {
			System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);
		}

		private void ExitMenuItem_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}

		private void ExportMenuItem_Click(object sender, RoutedEventArgs e) {
			writer.WriteCSV(DisplayComputerInfo(), DisplayMonitorInfo(), DisplayDeviceInfo());
		}

		private void ChangeDisplayOption_Click(object sender, RoutedEventArgs e) {
			DisplayMonitorInfo();
			DisplayDeviceInfo();
		}

		private void SwitchBaseThemeMenuItem_Click(object sender, RoutedEventArgs e) {
			if (themeChanged) {
				ThemeManager.Current.ChangeTheme(this, "Dark.Blue");
				themeChanged = !themeChanged;
			} 
			else {
				ThemeManager.Current.ChangeTheme(this, "Light.Red");
				themeChanged = !themeChanged;
			}
			
		}

		private void AboutMenuItem_Click(object sender, RoutedEventArgs e) {
			About_Window window = new About_Window();
			window.Title = "About";
			window.Show();
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

			List<MonitorInfo> monitorInfoListModified = infoGetter.FilterMonitorInfo(monitorInfoList, DecryptHexMenuItem.IsChecked);

			int monitorCount = 1;
			foreach (MonitorInfo monitor in monitorInfoList) {

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
			List<DeviceInfo> deviceInfoListModified = infoGetter.FilterDeviceInfo(deviceInfoList, DecryptHexMenuItem.IsChecked, ShowDisconnectedMenuItem.IsChecked, RequireSerialMenuItem.IsChecked, ExcludeMassStorageMenuItem.IsChecked);

			foreach (DeviceInfo device in deviceInfoListModified) {

				DeviceInfoUserControl info = new DeviceInfoUserControl(device);
				info.Title = "Device " + deviceCount;

				deviceCount += 1;

				DeviceInfoStackPanel.Children.Add(info);
			}

			return deviceInfoListModified;
		}

	}
}
