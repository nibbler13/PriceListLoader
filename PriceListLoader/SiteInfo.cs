using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PriceListLoader {
	public class SiteInfo : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public string UrlRoot { get; set; } = string.Empty;
		public string UrlServicesPage { get; set; } = string.Empty;
		public string CompanyName { get; set; } = string.Empty;
		public string XPathServices { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string SummaryColumnName { get; set; } = string.Empty;

		private string selectedPriceListFile = string.Empty;
		public string SelectedPriceListFile {
			get {
				return selectedPriceListFile;
			}
			set {
				if (value != selectedPriceListFile) {
					selectedPriceListFile = value;
					NotifyPropertyChanged();
				}
			}
		}

		public List<ItemServiceGroup> ServiceGroupItems { get; set; }
		private SortedDictionary<string, string> DictionaryAllServices { get; set; }
		public SiteName Name { get; set; }

		private Regex regexClearString = new Regex("[^а-яА-Яa-zA-Z0-9 -]");

		public enum SiteName {
			msk_fdoctor_ru,
			msk_familydoctor_ru,
			msk_familydoctor_ru_child,
			msk_alfazdrav_ru,
			msk_nrmed_ru,
			msk_nrmed_ru_child,
			msk_nrmedlab_ru,
			msk_onclinic_ru,
			msk_smclinic_ru,
			msk_smdoctor_ru,
			msk_invitro_ru,
			msk_cmd_online_ru,
			msk_helix_ru,
			msk_mrt24_ru,
			msk_dentol_ru,
			msk_zub_ru,
			msk_vse_svoi_ru,
			msk_novostom_ru,
			msk_masterdent_ru,
			msk_gemotest_ru,
			msk_kdl_ru,
			msk_medsi_ru,
			msk_medsiKDCB_ru,
			msk_medsiKPP_ru,
			msk_medsiPIROGOVKA_ru,
			msk_sm_stomatology_ru,
			msk_smclinic_ru_lab,
			spb_mc21_ru,
			spb_evro_med_ru,
			spb_baltzdrav_ru,
			spb_german_clinic,
			spb_german_dental,
			spb_clinic_complex_ru,
			spb_medswiss_spb_ru,
			spb_invitro_ru,
			spb_helix_ru,
			spb_emcclinic_ru,
			ufa_megi_clinic,
			ufa_promedicina_clinic,
			ufa_mamadeti_ru,
			ufa_rkbkuv_ru,
			ufa_mdplus_ru,
			yekuk_mfcrubin_ru,
			yekuk_ruslabs_ru,
			yekuk_mc_vd_ru,
			yekuk_immunoresurs_ru,
			yekuk_medkamensk_ru,
			kazan_ava_kazan_ru,
			kazan_mc_aybolit_ru,
			kazan_biomed_mc_ru,
			kazan_zdorovie7i_ru,
			kazan_starclinic_ru,
			kazan_love_dr_ru,
			kazan_medexpert_kazan_ru,
			krd_clinic23_ru,
			krd_clinicist_ru,
			krd_poly_clinic_ru,
			krd_clinica_nazdorovie_ru,
			krd_clinica_nazdorovie_ru_lab,
			krd_kuban_kbl_ru,
			sochi_armed_mc_ru,
			sochi_uzlovaya_poliklinika_ru,
			sochi_23doc_ru,
			sochi_medcentr_sochi_ru
		}

		public int GetFilId() {
			switch (City) {
				case "Москва":
					return 5;
				case "Санкт-Петербург":
					return 3;
				case "Уфа":
					return 9;
				case "Каменск-Уральский":
					return 15;
				case "Казань":
					return 10;
				case "Краснодар":
					return 8;
				case "Сочи":
					return 17;
				default:
					return 12;
			}
		}


		private string GetClearedString(string stringInitial) {
			return regexClearString.Replace(stringInitial.ToLower().Replace(" ", ""), "");
		}

		public object GetServicePrice(string serviceName) {
			serviceName = GetClearedString(serviceName);

			if (DictionaryAllServices == null) {
				DictionaryAllServices = new SortedDictionary<string, string>();
				foreach (ItemServiceGroup group in ServiceGroupItems)
					foreach (ItemService service in group.ServiceItems) {
						string key = GetClearedString(service.Name);
						if (DictionaryAllServices.ContainsKey(key))
							continue;

						DictionaryAllServices.Add(key, service.Price);
					}
			}
			
			if (DictionaryAllServices.ContainsKey(serviceName)) {
				string price = DictionaryAllServices[serviceName];

				if (double.TryParse(price, out double priceValue)) {
					switch (Name) {
						case SiteName.msk_fdoctor_ru:
							if (serviceName.Equals(GetClearedString(
								"профилактическая чистка зубов с помощью аппарата \" air-flow\" (за один зуб)")))
								return priceValue + "*28";
							break;
						case SiteName.msk_familydoctor_ru:
							if (serviceName.Equals(GetClearedString(
								"рентгенография органов грудной клетки (боковая проекция)")))
								return priceValue + "*2";
							break;
						case SiteName.msk_familydoctor_ru_child:
							if (serviceName.Equals(GetClearedString(
								"рентгенография органов грудной клетки (боковая проекция) (дети)")))
								return priceValue + "*2";
							break;
						case SiteName.msk_onclinic_ru:
							if (serviceName.Equals(GetClearedString(
								"снятие зубных отложений airflow (1 зуб)")))
								return priceValue + "*28";
							break;
						case SiteName.msk_sm_stomatology_ru:
							if (serviceName.Equals(
								GetClearedString("удаление зубного налета аэр флоу (1 челюсть)")))
								return priceValue + "*2";
							if (serviceName.Equals(GetClearedString(
								"снятие твердых зубных отложений ультразвуком (1 зуб)")))
								return priceValue + "*28";
							break;
						default:
							break;
					}

					return priceValue;
				} else
					return price;
			}
			
			return "NOT_FOUND";
		}


		public SiteInfo(SiteName name) {
			Name = name;
			ServiceGroupItems = new List<ItemServiceGroup>();

			switch (name) {
				case SiteName.msk_fdoctor_ru:
					UrlRoot = "https://www.fdoctor.ru";
					UrlServicesPage = UrlRoot + "/services";
					CompanyName = "ЗАО Сеть поликлиник \"Семейный доктор\"";
					XPathServices = "/html[1]/body[1]/div[2]/div[2]/section[1]/div[1]/div[3]/div[1]/div[2]//a[@href]";
					City = "Москва";
					SummaryColumnName = "Семейный доктор fdoctor.ru";
					break;
				case SiteName.msk_familydoctor_ru:
					UrlRoot = "http://www.familydoctor.ru";
					UrlServicesPage = UrlRoot + "/prices";
					CompanyName = "ООО \"Медицинская клиника \"Семейный доктор\"";
					XPathServices = "//*[@id=\"content\"]/article/div[4]//a[@href]";
					City = "Москва";
					SummaryColumnName = "МК Семейный доктор";
					break;
				case SiteName.msk_familydoctor_ru_child:
					UrlRoot = "http://www.familydoctor.ru";
					UrlServicesPage = UrlRoot + "/prices/child/";
					CompanyName = "ООО \"Медицинская клиника \"Семейный доктор\"";
					XPathServices = "//*[@id=\"content\"]/article/div[4]//a[@href]";
					City = "Москва";
					SummaryColumnName = "МК Семейный доктор (дети)";
					break;
				case SiteName.msk_alfazdrav_ru:
					UrlRoot = "https://www.alfazdrav.ru";
					UrlServicesPage = UrlRoot + "/services/price/";
					CompanyName = "ООО «МедАС» — «Альфа-Центр Здоровья»";
					XPathServices = "/html/body/section/section/div/div/ul[1]/li";
					City = "Москва";
					SummaryColumnName = "Альфа Центр Здоровья";
					break;
				case SiteName.msk_nrmed_ru:
					UrlRoot = "http://www.nrmed.ru";
					UrlServicesPage = UrlRoot + "/rus/dlya-vzroslykh/";
					CompanyName = "ООО \"НИАРМЕДИК ПЛЮС\"";
					XPathServices = "//div[@class='child__services']//a[@href]";
					City = "Москва";
					SummaryColumnName = "НИАРМЕДИК";
					break;
				case SiteName.msk_nrmed_ru_child:
					UrlRoot = "http://www.nrmed.ru";
					UrlServicesPage = UrlRoot + "/rus/dlya-detey/";
					CompanyName = "ООО \"НИАРМЕДИК ПЛЮС\"";
					XPathServices = "//div[@class='child__services']//a[@href]";
					City = "Москва";
					SummaryColumnName = "НИАРМЕДИК дети";
					break;
				case SiteName.msk_nrmedlab_ru:
					UrlRoot = "http://www.nrmed.ru";
					UrlServicesPage = UrlRoot + "/rus/laboratoriya/alphabet";
					CompanyName = "Лаборатория Ниармедик";
					XPathServices = "//div[@class='quattro-block quattro-block_seven']//a[@href]";
					City = "Москва";
					SummaryColumnName = "НИАРМЕДИК лаборатория";
					break;
				case SiteName.msk_onclinic_ru:
					UrlRoot = "https://www.onclinic.ru";
					UrlServicesPage = UrlRoot + "/all/";
					CompanyName = "ООО \"Он Клиник Геоконик\"";
					XPathServices = "//*[@id=\"center\"]/div//a[@href]";
					City = "Москва";
					SummaryColumnName = "ОН-Клиник";
					break;
				case SiteName.msk_smclinic_ru:
					UrlRoot = "http://www.smclinic.ru";
					UrlServicesPage = UrlRoot + "/doctors/";
					CompanyName = "ООО «СМ-Клиника»";
					XPathServices = "//div[@id=\"content\"]//a[@href]";
					City = "Москва";
					SummaryColumnName = "СМ-Клиник";
					break;
				case SiteName.msk_smdoctor_ru:
					UrlRoot = "http://www.smdoctor.ru";
					UrlServicesPage = UrlRoot + "/about/price/";
					CompanyName = "ООО «СМ-Доктор»";
					XPathServices = "//div[contains(@class, 'tab-price-panel')]";
					City = "Москва";
					SummaryColumnName = "СМ-Клиник дети";
					break;
				case SiteName.msk_invitro_ru:
					UrlRoot = "https://www.invitro.ru";
					UrlServicesPage = UrlRoot + "/analizes/for-doctors/";
					CompanyName = "ООО «ИНВИТРО»";
					//XPathServices = "/html/body/div[4]/div[2]/div[4]/div/div[1]/table/tbody";.
					XPathServices = "//table[@class='table_price_c']//tbody";
					City = "Москва";
					SummaryColumnName = "Инвитро";
					break;
				case SiteName.msk_cmd_online_ru:
					UrlRoot = "https://www.cmd-online.ru";
					UrlServicesPage = UrlRoot + "/analizy-i-tseny-po-gruppam/kompleksnyje-programmy-laboratornyh-issledovanij_323/";
					CompanyName = "ФБУН ЦНИИ Эпидемиологии Роспотребнадзора";
					XPathServices = "//*[@id=\"analyzes_and_rates\"]/div[1]//a[@href]";
					City = "Москва";
					SummaryColumnName = "ЦМД";
					break;
				case SiteName.msk_helix_ru:
					UrlRoot = "https://helix.ru";
					UrlServicesPage = UrlRoot + "/catalog";
					CompanyName = "ООО «НПФ «ХЕЛИКС»";
					//XPathServices = "/html/body/div[1]/div[6]/div[2]/div[1]//a[@href]";
					XPathServices = "//div[@class='Catalog-Content-Navigation']//span[starts-with(@class,'Catalog-Content-Navigation')]";
					City = "Москва";
					SummaryColumnName = "ХЕЛИКС";
					break;
				case SiteName.msk_mrt24_ru:
					UrlRoot = "http://mrt24.ru";
					UrlServicesPage = UrlRoot + "/services/";
					//UrlServicesPage = @"C:\Users\nn-admin\Desktop\Цены на услуги МРТ в Москве и Московской области в центрах МРТ24.html";
					CompanyName = "ООО \"ДЛ Медика\"";
					XPathServices = "//div[contains(@class,'_id_')]";
					City = "Москва";
					SummaryColumnName = "Сеть МРТ";
					break;
				case SiteName.msk_dentol_ru:
					UrlRoot = "https://dentol.ru";
					UrlServicesPage = UrlRoot + "/uslugi/";
					CompanyName = "ООО “Сеть Семейных Медицинских Центров”";
					XPathServices = "/html/body/div[3]/div[2]/div[1]/div[7]//a[@href]";
					City = "Москва";
					SummaryColumnName = "Сеть МЦ";
					break;
				case SiteName.msk_zub_ru:
					UrlRoot = "https://zub.ru";
					UrlServicesPage = UrlRoot + "/uslugi/";
					CompanyName = "ООО \"Зуб.ру\"";
					XPathServices = "//div[@class='services-grid pads']//a[@href]";
					City = "Москва";
					SummaryColumnName = "Зуб.ру";
					break;
				case SiteName.msk_vse_svoi_ru:
					UrlRoot = "https://vse-svoi.ru";
					UrlServicesPage = UrlRoot + "/msk/ceny/";
					CompanyName = "ООО \"ВСЕ СВОИ\"";
					XPathServices = "//div[@class='price-list-page']";
					City = "Москва";
					SummaryColumnName = "Все свои";
					break;
				case SiteName.msk_novostom_ru:
					UrlRoot = "http://www.novostom.ru";
					UrlServicesPage = UrlRoot + "/tceny/";
					CompanyName = "ООО СЦНТ \"НОВОСТОМ\"";
					XPathServices = "//table[@class='price-tbl']";
					City = "Москва";
					SummaryColumnName = "НОВОСТОМ";
					break;
				case SiteName.msk_masterdent_ru:
					UrlRoot = "http://masterdent.ru";
					UrlServicesPage = UrlRoot + "/prais.html";
					CompanyName = "Мастердент";
					XPathServices = "//div[@class='podrazdel']";
					City = "Москва";
					SummaryColumnName = "Мастердент";
					break;
				case SiteName.msk_gemotest_ru:
					UrlRoot = "https://www.gemotest.ru";
					UrlServicesPage = UrlRoot + "/catalog/po-laboratornym-napravleniyam/top-250-populyarnykh-uslug/";
					CompanyName = "ООО \"Лаборатория Гемотест\"";
					XPathServices = "//*[@id=\"d-content\"]/div/aside/nav/div[2]/ul//a[@href]";
					City = "Москва";
					SummaryColumnName = "Гемотест";
					break;
				case SiteName.msk_kdl_ru:
					UrlRoot = "https://kdl.ru";
					UrlServicesPage = UrlRoot + "/analizy-i-tseny";
					CompanyName = "ООО «КДЛ ДОМОДЕДОВО-ТЕСТ»";
					XPathServices = "//div[starts-with(@class,'a-catalog')]//a[@href]";
					City = "Москва";
					SummaryColumnName = "КДЛ Домодедово";
					break;
				case SiteName.msk_medsi_ru:
					UrlRoot = "https://medsi.ru";
					UrlServicesPage = UrlRoot + "/services/";
					CompanyName = "АО \"Группа компаний МЕДСИ\"";
					XPathServices = "//div[@class='b-services__row']//a[@href]";
					City = "Москва";
					break;
				case SiteName.msk_medsiKDCB_ru:
					UrlRoot = "https://medsi.ru";
					UrlServicesPage = UrlRoot + "/services/";
					CompanyName = "АО \"Группа компаний МЕДСИ\"";
					XPathServices = "//div[@class='b-services__row']//a[@href]";
					City = "Москва";
					SummaryColumnName = "Медси КДЦБ";
					break;
				case SiteName.msk_medsiKPP_ru:
					UrlRoot = "https://medsi.ru";
					UrlServicesPage = UrlRoot + "/services/";
					CompanyName = "АО \"Группа компаний МЕДСИ\"";
					XPathServices = "//div[@class='b-services__row']//a[@href]";
					City = "Москва";
					SummaryColumnName = "Медси КЛП";
					break;
				case SiteName.msk_medsiPIROGOVKA_ru:
					UrlRoot = "https://medsi.ru";
					UrlServicesPage = UrlRoot + "/services/";
					CompanyName = "АО \"Группа компаний МЕДСИ\"";
					XPathServices = "//div[@class='b-services__row']//a[@href]";
					City = "Москва";
					SummaryColumnName = "Медси на Пироговке";
					break;
				case SiteName.msk_sm_stomatology_ru:
					UrlRoot = "http://www.sm-stomatology.ru";
					UrlServicesPage = UrlRoot + "/services/";
					CompanyName = "СМ-Стоматология";
					XPathServices = "//div[@class='b-aside-menu']//a[@href]";
					City = "Москва";
					SummaryColumnName = "СМ-Клиник стоматология";
					break;
				case SiteName.msk_smclinic_ru_lab:
					UrlRoot = "http://www.smclinic.ru";
					UrlServicesPage = UrlRoot + "/calc/";
					CompanyName = "ООО «СМ-Клиника»";
					XPathServices = "//div[@class='panel panel-default pull-left']";
					City = "Москва";
					SummaryColumnName = "СМ-Клиник анализы";
					break;
				case SiteName.spb_mc21_ru:
					UrlRoot = "https://www.mc21.ru";
					UrlServicesPage = UrlRoot + "/price/";
					CompanyName = "Группа компаний Медицинский центр «XXI век»";
					XPathServices = "//div[@class='mc_short_price']";
					City = "Санкт-Петербург";
					SummaryColumnName = "21 ВЕК";
					break;
				case SiteName.spb_evro_med_ru:
					UrlRoot = "http://evro-med.ru";
					UrlServicesPage = UrlRoot + "/ceny/";
					CompanyName = "Многопрофильная клиника \"Европейский медицинский центр\"";
					XPathServices = "//div[@class='b-editor']//a[@href]";
					City = "Санкт-Петербург";
					break;
				case SiteName.spb_baltzdrav_ru:
					UrlRoot = "http://baltzdrav.ru";
					UrlServicesPage = UrlRoot + "/services";
					CompanyName = "Сеть многопрофильных клиник “БалтЗдрав”";
					XPathServices = "//div[@class='uk-panel uk-panel-box box']//a[@href]";
					City = "Санкт-Петербург";
					SummaryColumnName = "Балтздрав";
					break;
				case SiteName.spb_german_clinic:
					UrlRoot = "https://german.clinic";
					UrlServicesPage = UrlRoot;
					CompanyName = "Немецкая семейная клиника";
					XPathServices = "//ul[@class='topmenu__container']/li[2]/ul[1]//a[@href]";
					City = "Санкт-Петербург";
					SummaryColumnName = "Немецкая семейная клиника поликлиника";
					break;
				case SiteName.spb_german_dental:
					UrlRoot = "https://german.dental";
					UrlServicesPage = UrlRoot;
					CompanyName = "Немецкая семейная стоматология";
					XPathServices = "//div[@class='service']//a[@href]";
					City = "Санкт-Петербург";
					SummaryColumnName = "Немецкая семейная клиника стоматология";
					break;
				case SiteName.spb_clinic_complex_ru:
					UrlRoot = "http://clinic-complex.ru";
					UrlServicesPage = UrlRoot + "/tseny-i-atsii/tseny/";
					CompanyName = "Современные медицинские технологии";
					XPathServices = "//div[@class='mainblock-price-spollers-item']";
					City = "Санкт-Петербург";
					SummaryColumnName = "СМТ";
					break;
				case SiteName.spb_medswiss_spb_ru:
					UrlRoot = "http://medswiss-spb.ru";
					UrlServicesPage = UrlRoot + "/tseny/";
					CompanyName = "MedSwiss Санкт-Петер6ург";
					XPathServices = "//table[@id='viseble']//a[@href]";
					City = "Санкт-Петербург";
					SummaryColumnName = "Medswiss";
					break;
				case SiteName.spb_invitro_ru:
					UrlRoot = "https://www.invitro.ru";
					UrlServicesPage = UrlRoot + "/analizes/for-doctors/piter/";
					CompanyName = "ООО «ИНВИТРО»";
					XPathServices = "//table[@class='table_price_c']//tbody";
					City = "Санкт-Петербург";
					SummaryColumnName = "ИНВИТРО";
					break;
				case SiteName.spb_helix_ru:
					UrlRoot = "https://helix.ru";
					UrlServicesPage = UrlRoot + "/catalog";
					CompanyName = "ООО «НПФ «ХЕЛИКС»";
					XPathServices = "//div[@class='Catalog-Content-Navigation']//span[starts-with(@class,'Catalog-Content-Navigation')]";
					City = "Санкт-Петербург";
					SummaryColumnName = "ХЕЛИКС";
					break;
				case SiteName.spb_emcclinic_ru:
					UrlRoot = "http://www.emcclinic.ru";
					UrlServicesPage = UrlRoot + "/services";
					CompanyName = "ООО \"Единые Медицинские Системы\"";
					XPathServices = "//div[@class='n-services__item']//a[@href]";
					City = "Санкт-Петербург";
					SummaryColumnName = "ЕМС";
					break;
				case SiteName.ufa_megi_clinic:
					UrlRoot = "http://megi.clinic";
					UrlServicesPage = UrlRoot + "/cost/";
					CompanyName = "Сеть клиник «МЕГИ»";
					XPathServices = "//div[@class='detail_cost']//div[@class='test']";
					City = "Уфа";
					break;
				case SiteName.ufa_promedicina_clinic:
					UrlRoot = "https://www.promedicina.clinic";
					UrlServicesPage = UrlRoot + "/adult/services/";
					CompanyName = "ООО ММЦ «Профилактическая медицина»";
					XPathServices = "//div[starts-with(@class,'col-md-4')]//a[@href]";
					City = "Уфа";
					break;
				case SiteName.ufa_mamadeti_ru:
					UrlRoot = "http://ufa.mamadeti.ru";
					UrlServicesPage = UrlRoot + "/price-list2/the-clinic-mother-and-child-ufa/price/";
					CompanyName = "Группа компаний «Мать и дитя»";
					XPathServices = "//div[@class='b-tree_link__item']";
					City = "Уфа";
					break;
				case SiteName.ufa_rkbkuv_ru:
					UrlRoot = "https://rkbkuv.ru";
					UrlServicesPage = UrlRoot + "/list_paid_services";
					CompanyName = "ГБУЗ РКБ им. Г.Г. Куватова";
					XPathServices = "";
					City = "Уфа";
					break;
				case SiteName.ufa_mdplus_ru:
					UrlRoot = "http://www.ufamdplus.ru";
					UrlServicesPage = UrlRoot + "/services/prays-list/";
					CompanyName = "Клиника «МД плюс»";
					XPathServices = "//div[@class='col-lg-12']";
					City = "Уфа";
					break;
				case SiteName.yekuk_mfcrubin_ru:
					UrlRoot = "http://www.mfcrubin.ru";
					UrlServicesPage = UrlRoot + "/stoimostuslug";
					CompanyName = "ООО МФЦ РУБИН";
					XPathServices = "//div[@class='item column-1']//tr";
					City = "Каменск-Уральский";
					break;
				case SiteName.yekuk_ruslabs_ru:
					UrlRoot = "http://www.ruslabs.ru";
					UrlServicesPage = UrlRoot + "/uslugi/obsheklinicheskiye-issledovaniya/";
					CompanyName = "Лаборатория «Руслаб»";
					XPathServices = "//div[@class='price-sidebar']//a[@href]";
					City = "Каменск-Уральский";
					break;
				case SiteName.yekuk_mc_vd_ru:
					UrlRoot = "http://mc-vd.ru";
					UrlServicesPage = UrlRoot + "/uslugi/";
					CompanyName = "ООО \"Медсервис Каменск\"";
					XPathServices = "//div[@id='nextend-accordion-menu-nextendaccordionmenuwidget-2']//a[@href]";
					City = "Каменск-Уральский";
					break;
				case SiteName.yekuk_immunoresurs_ru:
					UrlRoot = "http://immunoresurs.ru";
					UrlServicesPage = UrlRoot + "/uslugi-i-ceny/";
					CompanyName = "ООО Медицинский центр \"Иммуноресурс\"";
					XPathServices = "//div[@id='global3']//a[@href]";
					City = "Каменск-Уральский";
					break;
				case SiteName.yekuk_medkamensk_ru:
					UrlRoot = "http://medkamensk.ru";
					UrlServicesPage = UrlRoot + "/prejskurant-tsen/";
					CompanyName = "ГБУЗ Свердловской обл. «Городская больница г.Каменск - Уральский»";
					XPathServices = "//table[@class='waffle']//tr";
					City = "Каменск-Уральский";
					break;
				case SiteName.kazan_ava_kazan_ru:
					UrlRoot = "https://ava-kazan.ru";
					UrlServicesPage = UrlRoot + "/price/";
					CompanyName = "АО \"АВА - Казань\"";
					XPathServices = "//a[@class='off_menu__link']";
					City = "Казань";
					break;
				case SiteName.kazan_mc_aybolit_ru:
					UrlRoot = "http://mc-aybolit.ru";
					UrlServicesPage = UrlRoot + "/nashi_uslugi";
					CompanyName = "ООО \"МЦ Айболит\"";
					XPathServices = "//div[@id='accordion']//a[@href]";
					City = "Казань";
					break;
				case SiteName.kazan_biomed_mc_ru:
					UrlRoot = "https://biomed-mc.ru";
					UrlServicesPage = UrlRoot + "/static5/uslugi";
					CompanyName = "ООО лечебно-диагностический центр \"БИОМЕД\"";
					XPathServices = "//div[starts-with(@class,'megamenu')]//a[@href]";
					City = "Казань";
					break;
				case SiteName.kazan_zdorovie7i_ru:
					UrlRoot = "http://zdorovie7i.ru";
					UrlServicesPage = UrlRoot + "/uslugi-i-tseny";
					CompanyName = "Сеть лечебно-диагностических центров «Здоровье семьи»";
					XPathServices = "//div[@class='mainpage']//a[@href]";
					City = "Казань";
					break;
				case SiteName.kazan_starclinic_ru:
					UrlRoot = "http://www.starclinic.ru";
					UrlServicesPage = UrlRoot + "/prays/";
					CompanyName = "Медцентр «ЗВЕЗДА»";
					XPathServices = "//div[@class='news-list']//a[@href]";
					City = "Казань";
					break;
				case SiteName.kazan_love_dr_ru:
					UrlRoot = "http://www.love-dr.ru";
					UrlServicesPage = UrlRoot + "/tseny/";
					CompanyName = "ООО \"Любимый доктор\"";
					XPathServices = "//td[@class='main-column']//ul[not(@*)]//a[@href]";
					City = "Казань";
					break;
				case SiteName.kazan_medexpert_kazan_ru:
					UrlRoot = "http://medexpert-kazan.ru";
					UrlServicesPage = UrlRoot + "/uslugi/";
					CompanyName = "Сеть медицинских клиник «Медэксперт»";
					XPathServices = "//ul[starts-with(@class,'service__list row')]//a[@href]";
					City = "Казань";
					break;
				case SiteName.krd_clinic23_ru:
					UrlRoot = "https://www.clinic23.ru";
					UrlServicesPage = UrlRoot + "";
					CompanyName = "КЛИНИКА «ЕКАТЕРИНИНСКАЯ»";
					XPathServices = "//table[@class='categoriya']//a[@href]";
					City = "Краснодар";
					break;
				case SiteName.krd_clinicist_ru:
					UrlRoot = "https://www.clinicist.ru";
					UrlServicesPage = UrlRoot + "/tseny/";
					CompanyName = "Сеть медицинских центров «Клиницист»";
					XPathServices = "//table[@class='visual-table']//tbody//tr";
					City = "Краснодар";
					break;
				case SiteName.krd_poly_clinic_ru:
					UrlRoot = "https://poly-clinic.ru";
					UrlServicesPage = UrlRoot + "/price/";
					CompanyName = "Клиника семейного здоровья Сити-Клиник";
					XPathServices = "//div[@class='news-list']//a[@href]";
					City = "Краснодар";
					break;
				case SiteName.krd_clinica_nazdorovie_ru:
					UrlRoot = "http://clinica-nazdorovie.ru";
					UrlServicesPage = UrlRoot + "";
					CompanyName = "ООО \"МФО \"Клиника На здоровье\"";
					XPathServices = "//div[@class=' top-menu-main']/ul[1]/li[4]//a[@href]";
					City = "Краснодар";
					break;
				case SiteName.krd_clinica_nazdorovie_ru_lab:
					UrlRoot = "http://clinica-nazdorovie.ru";
					UrlServicesPage = UrlRoot + "/m/products/";
					CompanyName = "ООО \"МФО \"Клиника На здоровье\"";
					XPathServices = "//div[@class='side-menu']//a[@href]";
					City = "Краснодар";
					break;
				case SiteName.krd_kuban_kbl_ru:
					UrlRoot = "https://www.kuban-kbl.ru";
					UrlServicesPage = UrlRoot + "/ceni.php";
					CompanyName = "ОАО «ЦВМР «Краснодарская бальнеолечебница»";
					XPathServices = "//div[@class='panel panel-default']";
					City = "Краснодар";
					break;
				case SiteName.sochi_armed_mc_ru:
					UrlRoot = "http://armed-mc.ru";
					UrlServicesPage = UrlRoot + "/tseny/";
					CompanyName = "ООО \"АРМЕД\"";
					XPathServices = "//div[@class='mk-accordion-single']";
					City = "Сочи";
					break;
				case SiteName.sochi_uzlovaya_poliklinika_ru:
					UrlRoot = "http://uzlovaya-poliklinika.ru";
					UrlServicesPage = UrlRoot + "/price/";
					CompanyName = "Узловая поликлиника на станции Сочи";
					XPathServices = "//div[@class='pricelist-group btop pd-m']";
					City = "Сочи";
					break;
				case SiteName.sochi_23doc_ru:
					UrlRoot = "http://23doc.ru";
					UrlServicesPage = UrlRoot + "/prays-list";
					CompanyName = "Детский диагностический центр \"Семья\"";
					XPathServices = "//ul[@class='accordion_square accordion-rounded2']//li";
					City = "Сочи";
					break;
				case SiteName.sochi_medcentr_sochi_ru:
					UrlRoot = "https://medcentr-sochi.ru";
					UrlServicesPage = UrlRoot + "/uslugi-i-ceny-medicinskogo-centra-proksima.html";
					CompanyName = "Медицинский клинический центр «Проксима»";
					XPathServices = "//div[@id='page']";
					City = "Сочи";
					break;
				default:
					return;
			}
		}

		public static string ClearString(string initial) {
			Dictionary<string, string> toReplace = new Dictionary<string, string>() {
				{ "\r\n", "" },
				{ "\r", "" },
				{ "\n", "" },
				{ "&nbsp;", " " },
				{ "&quot;", "\"" },
				{ "\t", "" },
				{ "&mdash;", "-" },
				{ "&laquo;", "«" },
				{ "&raquo;", "»" },
				{ "&ndash;", "" },
				{ "&lt;", "<" },
				{ "&gt;", ">" },
				{ "&#8212;", "-" },
				{ "&#171;", "«" },
				{ "&#187;", "»" },
				{ "&#160;", "" }
			};

			foreach (KeyValuePair<string, string> pair in toReplace)
				initial = initial.Replace(pair.Key, pair.Value);

			initial = initial.TrimStart(' ').TrimEnd(' ');

			return initial;
		}
	}

	public class ItemServiceGroup {
		public string Name { get; set; }
		public string Link { get; set; }
		public List<ItemService> ServiceItems { get; set; }

		public ItemServiceGroup() {
			ServiceItems = new List<ItemService>();
		}
	}

	public class ItemService {
		private static Dictionary<string, string> toReplace = new Dictionary<string, string>() {
			{ "р.", "" },
			{ " руб.", "" },
			{ "руб.", "" },
			{ " руб", "" },
			{ ",00", "" },
			{ " ₽", "" },
			{ ".00", "" },
			{ " ф", "" },
			{ " i", "" },
			{ " р", "" },
			{ " &#1088;&#1091;&#1073;.", "" },
			{ "Казань:", "" }
		};

		public string Name { get; set; }

		private string price;
		public string Price {
			get {
				return price;
			}
			set {
				string newValue = value;
				foreach (KeyValuePair<string, string> item in toReplace)
					if (newValue.Contains(item.Key))
						newValue = newValue.Replace(item.Key, item.Value);
				
				price = newValue.Replace(",", "");
			}
		}
	}
}
