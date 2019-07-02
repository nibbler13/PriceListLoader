using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace PriceListLoader {
	static class ExcelInterop {
		private static bool OpenWorkbook(string workbook, out Excel.Application xlApp, out Excel.Workbook wb, out Excel.Worksheet ws, string sheetName = "") {
			xlApp = null;
			wb = null;
			ws = null;

			xlApp = new Excel.Application();

			if (xlApp == null) {
				Logging.ToLog("Не удалось открыть приложение Excel");
				return false;
			}

			xlApp.Visible = false;

			wb = xlApp.Workbooks.Open(workbook);

			if (wb == null) {
				Logging.ToLog("Не удалось открыть книгу " + workbook);
				return false;
			}

			if (string.IsNullOrEmpty(sheetName))
				sheetName = "Данные";

			ws = wb.Sheets[sheetName];

			if (ws == null) {
				Logging.ToLog("Не удалось открыть лист Данные");
				return false;
			}

			return true;
		}

		private static void SaveAndCloseWorkbook(Excel.Application xlApp, Excel.Workbook wb, Excel.Worksheet ws) {
			if (ws != null) {
				Marshal.ReleaseComObject(ws);
				ws = null;
			}

			if (wb != null) {
				wb.Save();
				wb.Close(0);
				Marshal.ReleaseComObject(wb);
				wb = null;
			}

			if (xlApp != null) {
				xlApp.Quit();
				Marshal.ReleaseComObject(xlApp);
				xlApp = null;
			}

			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
		
		private static string GetExcelColumnName(int columnNumber) {
			int dividend = columnNumber;
			string columnName = String.Empty;
			int modulo;

			while (dividend > 0) {
				modulo = (dividend - 1) % 26;
				columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
				dividend = (int)((dividend - modulo) / 26);
			}

			return columnName;
		}


		public static void SetFormatting(string resultFile, BackgroundWorker bw) {
			double progressCurrent = 90;
			double progressStep = 0;
			bw.ReportProgress((int)progressCurrent, "Применение форматирования для книги: " + resultFile);

			if (!OpenWorkbook(resultFile, out Excel.Application xlApp, out Excel.Workbook wb,
				out Excel.Worksheet ws, "Data")) {
				bw.ReportProgress((int)progressCurrent, "Не удалось открыть книгу Excel");
				return;
			}

			try {
				int columnsUsed = ws.UsedRange.Columns.Count;
				int rowsUsed = ws.UsedRange.Rows.Count;

				progressStep = 10.0d / ((double)(columnsUsed - 4) / 3.0d * (rowsUsed - 1));

				for (int column = 5; column <= columnsUsed; column += 3) {
					int blockHeaderColorIndex = 43;
					for (int row = 2; row <= rowsUsed; row++) {
						bw.ReportProgress((int)progressCurrent);
						progressCurrent += progressStep;
						object priceValueObj = ws.Cells[row, column + 1].Value2;
						if (priceValueObj == null || string.IsNullOrEmpty(priceValueObj.ToString())) continue;

						string priceValue = priceValueObj.ToString();
						if (!priceValue.Equals("NOT_FOUND")) continue;

						ws.Range[GetExcelColumnName(column) + row.ToString() + ":" + GetExcelColumnName(column + 2) + row.ToString()].Interior.ColorIndex = 27;
						blockHeaderColorIndex = 27;
					}

					ws.Range[GetExcelColumnName(column) + "1:" + GetExcelColumnName(column + 2) + "1"].Interior.ColorIndex = blockHeaderColorIndex;
				}

				ws.Range["A1"].Select();
			} catch (Exception e) {
				Logging.ToLog(e.Message + Environment.NewLine + e.StackTrace);
			}

			bw.ReportProgress(100, "Сохранение книги Excel");
			SaveAndCloseWorkbook(xlApp, wb, ws);
		}
	}
}
