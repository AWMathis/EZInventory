using System.Reflection;
using System.Windows;


namespace EZInventory.Windows {
	/// <summary>
	/// Interaction logic for About_Window.xaml
	/// </summary>
	/// 

	public partial class About_Window : Window {

		public About_Window() {


			InitializeComponent();

			string VersionString = "Version " + Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
			VersionText.Text = VersionString;

			
		}
	}
}
