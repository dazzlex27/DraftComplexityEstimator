using ImageProcessor.Native;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageProcessor
{
	public class ExcelProcessor : IDisposable
	{
		private object _lock;
		private string _debugDirectory;

		public ExcelProcessor()
		{
			_lock = new object();
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
			ImageProcessingDll.CreateImageProcessor();

			var exeDir = AppDomain.CurrentDomain.BaseDirectory;
			_debugDirectory = Path.Combine(exeDir, "output");
			if (Directory.Exists(_debugDirectory))
				Directory.Delete(_debugDirectory, true);
			Directory.CreateDirectory(_debugDirectory);
			ImageProcessingDll.SetDebugPath(_debugDirectory);
		}

		public void Dispose()
		{
			ImageProcessingDll.DestroyImageProcessor();
		}

		public void ProcessXlsx(string filePath, string outputPath)
		{
			var ms = new MemoryStream();
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

				var outputFileInfo = new FileInfo(outputPath);
				package.SaveAs(outputFileInfo);
			}
		}

		private Dictionary<string, CalculationSample> GetSamplesFromSheet(ExcelWorksheet sheet)
		{
			try
			{
				var calculationSamples = new Dictionary<string, CalculationSample>();

				var lkDrawings = sheet.Drawings.ToLookup(x => $"{ x.From.Row}_{x.From.Column}");

				foreach (var drawing in lkDrawings)
				{
					var drawingKey = drawing.Key;

					ExcelPicture image = lkDrawings[drawingKey].ToList()[0] as ExcelPicture;
					if (image == null)
						continue;

					var innerBitmap = image.Image as Bitmap;
					if (innerBitmap == null)
						continue;

					var imageData = ImageUtils.GetImageDataFromBitmap(innerBitmap);
					//var debugImagePath = $"{drawingKey}_{DateTime.Now.Ticks}.png";
					//ImageUtils.SaveImageDataToFile(imageData, Path.Combine(_debugDirectory, debugImagePath));

					var complexityInfo = CalculateComplexity(imageData, drawingKey);

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

		private void SaveResultsToSheet(ExcelWorksheet sheet, Dictionary<string, CalculationSample> calculationSamples)
		{
			const int colToWrite = 8;
			sheet.Cells[1, colToWrite].Value = "Сложность";
			sheet.Cells[1, colToWrite + 1].Value = "Ширина";
			sheet.Cells[1, colToWrite + 2].Value = "Высота";

			foreach (var key in calculationSamples.Keys)
			{
				var keyTokens = key.Split('_');

				var row = int.Parse(keyTokens[0]) + 1;
				var col = int.Parse(keyTokens[1]);

				var rowToWrite = col == 0 ? row : row + 1;

				var info = calculationSamples[key].ComplexityInfo;

				sheet.Cells[rowToWrite, colToWrite].Value = info.Complexity;
				sheet.Cells[rowToWrite, colToWrite + 1].Value = info.ContourWidth;
				sheet.Cells[rowToWrite, colToWrite + 2].Value = info.ContourHeight;
			}
		}

		private ComplexityInfo CalculateComplexity(ImageData image, string drawingKey)
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
							DebugFileName = $"{drawingKey}_{DateTime.Now.Ticks}_native"
						};

						var calculationResult = ImageProcessingDll.CalculateObjectComplexity(calculationData);

						var complexity = -1.0f;
						var contourWidth = -1;
						var contourHeight = -1;

						if (calculationResult->Status == ComplexityCalculationStatus.Success)
						{
							complexity = calculationResult->Complexity;
							contourWidth = calculationResult->ContourWidth;
							contourHeight = calculationResult->ContourHeight;
						}

						ImageProcessingDll.DisposeCalculationResult(calculationResult);

						return new ComplexityInfo(complexity, contourWidth, contourHeight);
					}
				}
			}
		}
	}
}