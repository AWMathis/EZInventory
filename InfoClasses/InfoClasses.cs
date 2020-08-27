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

		public DeviceInfo() {
			Manufacturer = "";
			Model = "";
			SerialNumber = "";
			DriverName = "";
		}

		public DeviceInfo(string manufacturer, string model, string serial, string driverName) {
			Manufacturer = manufacturer;
			Model = model;
			SerialNumber = serial;
			DriverName = driverName;
		}
	}
}
