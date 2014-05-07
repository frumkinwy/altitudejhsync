using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyConnector.Model
{
    class ProductsDto
    {
        [JsonProperty("products")]
        public IList<Product> Products { get; set; }
    }
}
