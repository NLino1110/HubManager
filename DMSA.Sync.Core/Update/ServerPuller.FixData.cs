using ApiManager;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> FixInventory()
        {
            var productDb = new ProductProductDb(DbNameSqlite);
            var stockQuantDb = new StockQuantDb(DbNameSqlite);

            var allProducts = await productDb.GetItemsAsync(x=>x.active && x.sale_ok);
            
            for (var i = 0; i < allProducts.Count; i++)
            {
                var product = allProducts[i];
                var stockQuants = await stockQuantDb.GetItemsAsync(product.id);
                var totalQty = stockQuants.Sum(x => x.quantity);
                if (product.qty_available != totalQty)
                {
                    Debug.WriteLine($"Fixing Product ID {product.id} Qty Available from {product.qty_available} to {totalQty}");
                    product.qty_available = (float) totalQty;
                    await productDb.UpdateAsync(product);
                }
            }

            return true;
        }
    }
}
