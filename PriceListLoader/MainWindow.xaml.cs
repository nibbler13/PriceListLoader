using System;
using System.Collections.Generic;
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
		public MainWindow() {
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			BackgroundWorker backgroundWorker = new BackgroundWorker();
			backgroundWorker.WorkerReportsProgress = true;
			backgroundWorker.DoWork += BackgroundWorker_DoWork;
			backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
			backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
			backgroundWorker.RunWorkerAsync();
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
			siteParser.ParseSelectedSites();
		}
	}
}
