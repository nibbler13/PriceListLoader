using HtmlAgilityPack;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PriceListLoader {
	class SiteParser {
		private HtmlAgility _htmlAgility = new HtmlAgility();
		private BackgroundWorker _backgroundWorker;
		private SiteInfo siteInfo;

		private double progressCurrent = 0;
		private double progressTo = 100;

		public SiteParser(BackgroundWorker backgroundWorker) {
			_backgroundWorker = backgroundWorker;
		}



		public void ParseSelectedSite(SiteInfo siteInfo) {
			this.siteInfo = siteInfo;

			_backgroundWorker.ReportProgress((int)progressCurrent, this.siteInfo.CompanyName);
			_backgroundWorker.ReportProgress((int)progressCurrent, "Загрузка данных с сайта: " + this.siteInfo.UrlRoot);

			HtmlDocument docServices;

			bool isLocalFile = false;
			if (this.siteInfo.Name == SiteInfo.SiteName.invitro_ru ||
				this.siteInfo.Name == SiteInfo.SiteName.mrt24_ru ||
				this.siteInfo.Name == SiteInfo.SiteName.alfazdrav_ru) {
				isLocalFile = true;
				OpenFileDialog openFileDialog = new OpenFileDialog {
					Filter = "HTML файл (*.html)|*.html",
					CheckFileExists = true,
					Multiselect = false,
					Title = "Выберите HTML файл для обработки"
				};

				if (openFileDialog.ShowDialog() == true) {
					this.siteInfo.UrlServicesPage = openFileDialog.FileName;
				} else {
					_backgroundWorker.ReportProgress((int)progressCurrent, "Не выбран файл для обработки");
					return;
				}

			}

			docServices = _htmlAgility.GetDocument(this.siteInfo.UrlServicesPage, this.siteInfo.Name, isLocalFile);

			if (docServices == null) {
				_backgroundWorker.ReportProgress((int)progressCurrent, "Не удалось загрузить страницу: " + this.siteInfo.UrlServicesPage);
				Console.WriteLine("docServices is null");
				return;
			}

			switch (this.siteInfo.Name) {
				case SiteInfo.SiteName.fdoctor_ru:
				case SiteInfo.SiteName.familydoctor_ru:
				case SiteInfo.SiteName.familydoctor_ru_child:
				case SiteInfo.SiteName.nrmed_ru:
				case SiteInfo.SiteName.nrmed_ru_child:
				case SiteInfo.SiteName.smclinic_ru:
				case SiteInfo.SiteName.cmd_online_ru:
				case SiteInfo.SiteName.helix_ru:
				case SiteInfo.SiteName.dentol_ru:
				case SiteInfo.SiteName.zub_ru:
				case SiteInfo.SiteName.gemotest_ru:
				case SiteInfo.SiteName.kdllab_ru:
				case SiteInfo.SiteName.medsi_ru:
				case SiteInfo.SiteName.onclinic_ru:
				case SiteInfo.SiteName.nrlab_ru:
				case SiteInfo.SiteName.sm_stomatology_ru:
				case SiteInfo.SiteName.spb_evro_med_ru:
				case SiteInfo.SiteName.spb_baltzdrav_ru:
				case SiteInfo.SiteName.spb_german_clinic:
				case SiteInfo.SiteName.spb_german_dental:
				case SiteInfo.SiteName.spb_medswiss_spb_ru:
				case SiteInfo.SiteName.spb_helix_ru:
				case SiteInfo.SiteName.spb_emcclinic_ru:
				case SiteInfo.SiteName.ufa_promedicina_clinic:
				case SiteInfo.SiteName.yekuk_ruslabs_ru:
				case SiteInfo.SiteName.yekuk_mc_vd_ru:
				case SiteInfo.SiteName.yekuk_immunoresurs_ru:
					ParseSiteWithLinksOnMainPage(docServices);
					break;
				case SiteInfo.SiteName.alfazdrav_ru:
					ParseSiteAlfazdrav(docServices);
					break;
				case SiteInfo.SiteName.invitro_ru:
				case SiteInfo.SiteName.spb_invitro_ru:
					ParseSiteInvitroRU(docServices);
					break;
				case SiteInfo.SiteName.mrt24_ru:
					ParseSiteMrt24Ru(docServices);
					break;
				case SiteInfo.SiteName.vse_svoi_ru:
					ParseSiteVseSvoiRu(docServices);
					break;
				case SiteInfo.SiteName.novostom_ru:
					ParseSiteNovostomRu(docServices);
					break;
				case SiteInfo.SiteName.masterdent_ru:
					ParseSiteMasterdentRu(docServices);
					break;
				case SiteInfo.SiteName.smdoctor_ru:
					ParseSiteSmDoctorRu(docServices);
					break;
				case SiteInfo.SiteName.smclinic_ru_lab:
					ParseSiteSmClinicRuLab(docServices);
					break;
				case SiteInfo.SiteName.spb_mc21_ru:
					ParseSiteSpbMc21Ru(docServices);
					break;
				case SiteInfo.SiteName.spb_clinic_complex_ru:
					ParseSiteSpbClinicComplexRu(docServices);
					break;
				case SiteInfo.SiteName.ufa_megi_clinic:
					ParseSiteUfaMegiClinic(docServices);
					break;
				case SiteInfo.SiteName.ufa_mamadeti_ru:
					ParseSiteUfaMamadetiRu(docServices);
					break;
				case SiteInfo.SiteName.ufa_mdplus_ru:
					ParseSiteUfaMdplusRu(docServices);
					break;
				default:
					break;
			}

			if (this.siteInfo.ServiceGroupItems.Count == 0) {
				_backgroundWorker.ReportProgress((int)progressCurrent, "Не удалось найти ни одной группы услуг");
				return;
			}

			string resultFile = NpoiExcel.WriteItemSiteDataToExcel(this.siteInfo, _backgroundWorker, progressCurrent, progressTo);
		}



		private void ParseSiteUfaMdplusRu(HtmlDocument docServices) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			ItemServiceGroup itemServiceGroup = null;
			foreach (HtmlNode nodeServices in nodeCollectionServices) {
				foreach (HtmlNode child in nodeServices.ChildNodes) {
					if (child.Name.Equals("h4") && itemServiceGroup == null) {
						itemServiceGroup = new ItemServiceGroup() {
							Name = SiteInfo.ClearString(child.InnerText),
							Link = siteInfo.UrlServicesPage
						};
						continue;
					}

					if (child.Name.Equals("table") && itemServiceGroup != null) {
						HtmlNode nodeTbody = child.SelectSingleNode(child.XPath + "//tbody");
						if (nodeTbody == null) {
							Console.WriteLine("nodeTbody == null");
							continue;
						}

						List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeTbody, 1, 1);
						itemServiceGroup.ServiceItems.AddRange(serviceItems);

						if (itemServiceGroup.ServiceItems.Count > 0)
							siteInfo.ServiceGroupItems.Add(itemServiceGroup);

						itemServiceGroup = null;
					}
				}
			}
		}

		private void ParseSiteUfaMamadetiRu(HtmlDocument docServices) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			foreach (HtmlNode nodeGroup in nodeCollectionServices) {
				HtmlNode nodeGroupName = nodeGroup.SelectSingleNode(nodeGroup.XPath + "//div[@class='b-tree_link js-price-click']");
				if (nodeGroupName == null) {
					Console.WriteLine("nodeGroupName == null");
					continue;
				}

				string groupName = SiteInfo.ClearString(nodeGroupName.InnerText);
				ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
					Name = groupName,
					Link = siteInfo.UrlServicesPage
				};

				HtmlNodeCollection nodeCollectionTables = nodeGroup.SelectNodes(nodeGroup.XPath + "//table");
				if (nodeCollectionTables == null) {
					Console.WriteLine("nodeCollectionTables == null");
					continue;
				}

				foreach (HtmlNode nodeTable in nodeCollectionTables) {
					List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeTable);
					itemServiceGroup.ServiceItems.AddRange(serviceItems);
				}

				if (itemServiceGroup.ServiceItems.Count > 0)
					siteInfo.ServiceGroupItems.Add(itemServiceGroup);
			}
		}

		private void ParseSiteUfaMegiClinic(HtmlDocument docServices) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			foreach (HtmlNode htmlNodeGroup in nodeCollectionServices) {
				string groupName = string.Empty; 
				HtmlNode nodeGroupName = htmlNodeGroup.SelectSingleNode(htmlNodeGroup.XPath + "/div[@class='more']");
				if (nodeGroupName != null)
					groupName = SiteInfo.ClearString(nodeGroupName.InnerText);

				ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
					Name = groupName,
					Link = siteInfo.UrlServicesPage
				};

				HtmlNode nodeTbody = htmlNodeGroup.SelectSingleNode(htmlNodeGroup.XPath + "//tbody");
				if (nodeTbody == null) {
					Console.WriteLine("nodeTbody == null");
					continue;
				}

				List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeTbody);
				itemServiceGroup.ServiceItems.AddRange(serviceItems);

				if (itemServiceGroup.ServiceItems.Count > 0)
					siteInfo.ServiceGroupItems.Add(itemServiceGroup);
			}
		}

		private void ParseSiteSpbClinicComplexRu(HtmlDocument docServices) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			foreach (HtmlNode nodeService in nodeCollectionServices) {
				string xPathRootName = "//div[@class='mainblock-price-spollers-item-title__label']";
				HtmlNode nodeRootName = nodeService.SelectSingleNode(nodeService.XPath + xPathRootName);
				if (nodeRootName == null) {
					Console.WriteLine("nodeRootName == null");
					continue;
				}

				string rootName = SiteInfo.ClearString(nodeRootName.InnerText);
				string xPathService = "//div[@class='mainblock-price-spollers-item-block-item']";
				HtmlNodeCollection nodeCollectionService = nodeService.SelectNodes(nodeService.XPath + xPathService);
				if (nodeCollectionService == null) {
					Console.WriteLine("nodeCollectionService == null");
					continue;
				}

				foreach (HtmlNode nodeServiceInner in nodeCollectionService) {
					string xPathRootInner = "//div[starts-with(@class, 'mainblock-price-spollers-item-block-item__title')]";
					HtmlNode nodeRootInnerName = nodeServiceInner.SelectSingleNode(nodeServiceInner.XPath + xPathRootInner);
					if (nodeRootInnerName == null) {
						Console.WriteLine("nodeRootInnerName == null");
						continue;
					}

					string rootInnerName = rootName + " - " + SiteInfo.ClearString(nodeRootInnerName.InnerText);
					string xPathRows = "//div[@class='trow']";
					HtmlNodeCollection nodeCollectionRows = nodeServiceInner.SelectNodes(nodeServiceInner.XPath + xPathRows);
					if (nodeCollectionRows == null) {
						Console.WriteLine("nodeCollectionRows == null");
						continue;
					}

					ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
						Name = rootInnerName,
						Link = siteInfo.UrlServicesPage
					};

					foreach (HtmlNode nodeRow in nodeCollectionRows) {
						string xPathLabel = "//div[@class='mainblock-price-spollers__label']";
						string xPathPrice = "//div[starts-with(@class, 'mainblock-price-spollers__price')]";
						HtmlNode nodeLabel = nodeRow.SelectSingleNode(nodeRow.XPath + xPathLabel);
						HtmlNode nodePrice = nodeRow.SelectSingleNode(nodeRow.XPath + xPathPrice);

						if (nodeLabel == null || nodePrice == null) {
							Console.WriteLine("nodeLabel == null || nodePrice == null");
							continue;
						}

						ItemService itemService = new ItemService() {
							Name = SiteInfo.ClearString(nodeLabel.InnerText),
							Price = SiteInfo.ClearString(nodePrice.InnerText)
						};

						itemServiceGroup.ServiceItems.Add(itemService);
					}

					siteInfo.ServiceGroupItems.Add(itemServiceGroup);
				}
			}
		}

		private void ParseSiteSpbMc21Ru(HtmlDocument docServices, bool isFirstCycle = true, 
			HtmlNodeCollection nodeCollection = null, string rootName = "") {
			HtmlNodeCollection nodeCollectionServices;
			if (isFirstCycle) {
				nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
				nodeCollectionServices = nodeCollectionServices[0].ChildNodes;
			} else {
				nodeCollectionServices = nodeCollection;
			}

			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			ItemServiceGroup itemServiceGroup = null;
			foreach (HtmlNode htmlNode in nodeCollectionServices) {
				if (itemServiceGroup == null && (htmlNode.Name.Equals("h2") || htmlNode.Name.Equals("h3"))) {
					string groupName = SiteInfo.ClearString(htmlNode.InnerText);
					itemServiceGroup = new ItemServiceGroup() {
						Link = siteInfo.UrlServicesPage,
						Name = isFirstCycle ? groupName : rootName + " - " + groupName
					};
					continue;
				}

				if (itemServiceGroup != null && htmlNode.Name.Equals("div")) {
					string xPathCurrent = htmlNode.XPath;
					HtmlNodeCollection nodeCollectionInners = htmlNode.SelectNodes(xPathCurrent + "//h3");
					if (nodeCollectionInners == null) {
						HtmlNodeCollection nodeCollectionTr = htmlNode.SelectNodes(xPathCurrent + "//tr");
						if (nodeCollectionTr == null) {
							Console.WriteLine(itemServiceGroup.Name + " nodeCollectionTr == null");
							continue;
						}

						foreach (HtmlNode htmlNodeTr in nodeCollectionTr) {
							if (htmlNodeTr.ChildNodes.Count < 2) {
								Console.WriteLine("htmlNodeTr.ChildNodes.Count < 2");
								continue;
							}

							string serviceName = SiteInfo.ClearString(htmlNodeTr.ChildNodes[0].InnerText);
							string servicePriceClinic = SiteInfo.ClearString(htmlNodeTr.ChildNodes[1].InnerText);
							string servicePriceHome = htmlNodeTr.ChildNodes.Count == 3 ?
								SiteInfo.ClearString(htmlNodeTr.ChildNodes[2].InnerText) : string.Empty;

							if (string.IsNullOrEmpty(servicePriceHome)) {
								itemServiceGroup.ServiceItems.Add(
									new ItemService() { Name = serviceName, Price = servicePriceClinic });
							} else {
								itemServiceGroup.ServiceItems.Add(
									new ItemService() { Name = serviceName + ", амбулаторно", Price = servicePriceClinic });
								itemServiceGroup.ServiceItems.Add(
									new ItemService() { Name = serviceName + ", на дому", Price = servicePriceHome });
							}
						}
					} else {
						ParseSiteSpbMc21Ru(docServices, false, htmlNode.ChildNodes, itemServiceGroup.Name);
					}

					if (itemServiceGroup.ServiceItems.Count > 0)
						siteInfo.ServiceGroupItems.Add(itemServiceGroup);

					itemServiceGroup = null;
				}
			}
		}

		private void ParseSiteSmClinicRuLab(HtmlDocument docServices) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			foreach (HtmlNode node in nodeCollectionServices) {
				string blockName = string.Empty;
				HtmlNode nodeBlockName = node.SelectSingleNode(node.XPath + "/div[@class='panel-heading']");
				if (nodeBlockName != null)
					blockName = SiteInfo.ClearString(nodeBlockName.InnerText);

				ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
					Name = blockName,
					Link = siteInfo.UrlServicesPage
				};

				HtmlNodeCollection nodeCollectionItems = node.SelectNodes(node.XPath + "//div[@class='input-holder']");
				if (nodeCollectionItems == null) {
					Console.WriteLine("nodeCollectionItems == null");
					continue;
				}

				foreach (HtmlNode nodeItem in nodeCollectionItems) {
					try {
						string itemCost = SiteInfo.ClearString(nodeItem.ChildNodes[1].Attributes["data-price"].Value);
						string itemName = SiteInfo.ClearString(nodeItem.ChildNodes[2].InnerText);
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

				siteInfo.ServiceGroupItems.Add(itemServiceGroup);
			}
		}

		private void ParseSiteSmDoctorRu(HtmlDocument docServices) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
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

						string sectionName = SiteInfo.ClearString(nodeSectionName.InnerText);
						ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
							Name = sectionName,
							Link = siteInfo.UrlServicesPage
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

						siteInfo.ServiceGroupItems.Add(itemServiceGroup);
					} catch (Exception e) {
						Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
					}
				}

				HtmlNode nodeSingleTable = nodeGroup.SelectSingleNode(nodeGroup.XPath + "/table[@class='price-list']/tbody");
				if (nodeSingleTable != null) {
					ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
						Name = "Детская хирургия",
						Link = siteInfo.UrlServicesPage
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

					siteInfo.ServiceGroupItems.Add(itemServiceGroup);
				}
			}
		}

		private void ParseSiteMasterdentRu(HtmlDocument docServices) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
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
					Name = SiteInfo.ClearString(groupName),
					Link = siteInfo.UrlServicesPage
				};

				HtmlNode htmlNodeTable = node.SelectSingleNode("div[1]/table");
				if (htmlNodeTable == null) {
					Console.WriteLine("htmlNodeTable == null");
					continue;
				}

				List<ItemService> serviceItems = ReadTrNodesFdoctorRu(htmlNodeTable.SelectSingleNode("tbody"), 1, 1);
				itemServiceGroup.ServiceItems = serviceItems;

				siteInfo.ServiceGroupItems.Add(itemServiceGroup);
			}
		}

		private void ParseSiteNovostomRu(HtmlDocument docServices) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			foreach (HtmlNode node in nodeCollectionServices) {
				ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
					Link = siteInfo.UrlServicesPage
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
							Name = SiteInfo.ClearString(name)
						};

						if (htmlNodeCollectionTd.Count >= 2) {
							string price = htmlNodeCollectionTd[1].InnerText;
							itemService.Price = SiteInfo.ClearString(price);
						}

						itemServiceGroup.ServiceItems.Add(itemService);
					}
				}

				siteInfo.ServiceGroupItems.Add(itemServiceGroup);
			}
		}

		private void ParseSiteVseSvoiRu(HtmlDocument docServices) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
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
						Link = siteInfo.UrlServicesPage
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
									Price = SiteInfo.ClearString(price)
								};

								itemServiceGroup.ServiceItems.Add(itemService);
							} catch (Exception ex) {
								Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
							}
						}
					}

					siteInfo.ServiceGroupItems.Add(itemServiceGroup);
				} catch (Exception e) {
					Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				}
			}
		}

		private void ParseSiteMrt24Ru(HtmlDocument docServices) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
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

					siteInfo.ServiceGroupItems.Add(itemServiceGroup);
				} catch (Exception e) {
					Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				}
			}
		}

		private void ParseSiteInvitroRU(HtmlDocument docServices) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
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

				string serviceGroupName = SiteInfo.ClearString(nodeCollectionTh.First().InnerText);

				ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
					Name = serviceGroupName,
					Link = siteInfo.UrlServicesPage
				};

				List<ItemService> serviceItems = ReadTrNodesFdoctorRu(node, 1, 2);
				itemServiceGroup.ServiceItems.AddRange(serviceItems);
				siteInfo.ServiceGroupItems.Add(itemServiceGroup);
			}
		}

		private void ParseSiteAlfazdrav(HtmlDocument docServices) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
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

						ItemServiceGroup itemServiceGroup = new ItemServiceGroup() { Name = departmentName + ", " + groupName, Link = siteInfo.UrlServicesPage };

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

						siteInfo.ServiceGroupItems.Add(itemServiceGroup);
					}
				} catch (Exception e) {
					_backgroundWorker.ReportProgress((int)progressCurrent, e.Message + Environment.NewLine + e.StackTrace);
				}
			}
		}



		private void ParseSiteWithLinksOnMainPage(HtmlDocument docServices) {
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionServices is null");
				return;
			}

			if (siteInfo.Name == SiteInfo.SiteName.spb_baltzdrav_ru) {
				ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
					Link = siteInfo.UrlServicesPage
				};

				ParseSiteSpbBaltzdravRu(docServices, ref itemServiceGroup);

				if (itemServiceGroup.ServiceItems.Count > 0)
					siteInfo.ServiceGroupItems.Add(itemServiceGroup);
			}

			if (siteInfo.Name == SiteInfo.SiteName.yekuk_immunoresurs_ru) {
				ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
					Name = "",
					Link = siteInfo.UrlServicesPage
				};

				ParseSiteYekukImmunoresursRu(docServices, ref itemServiceGroup);
				siteInfo.ServiceGroupItems.Add(itemServiceGroup);
			}

			double progressStep = ((progressTo - progressCurrent) * 0.7) / nodeCollectionServices.Count;
			_backgroundWorker.ReportProgress((int)progressCurrent, "Загрузка прайс-листов для групп услуг");

			foreach (HtmlNode servicePage in nodeCollectionServices) {
				progressCurrent += progressStep;

				try {
					string serviceName = SiteInfo.ClearString(servicePage.InnerText);

					if (siteInfo.Name == SiteInfo.SiteName.spb_emcclinic_ru &&
						serviceName.ToLower().Equals("записаться"))
						continue;

					_backgroundWorker.ReportProgress((int)progressCurrent, serviceName);
					Console.WriteLine("serviceName: " + serviceName);

					string urlService = string.Empty;

					if (servicePage.Attributes.Contains("href")) {
						string hrefValue = servicePage.Attributes["href"].Value;
						if (hrefValue.StartsWith("http")) {
							urlService = hrefValue;
						} else {
							if (!hrefValue.StartsWith("/"))
								hrefValue = "/" + hrefValue;

							urlService = siteInfo.UrlRoot + hrefValue;
						}
					}

					if (siteInfo.Name == SiteInfo.SiteName.helix_ru ||
						siteInfo.Name == SiteInfo.SiteName.spb_helix_ru) {
						string onClickValue = servicePage.Attributes["onclick"].Value;
						onClickValue = onClickValue.Replace("$(location).attr('href', '", "").Replace(";');", "").Replace("');", "");
						urlService = siteInfo.UrlRoot + onClickValue;
					}

					if (siteInfo.Name == SiteInfo.SiteName.medsi_ru)
						if (urlService.Contains("#") && !urlService.EndsWith("/"))
							continue;

					if (siteInfo.Name == SiteInfo.SiteName.spb_emcclinic_ru) {
						if (string.IsNullOrEmpty(serviceName)) {
							string xPathServiceName = servicePage.ParentNode.XPath + "//div[@class='n-services__text']";
							HtmlNode htmlNodeServiceName = servicePage.SelectSingleNode(xPathServiceName);

							if (htmlNodeServiceName != null)
								serviceName = SiteInfo.ClearString(htmlNodeServiceName.InnerText);
						}
					}

					if (string.IsNullOrEmpty(urlService)) {
						Console.WriteLine("string.IsNullOrEmpty(urlService)");
						continue;
					}

					ItemServiceGroup itemServiceGroup = new ItemServiceGroup() { Name = serviceName, Link = urlService };

					HtmlDocument docService = _htmlAgility.GetDocument(urlService, siteInfo.Name);
					HtmlDocument docServicePrice = new HtmlDocument();

					if (siteInfo.Name == SiteInfo.SiteName.zub_ru)
						docServicePrice = _htmlAgility.GetDocument(urlService + "price/", siteInfo.Name);

					if (docService == null) {
						Console.WriteLine("docService is null");
						continue;
					}

					switch (siteInfo.Name) {
						case SiteInfo.SiteName.fdoctor_ru:
							ParseSiteFdoctorRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.familydoctor_ru:
						case SiteInfo.SiteName.familydoctor_ru_child:
							ParseSiteFamilyDoctorRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.smclinic_ru:
							if (serviceName.Equals("Диагностика")) {
								siteInfo.XPathServices = "//*[@id=\"content-in\"]/div[2]//a[@href]";
								ParseSiteWithLinksOnMainPage(docService);
								continue;
							} else if (serviceName.Equals("Лечение")) {
								siteInfo.XPathServices = "//*[@id=\"colleft\"]//a[@href]";
								ParseSiteWithLinksOnMainPage(docService);
								continue;
							}

							ParseSiteSmClinicRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.cmd_online_ru:
							ParseSiteCmdOnlineRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.helix_ru:
						case SiteInfo.SiteName.spb_helix_ru:
							ParseSiteHelixRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.nrmed_ru:
						case SiteInfo.SiteName.nrmed_ru_child:
							ParseSiteNrmedRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.dentol_ru:
							ParseSiteDentolRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.zub_ru:
							ParseSiteZubRu(docService, ref itemServiceGroup);
							ParseSiteZubRu(docServicePrice, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.gemotest_ru:
							ParseSiteGemotestRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.kdllab_ru:
							ParseSiteKdlLabRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.medsi_ru:
							ParseSiteMedsiRu(docService, itemServiceGroup);
							break;
						case SiteInfo.SiteName.onclinic_ru:
							ParseSiteOnClinic(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.nrlab_ru:
							ParseSiteNrLabRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.sm_stomatology_ru:
							ParseSiteSmStomatologyRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.spb_evro_med_ru:
							ParseSiteSpbEvroMedRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.spb_baltzdrav_ru:
							ParseSiteSpbBaltzdravRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.spb_german_clinic:
						case SiteInfo.SiteName.spb_german_dental:
							ParseSiteSpbGermanClinic(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.spb_medswiss_spb_ru:
							ParseSiteSpbMedswissSpbRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.spb_emcclinic_ru:
							ParseSiteSpbEmcclinicRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.ufa_promedicina_clinic:
							ParseSiteUfaPromedicinaClinic(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.yekuk_ruslabs_ru:
							ParseSiteYekukRuslabsRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.yekuk_mc_vd_ru:
							ParseSiteYekukMcVdRu(docService, ref itemServiceGroup);
							break;
						case SiteInfo.SiteName.yekuk_immunoresurs_ru:
							ParseSiteYekukImmunoresursRu(docService, ref itemServiceGroup);
							break;
						default:
							break;
					}

					if (itemServiceGroup.ServiceItems.Count == 0) {
						_backgroundWorker.ReportProgress((int)progressCurrent, "Услуг не обнаружено, пропуск");
						continue;
					}

					siteInfo.ServiceGroupItems.Add(itemServiceGroup);
				} catch (Exception e) {
					_backgroundWorker.ReportProgress((int)progressCurrent, e.Message);
					Console.WriteLine("Exception: " + e.Message +
						Environment.NewLine + servicePage.InnerText);
				}
			}

			Console.WriteLine("completed");
		}

		private void ParseSiteYekukImmunoresursRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroupRoot) {
			string xPathServices = "//div[@class='object record-item']";
			HtmlNodeCollection nodeCollectionServicesMainPage = _htmlAgility.GetNodeCollection(docService, xPathServices);
			Console.WriteLine(itemServiceGroupRoot.Name);

			if (nodeCollectionServicesMainPage != null)
				ParseSiteYekukImmunoresursRuGroup(nodeCollectionServicesMainPage, ref itemServiceGroupRoot);

			xPathServices = "//div[@id='id2']";
			HtmlNodeCollection nodeCollectionServicesLab = _htmlAgility.GetNodeCollection(docService, xPathServices);
			if (nodeCollectionServicesLab != null) {
				HtmlNode nodeH3Title = nodeCollectionServicesLab[0].SelectSingleNode(
					nodeCollectionServicesLab[0].XPath + "//h3[@class='contentTitle']");
				if (nodeH3Title != null) {
					string title = SiteInfo.ClearString(nodeH3Title.InnerText);

					HtmlNode htmlNodeTbody = nodeCollectionServicesLab[0].SelectSingleNode(
						nodeCollectionServicesLab[0].XPath + "//div[@class='contentText']//tbody");
					if (htmlNodeTbody != null) {
						List<ItemService> serviceItems = ReadTrNodesFdoctorRu(htmlNodeTbody, 1, 4);
						if (serviceItems.Count > 0) {
							ItemServiceGroup itemServiceGroup = new ItemServiceGroup() {
								Name = title,
								Link = itemServiceGroupRoot.Link
							};
							itemServiceGroup.ServiceItems = serviceItems;
							siteInfo.ServiceGroupItems.Add(itemServiceGroup);
						}
					}

					HtmlNodeCollection nodeCollectionDivObjects = nodeCollectionServicesLab[0].SelectNodes(
						nodeCollectionServicesLab[0].XPath + "//div[@class='object']");
					if (nodeCollectionDivObjects != null)
						ParseSiteYekukImmunoresursRuGroup(nodeCollectionDivObjects, ref itemServiceGroupRoot, true);
				}
			}
		}

		private void ParseSiteYekukImmunoresursRuGroup(HtmlNodeCollection nodeCollection, ref ItemServiceGroup itemServiceGroupRoot, bool notDefaultOffset = false) {
			foreach (HtmlNode nodeService in nodeCollection) {
				HtmlNode nodeTitle = nodeService.SelectSingleNode(nodeService.XPath + "//h4[@class='objectTitle']");
				if (nodeTitle == null) {
					Console.WriteLine("nodeTitle == null");
					continue;
				}

				string groupName = SiteInfo.ClearString(nodeTitle.InnerText);
				HtmlNode nodeTbody = nodeService.SelectSingleNode(nodeService.XPath + "//tbody");
				if (nodeTbody == null) {
					Console.WriteLine("nodeTbody == null");
					continue;
				}

				int offsetName = 0;
				int offsetPrice = 0;

				if (notDefaultOffset) {
					offsetName = 1;
					offsetPrice = 4;
				}

				List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeTbody, offsetName, offsetPrice);
				ItemServiceGroup itemServiceGroupInner = new ItemServiceGroup() {
					Name = groupName,
					Link = itemServiceGroupRoot.Link
				};
				itemServiceGroupInner.ServiceItems = serviceItems;

				if (itemServiceGroupInner.ServiceItems.Count > 0)
					siteInfo.ServiceGroupItems.Add(itemServiceGroupInner);
			}
		}

		private void ParseSiteYekukMcVdRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathServices = "//table[@id='displayProduct']";
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docService, xPathServices);
			Console.WriteLine(itemServiceGroup.Name);

			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			itemServiceGroup.ServiceItems.AddRange(ReadTrNodesFdoctorRu(nodeCollectionServices[0], 1, 2));

			string xPathNextPage = "//ul[@class='page-numbers']//a[@class='next page-numbers']";
			HtmlNodeCollection nodeNextPageCollection = _htmlAgility.GetNodeCollection(docService, xPathNextPage);
			if (nodeNextPageCollection == null)
				return;

			string linkNextPage = nodeNextPageCollection[0].Attributes["href"].Value;
			HtmlDocument docNextPage = _htmlAgility.GetDocument(linkNextPage, siteInfo.Name);

			if (docNextPage == null) {
				Console.WriteLine("docNextPage == null");
				return;
			}

			ParseSiteYekukMcVdRu(docNextPage, ref itemServiceGroup);

		}

		private void ParseSiteYekukRuslabsRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroupRoot) {
			string xPathServices = "//div[@class='price-content']";
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docService, xPathServices);
			Console.WriteLine(itemServiceGroupRoot.Name);

			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			ItemServiceGroup itemServiceGroupInner = null;
			HtmlNodeCollection nodeCollectionP = nodeCollectionServices[0].SelectNodes(nodeCollectionServices[0].XPath + "/p");
			foreach (HtmlNode nodeChild in nodeCollectionServices[0].ChildNodes) {
				if (nodeChild.Name.Equals("p") && itemServiceGroupInner == null) {
					itemServiceGroupInner = new ItemServiceGroup() {
						Name = itemServiceGroupRoot.Name + " - " + SiteInfo.ClearString(nodeChild.InnerText),
						Link = itemServiceGroupRoot.Link
					};
					continue;
				}

				if (nodeChild.Name.Equals("table")) {
					if (itemServiceGroupInner == null) {
						if (nodeCollectionP == null || nodeCollectionP.Count == 1) {
							itemServiceGroupInner = new ItemServiceGroup() {
								Name = itemServiceGroupRoot.Name,
								Link = itemServiceGroupRoot.Link
							};
						} else
							continue;
					}

					HtmlNode nodeTbody = nodeChild.SelectSingleNode(nodeChild.XPath + "/tbody");
					if (nodeTbody == null) {
						Console.WriteLine("nodeTbody == null");
						itemServiceGroupInner = null;
						continue;
					}
					
					itemServiceGroupInner.ServiceItems = ReadTrNodesFdoctorRu(nodeTbody, 1, 2);
					if (itemServiceGroupInner.ServiceItems.Count > 0)
						siteInfo.ServiceGroupItems.Add(itemServiceGroupInner);

					itemServiceGroupInner = null;
				}
			}
		}

		private void ParseSiteUfaPromedicinaClinic(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathServices = "//div[starts-with(@class,'price_block_item')]";
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docService, xPathServices);
			Console.WriteLine(itemServiceGroup.Name);

			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			foreach (HtmlNode nodeService in nodeCollectionServices) {
				HtmlNode nodeServiceName = nodeService.SelectSingleNode(nodeService.XPath + "/div[@class='price_text']");
				HtmlNode nodeServicePrice = nodeService.SelectSingleNode(nodeService.XPath + "//div[@class='price_value']");

				if (nodeServiceName == null) {
					Console.WriteLine("nodeServiceName == null");
					continue;
				}

				string serviceName = SiteInfo.ClearString(nodeServiceName.InnerText);
				string servicePrice = string.Empty;
				if (nodeServicePrice != null) {
					servicePrice = SiteInfo.ClearString(nodeServicePrice.InnerText);
					serviceName = SiteInfo.ClearString(serviceName.Substring(0, serviceName.IndexOf(servicePrice) - 1));
				}

				ItemService itemService = new ItemService() {
					Name = serviceName,
					Price = servicePrice
				};

				itemServiceGroup.ServiceItems.Add(itemService);
			}
		}

		private void ParseSiteSpbEmcclinicRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			siteInfo.XPathServices = "//div[@class='n-specs__inside']//a[@href]";
			ParseSiteWithLinksOnMainPage(docService);

			siteInfo.XPathServices = "//div[starts-with(@class,'n-text')]//li//a[@href]";
			ParseSiteWithLinksOnMainPage(docService);

			string xPathServices = "//div[@class='n-costs__item']";
			HtmlNodeCollection nodeCollectionServices = _htmlAgility.GetNodeCollection(docService, xPathServices);
			Console.WriteLine(itemServiceGroup.Name);

			if (nodeCollectionServices == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}


			foreach (HtmlNode nodeService in nodeCollectionServices) {
				try {
					string xPathName = nodeService.XPath + "//div[@class='n-costs__name']";
					string xPathCost = nodeService.XPath + "//div[@class='n-costs__cost']";

					HtmlNode htmlNodeName = nodeService.SelectSingleNode(xPathName);
					if (htmlNodeName == null) {
						Console.WriteLine("htmlNodeName == null");
						continue;
					}

					HtmlNode htmlNodeCost = nodeService.SelectSingleNode(xPathCost);
					if (htmlNodeCost == null) {
						Console.WriteLine("htmlNodeCost == null");
						continue;
					}

					ItemService itemService = new ItemService() {
						Name = SiteInfo.ClearString(htmlNodeName.InnerText),
						Price = SiteInfo.ClearString(htmlNodeCost.InnerText)
					};
					itemServiceGroup.ServiceItems.Add(itemService);
				} catch (Exception e) {
					Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				}
			}
		}

		private void ParseSiteSpbMedswissSpbRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathTable = "//div[contains(@class, 'inner-content')]//tbody";
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathTable);
			Console.WriteLine(itemServiceGroup.Name);

			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeCollectionService[0]);
			itemServiceGroup.ServiceItems.AddRange(serviceItems);
		}

		private void ParseSiteSpbGermanClinic(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			Thread.Sleep(10 * 1000);
			string xPathPrices = "//div[@class='prices']//tbody";
			HtmlNodeCollection nodeCollectionPrices = _htmlAgility.GetNodeCollection(docService, xPathPrices);
			Console.WriteLine(itemServiceGroup.Name);

			if (nodeCollectionPrices == null) {
				Console.WriteLine("nodeCollectionPrices is null");

				siteInfo.XPathServices = "//div[@class='service']//a[@href]";
				ParseSiteWithLinksOnMainPage(docService);
				
				return;
			}

			List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeCollectionPrices[0]);
			itemServiceGroup.ServiceItems.AddRange(serviceItems);
		}

		private void ParseSiteSpbBaltzdravRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathTable = "//div[starts-with(@class, 'wk-accordion')]";
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathTable);
			Console.WriteLine(itemServiceGroup.Name);

			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			ItemServiceGroup itemServiceCurrent = null;
			foreach (HtmlNode nodeInner in nodeCollectionService) {
				foreach (HtmlNode nodeChild in nodeInner.ChildNodes) {
				if (itemServiceCurrent == null && nodeChild.Name.ToLower().Equals("h3")) {
					itemServiceCurrent = new ItemServiceGroup() {
						Name = SiteInfo.ClearString(nodeChild.InnerText),
						Link = itemServiceGroup.Link
					};

					continue;
				}

				if (itemServiceCurrent != null && nodeChild.Name.ToLower().Equals("div")) {
					HtmlNodeCollection nodeCollectionTbody = nodeChild.SelectNodes(nodeChild.XPath + "//tbody");
					if (nodeCollectionTbody == null) {
						Console.WriteLine("nodeCollectionTbody == null");
						continue;
					}

					List<ItemService> serviceItems = ReadTrNodesFdoctorRu(nodeCollectionTbody[0], 1, 1);
					itemServiceCurrent.ServiceItems.AddRange(serviceItems);

					if (itemServiceCurrent.ServiceItems.Count > 0)
						siteInfo.ServiceGroupItems.Add(itemServiceCurrent);

					itemServiceCurrent = null;
					}
				}
			}
		}

		private void ParseSiteSpbEvroMedRu(HtmlDocument docService, ref ItemServiceGroup itemServiceGroup) {
			string xPathTable = "//table[@class='t-plainrows']/tbody";
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathTable);
			Console.WriteLine(itemServiceGroup.Name);

			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			foreach (HtmlNode node in nodeCollectionService) {
				int offsetName = 1;
				int offsetPrice = 1;

				if (itemServiceGroup.Name.ToLower().Contains("узи")) {
					offsetName = 0;
					offsetPrice = 0;
				} else if (itemServiceGroup.Name.ToLower().Contains("рефлексотерапия, мануальная терапия, массаж")) {
					offsetPrice = 2;
				}

				List<ItemService> serviceItems = ReadTrNodesFdoctorRu(node, offsetName, offsetPrice);
				itemServiceGroup.ServiceItems.AddRange(serviceItems);
			}
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

		private void ParseSiteOnClinic(HtmlDocument docServices, ref ItemServiceGroup itemServiceGroup, bool goDeep = true) {
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
						string serviceName = SiteInfo.ClearString(nodeLink.InnerText);
						_backgroundWorker.ReportProgress((int)progressCurrent, serviceName);
						Console.WriteLine("serviceName: " + serviceName);
						string hrefValue = nodeLink.Attributes["href"].Value;
						string urlService = siteInfo.UrlRoot + hrefValue;

						ItemServiceGroup itemServiceGroupInner = new ItemServiceGroup() { Name = serviceName, Link = urlService };
						HtmlDocument docService = _htmlAgility.GetDocument(urlService, siteInfo.Name);

						if (docService == null) {
							Console.WriteLine("docService == null");
							continue;
						}

						ParseSiteOnClinic(docService, ref itemServiceGroupInner, false);

						siteInfo.ServiceGroupItems.Add(itemServiceGroupInner);
					} catch (Exception e) {
						Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
					}
				}
			}
		}

		private void ParseSiteMedsiRu(HtmlDocument docService, ItemServiceGroup itemServiceGroupRoot) {
			string xPathClinicsId = "//*[starts-with(@class,'ui-select__nosearch')]/option";
			HtmlNodeCollection htmlNodeId = docService.DocumentNode.SelectNodes(xPathClinicsId);

			if (htmlNodeId == null) {
				Console.WriteLine("htmlNodeId == null");
				return;
			}

			Dictionary<string, string> idValues = new Dictionary<string, string>();
			foreach (HtmlNode nodeId in htmlNodeId) {
				try {
					string key = nodeId.Attributes["value"].Value;
					string value = SiteInfo.ClearString(nodeId.InnerText);

					if (!idValues.ContainsKey(key))
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
							string name = SiteInfo.ClearString(nodeRowPrice.SelectSingleNode("div[1]").InnerText);

							HtmlNode nodePrice = nodeRowPrice.SelectSingleNode("div[2]").SelectSingleNode("div[1]");
							HtmlNode nodePriceButton = nodePrice.SelectSingleNode("a[1]");

							string price;

							if (nodePriceButton != null)
								price = nodePriceButton.SelectSingleNode("i[1]").InnerText;
							else
								price = nodePrice.InnerText;

							ItemService itemService = new ItemService() {
								Name = name,
								Price = SiteInfo.ClearString(price)
							};

							itemServiceGroup.ServiceItems.Add(itemService);
						} catch (Exception priceExc) {
							Console.WriteLine(priceExc.Message + Environment.NewLine + priceExc.StackTrace);
						}
					}

					siteInfo.ServiceGroupItems.Add(itemServiceGroup);
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
							Name = SiteInfo.ClearString(name),
							Price = SiteInfo.ClearString(price)
						};

						itemServiceGroup.ServiceItems.Add(itemService);
					} catch (Exception e) {
						Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
					}
				}
			}


			string xPathDataService = "/html/body/div[3]/div/div[3]/div[4]";
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
						Name = SiteInfo.ClearString(name),
						Price = SiteInfo.ClearString(price)
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
			string xPathTable = "//div[@class='pediatrics__disease-row']";
			HtmlNodeCollection nodeCollectionService = _htmlAgility.GetNodeCollection(docService, xPathTable);

			if (nodeCollectionService == null) {
				Console.WriteLine("nodeCollectionService is null");
				return;
			}

			foreach (HtmlNode htmlNodeService in nodeCollectionService) {
				try {
					ItemService itemService = new ItemService() {
						Name = SiteInfo.ClearString(htmlNodeService.ChildNodes[3].InnerText).Remove(0, 6).TrimStart(' '),
						Price = htmlNodeService.ChildNodes[5].InnerText
					};
					itemServiceGroup.ServiceItems.Add(itemService);
				} catch (Exception e) {
					Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				}
			}
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

				siteInfo.XPathServices = "//*[@id=\"serv_list_content\"]//a[@href]";
				ParseSiteWithLinksOnMainPage(docService);

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
							siteInfo.UrlRoot +
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

					if (siteInfo.Name == SiteInfo.SiteName.spb_evro_med_ru) {
						HtmlNodeCollection nodeCollectionPName = nodeTd[nameOffset].SelectNodes("p");
						HtmlNodeCollection nodeCollectionPPrice = nodeTd[1 + priceOffset].SelectNodes("p");

						if (nodeCollectionPName != null) {
							string rootName = string.Empty;
							int pNameOffset = 0;
							int pPriceOffset = 0;

							if (SiteInfo.ClearString(nodeCollectionPName.Last().InnerText).StartsWith("-")) {
								rootName = SiteInfo.ClearString(nodeCollectionPName.First().InnerText);
								pNameOffset = 1;
								pPriceOffset = 1;
							}

							for (int i = pNameOffset; i < nodeCollectionPName.Count; i++) {
								string currentName = SiteInfo.ClearString(nodeCollectionPName[i].InnerText);
								string nameInner = string.IsNullOrEmpty(rootName) ? currentName : rootName + " " + currentName;
								string priceInner = SiteInfo.ClearString(nodeCollectionPPrice[pPriceOffset + i].InnerText);

								items.Add(new ItemService() {
									Name = nameInner,
									Price = priceInner
								});
							}

							continue;
						}
					}


					string nameRaw = string.Empty;
					string priceRaw = string.Empty;

					if (siteInfo.Name == SiteInfo.SiteName.yekuk_ruslabs_ru) {
						if (nodeTd.Count == 3) {
							nameOffset = 0;
							priceOffset = 0;
						} else {
							nameOffset = 1;
							priceOffset = 2;
						}
					}

					if (nodeTd.Count > 0 + nameOffset) {
						nameRaw = nodeTd[0 + nameOffset].InnerText;
					}

					if (nodeTd.Count > 1 + priceOffset) {
						if (siteInfo.Name == SiteInfo.SiteName.cmd_online_ru) {
							priceRaw = nodeTd[1 + priceOffset].ChildNodes[3].InnerText;
						} else
							priceRaw = nodeTd[1 + priceOffset].InnerText;
					}

					if (siteInfo.Name == SiteInfo.SiteName.yekuk_immunoresurs_ru) {
						if (nameRaw.Contains("\n") && priceRaw.Contains("\n")) {
							string[] names = nameRaw.Split(new string[] { "\n" }, StringSplitOptions.None);
							string[] prices = priceRaw.Split(new string[] { "\n" }, StringSplitOptions.None);

							for (int i = 0; i < names.Length; i++) {
								ItemService itemServiceInner = new ItemService() {
									Name = SiteInfo.ClearString(names[i]),
									Price = SiteInfo.ClearString(prices[i])
								};

								items.Add(itemServiceInner);
							}

							continue;
						}
					}

					string name = SiteInfo.ClearString(nameRaw);
					string price = SiteInfo.ClearString(priceRaw);

					if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(price))
						continue;
					
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
	}
}
