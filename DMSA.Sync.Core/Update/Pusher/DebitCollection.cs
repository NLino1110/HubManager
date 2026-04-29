using ApiManager;
using ApiManagerOdoo.Accounting;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tools;
using DMSA.Sync.Core.Database.Sqlite.DebitCollection;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using Newtonsoft.Json;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update.Pusher
{
    public static class DebitCollection
    {
        static public async Task<ApiResponseOdooRpcT<List<OdooRpcResultInt>>?> SendPayment(MultipleCobrosInvoice multipleCobrosInvoice, bool autosend)
        {
            string sync_mode = "manual";
            if(autosend)
                sync_mode = "automatic";

            ApiResponseOdooRpcT<List<OdooRpcResultInt>> resultTask = new ApiResponseOdooRpcT<List<OdooRpcResultInt>>()
            {
                id = 0,
                result = new List<OdooRpcResultInt>(),
                jsonrpc = "2.0",
                error = null
            };

            var multipleCobrosInvoiceDB = new MultipleCobrosInvoiceDb(Constants.Session.odooConnection.DbNameSqlite);

            if (multipleCobrosInvoice.payment_status == CobrosEstados.PENDIENTE)
            {
                string fechaActual = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10);
                
                //string newGuid = Guid.NewGuid().ToString("N");
                //multipleCobrosInvoice.external_guid = newGuid;
                multipleCobrosInvoice.payment_status = CobrosEstados.ENVIANDO;
                string new_recipe_name = await multipleCobrosInvoiceDB.BuildRecipeName(multipleCobrosInvoice);
                multipleCobrosInvoice.receipt_name = new_recipe_name;
                await multipleCobrosInvoiceDB.UpdateAsync(multipleCobrosInvoice);
            }

            if (DeviceInfo.Current.Platform == DevicePlatform.Android ||
                    DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                multipleCobrosInvoice.device_idiom = DeviceInfo.Idiom.ToString();
                multipleCobrosInvoice.device_model = DeviceInfo.Current.Model;
                multipleCobrosInvoice.device_manufacturer = DeviceInfo.Current.Manufacturer;
                multipleCobrosInvoice.sync_mode = sync_mode;
            }

            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                multipleCobrosInvoice.device_idiom = DeviceInfo.Idiom.ToString();
                multipleCobrosInvoice.device_model = "-";
                multipleCobrosInvoice.device_manufacturer = "-";
                multipleCobrosInvoice.sync_mode = sync_mode;
            }

            int user_id = Constants.Session.CurrentUserFront.uid;
            string user_name = Constants.Session.CurrentUserFront.username;
            
            if (multipleCobrosInvoice.receipt_receipts_id == 0)
            {
                var receiptReceiptsLineDb = new ReceiptReceiptsLineDb(Constants.Session.odooConnection.DbNameSqlite);
                var receiptLines = (await receiptReceiptsLineDb.GetItemsAsync(x => x._sale_user_id == user_id && x.state == "draft"))
                    .OrderBy(x=>x.number_seq)
                    .Take(1)
                    .ToList();

                if (receiptLines.Count > 0)
                {
                    multipleCobrosInvoice.receipt_receipts_id = receiptLines[0]._receipt_receipts_id;
                    multipleCobrosInvoice.receipt_receipts_line_id = receiptLines[0].id;
                    //multipleCobrosInvoice.recipe_name = multipleCobrosInvoiceDB.BuildName(multipleCobrosInvoice, user_name, multipleCobrosInvoice.receipt_receipts_line_id);

                    await multipleCobrosInvoiceDB.UpdateAsync(multipleCobrosInvoice);

                    receiptLines[0].state = "used";
                    await receiptReceiptsLineDb.UpdateAsync(receiptLines[0]);
                }
            }

            //TODO: Se coloca directamente "unknow" ya que las nuevas versiones de Android
            // no permiten obtener el número de serie de los dispositivos

            multipleCobrosInvoice.device_serial = "unknown" + "-" + Constants.Session.AppVersion;
            multipleCobrosInvoice.device_app_version = Constants.Session.AppVersion;
            multipleCobrosInvoice.origin_mobile_app = Constants.Session.AppCodeOdoo;

            HubMultipleCobrosInvoice apiProcessor = new HubMultipleCobrosInvoice(Constants.Session);

            var multipleCobrosInvoiceLineDb = new MultipleCobrosInvoiceLineDb(Constants.Session.odooConnection.DbNameSqlite);

            var multipleCobrosInvoiceLinList = await multipleCobrosInvoiceLineDb.GetItemsAsync(x => x.MultipleCobrosInvoiceId == multipleCobrosInvoice.id);

            var multipleCobrosInvoiceLinesAi = new MultipleCobrosInvoiceLineAiDb(Constants.Session.odooConnection.DbNameSqlite);

            bool everyThingOk = false;
            foreach (var payment in multipleCobrosInvoiceLinList)
            {
                //TODO: Check this line
                //payment.recipe_name = _accountPaymentHeader.recipe_name;

                decimal? CuadraturaAmount = 0;
                CuadraturaAmount = payment.Amount;

                //Se consultan lineas de pagos de documentos
                var apl = await multipleCobrosInvoiceLinesAi.GetItemsAsync(x => x.multiple_cobros_invoice_line_id == payment.Id);
                if (apl.Count > 0)
                {
                    payment.lines = apl.ToArray();

                    //Se realiza esta operación ya que Odoo requiere una estructura específica para la 
                    // recepcion de facturas en los pagos, ej:

                    //[
                    //  [
                    //      0,
                    //      0,
                    //      {
                    //          "invoice_line_id": 6629,                
                    //          "invoice_line_id_name": "Fact 200-201-000000070",
                    //          "payment_state": "draft",
                    //          "reconcile_amount": 12.0
                    //      }
                    //  ]
                    //]

                    var listLines = new List<MultipleCobrosInvoiceLineAiWrapper>();
                    
                    foreach (var line in payment.lines)
                    {
                        CuadraturaAmount -= line.amount_asigned;
                        listLines.Add(new MultipleCobrosInvoiceLineAiWrapper(line));
                    }

                    payment.MultipleCobrosInvoiceLineAi = listLines;
                }
                else
                {
                    payment.MultipleCobrosInvoiceLineAi = new List<MultipleCobrosInvoiceLineAiWrapper>();
                }

                var CuadraturasIds = new List<MultipleCobrosInvoiceLineCuadraturaWrapper>();

                if (CuadraturaAmount > 0)
                {
                    var CuadraturaItem = new MultipleCobrosInvoiceLineCuadratura();

                    CuadraturaItem.account_cuadre_id = Constants.Session.res_Company.cuadratura_account_id_;
                    CuadraturaItem.amount = CuadraturaAmount;
                    CuadraturaItem.partner_id = multipleCobrosInvoice.partner_id;
                    CuadraturasIds.Add(new MultipleCobrosInvoiceLineCuadraturaWrapper(CuadraturaItem));                    
                }

                payment.CuadraturaIds = CuadraturasIds;

                //TODO: Verificar si esta linea es necesaria
                //payment.check_number = payment.number_check_customer;
                payment.PartnerBankId = payment.PartnerBankId;
            }

            resultTask = await apiProcessor.Create(multipleCobrosInvoice);

            if (resultTask == null)
            {
                //await Toast.Make("Error: Datos!").Show();
                multipleCobrosInvoice.payment_status = CobrosEstados.ERROR;
                multipleCobrosInvoice.write_date = DateTime.Now;
                await multipleCobrosInvoiceDB.UpdateAsync(multipleCobrosInvoice);
            }
            else
            {
                if (resultTask.result!=null && resultTask.result.Count > 0 && resultTask.error == null)
                {
                    //Confirmación del pago (botón confirmar)
                    //ApiResponseOdooRpcT<int> resultTask_confirm = new ApiResponseOdooRpcT<int>()
                    //{
                    //    id = 0,
                    //    result = 0,
                    //    jsonrpc = "2.0",
                    //    error = null
                    //};

                    //resultTask_confirm = await apiProcessor.call_button(payment, resultTask);

                    //////var kwargs = new { };
                    //////object[] args = new object[] { };

                    //////var pre_aprobed_data = await apiProcessor.CallMethod<ApiResponseOdooRpcT<List<wkf_state_order>>>("/web/dataset/call_kw",
                    //////        Method.Post,
                    //////        args,
                    //////        kwargs, "wkf.state.order", "web_save");

                    //--------------------------------------------

                    //await Toast.Make("Envío de cobro correcto").Show();
                    Debug.WriteLine("Terminado envío!");
                    multipleCobrosInvoice.payment_status = CobrosEstados.PROCESADO;
                    multipleCobrosInvoice.write_date = DateTime.Now;

                    if(autosend)
                    {
                        if(!multipleCobrosInvoice.receipt_name.StartsWith("AUT"))
                        {
                            multipleCobrosInvoice.receipt_name = "AUT-" + multipleCobrosInvoice.receipt_name;
                        }
                    }

                    await multipleCobrosInvoiceDB.UpdateAsync(multipleCobrosInvoice);
                    everyThingOk = true;
                }
                else
                {
                    //await Toast.Make("Error: " + resultTask.message).Show();
                    multipleCobrosInvoice.payment_status = CobrosEstados.ERROR;
                    multipleCobrosInvoice.write_date = DateTime.Now;
                    await multipleCobrosInvoiceDB.UpdateAsync(multipleCobrosInvoice);
                    //everyThingOk = false;
                }
            }

            if (everyThingOk)
            {                
                var accountPaymentHeader = new HubMultipleCobrosInvoice(Constants.Session);
                string jsonSerialized = JsonConvert.SerializeObject(multipleCobrosInvoice);
                MultipleCobrosInvoice objSend = JsonConvert.DeserializeObject<MultipleCobrosInvoice>(jsonSerialized);
                objSend.lines = Array.Empty<MultipleCobrosInvoiceLine>();

                MultipleCobrosInvoiceLine[] paymentSend = new MultipleCobrosInvoiceLine[] { };

                List<MultipleCobrosInvoiceLine> paymentSendList = new List<MultipleCobrosInvoiceLine>();

                foreach (var paymentItem in multipleCobrosInvoiceLinList)
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";                    
                    settings.ContractResolver = new IgnorePropertyResolver("multiple_cobros_invoice_line_ai", "cuadratura_ids");

                    string jsonPaymentItem = JsonConvert.SerializeObject(paymentItem, settings);
                    MultipleCobrosInvoiceLine objPaymentSend = JsonConvert.DeserializeObject<MultipleCobrosInvoiceLine>(jsonPaymentItem, settings);

                    string JsonAccountPaymentSendLines = JsonConvert.SerializeObject(paymentItem.lines, settings);
                    objPaymentSend.lines = JsonConvert.DeserializeObject<MultipleCobrosInvoiceLineAi[]>(JsonAccountPaymentSendLines);

                    paymentSendList.Add(paymentItem);
                }

                paymentSend = paymentSendList.ToArray();
                
                foreach (var payment in paymentSend)
                {                        
                    //payment.lines = Array.Empty<MultipleCobrosInvoiceLineAi>();
                    payment.MultipleCobrosInvoiceId = resultTask.result[0].id;
                    var paymentsResult = await accountPaymentHeader.SendPayments(payment);                       
                    if (paymentsResult != null && paymentsResult.result != null && paymentsResult.result.Count > 0)
                    {
                        var lines = payment.lines;
                        if (lines != null)
                        {
                            var accountMoveDb = new AccountMoveDb(Constants.Session.odooConnection.DbNameSqlite);

                            foreach (var paymentLine in lines)
                            {
                                var accountMovefound = (await accountMoveDb.GetItemsAsync(x=> x.docnum_mask == paymentLine.docnum_mask)).FirstOrDefault();
                                if(accountMovefound != null)
                                {
                                    if(accountMovefound.amount_residual_virtual == 0 && accountMovefound.amount_residual > 0)
                                    {
                                        accountMovefound.amount_residual_virtual = accountMovefound.amount_residual;
                                    }

                                    accountMovefound.amount_residual_virtual -= paymentLine.amount_asigned;
                                    await accountMoveDb.UpdateAsync(accountMovefound);
                                }
                            }
                        }
                    }
                }                
            }

            return resultTask;
        }

        //public static async Task<ApiResponseOdooRpcT<check_cn_response[]>> CheckCreditNoteOverdraf(CreditNoteRequestGroup _accountMoveSendHeader)
        //{
        //    ApiResponseOdooRpcT<check_cn_response[]> resultTask = new ApiResponseOdooRpcT<check_cn_response[]>();

        //    List<credit_note_request_detail> account_Move_Line_Sends = new List<credit_note_request_detail>();

        //    HubCreditNoteRequest hubAccountMoveRefund = new HubCreditNoteRequest(Constants.Session);
        //    CreditNoteRequestDb accountMoveSendDb = new CreditNoteRequestDb(Constants.Session.odooConnection.DbNameSqlite);

        //    var accountMoveSendList = await accountMoveSendDb.GetByParent(_accountMoveSendHeader.id);

        //    foreach (var accountMoveSend in accountMoveSendList)
        //    {
        //        CreditNoteRequestDetailDb accountMoveLineSendDb = new CreditNoteRequestDetailDb(Constants.Session.odooConnection.DbNameSqlite);
        //        var linesItems = await accountMoveLineSendDb.GetItemsByParentAsync(accountMoveSend.id);

        //        int sequence = 1;
        //        foreach (var lineItem in linesItems)
        //        {
        //            account_Move_Line_Sends.Add(lineItem);
        //        }
        //    }

        //    var requestObject = new Models.General.Requests.ApiRequestCheckCn();
        //    requestObject.credit_note_lines = account_Move_Line_Sends.ToArray();

        //    resultTask = await hubAccountMoveRefund.check_credit_note_overdraft(requestObject);

        //    return resultTask;
        //}

        public static async Task<ApiResponseOdooRpcT<List<OdooRpcResultInt>>?> SendRequestCreditNote(CreditNoteRequestGroup _creditNoteRequestGroup)
        {
            ApiResponseOdooRpcT<List<OdooRpcResultInt>>? resultTask = new ApiResponseOdooRpcT<List<OdooRpcResultInt>>()
            {
                id = 0,
                jsonrpc = "2.0",
                result = new List<OdooRpcResultInt>(),
                error = null
            };

            CreditNoteRequestGroupDb accountMoveSendHeaderDb = new CreditNoteRequestGroupDb(Constants.Session.odooConnection.DbNameSqlite);

            if (_creditNoteRequestGroup.request_status == Models.MoveStatus.PENDIENTE)
            {
                string fechaActual = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10);                
                string request_name = await accountMoveSendHeaderDb.BuildRequestName(_creditNoteRequestGroup);

                _creditNoteRequestGroup.request_name = request_name;
                _creditNoteRequestGroup.request_status = Models.MoveStatus.ENVIANDO;
                await accountMoveSendHeaderDb.UpdateAsync(_creditNoteRequestGroup);
            }

            if (DeviceInfo.Current.Platform == DevicePlatform.Android ||
                    DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                _creditNoteRequestGroup.model = DeviceInfo.Idiom.ToString() + " - " + DeviceInfo.Current.Model;
                _creditNoteRequestGroup.manufacturer = DeviceInfo.Current.Manufacturer;
                _creditNoteRequestGroup.autosend = "N";
            }

            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                _creditNoteRequestGroup.model = DeviceInfo.Idiom.ToString() + " - " + "-";
                _creditNoteRequestGroup.manufacturer = "-";
                _creditNoteRequestGroup.autosend = "N";
            }

            _creditNoteRequestGroup.serialNumber = "unknown" + "-" + Constants.Session.AppVersion;

            CreditNoteRequestDb creditNoteRequestDb = new CreditNoteRequestDb(Constants.Session.odooConnection.DbNameSqlite);
            CreditNoteRequestDetailDb creditNoteRequestDetailDb = new CreditNoteRequestDetailDb(Constants.Session.odooConnection.DbNameSqlite);

            var creditNoteRequestDbList = await creditNoteRequestDb.GetByParent(_creditNoteRequestGroup.id);

            foreach (var CreditNoteRequestItem in creditNoteRequestDbList)
            {
                CreditNoteRequestItem.request_name = _creditNoteRequestGroup.request_name;
                                
                var linesItems = await creditNoteRequestDetailDb.GetItemsByParentAsync(CreditNoteRequestItem.id);
                if (linesItems.Count > 0)
                {
                    List<object> listLines = new List<object>();
                    
                    int sequence = 1;

                    foreach (var line in linesItems)
                    {
                        int[] tax_ids = Array.Empty<int>();

                        if (!string.IsNullOrWhiteSpace(line.tax_ids_json))
                        {
                            tax_ids = line.tax_ids_json
                                .Trim('[', ']')
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => int.TryParse(x, out var n) ? n : 0)
                                .Where(n => n != 0)
                                .ToArray();
                        }

                        line.tax_ids = tax_ids;

                        listLines.Add(new List<object>
                        {
                            0,
                            0,
                            line
                        });
                        sequence++;
                    }

                    CreditNoteRequestItem.invoice_line_ids = listLines;
                    CreditNoteRequestItem.accountMovesProducts = listLines;
                    CreditNoteRequestItem.lines = linesItems.ToArray();
                }
                else
                {
                    CreditNoteRequestItem.invoice_line_ids = new List<object>();
                }

                resultTask = await SendCreditNoteRequest(CreditNoteRequestItem);

                if (resultTask == null)
                {
                    //await Toast.Make("Error: Datos!").Show();
                    _creditNoteRequestGroup.request_status = Models.MoveStatus.ERROR;
                    _creditNoteRequestGroup.modification_datetime = DateTime.Now;
                    await accountMoveSendHeaderDb.UpdateAsync(_creditNoteRequestGroup);
                }
                else
                {
                    if (resultTask.result != null && resultTask.result.Count >0 && resultTask.error == null)
                    {
                        //await Toast.Make("Envío de solicitud correcto").Show();
                        _creditNoteRequestGroup.request_status = Models.MoveStatus.RECIBIDO;
                        _creditNoteRequestGroup.modification_datetime = DateTime.Now;
                        await accountMoveSendHeaderDb.UpdateAsync(_creditNoteRequestGroup);                        
                    }
                    else
                    {
                        //await Toast.Make("Error: " + resultTask.message).Show();
                        _creditNoteRequestGroup.request_status = Models.MoveStatus.ERROR;
                        _creditNoteRequestGroup.modification_datetime = DateTime.Now;
                        await accountMoveSendHeaderDb.UpdateAsync(_creditNoteRequestGroup);
                    }
                }
            }

            bool everyThingOk = true;
            if (everyThingOk)
            {
                //List<AccountPaymentSend> accountMoveSendList = new List<AccountPaymentSend>();

                //AccountPaymentSend[] paymentSend = new AccountPaymentSend[] { };


                //HubAccountPaymentHeader accountPaymentHeader = new HubAccountPaymentHeader(App.Session);
                //string jsonSerialized = JsonConvert.SerializeObject(_accountMoveSendHeader);
                //CreditNoteRequestGroup objSend = JsonConvert.DeserializeObject<CreditNoteRequestGroup>(jsonSerialized);
                
                List<credit_note_request> accountMoveSendListNew = new List<credit_note_request>();

                foreach (var accountMoveItem in creditNoteRequestDbList)
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    settings.ContractResolver = new IncludeJsonIgnoreResolver();

                    Console.WriteLine(accountMoveItem);

                    string jsonPaymentItem = JsonConvert.SerializeObject(accountMoveItem, settings);
                    credit_note_request objPaymentSend = JsonConvert.DeserializeObject<credit_note_request>(jsonPaymentItem, settings);

                    string JsonAccountPaymentSendLines = JsonConvert.SerializeObject(accountMoveItem.lines, settings);
                    objPaymentSend.lines = JsonConvert.DeserializeObject<credit_note_request_detail[]>(JsonAccountPaymentSendLines);
                    accountMoveSendListNew.Add(objPaymentSend);
                }

                //Se guarda cabecera de solicitudes en Odoo solo cuando se han enviado correctamente las anteriores
                HubCreditNoteRequestGroup hubAccountMoveHeader = new HubCreditNoteRequestGroup(Constants.Session);
                //string jsonSerialized = JsonConvert.SerializeObject(_accountMoveSendHeader);
                //Acco objSend = JsonConvert.DeserializeObject<AccountPaymentDaily>(jsonSerialized);

                ApiResponseOdooRpcT<int> headerResult = await hubAccountMoveHeader.SendHeader(_creditNoteRequestGroup);
                //var headerResult = await hubAccountMoveHeader.SendHeader(objSend);
                Debug.WriteLine(headerResult);

                if (headerResult != null && headerResult.result > 0)
                {
                    foreach (var accountMoveItem in accountMoveSendListNew.ToArray())
                    {
                        var lines = accountMoveItem.lines;
                       
                        accountMoveItem.lines = Array.Empty<credit_note_request_detail>();
                        accountMoveItem.invoice_line_ids = new  List<object>();
                        accountMoveItem.parent_id = headerResult.result;
                        //var hubSendAccountMoveSend = new HubAccountMoveSend(Constants.Session);
                        //var paymentsResult = await hubSendAccountMoveSend.SendAccountMove(accountMoveItem);

                        //if (paymentsResult != null && paymentsResult.result > 0)
                        //{
                        //    if (lines != null)
                        //    {
                        //        foreach (var paymentLine in lines)
                        //        {
                        //            paymentLine.parent_id = paymentsResult.result;
                        //            var hubSendAccountMoveLineSend = new HubAccountMoveLineSend(Constants.Session);
                        //            var lineResult = await hubSendAccountMoveLineSend.SendAccountMoveLine(paymentLine);
                        //            Console.WriteLine(lineResult);
                        //        }
                        //    }
                        //}
                    }
                }

            }            

            return resultTask;
        }

        public static async Task<ApiResponseOdooRpcT<List<OdooRpcResultInt>>?> SendCreditNoteRequest(credit_note_request crediNoteRequest)
        {
            //TODO: Agregar validaciones
            HubCreditNoteRequest hubAccountMoveRefund = new HubCreditNoteRequest(Constants.Session);

            var resultTask = await hubAccountMoveRefund.Create(crediNoteRequest);
            if (resultTask != null && resultTask.result!= null && resultTask.result.Any() && resultTask.error == null)
            {
                CreditNoteRequestDb creditNoteRequestDb = new CreditNoteRequestDb(Constants.Session.odooConnection.DbNameSqlite);
                //Primera actualización de la cabecera
                crediNoteRequest.doc_status = "sended";
                crediNoteRequest.send_date = DateTime.Now;
                await creditNoteRequestDb.UpdateAsync(crediNoteRequest);

                //AccountMoveLineSendDb accountMoveLineSendDb = new AccountMoveLineSendDb();
                //var linesItems = await accountMoveLineSendDb.GetItemsByParentAsync(_account_move_send.id);

                //int sequence = 1;
                //foreach (var lineItem in linesItems)
                //{
                //    lineItem.move_id = resultTask.create_id;
                //    lineItem.sequence = sequence;

                //    var itemResponse = await hubAccountMoveRefund.SendLine(lineItem);

                //    if (itemResponse.responseCode == 200)
                //    {
                //        //Segunda actualización de la cabecera
                //        // debe realizarse en el momento que todos las lineas
                //        // hayan sido enviadas con éxito
                //        // TODO: Debe agregarse nuevo campo en la cabecera y en las líneas
                //        // CAMPO DE ESTADO
                //        //_account_move_send.doc_status = "sended";
                //        //_account_move_send.items_status = "sended";
                //        //await accountMoveSendDb.UpdateAsync(_account_move_send);
                //    }

                //    sequence++;
                //}
                //_account_move_send
            }

            return resultTask;
        }

        public static async Task<ApiResponseOdooRpcT<int>> SendMoveItem(credit_note_request _account_move_send)
        {
            //TODO: Agregar validaciones
            HubCreditNoteRequest hubAccountMoveRefund = new HubCreditNoteRequest(Constants.Session);

            var resultTask = await hubAccountMoveRefund.SendHeader(_account_move_send);
            if (resultTask.result > 0)
            {
                CreditNoteRequestDb accountMoveSendDb = new CreditNoteRequestDb(Constants.Session.odooConnection.DbNameSqlite);
                //Primera actualización de la cabecera
                _account_move_send.doc_status = "sended";
                await accountMoveSendDb.UpdateAsync(_account_move_send);

                CreditNoteRequestDetailDb accountMoveLineSendDb = new CreditNoteRequestDetailDb(Constants.Session.odooConnection.DbNameSqlite);
                var linesItems = await accountMoveLineSendDb.GetItemsByParentAsync(_account_move_send.id);

                int sequence = 1;
                foreach (var lineItem in linesItems)
                {
                    lineItem.move_id = resultTask.result;
                    lineItem.sequence = sequence;

                    var itemResponse = await hubAccountMoveRefund.SendLine(lineItem);

                    if (itemResponse.result > 0)
                    {
                        //Segunda actualización de la cabecera
                        // debe realizarse en el momento que todos las lineas
                        // hayan sido enviadas con éxito
                        // TODO: Debe agregarse nuevo campo en la cabecera y en las líneas
                        // CAMPO DE ESTADO
                        //_account_move_send.doc_status = "sended";
                        //_account_move_send.items_status = "sended";
                        //await accountMoveSendDb.UpdateAsync(_account_move_send);
                    }

                    sequence++;
                }
                //_account_move_send
            }

            return resultTask;
        }
    }
}
