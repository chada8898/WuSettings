using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace WuSettings
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainWindowViewModel vm;

		public MainWindow()
		{
			vm = new MainWindowViewModel();
			vm.Initialize();
			InitializeComponent();
			Loaded += WindowLoaded;
			DataContext = vm;
		}

		private readonly Regex regexNumericOnly = new Regex("[^0-9]+");

		private void TextBox_PreviewNumericOnly(object sender, TextCompositionEventArgs e)
		{
			e.Handled = regexNumericOnly.IsMatch(e.Text);
		}

		private readonly Regex regexAlphaNumericOnly = new Regex("[^a-zA-Z0-9]+");

		private void TextBox_PreviewAlphaNumericOnly(object sender, TextCompositionEventArgs e)
		{
			e.Handled = regexAlphaNumericOnly.IsMatch(e.Text);
		}

		private void WindowLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			DeferFeatureUpdates.Focus();
		}
	}
}
