using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceListLoader.RegionParsers {
    class ParseMoscow : ParseGeneral {
        public ParseMoscow(HtmlAgility htmlAgility, BackgroundWorker bworker, SiteInfo siteInfo) : base(htmlAgility, bworker, siteInfo) { }

        public void ParseSiteSmClinicRuLab(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollectionServices == null) {
                Console.WriteLine("nodeCollectionServices is null");
                return;
            }

            foreach (HtmlNode node in nodeCollectionServices) {
                string blockName = string.Empty;
                HtmlNode nodeBlockName = node.SelectSingleNode(node.XPath + "/div[@class='panel-heading']");
                if (nodeBlockName != null)
                    blockName = ClearString(nodeBlockName.InnerText);

                Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
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
                        string itemCost = ClearString(nodeItem.ChildNodes[1].Attributes["data-price"].Value);
                        string itemName = ClearString(nodeItem.ChildNodes[2].InnerText);
                        itemServiceGroup.ServiceItems.Add(new Items.Service() {
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

        public void ParseSiteSmDoctorRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
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
                        Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
                            Name = sectionName,
                            Link = siteInfo.UrlServicesPage
                        };

                        HtmlNode nodeTableBody = nodeLi.SelectSingleNode(nodeLi.XPath + "//table[@class='price-list']/tbody");
                        if (nodeTableBody == null) {
                            Console.WriteLine("nodeTableBody == null");
                            continue;
                        }

                        List<Items.Service> serviceItems = ReadTrNodes(nodeTableBody);
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
                    Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
                        Name = "Детская хирургия",
                        Link = siteInfo.UrlServicesPage
                    };

                    HtmlNode nodeTableBody = nodeGroup.SelectSingleNode(nodeGroup.XPath + "//table[@class='price-list']/tbody");
                    if (nodeTableBody == null) {
                        Console.WriteLine("nodeTableBody == null");
                        continue;
                    }

                    try {
                        List<Items.Service> serviceItems = ReadTrNodes(nodeTableBody);
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

        public void ParseSiteMasterdentRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollectionServices == null) {
                Console.WriteLine("nodeCollectionServices is null");
                return;
            }

            Items.ServiceGroup itemServiceGroup = null;
            foreach (HtmlNode nodeTr in nodeCollectionServices) {
                HtmlNodeCollection nodeCollectionTd = nodeTr.SelectNodes(nodeTr.XPath + "//td");
                if (nodeCollectionTd == null) {
                    Console.WriteLine("nodeCollectionTd == null");
                    continue;
                }

                if (nodeCollectionTd.Count < 3) {
                    Console.WriteLine("nodeCollectionTd.Count < 3");
                    continue;
                }

                string serviceCode = ClearString(nodeCollectionTd[0].InnerText);
                string serviceName = ClearString(nodeCollectionTd[1].InnerText);
                string servicePrice = ClearString(nodeCollectionTd[2].InnerText);

                if (string.IsNullOrWhiteSpace(serviceCode) || string.IsNullOrEmpty(serviceCode)) {
                    if (itemServiceGroup != null)
                        siteInfo.ServiceGroupItems.Add(itemServiceGroup);

                    itemServiceGroup = new Items.ServiceGroup() {
                        Name = serviceName,
                        Link = siteInfo.UrlServicesPage
                    };
                    continue;
                }

                if (itemServiceGroup == null)
                    continue;

                Items.Service itemService = new Items.Service() {
                    Name = serviceName,
                    Price = servicePrice
                };
                itemServiceGroup.ServiceItems.Add(itemService);
            }

            if (itemServiceGroup != null)
                siteInfo.ServiceGroupItems.Add(itemServiceGroup);
        }

        public void ParseSiteNovostomRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollectionServices == null) {
                Console.WriteLine("nodeCollectionServices is null");
                return;
            }

            foreach (HtmlNode node in nodeCollectionServices) {
                Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
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

                        Items.Service itemService = new Items.Service() {
                            Name = ClearString(name)
                        };

                        if (htmlNodeCollectionTd.Count >= 2) {
                            string price = htmlNodeCollectionTd[1].InnerText;
                            itemService.Price = ClearString(price);
                        }

                        itemServiceGroup.ServiceItems.Add(itemService);
                    }
                }

                siteInfo.ServiceGroupItems.Add(itemServiceGroup);
            }
        }

        public void ParseSiteVseSvoiRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
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

                    Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
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
                                Items.Service itemService = new Items.Service() {
                                    Name = name,
                                    Price = ClearString(price)
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

        public void ParseSiteMrt24Ru(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollectionServices == null) {
                Console.WriteLine("nodeCollectionServices is null");
                return;
            }

            foreach (HtmlNode node in nodeCollectionServices) {
                try {
                    HtmlNode nodeServiceName = node.SelectSingleNode("div[1]/div[1]");
                    string serviceName = nodeServiceName.InnerText;
                    backgroundWorker.ReportProgress((int)progressCurrent, serviceName);
                    string nodeClassName = node.Attributes["class"].Value;

                    Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() { Name = serviceName };

                    HtmlNodeCollection nodeServices = node.SelectNodes("//div[@class='" + nodeClassName + "']//div[@class='group_container']");
                    if (nodeServices == null) {
                        Console.WriteLine("nodeServices == null");
                        continue;
                    }

                    foreach (HtmlNode nodeService in nodeServices) {
                        HtmlNode nodeItemServiceName = nodeService.SelectSingleNode("div[1]/div[1]");
                        string itemServiceName = nodeItemServiceName.InnerText;
                        backgroundWorker.ReportProgress((int)progressCurrent, itemServiceName);

                        HtmlNodeCollection nodesCol2InnerCol1 = nodeService.SelectNodes("div[2]/div[1]/div[1]/span");

                        if (nodesCol2InnerCol1 != null) {
                            string prefix = ", Закрытый контур, ";

                            if (nodesCol2InnerCol1.Count >= 1) {
                                string title = nodesCol2InnerCol1[0].ChildNodes[0].InnerText;
                                string price = nodesCol2InnerCol1[0].ChildNodes[1].InnerText;
                                string name = itemServiceName + prefix + title;
                                Items.Service itemService = new Items.Service() {
                                    Name = name,
                                    Price = price
                                };
                                itemServiceGroup.ServiceItems.Add(itemService);
                            }

                            if (nodesCol2InnerCol1.Count >= 2) {
                                string title = nodesCol2InnerCol1[1].ChildNodes[0].InnerText;
                                string price = nodesCol2InnerCol1[1].ChildNodes[1].InnerText;
                                string name = itemServiceName + prefix + title;
                                Items.Service itemService = new Items.Service() {
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
                                Items.Service itemService = new Items.Service() {
                                    Name = name,
                                    Price = price
                                };
                                itemServiceGroup.ServiceItems.Add(itemService);
                            }

                            if (nodesCol2InnerCol2.Count >= 2) {
                                string title = nodesCol2InnerCol2[1].ChildNodes[0].InnerText;
                                string price = nodesCol2InnerCol2[1].ChildNodes[1].InnerText;
                                string name = itemServiceName + prefix + title;
                                Items.Service itemService = new Items.Service() {
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
                                Items.Service itemService = new Items.Service() {
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
                                Items.Service itemService = new Items.Service() {
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
                                Items.Service itemService = new Items.Service() {
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
                                Items.Service itemService = new Items.Service() {
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

        public void ParseSiteAlfazdrav(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollectionServices == null) {
                Console.WriteLine("nodeCollectionServices is null");
                return;
            }

            foreach (HtmlNode node in nodeCollectionServices) {
                try {
                    HtmlNode nodeHead = node.ChildNodes[1];
                    string departmentName = nodeHead.InnerText;
                    backgroundWorker.ReportProgress((int)progressCurrent, departmentName);

                    HtmlNodeCollection nodeCollectionGroups = node.ChildNodes[3].SelectNodes("li");
                    if (nodeCollectionGroups == null) {
                        Console.WriteLine("nodeCollectionGroups == null");
                        continue;
                    }

                    foreach (HtmlNode nodeGroup in nodeCollectionGroups) {
                        HtmlNode nodeServiceGroup = nodeGroup.ChildNodes[1];
                        string groupName = nodeServiceGroup.InnerText;
                        backgroundWorker.ReportProgress((int)progressCurrent, groupName);

                        Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() { Name = departmentName + ", " + groupName, Link = siteInfo.UrlServicesPage };

                        HtmlNodeCollection nodeTables = nodeGroup.SelectNodes(nodeGroup.XPath + "//table");

                        if (nodeTables == null) {
                            Console.WriteLine("nodeTables == null");
                            continue;
                        }

                        foreach (HtmlNode nodeTable in nodeTables) {
                            List<Items.Service> serviceItems = ReadTrNodes(nodeTable.LastChild);

                            if (serviceItems.Count == 0) {
                                backgroundWorker.ReportProgress((int)progressCurrent, "Услуг не обнаружено, пропуск");
                                continue;
                            }

                            itemServiceGroup.ServiceItems.AddRange(serviceItems);
                        }

                        siteInfo.ServiceGroupItems.Add(itemServiceGroup);
                    }
                } catch (Exception e) {
                    backgroundWorker.ReportProgress((int)progressCurrent, e.Message + Environment.NewLine + e.StackTrace);
                }
            }
        }

        public void ParseSiteSmStomatologyRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTable = "//table[@class='table b-table']/tbody";
            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathTable);
            Console.WriteLine(itemServiceGroup.Name);

            if (nodeCollectionService == null) {
                Console.WriteLine("nodeCollectionService is null");
                return;
            }

            foreach (HtmlNode node in nodeCollectionService) {
                List<Items.Service> serviceItems = ReadTrNodes(node);
                itemServiceGroup.ServiceItems.AddRange(serviceItems);
            }
        }

        public void ParseSiteNrLabRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathDivFortitle = "//div[@class='in-medcenter__title fortitle']";
            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathDivFortitle);

            string xPathServiceName = "//div[@class='in-medcenter__title-count bluetitle']";
            string xPathServicePrice = "//div[@class='in-medcenter__title-price']";

            if (nodeCollectionService != null)
                itemServiceGroup.ServiceItems.AddRange(ParseNrlabRuRows(nodeCollectionService, xPathServiceName, xPathServicePrice));

            string xPathAnalysis = "//div[@class='in-medcenter__title with-analysis']";
            nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathAnalysis);

            xPathServiceName = "//div[@class='in-medcenter__title-count']//a";
            if (nodeCollectionService != null)
                itemServiceGroup.ServiceItems.AddRange(ParseNrlabRuRows(nodeCollectionService, xPathServiceName, xPathServicePrice));
        }

        private List<Items.Service> ParseNrlabRuRows(HtmlNodeCollection nodeCollection, string xPathServiceName, string xPathServicePrice) {
            List<Items.Service> serviceItems = new List<Items.Service>();

            foreach (HtmlNode node in nodeCollection) {
                HtmlNode nodeServiceName = node.SelectSingleNode(node.XPath + xPathServiceName);
                HtmlNode nodeServicePrice = node.SelectSingleNode(node.XPath + xPathServicePrice);

                if (nodeServiceName == null ||
                    nodeServicePrice == null) {
                    Console.WriteLine("nodeServiceName == null || nodeServicePrice == null");
                    continue;
                }

                string serviceName = ClearString(nodeServiceName.InnerText);
                string servicePrice = ClearString(nodeServicePrice.InnerText);

                if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(servicePrice))
                    continue;

                Items.Service itemService = new Items.Service() {
                    Name = serviceName,
                    Price = servicePrice
                };

                serviceItems.Add(itemService);
            }

            return serviceItems;
        }

        public void ParseSiteOnClinic(HtmlDocument docServices, ref Items.ServiceGroup itemServiceGroup, bool goDeep = true) {
            string xPathTable = "//div[@class='price-holder tbl-sm-font']";
            HtmlNodeCollection nodeCollectionPriceTable = htmlAgility.GetNodeCollection(docServices, xPathTable);
            if (nodeCollectionPriceTable != null) {
                try {
                    List<Items.Service> serviceItems = ReadTrNodes(nodeCollectionPriceTable[0].ChildNodes[1].ChildNodes[1]);
                    itemServiceGroup.ServiceItems = serviceItems;
                } catch (Exception e) {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            if (!goDeep)
                return;

            string xPathLeftMenu = "//div[@id='left-menu']";
            HtmlNodeCollection nodeCollectionLeftMenu = htmlAgility.GetNodeCollection(docServices, xPathLeftMenu);
            if (nodeCollectionLeftMenu != null) {
                HtmlNodeCollection nodeCollectionLinks = nodeCollectionLeftMenu[0].SelectNodes(xPathLeftMenu + "//a[@href]");
                if (nodeCollectionLinks == null) {
                    Console.WriteLine("nodeCollectionLinks == null");
                    return;
                }

                foreach (HtmlNode nodeLink in nodeCollectionLinks) {
                    try {
                        string serviceName = ClearString(nodeLink.InnerText);
                        backgroundWorker.ReportProgress((int)progressCurrent, serviceName);
                        Console.WriteLine("serviceName: " + serviceName);
                        string hrefValue = nodeLink.Attributes["href"].Value;
                        string urlService = siteInfo.UrlRoot + hrefValue;

                        Items.ServiceGroup itemServiceGroupInner = new Items.ServiceGroup() { Name = serviceName, Link = urlService };
                        HtmlDocument docService = htmlAgility.GetDocument(urlService, siteInfo);

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

        public void ParseSiteMedsiRu(HtmlDocument docService, Items.ServiceGroup itemServiceGroupRoot) {
            if (itemServiceGroupRoot.Name.StartsWith("Вакцинация от вируса папилломы"))
                Console.WriteLine("");

            string xPathClinicsId = "//*[starts-with(@class,'ui-select__nosearch')]/option";
            HtmlNodeCollection htmlNodeId = docService.DocumentNode.SelectNodes(xPathClinicsId);
            if (htmlNodeId != null) {
                Dictionary<string, string> idValues = new Dictionary<string, string>();
                foreach (HtmlNode nodeId in htmlNodeId) {
                    try {
                        string key = nodeId.Attributes["value"].Value;
                        string value = ClearString(nodeId.InnerText);

                        if (!idValues.ContainsKey(key))
                            idValues.Add(key, value);
                    } catch (Exception e) {
                        Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                    }
                }

                string xPathTable = "//div[@class='js-pricelist-clinic']";
                HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathTable);

                if (nodeCollectionService != null) {
                    foreach (HtmlNode nodeGroup in nodeCollectionService) {
                        if (nodeGroup.ChildNodes.Count <= 1) {
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

                            Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
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

                                    Items.Service itemService = new Items.Service() {
                                        Name = name,
                                        Price = ClearString(price)
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
            }

            string xPathBlades = "//div[@class='blades-item']";
            HtmlNodeCollection htmlNodesBladeItem = htmlAgility.GetNodeCollection(docService, xPathBlades);
            if (htmlNodesBladeItem != null) {
                foreach (HtmlNode nodeBlade in htmlNodesBladeItem) {
                    string xPathClinicName = nodeBlade.XPath + "//p[@class='cl-name']";
                    HtmlNode nodeClinicName = nodeBlade.SelectSingleNode(xPathClinicName);
                    string clinicName = ClearString(nodeClinicName.InnerText);
                    if (string.IsNullOrEmpty(clinicName)) {
                        Console.WriteLine("string.IsNullOrEmpty(clinicName)");
                        continue;
                    }

                    string xPathServices = nodeBlade.XPath + "//li[starts-with(@class,'hide-2-list-item')]";
                    HtmlNodeCollection htmlNodesPrice = nodeBlade.SelectNodes(xPathServices);
                    if (htmlNodesPrice == null) {
                        Console.WriteLine("htmlNodesPrice == null");
                        continue;
                    }

                    Items.ServiceGroup itemServiceGroupInner = new Items.ServiceGroup() {
                        Name = itemServiceGroupRoot.Name + " - " + clinicName,
                        Link = itemServiceGroupRoot.Link
                    };

                    foreach (HtmlNode nodePrice in htmlNodesPrice) {
                        string xPathServiceName = nodePrice.XPath + "/p[@class='hide-2-list-item-p']";
                        HtmlNode nodeServiceName = nodePrice.SelectSingleNode(xPathServiceName);

                        string xPathServicePrice = nodePrice.XPath + "/div[@class='price-block price-block-with-desc']";
                        HtmlNode nodeServicePrice = nodePrice.SelectSingleNode(xPathServicePrice);

                        if (nodeServiceName == null || nodeServicePrice == null) {
                            Console.WriteLine("nodeServiceName == null || nodeServicePrice == null");
                            continue;
                        }

                        string serviceName = ClearString(nodeServiceName.InnerText);
                        string servicePrice = ClearString(nodeServicePrice.InnerText);
                        if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(servicePrice)) {
                            Console.WriteLine("string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(servicePrice)");
                            continue;
                        }

                        Items.Service itemService = new Items.Service() { Name = serviceName, Price = servicePrice };
                        itemServiceGroupInner.ServiceItems.Add(itemService);
                    }

                    if (itemServiceGroupInner.ServiceItems.Count > 0)
                        siteInfo.ServiceGroupItems.Add(itemServiceGroupInner);
                }
            }
        }

        public void ParseSiteKdlRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            if (itemServiceGroup.Link.Equals("https://kdl.ru/analizy-i-tseny/gormoni-krovi-testi-reproduktsii"))
                Console.WriteLine();

            string xPathMainBlock = "//div[@class='h-card__inner']";
            HtmlNodeCollection nodeCollectionMainBlock = htmlAgility.GetNodeCollection(docService, xPathMainBlock);

            if (nodeCollectionMainBlock != null) {
                foreach (HtmlNode nodeCard in nodeCollectionMainBlock) {
                    try {
                        string name = nodeCard.ChildNodes[1].ChildNodes[3].InnerText;
                        string price = nodeCard.ChildNodes[5].ChildNodes[3].InnerText;

                        Items.Service itemService = new Items.Service() {
                            Name = ClearString(name),
                            Price = ClearString(price)
                        };

                        itemServiceGroup.ServiceItems.Add(itemService);
                    } catch (Exception e) {
                        Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                    }
                }
            }


            string xPathDataService = "//div[@class='a-results js-tests']//div[@data-services-list]";
            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathDataService);

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

                List<Items.Service> serviceItems = JsonConvert.DeserializeObject<List<Items.Service>>(jsonArray);
                itemServiceGroup.ServiceItems.AddRange(serviceItems);
            }
        }

        public void ParseSiteGemotestRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTable = "//table[@class='d-col_xs_12 d-tal catalog-table']/tbody";
            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathTable);

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

                    Items.Service itemService = new Items.Service() {
                        Name = ClearString(name),
                        Price = ClearString(price)
                    };

                    itemServiceGroup.ServiceItems.Add(itemService);
                } catch (Exception e) {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }

        }

        public void ParseSiteZubRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTable = "//table[@class='table-responsive']";
            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathTable);

            if (nodeCollectionService == null) {
                Console.WriteLine("nodeCollectionService is null");
                return;
            }

            foreach (HtmlNode node in nodeCollectionService) {
                try {
                    List<Items.Service> serviceItems = ReadTrNodes(node);
                    itemServiceGroup.ServiceItems.AddRange(serviceItems);
                } catch (Exception e) {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }
        }

        public void ParseSiteDentolRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTable = "//table[@class='wp_excel_cms_table wp_excel_cms_table_общий']";
            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathTable);

            if (nodeCollectionService == null) {
                Console.WriteLine("nodeCollectionService is null");
                return;
            }

            List<Items.Service> serviceItems = ReadTrNodes(nodeCollectionService.First());
            itemServiceGroup.ServiceItems = serviceItems;
        }

        public void ParseSiteNrmedRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathLinks = "//dd[@class='timeline-event-content']//a[@href]";
            HtmlNodeCollection nodeCollectionLinks = htmlAgility.GetNodeCollection(docService, xPathLinks);
            if (nodeCollectionLinks != null) {
                siteInfo.XPathServices = xPathLinks;
                ParseSiteWithLinksOnMainPage(docService);
            }

            string xPathAnalizes = "//div[@class='in-medcenter__title with-analysis']";
            HtmlNodeCollection nodeCollectionAnalizes = htmlAgility.GetNodeCollection(docService, xPathAnalizes);

            if (nodeCollectionAnalizes != null) {
                foreach (HtmlNode nodeChild in nodeCollectionAnalizes) {
                    HtmlNode htmlNodeServiceName = nodeChild.SelectSingleNode(nodeChild.XPath + "//div[@class='in-medcenter__title-count']");
                    HtmlNode htmlNodeServicePrice = nodeChild.SelectSingleNode(nodeChild.XPath + "//div[@class='in-medcenter__title-price']");

                    if (htmlNodeServiceName == null || htmlNodeServicePrice == null) {
                        Console.WriteLine("htmlNodeServiceName == null || htmlNodeServicePrice == null");
                        continue;
                    }

                    Items.Service itemService = new Items.Service() {
                        Name = ClearString(htmlNodeServiceName.InnerText),
                        Price = ClearString(htmlNodeServicePrice.InnerText)
                    };
                    itemServiceGroup.ServiceItems.Add(itemService);
                }
            }



            string xPathTableCurrent = "//table[@class='pricecurrent']//tbody";
            HtmlNodeCollection nodeCollectionTablesCurrent = htmlAgility.GetNodeCollection(docService, xPathTableCurrent);
            if (nodeCollectionTablesCurrent != null) {
                foreach (HtmlNode nodeTbody in nodeCollectionTablesCurrent) {
                    List<Items.Service> serviceItems = ReadTrNodes(nodeTbody);
                    itemServiceGroup.ServiceItems.AddRange(serviceItems);
                }
            }



            string xPathTable = "//div[@class='pediatrics__disease-row']";
            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathTable);

            if (nodeCollectionService != null) {
                foreach (HtmlNode htmlNodeService in nodeCollectionService) {
                    try {
                        Items.Service itemService = new Items.Service() {
                            Name = ClearString(htmlNodeService.ChildNodes[3].InnerText).Remove(0, 6).TrimStart(' '),
                            Price = htmlNodeService.ChildNodes[5].InnerText
                        };
                        itemServiceGroup.ServiceItems.Add(itemService);
                    } catch (Exception e) {
                        Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                    }
                }
            }

            string xPathPrices = "//div[@class='price']";
            HtmlNodeCollection nodeCollectionPrices = htmlAgility.GetNodeCollection(docService, xPathPrices);
            if (nodeCollectionPrices != null) {
                foreach (HtmlNode node in nodeCollectionPrices) {
                    try {
                        string xPathName = node.XPath + "/div[2]";
                        HtmlNode nodeName = node.SelectSingleNode(xPathName);

                        string xPathPrice = node.XPath + "/div[3]";
                        HtmlNode nodePrice = node.SelectSingleNode(xPathPrice);

                        if (nodeName == null || nodePrice == null) {
                            Console.WriteLine("nodeName == null || nodePrice == null");
                            continue;
                        }

                        string serviceName = ClearString(nodeName.InnerText);
                        string servicePrice = ClearString(nodePrice.InnerText);

                        if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(servicePrice))
                            continue;

                        Items.Service itemService = new Items.Service() { Name = serviceName, Price = servicePrice };
                        itemServiceGroup.ServiceItems.Add(itemService);
                    } catch (Exception e) {
                        Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                    }
                }
            }

        }

        public void ParseSiteCmdOnlineRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathPriceTable = "//*[@id=\"serv_list_content\"]/table[1]/tbody";

            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathPriceTable);
            if (nodeCollectionService == null) {
                Console.WriteLine("nodeCollectionService is null");

                siteInfo.XPathServices = "//*[@id=\"serv_list_content\"]//a[@href]";
                ParseSiteWithLinksOnMainPage(docService);

                return;
            }

            List<Items.Service> serviceItems = ReadTrNodes(nodeCollectionService.First(), 1, 2);
            itemServiceGroup.ServiceItems = serviceItems;
        }

        public void ParseSiteSmClinicRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
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

            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathPriceTable);
            if (nodeCollectionService == null) {
                Console.WriteLine("nodeCollectionService is null");
                return;
            }

            List<Items.Service> serviceItems = ReadTrNodes(nodeCollectionService.First());
            itemServiceGroup.ServiceItems = serviceItems;
        }

        public void ParseSiteFamilyDoctorRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathPriceTable = "//div[@class='tbl_prices']//table";
            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathPriceTable);
            if (nodeCollectionService == null) {
                Console.WriteLine("nodeCollectionService is null");
                return;
            }

            List<Items.Service> serviceItems = ReadTrNodes(nodeCollectionService.First());
            itemServiceGroup.ServiceItems = serviceItems;
        }

        public void ParseSiteFdoctorRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathServiceTables = "//table";
            string xPathServiceId = "//*[@id=\"site-wrapper\"]/section/div/div/div/div[1]";
            string xPathServiceDataIds = "//*[@id=\"service-inner-panes\"]";
            string urlGetPrices = "/ajax/services/inner/getPrices.php?service_id=@serviceId&service_type_id=@serviceTypeId";

            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathServiceTables);
            if (nodeCollectionService == null) {
                Console.WriteLine("nodeCollectionService is null");
                return;
            }

            try {
                HtmlNodeCollection nodeCollectionServiceId = htmlAgility.GetNodeCollection(docService, xPathServiceId);
                if (nodeCollectionServiceId == null) {
                    Console.WriteLine("nodeCollectionServiceId is null");
                    return;
                }

                HtmlNodeCollection nodeCollectionServiceDataIds = htmlAgility.GetNodeCollection(docService, xPathServiceDataIds);
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
                        string response = htmlAgility.GetResponse(urlGetPrice);

                        stringBuilder.Append(response);
                    } catch (Exception attrExc) {
                        backgroundWorker.ReportProgress((int)progressCurrent, attrExc.Message);
                        Console.WriteLine("exception attribute: " + attrExc.Message);
                    }
                }

                HtmlDocument docPrices = new HtmlDocument();
                docPrices.LoadHtml(stringBuilder.ToString());
                List<Items.Service> priceServiceItems = ReadTrNodes(docPrices.DocumentNode);
                itemServiceGroup.ServiceItems.AddRange(priceServiceItems);
            } catch (Exception idExc) {
                backgroundWorker.ReportProgress((int)progressCurrent, idExc.Message);
                Console.WriteLine("Exception serviceId: " + idExc.Message +
                    Environment.NewLine + idExc.StackTrace);
            }

            foreach (HtmlNode table in nodeCollectionService) {
                HtmlNode tbody = table.SelectSingleNode("tbody");
                List<Items.Service> serviceItems = ReadTrNodes(tbody);
                itemServiceGroup.ServiceItems.AddRange(serviceItems);
            }
        }

    }
}
