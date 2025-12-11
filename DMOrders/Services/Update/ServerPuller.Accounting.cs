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
        public async Task<bool> OnlineAccountTaxes()
        {
            var stopwatch = Stopwatch.StartNew();

            var productProductDb = new ProductProductDb(DbNameSqlite);

            var idstaxes = await productProductDb.GetAllTaxesIdsAsync();
            
            //string idsTaxesStr = string.Join(",", idstaxes.Distinct());

            ApiManager.HubAccountTax hubmanager = new ApiManager.HubAccountTax(App.Session);
            var resultCount = await hubmanager.GetCount(idstaxes);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit;

            var database = new AccountTaxDb(DbNameSqlite);

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetAll(idstaxes);

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

        private account_move[] FixAccountMove(account_move[] account_Moves)
        {
            foreach (var amItem in account_Moves)
            {
                //if (amItem.reversed_entry_id != null && amItem.reversed_entry_id.Length > 0)
                //{
                //    amItem._reversed_entry_id = amItem.reversed_entry_id[0].id;
                //}

                //if (amItem.partner_id != null && amItem.partner_id.Length > 0)
                //{
                //    amItem._partner_id = amItem.partner_id[0].id;
                //}

                //if (amItem.journal_id != null && amItem.journal_id.Length > 0)
                //{
                //    amItem._journal_id = amItem.journal_id[0].id;
                //}

                //if (amItem.l10n_latam_document_type_id != null && amItem.l10n_latam_document_type_id.Length > 0)
                //{
                //    amItem._l10n_latam_document_type_id = amItem.l10n_latam_document_type_id[0].id;
                //}

                //if (amItem.invoice_user_id != null && amItem.invoice_user_id.Length > 0)
                //{
                //    amItem._invoice_user_id = amItem.invoice_user_id[0].id;
                //}

                //if (amItem.printer_id != null && amItem.printer_id.Length > 0)
                //{
                //    amItem._printer_id = amItem.printer_id[0].id;
                //}

                //if (amItem.printer_id != null && amItem.printer_id.Length > 0)
                //{
                //    amItem._printer_id = amItem.printer_id[0].id;
                //}

                //if (amItem.company_id != null && amItem.company_id.Length > 0)
                //{
                //    amItem._company_id = amItem.company_id[0].id;
                //}

                //if (amItem.team_id != null && amItem.team_id.Length > 0)
                //{
                //    amItem._team_id = amItem.team_id[0].id;
                //}
            }

            return account_Moves;
        }
        //private async Task OnlineSyncBank()
        //{
        //    BankDb bankDb = new BankDb();
        //    await bankDb.Truncate();

        //    List<string> bank_ids_list = new List<string>();

        //    //Se obtienen las cuentas para ser insertados en la base local
        //    ApiManager.HubCuentas hubCuentas = new HubCuentas(App.Session);
        //    //var cuentasDeLista = await hubCuentas.GetAll(String.Join(",", accounts_journal_ids_list.ToArray()));
        //    var cuentasDeLista = await hubCuentas.GetAll();

        //    if (cuentasDeLista != null && cuentasDeLista.result != null && cuentasDeLista.result.Length > 0)
        //    {
        //        foreach (var pbItem in cuentasDeLista.result)
        //        {
        //            pbItem._bank_id = 0;
        //            pbItem._partner_id = 0;

        //            //if (pbItem.bank_id.Length > 0)
        //            //{
        //            //    pbItem._bank_id = pbItem.bank_id.FirstOrDefault().id;
        //            //    bank_ids_list.Add(pbItem._bank_id.ToString());
        //            //}

        //            //if (pbItem.partner_id.Length > 0)
        //            //{
        //            //    pbItem._partner_id = pbItem.partner_id.FirstOrDefault().id;
        //            //}

        //            //if (pbItem.currency_id.Length > 0)
        //            //{
        //            //    pbItem._currency_id = pbItem.currency_id.FirstOrDefault().id;
        //            //}

        //            if (pbItem.acc_holder_name.Trim().Equals("false"))
        //            {
        //                pbItem.acc_holder_name = "-";
        //            }
        //        }

        //        PartnerBankDb parnetBankDb = new PartnerBankDb();
        //        await parnetBankDb.InsertBatchAsync(cuentasDeLista.result);

        //        Debug.WriteLine(cuentasDeLista.result.Length);
        //    }

        //    //Se obtienen bancos para ser insertados en la base local

        //    ApiManager.HubBank hubBancos = new HubBank(App.Session);
        //    //var bancosDeLista = await hubBancos.GetAll(String.Join(",", bank_ids_list.ToArray()));
        //    var bancosDeLista = await hubBancos.GetAll();

        //    if (bancosDeLista != null && bancosDeLista.result != null && bancosDeLista.result.Length > 0)
        //    {
        //        await bankDb.InsertBatchAsync(bancosDeLista.result);
        //    }
        //}


        
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

    }
}
