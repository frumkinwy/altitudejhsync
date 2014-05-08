using OfficeOpenXml;
using ShopifyConnector.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShopifyConnector
{
    public class InventoryUpdater
    {
        private const string UnmanagedLog = "unmanaged.txt";
        private const string UnmatchedSkuLog = "unmatchedskus.txt";
        private const string ErrorLog = "err.txt";
        private const string SuccessLog = "out.txt";

        public ApiWrapper Api { get; set; }

        public InventoryUpdater(ApiWrapper api)
        {
            this.Api = api;
        }

        public void UpdateInventoryQuantities(
            string xlsxFile, 
            out int successCount, 
            out int errorCount)
        {
            // Get products from shopify
            IList<Product> products = Api.GetProducts();
            // The variants of all products
            var variants = products.SelectMany(x => x.Variants);

            // Only select variants that are managed by shopify
            var managed = variants.Where(x => x.InventoryManagement == "shopify");

            var unmanaged = variants.Except(managed);
            File.AppendAllText(UnmanagedLog, "#Started " + DateTime.Now + Environment.NewLine);
            foreach (var variant in unmanaged)
            {
                File.AppendAllText(UnmanagedLog, variant.Sku + Environment.NewLine);
            }
    
            Console.WriteLine("Managed Variant Count: " + managed.Count());

            // load update data from spreadsheet
            var updateValues = XlsxRow.LoadFromFile(xlsxFile);

            Console.WriteLine("Update Rows Count: " + updateValues.Count());

            // join existing products with update values on SKU
            var joined = from ma in managed
                         join ud in updateValues
                         on ma.Sku.Trim() equals ud.Sku.Trim()
                         select new
                         {
                             Variant = ma,
                             UpdateValues = ud
                         };
            

            var unjoined = variants.Except(joined.Select(x => x.Variant));
            File.AppendAllText(UnmatchedSkuLog, "#Started " + DateTime.Now + Environment.NewLine);
            foreach (var variant in unjoined)
            {
                File.AppendAllText(UnmatchedSkuLog, variant.Sku + Environment.NewLine);
            }

            Console.WriteLine("Joined: " + joined.Count());
            Console.WriteLine("Unjoined: " + unjoined.Count());

            // only update if qty has changed
            var updates = joined.Where(x => x.UpdateValues.Quantity != x.Variant.InventoryQuantity);

            Console.WriteLine("Updates: " + updates.Count());

            // initialise counters and their locks
            int localSuccessCount = 0;
            object successLock = new Object();
            int localErrorCount = 0;
            object errorLock = new Object();

            File.AppendAllText(SuccessLog, "#Started " + DateTime.Now + Environment.NewLine);
            File.AppendAllText(ErrorLog, "#Started " + DateTime.Now + Environment.NewLine);

            // issue requests concurrently as async task
            IList<Task> tasks = new List<Task>();
            foreach (var update in updates)
            {
                Thread.Sleep(500); // only issue 2 requests per second as per shopify API limits
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

            File.AppendAllText(SuccessLog, "Count: " + successCount + Environment.NewLine);
            File.AppendAllText(ErrorLog, "Count: " + DateTime.Now + Environment.NewLine);

            Console.WriteLine("Successes: " + successCount);
            Console.WriteLine("Errors: " + errorCount);
            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
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
                    File.AppendAllText(SuccessLog, "Updated " + variant.Sku + Environment.NewLine);
                }
                Console.Write("Updated " + variant.Sku + Environment.NewLine);
            }
            catch (ShopifyApiException ex)
            {
                Console.WriteLine("Error updating variant (" + variant.Sku + "):" + ex.Message);
                lock (errorLock)
                {
                    File.AppendAllText(ErrorLog,
                        "Error updating variant (" + variant.Sku + "):" + ex.ToString() + Environment.NewLine);
                    localErrorCount++;
                }
            }
        }
    }
}
