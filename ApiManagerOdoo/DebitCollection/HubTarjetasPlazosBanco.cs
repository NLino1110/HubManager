using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;
using System;

namespace ApiManagerOdoo.promotions
{
    public class HubTarjetasPlazosBanco : HubBase
    {
        string[] fields_array = {
            "id",
            "name",
            "display_name",
            "account_journal_id",
            "bank_id",
            "pos_tipo_pago",
            "tipo_pay",
            "tipo_canal_ids",
            "cuotas_mensuales",
            "porc_comision",
            "active",
            "create_date",
            "write_date"
        };

        public HubTarjetasPlazosBanco(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "tarjetas.plazos.banco";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(DateTime? dateTime)
        {            
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateTime?.ToString("yyyy-MM-dd") },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<TarjetasPlazosBanco[]>?> GetItems(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") }
            };
            return await SearchRead<ApiResponseOdooRpcT<TarjetasPlazosBanco[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<TarjetasPlazosBanco[]>?> GetActives(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { 
                    "end_datetime", ">=", dateIni.ToString("yyyy-MM-dd") 
                },
                new object[] {                    
                    "active", "=", true
                }
            };
            return await SearchRead<ApiResponseOdooRpcT<TarjetasPlazosBanco[]>>(args, _custom_args, kwargs, true);
        }
    }
}
