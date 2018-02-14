using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceListLoader {
	class SiteParser {
		private HtmlAgility _htmlAgility = new HtmlAgility();
		private BackgroundWorker _backgroundWorker;

		private string _urlRoot;
		private string _urlServices;
		private string _companyName;
		private string _xPathServices;
		private Sites _selectedSite;

		private double progressCurrent = 0;
		private double progressTo = 100;

		public enum Sites {
			fdoctor_ru,
			familydoctor_ru,
			familydoctor_ru_child,
			alfazdrav_ru,
			nrmed_ru,
			nrlab_ru,
			onclinic_ru,
			smclinic_ru,
			smdoctor_ru,
			invitro_ru, //!!!work only with offline html
			cmd_online_ru,
			helix_ru,
			mrt24_ru,
			dentol_ru,
			zub_ru,
			vse_svoi_ru,
			novostom_ru,
			masterdent_ru,
			gemotest_ru,
			kdllab_ru,
			medsi_ru,
			sm_stomatology_ru,
			smclinic_ru_lab
		}

		public SiteParser(BackgroundWorker backgroundWorker) {
			_backgroundWorker = backgroundWorker;
		}

		public void ParseSelectedSites() {
			_selectedSite = Sites.smdoctor_ru;

			switch (_selectedSite) {
				case Sites.fdoctor_ru:
					_urlRoot = "https://www.fdoctor.ru";
					_urlServices = _urlRoot + "/services";
					_companyName = "ЗАО Сеть поликлиник \"Семейный доктор\"";
					_xPathServices = "/html[1]/body[1]/div[2]/div[2]/section[1]/div[1]/div[3]/div[1]/div[2]//a[@href]";
					break;
				case Sites.familydoctor_ru:
					_urlRoot = "http://www.familydoctor.ru";
					_urlServices = _urlRoot + "/prices";
					_companyName = "ООО \"Медицинская клиника \"Семейный доктор\"";
					_xPathServices = "//*[@id=\"content\"]/article/div[4]//a[@href]";
					break;
				case Sites.familydoctor_ru_child:
					_urlRoot = "http://www.familydoctor.ru";
					_urlServices = _urlRoot + "/prices/child/";
					_companyName = "ООО \"Медицинская клиника \"Семейный доктор\"";
					_xPathServices = "//*[@id=\"content\"]/article/div[4]//a[@href]";
					break;
				case Sites.alfazdrav_ru:
					_urlRoot = @"C:\Users\nn-admin\Desktop";
					_urlServices = _urlRoot + @"\Прейскурант _ Альфа - Центр Здоровья.html";
					_companyName = "ООО «МедАС» — «Альфа-Центр Здоровья»";
					_xPathServices = "/html/body/section/section/div/div/ul[1]/li";
					break;
				case Sites.nrmed_ru:
					_urlRoot = "http://www.nrmed.ru";
					_urlServices = _urlRoot + "/rus/ourserv/prices/";
					_companyName = "ООО \"НИАРМЕДИК ПЛЮС\"";
					_xPathServices = "//*[@id=\"srv_list_wrap\"]//a[@href]";
					break;
				case Sites.nrlab_ru:
					_urlRoot = "http://www.nrlab.ru";
					_urlServices = _urlRoot + "/prices/groups/";
					_companyName = "Лаборатория Ниармедик";
					_xPathServices = "//div[@class=\"spis_analiz\"]//a[@href]";
					break;
				case Sites.onclinic_ru:
					_urlRoot = "https://www.onclinic.ru";
					_urlServices = _urlRoot + "/all/";
					_companyName = "ООО \"Он Клиник Геоконик\"";
					_xPathServices = "//*[@id=\"center\"]/div//a[@href]";
					break;
				case Sites.smclinic_ru:
					_urlRoot = "http://www.smclinic.ru";
					_urlServices = _urlRoot + "/doctors/";
					_companyName = "ООО «СМ-Клиника»";
					_xPathServices = "//div[@id=\"content\"]//a[@href]";
					break;
				case Sites.smdoctor_ru:
					_urlRoot = "http://www.smdoctor.ru";
					_urlServices = _urlRoot + "/about/price/";
					_companyName = "ООО «СМ-Доктор»";
					_xPathServices = "//div[contains(@class, 'tab-price-panel')]";
					break;
				case Sites.invitro_ru:
					_urlRoot = "https://www.invitro.ru";
					_urlServices = _urlRoot + "/analizes/for-doctors/"; //not working, need to be downloaded manual!!!
					_urlServices = @"C:\Users\nn-admin\Desktop\Сдать анализы в Москве - цены на анализы в медицинской лаборатории Инвитро.html";
					_companyName = "ООО «ИНВИТРО»";
					_xPathServices = "/html/body/div[4]/div[2]/div[4]/div/div[1]/table/tbody";
					break;
				case Sites.cmd_online_ru:
					_urlRoot = "https://www.cmd-online.ru";
					_urlServices = _urlRoot + "/analizy-i-tseny-po-gruppam/kompleksnyje-programmy-laboratornyh-issledovanij_323/";
					_companyName = "ФБУН ЦНИИ Эпидемиологии Роспотребнадзора";
					_xPathServices = "//*[@id=\"analyzes_and_rates\"]/div[1]//a[@href]";
					break;
				case Sites.helix_ru:
					_urlRoot = "https://helix.ru";
					_urlServices = _urlRoot + "/catalog";
					_companyName = "ООО «НПФ «ХЕЛИКС»";
					_xPathServices = "/html/body/div[1]/div[6]/div[2]/div[1]//a[@href]";
					break;
				case Sites.mrt24_ru:
					_urlRoot = "http://mrt24.ru";
					_urlServices = _urlRoot + "/services/";
					_urlServices = @"C:\Users\nn-admin\Desktop\Цены на услуги МРТ в Москве и Московской области в центрах МРТ24.html";
					_companyName = "ООО \"ДЛ Медика\"";
					_xPathServices = "//div[contains(@class,'_id_')]";
					break;
				case Sites.dentol_ru:
					_urlRoot = "https://dentol.ru";
					_urlServices = _urlRoot + "/uslugi/";
					_companyName = "ООО “Сеть Семейных Медицинских Центров”";
					_xPathServices = "/html/body/div[3]/div[2]/div[1]/div[7]//a[@href]";
					break;
				case Sites.zub_ru:
					_urlRoot = "https://zub.ru";
					_urlServices = _urlRoot + "/uslugi/";
					_companyName = "ООО \"Зуб.ру\"";
					_xPathServices = "//ul[@class='sn-lm']//a[@href]";
					break;
				case Sites.vse_svoi_ru:
					_urlRoot = "https://vse-svoi.ru";
					_urlServices = _urlRoot + "/msk/ceny/";
					_companyName = "ООО \"ВСЕ СВОИ\"";
					_xPathServices = "//div[@class='price-list-page']";
					break;
				case Sites.novostom_ru:
					_urlRoot = "http://www.novostom.ru";
					_urlServices = _urlRoot + "/tceny/ceny-v-moskvy/";
					_companyName = "ООО СЦНТ \"НОВОСТОМ\"";
					_xPathServices = "//table[@class='price-tbl']";
					break;
				case Sites.masterdent_ru:
					_urlRoot = "http://masterdent.ru";
					_urlServices = _urlRoot + "/prais.html";
					_companyName = "Мастердент";
					_xPathServices = "//div[@class='podrazdel']";
					break;
				case Sites.gemotest_ru:
					_urlRoot = "https://www.gemotest.ru";
					_urlServices = _urlRoot + "/catalog/po-laboratornym-napravleniyam/top-250-populyarnykh-uslug/";
					_companyName = "ООО \"Лаборатория Гемотест\"";
					_xPathServices = "//*[@id=\"d-content\"]/div/aside/nav/div[2]/ul//a[@href]";
					break;
				case Sites.kdllab_ru:
					_urlRoot = "https://kdllab.ru";
					_urlServices = _urlRoot + "/analizy-i-tseny";
					_companyName = "ООО «КДЛ ДОМОДЕДОВО-ТЕСТ»";
					_xPathServices = "/html/body/div[5]/div/div[3]/div[2]//a[@href]";
					break;
				case Sites.medsi_ru:
					_urlRoot = "https://medsi.ru";
					_urlServices = _urlRoot + "/services/";
					_companyName = "АО \"Группа компаний МЕДСИ\"";
					_xPathServices = "/html/body/div[5]/div[2]/div/div/div[2]//a[@href]";
					break;
				case Sites.sm_stomatology_ru:
					_urlRoot = "http://www.sm-stomatology.ru";
					_urlServices = _urlRoot + "/services/";
					_companyName = "СМ-Стоматология";
					_xPathServices = "//div[@class='b-aside-menu']//a[@href]";
					break;
				case Sites.smclinic_ru_lab:
					_urlRoot = "http://www.smclinic.ru";
					_urlServices = _urlRoot + "/calc/";
					_companyName = "ООО «СМ-Клиника»";
					_xPathServices = "//div[@class='panel panel-default pull-left']";
					break;
				default:
					return;
			}

			_backgroundWorker.ReportProgress((int)progressCurrent, _companyName);
			_backgroundWorker.ReportProgress((int)progressCurrent, "Загрузка данных с сайта: " + _urlRoot);

			ItemSiteData itemSiteData = new ItemSiteData() { SiteAddress = _urlServices, CompanyName = _companyName };

			HtmlDocument docServices;

			bool isLocalFile = false;
			if (_selectedSite == Sites.invitro_ru ||
				_selectedSite == Sites.mrt24_ru ||
				_selectedSite == Sites.alfazdrav_ru)
				isLocalFile = true;
				
			docServices = _htmlAgility.GetDocument(_urlServices, isLocalFile);

			if (docServices == null) {
				_backgroundWorker.ReportProgress((int)progressCurrent, "Не удалось загрузить страницу: " + _urlServices);
				Console.WriteLine("docServices is null");
				return;
			}

			switch (_selectedSite) {
				case Sites.fdoctor_ru:
				case Sites.familydoctor_ru:
				case Sites.familydoctor_ru_child:
				case Sites.nrmed_ru:
				case Sites.smclinic_ru:
				case Sites.cmd_online_ru:
				case Sites.helix_ru:
				case Sites.dentol_ru:
				case Sites.zub_ru:
				case Sites.gemotest_ru:
				case Sites.kdllab_ru:
				case Sites.medsi_ru:
				case Sites.onclinic_ru:
				case Sites.nrlab_ru:
				case Sites.sm_stomatology_ru:
					ParseSitesWithLinksOnMainPage(docServices, ref itemSiteData);
					break;
				case Sites.alfazdrav_ru:
					ParseSiteAlfazdrav(docServices, ref itemSiteData);
					break;
				case Sites.invitro_ru:
					ParseSiteInvitroRU(docServices, ref itemSiteData);
					break;
				case Sites.mrt24_ru:
					ParseSiteMrt24Ru(docServices, ref itemSiteData);
					break;
				case Sites.vse_svoi_ru:
					ParseSiteVseSvoiRu(docServices, ref itemSiteData);
					break;
				case Sites.novostom_ru:
					ParseSiteNovostomRu(docServices, ref itemSiteData);
					break;
				case Sites.masterdent_ru:
					ParseSiteMasterdentRu(docServices, ref itemSiteData);
					break;
				case Sites.smdoctor_ru:
					ParseSiteSmDoctorRu(docServices, ref itemSiteData);
					break;
				case Sites.smclinic_ru_lab:
					ParseSiteSmClinicRuLab(docServices, ref itemSiteData);
					break;
				default:
					break;
			}

			if (itemSiteData.ServiceGroupItems.Count == 0) {
				_backgroundWorker.ReportProgress((int)progressCurrent, "Не удалось найти ни одной группы услуг");
				return;
			}

			string resultFile = NpoiExcel.WriteItemSiteDataToExcel(itemSiteData, _backgroundWorker, progressCurrent, progressTo);
		}

		private void ParseSiteSmClinicRuLab(HtmlDocument docServices, ref ItemSiteData itemSiteData) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, _xPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			foreach (HtmlNode node in nodeCollectionServices) {
				string blockName = string.Empty;
				HtmlNode nodeBlockName = node.SelectSingleNode(node.XPath + "/div[@class='panel-heading']");
				if (nodeBlockName != null)
					blockName = ClearString(nodeBlockName.InnerText);

				ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
					Name = blockName,
					Link = _urlServices
				};

				HtmlNodeCollection nodeCollectionItems = node.SelectNodes(node.XPath + "//div[@class='input-holder']");
				if (nodeCollectionItems == null) {
					Console.WriteLine("nodeCollectionItems == null");
					continue;
				}

				foreach (HtmlNode nodeItem in nodeCollectionItems) {
					try {
						string itemCost = ClearString(nodeItem.ChildNodes[1].Attributes["data-price"].Value);
						string itemName = ClearString(nodeItem.ChildNodes[2].InnerText);
						itemServiceGroup.ServiceItems.Add(new ItemService() {
							Name = itemName,
							Price = itemCost
						});
					} catch (Exception e) {
						Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
					}
				}

				if (itemServiceGroup.ServiceItems.Count == 0)
					continue;

				itemSiteData.ServiceGroupItems.Add(itemServiceGroup);
			}
		}

		private void ParseSiteSmDoctorRu(HtmlDocument docServices, ref ItemSiteData itemSiteData) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, _xPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			foreach (HtmlNode nodeGroup in nodeCollectionServices) {
				HtmlNodeCollection nodeCollectionLi = nodeGroup.SelectNodes(nodeGroup.XPath + "//li[@class='section-item']");

				if (nodeCollectionLi == null) {
					Console.WriteLine("nodeCollectionLi == null");
					continue;
				}

				foreach (HtmlNode nodeLi in nodeCollectionLi) {
					try {
						HtmlNode nodeSectionName = nodeLi.SelectSingleNode(nodeLi.XPath + "/span");

						if (nodeSectionName == null) {
							Console.WriteLine("nodeSectionName == null");
							continue;
						}

						string sectionName = ClearString(nodeSectionName.InnerText);
						ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
							Name = sectionName,
							Link = _urlServices
						};

						HtmlNode nodeTableBody = nodeLi.SelectSingleNode(nodeLi.XPath + "//table[@class='price-list']/tbody");
						if (nodeTableBody == null) {
							Console.WriteLine("nodeTableBody == null");
							continue;
						}

						List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeTableBody);
						itemServiceGroup.ServiceItems.AddRange(serviceItems);

						if (itemServiceGroup.ServiceItems.Count == 0)
							continue;

						itemSiteData.ServiceGroupItems.Add(itemServiceGroup);
					} catch (Exception e) {
						Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
					}
				}

				HtmlNode nodeSingleTable = nodeGroup.SelectSingleNode(nodeGroup.XPath + "/table[@class='price-list']/tbody");
				if (nodeSingleTable != null) {
					ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
						Name = "Детская хирургия",
						Link = _urlServices
					};

					HtmlNode nodeTableBody = nodeGroup.SelectSingleNode(nodeGroup.XPath + "//table[@class='price-list']/tbody");
					if (nodeTableBody == null) {
						Console.WriteLine("nodeTableBody == null");
						continue;
					}

					try {
						List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeTableBody);
						itemServiceGroup.ServiceItems.AddRange(serviceItems);
					} catch (Exception e) {
						Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
					}

					if (itemServiceGroup.ServiceItems.Count == 0)
						continue;

					itemSiteData.ServiceGroupItems.Add(itemServiceGroup);
				}
			}
		}

		private void ParseSiteMasterdentRu(HtmlDocument docServices, ref ItemSiteData itemSiteData) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, _xPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			foreach (HtmlNode node in nodeCollectionServices) {
				HtmlNode htmlNodeGroup = node.SelectSingleNode("p");
				if (htmlNodeGroup == null) {
					Console.WriteLine("htmlNodeGroup == null");
					continue;
				}

				string groupName = htmlNodeGroup.InnerText;
				ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
					Name = ClearString(groupName),
					Link = _urlServices
				};

				HtmlNode htmlNodeTable = node.SelectSingleNode("div[1]/table");
				if (htmlNodeTable == null) {
					Console.WriteLine("htmlNodeTable == null");
					continue;
				}

				List<ItemService> serviceItems = ReadTrNodesFdoctorRu(htmlNodeTable.SelectSingleNode("tbody"), 1, 1);
				itemServiceGroup.ServiceItems = serviceItems;

				itemSiteData.ServiceGroupItems.Add(itemServiceGroup);
			}
		}

		private void ParseSiteNovostomRu(HtmlDocument docServices, ref ItemSiteData itemSiteData) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, _xPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			foreach (HtmlNode node in nodeCollectionServices) {
				ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
					Link = _urlServices
				};

				HtmlNode htmlNodeCaption = node.SelectSingleNode("caption");
				if (htmlNodeCaption != null) {
					string groupName = htmlNodeCaption.InnerText;
					itemServiceGroup.Name = groupName;
				}

				HtmlNode htmlNodeTbody = node.SelectSingleNode("tbody");
				if (htmlNodeTbody == null) {
					Console.WriteLine("htmlNodeTbody == null");
					continue;
				}

				HtmlNodeCollection htmlNodeCollectionTr = htmlNodeTbody.SelectNodes("tr");
				if (htmlNodeCollectionTr == null) {
					Console.WriteLine("htmlNodeCollectionTr == null");
					continue;
				}

				foreach (HtmlNode nodeTr in htmlNodeCollectionTr) {
					HtmlNodeCollection htmlNodeCollectionTd = nodeTr.SelectNodes("td");
					if (htmlNodeCollectionTd == null) {
						Console.WriteLine("htmlNodeCollectionTd == null");
						continue;
					}
					
					if (htmlNodeCollectionTd.Count >= 1) {
						string name = htmlNodeCollectionTd[0].InnerText;

						ItemService itemService = new ItemService() {
							Name = ClearString(name)
						};

						if (htmlNodeCollectionTd.Count >= 2) {
							string price = htmlNodeCollectionTd[1].InnerText;
							itemService.Price = ClearString(price);
						}

						itemServiceGroup.ServiceItems.Add(itemService);
					}
				}

				itemSiteData.ServiceGroupItems.Add(itemServiceGroup);
			}
		}

		private void ParseSiteVseSvoiRu(HtmlDocument docServices, ref ItemSiteData itemSiteData) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, _xPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			nodeCollectionServices = nodeCollectionServices[0].ChildNodes; 
			for (int i = 0; i < nodeCollectionServices.Count; i++) {
				try {
					HtmlNode nodeHeader = nodeCollectionServices[i];
					if (!nodeHeader.Name.Equals("h3") &&
						!nodeHeader.Name.Equals("noindex"))
						continue;

					ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
						Name = nodeHeader.InnerText,
						Link = _urlServices
					};

					for (int x = i + 1; x < nodeCollectionServices.Count; x++) {
						HtmlNode nodeTable = nodeCollectionServices[x];

						if (nodeTable.Name.Equals("h3") ||
							nodeTable.Name.Equals("noindex")) {
							i = x - 1;
							break;
						}

						if (!nodeTable.Name.Equals("table"))
							continue;

						HtmlNodeCollection nodeServices = nodeTable.ChildNodes[1].SelectNodes("tr");
						if (nodeServices == null) {
							Console.WriteLine("nodeServices == null");
							continue;
						}

						foreach (HtmlNode nodeService in nodeServices) {
							try {
								string name = nodeService.ChildNodes[0].InnerText;
								string price = nodeService.ChildNodes[1].FirstChild.FirstChild.InnerText;
								ItemService itemService = new ItemService() {
									Name = name,
									Price = ClearString(price)
								};

								itemServiceGroup.ServiceItems.Add(itemService);
							} catch (Exception ex) {
								Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
							}
						}
					}

					itemSiteData.ServiceGroupItems.Add(itemServiceGroup);
				} catch (Exception e) {
					Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				}
			}
		}

		private void ParseSiteMrt24Ru(HtmlDocument docServices, ref ItemSiteData itemSiteData) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, _xPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			foreach (HtmlNode node in nodeCollectionServices) {
				try {
					HtmlNode nodeServiceName = node.SelectSingleNode("div[1]/div[1]");
					string serviceName = nodeServiceName.InnerText;
					_backgroundWorker.ReportProgress((int)progressCurrent, serviceName);
					string nodeClassName = node.Attributes["class"].Value;

					ItemServiceGroup itemServiceGroup = new ItemServiceGroup() { Name = serviceName };

					HtmlNodeCollection nodeServices = node.SelectNodes("//div[@class='" + nodeClassName + "']//div[@class='group_container']");
					if (nodeServices == null) {
						Console.WriteLine("nodeServices == null");
						continue;
					}

					foreach (HtmlNode nodeService in nodeServices) {
						HtmlNode nodeItemServiceName = nodeService.SelectSingleNode("div[1]/div[1]");
						string itemServiceName = nodeItemServiceName.InnerText;
						_backgroundWorker.ReportProgress((int)progressCurrent, itemServiceName);

						HtmlNodeCollection nodesCol2InnerCol1 = nodeService.SelectNodes("div[2]/div[1]/div[1]/span");

						if (nodesCol2InnerCol1 != null) {
							string prefix = ", Закрытый контур, ";

							if (nodesCol2InnerCol1.Count >= 1) {
								string title = nodesCol2InnerCol1[0].ChildNodes[0].InnerText;
								string price = nodesCol2InnerCol1[0].ChildNodes[1].InnerText;
								string name = itemServiceName + prefix + title;
								ItemService itemService = new ItemService() {
									Name = name,
									Price = price
								};
								itemServiceGroup.ServiceItems.Add(itemService);
							}

							if (nodesCol2InnerCol1.Count >= 2) {
								string title = nodesCol2InnerCol1[1].ChildNodes[0].InnerText;
								string price = nodesCol2InnerCol1[1].ChildNodes[1].InnerText;
								string name = itemServiceName + prefix + title;
								ItemService itemService = new ItemService() {
									Name = name,
									Price = price
								};
								itemServiceGroup.ServiceItems.Add(itemService);
							}
						}
						
						HtmlNodeCollection nodesCol2InnerCol2 = nodeService.SelectNodes("div[2]/div[1]/div[2]/span");

						if (nodesCol2InnerCol2 != null) {
							string prefix = ", Открытый контур, ";

							if (nodesCol2InnerCol2.Count >= 1) {
								string title = nodesCol2InnerCol2[0].ChildNodes[0].InnerText;
								string price = nodesCol2InnerCol2[0].ChildNodes[1].InnerText;
								string name = itemServiceName + prefix + title;
								ItemService itemService = new ItemService() {
									Name = name,
									Price = price
								};
								itemServiceGroup.ServiceItems.Add(itemService);
							}

							if (nodesCol2InnerCol2.Count >= 2) {
								string title = nodesCol2InnerCol2[1].ChildNodes[0].InnerText;
								string price = nodesCol2InnerCol2[1].ChildNodes[1].InnerText;
								string name = itemServiceName + prefix + title;
								ItemService itemService = new ItemService() {
									Name = name,
									Price = price
								};
								itemServiceGroup.ServiceItems.Add(itemService);
							}
						}

						HtmlNodeCollection nodesCol2Col2InnerCol1 = nodeService.SelectNodes("div[2]/div[2]/div[1]/div[1]/span");

						if (nodesCol2Col2InnerCol1 != null) {
							if (nodesCol2Col2InnerCol1.Count >= 1) {
								string prefix = ", Закрытый контур, ";

								string title = nodesCol2Col2InnerCol1[0].ChildNodes[0].InnerText;
								string price = nodesCol2Col2InnerCol1[0].ChildNodes[1].InnerText;
								string name = itemServiceName + prefix + title;
								ItemService itemService = new ItemService() {
									Name = name,
									Price = price
								};
								itemServiceGroup.ServiceItems.Add(itemService);
							}

							if (nodesCol2Col2InnerCol1.Count >= 2) {
								string prefix = ", Открытый контур, ";
								string title = nodesCol2Col2InnerCol1[1].ChildNodes[0].InnerText;
								string price = nodesCol2Col2InnerCol1[1].ChildNodes[1].InnerText;
								string name = itemServiceName + prefix + title;
								ItemService itemService = new ItemService() {
									Name = name,
									Price = price
								};
								itemServiceGroup.ServiceItems.Add(itemService);
							}
						}
						
						HtmlNodeCollection nodesCol2Col2InnerCol2 = nodeService.SelectNodes("div[2]/div[2]/div[1]/div[2]/span");
						
						if (nodesCol2Col2InnerCol2 != null) {
							if (nodesCol2Col2InnerCol2.Count >= 1) {
								string prefix = ", Закрытый контур, ";

								string title = nodesCol2Col2InnerCol2[0].ChildNodes[0].InnerText;
								string price = nodesCol2Col2InnerCol2[0].ChildNodes[1].InnerText;
								string name = itemServiceName + prefix + title;
								ItemService itemService = new ItemService() {
									Name = name,
									Price = price
								};
								itemServiceGroup.ServiceItems.Add(itemService);
							}

							if (nodesCol2Col2InnerCol2.Count >= 2) {
								string prefix = ", Открытый контур, ";
								string title = nodesCol2Col2InnerCol2[1].ChildNodes[0].InnerText;
								string price = nodesCol2Col2InnerCol2[1].ChildNodes[1].InnerText;
								string name = itemServiceName + prefix + title;
								ItemService itemService = new ItemService() {
									Name = name,
									Price = price
								};
								itemServiceGroup.ServiceItems.Add(itemService);
							}
						}
					}

					itemSiteData.ServiceGroupItems.Add(itemServiceGroup);
				} catch (Exception e) {
					Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				}
			}
		}

		private void ParseSiteInvitroRU(HtmlDocument docServices, ref ItemSiteData itemSiteData) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, _xPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			foreach (HtmlNode node in nodeCollectionServices) {
				HtmlNodeCollection nodeCollectionTh = node.SelectNodes("tr/th");

				if (nodeCollectionTh == null) {
					Console.WriteLine("nodeCollectionTh == null");
					continue;
				}

				string serviceGroupName = ClearString(nodeCollectionTh.First().InnerText);

				ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
					Name = serviceGroupName
				};
				List<ItemService> serviceItems = ReadTrNodesFdoctorRu(node, 1, 1);
				itemServiceGroup.ServiceItems = serviceItems;
				itemSiteData.ServiceGroupItems.Add(itemServiceGroup);
			}
		}

		private void ParseSiteAlfazdrav(HtmlDocument docServices, ref ItemSiteData itemSiteData) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, _xPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			foreach (HtmlNode node in nodeCollectionServices) {
				try {
					HtmlNode nodeHead = node.ChildNodes[1];
					string departmentName = nodeHead.InnerText;
					_backgroundWorker.ReportProgress((int)progressCurrent, departmentName);

					HtmlNodeCollection nodeCollectionGroups = node.ChildNodes[3].SelectNodes("li");
					if (nodeCollectionGroups == null) {
						Console.WriteLine("nodeCollectionGroups == null");
						continue;
					}

					foreach (HtmlNode nodeGroup in nodeCollectionGroups) {
						HtmlNode nodeServiceGroup = nodeGroup.ChildNodes[1];
						string groupName = nodeServiceGroup.InnerText;
						_backgroundWorker.ReportProgress((int)progressCurrent, groupName);

						ItemServiceGroup itemServiceGroup = new ItemServiceGroup() 
							{ Name = departmentName + ", " + groupName, Link = _urlServices };

						HtmlNodeCollection nodeTables = nodeGroup.SelectNodes(nodeGroup.XPath + "//table");

						if (nodeTables == null) {
							Console.WriteLine("nodeTables == null");
							continue;
						}

						foreach (HtmlNode nodeTable in nodeTables) {
							List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeTable.LastChild);

							if (serviceItems.Count == 0) {
								_backgroundWorker.ReportProgress((int)progressCurrent, "Услуг не обнаружено, пропуск");
								continue;
							}

							itemServiceGroup.ServiceItems.AddRange(serviceItems);
						}

						itemSiteData.ServiceGroupItems.Add(itemServiceGroup);
					}
				} catch (Exception e) {
					_backgroundWorker.ReportProgress((int)progressCurrent, e.Message + Environment.NewLine + e.StackTrace);
				}
			}
		}



		private void ParseSitesWithLinksOnMainPage(HtmlDocument docServices, ref ItemSiteData itemSiteData) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, _xPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			double progressStep = ((progressTo - progressCurrent) * 0.7) / nodeCollectionServices.Count;
			_backgroundWorker.ReportProgress((int)progressCurrent, "Загрузка прайс-листов для групп услуг");

			foreach (HtmlNode servicePage in nodeCollectionServices) {
				progressCurrent += progressStep;

				try {
					string serviceName = ClearString(servicePage.InnerText);
					_backgroundWorker.ReportProgress((int)progressCurrent, serviceName);
					Console.WriteLine("serviceName: " + serviceName);
					string hrefValue = servicePage.Attributes["href"].Value;

					if (!hrefValue.StartsWith("/"))
						hrefValue = "/" + hrefValue;

					string urlService = _urlRoot + hrefValue;

					if (_selectedSite == Sites.medsi_ru)
						if (urlService.Contains("#") && !urlService.EndsWith("/"))
							continue;

					ItemServiceGroup itemServiceGroup = new ItemServiceGroup() { Name = serviceName, Link = urlService };

					HtmlDocument docService = _htmlAgility.GetDocument(urlService);
					HtmlDocument docServicePrice = new HtmlDocument();

					if (_selectedSite == Sites.zub_ru)
						docServicePrice = _htmlAgility.GetDocument(urlService + "price/");

					if (docService == null) {
						Console.WriteLine("docService is null");
						continue;
					}

					switch (_selectedSite) {
						case Sites.fdoctor_ru:
							ParseSiteFdoctorRu(docService, ref itemServiceGroup);
							break;
						case Sites.familydoctor_ru:
						case Sites.familydoctor_ru_child:
							ParseSiteFamilyDoctorRu(docService, ref itemServiceGroup);
							break;
						case Sites.smclinic_ru:
							if (serviceName.Equals("Диагностика")) {
								_xPathServices = "//*[@id=\"content-in\"]/div[2]//a[@href]";
								ParseSitesWithLinksOnMainPage(docService, ref itemSiteData);
								continue;
							} else if (serviceName.Equals("Лечение")) {
								_xPathServices = "//*[@id=\"colleft\"]//a[@href]";
								ParseSitesWithLinksOnMainPage(docService, ref itemSiteData);
								continue;
							}

							ParseSiteSmClinicRu(docService, ref itemServiceGroup);
							break;
						case Sites.cmd_online_ru:
							ParseSiteCmdOnlineRu(docService, ref itemServiceGroup, ref itemSiteData);
							break;
						case Sites.helix_ru:
							ParseSiteHelixRu(docService, ref itemServiceGroup);
							break;
						case Sites.nrmed_ru:
							ParseSiteNrmedRu(docService, ref itemServiceGroup);
							break;
						case Sites.dentol_ru:
							ParseSiteDentolRu(docService, ref itemServiceGroup);
							break;
						case Sites.zub_ru:
							ParseSiteZubRu(docService, ref itemServiceGroup);
							ParseSiteZubRu(docServicePrice, ref itemServiceGroup);
							break;
						case Sites.gemotest_ru:
							ParseSiteGemotestRu(docService, ref itemServiceGroup);
							break;
						case Sites.kdllab_ru:
							ParseSiteKdlLabRu(docService, ref itemServiceGroup);
							break;
						case Sites.medsi_ru:
							ParseSiteMedsiRu(docService, ref itemSiteData, itemServiceGroup);
							break;
						case Sites.onclinic_ru:
							ParseSiteOnClinic(docService, ref itemServiceGroup, ref itemSiteData);
							break;
						case Sites.nrlab_ru:
							ParseSiteNrLabRu(docService, ref itemServiceGroup);
							break;
						case Sites.sm_stomatology_ru:
							ParseSiteSmStomatologyRu(docService, ref itemServiceGroup);
							break;
						default:
							break;
					}

					if (itemServiceGroup.ServiceItems.Count == 0) {
						_backgroundWorker.ReportProgress((int)progressCurrent, "Услуг не обнаружено, пропуск");
						continue;
					}

					itemSiteData.ServiceGroupItems.Add(itemServiceGroup);
				} catch (Exception e) {
					_backgroundWorker.ReportProgress((int)progressCurrent, e.Message);
					Console.WriteLine("Exception: " + e.Message +
						Environment.NewLine + servicePage.InnerText);
				}
			}

			Console.WriteLine("completed");
		}

		private void ParseSiteSmStomatologyRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathTable = "//table[@class='table b-table']/tbody";
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathTable);
			Console.WriteLine(itemServiceGroup.Name);

			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			foreach (HtmlNode node in nodeCollectionService) {
				List<ItemService> serviceItems = ReadTrNodesFdoctorRu(node);
				itemServiceGroup.ServiceItems.AddRange(serviceItems);
			}
		}

		private void ParseSiteNrLabRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathTable = "//table[contains(@class, 'tab_issledov tab_op_isl')]";
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathTable);

			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			foreach (HtmlNode node in nodeCollectionService) {
				List<ItemService> serviceItems = ReadTrNodesFdoctorRu(node, 1, 3);
				itemServiceGroup.ServiceItems.AddRange(serviceItems);
			}
		}

		private void ParseSiteOnClinic(HtmlDocument docServices, ref ItemServiceGroup itemServiceGroup, ref ItemSiteData itemSiteData, bool goDeep = true) {
			string xPathTable = "//div[@class='price-holder tbl-sm-font']";
			HtmlNodeCollection nodeCollectionPriceTable = _htmlAgility.GetNodeCollection(docServices, xPathTable);
			if (nodeCollectionPriceTable != null) {
				try {
					List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeCollectionPriceTable[0].ChildNodes[1].ChildNodes[1]);
					itemServiceGroup.ServiceItems = serviceItems;
				} catch (Exception e) {
					Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				}
			}

			if (!goDeep)
				return;

			string xPathLeftMenu = "//div[@id='left-menu']";
			HtmlNodeCollection nodeCollectionLeftMenu = _htmlAgility.GetNodeCollection(docServices, xPathLeftMenu);
			if (nodeCollectionLeftMenu != null) {
				HtmlNodeCollection nodeCollectionLinks = nodeCollectionLeftMenu[0].SelectNodes(xPathLeftMenu + "//a[@href]");
				if (nodeCollectionLinks == null) {
					Console.WriteLine("nodeCollectionLinks == null");
					return;
				}

				foreach (HtmlNode nodeLink in nodeCollectionLinks) {
					try {
						string serviceName = ClearString(nodeLink.InnerText);
						_backgroundWorker.ReportProgress((int)progressCurrent, serviceName);
						Console.WriteLine("serviceName: " + serviceName);
						string hrefValue = nodeLink.Attributes["href"].Value;
						string urlService = _urlRoot + hrefValue;

						ItemServiceGroup itemServiceGroupInner = new ItemServiceGroup() { Name = serviceName, Link = urlService };
						HtmlDocument docService = _htmlAgility.GetDocument(urlService);

						if (docService == null) {
							Console.WriteLine("docService == null");
							continue;
						}

						ParseSiteOnClinic(docService, ref itemServiceGroupInner, ref itemSiteData, false);

						itemSiteData.ServiceGroupItems.Add(itemServiceGroupInner);
					} catch (Exception e) {
						Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
					}
				}
			}
		}

		private void ParseSiteMedsiRu(HtmlDocument docService, ref ItemSiteData itemSiteData, ItemServiceGroup itemServiceGroupRoot) {
			string xPathClinicsId = "//*[@class='ui-select__nosearch js-pricelist-select']/option";
			HtmlNodeCollection htmlNodeId = docService.DocumentNode.SelectNodes(xPathClinicsId);

			if (htmlNodeId == null) {
				Console.WriteLine("htmlNodeId == null");
				return;
			}

			Dictionary<string, string> idValues = new Dictionary<string, string>();
			foreach (HtmlNode nodeId in htmlNodeId) {
				try {
					string key = nodeId.Attributes["value"].Value;
					string value = ClearString(nodeId.InnerText);
					idValues.Add(key, value);
				} catch (Exception e) {
					Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				}
			}
			
			string xPathTable = "//div[@class='js-pricelist-clinic']";
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathTable);

			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			foreach (HtmlNode nodeGroup in nodeCollectionService) {
				if (nodeGroup.ChildNodes.Count <= 1 ) {
					Console.WriteLine("nodeGroup.ChildNodes.Count <= 1");
					continue;
				}

				try {
					string id = nodeGroup.Attributes["data-id"].Value;
					string priceType = nodeGroup.Attributes["data-price-type-id"].Value;
					string clinicName = "ClinicNameUnknown";
					if (idValues.ContainsKey(id))
						clinicName = idValues[id];

					if (priceType.Equals("1"))
						clinicName += ", выходные";
					else if (priceType.Equals("2"))
						clinicName += ", будни";

					HtmlNodeCollection nodeCollectionRowPrice = nodeGroup.SelectNodes(nodeGroup.XPath + "//div[@class='table-row-price']");
					if (nodeCollectionRowPrice == null) {
						Console.WriteLine("nodeCollectionRowPrice == null");
						continue;
					}

					ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
						Name = itemServiceGroupRoot.Name + ", " + clinicName,
						Link = itemServiceGroupRoot.Link
					};

					foreach (HtmlNode nodeRowPrice in nodeCollectionRowPrice) {
						try {
							string name = ClearString(nodeRowPrice.SelectSingleNode("div[1]").InnerText);

							HtmlNode nodePrice = nodeRowPrice.SelectSingleNode("div[2]").SelectSingleNode("div[1]");
							HtmlNode nodePriceButton = nodePrice.SelectSingleNode("a[1]");

							string price;

							if (nodePriceButton != null)
								price = nodePriceButton.SelectSingleNode("i[1]").InnerText;
							else
								price = nodePrice.InnerText;

							ItemService itemService = new ItemService() {
								Name = name,
								Price = ClearString(price)
							};

							itemServiceGroup.ServiceItems.Add(itemService);
						} catch (Exception priceExc) {
							Console.WriteLine(priceExc.Message + Environment.NewLine + priceExc.StackTrace);
						}
					}

					itemSiteData.ServiceGroupItems.Add(itemServiceGroup);
				} catch (Exception e) {
					Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				}
			}
		}

		private void ParseSiteKdlLabRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathMainBlock = "//div[@class='h-card__inner']";
			HtmlNodeCollection nodeCollectionMainBlock = _htmlAgility.GetNodeCollection(docService, xPathMainBlock);

			if (nodeCollectionMainBlock != null) {
				foreach (HtmlNode nodeCard in nodeCollectionMainBlock) {
					try {
						string name = nodeCard.ChildNodes[1].ChildNodes[3].InnerText;
						string price = nodeCard.ChildNodes[5].ChildNodes[3].InnerText;

						ItemService itemService = new ItemService() {
							Name = ClearString(name),
							Price = ClearString(price)
						};

						itemServiceGroup.ServiceItems.Add(itemService);
					} catch (Exception e) {
						Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
					}
				}
			}


			string xPathDataService = "/html/body/div[5]/div/div[3]/div[4]";
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathDataService);

			if (nodeCollectionService != null) {

				HtmlNode htmlNodeDataServiceList = nodeCollectionService.First();
				if (!htmlNodeDataServiceList.Attributes.Contains("data-services-list")) {
					Console.WriteLine("!htmlNodeDataServiceList.Attributes.Contains(\"data-services-list\")");
					return;
				}

				string jsonArray = htmlNodeDataServiceList.Attributes["data-services-list"].Value;
				if (string.IsNullOrEmpty(jsonArray) || string.IsNullOrWhiteSpace(jsonArray)) {
					Console.WriteLine("jsonArray is empty");
					return;
				}

				List<ItemService> serviceItems = JsonConvert.DeserializeObject<List<ItemService>>(jsonArray);
				itemServiceGroup.ServiceItems.AddRange(serviceItems);
			}
		}
		 
		private void ParseSiteGemotestRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathTable = "//table[@class='d-col_xs_12 d-tal catalog-table']/tbody";
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathTable);

			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			foreach (HtmlNode nodeTbody in nodeCollectionService) {
				try {
					HtmlNodeCollection htmlNodeCollectionTd = nodeTbody.SelectNodes("tr[1]/td");
					if (htmlNodeCollectionTd == null) {
						Console.WriteLine("htmlNodeCollectionTd == null");
						continue;
					}

					string name = htmlNodeCollectionTd[0].ChildNodes[1].InnerText;
					string price = htmlNodeCollectionTd[3].ChildNodes[1].ChildNodes[1].InnerText;

					ItemService itemService = new ItemService() {
						Name = ClearString(name),
						Price = ClearString(price)
					};

					itemServiceGroup.ServiceItems.Add(itemService);
				} catch (Exception e) {
					Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				}
			}
			
		}

		private void ParseSiteZubRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathTable = "//div[@class='price_block visible']";
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathTable);

			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			foreach (HtmlNode node in nodeCollectionService) {
				try {
					List<ItemService> serviceItems = ReadTrNodesFdoctorRu(node.SelectSingleNode("table[1]"));
					itemServiceGroup.ServiceItems.AddRange(serviceItems);
				} catch (Exception e) {
					Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				}
			}
		}

		private void ParseSiteDentolRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathTable = "//table[@class='wp_excel_cms_table wp_excel_cms_table_общий']";
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathTable);

			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeCollectionService.First());
			itemServiceGroup.ServiceItems = serviceItems;
		}

		private void ParseSiteNrmedRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathTable = "//*[@id=\"vacancies_block\"]/table/tbody";
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathTable);

			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeCollectionService.First(), 1, 1);
			itemServiceGroup.ServiceItems = serviceItems;
		}

		private void ParseSiteHelixRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPath = "//div[@class='Catalog-Container-Item']";
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPath);

			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			string xPathName = "div[1]/div[1]/b";
			string xPathPrice = "div[2]/span[1]/b";

			foreach (HtmlNode node in nodeCollectionService) {
				try {
					HtmlNodeCollection nodeName = node.SelectNodes(xPathName);
					if (nodeName != null) {
						string name = nodeName.First().InnerText;

						HtmlNodeCollection nodePrice = node.SelectNodes(xPathPrice);
						string price = nodePrice.First().InnerText;

						ItemService itemService = new ItemService() {
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

		private void ParseSiteCmdOnlineRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup, ref ItemSiteData itemSiteData) {
			string xPathPriceTable = "//*[@id=\"serv_list_content\"]/table[1]/tbody";
			
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathPriceTable);
			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");

				_xPathServices = "//*[@id=\"serv_list_content\"]//a[@href]";
				ParseSitesWithLinksOnMainPage(docService, ref itemSiteData);

				return;
			}

			List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeCollectionService.First(), 1, 2);
			itemServiceGroup.ServiceItems = serviceItems;
		}

		private void ParseSiteSmClinicRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathPriceTable;
			switch (itemServiceGroup.Name) {
				case "Консультация врача ":
				case "Вызов врача на дом":
				case "Дежурство бригады скорой медицинской помощи на мероприятии":
				case "Вакцинация":
				case "Скорая помощь":
				case "Медицинские справки":
					xPathPriceTable = "//*[@id=\"content-in\"]/div[2]/table[1]/tbody";
					break;
				case "ВЛОК":
				case "УФОК":
					xPathPriceTable = "//*[@id=\"content-in\"]/span/span/table/tbody";
					break;
				case "Стационары":
					xPathPriceTable = "//*[@id=\"content-in\"]/div[2]/div/div[22]/table/tbody";
					break;
				case "Терапевтические стационары":
					xPathPriceTable = "//*[@id=\"content-in\"]/div[2]/div/div[27]/table/tbody";
					break;
				case "Кардиология":
				case "Пульмонология":
				case "Нефрология":
					xPathPriceTable = "//*[@id=\"content-in\"]/div[2]/table/tbody";
					break;
				default:
					xPathPriceTable = "//*[@id=\"content-in\"]/table/tbody";
					break;
			}

			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathPriceTable);
			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeCollectionService.First());
			itemServiceGroup.ServiceItems = serviceItems;
		}

		private void ParseSiteFamilyDoctorRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathPriceTable = "//*[@id=\"content\"]/article/div[3]/table[1]";
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathPriceTable);
			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeCollectionService.First());
			itemServiceGroup.ServiceItems = serviceItems;
		}

		private void ParseSiteFdoctorRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathServiceTables = "//table";
			string xPathServiceId = "//*[@id=\"site-wrapper\"]/section/div/div/div/div[1]";
			string xPathServiceDataIds = "//*[@id=\"service-inner-panes\"]";
			string urlGetPrices = "/ajax/services/inner/getPrices.php?service_id=@serviceId&service_type_id=@serviceTypeId";

			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathServiceTables);
			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			try {
				HtmlNodeCollection nodeCollectionServiceId = _htmlAgility.GetNodeCollection(docService, xPathServiceId);
				if (nodeCollectionServiceId == null) {
					Console.WriteLine("nodeCollectionServiceId is null");
					return;
				}

				HtmlNodeCollection nodeCollectionServiceDataIds = _htmlAgility.GetNodeCollection(docService, xPathServiceDataIds);
				if (nodeCollectionServiceDataIds == null) {
					Console.WriteLine("nodeCollectionServiceDataIds is null");
					return;
				}

				nodeCollectionServiceDataIds = nodeCollectionServiceDataIds.First().SelectNodes("div");
				if (nodeCollectionServiceDataIds == null) {
					Console.WriteLine("nodeCollectionServiceDataIds.First().SelectNodes(\"div\") is null");
					return;
				}

				string dataValue = nodeCollectionServiceId.First().Attributes["data-value"].Value;
				StringBuilder stringBuilder = new StringBuilder();

				foreach (HtmlNode node in nodeCollectionServiceDataIds) {
					if (!node.HasAttributes || !node.Attributes.Contains("data-service-type-id"))
						continue;

					try {
						string dataServiceTypeId = node.Attributes["data-service-type-id"].Value;
						string urlGetPrice =
							_urlRoot +
							urlGetPrices.
							Replace("@serviceId", dataValue).
							Replace("@serviceTypeId", dataServiceTypeId);
						string response = _htmlAgility.GetResponse(urlGetPrice);

						stringBuilder.Append(response);
					} catch (Exception attrExc) {
						_backgroundWorker.ReportProgress((int)progressCurrent, attrExc.Message);
						Console.WriteLine("exception attribute: " + attrExc.Message);
					}
				}

				HtmlDocument docPrices = new HtmlDocument();
				docPrices.LoadHtml(stringBuilder.ToString());
				List<ItemService> priceServiceItems = ReadTrNodesFdoctorRu(docPrices.DocumentNode);
				itemServiceGroup.ServiceItems.AddRange(priceServiceItems);
			} catch (Exception idExc) {
				_backgroundWorker.ReportProgress((int)progressCurrent, idExc.Message);
				Console.WriteLine("Exception serviceId: " + idExc.Message +
					Environment.NewLine + idExc.StackTrace);
			}

			foreach (HtmlNode table in nodeCollectionService) {
				HtmlNode tbody = table.SelectSingleNode("tbody");
				List<ItemService> serviceItems = ReadTrNodesFdoctorRu(tbody);
				itemServiceGroup.ServiceItems.AddRange(serviceItems);
			}
		}

		private List<ItemService> ReadTrNodesFdoctorRu(HtmlNode node, int nameOffset = 0, int priceOffset = 0) {
			List <ItemService> items = new List<ItemService>();
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

					string name = string.Empty;
					string price = string.Empty;

					if (nodeTd.Count > 0 + nameOffset)
						name = ClearString(nodeTd[0 + nameOffset].InnerText);
					if (nodeTd.Count > 1 + priceOffset) {
						if (priceOffset == 2)
							price = ClearString(nodeTd[1 + priceOffset].ChildNodes[3].InnerText);
						else
							price = ClearString(nodeTd[1 + priceOffset].InnerText);
					}
					
					ItemService itemService = new ItemService() {
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

		private string ClearString(string initial) {
			Dictionary<string, string> toReplace = new Dictionary<string, string>() {
						{ "\r\n", "" },
						{ "\r", "" },
						{ "\n", "" },
						{ "&nbsp;", " " },
						{ "&quot;", "\"" },
						{ "\t", "" },
						{ "р.", "" },
						{ " руб.", "" },
						{ "руб.", "" },
						{ " руб", "" },
						{ ",00", "" },
						{ "&raquo;", "" },
						{ " ₽", "" }
					};

			foreach (KeyValuePair<string, string> pair in toReplace)
				initial = initial.Replace(pair.Key, pair.Value);

			initial = initial.TrimStart(' ').TrimEnd(' ');

			return initial;
		}
	}
}
