using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using System.Globalization;

namespace DMSA.Sync.Core.Database.Sqlite.DebitCollection
{
    public class MultipleCobrosInvoiceDb : SqliteDbBase<MultipleCobrosInvoice>
    {
        public MultipleCobrosInvoiceDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            
        }

        public async Task<string> BuildRecipeName(MultipleCobrosInvoice multipleCobrosInvoice)
        {
            await Init();
            string usuarioIniciales = Constants.Session.CurrentUserFront.username.ToUpper().Substring(0, 2);
            string company_abrev = Constants.Session.res_Company.name.ToUpper().Substring(0, 2);
            //multipleCobrosInvoice.create_uid.ToString();
            DateTime fechaActualDt = multipleCobrosInvoice.create_date;

            var datos = await Database.Table<MultipleCobrosInvoice>().ToListAsync();
            var datos_fin = datos.Where(
                c => c.create_date.Date == fechaActualDt.Date &&
                c.company_id == multipleCobrosInvoice.company_id &&
                c.receipt_name != null &&
                c.receipt_name != string.Empty).ToList();

            int lastId = 0;
            string fechaSinGuiones = fechaActualDt.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            if (datos_fin != null && datos_fin.Count > 0)
            {
                var ultimo = datos_fin.OrderByDescending(f => f.receipt_name).FirstOrDefault();
                if (ultimo.receipt_name != null)
                    lastId = int.Parse(ultimo.receipt_name.Split('-')[2]);
            }

            lastId++;

            string codigoRecibo = company_abrev +
                usuarioIniciales + "-" +
                fechaSinGuiones + "-" +
                lastId.ToString();

            return codigoRecibo;
        }

        public string BuildName(MultipleCobrosInvoice multipleCobrosInvoice, string user_name, int number_seq)
        {
            string usuarioIniciales = user_name.ToUpper().Substring(0,2);
            DateTime fechaActualDt = multipleCobrosInvoice.create_date;
            string fechaSinGuiones = fechaActualDt.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string nameReturn = usuarioIniciales + "-" + fechaSinGuiones + "-" + number_seq.ToString("D4");
            return nameReturn;
        }
    }
}
