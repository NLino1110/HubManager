using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Database.Sqlite
{
    public class ResPartnerDb : SqliteDbBase<res_partner>
    {        
        public ResPartnerDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        private static AsyncTableQuery<res_partner> ApplySort(
            AsyncTableQuery<res_partner> q, int filter_sort)
        {
            // 1: sequence ASC, 2: code ASC, 3: name ASC; default: id ASC
            return filter_sort switch
            {
                1 => q.OrderBy(x => x.name),
                //2 => q.OrderBy(x => x.code),
                //3 => q.OrderBy(x => x.name),
                _ => q.OrderBy(x => x.id)
            };
        }

        private AsyncTableQuery<res_partner> BuildQuery(
            string filter_code,
            string filter_vat,
            string filter_name,            
            int filter_days,
            int filter_status,
            int filter_sort)
        {
            Init();

            var q = Database.Table<res_partner>();

            //Excluimos los vendedores
            q = q.Where(x => x.is_salesman == false);


            // --- 1) Filtro por code (prioridad máxima, como tu método actual) ---
            if (!string.IsNullOrWhiteSpace(filter_code))
            {
                var raw = filter_code.Trim();

                // Si es numérico: buscar por id exacto (fallback lo haces fuera)
                if (int.TryParse(raw, out var idCode))
                {
                    q = q.Where(x => x.id == idCode);
                    // OJO: no aplicamos más filtros aquí para mantener tu comportamiento original.
                    return ApplySort(q, filter_sort);
                }
            }

            if (!string.IsNullOrWhiteSpace(filter_vat))
            {
                var raw = filter_vat.Trim();
                q = q.Where(x => x.vat_doc != null && x.vat_doc.Contains(raw));
                return ApplySort(q, filter_sort);                
            }

            // --- 2) Resto de filtros cuando NO hay filter_code ---
            if (!string.IsNullOrWhiteSpace(filter_name))
            {
                var nameTerm = filter_name.Trim().ToLowerInvariant();
                q = q.Where(x => x.name.ToLower().Contains(nameTerm));
            }

            if (filter_status == 1)
                q = q.Where(x => x.active == true);
            else if(filter_status == 2)
                q = q.Where(x => x.active == false);

            //if (filter_new == 1)
            //    q = q.Where(x => x.is_new);

            //if (filter_stock == 1)
            //    q = q.Where(x => x.qty_available > 0);

            // --- 3) Orden ---
            q = ApplySort(q, filter_sort);

            return q;
        }

        public async Task<(IList<res_partner> Items, int Total)> GetPagedAsync(
            string filter_code,
            string filter_vat,
            string filter_name,
            int filter_days,
            int filter_status,
            int filter_sort,
            int page, int pageSize, CancellationToken ct = default)
        {
            var q = BuildQuery(filter_code, filter_vat, filter_name, filter_days, filter_status, filter_sort);

            // COUNT(*) en SQLite, sin traer datos
            var total = await q.CountAsync();

            // LIMIT/OFFSET en SQLite (Skip/Take sobre AsyncTableQuery)
            var offset = Math.Max(0, (page - 1) * pageSize);
            var items = await q.Skip(offset).Take(pageSize).ToListAsync();

            return (items, total);
        }

        public async Task<List<res_partner>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<res_partner>().Take(50).ToListAsync();            
        }

        public async Task<res_partner> GetItemsAsync(int company_id, int partner_id)
        {
            await Init();
            return await Database.Table<res_partner>().Where(x=> (x._company_id == company_id || x._company_id == 0) && 
            x.id == partner_id).FirstOrDefaultAsync();
        }

        public async Task<List<res_partner>> GetItemsBySearchAsync(int company_id, string TextSearch, int limit)
        {
            await Init();
            int findCode = 0;

            int.TryParse(TextSearch, out findCode);

            if (findCode > 0)
            {
                return await Database.Table<res_partner>().Where(y =>
                (y._company_id == company_id || y._company_id == 0) && (
                y.id == findCode )
                ).Take(limit).ToListAsync();
            }
            else
            {
                if(TextSearch.Length < 3)
                {
                    //Sin texto para buscar
                    //await Database.Table<res_partner>().Where(x => x.id == -1).ToListAsync();
                    await Database.Table<res_partner>().Take(0).ToListAsync();
                }

                return await Database.Table<res_partner>().Where(y =>
                (y._company_id == company_id || y._company_id == 0) && (
                y.id == findCode ||
                y.name.Contains(TextSearch) ||
                y.email.Contains(TextSearch) ||
                y.vat.Contains(TextSearch))
                ).Take(limit).ToListAsync();
            }
            //return Database.Table<account_journal>().ToList();
        }

        //public async Task<List<res_partner>> GetItemsByPartnerForPaymentAsync(res_partner res_Partner)
        //{
        //    await Init();
        //    return await Database.Table<account_move>().Where(x=>
        //    x._partner_id == res_Partner.id &&
        //    x.move_type == "out_invoice" && 
        //    x.amount_residual > 0).ToListAsync();
        //    //return Database.Table<account_journal>().ToList();
        //}

        public async Task<res_partner> GetItem(int id)
        {
            await Init();
            return await Database.Table<res_partner>().Where(i => i.id == id).FirstOrDefaultAsync();
        }
    
    }
}
