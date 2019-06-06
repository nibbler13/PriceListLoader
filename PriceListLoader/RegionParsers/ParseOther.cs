using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PriceListLoader.RegionParsers {
    class ParseOther : ParseGeneral {
        public ParseOther(HtmlAgility htmlAgility, BackgroundWorker bworker, SiteInfo siteInfo) : base(htmlAgility, bworker, siteInfo) { }

        public void ParseSiteKovrovClinicalcenterRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTbodies = "//div[@class='tprice']//tbody";
            HtmlNodeCollection nodeCollectionTbodies = htmlAgility.GetNodeCollection(docService, xPathTbodies);
            if (nodeCollectionTbodies == null) {
                Console.WriteLine("nodeCollectionTbodies == null");
                return;
            }

            foreach (HtmlNode nodeTbody in nodeCollectionTbodies) {
                List<Items.Service> serviceItems = ReadTrNodes(nodeTbody, 1, 1);
                itemServiceGroup.ServiceItems.AddRange(serviceItems);
            }
        }

        public void ParseSiteNedorezovMcRu(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup) {
            string xPathTbodies = "//tbody[@id='grid_tab']//tr";
            HtmlNodeCollection nodeCollectionTbodies = htmlAgility.GetNodeCollection(docService, xPathTbodies);
            if (nodeCollectionTbodies == null) {
                Console.WriteLine("nodeCollectionTbodies == null");
                return;
            }

            foreach (HtmlNode nodeTbody in nodeCollectionTbodies) {
                HtmlNodeCollection htmlNodesTd = nodeTbody.SelectNodes(nodeTbody.XPath + "//td");
                if (htmlNodesTd == null || htmlNodesTd.Count != 9) {
                    Console.WriteLine("htmlNodesTd == null || htmlNodesTd.Count != 9");
                    continue;
                }

                Items.Service service = new Items.Service {
                    Name = ClearString(htmlNodesTd[0].InnerText),
                    R1 = ClearString(htmlNodesTd[1].InnerText),
                    Type = ClearString(htmlNodesTd[2].InnerText),
                    Lenght = ClearString(htmlNodesTd[3].InnerText),
                    Metalbase = ClearString(htmlNodesTd[4].InnerText),
                    Price1t = ClearString(htmlNodesTd[5].InnerText),
                    Price5t = ClearString(htmlNodesTd[6].InnerText),
                    Price10t = ClearString(htmlNodesTd[7].InnerText)
                };

                itemServiceGroup.ServiceItems.Add(service);
            }
        }

        public void ParseSiteNedorezovPromMetallKz(HtmlDocument docService, ref Items.ServiceGroup itemServiceGroup, string pageLink = "") {
            string xPathLoop = "//ul[starts-with(@class,'product-loop-categories')]//li//a[@href]";
            HtmlNodeCollection nodeCollectionLoop = htmlAgility.GetNodeCollection(docService, xPathLoop);
            if (nodeCollectionLoop != null) {
                siteInfo.XPathServices = xPathLoop;
                ParseSiteWithLinksOnMainPage(docService, false, itemServiceGroup.Name);
                return;
            }

            string xPathQuantity = "//form[@class='form-electro-wc-ppp']";
            HtmlNode nodeQuantity = docService.DocumentNode.SelectSingleNode(xPathQuantity);

            if (nodeQuantity != null) {
                Dictionary<string, string> paramDict = new Dictionary<string, string> { { "ppp", "768" } };
                if (string.IsNullOrEmpty(pageLink))
                    pageLink = itemServiceGroup.Link;

                docService = htmlAgility.ExecutePost(pageLink, paramDict);
            }

            string xPathServices = "//ul[starts-with(@class,'products')]//li[starts-with(@class,'product')]";
            HtmlNodeCollection nodeCollectionService = htmlAgility.GetNodeCollection(docService, xPathServices);

            if (nodeCollectionService == null) {
                Console.WriteLine("nodeCollectionService == null");
                return;
            }

            foreach (HtmlNode nodeService in nodeCollectionService) {
                HtmlNode nodeServiceName = nodeService.SelectSingleNode(nodeService.XPath + "//h2[@class='woocommerce-loop-product__title']");
                if (nodeServiceName == null) {
                    Console.WriteLine("");
                    continue;
                }

                Items.Service service = new Items.Service { Name = ClearString(nodeServiceName.InnerText) };
                itemServiceGroup.ServiceItems.Add(service);
            }

            string xPathNextPage = "//nav[@class='electro-advanced-pagination']//a[@class='next page-numbers' and @href]";
            HtmlNode nodeNextPage = docService.DocumentNode.SelectSingleNode(xPathNextPage);

            if (nodeNextPage == null)
                return;

            string link = nodeNextPage.Attributes["href"].Value;
            if (string.IsNullOrEmpty(link))
                return;

            backgroundWorker.ReportProgress((int)progressCurrent, "Загрузка страницы: " + link);

            HtmlDocument htmlDocument = null;

            for (int i = 0; i < 5; i++) {
                htmlDocument = htmlAgility.GetDocument(link, siteInfo, backgroundWorker, (int)progressCurrent);

                if (htmlDocument != null)
                    break;
            }

            if (htmlDocument == null) {
                backgroundWorker.ReportProgress((int)progressCurrent, "!!! Не удалось загрузить страницу");
                return;
            }

            ParseSiteNedorezovPromMetallKz(htmlDocument, ref itemServiceGroup, link);
        }
    }
}
