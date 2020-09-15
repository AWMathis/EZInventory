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
using EZInventory.InfoClasses;

namespace EZInventory.UserControls {
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class DeviceInfoUserControl : UserControl {

		protected DeviceInfo deviceInfo;

		public DeviceInfoUserControl() {

			InitializeComponent();
			this.DataContext = this;
		}

		public DeviceInfoUserControl(DeviceInfo info) {

			InitializeComponent();

			deviceInfo = info;
			Manufacturer = info.Manufacturer;
			Model = info.Model;
			SerialNumber = info.SerialNumber;
			DriverName = info.PNPEntityName != "" ? info.PNPEntityName : info.DriverName;
			Connected = info.Connected.ToString();
			ProductID = info.ProductID;
			VendorID = info.VendorID;

			DeviceConnected.Background = info.Connected ? Brushes.Green :  Brushes.Red;

			
			this.DataContext = this;
		}

		public string Manufacturer { get; set; }

		public string Model { get; set; }

		public string SerialNumber { get; set; }

		public string DriverName { get; set; }

		public string Connected { get; set; }

		public string Title { get; set; }

		public string ProductID { get; set; }

		public string VendorID { get; set; }
	}
}
