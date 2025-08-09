using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMCobranzas
{
    //cabeceraCobro
    public class AccountPaymentHeaderSend: AccountPaymentHeader
    {
        public AccountPaymentSend[]? payments { get; set; }
    }
}
