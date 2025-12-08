using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using SQLite;
using System.Globalization;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class AccountMoveSendHeaderDb : SqliteDbBase<AccountMoveSendHeader>
    {        
        public AccountMoveSendHeaderDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

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
    }
}
