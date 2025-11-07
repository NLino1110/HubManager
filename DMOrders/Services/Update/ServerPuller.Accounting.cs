using ApiManager;
using CommunityToolkit.Maui.Core;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Origin;
using DMSA.Models.Odoo.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public async Task<bool> OnlineCalificacionCrediticia()
        {
            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubCalificacionCrediticia hubmanager = new ApiManager.HubCalificacionCrediticia(App.Session);
            var resultCount = await hubmanager.GetCount();

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit;

            var database = new CalificacionCrediticiaDb(DbNameSqlite);

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetByCreateDate(limit, indice, year, month, day);

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



        private async Task OnlineSyncAccountModule()
        {
            ApiManager.HubAccountModule hubAccountModule = new HubAccountModule(App.Session);
            var dataList = await hubAccountModule.GetAll();

            if (dataList != null && dataList.data != null && dataList.data.Length > 0)
            {
                var database = new AccountModuleDb();
                await database.InsertBatchAsync(dataList.data);
            }
        }

        private async Task OnlineSyncAccountTypeModule()
        {
            ApiManager.HubAccountTypeModule hubAccountModule = new HubAccountTypeModule(App.Session);
            var dataList = await hubAccountModule.GetAll();

            if (dataList != null && dataList.result != null && dataList.result.Length > 0)
            {
                var database = new AccountTypeModuleDb();

                foreach (var user in dataList.result)
                {
                    if (user.module_id != null && user.module_id.Length > 0)
                    {
                        user._module_id = user.module_id[0].id;
                    }
                }
                await database.InsertBatchAsync(dataList.result);
            }
        }



        public async Task<bool> OnlineSyncAccountMove()
        {
            DateTime dateTimeIni = DateTime.Now;

            ApiManager.HubAccountMove hubmanager = new ApiManager.HubAccountMove(App.Session);
            var resultCount = await hubmanager.GetHeaderCount(dateTimeIni.Year, dateTimeIni.Month, dateTimeIni.Day);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int limit = App.Session.odooConnection.DbLimitDefault;
            int countTotal = resultCount.result / limit;

            var database = new AccountMoveDb();

            for (int indice = 0; indice <= countTotal; indice++)
            {
                var responseAll = await hubmanager.GetAccountMoves(dateTimeIni, limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    responseAll.result = FixAccountMove(responseAll.result);
                    await database.InsertBatchAsync(responseAll.result);
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

            TimeSpan span = (DateTime.Now - dateTimeIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }


        private account_move[] FixAccountMove(account_move[] account_Moves)
        {
            foreach (var amItem in account_Moves)
            {
                if (amItem.reversed_entry_id != null && amItem.reversed_entry_id.Length > 0)
                {
                    amItem._reversed_entry_id = amItem.reversed_entry_id[0].id;
                }

                if (amItem.partner_id != null && amItem.partner_id.Length > 0)
                {
                    amItem._partner_id = amItem.partner_id[0].id;
                }

                if (amItem.journal_id != null && amItem.journal_id.Length > 0)
                {
                    amItem._journal_id = amItem.journal_id[0].id;
                }

                if (amItem.l10n_latam_document_type_id != null && amItem.l10n_latam_document_type_id.Length > 0)
                {
                    amItem._l10n_latam_document_type_id = amItem.l10n_latam_document_type_id[0].id;
                }

                if (amItem.invoice_user_id != null && amItem.invoice_user_id.Length > 0)
                {
                    amItem._invoice_user_id = amItem.invoice_user_id[0].id;
                }

                if (amItem.printer_id != null && amItem.printer_id.Length > 0)
                {
                    amItem._printer_id = amItem.printer_id[0].id;
                }

                if (amItem.printer_id != null && amItem.printer_id.Length > 0)
                {
                    amItem._printer_id = amItem.printer_id[0].id;
                }

                if (amItem.company_id != null && amItem.company_id.Length > 0)
                {
                    amItem._company_id = amItem.company_id[0].id;
                }

                if (amItem.team_id != null && amItem.team_id.Length > 0)
                {
                    amItem._team_id = amItem.team_id[0].id;
                }
            }

            return account_Moves;
        }
        private async Task OnlineSyncBank()
        {
            BankDb bankDb = new BankDb();
            await bankDb.Truncate();

            List<string> bank_ids_list = new List<string>();

            //Se obtienen las cuentas para ser insertados en la base local
            ApiManager.HubCuentas hubCuentas = new HubCuentas(App.Session);
            //var cuentasDeLista = await hubCuentas.GetAll(String.Join(",", accounts_journal_ids_list.ToArray()));
            var cuentasDeLista = await hubCuentas.GetAll();

            if (cuentasDeLista != null && cuentasDeLista.result != null && cuentasDeLista.result.Length > 0)
            {
                foreach (var pbItem in cuentasDeLista.result)
                {
                    pbItem.BankId = 0;
                    pbItem.PartnerId = 0;

                    if (pbItem.bank_id.Length > 0)
                    {
                        pbItem.BankId = pbItem.bank_id.FirstOrDefault().id;
                        bank_ids_list.Add(pbItem.BankId.ToString());
                    }

                    if (pbItem.partner_id.Length > 0)
                    {
                        pbItem.PartnerId = pbItem.partner_id.FirstOrDefault().id;
                    }

                    if (pbItem.currency_id.Length > 0)
                    {
                        pbItem.CurrencyId = pbItem.currency_id.FirstOrDefault().id;
                    }

                    if (pbItem.acc_holder_name.Trim().Equals("false"))
                    {
                        pbItem.acc_holder_name = "-";
                    }
                }

                PartnerBankDb parnetBankDb = new PartnerBankDb();
                await parnetBankDb.InsertBatchAsync(cuentasDeLista.result);

                Debug.WriteLine(cuentasDeLista.result.Length);
            }

            //Se obtienen bancos para ser insertados en la base local

            ApiManager.HubBancos hubBancos = new HubBancos(App.Session);
            //var bancosDeLista = await hubBancos.GetAll(String.Join(",", bank_ids_list.ToArray()));
            var bancosDeLista = await hubBancos.GetAll();

            if (bancosDeLista != null && bancosDeLista.result != null && bancosDeLista.result.Length > 0)
            {
                await bankDb.InsertBatchAsync(bancosDeLista.result);
            }
        }

        private async Task OnlineSyncJournal()
        {
            InboundPaymentMethodDb inboundPaymentMethodDb = new InboundPaymentMethodDb();
            await inboundPaymentMethodDb.TruncateAsync();

            //Se obtienen los diarios para ser insertados en la base local
            ApiManager.HubJournal hubDiarios = new HubJournal(App.Session);

            var ids = App.Session.CurrentUserFront.empresas.Select(e => e.id);
            string strEmpresas = string.Join(",", ids);

            var responsehubhubDiariosAll = await hubDiarios.GetAccountJournal(strEmpresas);

            AccountJournalDb accountJournalDb = new AccountJournalDb();
            BankDb bankDb = new BankDb();
            await bankDb.Truncate();
            //var res = accountJournalDb.GetItemsAsync();

            //string[] accounts_journal_ids = new string[] { };
            List<string> accounts_journal_ids_list = new List<string>();

            if (responsehubhubDiariosAll != null && responsehubhubDiariosAll.result != null)
            {
                //Debug.WriteLine(res.Count);
                foreach (var itemData in responsehubhubDiariosAll.result)
                {
                    //Se evalúa si debe usarse en la app
                    if (!itemData.use_mobile_app)
                    {
                        var isForApp = itemData.mobile_app_tag_ids.Where(i => i.code == App.Session.AppCodeOdoo).FirstOrDefault();
                        if (isForApp != null)
                        {

                        }
                        else
                        {
                            continue;
                        }
                    }

                    //accountJournalDb.InsertAsync(itemData);
                    itemData.BankAccountId = 0;
                    if (itemData.bank_account_id.Count > 0)
                    {
                        itemData.BankAccountId = itemData.bank_account_id.FirstOrDefault().id;

                        //Se agrega a la lista
                        accounts_journal_ids_list.Add(itemData.bank_account_id.FirstOrDefault().id.ToString());
                    }

                    itemData.CompanyId = 0;
                    if (itemData.company_id.Count > 0)
                    {
                        itemData.CompanyId = itemData.company_id.FirstOrDefault().id;
                    }

                    if (itemData.inbound_payment_method_line_ids.Count > 0)
                    {
                        itemData.inbound_payment_method_line_ids.ForEach(x => x.parent_id = itemData.id);
                        await inboundPaymentMethodDb.InsertBatchAsync(itemData.inbound_payment_method_line_ids.ToArray());
                    }

                    //Solo se insertarán las cuentas que tengan habilitado su uso en las apps móviles
                    await accountJournalDb.InsertAsync(itemData);
                }
            }
        }


        public async Task<bool> OnlineSyncAccountMoveLine()
        {
            DateTime dateTimeIni = DateTime.Now;

            Console.WriteLine("Iniciando proceso:" + " " + DateTime.Now.ToString());

            ApiManager.HubAccountMoveLine hubmanager = new ApiManager.HubAccountMoveLine(App.Session);

            var resultCount = await hubmanager.GetDetailCount(dateTimeIni);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int limit = App.Session.odooConnection.DbLimitDefault;
            int countTotal = resultCount.result / limit;

            var databaseDet = new AccountMoveLineDb();

            for (int indice = 0; indice <= countTotal; indice++)
            {

                var responseAll = await hubmanager.GetAccountMoveLines(dateTimeIni);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    foreach (var amlItem in responseAll.result)
                    {
                        amlItem.productId = get_from_token(amlItem.product_id);
                        amlItem.accountId = get_from_token(amlItem.account_id);
                        amlItem.moveId = get_from_token(amlItem.move_id);

                        //if (amlItem.product_id != null && amlItem.product_id.Length > 0)
                        //{
                        //    amlItem.productId = amlItem.product_id[0].id;
                        //}

                        //if (amlItem.account_id != null && amlItem.account_id.Length > 0)
                        //{
                        //    amlItem.accountId = amlItem.account_id[0].id;
                        //}

                        //if (amlItem.move_id != null && amlItem.move_id.Length > 0)
                        //{
                        //    amlItem.moveId = amlItem.move_id[0].id;
                        //}
                    }

                    await databaseDet.InsertBatchAsync(responseAll.result);
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

            TimeSpan span = (DateTime.Now - dateTimeIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }

        public void set_to_token(JToken token, int value)
        {
            if (token is JArray array && array.Count > 0)
            {
                array[0] = value;
            }
            else if (token is JValue)
            {
                token = new JArray { value };
            }
            else
            {
                token = new JArray { value };
            }
        }

        public int get_from_token(JToken token)
        {
            if (token is JArray array && array.Count > 0)
            {
                return array[0].Type == JTokenType.Integer ? (int)array[0] : 0;
            }

            else if (token is JValue value && value.Type == JTokenType.Boolean)
            {
                return 0;
            }
            return 0;
        }

        public async Task<bool> OnlineSyncAccountPaymentDaily()
        {
            DateTime dateTimeIni = DateTime.Now;

            ApiManager.HubAccountPaymentDaily hubmanager = new ApiManager.HubAccountPaymentDaily(App.Session);
            var resultCount = await hubmanager.GetCountByUser();

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int limit = App.Session.odooConnection.DbLimitDefault;
            int countTotal = resultCount.result / limit;

            var database = new AccountPaymentDailyDb();

            for (int indice = 0; indice <= countTotal; indice++)
            {
                var responseAll = await hubmanager.GetByUser(0);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    //await database.InsertBatchAsync(responseAll.data);

                    foreach (var itemPaymentDaily in responseAll.result)
                    {
                        var foundItem = await database.GetItemAsync(itemPaymentDaily.company_id, itemPaymentDaily.closing_id);
                        if (foundItem != null)
                        {
                            continue;
                        }
                        itemPaymentDaily.was_odoo_synced = true;
                        await database.InsertAsync(itemPaymentDaily);
                    }
                }

                Console.WriteLine("Página:" + indice);

                if (indice >= 600)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            TimeSpan span = (DateTime.Now - dateTimeIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }


        private async Task OnlineSyncFacturas(ProgressBarAnimationBehaviorPage obj,
            IToast toast,
            ToastDuration duration,
            double fontSize,
            CancellationTokenSource cancellationTokenSource,
            string fechaActualizaTablet
            )
        {

            obj.SetTitle($"Actualización en línea (facturas)...");

            var database = new AccountMoveDb();

            //string fechaActualizaTablet = "2021-01-01 00:00:00";
            bool esActualizacion = false;

            if ((await database.GetCount()) > 0)
            {
                esActualizacion = true;
            }

            obj.SetPercentProgress(0.10);

            //_appSession.EndPointServer = "http://192.168.204.32:8069";
            //_appSession.CurrentUser = new User()
            //{
            //    api_key = "110C6C7QU1YSWT6HW0MNXWL48L7802TG"
            //};

            DateTime dateTimeIni = DateTime.Now;

            //await ProcessFacHeaderOdoo(_appSession, apiRequest);
            //await ProcessFacDetailOdoo(_appSession, apiRequest);

            await OnlineSyncAccountMove();
            await OnlineSyncAccountMoveLine();

            //TODO: Ya no se sincronizarán productos en linea
            //await OnlineSyncProduct(_appSession, apiRequest);

            //TODO: Eliminado brand para Odoo 18
            await OnlineSyncUsers();

            Debug.WriteLine("Importación en linea account.move terminada");
        }


        private async Task OnlineSyncUsers()
        {
            ApiManager.HubUser hubProductBrand = new HubUser(App.Session);
            var dataList = await hubProductBrand.GetItems();

            if (dataList != null && dataList.result != null && dataList.result.Length > 0)
            {
                var database = new UserDb(App.Session.odooConnection.DbNameSqlite);
                //foreach (var user in dataList.result)
                //{
                    //if (user.company_id != null && user.company_id.Length > 0)
                    //{
                    //    user._company_id = user.company_id[0].id;
                    //}

                    //if (user.partner_id != null && user.partner_id.Length > 0)
                    //{
                    //    user._partner_id = user.partner_id[0].id;
                    //    user.partner_name = user.partner_id[0].name;
                    //}

                    //if (user.sale_team_id != null && user.sale_team_id.Length > 0)
                    //{
                    //    user._sale_team_id = user.sale_team_id[0].id;
                    //}
                //}

                await database.InsertBatchAsync(dataList.result);
            }
        }


        public async Task<bool> OnlineSyncPaymentHeader()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            //settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            settings.ContractResolver = new IncludeJsonIgnoreResolver();

            DateTime dateIni = DateTime.Now.AddDays(-150);
            DateTime dateEnd = DateTime.Now;

            ApiManager.HubAccountPaymentHeader hubmanager = new ApiManager.HubAccountPaymentHeader(App.Session);
            var resultCount = await hubmanager.GetCount(dateIni, dateEnd);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int limit = App.Session.odooConnection.DbLimitDefault;
            int countTotal = resultCount.result / limit;

            var database = new AccountPaymentHeaderDb();
            var databasePay = new AccountPaymentDb();
            var databaseInvoLine = new AccountPaymentInvoiceLineDb();

            for (int indice = 0; indice <= countTotal; indice++)
            {
                var responseAll = await hubmanager.GetItemsFull(1, dateIni, dateEnd, limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    //await database.InsertBatchAsync(responseAll.data);

                    //Iniciando inserción
                    foreach (var headerItem in responseAll.result)
                    {
                        var foundHeader = await database.GetItemByGuidAsync(headerItem.guid);
                        if (foundHeader != null)
                        {
                            Debug.WriteLine("Registro ya existe en la base de datos!, no se sincronizará");
                            Debug.WriteLine(foundHeader.guid);
                            Debug.WriteLine(foundHeader.recipe_name);
                            continue;
                        }

                        string jsonHeaderItem = JsonConvert.SerializeObject(headerItem); //, settings);
                        var newHeaderItem = JsonConvert.DeserializeObject<AccountPaymentHeader>(jsonHeaderItem);

                        var itemFound = await database.GetItemByGuidAsync(newHeaderItem.guid);

                        if (itemFound != null) continue;

                        newHeaderItem.was_odoo_synced = true;
                        int newHeaderId = await database.InsertAsync(newHeaderItem);

                        //TODO: Podrian venir vacíos porque pudieron haberse borrado
                        if (headerItem.payments != null)
                        {
                            foreach (var paymentItem in headerItem.payments)
                            {
                                paymentItem.parent_id = newHeaderItem.id;

                                string jsonPaymentItem = JsonConvert.SerializeObject(paymentItem, settings); //, settings);
                                var newPaymentItem = JsonConvert.DeserializeObject<AccountPayment>(jsonPaymentItem, settings);

                                int newPayId = await databasePay.InsertAsync(newPaymentItem);

                                if (paymentItem.lines != null)
                                {
                                    foreach (var lineItem in paymentItem.lines)
                                    {
                                        lineItem.parent_payment_id = newPaymentItem.id;
                                        string jsonLineItem = JsonConvert.SerializeObject(lineItem, settings); //, settings);
                                        var newLineItem = JsonConvert.DeserializeObject<AccountPaymentInvoiceLine>(jsonLineItem, settings);
                                        await databaseInvoLine.InsertAsync(newLineItem);
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

    }
}
