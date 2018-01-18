using HtmlAgilityPack;
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
			nrmed_ru, //!!!script used, not implemented
			onclinic_ru,
			smclinic_ru,
			invitro_ru, //!!!work only with offline html
			cmd_online_ru,
			helix_ru
		}

		public SiteParser(BackgroundWorker backgroundWorker) {
			_backgroundWorker = backgroundWorker;
		}

		public void ParseSelectedSites() {
			_selectedSite = Sites.helix_ru;

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
					_urlRoot = "https://www.alfazdrav.ru";
					_urlServices = _urlRoot + "/services/price/";
					_companyName = "ООО «МедАС» — «Альфа-Центр Здоровья»";
					_xPathServices = "/html/body/section/section/div/div/ul[1]/li";
					break;
				case Sites.nrmed_ru:
					_urlRoot = "http://www.nrmed.ru";
					_urlServices = _urlRoot + "/rus/ourserv/prices/?action=show&id=192";
					_companyName = "ООО \"НИАРМЕДИК ПЛЮС\"";
					_xPathServices = "//*[@id=\"srv_list_wrap\"]//a[@href]";
					break;
				case Sites.onclinic_ru:
					_urlRoot = "https://www.onclinic.ru";
					_urlServices = _urlRoot + "/klientam/stoimost_uslug/";
					_companyName = "ООО \"Он Клиник Геоконик\"";
					_xPathServices = "//*[@id=\"center\"]/div";
					break;
				case Sites.smclinic_ru:
					_urlRoot = "http://www.smclinic.ru";
					_urlServices = _urlRoot + "/services/";
					_companyName = "ООО «СМ-Клиника»";
					_xPathServices = "//*[@id=\"colleft\"]//a[@href]";
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
					_xPathServices = "//*[@id=\"serv_list_content\"]/table//a[@href]";
					break;
				case Sites.helix_ru:
					_urlRoot = "https://helix.ru";
					_urlServices = _urlRoot + "/catalog";
					_companyName = "ООО «НПФ «ХЕЛИКС»";
					_xPathServices = "/html/body/div[1]/div[6]/div[2]/div[1]//a[@href]";
					break;
				default:
					return;
			}

			_backgroundWorker.ReportProgress((int)progressCurrent, _companyName);
			_backgroundWorker.ReportProgress((int)progressCurrent, "Загрузка данных с сайта: " + _urlRoot);

			ItemSiteData itemSiteData = new ItemSiteData() { SiteAddress = _urlServices, CompanyName = _companyName };

			HtmlDocument docServices;
			
			if (_selectedSite == Sites.invitro_ru) {
				docServices = _htmlAgility.GetDocument(_urlServices, true);
			} else {
				docServices = _htmlAgility.GetDocument(_urlServices);
			}

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
					ParseSitesWithLinksOnMainPage(docServices, ref itemSiteData);
					break;
				case Sites.alfazdrav_ru:
					ParseSiteAlfazdrav(docServices, ref itemSiteData);
					break;
				case Sites.onclinic_ru:
					ParseSiteOnClinic(docServices, ref itemSiteData);
					break;
				case Sites.invitro_ru:
					ParseSiteInvitroRU(docServices, ref itemSiteData);
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

		private void ParseSiteOnClinic(HtmlDocument docServices, ref ItemSiteData itemSiteData) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, _xPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			for (int i = 0; i < nodeCollectionServices.Count; i++) {
				HtmlNode nodeRoot = nodeCollectionServices[i];

				try {
					if (nodeRoot.HasAttributes && nodeRoot.Attributes.Contains("class")) {
						string value = nodeRoot.Attributes["class"].Value;

						if (value.Equals("price-holder")) {
							ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
								Name = "Первичный прием врача - специалиста",
								Link = _urlServices
							};

							_backgroundWorker.ReportProgress((int)progressCurrent, itemServiceGroup.Name);

							List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeRoot.ChildNodes[1].ChildNodes[1]);
							itemServiceGroup.ServiceItems = serviceItems;
							itemSiteData.ServiceGroupItems.Add(itemServiceGroup);
						}

						if (value.Equals("block")) {
							string serviceGroupName = nodeRoot.FirstChild.InnerText.Replace("\r", "").Replace("\n", "").TrimStart(' ').TrimEnd(' ');
							_backgroundWorker.ReportProgress((int)progressCurrent, serviceGroupName);
							List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeRoot.ChildNodes[2].ChildNodes[1].ChildNodes[1]);

							ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
								Name = serviceGroupName,
								Link = _urlServices,
								ServiceItems = serviceItems
							};

							itemSiteData.ServiceGroupItems.Add(itemServiceGroup);
						}
					}
				} catch (Exception e) {
					_backgroundWorker.ReportProgress((int)progressCurrent, e.Message + Environment.NewLine + e.StackTrace);
				}
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

						HtmlNodeCollection nodeServices = nodeGroup.SelectNodes("div[1]/table[1]");

						List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeServices.First());

						if (serviceItems.Count == 0) {
							_backgroundWorker.ReportProgress((int)progressCurrent, "Услуг не обнаружено, пропуск");
							continue;
						}

						itemServiceGroup.ServiceItems = serviceItems;
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
					string urlService = _urlRoot + servicePage.Attributes["href"].Value;

					ItemServiceGroup itemServiceGroup = new ItemServiceGroup() { Name = serviceName, Link = urlService };

					HtmlDocument docService = _htmlAgility.GetDocument(urlService);
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
							ParseSiteCmdOnlineRu(docService, ref itemServiceGroup);
							break;
						case Sites.helix_ru:
							ParseSiteHelixRu(docService, ref itemServiceGroup);
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

		private void ParseSiteCmdOnlineRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathPriceTable = "//*[@id=\"serv_list_content\"]/table[1]/tbody";
			
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathPriceTable);
			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
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
						{ " руб.", ""}
					};

			foreach (KeyValuePair<string, string> pair in toReplace)
				initial = initial.Replace(pair.Key, pair.Value);

			initial = initial.TrimStart(' ').TrimEnd(' ');

			return initial;
		}
	}
}
