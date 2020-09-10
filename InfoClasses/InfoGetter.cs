﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using EZInventory.InfoClasses;
using Microsoft.Win32;
using System.IO;


namespace EZInventory.InfoClasses {

	class InfoGetter {

		private Dictionary<string, VendorInfo> usbIDDictionary;

		private string _USBIDSPath;
		public string USBIDSPath{ get; set; }

		public InfoGetter() {
			usbIDDictionary = new Dictionary<string, VendorInfo>();

		}

		public ComputerInfo GetComputerInfo(string computer) {

			ManagementObjectCollection computerInfoCollection = CIMQuery(computer, "Win32_ComputerSystem");
			ManagementObject computerInfo = computerInfoCollection.OfType<ManagementObject>().First();

			IPAddress[] addresses = Dns.GetHostAddresses(computer);
			string computerIP = "";
			int i = 0;
			foreach (IPAddress a in addresses) {

				computerIP += a.ToString() + "; "; ;
			}

			ManagementObjectCollection biosInfoCollection = CIMQuery(computer, "Win32_Bios");
			ManagementObject biosInfo = biosInfoCollection.OfType<ManagementObject>().First();

			ManagementObjectCollection windowsInfoCollection = CIMQuery(computer, "Win32_OperatingSystem");
			ManagementObject windowsInfo = windowsInfoCollection.OfType<ManagementObject>().First();

			string name = computerInfo["Name"].ToString();
			string address = computerIP;
			string manufacturer = computerInfo["Manufacturer"].ToString();
			string model = (computerInfo["Manufacturer"].ToString()) + " " + (computerInfo["Model"].ToString());
			string serial = biosInfo["SerialNumber"].ToString();
			string version = windowsInfo["Caption"] + " " + windowsInfo["OSArchitecture"] + " (" + windowsInfo["Version"] + ")";
			ComputerInfo info = new ComputerInfo(name, address, manufacturer, model, serial, version);
			return info;
		}

		public List<MonitorInfo> GetMonitorInfo(string computer) {


			List<MonitorInfo> monitorInfos = new List<MonitorInfo>();

			ManagementObjectCollection monitors = WMIQuery(computer, "WMIMonitorID");

			if (monitors != null) {

				foreach (ManagementObject monitor in monitors) {

					string monitorModel = "";
					foreach (UInt16 i in (UInt16[])monitor["UserFriendlyName"]) {
						monitorModel += (char)i;
					}

					string monitorSerial = "";
					foreach (UInt16 i in (UInt16[])monitor["SerialNumberID"]) {
						monitorSerial += (char)i;
					}

					string monitorPID = "";
					foreach (UInt16 i in (UInt16[])monitor["ProductCodeID"]) {
						monitorPID += (char)i;
					}

					string monitorManufacturer = "";
					foreach (UInt16 i in (UInt16[])monitor["ManufacturerName"]) {
						monitorManufacturer += (char)i;
					}

					monitorInfos.Add(new MonitorInfo(monitorManufacturer, monitorModel, monitorSerial, monitorPID));

				}
			}

			return monitorInfos;
		}

		public List<DeviceInfo> GetDeviceInfoRegistry(string computer) {

			List<DeviceInfo> deviceInfos = new List<DeviceInfo>();
			List<string> deviceIDs = new List<string>();
			List<string> serialNumbers = new List<string>();
			List<string> driverNames = new List<string>();
			List<string> entityNames = new List<string>();
			List<string> manufacturers = new List<string>();
			List<bool> connecteds = new List<bool>();

			RegistryKey remoteReg = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, computer);

			RegistryKey regKey = remoteReg.OpenSubKey("SYSTEM").OpenSubKey("CurrentControlSet").OpenSubKey("Enum").OpenSubKey("USB");

			int deviceIndex = 0;
			foreach (string keyName in regKey.GetSubKeyNames()) { //Search registry for previously connected USB devices

				foreach (string subKeyName in regKey.OpenSubKey(keyName).GetSubKeyNames()) {
					if (!subKeyName.Any(ch => !Char.IsLetterOrDigit(ch))) {
						deviceIDs.Add(keyName); //deviceIDs.Add("USB\\" + keyName + "\\" + subKeyName);
						serialNumbers.Add(subKeyName);
						manufacturers.Add(regKey.OpenSubKey(keyName).OpenSubKey(subKeyName).GetValue("Mfg").ToString().Split(';')[1]);

						driverNames.Add(regKey.OpenSubKey(keyName).OpenSubKey(subKeyName).GetValue("DeviceDesc").ToString().Split(';')[1]);
						deviceIndex++;
					}
					else if (keyName.Split('&').Length > 1) { //Add devices with same VID/PID but different after
						if (deviceIDs.Contains(keyName.Split('&')[0] + "&" + keyName.Split('&')[1])) {
							string driverDesc = regKey.OpenSubKey(keyName).OpenSubKey(subKeyName).GetValue("DeviceDesc").ToString();
							if (driverDesc.Split(';').Length > 1) {
								driverNames[deviceIndex - 1] = driverDesc.Split(';')[1] + "; " + driverNames[deviceIndex - 1];
							}
							else {
								driverNames[deviceIndex - 1] = driverDesc + "; " + driverNames[deviceIndex - 1];
							}

						}
					}

				}

			}

			//Prevent any out of bounds errors
			while (entityNames.Count < deviceIDs.Count) {
				entityNames.Add(null);
			}

			//Query win32_pnpentity class to get device info
			ManagementObjectCollection pnpEntityQuery = CIMQuery(computer, "Win32_PnPEntity");
			foreach (ManagementObject entity in pnpEntityQuery) { //Cross reference drivers against the registry entries

				string entityDevice = ((string)entity.Properties["PNPDeviceID"].Value) ?? "";

				if (entityDevice.Split('\\').Length > 1) {
					entityDevice = entityDevice.Split('\\')[1]; // USB\VID_18D1&PID_0001&REV_0310 -> VID_18D1&PID_0001&REV_0310
				}

				string[] entityDeviceStrings = entityDevice.Split('&');
				if (entityDeviceStrings.Length > 1) {
					entityDevice = entityDeviceStrings[0] + "&" + entityDeviceStrings[1] + ""; // VID_18D1&PID_0001&REV_0310 -> VID_18D1&PID_0001

					int index = deviceIDs.IndexOf(entityDevice);
					if (index != -1) {
						if (entityNames[index] != null) {
							entityNames[index] += ", ";
						}
						entityNames[index] += (string)entity.Properties["Name"].Value ?? (string)entity.Properties["Caption"].Value ?? (string)entity.Properties["Description"].Value;
					}
				}
			}

			for (int i = 0; i < deviceIDs.Count; i++) { //Create device info objects

				string deviceID = deviceIDs[i]; //.Split('\\')[1];
				string vendor = deviceID.Substring(4, 4).ToUpper();
				string device = deviceID.Substring(13, 4).ToUpper();

				(string mfg, string dev) ret = DeviceIDLookup(vendor, device);

				string manufacturer = ret.mfg;
				if (manufacturer == null) {
					manufacturer = manufacturers[i];
				}

				string model = ret.dev;
				if (model == null) {
					model = "?????";
				}

				string serial = serialNumbers[i];	

				string driverName = driverNames[i];

				string entityName = entityNames[i];

				bool connected = (entityNames[i] != null);

				deviceInfos.Add(new DeviceInfo(manufacturer, model, serial, driverName, entityName, vendor, device, connected));
			}

			return deviceInfos;
		}


		public void ParseUSBIDs() {

			usbIDDictionary.Clear();

			//If it exists, use a usb.ids file in the same directory as the exe. It's updated fairly regularly so this will allow the program to remain relevant longer
			string usbIDS = "";

			if (File.Exists(USBIDSPath)) {
				Console.WriteLine("usb.ids file path supplied, will use that instead of embedded version");
				usbIDS = System.IO.File.ReadAllText(USBIDSPath);
			}
			else if (File.Exists("usb.ids")) {
				Console.WriteLine("usb.ids file detected, will use that instead of embedded version");
				usbIDS = System.IO.File.ReadAllText("usb.ids");
			}
			else {
				usbIDS = System.Text.Encoding.Default.GetString(Properties.Resources.usb);
			}

			string[] usbIDSArray = usbIDS.Split('\n');

			int arrayLength = usbIDSArray.Length;
			for (int i = 0; i < usbIDSArray.Length;) {
				if (usbIDSArray[i].Length > 0) {

					if (usbIDSArray[i][0] != '\t' && usbIDSArray[i][0] != '#') { //if it's a vendor ID

						if (usbIDSArray[i].Substring(0, 4).Contains(' ')) { //If we're past the device/vendor ids
							break;
						}

						string id = usbIDSArray[i].Substring(0, 4).ToUpper();
						string name = usbIDSArray[i].Substring(6);
						List<VendorDeviceInfo> children = new List<VendorDeviceInfo>();

						int childOffset = 1;
						while (usbIDSArray[i + childOffset][0] == '\t' && usbIDSArray[i + childOffset][0] != '#') { //while line is still a child

							string childID = usbIDSArray[i + childOffset].Substring(1, 4).ToUpper();
							string childName = usbIDSArray[i + childOffset].Substring(5);
							children.Add(new VendorDeviceInfo(childID, childName));

							if (i + childOffset + 1 >= arrayLength || usbIDSArray[i + childOffset + 1].Length < 6) { //Out of bounds prevention
								break;
							}

							childOffset++;

						}


						VendorInfo tempInfo = new VendorInfo(id, name, children);

						usbIDDictionary.Add(id, tempInfo);

						if (childOffset == 0) {
							childOffset = 1;
						}

						i += childOffset;
					}
					else {
						i++;
					}
				}
				else {
					i++;
				}
			}
			Console.WriteLine("usb.ids has finished parsing");
		}

		private (string mfg, string dev) DeviceIDLookup(string vendorID, string deviceID) {

			if (usbIDDictionary.ContainsKey(vendorID)) {

				VendorInfo vendor = usbIDDictionary[vendorID];
				VendorDeviceInfo device = null;

				foreach (VendorDeviceInfo vendorDevice in vendor.Products) {
					if (vendorDevice.ID == deviceID) {
						device = vendorDevice;
					}
				}

				if (device != null) {
					return (vendor.Name, device.Name);
				}
				else {
					return (vendor.Name, null);
				}

			}

			return (null, null);
		}

		//Apparently some serials can be encoded as hexidecimal (at least usbdeview has an option saying so). This tests the string to see if it's probably in hex and if so converts it to ASCII
		public string TestForHex(string testString) {

			char[] chars = testString.ToCharArray();

			string hexString = "";

			for (int i = 0; i < chars.Length; i += 2) {
				if ((chars[i] >= 50) && (chars[i] <= 55)) { //if ascii values could plausably be a string (no normally hidden characters) checks for 2-7
					if ((chars[i + 1] >= 48) && (chars[i + 1] <= 70)) { //if ascii values could plausably be a string (no normally hidden characters) checks for 0-F
						string temp = chars[i].ToString() + chars[i + 1].ToString();
						ushort tempShort = System.Convert.ToUInt16(temp, 16);
						char tempChar = System.Convert.ToChar(tempShort);
						hexString += tempChar.ToString();
					}
					else {
						return testString;
					}
				}
				else {
					return testString;
				}
			}

			return hexString;
		}

		private ManagementObjectCollection CIMQuery(string computer, string className) {
			ManagementScope scope = new ManagementScope("\\\\" + computer + "\\root\\cimv2");
			scope.Connect();
			ObjectQuery query = new ObjectQuery("SELECT * FROM " + className);
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

			ManagementObjectCollection queryCollection = searcher.Get();

			searcher.Dispose();

			return queryCollection;
		}

		private ManagementObjectCollection WMIQuery(string computer, string className) {

			ManagementScope scope = new ManagementScope("\\\\" + computer + "\\root\\wmi");
			scope.Connect();
			ObjectQuery query = new ObjectQuery("SELECT * FROM " + className);
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

			ManagementObjectCollection queryCollection = searcher.Get();

			searcher.Dispose();

			return queryCollection;
		}


















	}
}
