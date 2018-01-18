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

			Console.WriteLine(hc.DefaultRequestHeaders.TryAddWithoutValidation(
				"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8"));
			Console.WriteLine(hc.DefaultRequestHeaders.TryAddWithoutValidation(
				"Accept-Encoding", "gzip, deflate, br"));
			Console.WriteLine(hc.DefaultRequestHeaders.TryAddWithoutValidation(
				"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36"));
			Console.WriteLine(hc.DefaultRequestHeaders.TryAddWithoutValidation(
				"Accept-Charset", "ISO-8859-1"));


			if (isLocalFile) {
				string html = File.ReadAllText(url, Encoding.GetEncoding("windows-1251"));
				doc.LoadHtml(html);
			} else {
				HttpResponseMessage result = hc.GetAsync(url).Result;
				Stream stream = result.Content.ReadAsStreamAsync().Result;
				StreamReader streamReader = new StreamReader(stream, true);
				
				doc.Load(streamReader);

				result.Dispose();
				stream.Close();
				stream.Dispose();
				streamReader.Close();
				streamReader.Dispose();
			}
			
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
