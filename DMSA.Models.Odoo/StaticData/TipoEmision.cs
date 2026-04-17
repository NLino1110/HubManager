using DMSA.Models.Odoo.Abstract;

namespace DMSA.Models.Odoo.StaticData
{
    public class TipoEmision
    {
        public static readonly List<AppParameter> data = new()
        {
            new AppParameter { code = "transfer", name = "Trasferencia", abrev = "==== TRANSFERENCIA" },
            new AppParameter { code = "deposito", name = "Depósito", abrev = "==== DEPOSITO" },
            new AppParameter { code = "cash", name = "Efectivo", abrev = "==== EFECTIVO" },
            new AppParameter { code = "check_day", name = "Cheque Día" , abrev = "==== CH. DIA"},
            new AppParameter { code = "check", name = "Cheque PF" , abrev = "==== CH. POSF."},
            new AppParameter { code = "credit_card", name = "Tarjeta Crédito" , abrev = "==== TARJETA"},
            //new AppParameter { code = "otros", name = "Otros" , abrev = "OTROS"}
        };
    }
}
