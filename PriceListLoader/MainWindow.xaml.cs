using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
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
		public ObservableCollection<SiteInfo> PivotTableItems { get; set; } = new ObservableCollection<SiteInfo>();

		public MainWindow() {
			InitializeComponent();

			//ListViewSites.DataContext = this;
			DataGridSites.DataContext = this;
			ListViewPivotTable.DataContext = this;

			foreach (SiteInfo.SiteName name in Enum.GetValues(typeof(SiteInfo.SiteName)))
				SiteItems.Add(new SiteInfo(name));

			foreach (SiteInfo siteInfo in SiteItems)
				if (!ListBoxRegions.Items.Contains(siteInfo.City))
					ListBoxRegions.Items.Add(siteInfo.City);
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

			if (e.UserState != null) {
				Console.WriteLine(e.UserState as string);
				TextBoxResult.Text = DateTime.Now.ToShortTimeString() + ": " + e.UserState as string +
					Environment.NewLine + TextBoxResult.Text;
			}
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
			TextBoxPivotTableResult.Text = string.Empty;
			Cursor = Cursors.Wait;

			IsEnabled = false;

			BackgroundWorker backgroundWorkerPivotTable = new BackgroundWorker();
			backgroundWorkerPivotTable.WorkerReportsProgress = true;
			backgroundWorkerPivotTable.DoWork += BackgroundWorkerPivotTable_DoWork;
			backgroundWorkerPivotTable.RunWorkerCompleted += BackgroundWorkerPivotTable_RunWorkerCompleted;
			backgroundWorkerPivotTable.ProgressChanged += BackgroundWorkerPivotTable_ProgressChanged;
			backgroundWorkerPivotTable.RunWorkerAsync(new object[] { TextBoxPivotTableTemplate.Text, CheckboxLoadBzPrices.IsChecked.Value });
		}

		private void BackgroundWorkerPivotTable_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			ProgressBarPivotTable.Value = e.ProgressPercentage;

			string text = e.UserState as string;
			if (!string.IsNullOrEmpty(text))
				TextBoxPivotTableResult.Text = text + Environment.NewLine + TextBoxPivotTableResult.Text;
		}

		private void BackgroundWorkerPivotTable_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			if (e.Error != null) {
				TextBoxPivotTableResult.Text = e.Error.Message + Environment.NewLine + e.Error.StackTrace +
					Environment.NewLine + TextBoxPivotTableResult.Text;
			}

			IsEnabled = true;

			Cursor = Cursors.Arrow;
		}

		private void BackgroundWorkerPivotTable_DoWork(object sender, DoWorkEventArgs e) {
			object[] args = e.Argument as object[];
			string templateFile = args[0] is string ? args[0] as string : string.Empty;
			bool loadBzPrices = args[1] is bool ? (bool)args[1] : false;
			PriceSummary.Test(PivotTableItems, templateFile, sender as BackgroundWorker, loadBzPrices);
		}

		private void ListBoxRegions_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			PivotTableItems.Clear();

			foreach (SiteInfo siteInfo in SiteItems)
				if (siteInfo.City.Equals(ListBoxRegions.SelectedItem))
					PivotTableItems.Add(siteInfo);

			string pivotTableTemplate = TextBoxPivotTableTemplate.Text;
			if (!string.IsNullOrEmpty(pivotTableTemplate) &&
				!pivotTableTemplate.Contains("Template"))
				return;

			string selectedRegionTemplate = string.Empty;
			switch (ListBoxRegions.SelectedItem) {
				case "Москва":
					selectedRegionTemplate = "TemplateSummaryMoscow.xlsx";
					break;
				case "Санкт-Петербург":
					selectedRegionTemplate = "TemplateSummarySpb.xlsx";
					break;
				case "Уфа":
					selectedRegionTemplate = "TemplateSummaryUfa.xlsx";
					break;
				case "Каменск-Уральский":
					selectedRegionTemplate = "TemplateSummaryKamenskUralsky.xlsx";
					break;
				case "Казань":
					selectedRegionTemplate = "TemplateSummaryKazan.xlsx";
					break;
				case "Краснодар":
					selectedRegionTemplate = "TemplateSummaryKrasnodar.xlsx";
					break;
				case "Сочи":
					selectedRegionTemplate = "TemplateSummarySochi.xlsx";
					break;
				default:
					break;
			}

			if (!string.IsNullOrEmpty(selectedRegionTemplate))
				selectedRegionTemplate = System.IO.Path.Combine(Environment.CurrentDirectory, selectedRegionTemplate);

			TextBoxPivotTableTemplate.Text = selectedRegionTemplate;
		}

		private void ButtonSelectFile_Click(object sender, RoutedEventArgs e) {
			ListViewItem listViewItem = GetAncestorOfType<ListViewItem>(sender as Button);

			if (listViewItem == null)
				return;

			if (!(listViewItem.Content is SiteInfo siteInfo))
				return;

			if (SelectXlsxFile(out string selectedFile))
				siteInfo.SelectedPriceListFile = selectedFile;
		}

		private void ButtonClearSelected_Click(object sender, RoutedEventArgs e) {
			ListViewItem listViewItem = GetAncestorOfType<ListViewItem>(sender as Button);

			if (listViewItem == null)
				return;

			if (!(listViewItem.Content is SiteInfo siteInfo))
				return;
			
			siteInfo.SelectedPriceListFile = string.Empty;
		}

		public T GetAncestorOfType<T>(FrameworkElement child) where T : FrameworkElement {
			var parent = VisualTreeHelper.GetParent(child);
			if (parent != null && !(parent is T))
				return (T)GetAncestorOfType<T>((FrameworkElement)parent);
			return (T)parent;
		}

		private void ButtonSelectPivotTableTemplate_Click(object sender, RoutedEventArgs e) {
			if (SelectXlsxFile(out string selectedFile))
				TextBoxPivotTableTemplate.Text = selectedFile;
		}

		private void ButtonSelectFolderWithPrices_Click(object sender, RoutedEventArgs e) {
			using (CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()) {
				openFileDialog.IsFolderPicker = true;

				if (!string.IsNullOrEmpty(TextBoxFolderWithPrices.Text))
					openFileDialog.InitialDirectory = TextBoxFolderWithPrices.Text;

				if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
					TextBoxFolderWithPrices.Text = openFileDialog.FileName;
			}
		}

		private bool SelectXlsxFile(out string selectedFile) {
			selectedFile = string.Empty;

			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Книга Excel (*.xls*)|*.xls*";
			openFileDialog.CheckFileExists = true;
			openFileDialog.CheckPathExists = true;
			openFileDialog.Multiselect = false;
			openFileDialog.RestoreDirectory = true;

			if (openFileDialog.ShowDialog() == true)
				selectedFile = openFileDialog.FileName;
			else
				return false;

			return true;
		}

		private void ButtonMatchFiles_Click(object sender, RoutedEventArgs e) {
			if (PivotTableItems.Count == 0) {
				MessageBox.Show(this, "Отсутствует информация о прайс-листах для выбранного региона",
					"", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			if (ListBoxRegions.SelectedItem == null) {
				MessageBox.Show(this, "Не выбран регион", "",
					MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			string selectedFolder = TextBoxFolderWithPrices.Text;
			if (string.IsNullOrEmpty(selectedFolder)) {
				MessageBox.Show(this, "Не выбрана папка с прайс-листами", "",
					MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			string[] files = Directory.GetFiles(selectedFolder, "*.xls*");
			if (files.Length == 0) {
				MessageBox.Show(this, "В папке " + selectedFolder + " отсутсвуют файлы формата *.xls*", "",
					MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			foreach (SiteInfo siteInfo in PivotTableItems) {
				if (!string.IsNullOrEmpty(siteInfo.SelectedPriceListFile))
					continue;

				string fileStartsWith = NpoiExcel.GetFileName(siteInfo.UrlServicesPage);

				foreach (string file in files) {
					if (!file.Contains(fileStartsWith))
						continue;

					if (siteInfo.Name == SiteInfo.SiteName.msk_familydoctor_ru)
						if (file.Contains("child"))
							continue;

					siteInfo.SelectedPriceListFile = file;
					break;
				}
			}
		}
	}
}
