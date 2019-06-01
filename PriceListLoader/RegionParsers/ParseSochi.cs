using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceListLoader.RegionParsers {
    class ParseSochi : ParseGeneral {
        public ParseSochi(HtmlAgility htmlAgility, BackgroundWorker bworker, SiteInfo siteInfo) : base(htmlAgility, bworker, siteInfo) { }

        public void ParseSiteSochiMedcentrSochiRu(HtmlDocument docServices) {
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

                    HtmlNode htmlNodeTbody = nodeChild.SelectSingleNode(nodeChild.XPath + "//tbody");
                    if (htmlNodeTbody == null) {
                        Console.WriteLine("htmlNodeTbody == null");
                        continue;
                    }

                    List<Items.Service> serviceItems = ReadTrNodes(htmlNodeTbody, 1, 1);
                    itemServiceGroup.ServiceItems.AddRange(serviceItems);
                    siteInfo.ServiceGroupItems.Add(itemServiceGroup);
                }
            }
        }

        public void ParseSiteSochi23docRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollection = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollection != null) {
                foreach (HtmlNode nodeLi in nodeCollection) {
                    HtmlNode nodeA = nodeLi.SelectSingleNode(nodeLi.XPath + "//a");
                    if (nodeA == null) {
                        Console.WriteLine("nodeA == null");
                        continue;
                    }

                    Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
                        Name = ClearString(nodeA.InnerText),
                        Link = siteInfo.UrlServicesPage
                    };

                    backgroundWorker.ReportProgress(0, itemServiceGroup.Name);

                    HtmlNodeCollection htmlNodeTbodies = nodeLi.SelectNodes(nodeLi.XPath + "//tbody");
                    if (htmlNodeTbodies == null) {
                        Console.WriteLine("htmlNodeTbody == null");
                        continue;
                    }

                    foreach (HtmlNode nodeTbody in htmlNodeTbodies) {
                        List<Items.Service> serviceItems = ReadTrNodes(nodeTbody);
                        itemServiceGroup.ServiceItems.AddRange(serviceItems);
                    }

                    siteInfo.ServiceGroupItems.Add(itemServiceGroup);
                }
            }
        }

        public void ParseSiteSochiUzlovayaPoliklinikaRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollection = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollection == null) {
                Console.WriteLine("nodeCollection == null");
                return;
            }

            Items.ServiceGroup itemServiceGroup = null;
            foreach (HtmlNode nodeChild in nodeCollection[0].ChildNodes) {
                if (nodeChild.Name.Equals("h4")) {
                    itemServiceGroup = new Items.ServiceGroup() {
                        Name = ClearString(nodeChild.InnerText),
                        Link = siteInfo.UrlServicesPage
                    };

                    backgroundWorker.ReportProgress(0, itemServiceGroup.Name);
                } else if (nodeChild.Name.Equals("ul")) {
                    if (itemServiceGroup == null)
                        continue;

                    HtmlNodeCollection nodeCollectionServices = nodeChild.SelectNodes(nodeChild.XPath + "//div[@class='pricelist-item-container  pd-m']");
                    if (nodeCollectionServices == null) {
                        Console.WriteLine("nodeCollectionServices == null");
                        continue;
                    }


                    foreach (HtmlNode htmlNodeService in nodeCollectionServices) {
                        HtmlNode htmlNodeServiceName = htmlNodeService.SelectSingleNode(htmlNodeService.XPath + "//span[@class='js-pricelist-title']");
                        HtmlNode htmlNodeServicePrice = htmlNodeService.SelectSingleNode(htmlNodeService.XPath + "//div[@class='price-weight fs-large clearfix']");

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

                    siteInfo.ServiceGroupItems.Add(itemServiceGroup);
                }
            }
        }

        public void ParseSiteSochiArmedRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollection = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollection == null) {
                Console.WriteLine("nodeCollection == null");
                return;
            }

            foreach (HtmlNode nodeGroupDiv in nodeCollection) {
                HtmlNode nodeGroupName = nodeGroupDiv.SelectSingleNode(nodeGroupDiv.XPath + "//div[@class='mk-accordion-tab']");
                if (nodeGroupName == null) {
                    Console.WriteLine("nodeGroupName == null");
                    continue;
                }

                Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
                    Name = ClearString(nodeGroupName.InnerText),
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

        public void ParseSiteSochiMcDanielRu(HtmlDocument docServices) {
            HtmlNodeCollection nodeCollection = htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (nodeCollection != null) {
                foreach (HtmlNode nodeServiceRoot in nodeCollection) {
                    HtmlNode nodeGroupName = nodeServiceRoot.SelectSingleNode(nodeServiceRoot.XPath + "//div[@class='title text-style-strong']");

                    string groupName = string.Empty;
                    if (nodeGroupName != null)
                        groupName = ClearString(nodeGroupName.InnerText);

                    Items.ServiceGroup serviceGroup = new Items.ServiceGroup {
                        Link = siteInfo.UrlServicesPage,
                        Name = groupName
                    };

                    HtmlNodeCollection nodeCollectionServices = nodeServiceRoot.SelectNodes(nodeServiceRoot.XPath + "//li[@class='item closed']");

                    foreach (HtmlNode nodeService in nodeCollectionServices) {
                        string[] values = nodeService.InnerText.Split(new string[] { "&nbsp;" }, StringSplitOptions.None);
                        if (values.Length == 0)
                            continue;

                        string price = string.Empty;
                        for (int i = values.Length - 1; i >= 0; i--) {
                            string value = ClearString(values[i]).Replace("₽", "").TrimStart(' ').TrimEnd(' ');
                            if (int.TryParse(value, out int priceValue)) {
                                price = ClearString(values[i]);
                                break;
                            }
                        }

                        if (string.IsNullOrEmpty(price))
                            continue;

                        string name = ClearString(nodeService.InnerText).Replace(price, "").TrimStart(' ').TrimEnd(' ');

                        //Console.WriteLine("name: '" + name + "'");
                        //Console.WriteLine("price: '" + price + "'");

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

        public void ParseSiteSochi5vracheyCom(HtmlDocument docServices) {
            string url = "https://wix-visual-data.appspot.com/app/file?" +
                "compId=comp-jgdgts3t&instance=OEUK2YDKHQjGmlBfbueJESLm" +
                "XL2Wi97Uajq1SZVfccI.eyJpbnN0YW5jZUlkIjoiNjE1M2Y3ZWQtOW" +
                "RjYS00ODgzLTlhYTUtMTBjZjI0MWNmNTBjIiwiYXBwRGVmSWQiOiIx" +
                "MzQxMzlmMy1mMmEwLTJjMmMtNjkzYy1lZDIyMTY1Y2ZkODQiLCJtZX" +
                "RhU2l0ZUlkIjoiNDc1MjBkMTktYmQ5MS00MjBhLTk1MTgtOWMxMzY3" +
                "ZTdiNmQ3Iiwic2lnbkRhdGUiOiIyMDE5LTA0LTI0VDEzOjE4OjI4Lj" +
                "U1N1oiLCJ1aWQiOm51bGwsImlwQW5kUG9ydCI6IjkyLjI0Mi41My45" +
                "NC81MDcxNCIsInZlbmRvclByb2R1Y3RJZCI6bnVsbCwiZGVtb01vZG" +
                "UiOmZhbHNlLCJhaWQiOiJhMTAzOGRlOS1iYzViLTQzZGMtODI3Yy0z" +
                "NGUyYjUxMTU1MmQiLCJiaVRva2VuIjoiMjYwMWZhZjQtMjA1Yi0wYT" +
                "g5LTBmYmQtOGNkYzQzZmI0M2RiIiwic2l0ZU93bmVySWQiOiJhY2I4" +
                "ZTE1ZS04MjBmLTRhNWYtYmJmOS0wMzhmZDY0OWIwOGQifQ";

            HtmlDocument docManuallyLoaded = htmlAgility.GetDocument(url, siteInfo);

            //HtmlNodeCollection nodeCollection = _htmlAgility.GetNodeCollection(docServices, siteInfo.XPathServices);
            if (docManuallyLoaded == null) {
                Console.WriteLine("docManuallyLoaded == null");
                return;
            }

            Items.ServiceGroup itemServiceGroup = new Items.ServiceGroup() {
                Name = "",
                Link = siteInfo.UrlServicesPage
            };

            string[] lines = docManuallyLoaded.DocumentNode.InnerText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string line in lines) {
                int index = line.LastIndexOf(',');
                string name = line.Substring(0, index);
                string price = line.Substring(index + 1, line.Length - index - 1);

                Items.Service itemService = new Items.Service() {
                    Name = name,
                    Price = price
                };

                itemServiceGroup.ServiceItems.Add(itemService);
            }

            siteInfo.ServiceGroupItems.Add(itemServiceGroup);
        }

        public void ParseSiteSochi23docRuDoctors(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTbodies = "//ul[@class='accordion_square accordion-rounded2']//tbody";
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
