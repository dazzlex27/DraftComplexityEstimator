using Integration;
using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace ComplexityEstimator
{
	internal class MainWindowVm : BaseViewModel
	{
		private const string ProcessedFileSuffix = "_processed";

		private ExcelProcessor _excelProcessor;

		private string _inputFilePath;

		public MainWindowVm()
		{
			_excelProcessor = new ExcelProcessor();
			InputFilePath = "";
			SelectFileCommand = new CommandHandler(SelectFile, true);
			ProcessFileCommand = new CommandHandler(ProcessFile, true);
		}

		public ICommand SelectFileCommand { get; }
		public ICommand ProcessFileCommand { get; }

		public string InputFilePath
		{
			get => _inputFilePath;
			set => SetField(ref _inputFilePath, value, nameof(InputFilePath));
		}

		private void SelectFile()
		{
			try
			{
				var dialog = new OpenFileDialog
				{
					Filter = "xlsx files|*.xlsx",
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
			var outputFileName = "";

			try
			{
				if (!File.Exists(InputFilePath))
				{
					ShowErrorMessage($"Файл {InputFilePath} не найден");
					return;
				}

				outputFileName = GetCopyFileName(InputFilePath);
				_excelProcessor.ProcessXlsx(InputFilePath, outputFileName);

				ShowInfoMessage($"Готово! Файл сохранен по адресу оригинала с суффиксом \"{ProcessedFileSuffix}\"");
			}
			catch (Exception ex)
			{
				ShowErrorMessage($"Произошла ошибка: {ex.Message}");
				TryDeleteProcessedFile(outputFileName);
			}
		}

		private static void ShowInfoMessage(string message)
		{
			MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private static void ShowErrorMessage(string message)
		{
			MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private void TryDeleteProcessedFile(string processedFileName)
		{
			try
			{
				if (File.Exists(processedFileName))
					File.Delete(processedFileName);
			}
			catch { }
		}

		private string GetCopyFileName(string inputFilePath)
		{
			var baseDirectory = Path.GetDirectoryName(inputFilePath);
			var fileNameWithoutExt = Path.GetFileNameWithoutExtension(inputFilePath);
			var extension = Path.GetExtension(inputFilePath);
			var fileNameWithSuffix = $"{fileNameWithoutExt}{ProcessedFileSuffix}{extension}";
			var outputFileName = Path.Combine(baseDirectory, fileNameWithSuffix);
			TryDeleteProcessedFile(outputFileName);

			return outputFileName;
		}
	}
}