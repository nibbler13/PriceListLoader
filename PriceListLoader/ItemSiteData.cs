using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceListLoader {
	public class ItemSiteData {
		public string SiteAddress { get; set; }
		public string CompanyName { get; set; }
		public List<ItemServiceGroup> ServiceGroupItems { get; set; }

		public ItemSiteData() {
			ServiceGroupItems = new List<ItemServiceGroup>();
		}
	}
	
	public class ItemServiceGroup {
		public string Name { get; set; }
		public string Link { get; set; }
		public List<ItemService> ServiceItems { get; set; }

		public ItemServiceGroup() {
			ServiceItems = new List<ItemService>();
		}
	}
	
	public class ItemService {
		private static Dictionary<string, string> toReplace = new Dictionary<string, string>() {
			{ "р.", "" },
			{ " руб.", "" },
			{ "руб.", "" },
			{ " руб", "" },
			{ ",00", "" },
			{ " ₽", "" },
			{ ".00", "" },
			{ " ф", "" }
		};

		public string Name { get; set; }

		private string price;
		public string Price {
			get {
				return price;
			}
			set {
				string newValue = value;
				foreach (KeyValuePair<string, string> item in toReplace)
					newValue = newValue.Replace(item.Key, item.Value);

				price = newValue;
			}
		}
	}
}
