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
            out int errorCount,
            int concurrentRequests = 5)
        {
            IList<Product> products = Api.GetProducts();
            IDictionary<string, Variant> variants = products
                .SelectMany(x => x.Variants)
                .ToDictionary(x => x.Sku, x => x);

            int localSuccessCount = 0;
            object successLock = new Object();
            int localErrorCount = 0;
            object errorLock = new Object();

            FileStream file = File.OpenRead(xslxFile);
            var package = new ExcelPackage(file);

            ExcelWorksheet sheet = package.Workbook.Worksheets.First();
            var rows = sheet.Cells["d:d"];
             
            Parallel.ForEach(
                rows,
                new ParallelOptions() { MaxDegreeOfParallelism = concurrentRequests },
                qtyCell =>
                {
                    int qty;
                    int sku;
                    ExcelRangeBase skuCell = qtyCell.Offset(0, 2);
                    if (qtyCell == null ||
                        qtyCell.Value == null ||
                        !int.TryParse(qtyCell.Value.ToString(), out qty) ||
                        skuCell == null ||
                        skuCell.Value == null ||
                        !int.TryParse(skuCell.Value.ToString(), out sku))
                    {
                        return;
                    }

                    try
                    {
                        var variant = variants[sku.ToString()];
                        variant.InventoryQuantity = qty;

                        Api.SetVariant(variant);
                        lock (successLock)
                        {
                            localSuccessCount++;
                        }
                    }
                    catch (ShopifyApiException ex)
                    {
                        Console.WriteLine("Error updating variant (" + sku + "):" + ex.Message);
                        lock (errorLock)
                        {
                            localErrorCount++;
                        }
                    }
                    catch (KeyNotFoundException) // Thrown by single()
                    {
                        Console.WriteLine("Variant not found: " + sku);
                        lock (errorLock)
                        {
                            localErrorCount++;
                        }
                    }
                });

            successCount = localSuccessCount;
            errorCount = localErrorCount;

            Console.WriteLine("Successes: " + successCount);
            Console.WriteLine("Errors: " + errorCount);
            Console.ReadKey();
        }
    }
}
