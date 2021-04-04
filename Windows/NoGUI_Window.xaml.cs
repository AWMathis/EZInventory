using EZInventory.CSVWriter;
using EZInventory.InfoClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using System.ComponentModel;

namespace EZInventory.Windows {
	/// <summary>
	/// Interaction logic for NoGUI_Window.xaml
	/// </summary>
	public partial class NoGUI_Window : Window {

		private CSVWriterClass writer = new CSVWriterClass();

		private InputArgs globalArgs;

		private List<CSVInfo> masterList = new List<CSVInfo>();
		private List<String> computerList;
		private List<BackgroundWorker> backgroundWorkers;

		public NoGUI_Window(InputArgs args) {

			globalArgs = args;

			if (computerList == null) {
				if ((args.inputListPath != null) && (args.inputListPath != "")) {
					computerList = ReadInComputerNames(args.inputListPath);
				}
				else if ((args.computerName != null) && (args.computerName != "")) {
					computerList = new List<string>();
					computerList.Add(args.computerName);
				}
				else {
					computerList = new List<string>();
					computerList.Add(System.Environment.MachineName);
				}
			}

			if (backgroundWorkers == null) {
				createBackgroundWorkers(computerList);
			}

		}
		public List<string> ReadInComputerNames(string computerListPath) {
			if (File.Exists(computerListPath)) {
				List<string> computers = new List<string>();
				string currentLine;
				// Read the file
				System.IO.StreamReader file = new System.IO.StreamReader(computerListPath);
				while ((currentLine = file.ReadLine()) != null) {
					computers.Add(currentLine.Trim(' '));
				}
				file.Close();
				return computers;
			}
			else {
				Console.WriteLine("Error! A list of computer names was supplied but the file doesn't exist or couldn't be opened! Aborting...");
				return new List<string>();
			}
		}

		public void checkBackgroundWorkerCompletion() {
			foreach (BackgroundWorker b in backgroundWorkers) {
				if (b.IsBusy && b != null) {
					return;
				}
			}

			InputArgs args = globalArgs;

			if ((args.dbName != null) && (args.dbName != "")) {

				if (!File.Exists(args.dbName)) {
					writer.WriteCSV(new List<CSVInfo>(), args.dbName);
				}

				List<CSVInfo> currentDB = writer.ReadCSV(args.dbName);
				File.Delete(args.dbName);
				writer.WriteCSV(writer.MergeCSVLists(currentDB, masterList), args.dbName);
				List<CSVInfo> newDB = writer.ReadCSV(args.dbName);
				List<CSVInfo> added = new List<CSVInfo>();

				foreach (CSVInfo newInfo in newDB) {
					if (!currentDB.Contains(newInfo)) {
						added.Add(newInfo);
					}
				}

				writer.WriteCSV(added, "added.csv");

				System.Windows.Application.Current.Shutdown();
			}
		}
		public void createBackgroundWorkers(List<string> computerList) {
			backgroundWorkers = new List<BackgroundWorker>();
			foreach (string c in computerList) {
				Console.WriteLine("Creating BG Worker for computer " + c);
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += worker_QueryInfo;
				worker.RunWorkerCompleted += worker_Complete;
				backgroundWorkers.Add(worker);
				worker.RunWorkerAsync(c);
			}
		}

		private void worker_QueryInfo(object sender, DoWorkEventArgs e) {

			try {
				Ping ping = new Ping();
				PingReply pingReply = null;

				try {
					pingReply = ping.Send((string)e.Argument, 1000);
				}
				catch (PingException pingException) {
					Console.WriteLine("Error! Bad hostname: " + (string)e.Argument + ". Skipping...");
					ping.Dispose();
					return;
				}

				if (pingReply.Status != IPStatus.Success) {
					ping.Dispose();
					return;
				}
				ping.Dispose();

				InfoGetter infoGetter = new InfoGetter();
				infoGetter.USBIDSPath = globalArgs.usbIDSPath;
				if (!infoGetter.connectionTest((string)e.Argument)) {
					Console.WriteLine("WMI/CIM query failed on " + (string)e.Argument + ". Skipping...");
					return;
				}

				if (!infoGetter.registryTest((string)e.Argument)) {
					Console.WriteLine("Registry query failed on " + (string)e.Argument + ". Skipping...");
					return;
				}

				string computer = (string)e.Argument;
				
				ComputerInfo computerInfo = infoGetter.GetComputerInfo(computer);
				List<MonitorInfo>  monitorInfos = infoGetter.GetMonitorInfo(computer);
				monitorInfos = infoGetter.FilterMonitorInfo(monitorInfos, globalArgs);
				List<DeviceInfo> deviceInfos = infoGetter.GetDeviceInfoRegistry(computer);
				deviceInfos = infoGetter.FilterDeviceInfo(deviceInfos, globalArgs);

				List<CSVInfo> returnInfo = writer.IngestData(computerInfo, monitorInfos, deviceInfos);
				e.Result = returnInfo;
			}
			catch {
				Console.WriteLine("Unknown unhandled error querying " + e.Argument + ". Aborting...");
				e.Result = null;
			}

			
		}

		private void worker_Complete(object sender, RunWorkerCompletedEventArgs e) {
			if (e.Result != null) {
				List<CSVInfo> result = (List<CSVInfo>)e.Result;
				if (result.Count >= 1) {
					Console.WriteLine("Finished Querying " + result[0].ComputerName);
					masterList.AddRange(result);
				}
			}
			checkBackgroundWorkerCompletion();
		}
	}
}