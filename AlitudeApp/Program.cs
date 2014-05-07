using CsvHelper;
using CsvHelper.Configuration;
using ShopifyConnector;
using ShopifyConnector.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlitudeApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //specify file
            string csvFile = "testinput.csv";// args[0];

            // this is my test store using a private api key
            // it is yet to be tested using the public app interface as more code will need to be added
            var config = new PrivateApiConfiguration(
                "https://testshop1-92.myshopify.com/admin/",
                "73cfbcf2734ee2f0bb4693ac8ce99dbd",
                "7d2c8f2110f9bf2e160a5cc3f5632bce");
            var api = new ApiWrapper(config);

            CsvToShopify.UpdateInventoryQuantities(csvFile, api);
        }
    }
}
