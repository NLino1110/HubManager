
using DMSA.Models.Odoo.Sales;

namespace DMSA.Sync.Core.Database.Sqlite.Sales
{
    public class ProductPricelistItemDb : SqliteDbBase<product_pricelist_item>
    {
        public ProductPricelistItemDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            
        }

        protected override async Task OnAfterInit()
        {
            await Database.RunInTransactionAsync(tran =>
            {
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_product_pricelist_item__pricelist_id ON product_pricelist_item(_pricelist_id)");
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_product_pricelist_item__product_tmpl_id ON product_pricelist_item(_product_tmpl_id)");

            });
        }

        public async Task<product_pricelist_item> GetItem(int id)
        {
            return await GetItemAsync(x => x.id == id);
        }

        public async Task<List<product_pricelist_item>> GetItemsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<product_pricelist_item>();
            var ids_list = ids.ToList();
            return await GetItemsAsync(x => ids_list.Contains(x.id));
        }
    }
}
