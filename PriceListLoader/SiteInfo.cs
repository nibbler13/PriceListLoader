using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        public static readonly Dictionary<Enums.Cities, Type> CitySitesMap;

        static SiteInfo() {
            CitySitesMap = new Dictionary<Enums.Cities, Type>();
            foreach (Enums.Cities city in Enum.GetValues(typeof(Enums.Cities))) {
                string typeName = city.ToString() + "Sites";
                Type type = Type.GetType("PriceListLoader.Enums+" + typeName + ", PriceListLoader");
                CitySitesMap.Add(city, type);
            }
        }



		public string UrlRoot { get; set; } = string.Empty;
		public string UrlServicesPage { get; set; } = string.Empty;
		public string CompanyName { get; set; } = string.Empty;
		public string XPathServices { get; set; } = string.Empty;
		public string CityName { get; set; } = string.Empty;
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
					NotifyPropertyChanged("SelectedPriceListFileName");
				}
			}
		}

		public string SelectedPriceListFileName {
			get {
				return Path.GetFileName(SelectedPriceListFile);
			}
		}

		public List<Items.ServiceGroup> ServiceGroupItems { get; set; }
		private SortedDictionary<string, string> DictionaryAllServices { get; set; }
		public Enums.Cities CityValue { get; private set; }

        public bool IsLocalFile { get; private set; } = false;

        public int SiteValue { get; private set; }

		private readonly Regex regexClearString = new Regex("[^а-яА-Яa-zA-Z0-9 -]");

        public bool ShouldAutoLoad { get; private set; } = false;


		public int GetFilId() {
            switch (CityValue) {
                case Enums.Cities.Moscow:
                    return 12;
                case Enums.Cities.SaintPetersburg:
                    return 3;
                case Enums.Cities.Sochi:
                    return 17;
                case Enums.Cities.Kazan:
                    return 10;
                case Enums.Cities.KamenskUralsky:
                    return 15;
                case Enums.Cities.Krasnodar:
                    return 8;
                case Enums.Cities.Ufa:
                    return 9;
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
				foreach (Items.ServiceGroup group in ServiceGroupItems)
					foreach (Items.Service service in group.ServiceItems) {
						string key = GetClearedString(service.Name);
						if (DictionaryAllServices.ContainsKey(key))
							continue;

						DictionaryAllServices.Add(key, service.Price);
					}
			}
			
			if (DictionaryAllServices.ContainsKey(serviceName)) {
				string price = DictionaryAllServices[serviceName];

				if (double.TryParse(price, out double priceValue)) {
                    switch (CityValue) {
                        case Enums.Cities.Moscow:
                            switch ((Enums.MoscowSites)SiteValue) {
                                case Enums.MoscowSites.fdoctor_ru:
                                    if (serviceName.Equals(GetClearedString(
                                        "профилактическая чистка зубов с помощью аппарата \" air-flow\" (за один зуб)")))
                                        return priceValue + "*28";
                                    break;
                                case Enums.MoscowSites.familydoctor_ru:
                                    if (serviceName.Equals(GetClearedString(
                                        "рентгенография органов грудной клетки (боковая проекция)")))
                                        return priceValue + "*2";
                                    break;
                                case Enums.MoscowSites.familydoctor_ru_child:
                                    if (serviceName.Equals(GetClearedString(
                                        "рентгенография органов грудной клетки (боковая проекция) (дети)")))
                                        return priceValue + "*2";
                                    break;
                                case Enums.MoscowSites.onclinic_ru:
                                    if (serviceName.Equals(GetClearedString(
                                        "снятие зубных отложений airflow (1 зуб)")))
                                        return priceValue + "*28";
                                    break;
                                case Enums.MoscowSites.sm_stomatology_ru:
                                    if (serviceName.Equals(GetClearedString(
                                        "удаление зубного налета аэр флоу (1 челюсть)")))
                                        return priceValue + "*2";
                                    if (serviceName.Equals(GetClearedString(
                                        "снятие твердых зубных отложений ультразвуком (1 зуб)")))
                                        return priceValue + "*28";
                                    break;
                                case Enums.MoscowSites.dentol_ru:
                                    if (serviceName.Equals(GetClearedString(
                                        "Метод \"AIR - FLOW\" (одна челюсть)")))
                                        return priceValue + "*2";
                                    break;
                                case Enums.MoscowSites.novostom_ru:
                                    if (serviceName.Equals(GetClearedString(
                                        "Снятие пигментированного налета в области 1 зуба (Air-Flow)")))
                                        return priceValue + "*28";
                                    break;
                                case Enums.MoscowSites.masterdent_ru:
                                    if (serviceName.Equals(GetClearedString(
                                        "Airflow (1 зуб)")))
                                        return priceValue + "*28";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case Enums.Cities.Krasnodar:
                            switch ((Enums.KrasnodarSites)SiteValue) {
                                case Enums.KrasnodarSites.poly_clinic_ru:
                                    if (serviceName.Equals(GetClearedString(
                                        "Снятие зубных отложений методом Air-Flow (1 зуб)")))
                                        return priceValue + "*28";
                                    break;
                                default:
                                    break;
                            }
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




        private void SetupMoscowSites(Enums.MoscowSites moscowSite) {
            CityName = "Москва";

            switch (moscowSite) {
                case Enums.MoscowSites.fdoctor_ru:
                    UrlRoot = "https://www.fdoctor.ru";
                    UrlServicesPage = UrlRoot + "/services";
                    CompanyName = "ЗАО Сеть поликлиник \"Семейный доктор\"";
                    XPathServices = "/html[1]/body[1]/div[2]/div[2]/section[1]/div[1]/div[3]/div[1]/div[2]//a[@href]";
                    SummaryColumnName = "Семейный доктор fdoctor.ru";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.familydoctor_ru:
                    UrlRoot = "http://www.familydoctor.ru";
                    UrlServicesPage = UrlRoot + "/prices";
                    CompanyName = "ООО \"Медицинская клиника \"Семейный доктор\"";
                    XPathServices = "//div[@class='section_list_columns']//a[@href]";
                    SummaryColumnName = "МК Семейный доктор familydoctor.ru";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.familydoctor_ru_child:
                    UrlRoot = "http://www.familydoctor.ru";
                    UrlServicesPage = UrlRoot + "/prices/child/";
                    CompanyName = "ООО \"Медицинская клиника \"Семейный доктор\"";
                    XPathServices = "//div[@class='section_list_columns']//a[@href]";
                    SummaryColumnName = "МК Семейный доктор дети familydoctor.ru";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.alfazdrav_ru:
                    UrlRoot = "https://www.alfazdrav.ru";
                    UrlServicesPage = UrlRoot + "/services/price/";
                    CompanyName = "ООО «МедАС» — «Альфа-Центр Здоровья»";
                    XPathServices = "/html/body/section/section/div/div/ul[1]/li";
                    SummaryColumnName = "Альфа-центр здоровья";
                    IsLocalFile = true;
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.nrmed_ru:
                    UrlRoot = "http://www.nrmed.ru";
                    UrlServicesPage = UrlRoot + "/rus/dlya-vzroslykh/";
                    CompanyName = "ООО \"НИАРМЕДИК ПЛЮС\"";
                    XPathServices = "//div[@class='child__services']//a[@href]";
                    SummaryColumnName = "Ниармедик";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.nrmed_ru_child:
                    UrlRoot = "http://www.nrmed.ru";
                    UrlServicesPage = UrlRoot + "/rus/dlya-detey/";
                    CompanyName = "ООО \"НИАРМЕДИК ПЛЮС\"";
                    XPathServices = "//div[@class='child__services']//a[@href]";
                    SummaryColumnName = "Ниармедик дети";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.nrmedlab_ru:
                    UrlRoot = "http://www.nrmed.ru";
                    UrlServicesPage = UrlRoot + "/rus/laboratoriya/alphabet";
                    CompanyName = "Лаборатория Ниармедик";
                    XPathServices = "//div[@class='quattro-block quattro-block_seven']//a[@href]";
                    SummaryColumnName = "Ниармедик лаборатория";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.onclinic_ru:
                    UrlRoot = "https://www.onclinic.ru";
                    UrlServicesPage = UrlRoot + "/all/";
                    CompanyName = "ООО \"Он Клиник Геоконик\"";
                    XPathServices = "//*[@id=\"center\"]/div//a[@href]";
                    SummaryColumnName = "ОН Клиник";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.smclinic_ru:
                    UrlRoot = "http://www.smclinic.ru";
                    UrlServicesPage = UrlRoot + "/doctors/";
                    CompanyName = "ООО «СМ-Клиника»";
                    XPathServices = "//div[@id=\"content\"]//a[@href]";
                    SummaryColumnName = "СМ-Клиника";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.smdoctor_ru:
                    UrlRoot = "http://www.smdoctor.ru";
                    UrlServicesPage = UrlRoot + "/about/price/";
                    CompanyName = "ООО «СМ-Доктор»";
                    XPathServices = "//div[contains(@class, 'tab-price-panel')]";
                    SummaryColumnName = "СМ-Клиника дети";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.sm_stomatology_ru:
                    UrlRoot = "http://www.sm-stomatology.ru";
                    UrlServicesPage = UrlRoot + "/services/";
                    CompanyName = "СМ-Стоматология";
                    XPathServices = "//div[@class='b-aside-menu']//a[@href]";
                    SummaryColumnName = "СМ-Клиника стоматология";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.smclinic_ru_lab:
                    UrlRoot = "http://www.smclinic.ru";
                    UrlServicesPage = UrlRoot + "/calc/";
                    CompanyName = "ООО «СМ-Клиника»";
                    XPathServices = "//div[@class='panel panel-default pull-left']";
                    SummaryColumnName = "СМ-Клиника анализы";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.invitro_ru:
                    UrlRoot = "https://www.invitro.ru";
                    UrlServicesPage = UrlRoot + "/analizes/for-doctors/";
                    CompanyName = "ООО «ИНВИТРО»";
                    //XPathServices = "/html/body/div[4]/div[2]/div[4]/div/div[1]/table/tbody";
                    XPathServices = "//div[@class='node' and @data-prices]";
                    SummaryColumnName = "";
                    IsLocalFile = true;
                    break;

                case Enums.MoscowSites.cmd_online_ru:
                    UrlRoot = "https://www.cmd-online.ru";
                    UrlServicesPage = UrlRoot + "/analizy-i-tseny-po-gruppam/kompleksnyje-programmy-laboratornyh-issledovanij_323/";
                    CompanyName = "ФБУН ЦНИИ Эпидемиологии Роспотребнадзора";
                    XPathServices = "//*[@id=\"analyzes_and_rates\"]/div[1]//a[@href]";
                    SummaryColumnName = "";
                    break;

                case Enums.MoscowSites.helix_ru:
                    UrlRoot = "https://helix.ru";
                    UrlServicesPage = UrlRoot + "/catalog";
                    CompanyName = "ООО «НПФ «ХЕЛИКС»";
                    //XPathServices = "/html/body/div[1]/div[6]/div[2]/div[1]//a[@href]";
                    XPathServices = "//div[@class='Catalog-Content-Navigation']//span[starts-with(@class,'Catalog-Content-Navigation')]";
                    SummaryColumnName = "";
                    break;

                case Enums.MoscowSites.mrt24_ru:
                    UrlRoot = "http://mrt24.ru";
                    UrlServicesPage = UrlRoot + "/services/";
                    //UrlServicesPage = @"C:\Users\nn-admin\Desktop\Цены на услуги МРТ в Москве и Московской области в центрах МРТ24.html";
                    CompanyName = "ООО \"ДЛ Медика\"";
                    XPathServices = "//div[contains(@class,'_id_')]";
                    SummaryColumnName = "МРТ 24";
                    IsLocalFile = true;
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.dentol_ru:
                    UrlRoot = "https://dentol.ru";
                    UrlServicesPage = UrlRoot + "/uslugi/";
                    CompanyName = "ООО “Сеть Семейных Медицинских Центров”";
                    XPathServices = "/html/body/div[3]/div[2]/div[1]/div[7]//a[@href]";
                    SummaryColumnName = "Сеть МЦ (dentol.ru)";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.zub_ru:
                    UrlRoot = "https://zub.ru";
                    UrlServicesPage = UrlRoot + "/uslugi/";
                    CompanyName = "ООО \"Зуб.ру\"";
                    XPathServices = "//div[@class='services-grid pads']//a[@href]";
                    SummaryColumnName = "Зуб.ру";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.vse_svoi_ru:
                    UrlRoot = "https://vse-svoi.ru";
                    UrlServicesPage = UrlRoot + "/msk/ceny/";
                    CompanyName = "ООО \"ВСЕ СВОИ\"";
                    XPathServices = "//div[@class='price-list-page']";
                    SummaryColumnName = "Все свои";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.novostom_ru:
                    UrlRoot = "http://www.novostom.ru";
                    UrlServicesPage = UrlRoot + "/tceny/";
                    CompanyName = "ООО СЦНТ \"НОВОСТОМ\"";
                    XPathServices = "//table[@class='price-tbl']";
                    SummaryColumnName = "";
                    break;

                case Enums.MoscowSites.masterdent_ru:
                    UrlRoot = "http://masterdent.ru";
                    UrlServicesPage = UrlRoot + "/content/%D0%BF%D1%80%D0%B0%D0%B9%D1%81";
                    CompanyName = "Мастердент";
                    XPathServices = "//div[@class='field-item even']//tbody//tr";
                    SummaryColumnName = "";
                    break;

                case Enums.MoscowSites.gemotest_ru:
                    UrlRoot = "https://www.gemotest.ru";
                    UrlServicesPage = UrlRoot + "/catalog/po-laboratornym-napravleniyam/top-250-populyarnykh-uslug/";
                    CompanyName = "ООО \"Лаборатория Гемотест\"";
                    XPathServices = "//*[@id=\"d-content\"]/div/aside/nav/div[2]/ul//a[@href]";
                    SummaryColumnName = "";
                    break;

                case Enums.MoscowSites.kdl_ru:
                    UrlRoot = "https://kdl.ru";
                    UrlServicesPage = UrlRoot + "/analizy-i-tseny";
                    CompanyName = "ООО «КДЛ ДОМОДЕДОВО-ТЕСТ»";
                    XPathServices = "//div[starts-with(@class,'a-catalog')]//a[@href]";
                    SummaryColumnName = "";
                    break;

                case Enums.MoscowSites.medsi_ru:
                    UrlRoot = "https://medsi.ru";
                    UrlServicesPage = UrlRoot + "/services/";
                    CompanyName = "АО \"Группа компаний МЕДСИ\"";
                    XPathServices = "//div[@class='b-services__row']//a[@href]";
                    ShouldAutoLoad = true;
                    break;

                case Enums.MoscowSites.medsiKDCB_ru:
                    UrlRoot = "https://medsi.ru";
                    UrlServicesPage = UrlRoot + "/services/";
                    CompanyName = "АО \"Группа компаний МЕДСИ\" (на Белорусской)";
                    XPathServices = "";
                    SummaryColumnName = "МЕДСИ (на Белорусской)";
                    break;

                case Enums.MoscowSites.medsiKPP_ru:
                    UrlRoot = "https://medsi.ru";
                    UrlServicesPage = UrlRoot + "/services/";
                    CompanyName = "АО \"Группа компаний МЕДСИ\" (на Ленинградском)";
                    XPathServices = "";
                    SummaryColumnName = "МЕДСИ (на Ленинградском)";
                    break;

                case Enums.MoscowSites.medsiPIROGOVKA_ru:
                    UrlRoot = "https://medsi.ru";
                    UrlServicesPage = UrlRoot + "/services/";
                    CompanyName = "АО \"Группа компаний МЕДСИ\" (на Пироговке)";
                    XPathServices = "";
                    SummaryColumnName = "МЕДСИ (на Пироговке)";
                    break;

                case Enums.MoscowSites.legal_entity_k31:
                    UrlRoot = "";
                    UrlServicesPage = UrlRoot + "";
                    CompanyName = "К31";
                    XPathServices = "";
                    SummaryColumnName = "";
                    break;

                case Enums.MoscowSites.legal_entity_litfond:
                    UrlRoot = "";
                    UrlServicesPage = UrlRoot + "";
                    CompanyName = "Литфонд";
                    XPathServices = "";
                    SummaryColumnName = "";
                    break;

                case Enums.MoscowSites.legal_entity_ssmc:
                    UrlRoot = "";
                    UrlServicesPage = UrlRoot + "";
                    CompanyName = "ССМЦ";
                    XPathServices = "";
                    SummaryColumnName = "";
                    break;

                case Enums.MoscowSites.legal_entity_lechebniy_centr:
                    UrlRoot = "";
                    UrlServicesPage = UrlRoot + "";
                    CompanyName = "Лечебный центр";
                    XPathServices = "";
                    SummaryColumnName = "";
                    break;

                case Enums.MoscowSites.emcmos_ru:
                    UrlRoot = "https://www.emcmos.ru";
                    UrlServicesPage = UrlRoot + "/for-patients/pay";
                    CompanyName = "Европейский медицинский центр";
                    XPathServices = "//div[@class='row list-node-block price-lists-block']//a[@href]";
                    SummaryColumnName = "";
                    break;

                default:
                    break;
            }
        }

        private void SetupSaintPetersburgSites(Enums.SaintPetersburgSites saintPetersburgSite) {
            CityName = "Санкт-Петербург";

            switch (saintPetersburgSite) {
                case Enums.SaintPetersburgSites.mc21_ru:
                    UrlRoot = "https://www.mc21.ru";
                    UrlServicesPage = UrlRoot + "/price/";
                    CompanyName = "Группа компаний Медицинский центр «XXI век»";
                    XPathServices = "//div[@class='mc_short_price']";
                    SummaryColumnName = "Медицинский центр «XXI век»";
                    ShouldAutoLoad = true;
                    break;

                //case Enums.SaintPetersburgSites.evro_med_ru:
                //	UrlRoot = "http://evro-med.ru";
                //	UrlServicesPage = UrlRoot + "/ceny/";
                //	CompanyName = "Многопрофильная клиника \"Европейский медицинский центр\"";
                //	XPathServices = "//div[@class='b-editor']//a[@href]";
                //	break;

                case Enums.SaintPetersburgSites.baltzdrav_ru:
                    UrlRoot = "http://baltzdrav.ru";
                    UrlServicesPage = UrlRoot + "/services";
                    CompanyName = "Сеть многопрофильных клиник “БалтЗдрав”";
                    XPathServices = "//div[@class='uk-panel uk-panel-box box']//a[@href]";
                    SummaryColumnName = "Медицинский центр БалтЗдрав";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SaintPetersburgSites.german_clinic:
                    UrlRoot = "https://german.clinic";
                    UrlServicesPage = UrlRoot;
                    CompanyName = "Немецкая семейная клиника";
                    XPathServices = "//ul[@class='topmenu__container']/li[2]/ul[1]//a[@href]";
                    SummaryColumnName = "Немецкая семейная клиника";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SaintPetersburgSites.german_dental:
                    UrlRoot = "https://german.dental";
                    UrlServicesPage = UrlRoot;
                    CompanyName = "Немецкая семейная стоматология";
                    XPathServices = "//div[@class='service']//a[@href]";
                    SummaryColumnName = "Немецкая семейная клиника стоматология";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SaintPetersburgSites.clinic_complex_ru:
                    UrlRoot = "http://clinic-complex.ru";
                    UrlServicesPage = UrlRoot + "/price/";
                    CompanyName = "Современные медицинские технологии";
                    XPathServices = "//div[@class='col-md-12 col-sm-12']";
                    SummaryColumnName = "Современные Медицинские Технологии(СМТ)";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SaintPetersburgSites.medswiss_spb_ru:
                    UrlRoot = "http://medswiss-spb.ru";
                    UrlServicesPage = UrlRoot + "/tseny/";
                    CompanyName = "MedSwiss Санкт-Петер6ург";
                    XPathServices = "//table[@id='viseble']//a[@href]";
                    SummaryColumnName = "Медицинский центр MedSwiss";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SaintPetersburgSites.invitro_ru:
                    UrlRoot = "https://www.invitro.ru";
                    UrlServicesPage = UrlRoot + "/analizes/for-doctors/piter/";
                    CompanyName = "ООО «ИНВИТРО»";
                    XPathServices = "//div[@class='node' and @data-prices]";
                    SummaryColumnName = "";
                    IsLocalFile = true;
                    break;

                case Enums.SaintPetersburgSites.helix_ru:
                    UrlRoot = "https://helix.ru";
                    UrlServicesPage = UrlRoot + "/catalog";
                    CompanyName = "ООО «НПФ «ХЕЛИКС»";
                    XPathServices = "//div[@class='Catalog-Content-Navigation']//span[starts-with(@class,'Catalog-Content-Navigation')]";
                    SummaryColumnName = "";
                    break;

                case Enums.SaintPetersburgSites.emcclinic_ru:
                    UrlRoot = "http://www.emcclinic.ru";
                    UrlServicesPage = UrlRoot + "/services";
                    CompanyName = "ООО \"Единые Медицинские Системы\"";
                    XPathServices = "//div[@class='n-services__item']//a[@href]";
                    SummaryColumnName = "EMC-многопрофильная семейная клиника";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SaintPetersburgSites.legal_entity_allergomed:
                    UrlRoot = "";
                    UrlServicesPage = UrlRoot + "";
                    CompanyName = "Аллергомед";
                    XPathServices = "";
                    SummaryColumnName = "";
                    break;

                case Enums.SaintPetersburgSites.legal_entity_odont:
                    UrlRoot = "";
                    UrlServicesPage = UrlRoot + "";
                    CompanyName = "Одонт";
                    XPathServices = "";
                    SummaryColumnName = "";
                    break;

                case Enums.SaintPetersburgSites.legal_entity_pervaya_semeinaya_klinika:
                    UrlRoot = "";
                    UrlServicesPage = UrlRoot + "";
                    CompanyName = "Первая семейная клиника";
                    XPathServices = "";
                    SummaryColumnName = "";
                    break;

                case Enums.SaintPetersburgSites.reasunmed_ru:
                    UrlRoot = "https://reasunmed.ru";
                    UrlServicesPage = UrlRoot + "/prays-uslugi/";
                    CompanyName = "Реасанмед";
                    XPathServices = "//div[@id='imedica-dep-accordion']";
                    SummaryColumnName = "РеаСанМед";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SaintPetersburgSites.dcenergo_ru:
                    UrlRoot = "https://dcenergo.ru";
                    UrlServicesPage = UrlRoot + "/lechenie/";
                    CompanyName = "МЦ \"Энерго\"";
                    XPathServices = "//div[@class='d-c-list spacer']//a[@href]";
                    SummaryColumnName = "МЦ Энерго";
					ShouldAutoLoad = true;
                    break;

                case Enums.SaintPetersburgSites.dcenergo_kids_ru:
                    UrlRoot = "https://dcenergo.ru";
                    UrlServicesPage = UrlRoot + "/pediatricheskoe_otdelenie/";
                    CompanyName = "МЦ \"Энерго\"";
                    XPathServices = "//div[@class='specialties-item']//a[@class='specialties-item__title']";
                    SummaryColumnName = "МЦ Энерго дети";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SaintPetersburgSites.allergomed_ru:
                    UrlRoot = "https://allergomed.ru";
                    UrlServicesPage = UrlRoot + "/prices/";
                    CompanyName = "Аллергомед";
                    XPathServices = "//div[@class='one_adult_direction']";
                    SummaryColumnName = "Аллергомед";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SaintPetersburgSites.starsclinic_ru:
                    UrlRoot = "http://starsclinic.ru";
                    UrlServicesPage = UrlRoot + "/services/";
                    CompanyName = "Клиника \"Звездная\"";
                    XPathServices = "//div[@class='services']//div[@class='media-body']//h6//a[@href]";
                    SummaryColumnName = "Звездная";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SaintPetersburgSites.clinica_blagodat_ru:
                    UrlRoot = "https://clinica-blagodat.ru";
                    UrlServicesPage = UrlRoot + "/tseny/";
                    CompanyName = "Клиника \"Благодатная\"";
                    XPathServices = "//div[@class='entry-content']//a[@href]";
                    SummaryColumnName = "Благодатная";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SaintPetersburgSites.med_vdk_ru:
                    UrlRoot = "http://med-vdk.ru";
                    UrlServicesPage = UrlRoot + "/ceny/terapiya/terapevt/";
                    CompanyName = "МЦ \"Водоканал\"";
                    XPathServices = "//div[@class='menu-inner-main']//a[@href]";
                    SummaryColumnName = "МЦ Водоканал";
                    ShouldAutoLoad = true;
                    break;

                default:
                    break;
            }
        }

        private void SetupSochiSites(Enums.SochiSites sochiSite) {
            CityName = "Сочи";

            switch (sochiSite) {
                case Enums.SochiSites.armed_mc_ru:
                    UrlRoot = "http://armed-mc.ru";
                    UrlServicesPage = UrlRoot + "/tseny/";
                    CompanyName = "ООО \"АРМЕД\"";
                    XPathServices = "//div[@class='mk-accordion-single']";
                    SummaryColumnName = "Медицинский центр «АРМЕД»";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SochiSites.uzlovaya_poliklinika_ru:
                    UrlRoot = "http://uzlovaya-poliklinika.ru";
                    UrlServicesPage = UrlRoot + "/price/";
                    CompanyName = "Узловая поликлиника на станции Сочи";
                    XPathServices = "//div[@class='pricelist-group btop pd-m']";
                    SummaryColumnName = "";
                    break;

                case Enums.SochiSites._23doc_ru_main_price:
                    UrlRoot = "http://23doc.ru";
                    UrlServicesPage = UrlRoot + "/prays-list";
                    CompanyName = "Детский диагностический центр \"Семья\"";
                    XPathServices = "//ul[@class='accordion_square accordion-rounded2']//li";
                    CityName = "Сочи";
                    SummaryColumnName = "ДЦ Семья";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SochiSites._23doc_ru_doctors:
                    UrlRoot = "http://23doc.ru";
                    UrlServicesPage = UrlRoot + "/spetsialisty";
                    CompanyName = "Детский диагностический центр \"Семья\"";
                    XPathServices = "//li[@class='g-submenu__item']//a[@href]";
                    CityName = "Сочи";
                    SummaryColumnName = "ДЦ Семья специалисты";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SochiSites._23doc_ru_lab:
                    UrlRoot = "http://23doc.ru";
                    UrlServicesPage = UrlRoot + "/priyem-analizov";
                    CompanyName = "Детский диагностический центр \"Семья\"";
                    XPathServices = "//ul[@class='accordion_square accordion-rounded2']//li";
                    CityName = "Сочи";
                    SummaryColumnName = "ДЦ Семья лаборатория";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SochiSites.medcentr_sochi_ru:
                    UrlRoot = "https://medcentr-sochi.ru";
                    UrlServicesPage = UrlRoot + "/uslugi-i-ceny-medicinskogo-centra-proksima.html";
                    CompanyName = "Медицинский клинический центр «Проксима»";
                    XPathServices = "//div[@class='pt-4']";
                    CityName = "Сочи";
                    SummaryColumnName = "МКЦ ПРОКСИМА";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SochiSites.kb4sochi_ru:
                    UrlRoot = "http://kb4sochi.ru";
                    UrlServicesPage = UrlRoot + "/pages/108";
                    CompanyName = "ГБУЗ Краевая больница №4 Сочи";
                    CityName = "Сочи";
                    break;

                case Enums.SochiSites.medprofisochi_com:
                    UrlRoot = "http://www.medprofisochi.com";
                    UrlServicesPage = UrlRoot + "/vii----";
                    CompanyName = "КЛИНИКА МЕДПРОФИ";
                    CityName = "Сочи";
                    SummaryColumnName = "КЛИНИКА МЕДПРОФИ";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SochiSites.clinic23_ru:
                    UrlRoot = "https://sochi.clinic23.ru";
                    UrlServicesPage = UrlRoot + "";
                    CompanyName = "КЛИНИКА «ЕКАТЕРИНИНСКАЯ» Сочи";
                    XPathServices = "//li[starts-with(@class,'b-menu__item')]//li[starts-with(@class,'b-submenu__item3')]//a[@href]";
                    CityName = "Сочи";
                    SummaryColumnName = "";
                    break;

                case Enums.SochiSites.clinic23_ru_lab:
                    UrlRoot = "https://sochi.clinic23.ru";
                    UrlServicesPage = UrlRoot + "/diagnostika/analizy";
                    CompanyName = "КЛИНИКА «ЕКАТЕРИНИНСКАЯ» Сочи";
                    XPathServices = "//div[@class='iitem']//a[@href]";
                    CityName = "Сочи";
                    SummaryColumnName = "";
                    break;

                case Enums.SochiSites._5vrachey_com:
                    UrlRoot = "https://www.5vrachey.com";
                    UrlServicesPage = UrlRoot + "/uslugi-i-ceny";
                    CompanyName = "Мед. центр «Пять врачей»";
                    XPathServices = "//div[@id='tableWrapper']//tbody";
                    CityName = "Сочи";
                    SummaryColumnName = "Мед. центр «Пять врачей»";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SochiSites.mc_daniel_ru:
                    UrlRoot = "https://mc-daniel.ru";
                    UrlServicesPage = UrlRoot + "/price/";
                    CompanyName = "Мед. центр «Даниэль»";
                    XPathServices = "//li[starts-with(@class,'item expandable')]";
                    CityName = "Сочи";
                    SummaryColumnName = "Мед. центр «Даниэль»";
                    ShouldAutoLoad = true;
                    break;

                case Enums.SochiSites.medbr_ru:
                    UrlRoot = "https://medbr.ru";
                    UrlServicesPage = UrlRoot + "/services/";
                    CompanyName = "Клиника семейной медицины Bridge";
                    XPathServices = "//section[starts-with(@class,'boldSection top')]//div[starts-with(@class,'rowItem col-md')]";
                    CityName = "Сочи";
                    SummaryColumnName = "Клиника семейной медицины Bridge";
                    ShouldAutoLoad = true;
                    break;

				case Enums.SochiSites.analizy_sochi_ru:
					UrlRoot = "https://analizy-sochi.ru";
					UrlServicesPage = UrlRoot + "/analizy-pacientam.html";
					CompanyName = "Медицинская лаборатория ОПТИМУМ";
					XPathServices = "//table[@class='tests_costs_results']";
					CityName = "Сочи";
					SummaryColumnName = "";
					ShouldAutoLoad = true;
					break;

				default:
                    break;
            }
        }

        private void SetupKazanSites(Enums.KazanSites kazanSite) {
            switch (kazanSite) {
                case Enums.KazanSites.ava_kazan_ru:
                    UrlRoot = "https://ava-kazan.ru";
                    UrlServicesPage = UrlRoot + "";
                    CompanyName = "АО \"АВА - Казань\"";
                    XPathServices = "//nav[@class='top__menu']//a[@href]";
                    CityName = "Казань";
                    SummaryColumnName = "«АВА-КАЗАНЬ» (СКАНДИНАВИЯ)";
					ShouldAutoLoad = true;
                    break;

                case Enums.KazanSites.mc_aybolit_ru:
                    UrlRoot = "http://mc-aybolit.ru";
                    UrlServicesPage = UrlRoot + "/nashi_uslugi";
                    CompanyName = "ООО \"МЦ Айболит\"";
                    XPathServices = "//div[@id='accordion']//a[@href]";
                    CityName = "Казань";
                    SummaryColumnName = "МЦ «Айболит»";
					ShouldAutoLoad = true;
                    break;

                case Enums.KazanSites.biomed_mc_ru:
                    UrlRoot = "https://biomed-mc.ru";
                    UrlServicesPage = UrlRoot + "/static5/uslugi";
                    CompanyName = "ООО лечебно-диагностический центр \"БИОМЕД\"";
                    XPathServices = "//div[starts-with(@class,'megamenu')]//a[@href]";
                    SummaryColumnName = "Лечебно-диагностический центр «БИОМЕД»";
                    CityName = "Казань";
					ShouldAutoLoad = true;
                    break;

                case Enums.KazanSites.zdorovie7i_ru:
                    UrlRoot = "http://zdorovie7i.ru";
                    UrlServicesPage = UrlRoot + "/kalkulyator-uslug";
                    CompanyName = "Сеть лечебно-диагностических центров «Здоровье семьи»";
                    XPathServices = "//div[@class='doctor-item js-root-category']";
                    SummaryColumnName = "МЛДЦ Здоровье семьи";
                    CityName = "Казань";
					IsLocalFile = true;
					ShouldAutoLoad = true;
                    break;

                case Enums.KazanSites.starclinic_ru:
                    UrlRoot = "http://www.starclinic.ru";
                    UrlServicesPage = UrlRoot + "/prays/";
                    CompanyName = "Медцентр «ЗВЕЗДА»";
                    XPathServices = "//div[@class='news-list']//a[@href]";
                    CityName = "Казань";
                    SummaryColumnName = "МЦ «Звезда»";
					ShouldAutoLoad = true;
                    break;

                case Enums.KazanSites.love_dr_ru:
                    UrlRoot = "http://www.love-dr.ru";
                    UrlServicesPage = UrlRoot + "/tseny/";
                    CompanyName = "ООО \"Любимый доктор\"";
                    XPathServices = "//td[@class='main-column']//ul[not(@*)]//a[@href]";
                    CityName = "Казань";
                    break;

                case Enums.KazanSites.medexpert_kazan_ru:
                    UrlRoot = "http://medexpert-kazan.ru";
                    UrlServicesPage = UrlRoot + "/uslugi/";
                    CompanyName = "Сеть медицинских клиник «Медэксперт»";
                    XPathServices = "//ul[starts-with(@class,'service__list row')]//a[@href]";
                    CityName = "Казань";
                    break;

                case Enums.KazanSites.kazan_clinic_ru:
                    UrlRoot = "http://kazan-clinic.ru";
                    UrlServicesPage = UrlRoot + "/prices/";
                    CompanyName = "Казанская Клиника";
                    XPathServices = "//div[@class='catalog-section-list']//a[@href]";
                    CityName = "Казань";
                    SummaryColumnName = "Казанская Клиника";
					ShouldAutoLoad = true;
                    break;

                default:
                    break;
            }
        }

        private void SetupKamenskUralskySites(Enums.KamenskUralskySites kamenskUralskySite) {
            switch (kamenskUralskySite) {
                case Enums.KamenskUralskySites.mfcrubin_ru:
                    UrlRoot = "http://www.mfcrubin.ru";
                    UrlServicesPage = UrlRoot + "/stoimostuslug";
                    CompanyName = "ООО МФЦ РУБИН";
                    XPathServices = "//div[@class='price-row']";
                    CityName = "Каменск-Уральский";
                    SummaryColumnName = "МФЦ Рубин";
					ShouldAutoLoad = true;
                    break;

                case Enums.KamenskUralskySites.ruslabs_ru:
                    UrlRoot = "https://www.ruslabs.ru";
                    UrlServicesPage = UrlRoot + "/analizyi-i-czenyi/";
                    CompanyName = "Лаборатория «Руслаб»";
                    XPathServices = "//li[starts-with(@class,'sidebar-menu__item') and not(@id)]//a[@href]";
                    CityName = "Каменск-Уральский";
                    SummaryColumnName = "Клинико-диагностический центр Руслаб";
					ShouldAutoLoad = true;
                    break;

                case Enums.KamenskUralskySites.mc_vd_ru:
                    UrlRoot = "http://mc-vd.ru";
                    UrlServicesPage = UrlRoot + "/uslugi/";
                    CompanyName = "ООО \"Медсервис Каменск\"";
                    XPathServices = "//div[@id='nextend-accordion-menu-nextendaccordionmenuwidget-2']//a[@href]";
                    CityName = "Каменск-Уральский";
                    SummaryColumnName = "Медсервис Каменск (Ваш Доктор)";
					ShouldAutoLoad = true;
                    break;

                case Enums.KamenskUralskySites.immunoresurs_ru:
                    UrlRoot = "http://immunoresurs.ru";
                    UrlServicesPage = UrlRoot + "/uslugi-i-ceny/";
                    CompanyName = "ООО Медицинский центр \"Иммуноресурс\"";
                    XPathServices = "//div[@id='global3']//a[@href]";
                    CityName = "Каменск-Уральский";
                    SummaryColumnName = "Медицинский центр Иммуноресурс";
					ShouldAutoLoad = true;
                    break;

                case Enums.KamenskUralskySites.medkamensk_ru:
                    UrlRoot = "http://medkamensk.ru";
                    UrlServicesPage = UrlRoot + "/prejskurant-tsen/";
                    CompanyName = "ГБУЗ Свердловской обл. «Городская больница г.Каменск - Уральский»";
                    XPathServices = "//table[@class='waffle']//tr";
                    CityName = "Каменск-Уральский";
                    SummaryColumnName = "ГБУЗ СО Городская больница г. Каменск -Уральский";
                    IsLocalFile = true;
					ShouldAutoLoad = true;
                    break;

                case Enums.KamenskUralskySites.invitro_ru:
                    UrlRoot = "https://www.invitro.ru";
                    UrlServicesPage = UrlRoot + "/analizes/for-doctors/kamensk-uralskiy/";
                    CompanyName = "ООО «ИНВИТРО»";
                    XPathServices = "//div[@class='node' and @data-prices]";
                    CityName = "Каменск-Уральский";
                    IsLocalFile = true;
                    break;

                default:
                    break;
            }
        }

        private void SetupKrasnodarSites(Enums.KrasnodarSites krasnodarSite) {
            switch (krasnodarSite) {
                case Enums.KrasnodarSites.clinic23_ru:
                    UrlRoot = "https://www.clinic23.ru";
                    UrlServicesPage = UrlRoot + "/price";
                    CompanyName = "КЛИНИКА «ЕКАТЕРИНИНСКАЯ»";
                    XPathServices = "//div[@class='b-sidebar__section']//a[@href]";
                    CityName = "Краснодар";
                    SummaryColumnName = "КЛИНИКА «ЕКАТЕРИНИНСКАЯ»";
                    ShouldAutoLoad = true;
                    break;

                case Enums.KrasnodarSites.clinic23_ru_lab:
                    UrlRoot = "https://www.clinic23.ru";
                    UrlServicesPage = UrlRoot + "/diagnostika/analizy";
                    CompanyName = "КЛИНИКА «ЕКАТЕРИНИНСКАЯ»";
                    XPathServices = "//div[@class='iitem']//a[@href]";
                    CityName = "Краснодар";
                    SummaryColumnName = "КЛИНИКА «ЕКАТЕРИНИНСКАЯ» лаборатория";
					ShouldAutoLoad = true;
                    break;

                case Enums.KrasnodarSites.clinicist_ru:
                    UrlRoot = "https://www.clinicist.ru";
                    UrlServicesPage = UrlRoot + "/tseny/";
                    CompanyName = "Сеть медицинских центров «Клиницист»";
                    XPathServices = "//table[@class='visual-table']//tbody//tr";
                    CityName = "Краснодар";
                    SummaryColumnName = "Сеть МЦ КЛИНИЦИСТ";
                    ShouldAutoLoad = true;
                    break;

                case Enums.KrasnodarSites.poly_clinic_ru:
                    UrlRoot = "https://poly-clinic.ru";
                    UrlServicesPage = UrlRoot + "/price/";
                    CompanyName = "Клиника семейного здоровья Сити-Клиник";
                    XPathServices = "//div[@class='news-list']//a[@href]";
                    CityName = "Краснодар";
                    SummaryColumnName = "Сити-Клиник";
                    ShouldAutoLoad = true;
                    break;

                case Enums.KrasnodarSites.clinica_nazdorovie_ru:
                    UrlRoot = "http://clinica-nazdorovie.ru";
                    UrlServicesPage = UrlRoot + "";
                    CompanyName = "ООО \"МФО \"Клиника На здоровье\"";
                    XPathServices = "//div[@class=' top-menu-main']/ul[1]/li[4]//a[@href]";
                    CityName = "Краснодар";
                    SummaryColumnName = "Клиника На здоровье Тургеневская";
                    ShouldAutoLoad = true;
                    break;

                case Enums.KrasnodarSites.clinica_nazdorovie_ru_lab:
                    UrlRoot = "http://clinica-nazdorovie.ru";
                    UrlServicesPage = UrlRoot + "/m/products/";
                    CompanyName = "ООО \"МФО \"Клиника На здоровье\"";
                    XPathServices = "//div[@class='side-menu']//a[@href]";
                    CityName = "Краснодар";
                    SummaryColumnName = "Клиника На здоровье Тургеневская лаборатория";
					ShouldAutoLoad = true;
                    break;

                case Enums.KrasnodarSites.kuban_kbl_ru:
                    UrlRoot = "https://www.kuban-kbl.ru";
                    UrlServicesPage = UrlRoot + "/ceni.php";
                    CompanyName = "ОАО «ЦВМР «Краснодарская бальнеолечебница»";
                    XPathServices = "//div[@class='panel panel-default']";
                    CityName = "Краснодар";
                    SummaryColumnName = "Краснодарская Бальнеолечебница";
                    ShouldAutoLoad = true;
                    break;

                //case Enums.KrasnodarSites.solnechnaya:
                //	UrlRoot = "";
                //	UrlServicesPage = UrlRoot + "";
                //	CompanyName = "Солнечная";
                //	XPathServices = "";
                //	City = "Краснодар";
                //	SummaryColumnName = "Солнечная";
                //	break;

                //case Enums.KrasnodarSites.v_nadezhnyh_rykah:
                //	UrlRoot = "";
                //	UrlServicesPage = UrlRoot + "";
                //	CompanyName = "В надежных руках";
                //	XPathServices = "";
                //	City = "Краснодар";
                //	SummaryColumnName = "В надежных руках";
                //	break;

                //case Enums.KrasnodarSites.evromed:
                //	UrlRoot = "";
                //	UrlServicesPage = UrlRoot + "";
                //	CompanyName = "Евромед";
                //	XPathServices = "";
                //	City = "Краснодар";
                //	SummaryColumnName = "Евромед";
                //	break;

                case Enums.KrasnodarSites.vrukah_com:
                    UrlRoot = "https://vrukah.com";
                    UrlServicesPage = UrlRoot + "/branches/";
                    CompanyName = "В Надежных руках";
                    XPathServices = "//div[@class='branchesBlock']//a[@href]";
                    CityName = "Краснодар";
                    SummaryColumnName = "В Надежных руках";
                    ShouldAutoLoad = true;
                    break;

                case Enums.KrasnodarSites.vrukah_com_lab:
                    UrlRoot = "https://vrukah.com";
                    UrlServicesPage = UrlRoot + "/obsledovanie/";
                    CompanyName = "В Надежных руках";
                    XPathServices = "//div[@class='branchesBlock']//a[@href]";
                    CityName = "Краснодар";
                    SummaryColumnName = "";
                    break;

                default:
                    break;
            }
        }

        private void SetupUfaSites(Enums.UfaSites ufaSite) {
            switch (ufaSite) {
                case Enums.UfaSites.megi_clinic:
                    UrlRoot = "http://megi.clinic";
                    UrlServicesPage = UrlRoot + "/cost/";
                    CompanyName = "Сеть клиник «МЕГИ»";
                    XPathServices = "//div[@class='detail_cost']//div[@class='test']";
                    CityName = "Уфа";
                    SummaryColumnName = "МЕГИ";
					ShouldAutoLoad = true;
                    break;

                case Enums.UfaSites.promedicina_clinic:
                    UrlRoot = "https://www.promedicina.clinic";
                    UrlServicesPage = UrlRoot + "/adult/services/";
                    CompanyName = "ООО ММЦ «Профилактическая медицина»";
                    XPathServices = "//div[starts-with(@class,'col-md-4')]//a[@href]";
                    CityName = "Уфа";
                    break;

                case Enums.UfaSites.mamadeti_ru:
                    UrlRoot = "http://ufa.mamadeti.ru";
                    UrlServicesPage = UrlRoot + "/price-list2/hospital-mother-and-child-ufa/price/";
                    CompanyName = "Группа компаний «Мать и дитя»";
                    XPathServices = "//div[@class='b-tree_link__item']";
                    CityName = "Уфа";
                    SummaryColumnName = "«Мать и дитя»";
					ShouldAutoLoad = true;
                    break;

                case Enums.UfaSites.rkbkuv_ru:
                    UrlRoot = "https://rkbkuv.ru";
                    UrlServicesPage = UrlRoot + "/list_paid_services";
                    CompanyName = "ГБУЗ РКБ им. Г.Г. Куватова";
                    XPathServices = "";
                    CityName = "Уфа";
                    SummaryColumnName = "ГБУЗ РКБ им. Куватова";
					ShouldAutoLoad = true;
                    break;

                case Enums.UfaSites.mdplus_ru:
                    UrlRoot = "http://www.ufamdplus.ru";
                    UrlServicesPage = UrlRoot + "/services/prays-list/";
                    CompanyName = "Клиника «МД плюс»";
                    XPathServices = "//div[@class='col-lg-12']";
                    CityName = "Уфа";
                    SummaryColumnName = "МД +";
					ShouldAutoLoad = true;
                    break;

                default:
                    break;
            }
        }

        private void SetupOtherSites(Enums.OtherSites otherSite) {
            switch (otherSite) {
                case Enums.OtherSites.kovrov_clinicalcenter_ru:
                    UrlRoot = "https://clinicalcenter.ru";
                    UrlServicesPage = UrlRoot + "/uslugi/";
                    CompanyName = "Первый клинический медицинский центр";
                    XPathServices = "//div[@class='directions-list']//a[@href]";
                    CityName = "Ковров";
                    break;
                case Enums.OtherSites.nedorezov_mc_ru:
                    UrlRoot = "http://mc.ru:8080/";
                    UrlServicesPage = UrlRoot + "";
                    CompanyName = "Металл-Сервис";
                    XPathServices = "//div[@id='catalog']//ul[@class='inserted']//a[@href]";
                    CityName = "Нижний Новгород";
                    break;
                case Enums.OtherSites.nedorezov_prom_metall_kz:
                    UrlRoot = "https://prom-metall.kz/";
                    UrlServicesPage = UrlRoot + "";
                    CompanyName = "Промышленная металлургия";
                    //XPathServices = "//div[@class='container-fluid']//div[@class='grid']//figure[@class='effect-lily']//a[@href]";
                    XPathServices = "//ul[@id='menu-vertical-menu']//a[@href]";
                    CityName = "Нижний Новгород";
                    break;

                default:
                    break;
            }
        }




		public SiteInfo(Enums.Cities cityValue, int siteValue) {
			CityValue = cityValue;
            SiteValue = siteValue;
			ServiceGroupItems = new List<Items.ServiceGroup>();

            switch (cityValue) {
                case Enums.Cities.Moscow:
                    SetupMoscowSites((Enums.MoscowSites)siteValue);
                    break;
                case Enums.Cities.SaintPetersburg:
                    SetupSaintPetersburgSites((Enums.SaintPetersburgSites)siteValue);
                    break;
                case Enums.Cities.Sochi:
                    SetupSochiSites((Enums.SochiSites)siteValue);
                    break;
                case Enums.Cities.Kazan:
                    SetupKazanSites((Enums.KazanSites)siteValue);
                    break;
                case Enums.Cities.KamenskUralsky:
                    SetupKamenskUralskySites((Enums.KamenskUralskySites)siteValue);
                    break;
                case Enums.Cities.Krasnodar:
                    SetupKrasnodarSites((Enums.KrasnodarSites)siteValue);
                    break;
                case Enums.Cities.Ufa:
                    SetupUfaSites((Enums.UfaSites)siteValue);
                    break;
                case Enums.Cities.Other:
                    SetupOtherSites((Enums.OtherSites)siteValue);
                    break;
                default:
                    break;
            }
		}
	}
}
