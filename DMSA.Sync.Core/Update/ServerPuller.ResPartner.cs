using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tools;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using Newtonsoft.Json;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> DownloadAccountMoveRefund()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            //settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            settings.ContractResolver = new IncludeJsonIgnoreResolver();

            DateTime dateIni = DateTime.Now.AddDays(-150);
            DateTime dateEnd = DateTime.Now;

            ApiManager.HubAccountMoveSendHeader hubmanager = new ApiManager.HubAccountMoveSendHeader(appSession);
            var resultCount = await hubmanager.GetHeaderCount(dateIni, dateEnd);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / 300;

            var databaseHeader = new AccountMoveSendHeaderDb(Constants.Session.odooConnection.DbNameSqlite);
            var databaseMoveSend = new AccountMoveSendDb(Constants.Session.odooConnection.DbNameSqlite);
            var databaseMoveLineSend = new AccountMoveLineSendDb(Constants.Session.odooConnection.DbNameSqlite);

            for (int indice = 0; indice <= countTotal; indice++)
            {
                var responseAll = await hubmanager.GetItemsFull(dateIni, dateEnd, indice);

                if (responseAll != null && responseAll.Length > 0)
                {
                    //await database.InsertBatchAsync(responseAll.data);

                    //Iniciando inserción
                    foreach (var headerItem in responseAll)
                    {
                        //var foundHeader = await databaseHeader.GetItemByGuidAsync(headerItem.guid);
                        var foundHeader = await databaseHeader.GetByRequestName(headerItem.request_name);
                        if (foundHeader != null)
                        {
                            Debug.WriteLine("Registro ya existe en la base de datos!, no se sincronizará");
                            Debug.WriteLine(foundHeader.request_name);
                            //Debug.WriteLine(foundHeader.recipe_name);
                            continue;
                        }

                        string jsonHeaderItem = JsonConvert.SerializeObject(headerItem); //, settings);
                        var newHeaderItem = JsonConvert.DeserializeObject<AccountMoveSendHeader>(jsonHeaderItem);

                        //var itemFound = await databaseHeader.GetItemByGuidAsync(newHeaderItem.guid);

                        //if (itemFound != null) continue;

                        newHeaderItem.was_odoo_synced = true;
                        int newHeaderId = await databaseHeader.InsertAsync(newHeaderItem);

                        //TODO: Podrian venir vacíos porque pudieron haberse borrado
                        if (headerItem.account_moves != null)
                        {
                            foreach (var paymentItem in headerItem.account_moves)
                            {
                                paymentItem.parent_id = newHeaderItem.id;
                                paymentItem.was_odoo_synced = true;
                                string jsonPaymentItem = JsonConvert.SerializeObject(paymentItem, settings); //, settings);
                                var newPaymentItem = JsonConvert.DeserializeObject<account_move_send>(jsonPaymentItem, settings);

                                int newPayId = await databaseMoveSend.InsertAsync(newPaymentItem);

                                if (paymentItem.lines != null)
                                {
                                    foreach (var lineItem in paymentItem.lines)
                                    {
                                        lineItem.parent_move_id = newPaymentItem.id;
                                        lineItem.was_odoo_synced = true;
                                        string jsonLineItem = JsonConvert.SerializeObject(lineItem, settings); //, settings);
                                        var newLineItem = JsonConvert.DeserializeObject<account_move_line_send>(jsonLineItem, settings);
                                        await databaseMoveLineSend.InsertAsync(newLineItem);
                                    }
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("Página:" + indice);

                //TODO: Se fuerza la salida para que no se quede ciclado en caso de que haya
                // problemas de conexion con el servidor
                // el objetivo es que el servidor no se sobrecargue

                if (indice >= 600)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            TimeSpan span = (DateTime.Now - dateIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }

        public async Task<bool> OnlineSyncResPartner()
        {
            var stopwatch = Stopwatch.StartNew();

            var database = new ResPartnerDb(Constants.Session.odooConnection.DbNameSqlite);
            
            int partner_id = Constants.Session.CurrentUserFront.partner_id;

            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);
            
            ApiManager.HubResPartner hubmanager = new ApiManager.HubResPartner(appSession);
            var resultCount = await hubmanager.GetCountBySeller(lastDate.Value.Year, lastDate.Value.Month, lastDate.Value.Day, partner_id);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit;
            
            for (int indice = 0; indice <= countTotal; indice++)
            {
                var responseAll = await hubmanager.GetByWriteDateBySeller(lastDate.Value, limit, indice, partner_id);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                //obtenemos los campos child_ids para los contactos asociados
                for(int i = 0; i < responseAll.result.Length; i++)
                {
                    var item = responseAll.result[i];
                    
                    if(string.IsNullOrEmpty(item.childs_ids_json) || item.childs_ids_json.Equals("[]"))
                        continue;

                    var _childs_ids = JsonConvert.DeserializeObject<int[]>(item.childs_ids_json);

                    if (_childs_ids != null && _childs_ids.Length > 0)
                    {
                        var responseChild = await hubmanager.GetByIds(limit, 0, _childs_ids);
                        if (responseChild != null && responseChild.result != null && responseChild.result.Length > 0)
                        {
                            await database.InsertBatchAsync(responseChild.result);
                        }
                        //foreach (var child_id in _childs_ids)
                        //{

                        //}
                    }
                }

                Console.WriteLine("Página:" + indice);

                if (indice >= 600)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }


        public async Task<bool> OnlineSyncResPartnerFull()
        {
            var stopwatch = Stopwatch.StartNew();

            var database = new ResPartnerDb(Constants.Session.odooConnection.DbNameSqlite);

            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            ApiManager.HubResPartner hubmanager = new ApiManager.HubResPartner(appSession);
            var resultCount = await hubmanager.GetCount(lastDate.Value.Year, lastDate.Value.Month, lastDate.Value.Day);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit;

            for (int indice = 0; indice <= countTotal; indice++)
            {
                var responseAll = await hubmanager.GetByWriteDate(lastDate.Value, limit, indice);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                    //await database.InsertBatchControlAsync(responseAll.result);
                }

                Console.WriteLine("Página:" + indice);

                if (indice >= 600)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
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
