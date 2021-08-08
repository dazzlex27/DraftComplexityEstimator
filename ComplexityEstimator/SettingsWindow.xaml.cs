using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ComplexityEstimator
{
    internal partial class SettingsWindow : Window
    {
		private readonly Regex _floatValidationRegex;

		public SettingsWindow()
        {
            InitializeComponent();

			_floatValidationRegex = new Regex(@"[+-]?([0-9]+([.][0-9]*)?|[.][0-9]+)");
		}

		private void OnOkButtonClicked(object sender, RoutedEventArgs e)
		{
			var validationPassed = IsValid(TbMultiplier)
									&& IsValid(TbPower)
									&& IsValid(TbGainFactor)
									&& IsValid(TbSmallDetailGainFactor)
									&& IsValid(TbPerformanceRate);

			if (!validationPassed)
				return;

			DialogResult = true;
			Close();
		}

		private void BtCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private static bool IsValid(DependencyObject obj)
		{
			return !Validation.GetHasError(obj) && LogicalTreeHelper.GetChildren(obj).OfType<DependencyObject>().
					   All(IsValid);
		}

		private void CheckFloatTextPreview(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			if (!(sender is TextBox textBox))
				return;

			e.Handled = !_floatValidationRegex.IsMatch(e.Text);
			return;
		}
	}
}