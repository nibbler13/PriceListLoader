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
		public static void WritePriceListToSummary(string templateFileName, List<SiteInfo> sitesInfo, BackgroundWorker backgroundWorker, bool LoadBzPrices) {
			backgroundWorker.ReportProgress(50, "Заполнение шаблона сводной таблицы");
			string resultPath = Path.Combine(Environment.CurrentDirectory, "Results\\" + DateTime.Now.ToString("yyyyMMdd"));
			if (!Directory.Exists(resultPath))
				Directory.CreateDirectory(resultPath);

			string resultFile = Path.Combine(resultPath, "SummaryPrice_" +
				DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");

			IWorkbook workbook;
			try {
				backgroundWorker.ReportProgress(55, "Открытие книги: " + templateFileName);
				using (FileStream file = new FileStream(templateFileName, FileMode.Open, FileAccess.Read))
					workbook = new XSSFWorkbook(file);
			} catch (Exception e) {
				Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				return;
			}

			double progressCurrent = 55;
			ISheet sheet = workbook.GetSheet("Data");
			if (LoadBzPrices)
				WriteBzPricesToSummaryTable(sheet, sitesInfo[0].GetFilId(), backgroundWorker, (int)progressCurrent);
			else
				backgroundWorker.ReportProgress((int)progressCurrent, "Пропуск загрузки собственных цен");

			double progressStep = 40.0d / (double)sitesInfo.Count;
			foreach (SiteInfo siteInfo in sitesInfo) {
				progressCurrent += progressStep;
				backgroundWorker.ReportProgress((int)progressCurrent, "Обработка информации для сайта: " + siteInfo.CompanyName + " - " + 
					siteInfo.SummaryColumnName + " - " + siteInfo.SelectedPriceListFile);

				if (string.IsNullOrEmpty(siteInfo.SelectedPriceListFile)) {
					backgroundWorker.ReportProgress((int)progressCurrent, "Не был выбран прайс-лист, пропуск");
					continue;
				}

				int siteInfoColumnServiceName = -1;
				int siteInfoColumnServicePrice = -1;

				for (int rowCount = 0; rowCount <= sheet.LastRowNum; rowCount++) {
					IRow row = sheet.GetRow(rowCount);

					if (row == null)  //null is when the row only contains empty cells 
						continue;

					if (rowCount == 0) {
						backgroundWorker.ReportProgress((int)progressCurrent, "Поиск столбца с именем: " + siteInfo.SummaryColumnName);
						for (int columnCount = 4; columnCount < row.Cells.Count; columnCount += 3) {
							string currentSite = row.GetCell(columnCount).StringCellValue;
							if (!currentSite.Equals(siteInfo.SummaryColumnName))
								continue;

							siteInfoColumnServiceName = columnCount;
							siteInfoColumnServicePrice = columnCount + 1;
							break;
						}

						if (siteInfoColumnServiceName == -1) {
							backgroundWorker.ReportProgress((int)progressCurrent, "Не удалось найти столбец с именем: " + siteInfo.SummaryColumnName);
							break;
						}

						continue; //move to next row after headers
					}

					ICell cellName = row.GetCell(siteInfoColumnServiceName);
					if (cellName == null || cellName.CellType == CellType.Blank) 
						continue;

					string siteInfoServiceName = cellName.StringCellValue;
					ICell cellPrice = row.GetCell(siteInfoColumnServicePrice);
					object siteInfoServicePrice = siteInfo.GetServicePrice(siteInfoServiceName);

					if (siteInfoServicePrice.Equals("NOT_FOUND"))
						backgroundWorker.ReportProgress((int)progressCurrent, "--- Не удалось найти сопоставление для услуги: '" + siteInfoServiceName + "'");

					try {
						if (siteInfoServicePrice is string) {
							string price = siteInfoServicePrice as string;
							if ((price.Contains("*") && !price.EndsWith("*")) || price.Contains("/"))
								cellPrice.SetCellFormula(price);
							else
								cellPrice.SetCellValue(price);
						} else if (siteInfoServicePrice is double)
							cellPrice.SetCellValue((double)siteInfoServicePrice);
					} catch (Exception e) {
						Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
					}
				}
			}

			backgroundWorker.ReportProgress(100, "Запись данных в файл: " + resultFile);
			using (FileStream stream = new FileStream(resultFile, FileMode.Create, FileAccess.Write))
				workbook.Write(stream);

			workbook.Close();
		}

		private static void WriteBzPricesToSummaryTable(ISheet sheet, int filid, BackgroundWorker backgroundWorker, int progress) {
			backgroundWorker.ReportProgress(progress, "Загрузка собственных цен из базы Инфоклиники");
			string dbAddress = Properties.Settings.Default.MisDbAddress;
			string dbName = Properties.Settings.Default.MisDbName;
			string sqlQuery = Properties.Settings.Default.MisDbSelectPriceByCode;

			backgroundWorker.ReportProgress(progress, "Создание подключения к БД: " + dbAddress + ":" + dbName);
			FirebirdClient firebirdClient = new FirebirdClient(
				dbAddress,
				dbName,
				Properties.Settings.Default.MisDbUser,
				Properties.Settings.Default.MisDbPassword);

			backgroundWorker.ReportProgress(progress, "Выполнение запроса на получения цен для услуг (" + 
				(sheet.LastRowNum - 1) + " шт.)");
			for (int rowCount = 1; rowCount <= sheet.LastRowNum; rowCount++) {
				IRow row = sheet.GetRow(rowCount);

				if (row == null)  //null is when the row only contains empty cells 
					continue;
				
				string kodoper = GetCellValue(row.GetCell(1));
				//string schname = GetCellValue(row.GetCell(2));
				if (string.IsNullOrEmpty(kodoper))// || string.IsNullOrEmpty(schname))
					continue;

				DataTable dataTable = firebirdClient.GetDataTable(sqlQuery, new Dictionary<string, object>() {
					{ "@filid", filid },
					{ "@kodoper", kodoper }
     //               ,
					//{ "@schname", schname }
				});

				if (dataTable.Rows.Count == 0)
					continue;

				string price = dataTable.Rows[0][0].ToString();
				ICell cellPrice = row.GetCell(3);

				if (double.TryParse(price, out double result))
					cellPrice.SetCellValue(result);
				else
					cellPrice.SetCellValue(price);
			}

			firebirdClient.Close();
		}

		private static string GetCellValue(ICell cell) {
			return cell.CellType == CellType.String ? cell.StringCellValue : cell.NumericCellValue.ToString();
		}

		public static void ReadPriceList(SiteInfo siteInfo) {
			if (string.IsNullOrEmpty(siteInfo.SelectedPriceListFile)) {
				Console.WriteLine("siteInfo: " + siteInfo.Name + " - SelectedPriceListFile is empty");
				return;
			}

			IWorkbook workbook;
			try {
				using (FileStream file = new FileStream(siteInfo.SelectedPriceListFile, FileMode.Open, FileAccess.Read)) {
					workbook = new XSSFWorkbook(file);
				}
			} catch (Exception e) {
				Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				return;
			}

			ISheet sheet = workbook.GetSheet("Data");
			for (int rowCount = 1; rowCount <= sheet.LastRowNum; rowCount++) {
				IRow row = sheet.GetRow(rowCount);

				if (row == null)  //null is when the row only contains empty cells 
					continue;

				string groupName = string.Empty;
				string groupLink = string.Empty;
				string serviceName = string.Empty;
				string servicePrice = string.Empty;

				try {
					groupName = GetCellValue(row.GetCell(0));
					groupLink = GetCellValue(row.GetCell(1));
					serviceName = GetCellValue(row.GetCell(2));
					servicePrice = GetCellValue(row.GetCell(3));
				} catch (Exception e) {
					Console.WriteLine("row: " + rowCount + ", " + e.Message + Environment.NewLine + e.StackTrace);
					continue;
				}

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

			return;
		}

		public static string GetFileName(string siteName) {
			string fileName = siteName;

			foreach (char item in Path.GetInvalidFileNameChars())
				fileName = fileName.Replace(item, '-');

			return fileName;
		}

		public static string WriteItemSiteDataToExcel(SiteInfo siteInfo, BackgroundWorker backgroundWorker,
			double progressFrom, double progressTo) {
			double progressCurrent = progressFrom;

			string templateFile = Path.Combine(Logging.ASSEMBLY_DIRECTORY, "Template.xlsx");
			string resultFilePrefix = GetFileName(siteInfo.UrlServicesPage);

			backgroundWorker.ReportProgress((int)progressCurrent, "Выгрузка данных в Excel");

			if (!File.Exists(templateFile)) {
				backgroundWorker.ReportProgress((int)progressCurrent);
				return "Не удалось найти файл шаблона: " + templateFile;
			}

			string resultPath = Path.Combine(Logging.ASSEMBLY_DIRECTORY, "Results\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + siteInfo.City);
			if (!Directory.Exists(resultPath))
				try {
					Directory.CreateDirectory(resultPath);
				} catch (Exception e) {
					Logging.ToLog(e.Message + Environment.NewLine + e.StackTrace);
				}

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
