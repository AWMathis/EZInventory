using System.Collections.Generic;


namespace EZInventory.InfoClasses{
	public class ComputerInfo {
		public string ComputerName;
		public string IPAddress;
		public string Model;
		public string SerialNumber;
		public string WindowsVersion;
		public string Manufacturer;
		public string Username;

		public ComputerInfo(string name, string address, string manufacturer, string model, string serial, string version, string username) {
			ComputerName = (name ?? "").Trim();
			IPAddress = (address ?? "").Trim();
			Model = (model ?? "").Trim();
			Manufacturer = (manufacturer ?? "").Trim();
			SerialNumber = (serial ?? "").Trim();
			WindowsVersion = (version ?? "").Trim();
			Username = (username ?? "").Trim();
		}
		public ComputerInfo() {
			ComputerName = "";
			IPAddress = "";
			Manufacturer = "";
			Model = "";
			SerialNumber = "";
			WindowsVersion = "";
			Username = "";
		}

		public override string ToString() {
			string ret = "";
			string nl = System.Environment.NewLine;

			ret += "Computer Name: " + ComputerName + nl;
			ret += "IP Address: " + IPAddress + nl;
			ret += "Manufacturer: " + Manufacturer + nl;
			ret += "Model: " + Model + nl;
			ret += "Serial Number: " + SerialNumber + nl;
			ret += "Windows Version: " + WindowsVersion + nl;
			ret += "Current User: " + Username + nl;

			return ret;
		}
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


		public override string ToString() {
			string ret = "";
			string nl = System.Environment.NewLine;

			ret += "Manufacturer: " + Manufacturer + nl;
			ret += "Model: " + Model + nl;
			ret += "Serial Number: " + SerialNumber + nl;
			ret += "Product ID (PID): " + ProductID + nl;

			return ret;
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

		public override string ToString() {
			string ret = "";
			string nl = System.Environment.NewLine;

			ret += "Manufacturer: " + Manufacturer + nl;
			ret += "Model: " + Model + nl;
			ret += "Serial Number: " + SerialNumber + nl;
			ret += "Driver Name: " + DriverName + nl;
			ret += "PNP Entity Name: " + PNPEntityName + nl;
			ret += "Vendor ID (VID): " + VendorID + nl;
			ret += "Product ID (PID): " + ProductID + nl;
			ret += "Connected: " + Connected + nl;
			

		return ret;
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

