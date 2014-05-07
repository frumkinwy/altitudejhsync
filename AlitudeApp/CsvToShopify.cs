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
    public class CsvToShopify
    {
        public static void UpdateInventoryQuantities(string csvFile, ApiWrapper api)
        {
            IList<Product> products = api.GetProducts();

            Int32 successCount = 0;
            object successLock = new Object();
            Int32 errorCount = 0;
            object errorLock = new Object();

            CsvReader csv = new CsvReader(File.OpenText(csvFile));
            csv.Configuration.RegisterClassMap<InputRowMap>();

            Parallel.ForEach(
                csv.GetRecords<InputRow>(),
                new ParallelOptions() { MaxDegreeOfParallelism = 10 },
                row =>
                {
                    var variant = products
                        .SelectMany(x => x.Variants)
                        .Where(x => x.Sku == row.Sku)
                        .Single();
                    variant.InventoryQuantity = row.Quantity;

                    try
                    {
                        api.SetVariant(variant);
                        lock (successLock)
                        {
                            successCount++;
                        }
                        Console.WriteLine("Updated " + row.Sku);
                    }
                    catch (ShopifyApiException ex)
                    {
                        Console.WriteLine("Error updating variant (" + row.Sku + "):" + ex.Message);
                        lock (errorLock)
                        {
                            errorCount++;
                        }
                    }
                });

            Console.WriteLine("Successes: " + successCount);
            Console.WriteLine("Errors: " + errorCount);
            Console.ReadKey();
        }
    }

    internal class InputRow
    {
        public string Sku { get; set; }
        public int Quantity { get; set; }
    }

    internal class InputRowMap : CsvClassMap<InputRow>
    {
        [Obsolete]
        public override void CreateMap()
        {
            Map(m => m.Sku).Name("sku");
            Map(m => m.Quantity).Name("qty");
        }
    }
}
