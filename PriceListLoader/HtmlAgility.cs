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
		private	HttpClient hc = new HttpClient();

		public HtmlDocument GetDocument(string url, bool isLocalFile = false) {
			HtmlDocument doc = new HtmlDocument();
			string html;
			
			if (isLocalFile) {
				html = File.ReadAllText(url, Encoding.UTF8); //"windows-1251"
			} else {

				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
				request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";

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
