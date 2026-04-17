using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.Abstract
{
    [Obsolete]
    public class AppParameter
    {
        [Key]
        public string name { get; set; }
        public string value { get; set; }
        public string code { get; set; }
        public string abrev { get; set; }

        ////public List<AppParameter> LoadDefault()
        ////{
        ////    List<AppParameter> defaultSettings = new List<AppParameter>();
        ////    defaultSettings.Add(new AppParameter()
        ////    {
        ////        name = "ncr_journal_id",
        ////        value = "52"
        ////    }
        ////    );

        ////    defaultSettings.Add(new AppParameter()
        ////    {
        ////        name = "ncr_l10_latam_document_type_id",
        ////        value = "45"
        ////    }
        ////    );

        ////    defaultSettings.Add(new AppParameter()
        ////    {
        ////        name = "ncr_account_id",
        ////        value = "2108"
        ////    }
        ////    );

        ////    defaultSettings.Add(new AppParameter()
        ////    {
        ////        name = "ncr_currency_id",
        ////        value = "2"
        ////    }
        ////    );

        ////    return defaultSettings;
        ////}
    }
}
