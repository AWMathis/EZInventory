using EZInventory.CSVWriter;
using EZInventory.InfoClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;


namespace EZInventory.Windows {
	/// <summary>
	/// Interaction logic for NoGUI_Window.xaml
	/// </summary>
	public partial class NoGUI_Window : Window {

		private InfoGetter infoGetter = new InfoGetter();
		private CSVWriterClass writer = new CSVWriterClass();

		private ComputerInfo computerInfo = new ComputerInfo();
		private List<MonitorInfo> monitorInfoList = new List<MonitorInfo>();
		private List<DeviceInfo> deviceInfoList = new List<DeviceInfo>();

		private InputArgs globalArgs;


		public NoGUI_Window(InputArgs args) {
			Console.WriteLine("GUI is disabled, running in console only mode, current time is " + DateTime.Now + System.Environment.NewLine);

			globalArgs = args;

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
					foreach (string computer in computers) {
						Console.WriteLine("Querying " + computer + "...");
						if ((computer != "") && (computer != null)) {
							try {
								masterList.AddRange(No_GUI(args, computer));
							}
							catch {

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

		private List<CSVInfo> No_GUI(InputArgs args, string computerNameOverride) {

			string computerName = computerNameOverride ?? args.computerName ?? System.Environment.MachineName;
			computerName = computerName.Trim(' ');

			Ping ping = new Ping();
			PingReply pingReply = null;

			try {
				pingReply = ping.Send(computerName, 1000);
			}
			catch (PingException pingException) {
				Console.WriteLine("Error! Bad hostname: " + computerName + ". Skipping...");
				ping.Dispose();
				return null;
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


	}
}
