using OfficeOpenXml;
using ShopifyConnector.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyConnector
{
    public class InventoryUpdater
    {
        public ApiWrapper Api { get; set; }

        public InventoryUpdater(ApiWrapper api)
        {
            this.Api = api;
        }

        public void UpdateInventoryQuantities(
            string xslxFile, 
            out int successCount, 
            out int errorCount)
        {
            // Get products from shopify
            IList<Product> products = Api.GetProducts();
            // The variants of all products
            var variants = products.SelectMany(x => x.Variants);
    
            Console.WriteLine("Variant Count: " + variants.Count());

            // load update data from spreadsheet
            FileStream file = File.OpenRead(xslxFile);
            var package = new ExcelPackage(file);
            ExcelWorksheet sheet = package.Workbook.Worksheets.First();
            var updateValues = ExtractValuesFromRange(sheet.Cells["d:d"]);

            Console.WriteLine("Update Rows Count: " + updateValues.Count());

            // join existing products with update values on SKU
            var updates = from va in variants
                          join ud in updateValues
                          on va.Sku.Trim() equals ud.Sku.Trim()
                          where va.InventoryQuantity != ud.Quantity // only update if qty has changed
                          select new
                          {
                              Variant = va,
                              UpdateValues = ud
                          };

            Console.WriteLine("Joined Updates: " + updates.Count());

            // initialise counters and their locks
            int localSuccessCount = 0;
            object successLock = new Object();
            int localErrorCount = 0;
            object errorLock = new Object();

            // issue requests concurrently as async task
            IList<Task> tasks = new List<Task>();
            foreach (var update in updates)
            {
                Task.Delay(500); // only issue 2 requests per second as per shopify API limits
                tasks.Add(Task.Run(delegate
                {
                    update.Variant.InventoryQuantity = update.UpdateValues.Quantity;

                    UpdateVariant(
                        update.Variant,
                        ref localSuccessCount,
                        successLock,
                        ref localErrorCount,
                        errorLock);
                }));
            }

            Task.WaitAll(tasks.ToArray());

            successCount = localSuccessCount;
            errorCount = localErrorCount;

            Console.WriteLine("Successes: " + successCount);
            Console.WriteLine("Errors: " + errorCount);
            Console.ReadKey();
        }

        private static IEnumerable<CsvRecord> ExtractValuesFromRange(ExcelRangeBase rows)
        {
            foreach (var qtyCell in rows)
            {
                ExcelRangeBase skuCell = qtyCell.Offset(0, 2);

                // make sure the row is valid
                if (qtyCell != null &&
                    qtyCell.Value != null &&
                    qtyCell.Value.ToString().All(x => Char.IsDigit(x)) && // a string of digits
                    skuCell != null &&
                    skuCell.Value != null &&
                    skuCell.Value.ToString().All(x => Char.IsDigit(x))) // a string of digits
                {
                    int qty = int.Parse(qtyCell.Value.ToString());
                    string sku = skuCell.Value.ToString();
                    yield return new CsvRecord() { Sku = sku, Quantity = qty };
                }
            }
        }

        private class CsvRecord
        {
            public string Sku { get; set; }
            public int Quantity { get; set; }
        }

        private void UpdateVariant(
            Variant variant, 
            ref int localSuccessCount, 
            object successLock, 
            ref int localErrorCount, 
            object errorLock)
        {
            try
            {

                // make update call to shopify
                Api.SetVariant(variant);
                lock (successLock)
                {
                    localSuccessCount++;
                    File.AppendAllText("out.txt", "Updated " + variant.Sku + Environment.NewLine);
                }
                Console.Write("Updated " + variant.Sku + Environment.NewLine);
            }
            catch (ShopifyApiException ex)
            {
                Console.WriteLine("Error updating variant (" + variant.Sku + "):" + ex.Message);
                lock (errorLock)
                {
                    File.AppendAllText("err.txt",
                        "Error updating variant (" + variant.Sku + "):" + ex.ToString() + Environment.NewLine);
                    localErrorCount++;
                }
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Variant not found: " + variant.Sku);
                lock (errorLock)
                {
                    File.AppendAllText("err.txt", "Variant not found: " + variant.Sku + Environment.NewLine);
                    localErrorCount++;
                }
            }
        }
    }
}
