using DMSA.Models.Odoo.Native.Inventory;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class WmsStockQuantDb : SqliteDbBase<wms_stock_quant>
    {
        public WmsStockQuantDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            
        }

        protected override async Task OnAfterInit()
        {
            await Database.RunInTransactionAsync(tran =>
            {
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_wms_stock_quant_product_id ON wms_stock_quant(_product_id)");
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_wms_stock_quant_warehouse_id ON wms_stock_quant(_warehouse_id)");
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_wms_stock_quant_location_id ON wms_stock_quant(_location_id)");
            });
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
            await Init();
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
            await Init();

            string sql = @"
                UPDATE product_product
                SET cantidad_disponible = (
                    SELECT IFNULL(SUM(cantidad_disponible), 0)
                    FROM wms_stock_quant
                    WHERE wms_stock_quant._product_id = product_product.id
                      AND wms_stock_quant._warehouse_id IN (" + string.Join(",", wh_ids) + @")
                )
                WHERE cantidad_disponible <> (
                    SELECT IFNULL(SUM(cantidad_disponible), 0)
                    FROM wms_stock_quant
                    WHERE wms_stock_quant._product_id = product_product.id
                      AND wms_stock_quant._warehouse_id IN (" + string.Join(",", wh_ids) + @")
                );
            ";

            //string sql = @"
            //    UPDATE product_product p
            //    JOIN (
            //        SELECT _product_id, IFNULL(SUM(cantidad_disponible), 0) AS total
            //        FROM wms_stock_quant
            //        WHERE _warehouse_id IN (" + string.Join(",", wh_ids) + @")
            //        GROUP BY _product_id
            //    ) q ON q._product_id = p.id
            //    SET p.cantidad_disponible = q.total
            //    WHERE p.cantidad_disponible <> q.total;
            //";

            await Database.ExecuteAsync(sql);
        }
    }
}
