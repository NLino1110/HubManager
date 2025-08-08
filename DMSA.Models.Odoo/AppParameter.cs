using DMSA.Models.Odoo;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Models
{
    public class AppParameter
    {
        [Key]
        public string name { get; set; }
        public string value { get; set; }
        //public int id { get; set; }

        public List<AppParameter> LoadDefault()
        {
            List<AppParameter> defaultSettings = new List<AppParameter>();
            defaultSettings.Add(new AppParameter()
            {
                name = "ncr_journal_id",
                value = "52"
            }
            );

            defaultSettings.Add(new AppParameter()
            {
                name = "ncr_l10_latam_document_type_id",
                value = "45"
            }
            );

            defaultSettings.Add(new AppParameter()
            {
                name = "ncr_account_id",
                value = "2108"
            }
            );

            defaultSettings.Add(new AppParameter()
            {
                name = "ncr_currency_id",
                value = "2"
            }
            );

            return defaultSettings;
        }
    }
}
