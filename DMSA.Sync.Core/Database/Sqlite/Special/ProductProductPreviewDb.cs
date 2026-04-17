using DMSA.Models.Odoo.Native;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class ProductProductPreviewDb : SqliteDbBase<product_product_preview>
    {
        public ProductProductPreviewDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            //DatabaseFilename = DatabaseFilename + "_static";
        }

        protected override async Task OnAfterInit()
        {
            await Database.RunInTransactionAsync(tran =>
            {
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_product_product_preview_id ON product_product_preview(id)");
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_product_product_preview_code ON product_product_preview(code)");
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_product_product_preview_product_tmpl_id ON product_product_preview(_product_tmpl_id)");
            });
        }

        public async Task Reset()
        {
            await Init();

            await Database.ExecuteAsync(@"
                    UPDATE product_product_preview
                    SET image_256 = NULL,
                        image_1920 = NULL
                    WHERE image_256 IS NOT NULL
                       OR image_1920 IS NOT NULL
                ");
        }

        public async Task<int> UpdateImagesBatchAsync(List<product_product> items)
        {
            if (items == null || items.Count == 0)
                return 0;

            await Init();

            int count = 0;

            await Database.RunInTransactionAsync(tran =>
            {
                foreach (var item in items)
                {
                    tran.Execute(
                        @"UPDATE product_product_preview 
                  SET image_256 = ?, 
                      image_1920 = ?
                  WHERE id = ?",
                        item.image_256,
                        item.image_1920,
                        item.id
                    );

                    count++;
                }
            });

            return count;
        }

        public async Task<string> GetBase65_256(product_product product)
        {
            await Init();
            string Base64Source = string.Empty;
            var data = await Database.Table<product_product_preview>().Where(x=> x.id == product.id).FirstOrDefaultAsync();

            if (data != null)
            {
                Base64Source = data.image_256 is string s &&
                        !string.IsNullOrWhiteSpace(s) &&
                        !s.Equals("false", StringComparison.OrdinalIgnoreCase)
                            ? s
                            : string.Empty;
            }
            return Base64Source;
        }

        public async Task<string> GetBase65_1920(product_product product)
        {
            await Init();
            string Base64Source = string.Empty;
            var data = await Database.Table<product_product_preview>().Where(x => x.id == product.id).FirstOrDefaultAsync();

            if (data != null)
            {
                Base64Source = data.image_1920 is string s &&
                        !string.IsNullOrWhiteSpace(s) &&
                        !s.Equals("false", StringComparison.OrdinalIgnoreCase)
                            ? s
                            : string.Empty;
            }
            return Base64Source;
        }
    }
}
