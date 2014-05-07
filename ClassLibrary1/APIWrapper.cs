using Newtonsoft.Json;
using ShopifyConnector.Model;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyConnector
{
    public class ApiWrapper
    {
        public IApiConfiguration Configuration { get; set; }
        public ApiWrapper(IApiConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IList<Product> GetProducts()
        {
            var products = new List<Product>();

            IList<Product> pageProducts;
            int page = 1;
            do
            {
                pageProducts = GetProducts(page);
                products.AddRange(pageProducts);
                page++;
            } while (pageProducts.Count > 0);

            return products;
        }

        private IList<Product> GetProducts(int page)
        {
            string uri = string.Format(ApiResources.Products, page);
            var request = Configuration.CreateRequest(uri, HttpMethod.Get);

            WebResponse response;
            try
            {
                response = request.GetResponse();
            }
            catch (HttpRequestException ex)
            {
                throw new ShopifyApiException("Unable to get products", ex);
            }

            var reader = new StreamReader(response.GetResponseStream());
            string json = reader.ReadToEnd();
            var productsDto = JsonConvert.DeserializeObject<ProductsDto>(json);

            return productsDto.Products;
            
        }

        public void SetVariant(Variant variant)
        {
            try
            {
                string uri = string.Format(ApiResources.Variant, variant.ID);
                var request = Configuration.CreateRequest(uri, HttpMethod.Put);

                var writer = new StreamWriter(request.GetRequestStream());
                string json = JsonConvert.SerializeObject(new VariantDto() { Variant = variant });
                writer.Write(json);
                writer.Close();

                WebResponse response;
                response = request.GetResponse();
            }
            catch (WebException ex)
            {
                throw new ShopifyApiException("Unable to set product variant", ex);
            }
        }
    }

    internal static class ApiResources
    {
        public const string Products = "products.json?limit=250&page={0}";
        public const string Variant = "variants/{0}.json";
    }
}
