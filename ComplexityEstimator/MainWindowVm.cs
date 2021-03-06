using ImageProcessor;
using Microsoft.Win32;
using Primitives;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ComplexityEstimator
{
	internal class MainWindowVm : BaseViewModel, IDisposable
	{
		private static readonly string ConfigPath = GlobalConstants.ConfigFilePath;

		private CalculationSettings _settings;
		private FileProcessor _fileProcessor;

		private string _inputFilePath;

		public MainWindowVm()
		{
			InputFilePath = "";
			SelectFileCommand = new CommandHandler(SelectFile, true);
			ProcessFileCommand = new CommandHandler(ProcessFile, true);
			OpenSettingsCommand = new CommandHandler(OpenSettingsWindow, true);
			ShutDownCommand = new CommandHandler(ShutDown, true);
		}
		
		public async Task LoadDataAsync()
		{
			await InitializeSettings();
			
			_fileProcessor = new FileProcessor();
			_fileProcessor.SetCalculationSettings(_settings);
		}

		public ICommand SelectFileCommand { get; }

		public ICommand ProcessFileCommand { get; }

		public ICommand OpenSettingsCommand { get; }

		public ICommand ShutDownCommand { get; }

		public string InputFilePath
		{
			get => _inputFilePath;
			set => SetField(ref _inputFilePath, value, nameof(InputFilePath));
		}

		public void Dispose() => _fileProcessor?.Dispose();

		private void SelectFile()
		{
			try
			{
				var dialog = new OpenFileDialog
				{
					Filter = "excel files|*.xlsx;*.xlsm",
					DefaultExt = "xlsx",
					Multiselect = false,
					InitialDirectory = Assembly.GetEntryAssembly().Location
				};
				var result = dialog.ShowDialog();
				if (result != true)
					return;

				InputFilePath = dialog.FileName;
			}
			catch (Exception ex)
			{
				ShowErrorMessage($"Произошла ошибка: {ex.Message}");
			}
		}

		private void ProcessFile()
		{
			try
			{
				if (!File.Exists(InputFilePath))
				{
					ShowErrorMessage($"Файл {InputFilePath} не найден");
					return;
				}

				_fileProcessor.ProcessXlsx(InputFilePath);

				ShowInfoMessage($"Готово! Файл сохранен по адресу оригинала с суффиксом \"{GlobalConstants.ProcessedFileSuffix}\"");
			}
			catch (Exception ex)
			{
				ShowErrorMessage($"Произошла ошибка: {ex.Message}");
			}
		}

		private static void ShowInfoMessage(string message) =>
			MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

		private static void ShowErrorMessage(string message) =>
			MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

		private async void OpenSettingsWindow()
		{
			try
			{
				var settingsWindowVm = new SettingsWindowVm(_settings);

				var settingsWindow = new SettingsWindow
				{
					Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive),
					DataContext = settingsWindowVm
				};

				var settingsChanged = settingsWindow.ShowDialog() == true;
				if (!settingsChanged)
					return;

				_settings = settingsWindowVm.GetSettings();
				await IoUtils.SerializeSettingsToFile(_settings, ConfigPath);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Во время задания настроек произошла ошибка: {ex.Message}", "Ошибка");
			}
		}
		
		private async Task InitializeSettings()
		{
			try
			{
				var settingsFromFile = await IoUtils.DeserializeSettingsFromFile<CalculationSettings>(ConfigPath);
				if (settingsFromFile == null)
				{
					_settings = CalculationSettings.GetDefaultSettings();
					await IoUtils.SerializeSettingsToFile(_settings, ConfigPath);
				}
				else
					_settings = settingsFromFile;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Не удалось прочитать настройки: {ex.Message}, будут использованы настройки по умолчанию");
			}
		}

		private void ShutDown() => Environment.Exit(0);
	}
}