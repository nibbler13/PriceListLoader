using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PriceListLoader {
	class PriceSummary {
		public static void Test() {
			SiteInfo siteInfo = NpoiExcel.ReadPriceList(
				@"C:\_Projects C#\PriceListLoader\PriceListLoader\bin\Debug\Results\20180410\" +
				"Медси_Пироговка.xlsx", 
				SiteInfo.SiteName.medsi_ru);

			if (siteInfo.ServiceGroupItems.Count == 0) {
				MessageBox.Show("Не удалось считать услуги из файла");
				return;
			}

			NpoiExcel.WritePriceListToSummary(
				@"C:\_Projects C#\PriceListLoader\PriceListLoader\bin\Debug\Results\20180412\" +
				"SummaryPrice_20180412_001041.xlsx", 
				siteInfo);
			MessageBox.Show("Завершено");
		}
	}
}
