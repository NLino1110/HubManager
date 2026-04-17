using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;

namespace ApiManager
{
    public class HubProductMarca : HubBase
    {
        string[] fields_array = {
            "id",
            "name",
            "active",
            "clave_externa",
            //"use_mobile_app",
            "create_date",
            "write_date"
        };

        public HubProductMarca(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "product.marca";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int year, int month, int day)
        {            
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },                
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<product_marca[]>?> GetItemsById(string ids)
        {            
            //string fields = "fields=['id','name','description']";
            
            int limit = 300;
            int index = 0 ;

            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", $"[{ids}]" }
            };
            return await SearchRead<ApiResponseOdooRpcT<product_marca[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<product_marca[]>?> GetItems(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") }
            };
            return await SearchRead<ApiResponseOdooRpcT<product_marca[]>>(args, _custom_args, kwargs, true);
        }
    }
}
