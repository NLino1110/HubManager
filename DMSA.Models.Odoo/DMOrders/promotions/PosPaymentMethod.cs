using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders.promotions
{
    [Table("pos_payment_method")]
    public class PosPaymentMethod : OdooEntity
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        [JsonProperty("id")]
        public int Id { get; set; }

        // ----------------------------------------------------------------------
        // Básicos
        // ----------------------------------------------------------------------
        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("sequence")]
        public int sequence { get; set; }

        [JsonProperty("active")]
        public bool active { get; set; } = true;

        // Computados en Odoo (aquí como campos simples; si luego los calculas, ok)
        [JsonProperty("is_cash_count")]
        public bool is_cash_count { get; set; }

        [JsonProperty("hide_use_payment_terminal")]
        public bool hide_use_payment_terminal { get; set; }

        [JsonProperty("type")]
        public string type { get; set; } // 'cash' | 'bank' | 'pay_later' (compute en Odoo)

        [JsonProperty("image")]
        public string image { get; set; } // binario/base64 suele serializarse como string

        [JsonProperty("payment_method_type")]
        public string payment_method_type { get; set; } = "none"; // 'none' | 'terminal' | 'qr_code'

        [JsonProperty("default_qr")]
        public string default_qr { get; set; } // compute en Odoo

        [JsonProperty("qr_code_method")]
        public string qr_code_method { get; set; } // selección dinámica en Odoo

        [JsonProperty("hide_qr_code_method")]
        public bool hide_qr_code_method { get; set; }

        // Personalización usada por tus onchanges (si no la tienes en Odoo, puedes omitirla)
        [JsonProperty("aplica_plazos_banco")]
        public bool aplica_plazos_banco { get; set; } = false;

        [JsonProperty("split_transactions")]
        public bool split_transactions { get; set; } = false;

        // ----------------------------------------------------------------------
        // Many2one
        // ----------------------------------------------------------------------
        [Ignore]
        [JsonProperty("outstanding_account_id")]
        public JToken outstanding_account_id { get; set; } // account.account

        [JsonIgnore]
        public int _outstanding_account_id
        {
            get => GetId(outstanding_account_id);
            set => outstanding_account_id = SetId(outstanding_account_id, value);
        }

        [Ignore]
        [JsonProperty("receivable_account_id")]
        public JToken receivable_account_id { get; set; } // account.account

        [JsonIgnore]
        public int _receivable_account_id
        {
            get => GetId(receivable_account_id);
            set => receivable_account_id = SetId(receivable_account_id, value);
        }

        [Ignore]
        [JsonProperty("journal_id")]
        public JToken journal_id { get; set; } // account.journal

        [JsonIgnore]
        public int _journal_id
        {
            get => GetId(journal_id);
            set => journal_id = SetId(journal_id, value);
        }

        [Ignore]
        [JsonProperty("company_id")]
        public JToken company_id { get; set; } // res.company

        [JsonIgnore]
        public int _company_id
        {
            get => GetId(company_id);
            set => company_id = SetId(company_id, value);
        }

        // Nota: use_payment_terminal es Selection en Odoo
        [JsonProperty("use_payment_terminal")]
        public string use_payment_terminal { get; set; } // 'none' | 'terminal' | (posible 'qr_code')

        // ----------------------------------------------------------------------
        // Many2many
        // ----------------------------------------------------------------------
        [Ignore]
        [JsonProperty("open_session_ids")]
        public JToken open_session_ids { get; set; } // pos.session (compute)

        [JsonIgnore]
        public int[] _open_session_ids
        {
            get => GetIds(open_session_ids);
            set => open_session_ids = SetIds(open_session_ids, value);
        }

        [Ignore]
        [JsonProperty("config_ids")]
        public JToken config_ids { get; set; } // pos.config

        [JsonIgnore]
        public int[] _config_ids
        {
            get => GetIds(config_ids);
            set => config_ids = SetIds(config_ids, value);
        }
    }

}
