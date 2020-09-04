using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Win32;

namespace EZInventory.InfoClasses {
	class CSVWriter {

		public int WriteCSV(ComputerInfo computer, List<MonitorInfo> monitors, List<DeviceInfo> devices) {


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
				CSVInfo deviceInfo = new CSVInfo(computer.ComputerName, "USB Device", device.Manufacturer, device.Model, device.DriverName, device.PNPEntityName, device.SerialNumber, device.Connected.ToString(), device.ProductID, device.VendorID);
				CSVInfos.Add(deviceInfo);
			}


			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "CSV file (*.csv)|*.csv";
			if (saveFileDialog.ShowDialog() == true) {
				using (var writer = new StreamWriter(saveFileDialog.FileName))
				using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture)) {
					csv.Configuration.RegisterClassMap<CSVInfoMap>();
					csv.WriteRecords(CSVInfos);
					csv.Flush();
					writer.Flush();
				}
				
			} else {
				return 1;
			}
			
			return 0;
		}



	}
}
