using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EZInventory {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		private void Application_Startup(object sender, StartupEventArgs e) {

			string[] args = e.Args;
			InputArgs inputArgs = new InputArgs();
			inputArgs.noGUI = false;
			inputArgs.showDisconnected = false;
			inputArgs.decryptSerials = false;

			for (int i = 0; i < args.Length; i++) {

				switch (args[i].ToLower()) {

					case "/?":
					case "-?":
					case "-help":
					case "-h":
					case "/h":
					case "/help":
						//Show help dialog //need to implement
						break;

					case "-inputdb":
					case "/inputdb":
						//Path to usb.ids
						if (i+1 < args.Length) {
							i++;
							inputArgs.usbIDSPath = args[i];
						}
						break;

					case "-computer":
					case "-c":
					case "/computer":
					case "/c":
						//Computer name
						if (i + 1 < args.Length) {
							i++;
							inputArgs.computerName = args[i]; //need to test interactions with ip
						}
						break;

					case "-ip":
					case "/ip":
						if (i + 1 < args.Length) {
							i++;
							inputArgs.ipAddress = args[i]; //need to test
						}
						break;

					case "-o":
					case "-output":
					case "/o":
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

					default:
						Console.WriteLine("Error! Unknown arguments at position " + i + ". Use /? or /help to get information about available commands. Aborting...");
						return;
						break;
				}
			}

			

			if (e.Args.Length >= 1) {
				Info_Window window = new Info_Window(inputArgs);
				//MessageBox.Show("Yay! The name of the window is now " + e.Args[0]);
				//window.Title = e.Args[0];
				//window.Show();
				
				window.Show();

				
			}
			else {
				Info_Window window = new Info_Window();
				window.Title = "No Args :(";
				window.Show();
			}

		}

		private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
			MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
			e.Handled = true;
		}
	}
}
