using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyConnector.Model
{
    public class Variant
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("inventory_quantity")]
        public int InventoryQuantity { get; set; }

        [JsonProperty("old_inventory_quantity")]
        public int OldInventoryQuantity { get; set; }

        [JsonProperty("inventory_management")]
        public string InventoryManagement { get; set; }
    }
}
