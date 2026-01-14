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

        //public async Task<string> BuildRecipeName(MultipleCobrosInvoice accountPaymentHeader)
        //{
        //    await Init();
        //    string usuarioIniciales = accountPaymentHeader.create_uid.ToString();
        //    //string fechaSinGuiones = fechaActual; // fechaActual.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        //    DateTime fechaActualDt = accountPaymentHeader.create_date;

        //    var datos = await Database.Table<MultipleCobrosInvoice>().ToListAsync();
        //    var datos_fin = datos.Where(
        //        c => c.create_date.Date == fechaActualDt.Date &&
        //        c.company_id == accountPaymentHeader.company_id &&
        //        c.recipe_name != null &&
        //        c.recipe_name != string.Empty).ToList();

        //    int lastId = 0;
        //    string fechaSinGuiones = fechaActualDt.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        //    if (datos_fin != null && datos_fin.Count > 0)
        //    {
        //        //var ultimo = datos_fin.OrderByDescending(f => f.create_datetime).FirstOrDefault();
        //        var ultimo = datos_fin.OrderByDescending(f => f.recipe_name).FirstOrDefault();
        //        if (ultimo.recipe_name != null)
        //            lastId = int.Parse(ultimo.recipe_name.Split('-')[2]);
        //    }

        //    lastId++;

        //    string codigoRecibo = usuarioIniciales +
        //        accountPaymentHeader.company_id.ToString() + "-" +
        //        fechaSinGuiones + "-" +
        //        lastId.ToString();

        //    return codigoRecibo;
        //}

        public string BuildName(MultipleCobrosInvoice accountPaymentHeader, string user_name, int number_seq)
        {
            string usuarioIniciales = user_name.ToUpper().Substring(0,2);
            DateTime fechaActualDt = accountPaymentHeader.create_date;
            string fechaSinGuiones = fechaActualDt.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string nameReturn = usuarioIniciales + "-" + fechaSinGuiones + "-" + number_seq.ToString("D4");
            return nameReturn;
        }
    }
}
