
# EZ Inventory	
> A tool to collect hardware information

This program uses WMI/CIM to query remote computers and view hardware information including monitors and known devices.

![Before Searching](/MiscFiles/BeforeSearch.PNG)
![After Searching](/MiscFiles/AfterSearch.PNG)
![Options Menu](/MiscFiles/AfterSearchMenu.PNG)

## About

This program is useful for querying inventory information from multiple PCs (hence the name). At work I've found it annoying to have to constantly crawl around looking for serial numbers on various computers, monitors, printers, etc. Using this has made the process MUCH less painful. 

By using a simple powershell script, or via SCCM you would be able to query many PCs and then later process the spreadsheet data as needed.

For reference, here is the rough method by which each piece of information is obtained:

	Computer Name/IP/Model: CIM query to Win32_ComputerSystem
	Computer Serial: CIM query to Win32_Bios
	Windows Version: CIM query to Win32_OperatingSystem
	
	Monitor Model/Serial: WMI query to WMIMonitorID. This is the only query done in the \\root\wmi namespace due to it being unavailable in CIM
	
	Device PID/VID: Read the registry key HKLM\SYSTEM\CurrentControlSet\Enum\USB, The VID/PID pairs are part of the subkey names
	Device Manufacturer/Model: Lookup VID in the usb.ids database (either embedded or provided by the user
	Device Serial Number: Check each subkey's subkey in HKLM\SYSTEM\CurrentControlSet\Enum\USB\ subkeys without any special characters (typically '&' are normally the serial number
	Device Driver/Device Name: CIM query to Win32_PNPEntity, if that fails then get the driver names from the device's registry key HKLM\SYSTEM\CurrentControlSet\Enum\USB\...\...\DriverDesc
	Device Connected Status: Compare registry device list with Win32_PNPEntity. If it's not present in Win32_PNPEntity then it's not currently connected
	
	usb.ids: A file containing VID/PID's for various USB devices. Either use the embedded copy (obtained from http://www.linux-usb.org/usb-ids.html) or download/create your own. You can either place it in the same folder as the EXE, or specify it's path via command line argument
	

## Required Libraries

*	[CsvHelper](https://joshclose.github.io/CsvHelper/)

## Release History

* 1.0
    * Initial Release


## Meta

Alex Mathis – github@awmathis.com

