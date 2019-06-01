using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceListLoader.RegionParsers {
    class ParseUfa : ParseGeneral {
        public ParseUfa(HtmlAgility htmlAgility, BackgroundWorker bworker, SiteInfo siteInfo) : base(htmlAgility, bworker, siteInfo) { }

        public void ParseSiteUfaMdplusRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollectionServices == null) {
                Console.WriteLine("nodeCollectionServices is null");
                return;
            }

            Items.ServiceGroup itemServiceGroup = null;
            foreach (HtmlNode nodeServices in nodeCollectionServices) {
                foreach (HtmlNode child in nodeServices.ChildNodes) {
                    if (child.Name.Equals("h4") && itemServiceGroup == null) {
                        itemServiceGroup = new Items.ServiceGroup() {
                            Name = ClearString(child.InnerText),
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

                        List<Items.Service> serviceItems = ReadTrNodes(nodeTbody, 1, 1);
                        itemServiceGroup.ServiceItems.AddRange(serviceItems);

                        if (itemServiceGroup.ServiceItems.Count > 0)
                            siteInfo.ServiceGroupItems.Add(itemServiceGroup);

                        itemServiceGroup = null;
                    }
                }
            }
        }

        public void ParseSiteUfaMamadetiRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
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

                string groupName = ClearString(nodeGroupName.InnerText);
                Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
                    Name = groupName,
                    Link = siteInfo.UrlServicesPage
                };

                HtmlNodeCollection nodeCollectionTables = nodeGroup.SelectNodes(nodeGroup.XPath + "//table");
                if (nodeCollectionTables == null) {
                    Console.WriteLine("nodeCollectionTables == null");
                    continue;
                }

                foreach (HtmlNode nodeTable in nodeCollectionTables) {
                    List<Items.Service> serviceItems = ReadTrNodes(nodeTable);
                    itemServiceGroup.ServiceItems.AddRange(serviceItems);
                }

                if (itemServiceGroup.ServiceItems.Count > 0)
                    siteInfo.ServiceGroupItems.Add(itemServiceGroup);
            }
        }

        public void ParseSiteUfaMegiClinic(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollectionServices == null) {
                Console.WriteLine("nodeCollectionServices is null");
                return;
            }

            foreach (HtmlNode htmlNodeGroup in nodeCollectionServices) {
                string groupName = string.Empty;
                HtmlNode nodeGroupName = htmlNodeGroup.SelectSingleNode(htmlNodeGroup.XPath + "/div[@class='more']");
                if (nodeGroupName != null)
                    groupName = ClearString(nodeGroupName.InnerText);

                Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
                    Name = groupName,
                    Link = siteInfo.UrlServicesPage
                };

                HtmlNode nodeTbody = htmlNodeGroup.SelectSingleNode(htmlNodeGroup.XPath + "//tbody");
                if (nodeTbody == null) {
                    Console.WriteLine("nodeTbody == null");
                    continue;
                }

                List<Items.Service> serviceItems = ReadTrNodes(nodeTbody);
                itemServiceGroup.ServiceItems.AddRange(serviceItems);

                if (itemServiceGroup.ServiceItems.Count > 0)
                    siteInfo.ServiceGroupItems.Add(itemServiceGroup);
            }
        }

        public void ParseSiteUfaPromedicinaClinic(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathServices = "//div[starts-with(@class,'price_block_item')]";
            HtmlNodeCollection nodeCollectionServices = htmlAgility.GetNodeCollection(docService, xPathServices);
            Console.WriteLine(itemServiceGroup.Name);

            if (nodeCollectionServices == null) {
                Console.WriteLine("nodeCollectionService is null");
                return;
            }

            foreach (HtmlNode nodeService in nodeCollectionServices) {
                HtmlNode nodeServiceName = nodeService.SelectSingleNode(nodeService.XPath + "/div[starts-with(@class, 'price_text')]");
                HtmlNode nodeServicePrice = nodeService.SelectSingleNode(nodeService.XPath + "//div[@class='price_value']");

                if (nodeServiceName == null) {
                    Console.WriteLine("nodeServiceName == null");
                    continue;
                }

                string serviceName = ClearString(nodeServiceName.InnerText);
                string servicePrice = string.Empty;
                if (nodeServicePrice != null) {
                    servicePrice = ClearString(nodeServicePrice.InnerText);

                    if (serviceName.Contains(servicePrice) && serviceName.IndexOf(servicePrice) > 0)
                        try {
                            serviceName = ClearString(serviceName.Substring(0, serviceName.IndexOf(servicePrice) - 1));
                        } catch (Exception e) {
                            Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                        }
                }

                Items.Service itemService = new Items.Service() {
                    Name = serviceName,
                    Price = servicePrice
                };

                itemServiceGroup.ServiceItems.Add(itemService);
            }
        }

    }
}
