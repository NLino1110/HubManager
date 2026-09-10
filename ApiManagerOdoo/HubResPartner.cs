using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using RestSharp;

namespace ApiManager
{
    public class HubResPartner : HubBase
    {
        string[] fields_array = new[] {
                //"id_sequence",
                "id",
                "company_id",
                "vat",
                "vat_doc",
                "doc_type_identification_id",
                "name",
                "display_name",
                "email",
                "user_id",
                "contact_address",
                //"user_login",
                //"total_due",
                //"total_overdue",
                "debit",
                "credit",
                //"total_invoiced",
                "street",
                "street2",
                //"total_to_beat",
                //"positive_balance",
                //"client_type_id",
                //"client_type_name",
                "write_date",
                "create_date",
                //"birthdate",
                "parent_id",
                "zip",
                "phone",
                "mobile",
                "city_id",
                "city",
                "state_id",
                "country_id",
                //"contact_address_complete",
                "active",
                "customer",
                "product_pricelist_id",
                "adic_comercial_id",
                "adic_comercial_secundarios_ids",
                "adic_lunes",
                "adic_martes",
                "adic_miercoles",
                "adic_jueves",
                "adic_viernes",
                "adic_sabado",
                "adic_domingo",
                "is_salesman",
                "sale_available",
                "calificacion_crediticia_id",
                "facturacion_cupo_maximo",
                "facturacion_dias_credito_limite",
                "misc_comentarios",
                "child_ids",
                "saldo_vencido",
                "saldo_por_vencer",
                "saldo_a_favor",
                "saldo_total",
                "saldo_ch_posfechado",
                "misc_estado",
                "type"
        };

        public HubResPartner(AppSession _setAppSession) : base(_setAppSession)
        {            
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "res.partner";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int year, int month, int day)
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {
                new object[] {"write_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
            };
            return await GetCount(args, _custom_args);            
        }

        // Cobranzas: count de res.partner sin write_date (proceso aparte de Órdenes).
        // Sin filtro comercial; se mantiene por si otro flujo lo necesita.
        public async Task<ApiResponseOdooRpc?> GetCountAll()
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] { };
            return await GetCount(args, _custom_args);
        }

        // Cobranzas: search_read paginado de todos los res.partner, sin filtro write_date.
        // Sin filtro comercial; se mantiene por si otro flujo lo necesita.
        public async Task<ApiResponseOdooRpcT<res_partner[]>?> GetAll(int limit, int index)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] { };
            return await SearchRead<ApiResponseOdooRpcT<res_partner[]>>(args, _custom_args, kwargs, true);
        }

        // ANTES: OnlineSyncResPartnerCobranzasAll usaba GetCountAll() sin filtro (todos los partners).
        // DESPUÉS: variantes filtradas por comercial logueado (partner_id de sesión al iniciar sesión).
        //   Dominio Odoo: ["|", ["adic_comercial_id","=",partnerId], ["adic_comercial_secundarios_ids","=",partnerId]]
        //   GetCountAll / GetAll originales no se modifican.
        public async Task<ApiResponseOdooRpc?> GetCountAllByAdicComercial(int partnerId)
        {
            object[] args = new object[] { };
            object[] _custom_args = BuildAdicComercialDomain(partnerId);
            return await GetCount(args, _custom_args);
        }

        // Pareja paginada de GetCountAllByAdicComercial (mismo filtro comercial).
        public async Task<ApiResponseOdooRpcT<res_partner[]>?> GetAllByAdicComercial(int limit, int index, int partnerId)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = BuildAdicComercialDomain(partnerId);
            return await SearchRead<ApiResponseOdooRpcT<res_partner[]>>(args, _custom_args, kwargs, true);
        }

        // Dominio OR plano (GetCount/SearchRead ya envuelven _custom_args en args[[...]]).
        // Resultado esperado: args[["|",["adic_comercial_id","=",id],["adic_comercial_secundarios_ids","=",id]]]
        private static object[] BuildAdicComercialDomain(int partnerId) => new object[]
        {
            "|",
            new object[] { "adic_comercial_id", "=", partnerId },
            new object[] { "adic_comercial_secundarios_ids", "=", partnerId }
        };

        public async Task<ApiResponseOdooRpc?> GetCountBySeller(int year, int month, int day, int seller)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"write_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] { "adic_comercial_id", "=", seller },               
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<res_partner[]>?> GetByIds(int limit, int index, int[] ids)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"id", "in", ids },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_partner[]>>(args, _custom_args, kwargs, true);
        }

        //public async Task<ApiResponseOdooRpcT<res_partner[]>?> GetByCreateDateRange(int limit, int index, DateTime dateIni, DateTime dateEnd)
        //{
        //    var kwargs = new
        //    {
        //        limit = limit,
        //        offset = (index * limit),
        //        fields = fields_array 
        //    };

        //    object[] args = new object[] { };
        //    object[] _custom_args = new object[] {
        //        new object[] {"create_date", ">=", dateIni.ToString("yyyy-MM-dd 00:00:00") },
        //        new object[] {"create_date", "<=", dateEnd.ToString("yyyy-MM-dd 23:59:59") },
        //    };
        //    return await SearchRead<ApiResponseOdooRpcT<res_partner[]>>(args, _custom_args, kwargs, true);
        //}

        public async Task<ApiResponseOdooRpcT<res_partner[]>?> GetByWriteDate(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"write_date", ">=", dateIni.ToString("yyyy-MM-dd 00:00:00") },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_partner[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<res_partner[]>?> GetByWriteDateBySeller(DateTime dateIni, int limit, int index, int seller)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"write_date", ">=", dateIni.ToString("yyyy-MM-dd 00:00:00") },
                new object[] { "adic_comercial_id", "=", seller },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_partner[]>>(args, _custom_args, kwargs, true);
        }

        // ANTES: saldos venían en search_read (a menudo incorrectos, ej. saldo_total=0).
        // DESPUÉS (Cobranzas Fase 2): web_read con context tipo_partner=customer y specification de saldos.
        // REVERTIR: no llamar WebReadSaldos; desactivar EnableResPartnerCobranzasSaldosWebRead en ServerPuller.
        public async Task<ApiResponseOdooRpcT<res_partner_saldos_read[]>?> WebReadSaldos(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return null;
            }

            object[] args = new object[] { ids };
            var kwargs = new
            {
                context = new { tipo_partner = "customer" },
                specification = new
                {
                    saldo_a_favor = new { },
                    saldo_ch_posfechado = new { },
                    saldo_por_vencer = new { },
                    saldo_total = new { },
                    saldo_vencido = new { }
                }
            };

            return await CallMethod<ApiResponseOdooRpcT<res_partner_saldos_read[]>>(
                EndPointApi,
                Method.Post,
                args,
                kwargs,
                _modelname,
                "web_read");
        }
    }
}
