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
    public class ResPartnerDb
    {
        SQLiteAsyncConnection Database;
        
        public ResPartnerDb()
        {

        }

        public async Task<int> GetCount()
        {
            return await Database.Table<res_partner>().CountAsync();
        }

        public async Task<int> Truncate()
        {
            await Init();
            //Database.Table<account_move>().Delete();
            //Database.DeleteAll<account_move>();
            return await Database.DeleteAllAsync<res_partner>();
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

        public async Task<res_partner> GetItem(int id_sequence)
        {
            await Init();
            return await Database.Table<res_partner>().Where(i => i.id_sequence == id_sequence).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(res_partner item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(res_partner[] items)
        {
            ICollection<res_partner>? conflicts = null;

            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE");

            //try
            //{
            //    await Database.InsertAllAsync(items);
            //    return items.Length;
            //}
            //catch (SQLite.SQLiteException ex) when (IsUniqueConstraint(ex))
            //{
            //    int inserted = 0;

            //    await Database.ExecuteAsync("BEGIN");
            //    try
            //    {
            //        foreach (var p in items)
            //        {
            //            try
            //            {
            //                // Insert normal (sin OR REPLACE) para que dispare UNIQUE si ya existe
            //                await Database.InsertAsync(p);
            //                inserted++;
            //            }
            //            catch (SQLite.SQLiteException exItem) when (IsUniqueConstraint(exItem))
            //            {
            //                // Registrar el conflictivo
            //                conflicts?.Add(p);
            //                // Continúa con el siguiente
            //            }
            //        }

            //        await Database.ExecuteAsync("COMMIT");
            //    }
            //    catch
            //    {
            //        await Database.ExecuteAsync("ROLLBACK");
            //        throw;
            //    }

            //    return inserted; // Cantidad realmente insertada
            //}

            return 0;
        }

        static bool IsUniqueConstraint(SQLite.SQLiteException ex)
        {
            // sqlite-net expone Result y el mensaje trae el detalle de UNIQUE
            // Cubrimos ambos por seguridad.
            var isConstraint = ex.Result == SQLite3.Result.Constraint;
            var mentionsUnique = ex.Message?.IndexOf("UNIQUE", StringComparison.OrdinalIgnoreCase) >= 0
                              || ex.Message?.IndexOf("constraint failed", StringComparison.OrdinalIgnoreCase) >= 0;
            return isConstraint && mentionsUnique;
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<res_partner>();
            await Database.ExecuteAsync("CREATE UNIQUE INDEX IF NOT EXISTS idx_partner_unique ON res_partner (id, _company_id)");
        }
    
    }
}
