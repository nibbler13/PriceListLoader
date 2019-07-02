using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceListLoader.RegionParsers {
    class ParseKrd : ParseGeneral {
        public ParseKrd(HtmlAgility htmlAgility, BackgroundWorker bworker, SiteInfo siteInfo) : base(htmlAgility, bworker, siteInfo) { }

        public void ParseSiteKrdKubanKdlRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollection = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollection == null) {
                Console.WriteLine("nodeCollection == null");
                return;
            }

            foreach (HtmlNode nodeGroupDiv in nodeCollection) {
                HtmlNode nodeDivHead = nodeGroupDiv.SelectSingleNode(nodeGroupDiv.XPath + "//div[@class='panel-heading']");
                if (nodeDivHead == null) {
                    Console.WriteLine("nodeDivHead == null");
                    continue;
                }

                Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
                    Name = ClearString(nodeDivHead.InnerText),
                    Link = siteInfo.UrlServicesPage
                };

                backgroundWorker.ReportProgress(0, itemServiceGroup.Name);

                HtmlNode nodeTbody = nodeGroupDiv.SelectSingleNode(nodeGroupDiv.XPath + "//tbody");
                if (nodeTbody == null) {
                    Console.WriteLine("nodeTbody == null");
                    continue;
                }

                List<Items.Service> serviceItems = ReadTrNodes(nodeTbody);
                itemServiceGroup.ServiceItems.AddRange(serviceItems);
                siteInfo.ServiceGroupItems.Add(itemServiceGroup);
            }
        }

        public void ParseSiteKrdClinicistRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollectionTr = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollectionTr == null) {
                Console.WriteLine("nodeCollectionTr == null");
                return;
            }

            Items.ServiceGroup itemServiceGroup = null;
            foreach (HtmlNode nodeTr in nodeCollectionTr) {
                if (nodeTr.HasAttributes) {
                    if (itemServiceGroup == null)
                        continue;

                    HtmlNodeCollection nodeCollectionTd = nodeTr.SelectNodes(nodeTr.XPath + "//td");
                    if (nodeCollectionTd == null || nodeCollectionTd.Count < 2) {
                        Console.WriteLine("nodeCollectionTd == null || nodeCollectionTd.Count < 2");
                        continue;
                    }

                    HtmlNode nodeName = nodeCollectionTd[0];
                    HtmlNode nodePrice = nodeCollectionTd[1];
                    string serviceName = ClearString(nodeName.InnerText);
                    string servicePrice = ClearString(nodePrice.InnerText);

                    HtmlNode htmlNodeSpanInsidePrice = nodePrice.SelectSingleNode(nodePrice.XPath + "//span");
                    if (htmlNodeSpanInsidePrice != null)
                        servicePrice = servicePrice.Replace(ClearString(htmlNodeSpanInsidePrice.InnerHtml), "");

                    if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(servicePrice))
                        continue;

                    itemServiceGroup.ServiceItems.Add(new Items.Service() {
                        Name = serviceName,
                        Price = servicePrice
                    });
                } else {
                    if (itemServiceGroup != null)
                        siteInfo.ServiceGroupItems.Add(itemServiceGroup);

                    itemServiceGroup = new Items.ServiceGroup() {
                        Name = ClearString(nodeTr.InnerText),
                        Link = siteInfo.UrlServicesPage
                    };

                    backgroundWorker.ReportProgress(0, itemServiceGroup.Name);
                }
            }

            if (itemServiceGroup != null)
                siteInfo.ServiceGroupItems.Add(itemServiceGroup);
        }

        public void ParseSiteKrdVrukahCom(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathServiceGroups = "//div[starts-with(@class,'branchService__item')]";
            HtmlNodeCollection nodeCollectionServiceGroups = docService.DocumentNode.SelectNodes(xPathServiceGroups);

            if (nodeCollectionServiceGroups != null) {
                foreach (HtmlNode nodeServiceGroup in nodeCollectionServiceGroups) {
                    string xPathGroupName = "//div[starts-with(@class,'branchService__title')]/span";
                    HtmlNode nodeGroupName = nodeServiceGroup.SelectSingleNode(nodeServiceGroup.XPath + xPathGroupName);

                    if (nodeGroupName == null) {
                        Console.WriteLine("nodeGroupName == null");
                        continue;
                    }

                    Items.ServiceGroup serviceGroupInner = new Items.ServiceGroup {
                        Name = itemServiceGroup.Name + " @ " + ClearString(nodeGroupName.InnerText),
                        Link = itemServiceGroup.Link
                    };

                    string xPathGroupServices = "//div[@class='branchService__title--lvl2 d-flex']";
                    HtmlNodeCollection nodeCollectionServices = nodeServiceGroup.SelectNodes(nodeServiceGroup.XPath + xPathGroupServices);

                    if (nodeCollectionServices == null) {
                        Console.WriteLine("nodeCollectionServices == null");
                        continue;
                    }

                    foreach (HtmlNode nodeService in nodeCollectionServices) {
                        HtmlNode nodeName = nodeService.SelectSingleNode(nodeService.XPath + "//span[1]");
                        HtmlNode nodePrice = nodeService.SelectSingleNode(nodeService.XPath + "//span[2]");

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

                        serviceGroupInner.ServiceItems.Add(itemService);
                    }

                    siteInfo.ServiceGroupItems.Add(serviceGroupInner);
                }
            }

            string xPathServicesLab = "//table[@class='table table-striped']//tbody";
            HtmlNodeCollection nodeCollectionServicesLab = htmlAgility.GetNodeCollection(docService, xPathServicesLab);

            if (nodeCollectionServicesLab != null) {
                foreach (HtmlNode nodeServiceLab in nodeCollectionServicesLab) {
                    int nameOffset = 0;
                    int priceOffset = 0;

                    if (itemServiceGroup.Name.Contains("CITO"))
                        priceOffset = 1;

                    if (itemServiceGroup.Name.StartsWith("Предоперационные")) {
                        nameOffset = 1;
                        priceOffset = 1;
                    }

                    List<Items.Service> services = ReadTrNodes(nodeServiceLab, nameOffset, priceOffset);
                    itemServiceGroup.ServiceItems.AddRange(services);
                }
            }
        }

        public void ParseSiteKrdClinicaNazdorovieRuLab(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTbodies = "//div[@class=' page-right']//table[@class='table']//tbody";
            HtmlNodeCollection nodeCollectionTbodies = htmlAgility.GetNodeCollection(docService, xPathTbodies);
            if (nodeCollectionTbodies == null) {
                Console.WriteLine("nodeCollectionTbodies == null");
                return;
            }

            foreach (HtmlNode nodeTbody in nodeCollectionTbodies) {
                List<Items.Service> serviceItems = ReadTrNodes(nodeTbody, 1, 2);
                itemServiceGroup.ServiceItems.AddRange(serviceItems);
            }
        }

        public void ParseSiteKrdClinicaNazdorovieRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTbodies = "//table[@class='table table-bordered table-hover table-striped']//tbody";
            HtmlNodeCollection nodeCollectionTbodies = htmlAgility.GetNodeCollection(docService, xPathTbodies);
            if (nodeCollectionTbodies == null) {
                Console.WriteLine("nodeCollectionTbodies == null");
                return;
            }

            int priceOffset = 1;
            if (itemServiceGroup.Link.Equals("http://clinica-nazdorovie.ru/m/medicinskie-uslugi/gid28/pg0/"))
                priceOffset = 2;

            foreach (HtmlNode nodeTbody in nodeCollectionTbodies) {
                List<Items.Service> serviceItems = ReadTrNodes(nodeTbody, 1, priceOffset);
                itemServiceGroup.ServiceItems.AddRange(serviceItems);
            }
        }

        public void ParseSiteKrdPolyClinicRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTbodies = "//table[@id='sortTable']//tbody";
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
    }
}
