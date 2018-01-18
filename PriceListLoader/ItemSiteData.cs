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
		public string Name { get; set; }
		public string Price { get; set; }
	}
}
