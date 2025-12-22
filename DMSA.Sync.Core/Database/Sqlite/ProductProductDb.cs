using DMSA.Models.Odoo.Modules.Accounting;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using SQLite;
using System.Diagnostics;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class ProductProductDb : SqliteDbBase<product_product>
    {
        private Dictionary<int, decimal> cachedTaxesList = new Dictionary<int, decimal>();
        private Dictionary<int, decimal> cachedProductsWithPrices = new Dictionary<int, decimal>();
        private Dictionary<int, uom_uom> cachedUom = new();

        private Dictionary<int, string> cachedMarcas = new Dictionary<int, string>();
        private Dictionary<int, string> cachedCategorias = new Dictionary<int, string>();

        public ProductProductDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            Debug.WriteLine("Creacion de instancia ProductProductDb");
        }

        // En tu repositorio/capa de datos
        private async Task<AsyncTableQuery<product_product>> BuildQuery(
            string filter_code,
            string filter_name,
            int filter_brand,
            int filter_new,
            int filter_stock,
            int filter_sort,
            int filter_category, 
            int filter_status,
            int filter_pricelist)
        {
            await Init();

            var q = Database.Table<product_product>();

            await PreloadInfoData();

            if (filter_pricelist > 0)
            {
                await PreloadPricelistCache(filter_pricelist);
                var productTemplateIds = cachedProductsWithPrices.Keys.ToList();
                //TODO: Se quita filtro porque se necesita que se muestren todos
                //q = q.Where(p => productTemplateIds.Contains(p._product_tmpl_id));
            }

            // --- 1) Filtro por code (prioridad máxima, como tu método actual) ---
            if (!string.IsNullOrWhiteSpace(filter_code))
            {
                var raw = filter_code.Trim();

                // Si es numérico: buscar por id exacto (fallback lo haces fuera)
                if (int.TryParse(raw, out var idCode))
                {
                    q = q.Where(x => x.id == idCode || x.code.ToLower().Contains(filter_code.ToLower()) || x.barcode.ToLower().Contains(filter_code.ToLower()));                    
                    return ApplySort(q, filter_sort);
                }
                else
                {
                    var term = raw.ToLowerInvariant();                    
                    q = q.Where(x => x.code.ToLower().Contains(term) || x.barcode.ToLower().Contains(term));
                    return ApplySort(q, filter_sort);
                }
            }

            // --- 2) Resto de filtros cuando NO hay filter_code ---
            if (!string.IsNullOrWhiteSpace(filter_name))
            {
                var nameTerm = filter_name.Trim().ToLowerInvariant();
                q = q.Where(x => x.name.ToLower().Contains(nameTerm));
            }

            if (filter_brand > 0)
                q = q.Where(x => x._general_marca_id == filter_brand);

            //if (filter_new == 1)
            //    q = q.Where(x => x.is_new);

            if(filter_category > 0)
                q = q.Where(x => x._general_categoria_id == filter_category);

            if (filter_stock == 1)
                q = q.Where(x => x.qty_available > 0);            

            if(filter_status == 1) // Activos
            {
                q = q.Where(x => x.active == true);
            }
            else if(filter_status == 2) // Inactivos
            {
                q = q.Where(x => x.active == false);
            }

            // --- 3) Orden ---
            q = ApplySort(q, filter_sort);

            return q;
        }

        public async Task PreloadInfoData()
        {
            if (cachedUom.Count > 0)
                return;

            var items = await Database.Table<uom_uom>()
                .Where(x => x.active == true)
                .ToArrayAsync();

            cachedUom = items.ToDictionary(uom=>uom.id, uom=>uom);

            var itemsMarcas = await Database.Table<product_marca>()
                .Where(x => x.active == true)
                .ToArrayAsync();

            cachedMarcas = itemsMarcas.ToDictionary(marca => marca.id, marca => marca.name);

            var itemsCategs = await Database.Table<product_categoria>()
                .Where(x => x.active == true)
                .ToArrayAsync();

            cachedCategorias = itemsCategs.ToDictionary(categ => categ.id, categ => categ.name);
        }

        public async Task PreloadPricelistCache(int pricelistId)
        {
            if (cachedProductsWithPrices.Count > 0)
                return;

            var items = await Database.Table<product_pricelist_item>()
                .Where(x => x._pricelist_id == pricelistId)
                .ToArrayAsync();

            cachedProductsWithPrices = items
                .GroupBy(i => i._product_tmpl_id)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().fixed_price // o price, price_discount, amount, etc.
                );
        }

        public async Task PreloadTaxeslistCache()
        {
            if (cachedTaxesList.Count > 0)
                return;

            var accountTaxDb = new AccountTaxDb(Constants.Session.odooConnection.DbNameSqlite);
            var tax_sale = await accountTaxDb.GetItemsAsync(x=>x.active);

            var items = await Database.Table<AccountTax>()
                .ToArrayAsync();

            cachedTaxesList = items.ToDictionary(x => x.id, x => (decimal) x.amount);
        }


        private static AsyncTableQuery<product_product> ApplySort(
            AsyncTableQuery<product_product> q, int filter_sort)
        {
            // 1: sequence ASC, 2: code ASC, 3: name ASC; default: id ASC
            return filter_sort switch
            {
                1 => q.OrderBy(x => x.id),
                2 => q.OrderBy(x => x.code),
                3 => q.OrderBy(x => x.name),
                _ => q.OrderBy(x => x.id)
            };
        }


        public async Task<(IList<product_product> Items, int Total)> GetPagedAsync(
            string filter_code, string filter_name, int filter_brand, int filter_new, int filter_stock, int filter_sort,
            int filter_category, int filter_status,
            int filter_pricelist,
            int page, int pageSize, CancellationToken ct = default)
        {
            if (filter_pricelist == -1)
            {
                return (new List<product_product>(), 0);
            }

            var q = await BuildQuery(filter_code, filter_name, filter_brand, filter_new, filter_stock, filter_sort, filter_category, filter_status, filter_pricelist);

            // COUNT(*) en SQLite, sin traer datos
            var total = await q.CountAsync();

            // LIMIT/OFFSET en SQLite (Skip/Take sobre AsyncTableQuery)
            var offset = Math.Max(0, (page - 1) * pageSize);
            var items = await q.Skip(offset).Take(pageSize).ToListAsync();

            
            foreach (var p in items)
            {
                if (filter_pricelist > 0)
                {
                    if (cachedProductsWithPrices.TryGetValue(p._product_tmpl_id, out var price))
                    {
                        p.list_price = (float) price;
                    }
                    else
                    {
                        p.list_price = 0;
                    }
                }

                p.uom_display = cachedUom.TryGetValue(p._uom_id, out var uom) ? uom.clave_externa : "";
                p.marca_display = cachedMarcas.TryGetValue(p._general_marca_id, out var marcaName) ? marcaName : "";
                p.categoria_display = cachedCategorias.TryGetValue(p._general_categoria_id, out var categName) ? categName : "";
            }

            return (items, total);
        }

        public async Task<product_product> GetItem(int id)
        {
            await Init();
            return await Database.Table<product_product>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<product_product> GetByProductTemplate(int product_template_id, int filter_pricelist)
        {
            await Init();
            await PreloadPricelistCache(filter_pricelist);
            await PreloadTaxeslistCache();

            var product_return = await Database.Table<product_product>().Where(x => x._product_tmpl_id == product_template_id).FirstOrDefaultAsync();

            if(product_return == null)
                return null;

            var item_uom = await Database.Table<uom_uom>()
                .Where(x => x.id == product_return._uom_id).FirstOrDefaultAsync();

            product_return.uom_display = item_uom != null ? item_uom.clave_externa : "";

            decimal factor_iva = 1;
            if (cachedTaxesList.TryGetValue(product_return._taxes_id, out var tax_sale))
            {
                decimal iva_tax = tax_sale; //15m;
                factor_iva = 1 + (iva_tax / 100m);
            }

            if (cachedProductsWithPrices.TryGetValue(product_return._product_tmpl_id, out var price))
            {
                product_return.list_price = (float)price;
            }
            else
            {
                product_return.list_price = 0;
            }
            decimal price_list_value = (decimal)product_return.list_price;
            decimal price_without_iva = Math.Round(price_list_value / factor_iva, 7);
            product_return.list_price = (float)price_without_iva;

            return product_return;
        }

        public async Task<List<product_product>> GetByProductsTemplate(int[] product_template_ids, int filter_pricelist)
        {
            await Init();

            await PreloadPricelistCache(filter_pricelist);
            await PreloadTaxeslistCache();

            var items = await Database.Table<uom_uom>()
                .Where(x => x.active == true)
                .ToArrayAsync();

            cachedUom = items.ToDictionary(uom => uom.id, uom => uom);

            var products = await Database.Table<product_product>().Where(x => product_template_ids.Contains(x._product_tmpl_id)).ToListAsync();

            if(products == null || products.Count == 0)
                return new List<product_product>();

            foreach (var p in products)
            {
                p.uom_display = cachedUom.TryGetValue(p._uom_id, out var uom) ? uom.clave_externa : "";

                decimal factor_iva = 1;
                if (cachedTaxesList.TryGetValue(p._taxes_id, out var tax_sale))
                {
                    decimal iva_tax = tax_sale; //15m;
                    factor_iva = 1 + (iva_tax / 100m);
                }

                if (cachedProductsWithPrices.TryGetValue(p._product_tmpl_id, out var price))
                {
                    p.list_price = (float)price;
                }
                else
                {
                    p.list_price = 0;
                }

                decimal price_list_value = (decimal) p.list_price;
                decimal price_without_iva = Math.Round(price_list_value / factor_iva, 7);

                p.list_price = (float) price_without_iva;
            }

            return products;
        }

        public async Task<List<product_product>> GetByProductsIds(int[] product_ids, int filter_pricelist)
        {
            await Init();

            await PreloadPricelistCache(filter_pricelist);
            await PreloadTaxeslistCache();

            var items = await Database.Table<uom_uom>()
                .Where(x => x.active == true)
                .ToArrayAsync();

            cachedUom = items.ToDictionary(uom => uom.id, uom => uom);

            var products = await Database.Table<product_product>().Where(x => product_ids.Contains(x.id)).ToListAsync();

            if (products == null || products.Count == 0)
                return new List<product_product>();

            foreach (var p in products)
            {
                p.uom_display = cachedUom.TryGetValue(p._uom_id, out var uom) ? uom.clave_externa : "";

                decimal factor_iva = 1;
                if (cachedTaxesList.TryGetValue(p._taxes_id, out var tax_sale))
                {
                    decimal iva_tax = tax_sale; //15m;
                    factor_iva = 1 + (iva_tax / 100m);
                }

                if (cachedProductsWithPrices.TryGetValue(p._product_tmpl_id, out var price))
                {
                    p.list_price = (float)price;
                }
                else
                {
                    p.list_price = 0;
                }

                decimal price_list_value = (decimal)p.list_price;
                decimal price_without_iva = Math.Round(price_list_value / factor_iva, 7);

                p.list_price = (float)price_without_iva;
            }

            return products;
        }

        internal async Task<int[]> GetAllTaxesIdsAsync()
        {
            await Init();
            
            var products = await Database.Table<product_product>()
                .Where(x => x._taxes_id != 0)                
                .ToListAsync();

            var taxesIds = products
                .Select(x => x._taxes_id)
                .Distinct()
                .ToArray();

            return taxesIds;
        }

        public async Task<int[]> GetTopMarcas(int topCount)
        {
            await Init();

            var products = await Database.Table<product_product>()
                .Where(x => x._taxes_id != 0 && x._general_marca_id != 0)
                .ToListAsync();

            var topMarcasIds = products
                .GroupBy(x => x._general_marca_id)
                .OrderByDescending(g => g.Count())
                .Take(topCount)
                .Select(g => g.Key)
                .ToArray();

            return topMarcasIds;
        }
    }
}
