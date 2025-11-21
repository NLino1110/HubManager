using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using Microsoft.Data.Sqlite;
using Microsoft.Maui;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Database.Sqlite
{
    public class ProductProductDb : SqliteDbBase<product_product>
    {
        private Dictionary<int, decimal> cachedProductsWithPrices =
    new Dictionary<int, decimal>();

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
            Init();

            var q = Database.Table<product_product>();

            // --- 1) Filtro por code (prioridad máxima, como tu método actual) ---
            if (!string.IsNullOrWhiteSpace(filter_code))
            {
                var raw = filter_code.Trim();

                // Si es numérico: buscar por id exacto (fallback lo haces fuera)
                if (int.TryParse(raw, out var idCode))
                {
                    q = q.Where(x => x.id == idCode || x.code.ToLower().Contains(filter_code.ToLower()));
                    // OJO: no aplicamos más filtros aquí para mantener tu comportamiento original.
                    return ApplySort(q, filter_sort);
                }
                else
                {
                    var term = raw.ToLowerInvariant();
                    q = q.Where(x => x.code.ToLower().Contains(term));
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

            if (filter_stock == 1)
                q = q.Where(x => x.qty_available > 0);

            if (filter_pricelist > 0)
            {
                await PreloadPricelistCache(filter_pricelist);
                var productTemplateIds = cachedProductsWithPrices.Keys.ToList();
                //TODO: Se quita filtro porque se necesita que se muestren todos
                //q = q.Where(p => productTemplateIds.Contains(p._product_tmpl_id));
            }

            // --- 3) Orden ---
            q = ApplySort(q, filter_sort);

            return q;
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

            if (filter_pricelist > 0)
            {
                foreach (var p in items)
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
            }

            return (items, total);
        }

        public async Task<product_product> GetItem(int id)
        {
            await Init();
            return await Database.Table<product_product>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        internal async Task<product_product> GetByProductTemplate(int product_template_id)
        {
            await Init();
            return await Database.Table<product_product>().Where(x => x._product_tmpl_id == product_template_id).FirstOrDefaultAsync();
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

    }
}
