using DMSA.Models.Odoo.DMCobranzas;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class AccountPaymentDailyDb : SqliteDbBase<AccountPaymentDaily>
    {
        public AccountPaymentDailyDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

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
    }
}
