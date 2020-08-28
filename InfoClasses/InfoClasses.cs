using System.Collections.Generic;
using System.Windows.Documents;

namespace EZInventory.InfoClasses{
	public class ComputerInfo {
		public string ComputerName;
		public string IPAddress;
		public string Model;
		public string SerialNumber;
		public string WindowsVersion;

		public ComputerInfo(string name, string address, string model, string serial, string version) {
			ComputerName = name;
			IPAddress = address;
			Model = model;
			SerialNumber = serial;
			WindowsVersion = version;
		}
		public ComputerInfo() {
			ComputerName = "";
			IPAddress = "";
			Model = "";
			SerialNumber = "";
			WindowsVersion = "";
		}

	}

	public class MonitorInfo {
		public string Manufacturer;
		public string Model;
		public string SerialNumber;

		public MonitorInfo() {
			Manufacturer = "";
			Model = "";
			SerialNumber = "";
		}
		public MonitorInfo(string manufacturer, string model, string serial) {
			Manufacturer = manufacturer;
			Model = model;
			SerialNumber = serial;
		}

	}

	public class DeviceInfo {
		public string Manufacturer;
		public string Model;
		public string SerialNumber;
		public string DriverName;
		public string VendorID;
		public string DeviceID;

		public DeviceInfo() {
			Manufacturer = "";
			Model = "";
			SerialNumber = "";
			DriverName = "";
			VendorID = "";
			DeviceID = "";
		}

		public DeviceInfo(string manufacturer, string model, string serial, string driverName, string vendorID, string deviceID) {
			Manufacturer = manufacturer;
			Model = model;
			SerialNumber = serial;
			DriverName = driverName;
			VendorID = vendorID;
			DeviceID = deviceID;
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
}
