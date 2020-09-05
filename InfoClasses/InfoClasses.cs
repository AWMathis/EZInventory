using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace EZInventory.InfoClasses{
	public class ComputerInfo {
		public string ComputerName;
		public string IPAddress;
		public string Model;
		public string SerialNumber;
		public string WindowsVersion;
		public string Manufacturer;

		public ComputerInfo(string name, string address, string manufacturer, string model, string serial, string version) {
			ComputerName = (name ?? "").Trim(); ;
			IPAddress = (address ?? "").Trim(); ;
			Model = (model ?? "").Trim(); ;
			Manufacturer = (manufacturer ?? "").Trim(); ;
			SerialNumber = (serial ?? "").Trim(); ;
			WindowsVersion = (version ?? "").Trim(); ;
		}
		public ComputerInfo() {
			ComputerName = "";
			IPAddress = "";
			Manufacturer = "";
			Model = "";
			SerialNumber = "";
			WindowsVersion = "";
		}

	}

	public class MonitorInfo {
		public string Manufacturer;
		public string Model;
		public string SerialNumber;
		public string ProductID;

		public MonitorInfo() {
			Manufacturer = "";
			Model = "";
			SerialNumber = "";
			ProductID = "";
		}
		public MonitorInfo(string manufacturer, string model, string serial, string productID) {
			Manufacturer = (manufacturer ?? "").Trim();
			Model = (model ?? "").Trim();
			SerialNumber = (serial ?? "").Trim();
			ProductID = (productID ?? "").Trim();
		}

		public MonitorInfo(MonitorInfo info) {
			Manufacturer = info.Manufacturer;
			Model = info.Model;
			SerialNumber = info.SerialNumber;
			ProductID = info.ProductID;
		}

		public MonitorInfo Copy() {
			return new MonitorInfo(this);
		}
	}

	public class DeviceInfo {
		public string Manufacturer;
		public string Model;
		public string SerialNumber;
		public string DriverName;
		public string VendorID;
		public string ProductID;
		public bool Connected;
		public string PNPEntityName;

		public DeviceInfo() {
			Manufacturer = "";
			Model = "";
			SerialNumber = "";
			DriverName = "";
			VendorID = "";
			ProductID = "";
			Connected = false;
			PNPEntityName = "";
		}

		public DeviceInfo(string manufacturer, string model, string serial, string driverName, string pnpEntityName, string vendorID, string productID, bool connected) {
			Manufacturer = (manufacturer ?? "").Trim();
			Model = (model ?? "").Trim();
			SerialNumber = (serial ?? "").Trim();
			DriverName = (driverName ?? "").Trim();
			VendorID = (vendorID ?? "").Trim();
			ProductID = (productID ?? "").Trim();
			Connected = connected;
			PNPEntityName = (pnpEntityName ?? "").Trim();
		}

		public DeviceInfo(DeviceInfo info) {
			Manufacturer = info.Manufacturer;
			Model = info.Model;
			SerialNumber = info.SerialNumber;
			DriverName = info.DriverName;
			VendorID = info.VendorID;
			ProductID = info.ProductID;
			Connected = info.Connected;
			PNPEntityName = info.PNPEntityName;
		}

		public DeviceInfo Copy() {
			return new DeviceInfo(this);
		}

	}

	public class VendorInfo {
		public string ID;
		public string Name;
		public List<VendorDeviceInfo> Products;

		public VendorInfo() {
			ID = "";
			Name = "";
			Products = new List<VendorDeviceInfo>();
		}
		
		public VendorInfo(string vendorID, string vendorName, List<VendorDeviceInfo> list) {
			ID = vendorID;
			Name = vendorName;
			Products = list;
		}

	}

	public class VendorDeviceInfo {
		public string ID;
		public string Name;

		public VendorDeviceInfo() {
			ID = "";
			Name = "";
		}

		public VendorDeviceInfo(string deviceID, string deviceName) {
			ID = deviceID;
			Name = deviceName;
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
