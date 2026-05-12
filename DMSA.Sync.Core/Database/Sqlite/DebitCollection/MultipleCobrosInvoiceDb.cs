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

        ////public async Task<string> BuildRecipeName(MultipleCobrosInvoice multipleCobrosInvoice)
        ////{
        ////    await Init();
        ////    string usuarioIniciales = Constants.Session.CurrentUserFront.username.ToUpper().Substring(0, 2);
        ////    string company_abrev = Constants.Session.res_Company.name.ToUpper().Substring(0, 2);
        ////    //multipleCobrosInvoice.create_uid.ToString();
        ////    DateTime fechaActualDt = multipleCobrosInvoice.create_date;

        ////    var datos = await Database.Table<MultipleCobrosInvoice>().ToListAsync();
        ////    var datos_fin = datos.Where(
        ////        c => c.create_date.Date == fechaActualDt.Date &&
        ////        c.company_id == multipleCobrosInvoice.company_id &&
        ////        c.receipt_name != null &&
        ////        c.receipt_name != string.Empty).ToList();

        ////    int lastId = 0;
        ////    string fechaSinGuiones = fechaActualDt.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        ////    if (datos_fin != null && datos_fin.Count > 0)
        ////    {
        ////        var ultimo = datos_fin.OrderByDescending(f => f.receipt_name).FirstOrDefault();
        ////        if (ultimo.receipt_name != null)
        ////            lastId = int.Parse(ultimo.receipt_name.Split('-')[2]);
        ////    }

        ////    lastId++;

        ////    string codigoRecibo = company_abrev +
        ////        usuarioIniciales + "-" +
        ////        fechaSinGuiones + "-" +
        ////        lastId.ToString();

        ////    return codigoRecibo;
        ////}

        public async Task<string> BuildRecipeName(MultipleCobrosInvoice multipleCobrosInvoice)
        {
            await Init();

            var username = Constants.Session.CurrentUserFront.username ?? string.Empty;
            var companyName = Constants.Session.res_Company.name ?? string.Empty;

            string usuarioIniciales = username.Length >= 2 ? username.Substring(0, 2).ToUpper() : username.ToUpper();
            string company_abrev = companyName.Length >= 2 ? companyName.Substring(0, 2).ToUpper() : companyName.ToUpper();

            DateTime fechaActualDt = multipleCobrosInvoice.create_date;
            string fechaSinGuiones = fechaActualDt.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            var fechaInicio = fechaActualDt.Date;
            var fechaFin = fechaInicio.AddDays(1);

            var datos = await Database.Table<MultipleCobrosInvoice>()
                .Where(c =>
                    c.company_id == multipleCobrosInvoice.company_id &&
                    c.receipt_name != null &&
                    c.receipt_name != string.Empty)
                .ToListAsync();

            var datos_fin = datos
                .Where(c => c.create_date >= fechaInicio && c.create_date < fechaFin)
                .ToList();

            int lastId = 0;

            if (datos_fin.Count > 0)
            {
                lastId = datos_fin
                    .Select(c =>
                    {
                        var parts = c.receipt_name.Split('-');
                        if (parts.Length == 3 && int.TryParse(parts[2], out int num))
                            return num;
                        return 0;
                    })
                    .Max();
            }

            lastId++;

            string codigoRecibo = $"{company_abrev}{usuarioIniciales}-{fechaSinGuiones}-{lastId}";

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
