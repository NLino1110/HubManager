using ApiManagerOdoo.promotions;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Sync.Core.Database.Sqlite.Benefits;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> LoyaltyFilters(PromotionBenefit promotionBenfit, bool force)
        {
            var stopwatch = Stopwatch.StartNew();

            var hubmanager = new HubLoyaltyFilters(Constants.Session);
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

        public async Task<bool> LoyaltyFiltersDetail(PromotionBenefit promotionBenfit, bool force)
        {
            var stopwatch = Stopwatch.StartNew();

            var hubmanager = new HubLoyaltyFiltersDetail(Constants.Session);
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

            var hubmanager = new HubPosPaymentMethod(Constants.Session);
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

            var hubmanager = new HubPosTarjetasCanal(Constants.Session);
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
            var database = new PromotionProductDb(DbNameSqlite);

            if (force)
            {
                await database.DeleteAllAsync(x => x._promo_id == promotionBenfit.id);
            }

            var stopwatch = Stopwatch.StartNew();

            HubPromotionProduct hubmanager = new HubPromotionProduct(Constants.Session);
            var resultCount = await hubmanager.GetCount(promotionBenfit.id);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit; // App.Session.odooConnection.DbLimitDefault;

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

            Debug.WriteLine(String.Format("Lapso transcurrido: OnlinePromotionProducts {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }

        public async Task<bool> OnlinePromotionProductDetail(PromotionBenefit promotionBenefit, bool force)
        {
            var database = new PromotionProductDetailDb(DbNameSqlite);

            if (force)
            {
                await database.DeleteAllAsync(x => x._promo_id == promotionBenefit.id);
            }

            var stopwatch = Stopwatch.StartNew();

            HubPromotionProductDetail hubmanager = new HubPromotionProductDetail(Constants.Session);
            var resultCount = await hubmanager.GetCount(promotionBenefit.id);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / Constants.Session.odooConnection.DbLimitDefault;
                        

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetItemsByParentId(promotionBenefit.id, limit, indice);

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

            Debug.WriteLine(String.Format("Lapso transcurrido: OnlinePromotionProductDetail {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }

        public async Task<bool> OnlinePromotionProductDetailByRules(int[] promotionRule, bool force)
        {
            var database = new PromotionProductDetailDb(DbNameSqlite);

            if (force)
            {
                await database.DeleteAllAsync(x => promotionRule.Contains( x._bonus_id ));
            }

            var stopwatch = Stopwatch.StartNew();

            HubPromotionProductDetail hubmanager = new HubPromotionProductDetail(Constants.Session);
            var resultCount = await hubmanager.GetCountByBonus(promotionRule);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / Constants.Session.odooConnection.DbLimitDefault;

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetItemsByBonusId(promotionRule, limit, indice);

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

        public async Task<bool> OnlinePromotionProductByRules(int[] promotionRule, bool force)
        {
            var database = new PromotionProductDb(DbNameSqlite);

            if (force)
            {
                await database.DeleteAllAsync(x => promotionRule.Contains(x._bonus_id));
            }

            var stopwatch = Stopwatch.StartNew();

            HubPromotionProduct hubmanager = new HubPromotionProduct(Constants.Session);
            var resultCount = await hubmanager.GetCountByBonus(promotionRule);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / Constants.Session.odooConnection.DbLimitDefault;

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetItemsByBonusId(promotionRule, limit, indice);

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

            var hubmanager = new HubPromoCenters(Constants.Session);
            var resultCount = await hubmanager.GetCount(promotionBenfit.id);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / Constants.Session.odooConnection.DbLimitDefault;

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

            Debug.WriteLine(String.Format("Lapso transcurrido: PromoCenter {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }

        public async Task<bool> PromoRules(PromotionBenefit promotionBenfit, bool force)
        {
            var database = new PromoRulesDb(DbNameSqlite);

            if(force)
            {
                await database.DeleteAllAsync(x=> x._promo_id == promotionBenfit.id);
            }

            var stopwatch = Stopwatch.StartNew();

            var hubmanager = new HubPromoRules(Constants.Session);
            var resultCount = await hubmanager.GetCount(promotionBenfit.id);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / Constants.Session.odooConnection.DbLimitDefault;

            

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetItemsByParentId(promotionBenfit.id, limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);                    
                }

                int[] RulesIds = responseAll.result.Select(r => r.id).ToArray();

                await OnlinePromotionProductByRules(RulesIds, true);
                await OnlinePromotionProductDetailByRules(RulesIds, true);

                if (indice >= maxIndexExceeded)
                {
                    Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: PromoRules {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }

        public async Task<bool> OnlinePromotionBenefit(bool force)
        {
            await PaymentMethod(true);
            await PosTarjetasCanal(true);

            var database = new PromotionBenefitDb(DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);
            DateTime current_datetime = DateTime.Now;

            var stopwatch = Stopwatch.StartNew();

            HubPromotionBenefit hubmanager = new HubPromotionBenefit(Constants.Session);
            var resultCount = await hubmanager.GetCount(lastDate.Value.Year, lastDate.Value.Month, lastDate.Value.Day);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / Constants.Session.odooConnection.DbLimitDefault;
            
            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetActivesByWriteDate(lastDate.Value, current_datetime, limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);

                    foreach (var item in responseAll.result)
                    {
                        //await LoyaltyFilters(item, true);
                        //await LoyaltyFiltersDetail(item, true);
                        
                        await PromoRules(item, true);
                        await OnlinePromotionProducts(item, true);
                        await OnlinePromotionProductDetail(item, true);
                        await PromoCenter(item, true);
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
