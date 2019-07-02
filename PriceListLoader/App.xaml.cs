using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PriceListLoader {
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary>
	public partial class App : Application {
		private string autoModeResult = string.Empty;
		private string delimiter = Environment.NewLine + new string('-', 20) +
			Environment.NewLine;
		public static string errorPrefix = "!!!!!-----Ошибка-----!!!!! ";

		private void Application_Startup(object sender, StartupEventArgs e) {
			if (e.Args.Length > 0 && e.Args[0].ToLower().Equals("auto")) {
				Logging.ToLog("---Автоматическая выгрузка прайс-листов для всех сайтов");
				//MessageBox.Show(Logging.ASSEMBLY_DIRECTORY);
				LoadAllSites();
				Shutdown();
				Logging.ToLog("---Завершение работы");
			}

			MainWindow window = new MainWindow();
			window.Show();
		}

		private void LoadAllSites() {
			BackgroundWorker backgroundWorker = new BackgroundWorker {
				WorkerReportsProgress = true
			};

			backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged; 
			SiteParser siteParser = new SiteParser(backgroundWorker);

			autoModeResult += "Автоматическая загрузка прайс-листов. Организаций в списке: " + SiteInfo.CitySitesMap.Values.Count + delimiter;

			foreach (KeyValuePair<Enums.Cities, Type> keyValuePair in SiteInfo.CitySitesMap) {
                foreach (int siteValue in Enum.GetValues(keyValuePair.Value)) {
                    SiteInfo siteInfo = new SiteInfo(keyValuePair.Key, siteValue);
                    if (!siteInfo.ShouldAutoLoad) continue;

                    autoModeResult += siteInfo.CityName + " | " + siteInfo.CompanyName + " | " +
                        siteInfo.UrlServicesPage + Environment.NewLine;
                    string resultFile = siteParser.ParseSelectedSite(siteInfo, true);
                    autoModeResult += "Получено групп услуг: " + siteInfo.ServiceGroupItems.Count + Environment.NewLine;

                    if (siteInfo.ServiceGroupItems.Count == 0 || string.IsNullOrEmpty(resultFile))
                        autoModeResult += errorPrefix + "Не удалось получить список услуг";
                    else
                        autoModeResult += "Файл с прайс-листом сохранен по адресу: " + resultFile;

                    autoModeResult += delimiter;
                }
			}

			autoModeResult += "Завершение работы, подробности в локальном журнале работы";

			SystemMail.SendMail("Загрузка прайс-листов", autoModeResult, PriceListLoader.Properties.Settings.Default.MailTo);
		}

		private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			if (e.UserState != null) {
				string state = e.UserState.ToString();
				Logging.ToLog(state);
				autoModeResult += state + Environment.NewLine;
			}
		}
	}
}
