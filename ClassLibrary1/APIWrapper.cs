using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyConnector
{
    public class APIWrapper
    {
        public APIConfiguration MyAPIConfiguration { get; set; }
        public APIWrapper(APIConfiguration api)
        {
            MyAPIConfiguration = api;
        }

        public void Connect()
        {
            
        }

        public void GetProducts()
        {
            
        }

    }
    public class APIConfiguration{
    public APIConfiguration(string apikey)
    {
        APIKey = apikey;
        }
        public string APIKey { get; set; }
        }
}
