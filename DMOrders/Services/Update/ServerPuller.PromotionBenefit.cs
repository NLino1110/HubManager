using ApiManager;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> OnlinePromotionProducts(PromotionBenefit promotionBenfit, bool force)
        {
            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubPromotionProduct hubmanager = new ApiManager.HubPromotionProduct(App.Session);
            var resultCount = await hubmanager.GetCount(promotionBenfit.id);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit; // App.Session.odooConnection.DbLimitDefault;

            var database = new PromotionProductDb();

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetItemsByParentId(promotionBenfit.id, limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                if (indice >= 600)
                {
                    Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }

        public async Task<bool> OnlinePromotionProductDetail(PromotionBenefit promotionBenfit, bool force)
        {
            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubPromotionProductDetail hubmanager = new ApiManager.HubPromotionProductDetail(App.Session);
            var resultCount = await hubmanager.GetCount(promotionBenfit.id);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / App.Session.odooConnection.DbLimitDefault;

            var database = new PromotionProductDetailDb();

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetItemsByParentId(promotionBenfit.id, limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                if (indice >= 600)
                {
                    Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }

        public async Task<bool> OnlinePromotionProductsDetail(PromotionBenefit promotionBenfit, bool force)
        {
            return true;
        }

        public async Task<bool> OnlineProductPriceList(PromotionBenefit promotionBenfit, bool force)
        {
            return true;
        }

        public async Task<bool> OnlinePromoCenter(PromotionBenefit promotionBenfit, bool force)
        {
            return true;
        }

        public async Task<bool> OnlinePromoRules(PromotionBenefit promotionBenfit, bool force)
        {
            return true;
        }

        public async Task<bool> OnlinePromotionBenefit(bool force)
        {
            DateTime current_datetime = DateTime.Now;

            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubPromotionBenefit hubmanager = new ApiManager.HubPromotionBenefit(App.Session);
            var resultCount = await hubmanager.GetCount(current_datetime.Year, current_datetime.Month, current_datetime.Day);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / App.Session.odooConnection.DbLimitDefault;

            var database = new PromotionBenefitDb();

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetActives(current_datetime, limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);

                    foreach (var item in responseAll.result)
                    {
                        await OnlinePromotionProducts(item, true);
                        await OnlinePromotionProductDetail(item, true);
                    }
                }

                if (indice >= 600)
                {
                    Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            stopwatch.Stop();
            
            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }
    }
}
