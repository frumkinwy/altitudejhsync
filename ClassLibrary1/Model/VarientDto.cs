using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyConnector.Model
{
    class VarientDto
    {
        [JsonProperty("variant")]
        public Variant Variant { get; set; }
    }
}
