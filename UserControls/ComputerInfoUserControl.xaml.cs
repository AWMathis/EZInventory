using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

			var handler = searchButtonClickEvent;

			if (searchButtonClickEvent != null) {
				searchButtonClickEvent();
			}

		}

	}
}
