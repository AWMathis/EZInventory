using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;
using EZInventory.InfoClasses;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace EZInventory.CSVWriter {
	class CSVWriterClass {

		public List<CSVInfo> IngestData(ComputerInfo computer, List<MonitorInfo> monitors, List<DeviceInfo> devices) {

			List<CSVInfo> CSVInfos = new List<CSVInfo>();
			string nul = "N/A";

			CSVInfo computerInfo = new CSVInfo(computer.ComputerName, "Computer", computer.Manufacturer, computer.Model, nul, computer.ComputerName, computer.SerialNumber, "True", nul, nul, computer.Username);
			CSVInfo osInfo = new CSVInfo(computer.ComputerName, "Operating System", "Microsoft", computer.WindowsVersion, nul, nul, nul, "True", nul, nul, computer.Username);

			CSVInfos.Add(computerInfo);
			CSVInfos.Add(osInfo);

			foreach (MonitorInfo monitor in monitors) {
				CSVInfo monitorInfo = new CSVInfo(computer.ComputerName, "Monitor", monitor.Manufacturer, monitor.Model, nul, nul, monitor.SerialNumber, "True", monitor.ProductID, nul, computer.Username);
				CSVInfos.Add(monitorInfo);
			}

			foreach (DeviceInfo device in devices) {
				CSVInfo deviceInfo = new CSVInfo(computer.ComputerName, "Device", device.Manufacturer, device.Model, device.DriverName, device.PNPEntityName, device.SerialNumber, device.Connected.ToString(), device.ProductID, device.VendorID, computer.Username);
				CSVInfos.Add(deviceInfo);
			}

			return CSVInfos;
		}




		public int WriteCSV(ComputerInfo computer, List<MonitorInfo> monitors, List<DeviceInfo> devices, string outputPath) {

			List<CSVInfo> CSVInfos = IngestData(computer, monitors, devices);

			return WriteCSV(CSVInfos, outputPath);
		}
		public int WriteCSV(List<CSVInfo> CSVInfos, string outputPath) {

			CSVInfos = CSVInfos.OrderBy(o => o.ComputerName).ToList();

			//Skip header line if the file already exists
			CsvConfiguration csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture) {
				HasHeaderRecord = !File.Exists(outputPath)
			};

			Console.WriteLine("Exporting data to file  " + outputPath);
			using (var writer = new StreamWriter(outputPath, append: true))
			using (var csv = new CsvWriter(writer, csvConfig)) {
				csv.Configuration.RegisterClassMap<CSVInfo.CSVInfoMap>();
				csv.WriteRecords(CSVInfos);
				csv.Flush();
				writer.Flush();
			}

			return 0;
		}

		public int WriteCSV(List<CSVInfo> CSVInfos) {

			CSVInfos = CSVInfos.OrderBy(o => o.ComputerName).ToList();

			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "CSV file (*.csv)|*.csv";
			if (saveFileDialog.ShowDialog() == true) {
				using (var writer = new StreamWriter(saveFileDialog.FileName))
				using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture)) {
					csv.Configuration.RegisterClassMap<CSVInfo.CSVInfoMap>();
					csv.WriteRecords(CSVInfos);
					csv.Flush();
					writer.Flush();
				}
			}
			else {
				return 1;
			}

			return 0;

		}

		public int WriteCSV(ComputerInfo computer, List<MonitorInfo> monitors, List<DeviceInfo> devices) {

			List<CSVInfo> CSVInfos = IngestData(computer, monitors, devices);

			return WriteCSV(CSVInfos);
			
		}

		public List<CSVInfo> ReadCSV(string path) {

			List<CSVInfo> readInfo = new List<CSVInfo>();

			using (var reader = new StreamReader(path))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
				csv.Configuration.RegisterClassMap<CSVInfo.CSVInfoMap>();
				readInfo = csv.GetRecords<CSVInfo>().ToList();
			}

			foreach (CSVInfo info in readInfo) {
				if ((info.TimeStamp == "") || (info.TimeStamp == null)) {
					info.TimeStamp = DateTime.Now.ToString();
				}
			}

			return readInfo;
		}

		//Merge two lists together. Removes duplicates and any conflicts where the items evaluate as equal it will use the version from l2
		public List<CSVInfo> MergeCSVLists(List<CSVInfo> l1, List<CSVInfo> l2, bool overrideEntries) {

			int discardThreshold = 5; //If set to override old entries, override entries this many minutes older (default 5 minutes)
			List<CSVInfo> returnList = new List<CSVInfo>();
			List<string> alreadyRemoved = new List<string>();

			//Add L2 first, use it as the base to any conflicts/updates use the version from the more recent list
			foreach (CSVInfo info in l2) {
				if (!returnList.Contains(info)) {
					returnList.Add(info);
				}		
			}


			foreach (CSVInfo info in l1) {
				if (overrideEntries) {
					if (!returnList.Contains(info) && !alreadyRemoved.Contains(info.ComputerName)) {
						returnList.RemoveAll(l1Info => ((l1Info.ComputerName == info.ComputerName) && ( (Convert.ToDateTime(l1Info.TimeStamp)-DateTime.Now).TotalMinutes < discardThreshold) ));
						
						returnList.Add(info);
						alreadyRemoved.Add(info.ComputerName);
					}
				}
				else {
					if (!returnList.Contains(info)) {
						returnList.Add(info);
					}
					else {
						returnList.Find(x => x == info).TimeStamp = info.TimeStamp;
					}
				}
				
			}
			returnList = returnList.OrderBy(o => o.ComputerName).ToList();
			return returnList;
		}
	}

	public class CSVInfo : IEquatable<CSVInfo> {
		public string ComputerName;
		public string DeviceType;
		public string Manufacturer;
		public string Model;
		public string DriverName;
		public string PNPEntityName;
		public string SerialNumber;
		public string CurrentlyConnected;
		public string PID;
		public string VID;
		public string CurrentUser;
		public string TimeStamp;

		public CSVInfo() : this("", "", "", "", "", "", "", "", "", "", "") { }

		public CSVInfo(string computer, string type, string manufacturer, string model, string driverName, string entityName, string serial, string connected, string pid, string vid, string user) {
			ComputerName = computer;
			DeviceType = type;
			Manufacturer = manufacturer;
			Model = model;
			DriverName = driverName;
			PNPEntityName = entityName;
			SerialNumber = serial;
			CurrentlyConnected = connected;
			PID = pid;
			VID = vid;
			CurrentUser = user;
			TimeStamp = DateTime.Now.ToString();
			
		}


		public bool Equals(CSVInfo other) {

			bool same = (ComputerName == other.ComputerName) && (DeviceType == other.DeviceType) && (Manufacturer == other.Manufacturer) && (Model == other.Model) 
			&& (SerialNumber == other.SerialNumber) && (VID == other.VID) && (PID == other.PID);
			//(CurrentlyConnected == other.CurrentlyConnected) && (DriverName == other.DriverName) && (PNPEntityName == other.PNPEntityName)

			return same;
		}

		public class CSVInfoMap : ClassMap<CSVInfo> {
			public CSVInfoMap() {
				Map(m => m.ComputerName).Index(0).Name("Computer Name");
				Map(m => m.DeviceType).Index(1).Name("Device Type");
				Map(m => m.Manufacturer).Index(2).Name("Manufacturer");
				Map(m => m.Model).Index(3).Name("Model");
				Map(m => m.DriverName).Index(4).Name("Driver Name");
				Map(m => m.PNPEntityName).Index(5).Name("PNP Entity Name");
				Map(m => m.SerialNumber).Index(6).Name("Serial Number");
				Map(m => m.CurrentlyConnected).Index(7).Name("Currently Connected");
				Map(m => m.PID).Index(8).Name("Product ID (PID)");
				Map(m => m.VID).Index(9).Name("Vendor ID (VID)");
				Map(m => m.CurrentUser).Index(10).Name("Current User when detected");
				Map(m => m.TimeStamp).Index(11).Name("Time last detected").Optional();
			}
		}

	}
}
