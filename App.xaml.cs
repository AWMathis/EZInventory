using EZInventory.Windows;
using System;
using System.Windows;


namespace EZInventory {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {

		[System.Runtime.InteropServices.DllImport("Kernel32.dll")]
		public static extern bool AttachConsole(int processId);

		private string windowName = "EZ Inventory";

		private void Application_Startup(object sender, StartupEventArgs e) {

			AttachConsole(-1); //Attach a console so console output works.

			string[] args = e.Args;
			InputArgs inputArgs = new InputArgs();

			//Set default arguments
			inputArgs.noGUI = false;
			inputArgs.decryptSerials = true;
			inputArgs.showDisconnected = true;
			inputArgs.requireSerial = true;
			inputArgs.excludeUSBMassStorage = false;
			inputArgs.excludeUSBHubs = false;

			for (int i = 0; i < args.Length; i++) {

				switch (args[i].ToLower()) {

					case "/?":
					case "-?":
					case "-help":
					case "-h":
					case "/h":
					case "/help":
						//Show help dialog //need to implement
						string nl = System.Environment.NewLine + "\t";
						string tb = "\t";
						string helpMsg = "Available commands:" + nl;
						helpMsg += tb + "/InputDB <DB Path> - Path to a usb.ids file. If missing the program will check the same directory as this exe first, then fallback to using an embedded version from when this was built." + nl;
						helpMsg += tb + "/Computer <Computer Name> - Name of a computer to search, autofills when running in GUI, required to search when using command line" + nl;
						helpMsg += tb + "/IP <IP Address> - IP address of a computer to search, autofills when running in GUI" + nl;
						helpMsg += tb + "/Output <Output Path.csv> - The the full path (including filename) to the output CSV file. If this is omited no CSV file will be generated" + nl;
						helpMsg += tb + "/ComputerList <list.txt> - Provide a list of computers (in a text file) to search one at a time. Requires the use of /NoGUI" + nl;
						helpMsg += tb + "/ShowDisconnected - Include disconnected devices in the search" + nl;
						helpMsg += tb + "/DecryptSerials - Automatically detect and decode serials encoded in hexidecimal. Usually this should be enabled" + nl;
						helpMsg += tb + "/RequireSerial - Only include devices with a valid serial number. If not included the software still only includes those with matching VID/PIDs from the usb.ids file" + nl;
						helpMsg += tb + "/ExcludeMassStorage - Exclude any \"USB Mass Storage\" devices" + nl;
						helpMsg += tb + "/NoGUI - Run the app in a console window only, command line arguments are the only way to adjust settings." + nl;
						helpMsg += tb + "/DB - Use a file DB.csv in the same folder as this EXE as a base. Running will update the DB with new info (overwriting old info if needed) as well as an added.csv file and a removed.csv file" + nl;

						Console.WriteLine(helpMsg);
						//System.Windows.Forms.SendKeys.SendWait("{ENTER}"); //Press enter to return to the command line automatically
						this.Shutdown();
						return;


					case "-inputdb":
					case "/inputdb":
						//Path to usb.ids
						if (i+1 < args.Length) {
							i++;
							inputArgs.usbIDSPath = args[i];
						}
						break;

					case "-computer":
					case "/computer":
						//Computer name
						if (i + 1 < args.Length) {
							i++;
							inputArgs.computerName = args[i]; //need to test interactions with ip
						}
						break;

					case "-computerlist":
					case "/computerlist":
						//Computer list
						if (i + 1 < args.Length) {
							i++;
							inputArgs.inputListPath = args[i]; //need to test interactions with ip
						}
						break;

					case "-ip":
					case "/ip":
						if (i + 1 < args.Length) {
							i++;
							inputArgs.ipAddress = args[i]; //need to test
						}
						break;

					case "-output":
					case "/output":
						if (i + 1 < args.Length) {
							i++;
							inputArgs.outputPath = args[i];
						}
						break;

					case "-showdisconnected":
					case "/showdisconnected":
						inputArgs.showDisconnected = true;
						break;

					case "-decryptserials":
					case "/decryptserials":
						inputArgs.decryptSerials = true;
						break;

					case "-nogui":
					case "/nogui":
						inputArgs.noGUI = true;
						break;

					case "-requireserial":
					case "/requireserial":
						inputArgs.requireSerial = true;
						break;

					case "-excludemassstorage":
					case "/excludemassstorage":
						inputArgs.excludeUSBMassStorage = true;
						break;

					case "-db":
					case "/db":
						inputArgs.db = true;
						break;


					default:
						Console.WriteLine("Error! Unknown arguments at position " + i + ": " + args[i] + ". Use /? or /help to get information about available commands. Aborting...");
						//System.Windows.Forms.SendKeys.SendWait("{ENTER}"); //Press enter to return to the command line automatically
						this.Shutdown();
						return;
				}
			}



			if (inputArgs.noGUI) {
				NoGUI_Window window = new NoGUI_Window(inputArgs);
				//System.Windows.Forms.SendKeys.SendWait("{ENTER}"); //Press enter to return to the command line automatically
				this.Shutdown();
			}
			else {
				Info_Window window = new Info_Window(inputArgs);
				window.Title = windowName;
				window.Show();
			}	

	}

		private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
			MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
			e.Handled = true;
		}
	}
}
