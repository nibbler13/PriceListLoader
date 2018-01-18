using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PriceListLoader {
	class NpoiExcel {
		public static string WriteItemSiteDataToExcel(ItemSiteData itemSiteData, BackgroundWorker backgroundWorker,
			double progressFrom, double progressTo) {
			double progressCurrent = progressFrom;

			string templateFile = Environment.CurrentDirectory + "\\Template.xlsx";
			string resultFilePrefix = itemSiteData.SiteAddress;
			foreach (char item in Path.GetInvalidFileNameChars())
				resultFilePrefix = resultFilePrefix.Replace(item, '-');

			backgroundWorker.ReportProgress((int)progressCurrent, "Выгрузка данных в Excel");

			if (!File.Exists(templateFile)) {
				backgroundWorker.ReportProgress((int)progressCurrent);
				return "Не удалось найти файл шаблона: " + templateFile;
			}

			string resultPath = Path.Combine(Environment.CurrentDirectory, "Results");
			if (!Directory.Exists(resultPath))
				Directory.CreateDirectory(resultPath);

			string resultFile = Path.Combine(resultPath, resultFilePrefix + "_" +
				DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
			
			IWorkbook workbook;
			using (FileStream stream = new FileStream(templateFile, FileMode.Open, FileAccess.Read))
				workbook = new XSSFWorkbook(stream);

			progressCurrent += 10;
			backgroundWorker.ReportProgress((int)progressCurrent);

			int rowNumber = 1;
			int columnNumber = 0;

			ISheet sheet = workbook.GetSheet("Data");

			double progressStep = (progressTo / 2 - progressCurrent) / itemSiteData.ServiceGroupItems.Count;
			
			foreach (ItemServiceGroup group in itemSiteData.ServiceGroupItems) {
				backgroundWorker.ReportProgress((int)progressCurrent);
				progressCurrent += progressStep;

				foreach (ItemService service in group.ServiceItems) {
					IRow row = sheet.CreateRow(rowNumber);
					string[] values = new string[] { group.Name, group.Link, service.Name, service.Price };

					foreach (string value in values) {
						ICell cell = row.CreateCell(columnNumber);

						if (double.TryParse(value, out double result))
							cell.SetCellValue(result);
						else
							cell.SetCellValue(value);

						columnNumber++;
					}

					columnNumber = 0;
					rowNumber++;
				}
			}

			progressStep = (progressTo - progressCurrent) / 9;

			progressCurrent += progressStep;
			backgroundWorker.ReportProgress((int)progressCurrent);

			using (FileStream stream = new FileStream(resultFile, FileMode.Create, FileAccess.Write))
				workbook.Write(stream);

			progressCurrent += progressStep;
			backgroundWorker.ReportProgress((int)progressCurrent);

			workbook.Close();

			progressCurrent += progressStep;
			backgroundWorker.ReportProgress((int)progressCurrent);

			return resultFile;
		}
	}
}
