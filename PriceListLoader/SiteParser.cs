using HtmlAgilityPack;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace PriceListLoader {
    class SiteParser {
		private readonly HtmlAgility htmlAgility = new HtmlAgility();
		private readonly BackgroundWorker backgroundWorker;

		private double progressCurrent = 0;
		private double progressTo = 100;

		public SiteParser(BackgroundWorker backgroundWorker) {
			this.backgroundWorker = backgroundWorker;
		}

		public string ParseSelectedSite(SiteInfo siteInfo, bool isAutoMode = false) {
			backgroundWorker.ReportProgress((int)progressCurrent, siteInfo.CompanyName);
			backgroundWorker.ReportProgress((int)progressCurrent, "Загрузка данных с сайта: " + siteInfo.UrlRoot);

			HtmlDocument docServices;

            if (siteInfo.IsLocalFile) {
				if (isAutoMode) {
					backgroundWorker.ReportProgress(0, "Требуется выбрать файл для обработки");
					return string.Empty;
				}

				OpenFileDialog openFileDialog = new OpenFileDialog {
					Filter = "HTML файл (*.html)|*.html",
					CheckFileExists = true,
					Multiselect = false,
					Title = "Выберите HTML файл для обработки"
				};

				if (openFileDialog.ShowDialog() == true) {
					siteInfo.UrlServicesPage = openFileDialog.FileName;
				} else {
					backgroundWorker.ReportProgress((int)progressCurrent, "Не выбран файл для обработки");
					return string.Empty;
				}

			}

			docServices = htmlAgility.GetDocument(siteInfo.UrlServicesPage, siteInfo);

			if (docServices == null) {
				backgroundWorker.ReportProgress((int)progressCurrent, "Не удалось загрузить страницу: " + siteInfo.UrlServicesPage);
				Console.WriteLine("docServices is null");
				return string.Empty;
			}

            switch (siteInfo.CityValue) {
                case Enums.Cities.Moscow:
                    RegionParsers.ParseMoscow parseMoscow = new RegionParsers.ParseMoscow(htmlAgility, backgroundWorker, siteInfo);

                    switch ((Enums.MoscowSites)siteInfo.SiteValue) {
                        case Enums.MoscowSites.fdoctor_ru:
                        case Enums.MoscowSites.familydoctor_ru:
                        case Enums.MoscowSites.familydoctor_ru_child:
                        case Enums.MoscowSites.nrmed_ru:
                        case Enums.MoscowSites.nrmed_ru_child:
                        case Enums.MoscowSites.smclinic_ru:
                        case Enums.MoscowSites.cmd_online_ru:
                        case Enums.MoscowSites.helix_ru:
                        case Enums.MoscowSites.dentol_ru:
                        case Enums.MoscowSites.zub_ru:
                        case Enums.MoscowSites.gemotest_ru:
                        case Enums.MoscowSites.kdl_ru:
                        case Enums.MoscowSites.medsi_ru:
                        case Enums.MoscowSites.onclinic_ru:
                        case Enums.MoscowSites.nrmedlab_ru:
                        case Enums.MoscowSites.sm_stomatology_ru:
                            parseMoscow.ParseSiteWithLinksOnMainPage(docServices);
                            break;
                        case Enums.MoscowSites.alfazdrav_ru:
                            parseMoscow.ParseSiteAlfazdrav(docServices);
                            break;
                        case Enums.MoscowSites.invitro_ru:
                            parseMoscow.ParseSiteInvitroRU(docServices);
                            break;
                        case Enums.MoscowSites.mrt24_ru:
                            parseMoscow.ParseSiteMrt24Ru(docServices);
                            break;
                        case Enums.MoscowSites.vse_svoi_ru:
                            parseMoscow.ParseSiteVseSvoiRu(docServices);
                            break;
                        case Enums.MoscowSites.novostom_ru:
                            parseMoscow.ParseSiteNovostomRu(docServices);
                            break;
                        case Enums.MoscowSites.masterdent_ru:
                            parseMoscow.ParseSiteMasterdentRu(docServices);
                            break;
                        case Enums.MoscowSites.smdoctor_ru:
                            parseMoscow.ParseSiteSmDoctorRu(docServices);
                            break;
                        case Enums.MoscowSites.smclinic_ru_lab:
                            parseMoscow.ParseSiteSmClinicRuLab(docServices);
                            break;

                        default:
                            break;
                    }

                    break;

                case Enums.Cities.SaintPetersburg:
                    RegionParsers.ParseSpb parseSpb = new RegionParsers.ParseSpb(htmlAgility, backgroundWorker, siteInfo);

                    switch ((Enums.SaintPetersburgSites)siteInfo.SiteValue) {
                        //case Enums.SaintPetersburgSites.evro_med_ru:
                        case Enums.SaintPetersburgSites.baltzdrav_ru:
                        case Enums.SaintPetersburgSites.german_clinic:
                        case Enums.SaintPetersburgSites.german_dental:
                        case Enums.SaintPetersburgSites.medswiss_spb_ru:
                        case Enums.SaintPetersburgSites.helix_ru:
                        case Enums.SaintPetersburgSites.emcclinic_ru:
                        case Enums.SaintPetersburgSites.dcenergo_ru:
                        case Enums.SaintPetersburgSites.dcenergo_kids_ru:
                        case Enums.SaintPetersburgSites.starsclinic_ru:
                        case Enums.SaintPetersburgSites.clinica_blagodat_ru:
                        case Enums.SaintPetersburgSites.med_vdk_ru:
                            parseSpb.ParseSiteWithLinksOnMainPage(docServices);
                            break;
                        case Enums.SaintPetersburgSites.invitro_ru:
                            parseSpb.ParseSiteInvitroRU(docServices);
                            break;
                        case Enums.SaintPetersburgSites.clinic_complex_ru:
                            parseSpb.ParseSiteSpbClinicComplexTitlePage(docServices);
                            break;
                        case Enums.SaintPetersburgSites.reasunmed_ru:
                            parseSpb.ParseSiteSpbReasunmedRu(docServices);
                            break;
                        case Enums.SaintPetersburgSites.allergomed_ru:
                            parseSpb.ParseSiteSpbAllergomedRu(docServices);
                            break;
                        case Enums.SaintPetersburgSites.mc21_ru:
                            parseSpb.ParseSiteSpbMc21Ru(docServices);
                            break;
                        default:
                            break;
                    }

                    break;

                case Enums.Cities.Sochi:
                    RegionParsers.ParseSochi parseSochi = new RegionParsers.ParseSochi(htmlAgility, backgroundWorker, siteInfo);

                    switch ((Enums.SochiSites)siteInfo.SiteValue) {
                        case Enums.SochiSites._23doc_ru_doctors:
                        case Enums.SochiSites.clinic23_ru:
                        case Enums.SochiSites.clinic23_ru_lab:
                            parseSochi.ParseSiteWithLinksOnMainPage(docServices);
                            break;
                        case Enums.SochiSites.armed_mc_ru:
                            parseSochi.ParseSiteArmedRu(docServices);
                            break;
                        case Enums.SochiSites.uzlovaya_poliklinika_ru:
                            parseSochi.ParseSiteUzlovayaPoliklinikaRu(docServices);
                            break;
                        case Enums.SochiSites._23doc_ru_main_price:
                        case Enums.SochiSites._23doc_ru_lab:
                            parseSochi.ParseSite23docRu(docServices);
                            break;
                        case Enums.SochiSites.medcentr_sochi_ru:
                            parseSochi.ParseSiteMedcentrSochiRu(docServices);
                            break;
                        case Enums.SochiSites._5vrachey_com:
                            parseSochi.ParseSite5vracheyCom(docServices);
                            break;
                        case Enums.SochiSites.mc_daniel_ru:
                            parseSochi.ParseSiteMcDanielRu(docServices);
                            break;
                        default:
                            break;
                    }

                    break;

                case Enums.Cities.Kazan:
                    RegionParsers.ParseKazan parseKazan = new RegionParsers.ParseKazan(htmlAgility, backgroundWorker, siteInfo);

                    switch ((Enums.KazanSites)siteInfo.SiteValue) {
						case Enums.KazanSites.ava_kazan_ru:
                        case Enums.KazanSites.mc_aybolit_ru:
                        case Enums.KazanSites.biomed_mc_ru:
                        case Enums.KazanSites.starclinic_ru:
                        case Enums.KazanSites.love_dr_ru:
                        case Enums.KazanSites.medexpert_kazan_ru:
                        case Enums.KazanSites.kazan_clinic_ru:
                            parseKazan.ParseSiteWithLinksOnMainPage(docServices);
                            break;
						case Enums.KazanSites.zdorovie7i_ru:
							parseKazan.ParseSiteKazanZdorovie7iRu(docServices);
							break;
						default:
                            break;
                    }

                    break;

                case Enums.Cities.KamenskUralsky:
                    RegionParsers.ParseYekuk parseYekuk = new RegionParsers.ParseYekuk(htmlAgility, backgroundWorker, siteInfo);

                    switch ((Enums.KamenskUralskySites)siteInfo.SiteValue) {
                        case Enums.KamenskUralskySites.ruslabs_ru:
                        case Enums.KamenskUralskySites.mc_vd_ru:
                        case Enums.KamenskUralskySites.immunoresurs_ru:
                            parseYekuk.ParseSiteWithLinksOnMainPage(docServices);
                            break;
                        case Enums.KamenskUralskySites.invitro_ru:
                            parseYekuk.ParseSiteInvitroRU(docServices);
                            break;
                        case Enums.KamenskUralskySites.mfcrubin_ru:
                            Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() { Link = siteInfo.UrlServicesPage };
                            parseYekuk.ParseSiteYekukMfcrubinRu(docServices, ref itemServiceGroup);
                            siteInfo.ServiceGroupItems.Add(itemServiceGroup);
                            break;
                        case Enums.KamenskUralskySites.medkamensk_ru:
                            parseYekuk.ParseSiteYekukMedkamenskRu(docServices);
                            break;
                        default:
                            break;
                    }

                    break;

                case Enums.Cities.Krasnodar:
                    RegionParsers.ParseKrd parseKrd = new RegionParsers.ParseKrd(htmlAgility, backgroundWorker, siteInfo);

                    switch ((Enums.KrasnodarSites)siteInfo.SiteValue) {
                        case Enums.KrasnodarSites.clinic23_ru:
                        case Enums.KrasnodarSites.clinic23_ru_lab:
                        case Enums.KrasnodarSites.poly_clinic_ru:
                        case Enums.KrasnodarSites.clinica_nazdorovie_ru:
                        case Enums.KrasnodarSites.clinica_nazdorovie_ru_lab:
                        case Enums.KrasnodarSites.vrukah_com:
                        case Enums.KrasnodarSites.vrukah_com_lab:
                            parseKrd.ParseSiteWithLinksOnMainPage(docServices);
                            break;
                        case Enums.KrasnodarSites.clinicist_ru:
                            parseKrd.ParseSiteKrdClinicistRu(docServices);
                            break;
                        case Enums.KrasnodarSites.kuban_kbl_ru:
                            parseKrd.ParseSiteKrdKubanKdlRu(docServices);
                            break;
                        default:
                            break;
                    }

                    break;

                case Enums.Cities.Ufa:
                    RegionParsers.ParseUfa parseUfa = new RegionParsers.ParseUfa(htmlAgility, backgroundWorker, siteInfo);

                    switch ((Enums.UfaSites)siteInfo.SiteValue) {
                        case Enums.UfaSites.promedicina_clinic:
                            parseUfa.ParseSiteWithLinksOnMainPage(docServices);
                            break;
                        case Enums.UfaSites.megi_clinic:
                            parseUfa.ParseSiteUfaMegiClinic(docServices);
                            break;
                        case Enums.UfaSites.mamadeti_ru:
                            parseUfa.ParseSiteUfaMamadetiRu(docServices);
                            break;
                        case Enums.UfaSites.mdplus_ru:
                            parseUfa.ParseSiteUfaMdplusRu(docServices);
                            break;
                        default:
                            break;
                    }

                    break;

                case Enums.Cities.Other:
                    RegionParsers.ParseOther parseOther = new RegionParsers.ParseOther(htmlAgility, backgroundWorker, siteInfo);

                    switch ((Enums.OtherSites)siteInfo.SiteValue) {
                        case Enums.OtherSites.kovrov_clinicalcenter_ru:
                        case Enums.OtherSites.nedorezov_mc_ru:
                        case Enums.OtherSites.nedorezov_prom_metall_kz:
                            parseOther.ParseSiteWithLinksOnMainPage(docServices);
                            break;
                        default:
                            break;
                    }

                    break;

                default:
                    break;
            }

			if (siteInfo.ServiceGroupItems.Count == 0) {
				backgroundWorker.ReportProgress((int)progressCurrent, App.errorPrefix + "Не удалось найти ни одной группы услуг");
				return string.Empty;
			}

			return NpoiExcel.WriteItemSiteDataToExcel(siteInfo, backgroundWorker, progressCurrent, progressTo);
		}
	}
}
