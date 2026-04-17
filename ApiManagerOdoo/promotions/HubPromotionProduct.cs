using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;

namespace ApiManagerOdoo.promotions
{
    public class HubPromotionProduct : HubBase
    {
        string[] fields_array = {
            "id",
            "name",
            "promo_id",
            "bonus_id",
            "general_product_id",
            "product_id",
            "product_uom_id",
            "qty",
            "discount",
            "available_discount",
            "is_fixed",
            "product_file",
            "product_file_name",
            "product_id_count",
            "show_detail",
            "imported_from_file",
            "all_products",
            "summary",
            "detail_ids",
            "view_type_name",
            "state",
            "last_run_signature",
            "last_run_at",
            "display_name",
            "create_uid",
            "create_date",
            "write_uid",
            "write_date",
        };

        public HubPromotionProduct(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "promotion.product";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int parent_id)
        {            
            object[] args = new object[] { };

            object[] _custom_args = new object[] {                
                 new object[] { "promo_id", "=", parent_id }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByParents(int[] parent_ids)
        {
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                 new object[] { "promo_id", "in", parent_ids }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByBonus(int[] bonus_ids)
        {
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                 new object[] { "bonus_id", "in", bonus_ids }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<PromotionProduct[]>?> GetItemsByBonusId(int[] bonus_ids, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "bonus_id", "in", bonus_ids }
            };
            return await SearchRead<ApiResponseOdooRpcT<PromotionProduct[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<PromotionProduct[]>?> GetItemsByParentId(int id, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "promo_id", "=", id }
            };
            return await SearchRead<ApiResponseOdooRpcT<PromotionProduct[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<PromotionProduct[]>?> GetItemsByParentIds(int[] ids, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "promo_id", "in", ids }
            };
            return await SearchRead<ApiResponseOdooRpcT<PromotionProduct[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<PromotionProduct[]>?> GetItemsByParentIds(string ids, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "promo_id", "in", $"[{ids}]" }
            };
            return await SearchRead<ApiResponseOdooRpcT<PromotionProduct[]>>(args, _custom_args, kwargs, true);
        }

    }
}
