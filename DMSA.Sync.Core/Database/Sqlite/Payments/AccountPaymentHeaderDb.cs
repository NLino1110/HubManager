using DMSA.Models.Odoo.Accounting;
using SQLite;
using System.Globalization;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    [Obsolete]
    public class AccountPaymentHeaderDb : SqliteDbBase<AccountPaymentHeader>
    {   
        public AccountPaymentHeaderDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<AccountPaymentHeader>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<AccountPaymentHeader>().ToListAsync();
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
            (i.payment_status == DMSA.Models.CobrosEstados.PROCESANDO ||
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
    }
}
