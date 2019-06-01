using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceListLoader.RegionParsers {
    class ParseYekuk : ParseGeneral {
        public ParseYekuk(HtmlAgility htmlAgility, BackgroundWorker bworker, SiteInfo siteInfo) : base(htmlAgility, bworker, siteInfo) { }

        public void ParseSiteYekukMedkamenskRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollection = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollection == null) {
                Console.WriteLine("nodeCollection == null");
                return;
            }
        }

        public void ParseSiteYekukMfcrubinRu(HtmlDocument docServices, ref Items.ServiceGroup itemServiceGroup) {
            HtmlNodeCollection nodeCollection = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollection == null) {
                Console.WriteLine("nodeCollection == null");
                return;
            }

            foreach (HtmlNode nodeService in nodeCollection) {
                string xPathName = nodeService.XPath + "//p[@class='price-column']";
                string xPathPrice = nodeService.XPath + "//p[@class='price-amount']/b";

                HtmlNode nodeName = nodeService.SelectSingleNode(xPathName);
                HtmlNode nodePrice = nodeService.SelectSingleNode(xPathPrice);

                if (nodeName == null || nodePrice == null) {
                    Console.WriteLine("nodeName == null || nodePrice == null");
                    continue;
                }

                Items.Service itemService = new Items.Service() {
                    Name = ClearString(nodeName.InnerText),
                    Price = ClearString(nodePrice.InnerText)
                };

                itemServiceGroup.ServiceItems.Add(itemService);
            }

            string xPathNextPage = "//ul[@class='pagination']//li[@class='next']//a[@href]";
            HtmlNode htmlNodeNext = docServices.DocumentNode.SelectSingleNode(xPathNextPage);
            if (htmlNodeNext == null || !htmlNodeNext.Attributes.Contains("href"))
                return;

            string nextPageUrl = string.Empty;
            string hrefValue = htmlNodeNext.Attributes["href"].Value;
            if (hrefValue.StartsWith("http")) {
                nextPageUrl = hrefValue;
            } else {
                if (!hrefValue.StartsWith("/"))
                    hrefValue = "/" + hrefValue;
                nextPageUrl = siteInfo.UrlRoot + hrefValue;
            }

            HtmlDocument docNextPage = htmlAgility.GetDocument(nextPageUrl, siteInfo);
            if (docNextPage != null)
                ParseSiteYekukMfcrubinRu(docNextPage, ref itemServiceGroup);
        }

        public void ParseSiteYekukImmunoresursRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroupRoot) {
            string xPathServices = "//div[@class='object record-item']";
            HtmlNodeCollection nodeCollectionServicesMainPage = htmlAgility.GetNodeCollection(docService, xPathServices);
            Console.WriteLine(itemServiceGroupRoot.Name);

            if (nodeCollectionServicesMainPage != null)
                ParseSiteYekukImmunoresursRuGroup(nodeCollectionServicesMainPage, ref itemServiceGroupRoot);

            xPathServices = "//div[@id='id2']";
            HtmlNodeCollection nodeCollectionServicesLab = htmlAgility.GetNodeCollection(docService, xPathServices);
            if (nodeCollectionServicesLab != null) {
                HtmlNode nodeH3Title = nodeCollectionServicesLab[0].SelectSingleNode(
                    nodeCollectionServicesLab[0].XPath + "//h3[@class='contentTitle']");
                if (nodeH3Title != null) {
                    string title = ClearString(nodeH3Title.InnerText);

                    HtmlNode htmlNodeTbody = nodeCollectionServicesLab[0].SelectSingleNode(
                        nodeCollectionServicesLab[0].XPath + "//div[@class='contentText']//tbody");
                    if (htmlNodeTbody != null) {
                        List<Items.Service> serviceItems = ReadTrNodes(htmlNodeTbody, 1, 4);
                        if (serviceItems.Count > 0) {
                            Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
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

        public void ParseSiteYekukImmunoresursRuGroup(HtmlNodeCollection nodeCollection, ref Items.ServiceGroup itemServiceGroupRoot, bool notDefaultOffset = false) {
            foreach (HtmlNode nodeService in nodeCollection) {
                HtmlNode nodeTitle = nodeService.SelectSingleNode(nodeService.XPath + "//h4[@class='objectTitle']");
                if (nodeTitle == null) {
                    Console.WriteLine("nodeTitle == null");
                    continue;
                }

                string groupName = ClearString(nodeTitle.InnerText);
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

                List<Items.Service> serviceItems = ReadTrNodes(nodeTbody, offsetName, offsetPrice);
                Items.ServiceGroup itemServiceGroupInner = new Items.ServiceGroup() {
                    Name = groupName,
                    Link = itemServiceGroupRoot.Link
                };
                itemServiceGroupInner.ServiceItems = serviceItems;

                if (itemServiceGroupInner.ServiceItems.Count > 0)
                    siteInfo.ServiceGroupItems.Add(itemServiceGroupInner);
            }
        }

        public void ParseSiteYekukMcVdRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathServices = "//table[@id='displayProduct']";
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docService, xPathServices);
            Console.WriteLine(itemServiceGroup.Name);

            if (nodeCollectionServices == null) {
                Console.WriteLine("nodeCollectionService is null");
                return;
            }

            itemServiceGroup.ServiceItems.AddRange(ReadTrNodes(nodeCollectionServices[0], 1, 2));

            string xPathNextPage = "//ul[@class='page-numbers']//a[@class='next page-numbers']";
            HtmlNodeCollection nodeNextPageCollection = htmlAgility.GetNodeCollection(docService, xPathNextPage);
            if (nodeNextPageCollection == null)
                return;

            string linkNextPage = nodeNextPageCollection[0].Attributes["href"].Value;
            HtmlDocument docNextPage = htmlAgility.GetDocument(linkNextPage, siteInfo);

            if (docNextPage == null) {
                Console.WriteLine("docNextPage == null");
                return;
            }

            ParseSiteYekukMcVdRu(docNextPage, ref itemServiceGroup);

        }

        public void ParseSiteYekukRuslabsRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroupRoot) {
            string xPathServices = "//div[@class='service-item']";
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docService, xPathServices);
            Console.WriteLine(itemServiceGroupRoot.Name);

            if (nodeCollectionServices == null) {
                Console.WriteLine("nodeCollectionService is null");
                return;
            }

            foreach (HtmlNode nodeChild in nodeCollectionServices) {
                string xPathServiceName = "//div[@class='service-item__pagetitle']";
                string xPathServicePrice = "//span[@class='service-item__price-val']";

                HtmlNode nodeServiceName = nodeChild.SelectSingleNode(nodeChild.XPath + xPathServiceName);
                HtmlNode nodeServicePrice = nodeChild.SelectSingleNode(nodeChild.XPath + xPathServicePrice);

                if (nodeServiceName == null || nodeServicePrice == null) {
                    Console.WriteLine(nodeServiceName == null || nodeServicePrice == null);
                    continue;
                }

                Items.Service itemService = new Items.Service {
                    Name = ClearString(nodeServiceName.InnerText),
                    Price = ClearString(nodeServicePrice.InnerText)
                };

                itemServiceGroupRoot.ServiceItems.Add(itemService);

                //string innerGroupName = itemServiceGroupRoot.Name;
                //HtmlNode nodeGroupName = nodeChild.SelectSingleNode(nodeChild.XPath + "/h2");

                //if (nodeGroupName != null) {
                //    string nodeInnerText = ClearString(nodeGroupName.InnerText);
                //    if (!string.IsNullOrEmpty(nodeInnerText))
                //        innerGroupName += " - " + nodeInnerText;
                //}

                //Items.ServiceGroup itemServiceGroupInner = new Items.ServiceGroup {
                //    Name = innerGroupName,
                //    Link = itemServiceGroupRoot.Link
                //};

                //HtmlNode nodeTbody = nodeChild.SelectSingleNode(nodeChild.XPath + "//tbody");
                //if (nodeTbody == null) {
                //    Console.WriteLine("nodeTbody == null");
                //    itemServiceGroupInner = null;
                //    continue;
                //}

                //itemServiceGroupInner.ServiceItems = ReadTrNodes(nodeTbody, 1, 2);
                //if (itemServiceGroupInner.ServiceItems.Count > 0)
                //    siteInfo.ServiceGroupItems.Add(itemServiceGroupInner);

                //itemServiceGroupInner = null;
            }

            string xPathPagination = "//div[@class='pagination']";
            HtmlNode nodePagination = docService.DocumentNode.SelectSingleNode(xPathPagination);
            if (nodePagination != null)
                Console.WriteLine("!!! node pagination is not null");
        }

    }
}
