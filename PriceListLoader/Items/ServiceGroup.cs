using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceListLoader.Items {
    public class ServiceGroup {
        public string Name { get; set; }
        public string Link { get; set; }
        public List<Service> ServiceItems { get; set; } = new List<Service>();
    }
}
