using System;
using System.ComponentModel;

namespace ComplexityEstimator
{
	internal partial class MainWindow
	{
		private readonly MainWindowVm _vm;
        
		public MainWindow()
		{
			InitializeComponent();
			_vm = (MainWindowVm) DataContext;
		}
        
		private async void OnContentRendered(object sender, EventArgs e)
		{
			try
			{
				Focus();
				Activate();
				await _vm.LoadDataAsync();
			}
			catch (Exception ex)
			{
				//
			}
		}

		private void OnMainWindowClosing(object sender, CancelEventArgs e)
		{
			_vm?.Dispose();
		}
	}
}