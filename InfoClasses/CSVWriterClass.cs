using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;
using EZInventory.InfoClasses;
using System.Globalization;

namespace EZInventory.CSVWriter {
	class CSVWriterClass {

		public List<CSVInfo> IngestData(ComputerInfo computer, List<MonitorInfo> monitors, List<DeviceInfo> devices) {

			List<CSVInfo> CSVInfos = new List<CSVInfo>();
			string nul = "N/A";

			CSVInfo computerInfo = new CSVInfo(computer.ComputerName, "Computer", computer.IPAddress, computer.Manufacturer, computer.Model, computer.WindowsVersion, nul, nul, computer.SerialNumber, "True", computer.AssetTag, nul, nul, nul, nul, computer.Username, computer.UsernameDisplayName);
			//CSVInfo osInfo = new CSVInfo(computer.ComputerName, "Operating System", "Microsoft", computer.WindowsVersion, nul, nul, nul, "True", nul, nul, computer.Username);

			CSVInfos.Add(computerInfo);
			//CSVInfos.Add(osInfo);

			foreach (MonitorInfo monitor in monitors) {
				CSVInfo monitorInfo = new CSVInfo(computer.ComputerName, "Monitor", nul, monitor.Manufacturer, monitor.Model, nul, nul, nul, monitor.SerialNumber, "True", nul, monitor.ProductID, nul, monitor.VideoOutputType.ToString(), monitor.VideoOutputTypeFriendly, computer.Username, computer.UsernameDisplayName);
				CSVInfos.Add(monitorInfo);
			}

			foreach (DeviceInfo device in devices) {
				CSVInfo deviceInfo = new CSVInfo(computer.ComputerName, "Device", nul, device.Manufacturer, device.Model, nul, device.DriverName, device.PNPEntityName, device.SerialNumber, device.Connected.ToString(), nul, device.ProductID, device.VendorID, nul, nul, computer.Username, computer.UsernameDisplayName);
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
			try {
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
			}
			catch {
				Console.WriteLine("Error - Unable to open a StreamReader to file " + path);
			}
			

			return readInfo;
		}

		//Merge two lists together. Removes duplicates and any conflicts where the items evaluate as equal it will use the version from l2
		public List<CSVInfo> MergeCSVLists(List<CSVInfo> l1, List<CSVInfo> l2) {

			int discardThreshold = 20; //Mark any entries older than this (time in seconds) as disconnected (default 300 seconds(5 minutes))
			List<CSVInfo> returnList = new List<CSVInfo>();
			List<string> alreadyRemoved = new List<string>();

			//Add L2 first, use it as the base to any conflicts/updates use the version from the more recent list
			foreach (CSVInfo info in l2) {
				if (!returnList.Contains(info)) {
					returnList.Add(info);
				}		
			}


			foreach (CSVInfo info in l1) {

				if (!returnList.Contains(info)) {
					returnList.Add(info);
				}

			}

			foreach (CSVInfo info in returnList) {
				if (info.CurrentlyConnected.ToLower() == "true") {
					if ((DateTime.Now - Convert.ToDateTime(info.TimeStamp)).TotalSeconds >= discardThreshold) {
						info.CurrentlyConnected = "False";
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
		public string IPAddress;
		public string Manufacturer;
		public string Model;
		public string WindowsVersion;
		public string DriverName;
		public string PNPEntityName;
		public string SerialNumber;
		public string AssetTag;
		public string CurrentlyConnected;
		public string PID;
		public string VID;
		public string VideoOutputType;
		public string VideoOutputTypeFriendly;
		public string CurrentUser;
		public string CurrentUserDisplayName;
		public string TimeStamp;

		public CSVInfo() : this("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "") { }

		public CSVInfo(string computer, string type, string IPAddress,string manufacturer, string model, string windowsVersion, string driverName, string entityName, string serial, string connected, string assetTag, string pid, string vid, string videoOutputType, string videoOutputTypeFriendly, string user, string userDisplayName) {
			ComputerName = computer;
			DeviceType = type;
			this.IPAddress = IPAddress;
			Manufacturer = manufacturer;
			Model = model;
			WindowsVersion = windowsVersion;
			DriverName = driverName;
			PNPEntityName = entityName;
			SerialNumber = serial;
			CurrentlyConnected = connected;
			AssetTag = assetTag;
			PID = pid;
			VID = vid;
			VideoOutputType = videoOutputType;
			VideoOutputTypeFriendly = videoOutputTypeFriendly;
			CurrentUser = user;
			CurrentUserDisplayName = userDisplayName;
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
				Map(m => m.IPAddress).Index(2).Name("IP Address");
				Map(m => m.Manufacturer).Index(3).Name("Manufacturer");
				Map(m => m.Model).Index(4).Name("Model");
				Map(m => m.WindowsVersion).Index(5).Name("Windows Version");
				Map(m => m.DriverName).Index(6).Name("Driver Name");
				Map(m => m.PNPEntityName).Index(7).Name("PNP Entity Name");
				Map(m => m.SerialNumber).Index(8).Name("Serial Number");
				Map(m => m.CurrentlyConnected).Index(9).Name("Currently Connected");
				Map(m => m.AssetTag).Index(10).Name("Asset Tag");
				Map(m => m.PID).Index(11).Name("Product ID (PID)");
				Map(m => m.VID).Index(12).Name("Vendor ID (VID)");
				Map(m => m.VideoOutputType).Index(13).Name("Video Output Type");
				Map(m => m.VideoOutputTypeFriendly).Index(14).Name("Video Output Type Friendly");
				Map(m => m.CurrentUser).Index(15).Name("Current User");
				Map(m => m.CurrentUserDisplayName).Index(16).Name("Current User Display Name");
				Map(m => m.TimeStamp).Index(17).Name("Time last detected").Optional();
			}
		}

	}
}
