using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PriceListLoader.RegionParsers {
    class ParseSpb : ParseGeneral {
        public ParseSpb(HtmlAgility htmlAgility, BackgroundWorker bworker, SiteInfo siteInfo) : base(htmlAgility, bworker, siteInfo) { }

        public void ParseSiteSpbMc21Ru(HtmlDocument docServices, bool isFirstCycle = true,
            HtmlNodeCollection nodeCollection = null, string rootName = "") {
            HtmlNodeCollection nodeCollectionServices;
            if (isFirstCycle) {
                nodeCollectionServices = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
                nodeCollectionServices = nodeCollectionServices[0].ChildNodes;
            } else {
                nodeCollectionServices = nodeCollection;
            }

            if (nodeCollectionServices == null) {
                Console.WriteLine("nodeCollectionServices is null");
                return;
            }

            Items.ServiceGroup itemServiceGroup = null;
            foreach (HtmlNode htmlNode in nodeCollectionServices) {
                if (itemServiceGroup == null && (htmlNode.Name.Equals("h2") || htmlNode.Name.Equals("h3"))) {
                    string groupName = ClearString(htmlNode.InnerText);
                    itemServiceGroup = new Items.ServiceGroup() {
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

                            string serviceName = ClearString(htmlNodeTr.ChildNodes[0].InnerText);
                            string servicePriceClinic = ClearString(htmlNodeTr.ChildNodes[1].InnerText);
                            string servicePriceHome = htmlNodeTr.ChildNodes.Count == 3 ?
                                ClearString(htmlNodeTr.ChildNodes[2].InnerText) : string.Empty;

                            if (string.IsNullOrEmpty(servicePriceHome)) {
                                itemServiceGroup.ServiceItems.Add(
                                    new Items.Service() { Name = serviceName, Price = servicePriceClinic });
                            } else {
                                itemServiceGroup.ServiceItems.Add(
                                    new Items.Service() { Name = serviceName + ", амбулаторно", Price = servicePriceClinic });
                                itemServiceGroup.ServiceItems.Add(
                                    new Items.Service() { Name = serviceName + ", на дому", Price = servicePriceHome });
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

        public void ParseSiteSpbClinicComplexTitlePage(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollectionServicesRoot = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollectionServicesRoot == null) {
                Console.WriteLine("nodeCollectionServices is null");
                return;
            }

            foreach (HtmlNode nodeServiceRoot in nodeCollectionServicesRoot) {
                string xPathTitle = nodeServiceRoot.XPath + "//div[@class='title']";

                HtmlNode htmlNodeTitle = nodeServiceRoot.SelectSingleNode(xPathTitle);
                if (htmlNodeTitle == null) {
                    Console.WriteLine("htmlNodeTitle == null");
                    continue;
                }

                string title = ClearString(htmlNodeTitle.InnerText);
                backgroundWorker.ReportProgress(0, "Группа услуг - " + title);
                siteInfo.XPathServices = nodeServiceRoot.XPath + "//div[@class='text childs']//li//a[@href]";

                ParseSiteWithLinksOnMainPage(docServices, itemGroupNamePrefix: title);
            }
        }

        public void ParseSiteSpbAllergomedRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollection = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollection != null) {
                foreach (HtmlNode nodeServiceRoot in nodeCollection) {
                    HtmlNode nodeGroupName = nodeServiceRoot.SelectSingleNode(nodeServiceRoot.XPath + "//div[@class='header_one_adult_direction']");

                    string groupName = string.Empty;
                    if (nodeGroupName != null)
                        groupName = ClearString(nodeGroupName.InnerText);

                    Items.ServiceGroup serviceGroup = new Items.ServiceGroup {
                        Link = siteInfo.UrlServicesPage,
                        Name = groupName
                    };

                    HtmlNodeCollection nodeCollectionServices = nodeServiceRoot.SelectNodes(nodeServiceRoot.XPath + "//div[@class='one_content_one_adult_direction']");

                    foreach (HtmlNode nodeService in nodeCollectionServices) {
                        HtmlNode nodeName = nodeService.SelectSingleNode(nodeService.XPath + "/span[1]");
                        HtmlNode nodePrice = nodeService.SelectSingleNode(nodeService.XPath + "/span[2]");

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

                        serviceGroup.ServiceItems.Add(itemService);
                    }

                    siteInfo.ServiceGroupItems.Add(serviceGroup);
                }
            }
        }

        public void ParseSiteSpbReasunmedRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollection = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollection == null) {
                Console.WriteLine("nodeCollection == null");
                return;
            }

            Items.ServiceGroup itemServiceGroup = null;
            foreach (HtmlNode nodeChild in nodeCollection[0].ChildNodes) {
                if (nodeChild.Name.Equals("h3")) {
                    itemServiceGroup = new Items.ServiceGroup() {
                        Name = ClearString(nodeChild.InnerText),
                        Link = siteInfo.UrlServicesPage
                    };
                    backgroundWorker.ReportProgress(0, itemServiceGroup.Name);
                } else if (nodeChild.Name.Equals("table") || nodeChild.Name.Equals("div")) {
                    if (itemServiceGroup == null)
                        continue;

                    HtmlNodeCollection htmlNodeTbody = nodeChild.SelectNodes(nodeChild.XPath + "//tbody");

                    if (htmlNodeTbody == null)
                        htmlNodeTbody = nodeChild.SelectNodes(nodeChild.XPath + "//table");

                    if (htmlNodeTbody == null) {
                        Console.WriteLine("htmlNodeTbody == null");
                        continue;
                    }

                    foreach (HtmlNode nodeData in htmlNodeTbody) {
                        List<Items.Service> serviceItems = ReadTrNodes(nodeData);
                        itemServiceGroup.ServiceItems.AddRange(serviceItems);
                    }

                    siteInfo.ServiceGroupItems.Add(itemServiceGroup);
                }
            }
        }

        public void ParseSiteSpbMedVdkRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup, bool isFirstCycle) {
            if (isFirstCycle) {
                string xPathSecondaryLinks = "//div[@class='menu-inner-secondary']//a[@href]";
                HtmlNodeCollection secondatyLinks = docService.DocumentNode.SelectNodes(xPathSecondaryLinks);
                if (secondatyLinks != null) {
                    siteInfo.XPathServices = xPathSecondaryLinks;
                    siteInfo.UrlServicesPage = itemServiceGroup.Link;
                    ParseSiteWithLinksOnMainPage(docService, false);
                }
            }

            string xPathServices = "//div[@class='table-price']//tbody";
            HtmlNodeCollection nodeServices = docService.DocumentNode.SelectNodes(xPathServices);

            if (nodeServices == null) {
                Console.WriteLine("nodeServices == null");
                return;
            }

            foreach (HtmlNode nodeService in nodeServices) {
                List<Items.Service> itemServices = ReadTrNodes(nodeService);
                itemServiceGroup.ServiceItems.AddRange(itemServices);
            }
        }

        public void ParseSiteSpbClinicaBlagodatRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTbodies = "//div[starts-with(@class,'blog')]//tbody";
            HtmlNodeCollection nodeCollection = docService.DocumentNode.SelectNodes(xPathTbodies);

            if (nodeCollection == null) {
                Console.WriteLine("nodeCollectionTbodies == null");
                return;
            }

            foreach (HtmlNode node in nodeCollection) {
                int nameOffset = 0;
                int priceOffset = 0;

                string currentlLink = itemServiceGroup.Link;

                if (currentlLink.Equals("https://clinica-blagodat.ru/laboratornye-analizy-tseny/")) {
                    nameOffset = 1;
                    priceOffset = 1;
                } else if (
                    currentlLink.Equals("https://clinica-blagodat.ru/dermatologiya-tseny/") ||
                    currentlLink.Equals("https://clinica-blagodat.ru/kosmetologiya-tseny/") ||
                    currentlLink.Equals("https://clinica-blagodat.ru/massazh-tseny/") ||
                    currentlLink.Equals("https://clinica-blagodat.ru/osteopatiya-tseny/") ||
                    currentlLink.Equals("https://clinica-blagodat.ru/su-dzhok-terapiya-tsena/") ||
                    currentlLink.Equals("https://clinica-blagodat.ru/terapiya-tseny/") ||
                    currentlLink.Equals("https://clinica-blagodat.ru/tseny-uzi/") ||
                    currentlLink.Equals("https://clinica-blagodat.ru/fizioterapiya-tseny/") ||
                    currentlLink.Equals("https://clinica-blagodat.ru/funktsionalnaya-diagnostika-tseny/") ||
                    currentlLink.Equals("https://clinica-blagodat.ru/hirurgiya-tseny/") ||
                    currentlLink.Equals("https://clinica-blagodat.ru/refleksoterapiya-tseny/")) {
                    priceOffset = 1;
                }

                List<Items.Service> services = ReadTrNodes(node, nameOffset, priceOffset);

                if (services.Count > 0)
                    if (services[0].Price.ToLower().Contains("мин"))
                        services = ReadTrNodes(node);

                itemServiceGroup.ServiceItems.AddRange(services);
            }
        }

        public void ParseSiteSpbStarsclinicRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTbodies = "//table[@class='service_price']";
            HtmlNodeCollection nodeCollection = docService.DocumentNode.SelectNodes(xPathTbodies);

            if (nodeCollection == null) {
                Console.WriteLine("nodeCollectionTbodies == null");
                return;
            }

            foreach (HtmlNode node in nodeCollection) {
                int priceOffset = 0;
                if (itemServiceGroup.Link.Equals("http://starsclinic.ru/services/vaccination/"))
                    priceOffset = 1;

                List<Items.Service> services = ReadTrNodes(node, 0, priceOffset);
                itemServiceGroup.ServiceItems.AddRange(services);
            }
        }

        public void ParseSiteSpbDcenergoKidsRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTbodies = "//div[@class='price-item']";
            HtmlNodeCollection nodeCollectionTbodies = htmlAgility.GetNodeCollection(docService, xPathTbodies);
            if (nodeCollectionTbodies == null) {
                Console.WriteLine("nodeCollectionTbodies == null");
                return;
            }

            foreach (HtmlNode nodeService in nodeCollectionTbodies) {
                HtmlNode nodeName = nodeService.SelectSingleNode(nodeService.XPath + "//div[@class='price-item__title']");
                HtmlNode nodePrice = nodeService.SelectSingleNode(nodeService.XPath + "//div[@class='price-item__summ']");

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

        public void ParseSiteSpbDcenergoRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTbodies = "//div[contains(@class,'table-price-row ')]";
            HtmlNodeCollection nodeCollectionTbodies = htmlAgility.GetNodeCollection(docService, xPathTbodies);
            if (nodeCollectionTbodies == null) {
                Console.WriteLine("nodeCollectionTbodies == null");
                return;
            }

            foreach (HtmlNode nodeService in nodeCollectionTbodies) {
                HtmlNode nodeName = nodeService.SelectSingleNode(nodeService.XPath + "//div[@class='table-price-column table-price-column-name']");
                HtmlNode nodePrice = nodeService.SelectSingleNode(nodeService.XPath + "//div[@class='table-price-column table-price-value']");

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

        public void ParseSiteSpbClinicComplexRu(HtmlDocument docServices, ref Items.ServiceGroup itemServiceGroup) {
            string xPathService = "//div[@class='col-md-12 col-sm-12']";
            HtmlNodeCollection htmlNodesService = htmlAgility.GetNodeCollection(docServices, xPathService);
            if (htmlNodesService == null) {
                Console.WriteLine("htmlNodesService == null");
                return;
            }

            foreach (HtmlNode nodeService in htmlNodesService) {
                string xPathTitle = nodeService.XPath + "//div[@class='title with_p']";
                string xPathPrice = nodeService.XPath + "//div[@class='prices pull-right']/span";

                HtmlNode nodeTitle = nodeService.SelectSingleNode(xPathTitle);
                HtmlNode nodePrice = nodeService.SelectSingleNode(xPathPrice);

                if (nodeTitle == null || nodePrice == null) {
                    Console.WriteLine("nodeTitle == null || nodePrice == null");
                    continue;
                }

                Items.Service itemService = new Items.Service() {
                    Name = ClearString(nodeTitle.InnerText),
                    Price = ClearString(nodePrice.InnerText)
                };

                itemServiceGroup.ServiceItems.Add(itemService);
            }

            string xPathNextPage = "//div[@class='wrap_pagination']//li[@class='next']//a[@href]";
            HtmlNode nodeNext = docServices.DocumentNode.SelectSingleNode(xPathNextPage);
            if (nodeNext == null)
                return;

            if (!nodeNext.Attributes.Contains("href"))
                return;

            string nextPageUrl = string.Empty;
            string hrefValue = nodeNext.Attributes["href"].Value;
            if (hrefValue.StartsWith("http")) {
                nextPageUrl = hrefValue;
            } else {
                if (!hrefValue.StartsWith("/"))
                    hrefValue = "/" + hrefValue;
                nextPageUrl = siteInfo.UrlRoot + hrefValue;
            }

            HtmlDocument docNextPage = htmlAgility.GetDocument(nextPageUrl, siteInfo);
            if (docNextPage != null)
                ParseSiteSpbClinicComplexRu(docNextPage, ref itemServiceGroup);
        }

        public void ParseSiteSpbEmcclinicRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            siteInfo.XPathServices = "//div[@class='n-specs__inside']//a[@href]";
            ParseSiteWithLinksOnMainPage(docService);

            siteInfo.XPathServices = "//div[starts-with(@class,'n-text')]//li//a[@href]";
            ParseSiteWithLinksOnMainPage(docService);

            string xPathServices = "//div[@class='n-costs__item']";
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docService, xPathServices);
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

                    Items.Service itemService = new Items.Service() {
                        Name = ClearString(htmlNodeName.InnerText),
                        Price = ClearString(htmlNodeCost.InnerText)
                    };
                    itemServiceGroup.ServiceItems.Add(itemService);
                } catch (Exception e) {
                    Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                }
            }
        }

        public void ParseSiteSpbMedswissSpbRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTable = "//div[contains(@class, 'inner-content')]//tbody";
            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathTable);
            Console.WriteLine(itemServiceGroup.Name);

            if (nodeCollectionService == null) {
                Console.WriteLine("nodeCollectionService is null");
                return;
            }

            List<Items.Service> serviceItems = ReadTrNodes(nodeCollectionService[0]);
            itemServiceGroup.ServiceItems.AddRange(serviceItems);
        }

        public void ParseSiteSpbGermanClinic(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            Thread.Sleep(10 * 1000);
            string xPathPrices = "//div[@class='prices']//tbody";
            HtmlNodeCollection nodeCollectionPrices = htmlAgility.GetNodeCollection(docService, xPathPrices);
            Console.WriteLine(itemServiceGroup.Name);

            if (nodeCollectionPrices == null) {
                Console.WriteLine("nodeCollectionPrices is null");

                siteInfo.XPathServices = "//div[@class='service']//a[@href]";
                ParseSiteWithLinksOnMainPage(docService);

                return;
            }

            List<Items.Service> serviceItems = ReadTrNodes(nodeCollectionPrices[0]);
            itemServiceGroup.ServiceItems.AddRange(serviceItems);
        }

        public void ParseSiteSpbBaltzdravRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTable = "//div[starts-with(@class, 'wk-accordion')]";
            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathTable);
            Console.WriteLine(itemServiceGroup.Name);

            if (nodeCollectionService == null) {
                Console.WriteLine("nodeCollectionService is null");
                return;
            }

            Items.ServiceGroup itemServiceCurrent = null;
            foreach (HtmlNode nodeInner in nodeCollectionService) {
                foreach (HtmlNode nodeChild in nodeInner.ChildNodes) {
                    if (itemServiceCurrent == null && nodeChild.Name.ToLower().Equals("h3")) {
                        itemServiceCurrent = new Items.ServiceGroup() {
                            Name = ClearString(nodeChild.InnerText),
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

                        List<Items.Service> serviceItems = ReadTrNodes(nodeCollectionTbody[0], 1, 1);
                        itemServiceCurrent.ServiceItems.AddRange(serviceItems);

                        if (itemServiceCurrent.ServiceItems.Count > 0)
                            siteInfo.ServiceGroupItems.Add(itemServiceCurrent);

                        itemServiceCurrent = null;
                    }
                }
            }
        }

        public void ParseSiteSpbEvroMedRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTable = "//table[@class='t-plainrows']/tbody";
            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathTable);
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

                List<Items.Service> serviceItems = ReadTrNodes(node, offsetName, offsetPrice);
                itemServiceGroup.ServiceItems.AddRange(serviceItems);
            }
        }

    }
}
