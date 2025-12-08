using DMSA.Sync.Core.Database.Sqlite;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> FixInventory()
        {
            var productDb = new ProductProductDb(Constants.Session.odooConnection.DbNameSqlite);
            var stockQuantDb = new StockQuantDb(Constants.Session.odooConnection.DbNameSqlite);

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
