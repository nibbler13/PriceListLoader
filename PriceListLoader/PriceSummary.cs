using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PriceListLoader {
	class PriceSummary {
		public static void Test(ObservableCollection<SiteInfo> pivotTableItems, string templateFile, BackgroundWorker backgroundWorker, bool LoadBzPrices) {
			backgroundWorker.ReportProgress(0, "Считывание прайс-листов");
			double progressCurrent = 0;
			double progressStep = 45.0d / (double)pivotTableItems.Count;
			foreach (SiteInfo siteInfo in pivotTableItems) {
				progressCurrent += progressStep;
				if (string.IsNullOrEmpty(siteInfo.SelectedPriceListFile)) {
					backgroundWorker.ReportProgress((int)progressCurrent, "Для сайта " + siteInfo.CompanyName + " не выбран файл с прайс-листом, пропуск");
					continue;
				}

				backgroundWorker.ReportProgress((int)progressCurrent, siteInfo.CompanyName);

				int serviceCountCurrent = NpoiExcel.ReadPriceList(siteInfo);
				backgroundWorker.ReportProgress((int)progressCurrent,  "считано услуг: " + serviceCountCurrent +
					" - " + siteInfo.SelectedPriceListFile);

				if (siteInfo.ServiceGroupItems.Count == 0) 
					backgroundWorker.ReportProgress((int)progressCurrent, "!!! Внимание! Не считано ни одной группы услуг");
			}

			NpoiExcel.WritePriceListToSummary(templateFile, pivotTableItems.ToList(), backgroundWorker, LoadBzPrices);

			MessageBox.Show("Завершено");
		}
	}
}
