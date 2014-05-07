using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyConnector
{
    public class PrivateApiConfiguration : IApiConfiguration
    {
        public string BaseUrl { get; private set; }
        public string apiKey { get; private set; }
        public string Password { get; private set; }

        public PrivateApiConfiguration(string baseUrl, string apiKey, string password)
        {
            this.BaseUrl = baseUrl;
            this.apiKey = apiKey;
            this.Password = password;
        }

        public WebRequest CreateRequest(string resourcePath, HttpMethod httpMethod)
        {
            WebRequest request = HttpWebRequest.Create(BaseUrl + resourcePath);

            request.Method = httpMethod.Method;
            request.ContentType = "application/json; charset=utf-8";
            string authInfo = apiKey + ":" + Password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;

            return request;
        }
    }
}
