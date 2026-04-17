using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;

using System.Data;


namespace ApiManager
{
    public class HubTypeParentNc : HubBase
    {
        string[] fields_array = {
            "id",
            "name",
            "display_name",
            "nc_type",            
            "active",
            "code",
            "motivo_val_dev_nc",
            "create_date",
            "write_date"
        };

        public HubTypeParentNc(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "type.parent.nc";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(DateTime dateIni)
        {
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<TypeParentNc[]>?> GetItemsById(string ids)
        {            
            int limit = 300;
            int index = 0;

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
            return await SearchRead<ApiResponseOdooRpcT<TypeParentNc[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<TypeParentNc[]>?> GetItems(DateTime dateIni, int limit, int index)
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
            return await SearchRead<ApiResponseOdooRpcT<TypeParentNc[]>>(args, _custom_args, kwargs, true);
        }
    }
}
