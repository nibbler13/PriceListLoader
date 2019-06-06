using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceListLoader.Items {
    public class Service {
        private static readonly Dictionary<string, string> toReplace = new Dictionary<string, string>() {
            { "р.", "" },
            { " руб.", "" },
            { "руб.", "" },
            { " руб", "" },
            { ",00", "" },
            { " ₽", "" },
            { ".00", "" },
            { " ф", "" },
            { " i", "" },
            { " р", "" },
            { " &#1088;&#1091;&#1073;.", "" },
            { "Казань:", "" },
            { ",0", "" },
            { "RUB ₽", "" },
            { "RUB", "" }
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
                    if (newValue.Contains(item.Key))
                        newValue = newValue.Replace(item.Key, item.Value);

                price = newValue.Replace(",", "");
            }
        }

        public string R1 { get; set; }
        public string Type { get; set; }
        public string Lenght { get; set; }
        public string Metalbase { get; set; }
        public string Price1t { get; set; }
        public string Price5t { get; set; }
        public string Price10t { get; set; }
    }
}
