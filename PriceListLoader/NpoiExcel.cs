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
		public static void WritePriceListToSummary(string fileName, SiteInfo siteInfo) {
			string resultPath = Path.Combine(Environment.CurrentDirectory, "Results\\" + DateTime.Now.ToString("yyyyMMdd"));
			if (!Directory.Exists(resultPath))
				Directory.CreateDirectory(resultPath);

			string resultFile = Path.Combine(resultPath, "SummaryPrice_" +
				DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");

			IWorkbook workbook;
			try {
				using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
					workbook = new XSSFWorkbook(file);
				}
			} catch (Exception e) {
				Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				return;
			}

			int siteInfoColumnServiceName = -1;
			int siteInfoColumnServicePrice = -1;
			ISheet sheet = workbook.GetSheet("Data");
			for (int rowCount = 0; rowCount <= sheet.LastRowNum; rowCount++) {
				IRow row = sheet.GetRow(rowCount);

				if (row == null)  //null is when the row only contains empty cells 
					continue;

				if (rowCount == 0) {
					for (int columnCount = 3; columnCount < row.Cells.Count; columnCount += 3) {
						string currentSite = row.GetCell(columnCount).StringCellValue;
						if (!currentSite.Equals(siteInfo.SummaryColumnName))
							continue;

						siteInfoColumnServiceName = columnCount;
						siteInfoColumnServicePrice = columnCount + 1;
						break;
					}

					if (siteInfoColumnServiceName == -1) {
						Console.WriteLine("Не удалось найти столбец для сайта: " + siteInfo.SummaryColumnName);
						break;
					}

					continue;
				}

				if (rowCount == 85)
					Console.WriteLine();

				ICell cellName = row.GetCell(siteInfoColumnServiceName);
				if (cellName == null || cellName.CellType == CellType.Blank) {
					Console.WriteLine("Row: " + rowCount + " - value is empty");
					continue;
				}

				string siteInfoServiceName = cellName.StringCellValue;
				Console.WriteLine("Row: " + rowCount + " - value: " + siteInfoServiceName);
				ICell cellPrice = row.GetCell(siteInfoColumnServicePrice);
				object siteInfoServicePrice = siteInfo.GetServicePrice(siteInfoServiceName);
				if (siteInfoServicePrice is string) {
					string price = siteInfoServicePrice as string;
					if (price.Contains("*") || price.Contains("/"))
						cellPrice.SetCellFormula(price);
					else
						cellPrice.SetCellValue(price);
				} else if (siteInfoServicePrice is double)
					cellPrice.SetCellValue((double)siteInfoServicePrice);
			}


			using (FileStream stream = new FileStream(resultFile, FileMode.Create, FileAccess.Write))
				workbook.Write(stream);

			workbook.Close();
		}

		public static SiteInfo ReadPriceList(string fileName, SiteInfo.SiteName name) {
			SiteInfo siteInfo = new SiteInfo(name);
			
			IWorkbook workbook;
			try {
				using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
					workbook = new XSSFWorkbook(file);
				}
			} catch (Exception e) {
				Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				return siteInfo;
			}

			ISheet sheet = workbook.GetSheet("Data");
			for (int rowCount = 1; rowCount <= sheet.LastRowNum; rowCount++) {
				IRow row = sheet.GetRow(rowCount);

				if (row == null)  //null is when the row only contains empty cells 
					continue;

				string groupName = row.GetCell(0).StringCellValue;
				string groupLink = row.GetCell(1).StringCellValue;
				string serviceName = row.GetCell(2).StringCellValue;
				string servicePrice = 
					row.GetCell(3).CellType == CellType.String ? 
					row.GetCell(3).StringCellValue : 
					row.GetCell(3).NumericCellValue.ToString();

				int groupCounter = -1;
				for (int i = 0; i < siteInfo.ServiceGroupItems.Count; i++) {
					if (siteInfo.ServiceGroupItems[i].Name.Equals(groupName) &&
						siteInfo.ServiceGroupItems[i].Link.Equals(groupLink)) {
						groupCounter = i;
						break;
					}
				}

				if (groupCounter == -1) {
					ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
						Name = groupName,
						Link = groupLink
					};
					siteInfo.ServiceGroupItems.Add(itemServiceGroup);
					groupCounter = siteInfo.ServiceGroupItems.Count - 1;
				}

				ItemService itemService = new ItemService() {
					Name = serviceName,
					Price = servicePrice
				};

				siteInfo.ServiceGroupItems[groupCounter].ServiceItems.Add(itemService);
			}

			workbook.Close();

			return siteInfo;
		}

		public static string WriteItemSiteDataToExcel(SiteInfo siteInfo, BackgroundWorker backgroundWorker,
			double progressFrom, double progressTo) {
			double progressCurrent = progressFrom;

			string templateFile = Environment.CurrentDirectory + "\\Template.xlsx";
			string resultFilePrefix = siteInfo.UrlServicesPage;
			foreach (char item in Path.GetInvalidFileNameChars())
				resultFilePrefix = resultFilePrefix.Replace(item, '-');

			backgroundWorker.ReportProgress((int)progressCurrent, "Выгрузка данных в Excel");

			if (!File.Exists(templateFile)) {
				backgroundWorker.ReportProgress((int)progressCurrent);
				return "Не удалось найти файл шаблона: " + templateFile;
			}

			string resultPath = Path.Combine(Environment.CurrentDirectory, "Results\\" + DateTime.Now.ToString("yyyyMMdd"));
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

			double progressStep = (progressTo / 2 - progressCurrent) / siteInfo.ServiceGroupItems.Count;
			
			foreach (ItemServiceGroup group in siteInfo.ServiceGroupItems) {
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
