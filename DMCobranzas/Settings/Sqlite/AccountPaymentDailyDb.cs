using CobranzasDMSA_Odoo.Models;
using Microsoft.Data.Sqlite;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Settings.Sqlite
{
    public class AccountPaymentDailyDb
    {
        SQLiteAsyncConnection Database;

        public AccountPaymentDailyDb()
        {

        }

        public async Task<int> GetCount()
        {
            return await Database.Table<AccountPaymentDaily>().CountAsync();
        }

        public async Task<List<AccountPaymentDaily>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<AccountPaymentDaily>().ToListAsync();
        }

        public async Task<AccountPaymentDaily> GetItemAsync(int empresa, string idCierre)
        {
            await Init();
            return await Database.Table<AccountPaymentDaily>().Where(i => i.company_id == empresa && i.closing_id == idCierre).FirstOrDefaultAsync();
        }

        public async Task<AccountPaymentDaily> GetItemsDateCutAsync(int company_id, DateTime fechaCierre)
        {
            await Init();
            string fechaBusqueda = fechaCierre.ToString("yyyy-MM-dd");
            //TODO: Revisar optimización, y cambios de tipo de datos, por ejemplo se esta usando strings en vez de DateTime
            var datos = await Database.Table<AccountPaymentDaily>().Where(c => c.closing_id.StartsWith(fechaBusqueda) && c.company_id == company_id).FirstOrDefaultAsync();            
            return datos;
        }

        public async Task<List<AccountPaymentDaily>> GetItemsAsync(int company_id, 
            DateTime fdesde, 
            DateTime fhasta, 
            string codigoUsuario, 
            bool ordenEnvio)
        {
            await Init();

            if(ordenEnvio)
            {
                
            }

            return await Database.Table<AccountPaymentDaily>()
                .Where(i => i.company_id == company_id) // && (i.CODESTADO=="ACTIVO" || i.CODESTADO== "CONFIRMADO"))
                .ToListAsync();
        }

        public async Task<int> InsertAsync(AccountPaymentDaily item)
        {   
            await Init();            
            
            return await Database.InsertAsync(item);
        }

        public async Task<int> InsertBatchAsync(AccountPaymentDaily[] items)
        {
            await Init();            
            return await Database.InsertAllAsync(items);
        }

        public async Task<int> UpdateAsync(AccountPaymentDaily item)
        {
            await Init();            
            return await Database.UpdateAsync(item);
        }

        public async Task<int> DeleteItemAsync(AccountPaymentDaily item)
        {
            await Init();
            return await Database.DeleteAsync(item);
        }

        public async Task<int> TruncateAsync()
        {
            await Init();
            await Database.DeleteAllAsync<AccountPaymentDaily>();
            return 0;
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);            
            var result = await Database.CreateTableAsync<AccountPaymentDaily>();

        //    //SQLiteConnection mDBcon = new SQLiteConnection();
        //    //mDBcon.ConnectionString = "Data Source=" + DataSourcePath;
        //    //mDBcon.Open();

        //    SQLiteCommand cmd = new SQLiteCommand(Database.GetConnection());
        //    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS COBCIERRE (
        //CODEMPRESA TEXT, 
        //IDCIERRE TEXT, 
        //BANCO INTEGER, 
        //NUMDEPOSITO TEXT, 
        //VALOR DECIMAL(10, 2), 
        //DETALLECIERRE TEXT)";
        //    cmd.ExecuteNonQuery();
        }
    
    }
}
