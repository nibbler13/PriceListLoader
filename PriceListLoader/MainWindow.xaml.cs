using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PriceListLoader {
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public ObservableCollection<SiteInfo> SiteItems { get; set; } = new ObservableCollection<SiteInfo>();

		public MainWindow() {
			InitializeComponent();

			//ListViewSites.DataContext = this;
			DataGridSites.DataContext = this;

			foreach (SiteInfo.SiteName name in Enum.GetValues(typeof(SiteInfo.SiteName)))
				SiteItems.Add(new SiteInfo(name));
			
		}

		private void ButtonExecute_Click(object sender, RoutedEventArgs e) {
			BackgroundWorker backgroundWorker = new BackgroundWorker {
				WorkerReportsProgress = true
			};

			backgroundWorker.DoWork += BackgroundWorker_DoWork;
			backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
			backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
			backgroundWorker.RunWorkerAsync(DataGridSites.SelectedItems);
			IsEnabled = false;
		}

		private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			ProgressBarResult.Value = e.ProgressPercentage;

			if (e.UserState != null)
				TextBoxResult.Text = DateTime.Now.ToShortTimeString() + ": " + e.UserState as string +
					Environment.NewLine + TextBoxResult.Text;
		}

		private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			if (e.Error != null) {
				TextBoxResult.Text = e.Error.Message + Environment.NewLine + e.Error.StackTrace +
					Environment.NewLine + TextBoxResult.Text;
			} else {
				TextBoxResult.Text = "Завершено" + Environment.NewLine + TextBoxResult.Text;
			}

			IsEnabled = true;

			MessageBox.Show(this, "Завершено", "", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
			SiteParser siteParser = new SiteParser(sender as BackgroundWorker);

			foreach (SiteInfo siteInfo in (System.Collections.IList)e.Argument)
				siteParser.ParseSelectedSite(siteInfo);
		}

		private void DataGridSites_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ButtonExecute.IsEnabled = DataGridSites.SelectedItems.Count > 0;
		}

		private void ButtonDo_Click(object sender, RoutedEventArgs e) {
			PriceSummary.Test();
		}
	}
}
