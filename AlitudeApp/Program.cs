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
            string csvFile = "WEBEXPORT.xlsx";// args[0];

            var settings = Properties.Settings.Default;
            var config = new PrivateApiConfiguration(
                settings.BaseUrl,
                settings.ApiKey,
                settings.Password);

            var api = new ApiWrapper(config);

            var updater = new InventoryUpdater(api);
            int successCount, errorCount;
            updater.UpdateInventoryQuantities(csvFile, out successCount, out errorCount);
        }
    }
}
