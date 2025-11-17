using ApiManagerOdoo.promotions;
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
        public async Task<bool> LoyaltyFilters(PromotionBenefit promotionBenfit, bool force)
        {
            var stopwatch = Stopwatch.StartNew();

            var hubmanager = new HubLoyaltyFilters(appSession);
            var resultCount = await hubmanager.GetCount(promotionBenfit.id);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit; // App.Session.odooConnection.DbLimitDefault;

            var database = new LoyaltyFiltersDb(DbNameSqlite);

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

        public async Task<bool> LoyaltyFiltersDetail(PromotionBenefit promotionBenfit, bool force)
        {
            var stopwatch = Stopwatch.StartNew();

            var hubmanager = new HubLoyaltyFiltersDetail(appSession);
            var resultCount = await hubmanager.GetCount(promotionBenfit.id);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit; // App.Session.odooConnection.DbLimitDefault;

            var database = new LoyaltyFiltersDetailsDb(DbNameSqlite);

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

        public async Task<bool> PaymentMethod(bool force)
        {
            var stopwatch = Stopwatch.StartNew();

            var hubmanager = new HubPosPaymentMethod(appSession);
            var resultCount = await hubmanager.GetCount(year, month, day);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit; // App.Session.odooConnection.DbLimitDefault;

            var database = new PosPaymentMethodDb(DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetItems(lastDate.Value, limit, indice);

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

        public async Task<bool> PosTarjetasCanal(bool force)
        {
            var stopwatch = Stopwatch.StartNew();

            var hubmanager = new HubPosTarjetasCanal(appSession);
            var resultCount = await hubmanager.GetCount(year, month, day);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit; // App.Session.odooConnection.DbLimitDefault;

            var database = new PosTarjetasCanalDb(DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetItems(lastDate.Value, limit, indice);

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

        public async Task<bool> OnlinePromotionProducts(PromotionBenefit promotionBenfit, bool force)
        {
            var stopwatch = Stopwatch.StartNew();

            HubPromotionProduct hubmanager = new HubPromotionProduct(appSession);
            var resultCount = await hubmanager.GetCount(promotionBenfit.id);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit; // App.Session.odooConnection.DbLimitDefault;

            var database = new PromotionProductDb(DbNameSqlite);

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

            HubPromotionProductDetail hubmanager = new HubPromotionProductDetail(appSession);
            var resultCount = await hubmanager.GetCount(promotionBenfit.id);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / appSession.odooConnection.DbLimitDefault;

            var database = new PromotionProductDetailDb(DbNameSqlite);

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

        //public async Task<bool> OnlinePromotionProductsDetail(PromotionBenefit promotionBenfit, bool force)
        //{
        //    return true;
        //}

        public async Task<bool> PromoCenter(PromotionBenefit promotionBenfit, bool force)
        {
            var stopwatch = Stopwatch.StartNew();

            var hubmanager = new HubPromoCenters(appSession);
            var resultCount = await hubmanager.GetCount(promotionBenfit.id);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / appSession.odooConnection.DbLimitDefault;

            var database = new PromoCentersDb(DbNameSqlite);

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

        public async Task<bool> PromoRules(PromotionBenefit promotionBenfit, bool force)
        {
            var stopwatch = Stopwatch.StartNew();

            var hubmanager = new HubPromoRules(appSession);
            var resultCount = await hubmanager.GetCount(promotionBenfit.id);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / appSession.odooConnection.DbLimitDefault;

            var database = new PromoRulesDb(DbNameSqlite);

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetItemsByParentId(promotionBenfit.id, limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                if (indice >= maxIndexExceeded)
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

        public async Task<bool> OnlinePromotionBenefit(bool force)
        {
            await PaymentMethod(true);
            await PosTarjetasCanal(true);

            DateTime current_datetime = DateTime.Now;

            var stopwatch = Stopwatch.StartNew();

            HubPromotionBenefit hubmanager = new HubPromotionBenefit(appSession);
            var resultCount = await hubmanager.GetCount(current_datetime.Year, current_datetime.Month, current_datetime.Day);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / appSession.odooConnection.DbLimitDefault;

            var database = new PromotionBenefitDb(DbNameSqlite);

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetActives(current_datetime, limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);

                    foreach (var item in responseAll.result)
                    {
                        await LoyaltyFilters(item, true);
                        await LoyaltyFiltersDetail(item, true);
                        
                        await PromoRules(item, true);
                        await PromoCenter(item, true);
                        await OnlinePromotionProducts(item, true);
                        await OnlinePromotionProductDetail(item, true);
                    }
                }

                if (indice >= maxIndexExceeded)
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
