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

namespace EZInventory.CSVWriter {
	class CSVWriterClass {

		public List<CSVInfo> IngestData(ComputerInfo computer, List<MonitorInfo> monitors, List<DeviceInfo> devices) {

			List<CSVInfo> CSVInfos = new List<CSVInfo>();
			string nul = "N/A";

			CSVInfo computerInfo = new CSVInfo(computer.ComputerName, "Computer", computer.Manufacturer, computer.Model, nul, computer.ComputerName, computer.SerialNumber, "True", nul, nul);
			CSVInfo osInfo = new CSVInfo(computer.ComputerName, "Operating System", "Microsoft", computer.WindowsVersion, nul, nul, nul, "True", nul, nul);

			CSVInfos.Add(computerInfo);
			CSVInfos.Add(osInfo);

			foreach (MonitorInfo monitor in monitors) {
				CSVInfo monitorInfo = new CSVInfo(computer.ComputerName, "Monitor", monitor.Manufacturer, monitor.Model, nul, nul, monitor.SerialNumber, "True", monitor.ProductID, nul);
				CSVInfos.Add(monitorInfo);
			}

			foreach (DeviceInfo device in devices) {
				CSVInfo deviceInfo = new CSVInfo(computer.ComputerName, "Device", device.Manufacturer, device.Model, device.DriverName, device.PNPEntityName, device.SerialNumber, device.Connected.ToString(), device.ProductID, device.VendorID);
				CSVInfos.Add(deviceInfo);
			}

			return CSVInfos;
		}

		public int WriteCSV(ComputerInfo computer, List<MonitorInfo> monitors, List<DeviceInfo> devices, string outputPath) {

			List<CSVInfo> CSVInfos = IngestData(computer, monitors, devices);

			if (outputPath != null) {
				Console.WriteLine("Exporting data to file  " + outputPath);
				using (var writer = new StreamWriter(outputPath))
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

	}

	public class CSVInfo {
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

		public CSVInfo(string computer, string type, string manufacturer, string model, string driverName, string entityName, string serial, string connected, string pid, string vid) {
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
			}
		}
	}
}
