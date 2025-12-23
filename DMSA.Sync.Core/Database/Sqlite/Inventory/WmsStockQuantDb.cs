using DMSA.Models.Odoo.Native.Inventory;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class WmsStockQuantDb : SqliteDbBase<wms_stock_quant>
    {
        public WmsStockQuantDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            Task.Run(async () =>
            {
                await InitializeAsync();
            });
        }

        public async Task InitializeAsync()
        {
            await Init();
            await Database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_wms_stock_quant_product_id ON stock_quant(_product_id)");
            await Database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_wms_stock_quant_warehouse_id ON stock_quant(_warehouse_id)");
            await Database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_wms_stock_quant_location_id ON stock_quant(_location_id)");            
        }

        public async Task<wms_stock_quant> GetItem(int id)
        {
            return await GetItemAsync(x => x.id == id);
        }

        public async Task<List<wms_stock_quant>> GetItemsAsync(int product_id)
        {
            return await GetItemsAsync(x => x._product_id == product_id);
        }

        public async Task UpdateCantidadDisponibleAsync()
        {
            string sql = @"
                    UPDATE product_product
                    SET cantidad_disponible = (
                        SELECT IFNULL(SUM(cantidad_disponible), 0)
                        FROM wms_stock_quant
                        WHERE wms_stock_quant._product_id = product_product.id
                    );
                ";

            await Database.ExecuteAsync(sql);
        }

        public async Task UpdateCantidadDisponibleAsync(int[] wh_ids)
        {
            string sql = @"
                    UPDATE product_product
                    SET cantidad_disponible = (
                        SELECT IFNULL(SUM(cantidad_disponible), 0)
                        FROM wms_stock_quant
                        WHERE wms_stock_quant._product_id = product_product.id
                        and wms_stock_quant._warehouse_id IN (" + string.Join(",", wh_ids) + @")
                    );
                ";

            await Database.ExecuteAsync(sql);
        }
    }
}
