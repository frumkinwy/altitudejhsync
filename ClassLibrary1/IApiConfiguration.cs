using System;
using System.Net.Http;
namespace ShopifyConnector
{
    public interface IApiConfiguration
    {
        string BaseUrl { get; }
        global::System.Net.WebRequest CreateRequest(string resourcePath, HttpMethod httpMethod);
    }
}
