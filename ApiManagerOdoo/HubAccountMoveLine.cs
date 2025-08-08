using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using Newtonsoft.Json;
using RestSharp;
using static System.Net.Mime.MediaTypeNames;

namespace ApiManager
{
    public class HubAccountMoveLine : HubBase
    {
        public HubAccountMoveLine(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.move.line";
        }

        //public HubAccountMoveLine(AppSession _setAppSession, string modelname) : base(_setAppSession)
        //{

        //}

        public async Task<ApiResponseOdooRpc?> GetDetailCount(DateTime dateIni)
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {
                new object[] { "date", ">=", $"{dateIni.ToString("yyyy-MM-dd")}" },
                new object[] { "invoice_date", "!=", false },
            };
            return await GetCount(args, _custom_args);
        }
        
        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLines(DateTime dateIni)
        {
            var kwargs = new
            {
                fields = new[] { "id", "move_id", "sequence", "name", "product_id", 
                    "quantity", "price_unit", "price_subtotal", "discount_balance", 
                    "price_total", "discount_percentage", "tax_ids", "analytic_line_ids", 
                    "display_type", "account_id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "date", ">=", $"{dateIni.ToString("yyyy-MM-dd")}" },
            };

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs);

        }

        public async Task<ApiResponseOdooRpc?> GetAccountMoveLinesCountByDate(int year, int month, int day)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] { "create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);

        }

        public async Task<ApiResponseOdooRpc?> GetAccountMoveLinesCountByWriteDate(int year, int month, int day)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLinesByDate(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id","move_id","sequence","name","product_id","quantity","price_unit","price_subtotal","discount_balance","price_total","discount_percentage","tax_ids","analytic_line_ids","display_type","account_id","create_date","write_date"
 }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] { "create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs);

        }

        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLinesByWriteDate(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id","move_id","sequence","name","product_id","quantity","price_unit","price_subtotal","discount_balance","price_total","discount_percentage","tax_ids","analytic_line_ids","display_type","account_id","create_date","write_date"
 }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs);

        }

        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLinesByWriteDate_dl(int year, int month, int day)
        {            
            
            var kwargs = new
            {
                fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs);

        }

        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLineByCreateDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs);

        }
    }
}
