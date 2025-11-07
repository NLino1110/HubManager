using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Database.Sqlite
{
    public class AccountMoveSendHeaderDb
    {
        SQLiteAsyncConnection Database;
        
        public AccountMoveSendHeaderDb()
        {

        }

        public async Task<int> GetCount()
        {
            return await Database.Table<AccountMoveSendHeader>().CountAsync();
        }

        public async Task<int> Truncate()
        {
            await Init();            
            return await Database.DeleteAllAsync<AccountMoveSendHeader>();
        }

        public async Task<int> DeleteRecursive(AccountMoveSendHeader parent)
        {
            await Init();
            int count = 0;

            var resultItemsMove = (await Database.Table<account_move_send>().ToListAsync()).Where(i => i.parent_id == parent.id);

            foreach (var moveItem in resultItemsMove)
            {
                var resultItems = (await Database.Table<account_move_line_send>().ToListAsync()).Where(i => i.parent_move_id == moveItem.id);

                //Elimina detalles
                foreach (var item in resultItems)
                {
                    count++;
                    await Database.DeleteAsync(item);
                }

                //Eliminar movimientos
                await Database.DeleteAsync(moveItem);
            }

            //Elimina cabecera
            await Database.DeleteAsync(parent);
            return count;
        }

        public async Task<List<AccountMoveSendHeader>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<AccountMoveSendHeader>().ToListAsync();            
        }

        public async Task<List<AccountMoveSendHeader>> GetItemsAsync(int company_id, DateTime dateIni, DateTime dateEnd, int user_id)
        {
            await Init();

            var datos = await Database.Table<AccountMoveSendHeader>().ToListAsync();
            var result = datos.Where(i => i.company_id == company_id &&
            i.uid == user_id &&
            Convert.ToDateTime(i.create_datetime) >= dateIni && 
            Convert.ToDateTime(i.create_datetime) <= dateEnd).ToList();

            return result;            
        }

        public async Task<List<AccountMoveSendHeader>> GetItemsAsync(int company_id, 
            DateTime dateIni, 
            DateTime dateEnd, 
            int user_id,
            string TextSearch)
        {
            await Init();

            var datos = await Database.Table<AccountMoveSendHeader>().ToListAsync();
            var result = datos.Where(i => i.company_id == company_id &&
            i.uid == user_id &&
            Convert.ToDateTime(i.create_datetime) >= dateIni &&
            Convert.ToDateTime(i.create_datetime) <= dateEnd &&
            i.partner_name.ToUpper().Contains(TextSearch.ToUpper())).ToList();

            return result;
        }

        public async Task<List<AccountMoveSendHeader>> GetItemsByPartnerForPaymentAsync(res_partner res_Partner)
        {
            await Init();
            return await Database.Table<AccountMoveSendHeader>().Where(x=>
            x.partner_id == res_Partner.id).ToListAsync();            
        }

        public async Task<AccountMoveSendHeader> GetByRequestName(string request_name)
        {
            await Init();
            return await Database.Table<AccountMoveSendHeader>().Where(i => i.request_name == request_name).FirstOrDefaultAsync();
        }

        public async Task<AccountMoveSendHeader> GetItem(int id)
        {
            await Init();
            return await Database.Table<AccountMoveSendHeader>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(AccountMoveSendHeader item)
        {
            await Init();

            await Database.InsertAsync(item);

            return 0;
        }

        public async Task<int> InsertBatchAsync(AccountMoveSendHeader[] items)
        {
            await Init();

            await Database.InsertAllAsync(items, "OR REPLACE");
            
            return 0;
        }

        public async Task<int> UpdateAsync(AccountMoveSendHeader item)
        {
            await Init();
            return await Database.UpdateAsync(item);
        }

        [Obsolete]
        public string BuildName(int uid, int codigoEmpresa, string fechaActual, string data)
        {
            string usuarioIniciales = uid.ToString();
            DateTime fechaActualDt = DateTime.Parse(fechaActual);
            string fechaSinGuiones = fechaActualDt.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string codigoRecibo = usuarioIniciales + codigoEmpresa + "-" + fechaSinGuiones + "-" + data;
            return codigoRecibo;
        }

        public async Task<string> BuildRequestName(AccountMoveSendHeader accountPaymentHeader)
        {
            await Init();
            string usuarioIniciales = accountPaymentHeader.uid.ToString();            
            DateTime fechaActualDt = accountPaymentHeader.create_datetime;

            var datos = await Database.Table<AccountMoveSendHeader>().ToListAsync();
            var datos_fin = datos.Where(
                c => c.create_datetime.Date == fechaActualDt.Date &&
                c.company_id == accountPaymentHeader.company_id &&
                c.request_name != null &&
                c.request_name != string.Empty).ToList();

            int lastId = 0;
            string fechaSinGuiones = fechaActualDt.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            if (datos_fin != null && datos_fin.Count > 0)
            {
                //var ultimo = datos_fin.OrderByDescending(f => f.create_datetime).FirstOrDefault();
                var ultimo = datos_fin.OrderByDescending(f => f.request_name).FirstOrDefault();
                if (ultimo.request_name != null)
                    lastId = int.Parse(ultimo.request_name.Split('-')[2]);
            }

            lastId++;

            string codigoRecibo = usuarioIniciales +
                accountPaymentHeader.company_id.ToString() + "-" +
                fechaSinGuiones + "-" +
                lastId.ToString();

            return codigoRecibo;
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<AccountMoveSendHeader>();
        }
    
    }
}
