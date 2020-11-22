using System.Windows;
using System.Windows.Controls;

namespace EZInventory.UserControls {
	/// <summary>
	/// Interaction logic for ComputerInfoUserControl.xaml
	/// </summary>
	public partial class ComputerInfoUserControl : UserControl {

		//Used to pass the button click to the main window
		public delegate void searchButtonClick();
		public event searchButtonClick searchButtonClickEvent;

		public ComputerInfoUserControl() {

			InitializeComponent();
		}

		//Used to pass the button click to the main window
		private void SearchButton_Click(object sender, RoutedEventArgs e) {
			Clear();
			var handler = searchButtonClickEvent;

			if (searchButtonClickEvent != null) {
				searchButtonClickEvent();
			}

		}

		private void Clear() {
			string saveComputerName = null;
			string saveIP = null;
			if (ComputerName.Text == null && IPAddress.Text == null) {
				saveComputerName = System.Environment.MachineName;
			}
			else if (ComputerName.Text == null && IPAddress.Text != null) {
				saveIP = IPAddress.Text;
			}
			else {
				saveComputerName = ComputerName.Text;
			}

			ComputerName.Text = saveComputerName ?? "";
			IPAddress.Text = saveIP ?? "";
			WindowsVersion.Text = "";
			SerialNumber.Text = "";
			ComputerModel.Text = "";
			CurrentUser.Text = "";
			AssetTag.Text = "";


		}

	}
}
