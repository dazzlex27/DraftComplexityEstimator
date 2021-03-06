using ImageProcessor.Native;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageProcessor
{
	public class FileProcessor : IDisposable
	{
		private readonly object _lock;
		private string _debugDirectoryPath;

		public FileProcessor()
		{
			_lock = new object();
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
			ImageProcessingDll.CreateImageProcessor();
		}

		public void Dispose() => ImageProcessingDll.DestroyImageProcessor();

		public void ProcessXlsx(string filePath)
		{
			var outputPath = "";
			
			try
			{
				using (var fs = File.OpenRead(filePath))
				using (var package = new ExcelPackage(fs))
				{
					foreach (var sheet in package.Workbook.Worksheets)
					{
						if (sheet.Name.ToLower().Contains("пусто"))
							continue;

						var calculationSamples = GetSamplesFromSheet(sheet);
						SaveResultsToSheet(sheet, calculationSamples);
					}

					outputPath = GetCopyFileName(filePath);
					var outputFileInfo = new FileInfo(outputPath);
					package.SaveAs(outputFileInfo);
				}
			}
			catch (Exception)
			{
				if (!string.IsNullOrEmpty(outputPath))
					TryDeleteProcessedFile(outputPath);

				throw;
			}
		}

		public void SetCalculationSettings(CalculationSettings settings)
		{
			var parameters = new ComplexityCalculationParams
			{
				Multiplier = settings.Multiplier,
				Power = settings.Power,
				GainFactor = settings.GainFactor,
				SmallDetailGainFactor = settings.SmallDetailGainFactor,
				PerformanceRate = settings.PerformanceRate
			};

			ImageProcessingDll.SetCalculationParams(parameters);
			_debugDirectoryPath = settings.DebugDirectoryPath;
			ImageProcessingDll.SetDebugParams(settings.EnableDebug, _debugDirectoryPath);
		}

		private Dictionary<string, CalculationSample> GetSamplesFromSheet(ExcelWorksheet sheet)
		{
			try
			{
				var calculationSamples = new Dictionary<string, CalculationSample>();

				var lkDrawings = sheet.Drawings.ToLookup(x => $"{x.From.Row}_{x.From.Column}");

				foreach (var drawing in lkDrawings)
				{
					var drawingKey = drawing.Key;

					var image = lkDrawings[drawingKey].ToList()[0] as ExcelPicture;
					var innerBitmap = image?.Image as Bitmap;
					if (innerBitmap == null)
						continue;

					var (row, col) = ParseImageLookupKey(drawingKey);
					var dimensionsString = sheet.Cells[row + 1, col + 2].Value.ToString();
					var (width, height) = ParsePartDimensions(dimensionsString);

					ResetDebugDirectory();
					var imageData = ImageUtils.GetImageDataFromBitmap(innerBitmap);
					var complexityInfo = CalculateComplexity(imageData, drawingKey, width, height);

					calculationSamples.Add(drawingKey, new CalculationSample(imageData, complexityInfo));
				}

				return calculationSamples;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);

				return new Dictionary<string, CalculationSample>();
			}
		}

		private static void SaveResultsToSheet(ExcelWorksheet sheet, Dictionary<string, CalculationSample> calculationSamples)
		{
			const int laborIntensityCol = 8; // H
			const int totalComplexityCol = laborIntensityCol + 1; // I
			const int calculatedComplexityCol = totalComplexityCol + 1; // J
			const int presumedComplexityCol = calculatedComplexityCol + 1; // K

			sheet.Cells[1, laborIntensityCol].Value = "T";
			sheet.Cells[1, totalComplexityCol].Value = "H";
			sheet.Cells[1, calculatedComplexityCol].Value = "Ht";
			sheet.Cells[1, presumedComplexityCol].Value = "Hp";

			foreach (var key in calculationSamples.Keys)
			{
				var keyItems = ParseImageLookupKey(key);
				var row = keyItems.Item1 + 1;
				var col = keyItems.Item2;

				var rowToWrite = col == 0 ? row : row + 1;

				var info = calculationSamples[key].ComplexityInfo;

				sheet.Cells[rowToWrite, laborIntensityCol].Value = info.LaborIntensity;
				sheet.Cells[rowToWrite, totalComplexityCol].Value = info.TotalComplexity;
				sheet.Cells[rowToWrite, calculatedComplexityCol].Value = info.CalculatedComplexity;
				sheet.Cells[rowToWrite, presumedComplexityCol].Value = info.PresumedComplexity;
			}
		}

		private ComplexityInfo CalculateComplexity(ImageData image, string drawingKey, int partWidth, int partHeight)
		{
			lock (_lock)
			{
				unsafe
				{
					fixed (byte* colorData = image.Data)
					{
						var nativeColorImage = new ColorImage
						{
							Width = image.Width,
							Height = image.Height,
							Data = colorData,
							BytesPerPixel = image.BytesPerPixel
						};

						var calculationData = new ComplexityCalculationData
						{
							ColorImage = &nativeColorImage,
							PartWidth = partWidth,
							PartHeight = partHeight,
							DebugFileName = $"{DateTime.Now.Ticks}_{drawingKey}"
						};

						var calculationResult = ImageProcessingDll.CalculateObjectComplexity(calculationData);

						var laborIntensityMinutes = -1.0f;
						var totalComplexity = -1.0f;
						var calculatedComplexity = -1.0f;
						var presumedComplexity = -1.0f;

						if (calculationResult->Status == ComplexityCalculationStatus.Success)
						{
							laborIntensityMinutes = calculationResult->LaborIntensityMinutes;
							totalComplexity = calculationResult->TotalComplexity;
							calculatedComplexity = calculationResult->CalculatedComplexity;
							presumedComplexity = calculationResult->PresumedComplexity;
						}

						ImageProcessingDll.DisposeCalculationResult(calculationResult);

						return new ComplexityInfo(laborIntensityMinutes, totalComplexity, calculatedComplexity,
							presumedComplexity);
					}
				}
			}
		}

		private void ResetDebugDirectory()
		{
			try
			{
				if (string.IsNullOrEmpty(_debugDirectoryPath))
					return;

				if (Directory.Exists(_debugDirectoryPath))
					Directory.Delete(_debugDirectoryPath, true);
				Directory.CreateDirectory(_debugDirectoryPath);
			}
			catch
			{
			}
		}

		private static Tuple<int, int> ParseImageLookupKey(string key)
		{
			var keyTokens = key.Split('_');

			var row = int.Parse(keyTokens[0]);
			var col = int.Parse(keyTokens[1]);

			return new Tuple<int, int>(row, col);
		}

		private static Tuple<int, int> ParsePartDimensions(string str)
		{
			var keyTokens = str.Split(" x ");

			var width = int.Parse(keyTokens[0]);
			var height = int.Parse(keyTokens[1]);

			return new Tuple<int, int>(width, height);
		}

		private string GetCopyFileName(string inputFilePath)
		{
			var baseDirectory = Path.GetDirectoryName(inputFilePath);
			var fileNameWithoutExt = Path.GetFileNameWithoutExtension(inputFilePath);
			var extension = Path.GetExtension(inputFilePath);
			var fileNameWithSuffix = $"{fileNameWithoutExt}{GlobalConstants.ProcessedFileSuffix}{extension}";
			var outputFileName = Path.Combine(baseDirectory, fileNameWithSuffix);
			TryDeleteProcessedFile(outputFileName);

			return outputFileName;
		}
		
		private static void TryDeleteProcessedFile(string processedFileName)
		{
			try
			{
				if (File.Exists(processedFileName))
					File.Delete(processedFileName);
			}
			catch { }
		}
	}
}