using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyConnector.Model
{
    public class Product
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("variants")]
        public IList<Variant> Variants { get; set; }
    }
}
