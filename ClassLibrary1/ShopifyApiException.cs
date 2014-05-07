using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyConnector
{
    public class ShopifyApiException : ApplicationException
    {
        public ShopifyApiException(string message, Exception innerException) : 
            base(message, innerException) { }
    }
}
