using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceListLoader.RegionParsers {
    abstract class ParseGeneral {
        protected readonly HtmlAgility htmlAgility = new HtmlAgility();
        protected BackgroundWorker backgroundWorker;
        protected SiteInfo siteInfo;

        protected double progressCurrent = 0;
        protected double progressTo = 100;

        public ParseGeneral(HtmlAgility htmlAgility, BackgroundWorker bworker, SiteInfo siteInfo) {
            this.htmlAgility = htmlAgility;
            backgroundWorker = bworker;
            this.siteInfo = siteInfo;
        }

        public void ParseSiteWithLinksOnMainPage(HtmlDocument docServices, bool isFirstCycle = true, string itemGroupNamePrefix = "") {
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollectionServices == null) {
                Console.WriteLine("nodeCollectionServices is null");
                return;
            }

            if (siteInfo.CityValue == Enums.Cities.SaintPetersburg &&
                (Enums.SaintPetersburgSites)siteInfo.SiteValue == Enums.SaintPetersburgSites.baltzdrav_ru) {
                Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
                    Link = siteInfo.UrlServicesPage
                };

                (this as RegionParsers.ParseSpb).ParseSiteSpbBaltzdravRu(docServices, ref itemServiceGroup);

                if (itemServiceGroup.ServiceItems.Count > 0)
                    siteInfo.ServiceGroupItems.Add(itemServiceGroup);
            }

            if (siteInfo.CityValue == Enums.Cities.KamenskUralsky &&
                (Enums.KamenskUralskySites)siteInfo.SiteValue == Enums.KamenskUralskySites.immunoresurs_ru) {
                Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
                    Name = "",
                    Link = siteInfo.UrlServicesPage
                };

                (this as RegionParsers.ParseYekuk).ParseSiteYekukImmunoresursRu(docServices, ref itemServiceGroup);
                siteInfo.ServiceGroupItems.Add(itemServiceGroup);
            }

            double progressStep = (progressTo - progressCurrent) * 0.7 / nodeCollectionServices.Count;
            backgroundWorker.ReportProgress((int)progressCurrent, "Загрузка прайс-листов для групп услуг");

            foreach (HtmlNode servicePage in nodeCollectionServices) {
                progressCurrent += progressStep;

                try {
                    string serviceName = ClearString(servicePage.InnerText);

                    if (siteInfo.CityValue == Enums.Cities.SaintPetersburg &&
                        (Enums.SaintPetersburgSites)siteInfo.SiteValue == Enums.SaintPetersburgSites.emcclinic_ru &&
                        serviceName.ToLower().Equals("записаться"))
                        continue;

                    if (siteInfo.CityValue == Enums.Cities.Other &&
                        (Enums.OtherSites)siteInfo.SiteValue == Enums.OtherSites.nedorezov_prom_metall_kz) {
                        HtmlNode nodeServiceNameH2 = servicePage.SelectSingleNode(servicePage.XPath + "//h2");
                        if (nodeServiceNameH2 != null)
                            serviceName = ClearString(nodeServiceNameH2.InnerText);

                        if (isFirstCycle && !serviceName.StartsWith("Черный прокат"))
                            continue;
                    }

                    backgroundWorker.ReportProgress((int)progressCurrent, serviceName);
                    Console.WriteLine("serviceName: " + serviceName);

                    string urlService = string.Empty;

                    if (servicePage.Attributes.Contains("href")) {
                        string hrefValue = servicePage.Attributes["href"].Value;
                        if (hrefValue.StartsWith("http")) {
                            urlService = hrefValue;
                        } else {
                            if (!hrefValue.StartsWith("/"))
                                hrefValue = "/" + hrefValue;

                            if (siteInfo.CityValue == Enums.Cities.Moscow &&
                                (Enums.MoscowSites)siteInfo.SiteValue == Enums.MoscowSites.nrmedlab_ru)
                                urlService = siteInfo.UrlServicesPage + hrefValue;
                            else
                                urlService = siteInfo.UrlRoot + hrefValue;
                        }
                    }

                    if ((siteInfo.CityValue == Enums.Cities.Moscow && 
                        (Enums.MoscowSites)siteInfo.SiteValue == Enums.MoscowSites.helix_ru) ||
                        (siteInfo.CityValue == Enums.Cities.SaintPetersburg && 
                        (Enums.SaintPetersburgSites)siteInfo.SiteValue == Enums.SaintPetersburgSites.helix_ru)) {
                        string onClickValue = servicePage.Attributes["onclick"].Value;
                        onClickValue = onClickValue.Replace("$(location).attr('href', '", "").Replace(";');", "").Replace("');", "");
                        urlService = siteInfo.UrlRoot + onClickValue;
                    }

                    if (siteInfo.CityValue == Enums.Cities.Moscow &&
                        (Enums.MoscowSites)siteInfo.SiteValue == Enums.MoscowSites.medsi_ru)
                        if (urlService.Contains("#") && !urlService.EndsWith("/"))
                            continue;

                    if (siteInfo.CityValue == Enums.Cities.SaintPetersburg &&
                        (Enums.SaintPetersburgSites)siteInfo.SiteValue == Enums.SaintPetersburgSites.emcclinic_ru) {
                        if (string.IsNullOrEmpty(serviceName)) {
                            string xPathServiceName = servicePage.ParentNode.XPath + "//div[@class='n-services__text']";
                            HtmlNode htmlNodeServiceName = servicePage.SelectSingleNode(xPathServiceName);

                            if (htmlNodeServiceName != null)
                                serviceName = ClearString(htmlNodeServiceName.InnerText);
                        }
                    }

                    if (siteInfo.CityValue == Enums.Cities.Other &&
                        (Enums.OtherSites)siteInfo.SiteValue == Enums.OtherSites.nedorezov_mc_ru)
                        urlService = urlService.TrimEnd('/') + "/PageAll/1";

                    if (string.IsNullOrEmpty(urlService)) {
                        Console.WriteLine("string.IsNullOrEmpty(urlService)");
                        continue;
                    }

                    Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() { Name = serviceName, Link = urlService };

                    if (!string.IsNullOrEmpty(itemGroupNamePrefix))
                        itemServiceGroup.Name = itemGroupNamePrefix + " @ " + itemServiceGroup.Name;

                    if (siteInfo.CityValue == Enums.Cities.Krasnodar &&
                        (Enums.KrasnodarSites)siteInfo.SiteValue == Enums.KrasnodarSites.clinic23_ru)
                        if (itemServiceGroup.Link.Contains("50510"))
                            continue;

                    HtmlDocument docService = htmlAgility.GetDocument(urlService, siteInfo);
                    HtmlDocument docServicePrice = new HtmlDocument();

                    if (siteInfo.CityValue == Enums.Cities.Moscow &&
                        (Enums.MoscowSites)siteInfo.SiteValue == Enums.MoscowSites.zub_ru)
                        docServicePrice = htmlAgility.GetDocument(urlService + "price/", siteInfo);

                    if (docService == null) {
                        Console.WriteLine("docService is null");
                        continue;
                    }

                    switch (siteInfo.CityValue) {
                        case Enums.Cities.Moscow:
                            RegionParsers.ParseMoscow parseMoscow = this as RegionParsers.ParseMoscow;

                            switch ((Enums.MoscowSites)siteInfo.SiteValue) {
                                case Enums.MoscowSites.fdoctor_ru:
                                    parseMoscow.ParseSiteFdoctorRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.MoscowSites.familydoctor_ru:
                                case Enums.MoscowSites.familydoctor_ru_child:
                                    parseMoscow.ParseSiteFamilyDoctorRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.MoscowSites.smclinic_ru:
                                    if (serviceName.Equals("Диагностика")) {
                                        siteInfo.XPathServices = "//*[@id=\"content-in\"]/div[2]//a[@href]";
                                        ParseSiteWithLinksOnMainPage(docService);
                                        continue;
                                    } else if (serviceName.Equals("Лечение")) {
                                        siteInfo.XPathServices = "//*[@id=\"colleft\"]//a[@href]";
                                        ParseSiteWithLinksOnMainPage(docService);
                                        continue;
                                    }

                                    parseMoscow.ParseSiteSmClinicRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.MoscowSites.cmd_online_ru:
                                    parseMoscow.ParseSiteCmdOnlineRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.MoscowSites.helix_ru:
                                    ParseSiteHelixRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.MoscowSites.nrmed_ru:
                                case Enums.MoscowSites.nrmed_ru_child:
                                    parseMoscow.ParseSiteNrmedRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.MoscowSites.dentol_ru:
                                    parseMoscow.ParseSiteDentolRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.MoscowSites.zub_ru:
                                    parseMoscow.ParseSiteZubRu(docService, ref itemServiceGroup);
                                    parseMoscow.ParseSiteZubRu(docServicePrice, ref itemServiceGroup);
                                    break;
                                case Enums.MoscowSites.gemotest_ru:
                                    parseMoscow.ParseSiteGemotestRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.MoscowSites.kdl_ru:
                                    parseMoscow.ParseSiteKdlRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.MoscowSites.medsi_ru:
                                    parseMoscow.ParseSiteMedsiRu(docService, itemServiceGroup);
                                    break;
                                case Enums.MoscowSites.onclinic_ru:
                                    parseMoscow.ParseSiteOnClinic(docService, ref itemServiceGroup);
                                    break;
                                case Enums.MoscowSites.nrmedlab_ru:
                                    parseMoscow.ParseSiteNrLabRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.MoscowSites.sm_stomatology_ru:
                                    parseMoscow.ParseSiteSmStomatologyRu(docService, ref itemServiceGroup);
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case Enums.Cities.SaintPetersburg:
                            RegionParsers.ParseSpb parseSpb = this as RegionParsers.ParseSpb;

                            switch ((Enums.SaintPetersburgSites)siteInfo.SiteValue) {
                                case Enums.SaintPetersburgSites.helix_ru:
                                    ParseSiteHelixRu(docService, ref itemServiceGroup);
                                    break;
                                //case Enums.SaintPetersburgSites.evro_med_ru:
                                //	parseSpb.ParseSiteSpbEvroMedRu(docService, ref itemServiceGroup);
                                //	break;
                                case Enums.SaintPetersburgSites.baltzdrav_ru:
                                    parseSpb.ParseSiteSpbBaltzdravRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.SaintPetersburgSites.german_clinic:
                                case Enums.SaintPetersburgSites.german_dental:
                                    parseSpb.ParseSiteSpbGermanClinic(docService, ref itemServiceGroup);
                                    break;
                                case Enums.SaintPetersburgSites.medswiss_spb_ru:
                                    parseSpb.ParseSiteSpbMedswissSpbRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.SaintPetersburgSites.emcclinic_ru:
                                    parseSpb.ParseSiteSpbEmcclinicRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.SaintPetersburgSites.clinic_complex_ru:
                                    parseSpb.ParseSiteSpbClinicComplexRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.SaintPetersburgSites.dcenergo_ru:
                                    parseSpb.ParseSiteSpbDcenergoRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.SaintPetersburgSites.dcenergo_kids_ru:
                                    parseSpb.ParseSiteSpbDcenergoKidsRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.SaintPetersburgSites.starsclinic_ru:
                                    parseSpb.ParseSiteSpbStarsclinicRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.SaintPetersburgSites.clinica_blagodat_ru:
                                    parseSpb.ParseSiteSpbClinicaBlagodatRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.SaintPetersburgSites.med_vdk_ru:
                                    parseSpb.ParseSiteSpbMedVdkRu(docService, ref itemServiceGroup, isFirstCycle);
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case Enums.Cities.Sochi:
                            RegionParsers.ParseSochi parseSochi = this as RegionParsers.ParseSochi;

                            switch ((Enums.SochiSites)siteInfo.SiteValue) {
                                case Enums.SochiSites.clinic23_ru:
                                case Enums.SochiSites.clinic23_ru_lab:
                                    ParseSiteClinic23Ru(docService, ref itemServiceGroup);
                                    break;
                                case Enums.SochiSites._23doc_ru_doctors:
                                    parseSochi.ParseSite23docRuDoctors(docService, ref itemServiceGroup);
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case Enums.Cities.Kazan:
                            RegionParsers.ParseKazan parseKazan = this as RegionParsers.ParseKazan;

                            switch ((Enums.KazanSites)siteInfo.SiteValue) {
                                case Enums.KazanSites.ava_kazan_ru:
                                    parseKazan.ParseSiteKazanAvaKazanRu(docService, ref itemServiceGroup, isFirstCycle);
                                    break;
                                case Enums.KazanSites.mc_aybolit_ru:
                                    parseKazan.ParseSiteKazanMcAybolitRy(docService, ref itemServiceGroup);
                                    break;
                                case Enums.KazanSites.biomed_mc_ru:
                                    parseKazan.ParseSiteKazanBiomedMcRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.KazanSites.zdorovie7i_ru:
                                    parseKazan.ParseSiteKazanZdorovie7iRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.KazanSites.starclinic_ru:
                                    parseKazan.ParseSiteKazanStarclinicRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.KazanSites.love_dr_ru:
                                    parseKazan.ParseSiteKazanLoveDrRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.KazanSites.medexpert_kazan_ru:
                                    parseKazan.ParseSiteKazanMedexpertKazanRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.KazanSites.kazan_clinic_ru:
                                    parseKazan.ParseSiteKazanKazanClinicRu(docService, ref itemServiceGroup);
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case Enums.Cities.KamenskUralsky:
                            RegionParsers.ParseYekuk parseYekuk = this as RegionParsers.ParseYekuk;

                            switch ((Enums.KamenskUralskySites)siteInfo.SiteValue) {
                                case Enums.KamenskUralskySites.ruslabs_ru:
                                    parseYekuk.ParseSiteYekukRuslabsRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.KamenskUralskySites.mc_vd_ru:
                                    parseYekuk.ParseSiteYekukMcVdRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.KamenskUralskySites.immunoresurs_ru:
                                    parseYekuk.ParseSiteYekukImmunoresursRu(docService, ref itemServiceGroup);
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case Enums.Cities.Krasnodar:
                            RegionParsers.ParseKrd parseKrd = this as RegionParsers.ParseKrd;

                            switch ((Enums.KrasnodarSites)siteInfo.SiteValue) {
                                case Enums.KrasnodarSites.clinic23_ru:
                                case Enums.KrasnodarSites.clinic23_ru_lab:
                                    ParseSiteClinic23Ru(docService, ref itemServiceGroup);
                                    break;
                                case Enums.KrasnodarSites.poly_clinic_ru:
                                    parseKrd.ParseSiteKrdPolyClinicRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.KrasnodarSites.clinica_nazdorovie_ru:
                                    parseKrd.ParseSiteKrdClinicaNazdorovieRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.KrasnodarSites.clinica_nazdorovie_ru_lab:
                                    parseKrd.ParseSiteKrdClinicaNazdorovieRuLab(docService, ref itemServiceGroup);
                                    break;
                                case Enums.KrasnodarSites.vrukah_com:
                                    parseKrd.ParseSiteKrdVrukahCom(docService, ref itemServiceGroup);
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case Enums.Cities.Ufa:
                            RegionParsers.ParseUfa parseUfa = this as RegionParsers.ParseUfa;

                            switch ((Enums.UfaSites)siteInfo.SiteValue) {
                                case Enums.UfaSites.promedicina_clinic:
                                    parseUfa.ParseSiteUfaPromedicinaClinic(docService, ref itemServiceGroup);
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case Enums.Cities.Other:
                            RegionParsers.ParseOther parseOther = this as RegionParsers.ParseOther;

                            switch ((Enums.OtherSites)siteInfo.SiteValue) {
                                case Enums.OtherSites.kovrov_clinicalcenter_ru:
                                    parseOther.ParseSiteKovrovClinicalcenterRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.OtherSites.nedorezov_mc_ru:
                                    parseOther.ParseSiteNedorezovMcRu(docService, ref itemServiceGroup);
                                    break;
                                case Enums.OtherSites.nedorezov_prom_metall_kz:
                                    parseOther.ParseSiteNedorezovPromMetallKz(docService, ref itemServiceGroup);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }

                    if (itemServiceGroup.ServiceItems.Count == 0) {
                        backgroundWorker.ReportProgress((int)progressCurrent, App.errorPrefix + "Услуг не обнаружено, пропуск: " + itemServiceGroup.Name);
                        continue;
                    }

                    siteInfo.ServiceGroupItems.Add(itemServiceGroup);
                } catch (Exception e) {
                    backgroundWorker.ReportProgress((int)progressCurrent, e.Message);
                    Console.WriteLine("Exception: " + e.Message +
                        Environment.NewLine + e.StackTrace);
                }
            }

            Console.WriteLine("completed");
        }
        
        public void ParseSiteInvitroRU(HtmlDocument docServices) {
            siteInfo.ServiceGroupItems.Add(new Items.ServiceGroup() { Link = siteInfo.UrlServicesPage });
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);

            if (nodeCollectionServices != null) {
                foreach (HtmlNode node in nodeCollectionServices) {
                    HtmlNode nodeName = node.SelectSingleNode(node.XPath + "//a");
                    if (nodeName == null)
                        continue;

                    Items.Service itemService = new Items.Service() {
                        Name = ClearString(nodeName.InnerText),
                        Price = ClearString(node.Attributes["data-prices"].Value)
                    };

                    siteInfo.ServiceGroupItems[0].ServiceItems.Add(itemService);


                    //HtmlNodeCollection nodeCollectionTh = node.SelectNodes("tr/th");

                    //if (nodeCollectionTh == null) {
                    //    Console.WriteLine("nodeCollectionTh == null");
                    //    continue;
                    //}

                    //string serviceGroupName = ClearString(nodeCollectionTh.First().InnerText);

                    //Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
                    //    Name = serviceGroupName,
                    //    Link = siteInfo.UrlServicesPage
                    //};

                    //List<Items.Service> serviceItems = ReadTrNodes(node, 1, 2);
                    //itemServiceGroup.ServiceItems.AddRange(serviceItems);
                    //siteInfo.ServiceGroupItems.Add(itemServiceGroup);
                }
            }

            //nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, "//div[@class='show-block-wrap']");
            //if (nodeCollectionServices != null) {

            //}
        }
        
        public void ParseSiteHelixRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPath = "//div[@class='Catalog-Container-Item']";
            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPath);

            if (nodeCollectionService == null) {
                Console.WriteLine("nodeCollectionService is null");
                return;
            }

            string xPathName = "div[1]/div[1]/b";
            string xPathPrice = "div[3]/span[1]/b";

            foreach (HtmlNode node in nodeCollectionService) {
                try {
                    HtmlNodeCollection nodeName = node.SelectNodes(xPathName);
                    if (nodeName != null) {
                        string name = nodeName.First().InnerText;

                        HtmlNodeCollection nodePrice = node.SelectNodes(xPathPrice);
                        string price = nodePrice.First().InnerText;

                        Items.Service itemService = new Items.Service() {
                            Name = name,
                            Price = price
                        };

                        itemServiceGroup.ServiceItems.Add(itemService);
                    }
                } catch (Exception e) {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }
        }
        
        public void ParseSiteClinic23Ru(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            //if (siteInfo.ServiceGroupItems.Select(l => l.Link).ToList().Contains(itemServiceGroup.Link) ||
            //	itemServiceGroup.Link.Equals("https://www.clinic23.ru/khirurgiya/item/ambulatornaya-hirurgiya"))
            //	return;

            //string xPathLinksBlocks = "//div[@class='b-content__list']//div[@class='iitem']//a[@class='b-content__item-title-link']";
            //HtmlNodeCollection nodeCollectionLinksBlocks = htmlAgility.GetNodeCollection(docService, xPathLinksBlocks);
            //if (nodeCollectionLinksBlocks != null) {
            //    siteInfo.XPathServices = xPathLinksBlocks;
            //    ParseSiteWithLinksOnMainPage(docService);
            //}

            //string xPathLinks = "//div[@class='description-full']//table[@class='categoriya']//a[@href]";
            //HtmlNodeCollection nodeCollectionLinks = _htmlAgility.GetNodeCollection(docService, xPathLinks);
            //if (nodeCollectionLinks != null) {
            //	siteInfo.XPathServices = xPathLinks;
            //	ParseSiteWithLinksOnMainPage(docService);
            //} else
            //	Console.WriteLine("nodeCollectionLinks == null");

            string xPathServiceGroup = "//div[@class='price_line']";
            HtmlNodeCollection nodeCollectionGroups = htmlAgility.GetNodeCollection(docService, xPathServiceGroup);

            foreach (HtmlNode nodeGroup in nodeCollectionGroups) {
                string xPathGroupName = "//a[starts-with(@class,'prince_button2')]";
                HtmlNode nodeGroupName = nodeGroup.SelectSingleNode(nodeGroup.XPath + xPathGroupName);

                if (nodeGroupName == null) {
                    Console.WriteLine("nodeGroupName == null");
                    continue;
                }

                string groupName = ClearString(nodeGroupName.InnerText);
                Items.ServiceGroup serviceGroupInner = new Items.ServiceGroup {
                    Name = itemServiceGroup.Name + " - " + groupName,
                    Link = itemServiceGroup.Link
                };

                string xPathTableTbodyDiag = "//table[@class='table table-diagnostic']//tbody";
                HtmlNodeCollection nodeCollectionTbodyDiag = nodeGroup.SelectNodes(nodeGroup.XPath + xPathTableTbodyDiag);
                if (nodeCollectionTbodyDiag != null) {
                    foreach (HtmlNode nodeTbody in nodeCollectionTbodyDiag) {
                        List<Items.Service> serviceItems = ReadTrNodes(nodeTbody);
                        if (serviceItems.Count > 0)
                            serviceGroupInner.ServiceItems.AddRange(serviceItems);
                    }
                }

                string xPathTableTbodyAnaliz = "//table[@class='table analiz-table ']/tbody";
                HtmlNodeCollection nodeCollectionTbodyAnaliz = nodeGroup.SelectNodes(nodeGroup.XPath + xPathTableTbodyAnaliz);
                if (nodeCollectionTbodyAnaliz != null) {
                    foreach (HtmlNode nodeTbody in nodeCollectionTbodyAnaliz) {
                        List<Items.Service> serviceItems = ReadTrNodes(nodeTbody, 0, 1);
                        if (serviceItems.Count > 0)
                            serviceGroupInner.ServiceItems.AddRange(serviceItems);
                    }
                }

                siteInfo.ServiceGroupItems.Add(serviceGroupInner);
            }


            //if (itemServiceGroup.ServiceItems.Count == 0)
            //    Console.WriteLine("itemServiceGroup.ServiceItems.Count == 0");



            //string xPathTbodies = "//main[@class='tm-content']//table[@class='uk-table uk-table-striped' or @class=' uk-table uk-table-striped']//tbody";
            //HtmlNodeCollection nodeCollectionTbodies = _htmlAgility.GetNodeCollection(docService, xPathTbodies);
            //if (nodeCollectionTbodies != null) {
            //	foreach (HtmlNode nodeTbody in nodeCollectionTbodies) {
            //		List<Items.Service> serviceItems = ReadTrNodes(nodeTbody);
            //		itemServiceGroup.ServiceItems.AddRange(serviceItems);
            //	}
            //} else {
            //	string xPathHiddenTables = "//div[@class='content wk-content clearfix']//tbody";
            //	HtmlNodeCollection nodeCollectionHiddenTables = _htmlAgility.GetNodeCollection(docService, xPathHiddenTables);
            //	if (nodeCollectionHiddenTables != null) {
            //		foreach (HtmlNode nodeTbody in nodeCollectionHiddenTables) {
            //			List<Items.Service> serviceItems = ReadTrNodes(nodeTbody);
            //			itemServiceGroup.ServiceItems.AddRange(serviceItems);
            //		}
            //	} else
            //		Console.WriteLine("nodeCollectionHiddenTables == null");
            //}
        }

        public List<Items.Service> ReadTrNodes(HtmlNode node, int nameOffset = 0, int priceOffset = 0) {
            List<Items.Service> items = new List<Items.Service>();
            HtmlNodeCollection nodeCollectionTr = node.SelectNodes("tr");
            if (nodeCollectionTr == null) {
                Console.WriteLine("nodeCollectionTr is null, node: " + node.InnerText);
                return items;
            }

            foreach (HtmlNode tr in nodeCollectionTr) {
                try {
                    HtmlNodeCollection nodeTd = tr.SelectNodes("td");
                    if (nodeTd == null) {
                        Console.WriteLine("nodeTd == null");
                        continue;
                    }

                    //if (siteInfo.Name == Enums.SaintPetersburgSites.evro_med_ru) {
                    //	HtmlNodeCollection nodeCollectionPName = nodeTd[nameOffset].SelectNodes("p");
                    //	HtmlNodeCollection nodeCollectionPPrice = nodeTd[1 + priceOffset].SelectNodes("p");

                    //	if (nodeCollectionPName != null) {
                    //		string rootName = string.Empty;
                    //		int pNameOffset = 0;
                    //		int pPriceOffset = 0;

                    //		if (ClearString(nodeCollectionPName.Last().InnerText).StartsWith("-")) {
                    //			rootName = ClearString(nodeCollectionPName.First().InnerText);
                    //			pNameOffset = 1;
                    //			pPriceOffset = 1;
                    //		}

                    //		for (int i = pNameOffset; i < nodeCollectionPName.Count; i++) {
                    //			string currentName = ClearString(nodeCollectionPName[i].InnerText);
                    //			string nameInner = string.IsNullOrEmpty(rootName) ? currentName : rootName + " " + currentName;
                    //			string priceInner = ClearString(nodeCollectionPPrice[pPriceOffset + i].InnerText);

                    //			items.Add(new Items.Service() {
                    //				Name = nameInner,
                    //				Price = priceInner
                    //			});
                    //		}

                    //		continue;
                    //	}
                    //}

                    string nameRaw = string.Empty;
                    string priceRaw = string.Empty;

                    if (siteInfo.CityValue == Enums.Cities.KamenskUralsky &&
                        (Enums.KamenskUralskySites)siteInfo.SiteValue == Enums.KamenskUralskySites.ruslabs_ru) {
                        if (nodeTd.Count == 3) {
                            nameOffset = 0;
                            priceOffset = 0;
                        } else {
                            nameOffset = 1;
                            priceOffset = 2;
                        }

                    } else if (siteInfo.CityValue == Enums.Cities.Kazan) {
                        if ((Enums.KazanSites)siteInfo.SiteValue == Enums.KazanSites.mc_aybolit_ru) {
                            if (nodeTd.Count == 2) {
                                nameOffset = 0;
                                priceOffset = 0;
                            }

                        } else if ((Enums.KazanSites)siteInfo.SiteValue == Enums.KazanSites.biomed_mc_ru) {
                            if (nodeTd.Count == 3)
                                priceOffset = 1;

                        } else if ((Enums.KazanSites)siteInfo.SiteValue == Enums.KazanSites.starclinic_ru) {
                            if (nodeTd.Count == 3) {
                                nameOffset = 1;
                                priceOffset = 1;
                            }
                        }

                    } else if (siteInfo.CityValue == Enums.Cities.Moscow &&
                        (Enums.MoscowSites)siteInfo.SiteValue == Enums.MoscowSites.onclinic_ru) {
                        if (nodeTd.Count == 3) {
                            nameOffset = 1;
                            priceOffset = 1;
                        }

                    } else if (siteInfo.CityValue == Enums.Cities.Sochi) {
                        Enums.SochiSites sochiSite = (Enums.SochiSites)siteInfo.SiteValue;

                        if (sochiSite == Enums.SochiSites._23doc_ru_main_price ||
                            sochiSite == Enums.SochiSites._23doc_ru_doctors ||
                            sochiSite == Enums.SochiSites._23doc_ru_lab) {
                            if (nodeTd.Count == 3)
                                priceOffset = 1;
                            else if (nodeTd.Count == 4) {
                                nameOffset = 1;
                                priceOffset = 1;
                            }
                        }
                    }

                    if (nodeTd.Count > 0 + nameOffset) {
                        nameRaw = nodeTd[0 + nameOffset].InnerText;
                    }

                    if (nodeTd.Count > 1 + priceOffset) {
                        if (siteInfo.CityValue == Enums.Cities.Moscow &&
                            (Enums.MoscowSites)siteInfo.SiteValue == Enums.MoscowSites.cmd_online_ru) {
                            priceRaw = nodeTd[1 + priceOffset].ChildNodes[3].InnerText;
                        } else
                            priceRaw = nodeTd[1 + priceOffset].InnerText;
                    }

                    if (siteInfo.CityValue == Enums.Cities.KamenskUralsky &&
                        (Enums.KamenskUralskySites)siteInfo.SiteValue == Enums.KamenskUralskySites.immunoresurs_ru) {
                        if (nameRaw.Contains("\n") && priceRaw.Contains("\n")) {
                            string[] names = nameRaw.Split(new string[] { "\n" }, StringSplitOptions.None);
                            string[] prices = priceRaw.Split(new string[] { "\n" }, StringSplitOptions.None);

                            for (int i = 0; i < names.Length; i++) {
                                Items.Service itemServiceInner = new Items.Service() {
                                    Name = ClearString(names[i]),
                                    Price = ClearString(prices[i])
                                };

                                items.Add(itemServiceInner);
                            }

                            continue;
                        }

                    } else if (siteInfo.CityValue == Enums.Cities.Kazan &&
                        (Enums.KazanSites)siteInfo.SiteValue == Enums.KazanSites.biomed_mc_ru) {
                        if (!ClearString(nameRaw).Equals("Гормон роста СТГ (ИХА)") &&
                            !ClearString(nameRaw).Equals("Соматомедин С (ИФР-I)(инсулин-подобный фактор")) {

                            if (priceOffset == 1 && !priceRaw.Contains("руб"))
                                priceRaw = nodeTd[1].InnerText;
                        }

                    } else if (siteInfo.CityValue == Enums.Cities.Krasnodar &&
                        (Enums.KrasnodarSites)siteInfo.SiteValue == Enums.KrasnodarSites.clinica_nazdorovie_ru_lab) {
                        HtmlNode nodeAnalizRulez = nodeTd[0 + nameOffset].SelectSingleNode(nodeTd[0 + nameOffset].XPath + "//div[@class='analiz-rulez']");
                        if (nodeAnalizRulez != null && !string.IsNullOrEmpty(nodeAnalizRulez.InnerText))
                            nameRaw = nameRaw.Replace(nodeAnalizRulez.InnerText, "");
                    }

                    string name = ClearString(nameRaw);
                    string price = ClearString(priceRaw);

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(price))
                        continue;

                    if (siteInfo.CityValue == Enums.Cities.Krasnodar &&
                        (Enums.KrasnodarSites)siteInfo.SiteValue == Enums.KrasnodarSites.clinic23_ru &&
                        price.EndsWith(",0"))
                        price = price.Replace(",0", "");

                    Items.Service itemService = new Items.Service() {
                        Name = name,
                        Price = price
                    };

                    items.Add(itemService);
                } catch (Exception e) {
                    Console.WriteLine("ReadTrNodes: " + e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            return items;
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
                { "&#160;", "" },
                { "&#215;", "x" }
            };

            foreach (KeyValuePair<string, string> pair in toReplace)
                initial = initial.Replace(pair.Key, pair.Value);

            initial = initial.TrimStart(' ').TrimEnd(' ');

            return initial;
        }
    }
}
