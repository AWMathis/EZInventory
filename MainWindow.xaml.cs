using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Management.Infrastructure;
using System.Management;

namespace EZInventory {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		private void mainGrid_MouseUp(object sender, MouseButtonEventArgs args) {
			MessageBox.Show("You clicked me! Stranger Danger!!!!\nPosition = " + args.GetPosition(this).ToString());

		}

		private void ButtonClick(object sender, RoutedEventArgs e) {
			LBResult.Items.Add(mainPanel.FindResource("comboBoxButton"));
			WMITest();
		}

		private void WMITest() {

			ManagementScope scope = new ManagementScope("\\\\localhost\\root\\cimv2");
			scope.Connect();
			ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

			ManagementObjectCollection queryCollection = searcher.Get();
			foreach (ManagementObject m in queryCollection) {

				MessageBox.Show(m["csname"].ToString());
				// Display the remote computer information
				Console.WriteLine("Computer Name     : {0}", m["csname"]);
				Console.WriteLine("Windows Directory : {0}", m["WindowsDirectory"]);
				Console.WriteLine("Operating System  : {0}", m["Caption"]);
				Console.WriteLine("Version           : {0}", m["Version"]);
				Console.WriteLine("Manufacturer      : {0}", m["Manufacturer"]);

				LBResult.Items.Add(m["csname"].ToString());

			}
		}


	}
}
