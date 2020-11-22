using System.Windows.Controls;


namespace EZInventory.UserControls {
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class MonitorInfoUserControl : UserControl {
		public MonitorInfoUserControl() {

			InitializeComponent();
			this.DataContext = this;
		}

		public MonitorInfoUserControl(MonitorInfo info) {
			Manufacturer = info.Manufacturer;
			Model = info.Model;
			SerialNumber = info.SerialNumber;
			ProductID = info.ProductID;

			InitializeComponent();
			this.DataContext = this;
		}

		public string Manufacturer { get; set; }

		public string Model { get; set; }

		public string SerialNumber { get; set; }

		public string Title { get; set; }

		public string ProductID { get; set; }
	}
}
