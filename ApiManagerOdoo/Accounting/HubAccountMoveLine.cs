using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;
using Newtonsoft.Json;
using RestSharp;
using static System.Net.Mime.MediaTypeNames;

namespace ApiManagerOdoo.Accounting
{
    public class HubAccountMoveLine : HubBase
    {
        private readonly string[] fields_array = new[] { 
            "id",
            "move_id",
            "sequence",
            "name",
            "product_id",
            "quantity",
            "quantity_available",
            "price_unit",
            "price_subtotal",
            "discount_balance",
            "price_total",
            "discount", //"discount_percentage", 
            "tax_ids",
            "product_uom_id",
            "analytic_line_ids",
            "display_type",
            "account_id",
            "create_date",
            "write_date" 
        };

        public HubAccountMoveLine(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.move.line";
        }

        public async Task<ApiResponseOdooRpc?> GetDetailCount(DateTime dateIni)
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", $"{dateIni.Year}-{dateIni.Month:00}-{dateIni.Day:00} 00:00:00" },
                new object[] { "invoice_date", "!=", false },
            }
            .Concat(AccountMoveDocumentDisplay.BuildSyncMoveTypeDomain("move_id.move_type"))
            .ToArray();
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetDetailCountByInvoiceDateRange(DateTime dateFrom, DateTime dateTo)
        {
            object[] args = new object[] { };
            object[] _custom_args = BuildLineDomainByInvoiceDateRange(dateFrom.Date, dateTo.Date);
            return await GetCount(args, _custom_args);
        }

        private static object[] BuildLineDomainByInvoiceDateRange(DateTime dateFrom, DateTime dateTo)
        {
            return new object[] {
                new object[] { "move_id.invoice_date", ">=", dateFrom.ToString("yyyy-MM-dd") },
                new object[] { "move_id.invoice_date", "<=", dateTo.ToString("yyyy-MM-dd") },
                new object[] { "move_id.invoice_date", "!=", false },
            }
            .Concat(AccountMoveDocumentDisplay.BuildSyncMoveTypeDomain("move_id.move_type"))
            .ToArray();
        }

        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLinesByInvoiceDateRange(
            DateTime dateFrom, DateTime dateTo, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array,
                order = "write_date asc"
            };

            object[] args = new object[] { };
            object[] _custom_args = BuildLineDomainByInvoiceDateRange(dateFrom.Date, dateTo.Date);
            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs, true);
        }
        
        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLines(DateTime dateIni)
        {
            var kwargs = new
            {
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "date", ">=", $"{dateIni.ToString("yyyy-MM-dd")}" },
            }
            .Concat(AccountMoveDocumentDisplay.BuildSyncMoveTypeDomain("move_id.move_type"))
            .ToArray();

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs);

        }

        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLines(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array,
                order = "write_date asc"
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", $"{dateIni.Year}-{dateIni.Month:00}-{dateIni.Day:00} 00:00:00" },
                new object[] { "invoice_date", "!=", false },
            }
            .Concat(AccountMoveDocumentDisplay.BuildSyncMoveTypeDomain("move_id.move_type"))
            .ToArray();

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs);

        }

        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLinesByMove(int move_id)
        {
            var kwargs = new
            {                
                fields = fields_array,
                order = "write_date asc"
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "move_id", "=", move_id },
            };

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs, true);

        }

        public async Task<ApiResponseOdooRpc?> GetCountByIds(int[] ids)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", ids }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLineByIds(int[] ids, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array,
                order = "write_date asc"
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "id", "in", ids },
            };

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByMove(int move_id)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "move_id", "=", move_id }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<account_move_line[]>?> GetAccountMoveLinesByMove(int move_id, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array,
                order = "write_date asc"
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "move_id", "=", move_id },
            };

            return await SearchRead<ApiResponseOdooRpcT<account_move_line[]>>(args, _custom_args, kwargs, true);

        }
    }
}
