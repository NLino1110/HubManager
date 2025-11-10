using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using Microsoft.Maui;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Database.Sqlite
{
    public class ProductProductDb : SqliteDbBase<product_product>
    {
        public ProductProductDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        // En tu repositorio/capa de datos
        private AsyncTableQuery<product_product> BuildQuery(
            string filter_code,
            string filter_name,
            int filter_brand,
            int filter_new,
            int filter_stock,
            int filter_sort,
            int filter_category, 
            int filter_status)
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

            // --- 3) Orden ---
            q = ApplySort(q, filter_sort);

            return q;
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
            int page, int pageSize, CancellationToken ct = default)
        {
            var q = BuildQuery(filter_code, filter_name, filter_brand, filter_new, filter_stock, filter_sort, filter_category, filter_status);

            // COUNT(*) en SQLite, sin traer datos
            var total = await q.CountAsync();

            // LIMIT/OFFSET en SQLite (Skip/Take sobre AsyncTableQuery)
            var offset = Math.Max(0, (page - 1) * pageSize);
            var items = await q.Skip(offset).Take(pageSize).ToListAsync();

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
    }
}
