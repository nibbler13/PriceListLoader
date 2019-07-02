using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceListLoader.RegionParsers {
    class ParseKazan : ParseGeneral {
        public ParseKazan(HtmlAgility htmlAgility, BackgroundWorker bworker, SiteInfo siteInfo) : base(htmlAgility, bworker, siteInfo) { }

        public void ParseSiteKazanKazanClinicRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathServices = "//div[@class='module-prices']//div[@class='item']";
            HtmlNodeCollection nodeCollectionServices = docService.DocumentNode.SelectNodes(xPathServices);

            if (nodeCollectionServices == null) {
                Console.WriteLine("nodeCollectionServices == null");
                return;
            }

            foreach (HtmlNode nodeService in nodeCollectionServices) {
                HtmlNode nodeName = nodeService.SelectSingleNode(nodeService.XPath + "//div[@class='item-title']");
                HtmlNode nodePrice = nodeService.SelectSingleNode(nodeService.XPath + "//div[@class='price']");

                if (nodeName == null || nodePrice == null)
                    continue;

                string name = ClearString(nodeName.InnerText);
                string price = ClearString(nodePrice.InnerText);

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(price))
                    continue;

                Items.Service itemService = new Items.Service() {
                    Name = name,
                    Price = price
                };

                itemServiceGroup.ServiceItems.Add(itemService);
            }
        }

        public void ParseSiteKazanMedexpertKazanRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTbodies = "//div[@class='price-table']//tbody";
            HtmlNodeCollection nodeCollectionTbodies = htmlAgility.GetNodeCollection(docService, xPathTbodies);
            if (nodeCollectionTbodies == null) {
                Console.WriteLine("nodeCollectionTbodies == null");
                return;
            }

            foreach (HtmlNode nodeTbody in nodeCollectionTbodies) {
                List<Items.Service> serviceItems = ReadTrNodes(nodeTbody);
                itemServiceGroup.ServiceItems.AddRange(serviceItems);
            }
        }

        public void ParseSiteKazanLoveDrRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string groupName = itemServiceGroup.Name.ToLower();
            if (groupName.Equals("главная") ||
                groupName.Equals("цены"))
                return;

            if (groupName.Equals("массаж")) {
                ParseSiteWithLinksOnMainPage(docService);
                return;
            }

            string xPath = "//td[@class='main-column']//tbody";
            HtmlNodeCollection nodeCollectionTbody = htmlAgility.GetNodeCollection(docService, xPath);
            if (nodeCollectionTbody == null) {
                Console.WriteLine("nodeCollectionTbody == null");
                return;
            }

            foreach (HtmlNode nodeTbody in nodeCollectionTbody) {
                HtmlNodeCollection nodeCollectionTr = nodeTbody.SelectNodes(nodeTbody.XPath + "//tr");
                if (nodeCollectionTr == null) {
                    Console.WriteLine("nodeCollectionTr == null");
                    continue;
                }

                string serviceNamePostfix = string.Empty;
                foreach (HtmlNode nodeTr in nodeCollectionTr) {
                    HtmlNodeCollection nodeCollectionTd = nodeTr.SelectNodes(nodeTr.XPath + "//td");
                    if (nodeCollectionTd == null) {
                        Console.WriteLine("nodeCollectionTd == null");
                        continue;
                    }

                    if (nodeCollectionTd.Count == 1) {
                        string tdInnerText = ClearString(nodeCollectionTd[0].InnerText).ToLower();
                        if (tdInnerText.Contains("дети") ||
                            tdInnerText.Contains("взрослые"))
                            serviceNamePostfix = " - " + tdInnerText;
                    }

                    if (nodeCollectionTd.Count != 2)
                        continue;

                    string serviceName = ClearString(nodeCollectionTd[0].InnerText);
                    string servicePrice = ClearString(nodeCollectionTd[1].InnerText);

                    if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(servicePrice))
                        continue;

                    Items.Service itemService = new Items.Service() {
                        Name = serviceName + serviceNamePostfix,
                        Price = servicePrice
                    };

                    itemServiceGroup.ServiceItems.Add(itemService);
                }
            }

        }

        public void ParseSiteKazanStarclinicRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPath = "//div[@class='news-detail']//tbody";
            HtmlNodeCollection nodeCollection = htmlAgility.GetNodeCollection(docService, xPath);
            if (nodeCollection == null) {
                Console.WriteLine("nodeCollection == null");
                return;
            }

            foreach (HtmlNode nodeTbody in nodeCollection)
                itemServiceGroup.ServiceItems.AddRange(ReadTrNodes(nodeTbody));
        }

        public void ParseSiteKazanZdorovie7iRu(HtmlDocument docService) {
			HtmlNodeCollection nodeCollection = htmlAgility.GetNodeCollection(docService, siteInfo.XPathServices);
			if (nodeCollection == null) {
				Console.WriteLine("nodeCollection == null");
				return;
			}

			foreach (HtmlNode nodeRoot in nodeCollection) {
				HtmlNode nodeRootGroupName = nodeRoot.SelectSingleNode(nodeRoot.XPath + "//div[starts-with(@class,'doctor-item-header')]//div[@class='doctor-item__title']");
				if (nodeRootGroupName == null) {
					Console.WriteLine("nodeRootGroupName == null");
					continue;
				}

				string rootGroupName = ClearString(nodeRootGroupName.InnerText);
				backgroundWorker.ReportProgress((int)progressCurrent, rootGroupName);
				Items.ServiceGroup serviceGroup = new Items.ServiceGroup {
					Link = siteInfo.UrlServicesPage,
					Name = rootGroupName
				};

				string xPathInnerGroup = "//div[@class='doctor-elems']/div[@class='doctor-elem']";
				HtmlNodeCollection nodeCollectionInnerGroup = nodeRoot.SelectNodes(nodeRoot.XPath + xPathInnerGroup);
				if (nodeCollectionInnerGroup == null) {
					Console.WriteLine("nodeCollectionInnerGroup == null");
					continue;
				}

				foreach (HtmlNode nodeInner in nodeCollectionInnerGroup) {
					string innerGroupName = string.Empty;

					HtmlNode nodeInnerGroupName = nodeInner.SelectSingleNode(nodeInner.XPath + "/div[@class='doctor-elem__title']");
					if (nodeInnerGroupName != null)
						innerGroupName = ClearString(nodeInnerGroupName.InnerText);

					Items.ServiceGroup serviceGroupInner;
					if (string.IsNullOrEmpty(innerGroupName))
						serviceGroupInner = serviceGroup;
					else
						serviceGroupInner = new Items.ServiceGroup {
							Link = serviceGroup.Link,
							Name = serviceGroup.Name + " - " + innerGroupName
						};

					backgroundWorker.ReportProgress((int)progressCurrent, serviceGroupInner.Name);

					string xPathService = "//ul[@class='doctor-elem-item js-calc-pos']";
					HtmlNodeCollection nodeCollectionServices = nodeInner.SelectNodes(nodeInner.XPath + xPathService);
					if (nodeCollectionServices == null) {
						Console.WriteLine("nodeCollectionServices == null");
						continue;
					}

					string xPathServiceName = "//label[@class='js-calc-pos-name']";
					string xPathServiceNameExclude = "//div[@class='serice-item-info']";
					string xPathServiceCost = "/li[2]";
					foreach (HtmlNode nodeService in nodeCollectionServices) {
						HtmlNode nodeServiceName = nodeService.SelectSingleNode(nodeService.XPath + xPathServiceName);
						HtmlNode nodeServiceNameExclude = nodeService.SelectSingleNode(nodeService.XPath + xPathServiceNameExclude);
						HtmlNode nodeServiceCost = nodeService.SelectSingleNode(nodeService.XPath + xPathServiceCost);

						if (nodeServiceName == null || nodeServiceCost == null) {
							Console.WriteLine("nodeServiceName == null || nodeServiceCost == null");
							continue;
						}

						string serviceName = nodeServiceName.InnerText;
						if (nodeServiceNameExclude != null) 
							serviceName = serviceName.Replace(nodeServiceNameExclude.InnerText, "");
						serviceName = ClearString(serviceName);

						string serviceCost = ClearString(nodeServiceCost.InnerText);

						Items.Service service = new Items.Service {
							Name = serviceName,
							Price = serviceCost
						};

						serviceGroupInner.ServiceItems.Add(service);
					}

					siteInfo.ServiceGroupItems.Add(serviceGroupInner);
				}
			}






            //Console.WriteLine(itemServiceGroup.Name);

            //if (itemServiceGroup.Name.Equals("Анализы")) {
            //    siteInfo.XPathServices = "//div[@class='test-list2']//a[@href]";
            //    ParseSiteWithLinksOnMainPage(docService);
            //    return;
            //}

            //string xPathUlPriceList = "//ul[@class='pricelist']//li";
            //HtmlNodeCollection nodeCollectionLi = htmlAgility.GetNodeCollection(docService, xPathUlPriceList);

            //if (nodeCollectionLi != null)
            //    itemServiceGroup.ServiceItems.AddRange(ParseLiItemsForKazanZdorovie7iRu(nodeCollectionLi));

            //string xPathUslugiTbody = "//div[@class='uslugi']//tbody";
            //HtmlNodeCollection nodeCollectionTbody = htmlAgility.GetNodeCollection(docService, xPathUslugiTbody);

            //if (nodeCollectionTbody != null) {
            //    foreach (HtmlNode nodeTbody in nodeCollectionTbody) {
            //        itemServiceGroup.ServiceItems.AddRange(ReadTrNodes(nodeTbody));
            //    }
            //}

            //if (itemServiceGroup.Name.Equals("Онкомаркеры"))
            //    Console.WriteLine();

            //string xPathLab = "//ul[@class='mainanalyses']//li";
            //HtmlNodeCollection nodeCollectionLab = htmlAgility.GetNodeCollection(docService, xPathLab);

            //if (nodeCollectionLab != null) {
            //    foreach (HtmlNode nodeLi in nodeCollectionLab) {
            //        HtmlNode nodeServiceName = nodeLi.SelectSingleNode(nodeLi.XPath + "//span[@class='pr-name']");
            //        HtmlNode nodeServicePrice = nodeLi.SelectSingleNode(nodeLi.XPath + "//span[@class='pr']");

            //        if (nodeServiceName == null ||
            //            nodeServicePrice == null) {
            //            continue;
            //        }

            //        Items.Service itemService = new Items.Service() {
            //            Name = ClearString(nodeServiceName.InnerText),
            //            Price = ClearString(nodeServicePrice.InnerText)
            //        };

            //        itemServiceGroup.ServiceItems.Add(itemService);
            //    }
            //}

            //string xPathSpoilerDental = "//div[@class='item-page']//div[@class='spoiler']";
            //HtmlNodeCollection nodeCollectionSpoilersSental = htmlAgility.GetNodeCollection(docService, xPathSpoilerDental);

            //if (nodeCollectionSpoilersSental != null) {
            //    foreach (HtmlNode nodeSpoilerDental in nodeCollectionSpoilersSental) {
            //        HtmlNode nodeGroupName = nodeSpoilerDental.SelectSingleNode(nodeSpoilerDental.XPath + "/p[@class='spoiler-text']");
            //        if (nodeGroupName == null)
            //            nodeGroupName = nodeSpoilerDental.SelectSingleNode(nodeSpoilerDental.XPath + "/div[@class='spoiler-text']");

            //        if (nodeGroupName == null) {
            //            Console.WriteLine("nodeGroupName == null");
            //            continue;
            //        }

            //        Items.ServiceGroup itemServiceGroupInner = new Items.ServiceGroup() {
            //            Name = itemServiceGroup.Name + " - " + ClearString(nodeGroupName.InnerText),
            //            Link = itemServiceGroup.Link
            //        };

            //        string xPathTbody = nodeSpoilerDental.XPath + "//tbody";
            //        HtmlNodeCollection nodeCollectionTbodies = nodeSpoilerDental.SelectNodes(xPathTbody);
            //        if (nodeCollectionTbodies == null) {
            //            Console.WriteLine("nodeCollectionTbody == null");
            //            continue;
            //        }

            //        foreach (HtmlNode nodeTbody in nodeCollectionTbodies)
            //            itemServiceGroupInner.ServiceItems.AddRange(ReadTrNodes(nodeTbody, 1, 2));

            //        siteInfo.ServiceGroupItems.Add(itemServiceGroupInner);
            //    }
            //}

            //string xPathSpoilerMassage = "//div[@class='uslugi']//div[@class='spoiler']";
            //HtmlNodeCollection nodeCollectionSpoilersMassage = htmlAgility.GetNodeCollection(docService, xPathSpoilerMassage);

            //if (nodeCollectionSpoilersMassage != null) {
            //    foreach (HtmlNode nodeSpoilerMassage in nodeCollectionSpoilersMassage) {
            //        HtmlNode nodeGroupName = nodeSpoilerMassage.SelectSingleNode(nodeSpoilerMassage.XPath + "//div[@class='spoiler-text']");
            //        if (nodeGroupName == null) {
            //            Console.WriteLine("nodeGroupName == null");
            //            continue;
            //        }

            //        Items.ServiceGroup itemServiceGroupInner = new Items.ServiceGroup() {
            //            Name = itemServiceGroup.Name + " - " + ClearString(nodeGroupName.InnerText),
            //            Link = itemServiceGroup.Link
            //        };

            //        HtmlNodeCollection nodeCollectionLiMassage =
            //            nodeSpoilerMassage.SelectNodes(nodeSpoilerMassage.XPath + "//ul[@class='pricelist']//li");
            //        if (nodeCollectionLiMassage == null) {
            //            Console.WriteLine("nodeCollectionLiMassage == null");
            //            continue;
            //        }

            //        itemServiceGroupInner.ServiceItems.AddRange(ParseLiItemsForKazanZdorovie7iRu(nodeCollectionLiMassage));
            //        siteInfo.ServiceGroupItems.Add(itemServiceGroupInner);
            //    }
            //}
        }

        private List<Items.Service> ParseLiItemsForKazanZdorovie7iRu(HtmlNodeCollection nodeCollectionLi) {
            List<Items.Service> serviceItems = new List<Items.Service>();
            string previousLiText = string.Empty;

            for (int i = nodeCollectionLi.Count - 1; i >= 0; i--) {
                HtmlNode nodeLi = nodeCollectionLi[i];
                string nodeLiInnerText = nodeLi.InnerText;
                if (!string.IsNullOrEmpty(previousLiText))
                    nodeLiInnerText = nodeLiInnerText.Replace(previousLiText, "");

                previousLiText = nodeLi.InnerText;

                string xPathPrice = nodeLi.XPath + "/span";
                HtmlNode nodePrice = nodeLi.SelectSingleNode(xPathPrice);
                if (nodePrice == null) {
                    Console.WriteLine("nodePrice == null");
                    continue;
                }

                string priceRaw = nodePrice.InnerText;
                string name = ClearString(nodeLiInnerText.Replace(priceRaw, ""));
                Items.Service itemService = new Items.Service() {
                    Name = name,
                    Price = ClearString(priceRaw)
                };

                serviceItems.Add(itemService);
            }

            return serviceItems;
        }

        public void ParseSiteKazanBiomedMcRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            Console.WriteLine(itemServiceGroup.Name);

            HtmlNodeCollection nodeCollectionInnerLinks = htmlAgility.GetNodeCollection(docService, siteInfo.XPathServices);
            if (nodeCollectionInnerLinks != null &&
                itemServiceGroup.Name.ToUpper().Equals("ПРИЁМ СПЕЦИАЛИСТОВ") ||
                itemServiceGroup.Name.ToUpper().Equals("ЛАБОРАТОРНЫЕ ИССЛЕДОВАНИЯ"))
                ParseSiteWithLinksOnMainPage(docService);

            string[] xPathTables = new string[] {
                "//div[starts-with(@class,'mainalltext')]//table",
                "//div[starts-with(@class,'textinside')]//table"
            };

            foreach (string xPathTable in xPathTables) {
                HtmlNodeCollection nodeCollectionTables = htmlAgility.GetNodeCollection(docService, xPathTable);
                if (nodeCollectionTables == null) {
                    Console.WriteLine("nodeCollectionTables == null");
                    continue;
                }

                foreach (HtmlNode nodeTable in nodeCollectionTables) {
                    if (nodeTable.Attributes.Contains("class") &&
                        nodeTable.Attributes["class"].Value.Equals("disalow"))
                        continue;

                    HtmlNode htmlNodeTrs = null;

                    HtmlNode htmlNodeTbody = nodeTable.SelectSingleNode(nodeTable.XPath + "//tbody");
                    if (htmlNodeTbody != null)
                        htmlNodeTrs = htmlNodeTbody;
                    else
                        htmlNodeTrs = nodeTable;

                    itemServiceGroup.ServiceItems.AddRange(ReadTrNodes(htmlNodeTrs));
                }
            }
        }

        public void ParseSiteKazanMcAybolitRy(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            siteInfo.XPathServices = "//div[@class='content']//a[@href]";
            HtmlNodeCollection nodeCollectionInnerLinks = htmlAgility.GetNodeCollection(docService, siteInfo.XPathServices);
            Console.WriteLine(itemServiceGroup.Name);

            if (nodeCollectionInnerLinks != null)
                ParseSiteWithLinksOnMainPage(docService);

            string xPathTbody = "//div[@class='content']//tbody";
            HtmlNodeCollection htmlNodesTbody = htmlAgility.GetNodeCollection(docService, xPathTbody);

            if (htmlNodesTbody != null) {
                int nameOffset = 1;
                int priceOffset = 1;

                string groupName = itemServiceGroup.Name.ToUpper();

                if (groupName.Equals("ЛОГОПЕД") || groupName.Equals("ЭНДОСКОПИЯ")) {
                    nameOffset = 0;
                    priceOffset = 0;
                }

                if (!groupName.Equals("СРЕДНИЙ МЕДИЦИНСКИЙ ПЕРСОНАЛ") &&
                    !groupName.Equals("ОБСЛУЖИВАНИЕ  ПО ДМС"))
                    foreach (HtmlNode nodeTbody in htmlNodesTbody)
                        itemServiceGroup.ServiceItems.AddRange(ReadTrNodes(nodeTbody, nameOffset, priceOffset));
            }

            string xPathLab = "//div[starts-with(@class,'calclist')]";
            HtmlNodeCollection htmlNodesLabs = htmlAgility.GetNodeCollection(docService, xPathLab);
            if (htmlNodesLabs != null) {
                foreach (HtmlNode nodeLab in htmlNodesLabs) {
                    string xPathGroupName = nodeLab.XPath + "//div[@class='calcin']";
                    HtmlNode nodeGroupName = nodeLab.SelectSingleNode(xPathGroupName);
                    if (nodeGroupName == null) {
                        Console.WriteLine(nodeGroupName == null);
                        continue;
                    }

                    Items.ServiceGroup itemServiceGroupInner = new Items.ServiceGroup() {
                        Name = itemServiceGroup.Name + " - " + ClearString(nodeGroupName.InnerText),
                        Link = itemServiceGroup.Link
                    };

                    string xPathRows = nodeLab.XPath + "//div[@class='rowitemwrap']";
                    HtmlNodeCollection nodeCollectionRows = nodeLab.SelectNodes(xPathRows);
                    if (nodeCollectionRows == null) {
                        Console.WriteLine("nodeCollectionRows == null");
                        continue;
                    }

                    foreach (HtmlNode nodeRow in nodeCollectionRows) {
                        string xPathServiceName = nodeRow.XPath + "//div[@class='colname w66']";
                        string xPathServicePrice = nodeRow.XPath + "//span[@class='cprice']";
                        HtmlNode nodeServiceName = nodeRow.SelectSingleNode(xPathServiceName);
                        HtmlNode nodeServicePrice = nodeRow.SelectSingleNode(xPathServicePrice);
                        if (nodeServiceName == null || nodeServicePrice == null) {
                            Console.WriteLine("nodeServiceName == null || nodeServicePrice == null");
                            continue;
                        }

                        Items.Service itemService = new Items.Service() {
                            Name = ClearString(nodeServiceName.InnerText),
                            Price = ClearString(nodeServicePrice.InnerText)
                        };

                        itemServiceGroupInner.ServiceItems.Add(itemService);
                    }

                    siteInfo.ServiceGroupItems.Add(itemServiceGroupInner);
                }
            }
        }

        public void ParseSiteKazanAvaKazanRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup, bool goDeeper = true) {
            string xPathInnerLinks = "//ul[@class='ul-marker']//a[@href]";
            HtmlNodeCollection nodeCollectionsInnerLinks = htmlAgility.GetNodeCollection(docService, xPathInnerLinks);

            if (nodeCollectionsInnerLinks != null && goDeeper)
                foreach (HtmlNode nodeInnerLink in nodeCollectionsInnerLinks) {
                    siteInfo.XPathServices = xPathInnerLinks;
                    siteInfo.UrlServicesPage = siteInfo.UrlRoot + nodeInnerLink.Attributes["href"].Value;
                    ParseSiteWithLinksOnMainPage(docService, false);
                }

            string xPathServices = "//div[starts-with(@class,'price')]";
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docService, xPathServices);
            Console.WriteLine(itemServiceGroup.Name);

            if (nodeCollectionServices == null) {
                Console.WriteLine();
                return;
            }

            foreach (HtmlNode nodeService in nodeCollectionServices) {
                try {
                    HtmlNode nodeGroupSubName = nodeService.SelectSingleNode(nodeService.XPath + "//h2");
                    if (nodeGroupSubName == null) {
                        Console.WriteLine("nodeGroupSubName == null");
                        continue;
                    }

                    Items.ServiceGroup itemServiceGroupInner = new Items.ServiceGroup() {
                        Name = itemServiceGroup.Name + " - " + ClearString(nodeGroupSubName.InnerText),
                        Link = itemServiceGroup.Link
                    };

                    HtmlNodeCollection htmlNodesTbody = nodeService.SelectNodes(nodeService.XPath + "//tbody");
                    if (htmlNodesTbody == null) {
                        Console.WriteLine("htmlNodesTbody == null");
                        continue;
                    }

                    foreach (HtmlNode nodeTbody in htmlNodesTbody)
                        itemServiceGroupInner.ServiceItems.AddRange(ReadTrNodes(nodeTbody));

                    if (itemServiceGroupInner.ServiceItems.Count > 0)
                        siteInfo.ServiceGroupItems.Add(itemServiceGroupInner);
                } catch (Exception e) {
                    Console.WriteLine(e.Message + Environment.StackTrace + e.StackTrace);
                }
            }
        }

    }
}
