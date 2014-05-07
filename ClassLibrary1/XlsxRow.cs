using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyConnector
{
    class XlsxRow
    {
        public string Sku { get; set; }
        public int Quantity { get; set; }


        public static IEnumerable<XlsxRow> LoadFromFile(string fileName)
        {
            var package = new ExcelPackage(new FileInfo(fileName));
            ExcelWorksheet sheet = package.Workbook.Worksheets.First();
            var rows = ExtractValuesFromRange(sheet.Cells["d:d"]);
            return rows;
        }

        private static IEnumerable<XlsxRow> ExtractValuesFromRange(ExcelRangeBase rows)
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
                    yield return new XlsxRow() { Sku = sku, Quantity = qty };
                }
            }
        }

    }
}
