using DMCobranzas.Models;
using DMSA.Models.Odoo.DMCobranzas;
using Microsoft.Data.Sqlite;
using SQLite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static CoreFoundation.DispatchSource;

namespace DMCobranzas.Settings.Sqlite
{
    public class AccountPaymentHeaderDb
    {
        SQLiteAsyncConnection Database;

        public AccountPaymentHeaderDb()
        {

        }

        public async Task<int> GetCount()
        {
            return await Database.Table<AccountPaymentHeader>().CountAsync();
        }

        public async Task<List<AccountPaymentHeader>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<AccountPaymentHeader>().ToListAsync();
        }

        public async Task<AccountPaymentHeader> GetItemAsync(string id)
        {
            await Init();
            return await Database.Table<AccountPaymentHeader>().Where(i => i.recipe_name == id).FirstOrDefaultAsync();
        }

        public async Task<AccountPaymentHeader> GetItemByGuidAsync(string guid)
        {
            await Init();
            return await Database.Table<AccountPaymentHeader>().Where(i => i.guid == guid).FirstOrDefaultAsync();
        }

        public async Task<List<AccountPaymentHeader>> GetItemsAsync(int empresa,
            DateTime fdesde,
            DateTime fhasta,
            string SearchString,
            int uid,
            bool ordenEnvio)
        {
            await Init();

            if (ordenEnvio)
            {

            }

            var datos = await Database.Table<AccountPaymentHeader>().ToListAsync();
            var result = datos.Where(i => i.company_id == empresa &&
            i.uid == uid &&
            Convert.ToDateTime(i.create_datetime) >= fdesde &&
            Convert.ToDateTime(i.create_datetime) <= fhasta &&
            (
                i.partner_name.Contains(SearchString) //|| i.IDRECIBO.Contains(SearchString)
            )
            ).ToList();            

            return result;            
        }

        public async Task<List<AccountPaymentHeader>> GetItemsAsync(int empresa, 
            DateTime fdesde, 
            DateTime fhasta, 
            int uid, 
            bool ordenEnvio)
        {
            await Init();

            if(ordenEnvio)
            {
                
            }

            var datos = await Database.Table<AccountPaymentHeader>().ToListAsync();
            var result = datos.Where(i => i.company_id == empresa &&
            i.uid == uid &&
            Convert.ToDateTime(i.create_datetime) >= fdesde && Convert.ToDateTime(i.create_datetime) <= fhasta).ToList();

            //foreach (var item in result)
            //{
            //    string EMAILCLIENTE = "";
            //    string NOMBREUSUARIO = "";

            //    CobCarteraCab cobCarteraCab = new CobCarteraCab();
            //    cobCarteraCab = await Database.Table<CobCarteraCab>().Where(cc => cc.CODCLIENTE == item.CODCLIENTE && cc.CODEMPRESA == item.CODEMPRESA).FirstOrDefaultAsync();

            //    user_access cobUsuarios = new user_access();
            //    cobUsuarios = await Database.Table<user_access>().Where(us => us.uid== item.uid).FirstOrDefaultAsync();

            //    if (cobCarteraCab!=null)
            //    {
            //        EMAILCLIENTE = cobCarteraCab.EMAILCLIENTE;
            //        NOMBREUSUARIO = cobUsuarios.name;
            //    }

            //    item.EMAILCLIENTE = EMAILCLIENTE;
            //    item.NOMBREUSUARIO = NOMBREUSUARIO;
            //}

            return result;
            //return await Database.Table<CobReciboCab>()
            //    .Where(i => i.CODEMPRESA == empresa && Convert.ToDateTime(i.FECHA) >= fdesde) // && Convert.ToDateTime(i.FECHA) <= fhasta) // && (i.CODESTADO=="ACTIVO" || i.CODESTADO== "CONFIRMADO"))
            //    .ToListAsync();
        }

        public async Task<List<AccountPaymentHeader>> GetItemsPendingAsync(int uid)
        {
            await Init();
                        
            var datos = await Database.Table<AccountPaymentHeader>().ToListAsync();
            var result = datos.Where(i => i.uid == uid &&
            Convert.ToDateTime(i.create_datetime).Date != DateTime.Today && 
            (i.payment_status == DMSA.Models.CobrosEstados.ENVIANDO ||
            i.payment_status == DMSA.Models.CobrosEstados.ERROR ||
            i.payment_status == DMSA.Models.CobrosEstados.PENDIENTE)).ToList();

            return result;            
        }

        public async Task<List<AccountPaymentHeader>> GetItemsAsync(int empresa, DateTime fechaCierre)
        {
            await Init();
            //string fechaBusqueda = fechaCierre.ToString("yyyy-MM-dd");
            //TODO: Revisar optimización, y cambios de tipo de datos, por ejemplo se esta usando strings en vez de DateTime
            var datos = await Database.Table<AccountPaymentHeader>().Where(c => c.create_datetime.Date == fechaCierre.Date && c.company_id== empresa).ToListAsync();
            //var datos = await Database.Table<CobReciboCab>().Where(c => c.CODEMPRESA == empresa).ToListAsync();
            return datos;
        }

        public DateTime TruncateTime(DateTime time)
        {
            return time.Date;
        }

        public async Task<List<AccountPaymentHeader>> GetItemsDateCutAsync(int empresa, DateTime fechaCierre)
        {
            await Init();

            //var datos = await Database.Table<AccountPaymentHeader>().Where(c => c.FECHA.Date == fechaCierre.Date && c.CODEMPRESA == empresa).ToListAsync();
            var datos = await Database.Table<AccountPaymentHeader>().ToListAsync();
            var datos_fin = datos.Where(
                c => c.create_datetime.Date == fechaCierre.Date &&
                c.company_id == empresa).ToList();
            return datos_fin;
        }

        [Obsolete]
        public string GenerarCodigoRecibo(int uid, int codigoEmpresa, string fechaActual, string data)
        {
            string usuarioIniciales = uid.ToString(); // uid.Substring(0, 2).ToUpper();

            //string fechaSinGuiones = fechaActual; // fechaActual.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            DateTime fechaActualDt = DateTime.Parse(fechaActual);
            string fechaSinGuiones = fechaActualDt.ToString("yyyyMMdd", CultureInfo.InvariantCulture);            
            string codigoRecibo = usuarioIniciales + codigoEmpresa + "-" + fechaSinGuiones + "-" + data;
            return codigoRecibo;
        }

        public async Task<string> BuildRecipeName(AccountPaymentHeader accountPaymentHeader)
        {
            await Init();
            string usuarioIniciales = accountPaymentHeader.uid.ToString();
            //string fechaSinGuiones = fechaActual; // fechaActual.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            DateTime fechaActualDt = accountPaymentHeader.create_datetime;

            var datos = await Database.Table<AccountPaymentHeader>().ToListAsync();
            var datos_fin = datos.Where(
                c => c.create_datetime.Date == fechaActualDt.Date &&
                c.company_id == accountPaymentHeader.company_id &&
                c.recipe_name != null && 
                c.recipe_name != string.Empty).ToList();
            
            int lastId = 0;
            string fechaSinGuiones = fechaActualDt.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            if (datos_fin != null && datos_fin.Count > 0)
            {
                //var ultimo = datos_fin.OrderByDescending(f => f.create_datetime).FirstOrDefault();
                var ultimo = datos_fin.OrderByDescending(f => f.recipe_name).FirstOrDefault();
                if (ultimo.recipe_name != null)
                    lastId = int.Parse(ultimo.recipe_name.Split('-')[2]);
            }

            lastId++;

            string codigoRecibo = usuarioIniciales + 
                accountPaymentHeader.company_id.ToString() + "-" + 
                fechaSinGuiones + "-" + 
                lastId.ToString();

            return codigoRecibo;
        }

        public async Task<int> InsertAsync(AccountPaymentHeader item)
        {   
            await Init();
            //if (item.CODARTICULO != "")
            //    return await Database.UpdateAsync(item);
            //else

            //TODO: NO SE CREA LA SECUENCIA HASTA QUE NO SE HAYA INTENTADO EL ENVIO
            // Se cambia a estado CONFIRMADO una vez que se le asigna el secuencial

            //string fechaActual = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10);
            //var secuencia = await obtenerSecuenciaRecibo(item.CODEMPRESA, item.CODUSUARIO, fechaActual);
            //string secuencia_final = GenerarCodigoRecibo(item.CODUSUARIO, item.CODEMPRESA, fechaActual, secuencia.ToString());
            //item.IDRECIBO = secuencia_final;
            
            return await Database.InsertAsync(item);
        }

        public async Task<int> InsertBatchAsync(AccountPaymentHeader[] items)
        {
            await Init();
            //if (item.CODARTICULO != "")
            //    return await Database.UpdateAsync(item);
            //else
            return await Database.InsertAllAsync(items);
        }

        public async Task<int> UpdateAsync(AccountPaymentHeader item)
        {
            await Init();
            return await Database.UpdateAsync(item);
        }

        public async Task<int> DeleteItemAsync(AccountPaymentHeader item)
        {
            //await Init();
            return await Database.DeleteAsync(item);
        }

        public async Task<int> TruncateAsync()
        {
            await Init();
           
            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            await Database.DeleteAllAsync<AccountPaymentHeader>();

            //SQLiteCommand cmd = new SQLiteCommand(Database.GetConnection());
            //cmd.CommandText = @"DELETE FROM COBRECIBOCAB";
            //cmd.ExecuteNonQuery();

            return 0;
        }

        public async Task Drop()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            //No se procede a la creación automática porque contiene varios campos PK
            var result = await Database.DropTableAsync<AccountPaymentHeader>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            //No se procede a la creación automática porque contiene varios campos PK
            var result = await Database.CreateTableAsync<AccountPaymentHeader>();
        }

        [Obsolete]
        public async Task<int> obtenerSecuenciaRecibo(int empresa, int uid, string fechaActual)
        {
            await Init();

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
          
            fechaActual = fechaActual.Substring(0,10);
            SQLiteCommand cmd = new SQLiteCommand(Database.GetConnection());
            cmd.CommandText = @"SELECT MAX(id)  AS SECUENCIA  " +
              "  FROM AccountPaymentHeader CB" +
              " WHERE DATE(CB.create_datetime) = '" + fechaActual + "'" +
              "   AND CB.company_id = '" + empresa + "'" +
              "   AND CB.uid = " + uid + "" +
              " ORDER BY id DESC LIMIT 1 ";

            var secuencia = cmd.ExecuteQueryScalars<int>();
            int val_secuencia = 0;
            if (secuencia.Count() != 0)
            {
                val_secuencia = secuencia.FirstOrDefault();
            }
            else
            {
                val_secuencia++;
            }

            //val_secuencia++;

            //string secuencia_final = "";
            return val_secuencia;
        }

        
    }
}
