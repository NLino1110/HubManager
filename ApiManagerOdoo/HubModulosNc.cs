
using ApiManagerOdoo.Base;
using AppManagerOdoo.Tools;
using DMSA.Models.MovilCobranzas.Api;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using RestSharp;

namespace ApiManager
{
    public class HubModulosNc : HubBase
    {
        string EndPointServer = "";
        //string EndPointApi = "/api/pos.payment.method"; //Se espera que este sea el endpoint definitivo
        string EndPointApi = "/connect/get_modulos_nc"; //Se espera que este sea el endpoint definitivo

        public HubModulosNc(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/connect/get_account_journal";
            _modelname = "account.journal";
        }

        public async Task<ApiResponseOdooRpcT<st_modulosnc[]>?> GetAll()
        {
            var kwargs = new
            {
                fields = new[] { "id", }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "date", ">=", $"" },
            };

            return await SearchRead<ApiResponseOdooRpcT<st_modulosnc[]>>(args, _custom_args, kwargs);
        }        
    }
}
