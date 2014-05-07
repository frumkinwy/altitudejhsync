using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyConnector
{
    // This is not tested in any way, it will 99% not work
    public class PublicApiConfiguration : IApiConfiguration
    {
        public string BaseUrl { get; private set; }
        public string ApiKey { get; private set; }
        public string Secret { get; private set; }

        public PublicApiConfiguration(string baseUrl, string apiKey, string secret)
        {
            this.BaseUrl = baseUrl;
            this.ApiKey = apiKey;
            this.Secret = secret;
        }

        public WebRequest CreateRequest(string resourcePath, HttpMethod httpMethod)
        {
            WebRequest request = HttpWebRequest.Create(BaseUrl + resourcePath);

            request.Method = httpMethod.Method;
            request.ContentType = "application/json; charset=utf-8";

            byte[] passwordBytes = Encoding.ASCII.GetBytes(Secret + Secret); //this is how api php does it
            string passwordMD5;
            using (var md5 = MD5.Create())
            {
                passwordMD5 = BitConverter.ToString(md5.ComputeHash(passwordBytes));
            }

            string authInfo = ApiKey + ":";// + Password; TODO Fix this
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;

            return request;
        }
    }
}
