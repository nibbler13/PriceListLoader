using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;


namespace PriceListLoader {
	class HtmlAgility {
		public HtmlDocument GetDocument(string url, SiteInfo.SiteName siteName, bool isLocalFile = false) {
			HtmlDocument doc = new HtmlDocument();
			string html = string.Empty;
			
			if (isLocalFile) {
				Encoding encoding = Encoding.UTF8;
				if (siteName == SiteInfo.SiteName.msk_invitro_ru ||
					siteName == SiteInfo.SiteName.spb_invitro_ru)
					encoding = Encoding.GetEncoding("windows-1251");

				html = File.ReadAllText(url, encoding);
			} else {
				for (int i = 0; i < 5; i++) {
					try {
						HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
						request.UserAgent =
							"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36";

						if (siteName == SiteInfo.SiteName.spb_helix_ru) {
							CookieContainer cookieContainer = new CookieContainer();
							Uri uri = new Uri(url);
							Cookie cookieRegion =
								new Cookie("Region", "%D0%A1%D0%B0%D0%BD%D0%BA%D1%82-%D0%9F%D0%B5%D1%82%D0%B5%D1%80%D0%B1%D1%83%D1%80%D0%B3") { Domain = uri.Host };
							Cookie cookieRegionConfirm = new Cookie("RegionConfirm", "Yes") { Domain = uri.Host };
							cookieContainer.Add(cookieRegion);
							cookieContainer.Add(cookieRegionConfirm);
							request.CookieContainer = cookieContainer;
						}

						HttpWebResponse response = (HttpWebResponse)request.GetResponse();

						if (response.StatusCode == HttpStatusCode.OK) {
							Stream receiveStream = response.GetResponseStream();
							StreamReader readStream = null;

							if (response.CharacterSet == null) {
								readStream = new StreamReader(receiveStream);
							} else {
								readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
							}

							html = readStream.ReadToEnd();

							response.Close();
							readStream.Close();
						} else
							return doc;

						break;
					} catch (Exception e) {
						Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
					}
				}
			}

			doc.LoadHtml(html);

			return doc;
		}

		public HtmlNodeCollection GetNodeCollection(HtmlDocument doc, string xPath) {
			return doc.DocumentNode.SelectNodes(xPath);
		}

		public string GetResponse(string uri) {
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream)) {
				return reader.ReadToEnd();
			}
		}
	}
}
