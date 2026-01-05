using ApiManager;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.DMCobranzas;
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
        static public async Task<ApiResponseOdooRpcT<int>?> SendPayment(MultipleCobrosInvoice _multipleCobrosInvoice, bool autosend)
        {
            ApiResponseOdooRpcT<int> resultTask = new ApiResponseOdooRpcT<int>()
            {
                id = 0,
                result = 0,
                jsonrpc = "2.0",
                error = null
            };

            MultipleCobrosInvoiceDb multipleCobrosInvoiceDb = new MultipleCobrosInvoiceDb(Constants.Session.odooConnection.DbNameSqlite);

            if (_multipleCobrosInvoice.payment_status == Models.CobrosEstados.PENDIENTE)
            {
                string fechaActual = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10);
                //string secuencia_final = cobReciboCabDb.GenerarCodigoRecibo(_accountPaymentHeader.uid, _accountPaymentHeader.company_id, fechaActual, _accountPaymentHeader.id.ToString());

                string secuencia_final = await multipleCobrosInvoiceDb.BuildRecipeName(_multipleCobrosInvoice);

                string newGuid = Guid.NewGuid().ToString("N");
                _multipleCobrosInvoice.guid = newGuid;
                _multipleCobrosInvoice.recipe_name = secuencia_final;
                _multipleCobrosInvoice.payment_status = Models.CobrosEstados.ENVIANDO;
                await multipleCobrosInvoiceDb.UpdateAsync(_multipleCobrosInvoice);
            }

            if (DeviceInfo.Current.Platform == DevicePlatform.Android ||
                    DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                _multipleCobrosInvoice.model = DeviceInfo.Idiom.ToString() + " - " + DeviceInfo.Current.Model;
                _multipleCobrosInvoice.manufacturer = DeviceInfo.Current.Manufacturer;
                _multipleCobrosInvoice.autosend = autosend;
            }

            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                _multipleCobrosInvoice.model = DeviceInfo.Idiom.ToString() + " - " + "-";
                _multipleCobrosInvoice.manufacturer = "-";
                _multipleCobrosInvoice.autosend = autosend;
            }

            //TODO: Se coloca directamente "unknow" ya que las nuevas versiones de Android
            // no permiten obtener el número de serie de los dispositivos

            _multipleCobrosInvoice.serial = "unknown" + "-" + Constants.Session.AppVersion;

            HubAccountPayment apiProcessor = new HubAccountPayment(Constants.Session);

            AccountPaymentDb accountPaymentDb = new AccountPaymentDb(Constants.Session.odooConnection.DbNameSqlite);

            var paymentList = await accountPaymentDb.GetByParent(_multipleCobrosInvoice.id);

            AccountPaymentInvoiceLineDb accountPaymentLines = new AccountPaymentInvoiceLineDb(Constants.Session.odooConnection.DbNameSqlite);

            bool everyThingOk = false;
            foreach (var payment in paymentList)
            {
                //Si partner_bank_id requiere verificacion/sincronizacion con Odoo

                if (payment.partner_bank_id < 0)
                {
                    PartnerBankDb bankDb = new PartnerBankDb(Constants.Session.odooConnection.DbNameSqlite);
                    var partnerBankItemRed = await bankDb.GetItemAsync(x => x.id == payment.partner_bank_id);

                    res_partner_bank_send partnerBankItem = new res_partner_bank_send();

                    partnerBankItem.partner_id = partnerBankItemRed._partner_id;
                    partnerBankItem.bank_id = partnerBankItemRed._bank_id;
                    partnerBankItem.currency_id = partnerBankItemRed._currency_id;
                    partnerBankItem.acc_number = partnerBankItemRed.acc_number;
                    partnerBankItem.acc_holder_name = partnerBankItemRed.acc_holder_name;
                    partnerBankItem.type_account = partnerBankItemRed.type_account;
                    partnerBankItem.allow_out_payment = partnerBankItemRed.allow_out_payment;
                    partnerBankItem.sequence = 0;
                    partnerBankItem.use_bank_type = partnerBankItemRed.use_bank_type;
                    partnerBankItem.company_id = Array.Empty<object>();
                    partnerBankItem.bank_account_id = Array.Empty<object>();
                    partnerBankItem.sanitized_acc_number = partnerBankItemRed.acc_number;

                    partnerBankItem.active = true;

                    HubResPartnerBank hubCuentas = new HubResPartnerBank(Constants.Session);
                    var newAccount = await hubCuentas.CreateIfNotExists(partnerBankItem);

                    if (newAccount.result > 0)
                    {
                        int oldId = partnerBankItemRed.id;
                        partnerBankItemRed.id = newAccount.result;
                        //partnerBankItem.id = newAccount.data[0].id;
                        payment.partner_bank_id = partnerBankItemRed.id;
                        payment.bank_account_id = partnerBankItemRed.id;
                        //Se actualiza el ID y otros datos en la base de cuentas
                        await bankDb.UpdateAsync(partnerBankItemRed, oldId);
                        //Se actualiza el ID en la base de pagos
                        await accountPaymentDb.UpdateAsync(payment);
                    }
                    else
                    {
                        int oldId = partnerBankItemRed.id;
                        partnerBankItemRed.id = newAccount.result;
                        payment.partner_bank_id = partnerBankItemRed.id;
                        payment.bank_account_id = partnerBankItemRed.id;
                        //Se actualiza el ID y otros datos en la base de cuentas
                        await bankDb.UpdateAsync(partnerBankItemRed, oldId);
                        //Se actualiza el ID en la base de pagos
                        await accountPaymentDb.UpdateAsync(payment);
                    }
                }

                payment.recipe_name = _multipleCobrosInvoice.recipe_name;
                //Se consultan lineas de pagos de documentos
                var apl = await accountPaymentLines.GetItemsAsync(payment);
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

                    List<object> listLines = new List<object>();

                    foreach (var line in payment.lines)
                    {
                        listLines.Add(new List<object>
                        {
                            0,
                            0,
                            line
                        });
                    }

                    payment.payment_invoice_line_ids = listLines;
                }
                else
                {
                    payment.payment_invoice_line_ids = new List<object>();
                }

                //TODO: Verificar si esta linea es necesaria
                //payment.check_number = payment.number_check_customer;
                payment.bank_account_id = payment.partner_bank_id;

                resultTask = await apiProcessor.Send(payment);

                if (resultTask == null)
                {
                    //await Toast.Make("Error: Datos!").Show();
                    _multipleCobrosInvoice.payment_status = Models.CobrosEstados.ERROR;
                    _multipleCobrosInvoice.write_date = DateTime.Now;
                    await multipleCobrosInvoiceDb.UpdateAsync(_multipleCobrosInvoice);
                }
                else
                {
                    if (resultTask.result > 0 && resultTask.error == null)
                    {
                        //Confirmación del pago (botón confirmar)
                        ApiResponseOdooRpcT<int> resultTask_confirm = new ApiResponseOdooRpcT<int>()
                        {
                            id = 0,
                            result = 0,
                            jsonrpc = "2.0",
                            error = null
                        };

                        resultTask_confirm = await apiProcessor.call_button(payment, resultTask);
                        //--------------------------------------------

                        //await Toast.Make("Envío de cobro correcto").Show();
                        Debug.WriteLine("Terminado envío!");
                        _multipleCobrosInvoice.payment_status = Models.CobrosEstados.RECIBIDO;
                        _multipleCobrosInvoice.write_date = DateTime.Now;
                        await multipleCobrosInvoiceDb.UpdateAsync(_multipleCobrosInvoice);
                        everyThingOk = true;
                    }
                    else
                    {
                        //await Toast.Make("Error: " + resultTask.message).Show();
                        _multipleCobrosInvoice.payment_status = Models.CobrosEstados.ERROR;
                        _multipleCobrosInvoice.write_date = DateTime.Now;
                        await multipleCobrosInvoiceDb.UpdateAsync(_multipleCobrosInvoice);
                        //everyThingOk = false;
                    }
                }
            }

            if (everyThingOk)
            {
                //try
                //{
                HubMultipleCobrosInvoice hubMultipleCobrosInvoice = new HubMultipleCobrosInvoice(Constants.Session);
                string jsonSerialized = JsonConvert.SerializeObject(_multipleCobrosInvoice);
                MultipleCobrosInvoice objSend = JsonConvert.DeserializeObject<MultipleCobrosInvoice>(jsonSerialized);
                objSend.lines = Array.Empty<MultipleCobrosInvoiceLine>();

                AccountPaymentSend[] paymentSend = new AccountPaymentSend[] { };

                List<AccountPaymentSend> paymentSendList = new List<AccountPaymentSend>();

                foreach (var paymentItem in paymentList)
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    settings.ContractResolver = new IncludeJsonIgnoreResolver();

                    Console.WriteLine(paymentItem);

                    string jsonPaymentItem = JsonConvert.SerializeObject(paymentItem, settings);
                    AccountPaymentSend objPaymentSend = JsonConvert.DeserializeObject<AccountPaymentSend>(jsonPaymentItem, settings);

                    string JsonAccountPaymentSendLines = JsonConvert.SerializeObject(paymentItem.lines, settings);
                    objPaymentSend.lines = JsonConvert.DeserializeObject<AccountPaymentInvoiceLineSend[]>(JsonAccountPaymentSendLines);
                    paymentSendList.Add(objPaymentSend);
                }

                paymentSend = paymentSendList.ToArray();
                //Se deben enviar los pagos a parte asi mismo las lineas de facturas
                // luego de ser almacenadas deben extraerse para que se sincronicen con la informacion de la tablet
                // en caso de que los datos no existan

                var headerResult = await hubMultipleCobrosInvoice.SendHeader(objSend);
                if (headerResult != null && headerResult.result > 0)
                {
                    foreach (var payment in paymentSend)
                    {
                        var lines = payment.lines;
                        payment.lines = Array.Empty<AccountPaymentInvoiceLineSend>();
                        payment.parent_id = headerResult.result;

                        var paymentsResult = await hubMultipleCobrosInvoice.SendPayments(payment);

                        if (paymentsResult != null && paymentsResult.result > 0)
                        {
                            if (lines != null)
                            {
                                foreach (var paymentLine in lines)
                                {
                                    paymentLine.parent_payment_id = paymentsResult.result;
                                    var lineResult = await hubMultipleCobrosInvoice.SendPaymentsInvoiceLine(paymentLine);
                                }
                            }
                        }
                    }
                }

                //}
                //catch (InvalidCastException e)
                //{
                //    // Manejar el caso en que la conversión no es posible
                //    Debug.WriteLine(e);
                //}
            }

            return resultTask;
        }


        public static async Task<ApiResponseOdooRpcT<check_cn_response[]>> CheckCreditNoteOverdraf(AccountMoveSendHeader _accountMoveSendHeader)
        {
            ApiResponseOdooRpcT<check_cn_response[]> resultTask = new ApiResponseOdooRpcT<check_cn_response[]>();

            List<account_move_line_send> account_Move_Line_Sends = new List<account_move_line_send>();

            HubAccountMoveRefund hubAccountMoveRefund = new HubAccountMoveRefund(Constants.Session);
            AccountMoveSendDb accountMoveSendDb = new AccountMoveSendDb(Constants.Session.odooConnection.DbNameSqlite);

            var accountMoveSendList = await accountMoveSendDb.GetByParent(_accountMoveSendHeader.id);

            foreach (var accountMoveSend in accountMoveSendList)
            {
                AccountMoveLineSendDb accountMoveLineSendDb = new AccountMoveLineSendDb(Constants.Session.odooConnection.DbNameSqlite);
                var linesItems = await accountMoveLineSendDb.GetItemsByParentAsync(accountMoveSend.id);

                int sequence = 1;
                foreach (var lineItem in linesItems)
                {
                    account_Move_Line_Sends.Add(lineItem);
                }
            }

            var requestObject = new Models.General.Requests.ApiRequestCheckCn();
            requestObject.credit_note_lines = account_Move_Line_Sends.ToArray();

            resultTask = await hubAccountMoveRefund.check_credit_note_overdraft(requestObject);

            return resultTask;
        }

        public static async Task<ApiResponseOdooRpcT<int>> SendRequestCreditNote(AccountMoveSendHeader _accountMoveSendHeader)
        {
            ApiResponseOdooRpcT<int> resultTask = new ApiResponseOdooRpcT<int>()
            {
                id = 0,
                jsonrpc = "2.0",
                result = 0,
                error = null
            };

            AccountMoveSendHeaderDb accountMoveSendHeaderDb = new AccountMoveSendHeaderDb(Constants.Session.odooConnection.DbNameSqlite);

            if (_accountMoveSendHeader.request_status == Models.MoveStatus.PENDIENTE)
            {
                string fechaActual = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10);
                //var secuencia = await cobReciboCabDb.obtenerSecuenciaRecibo(_accountPaymentHeader.company_id, _accountPaymentHeader.uid, fechaActual);
                //string request_name = accountMoveSendHeaderDb.BuildName(_accountMoveSendHeader.uid, _accountMoveSendHeader.company_id, fechaActual, _accountMoveSendHeader.id.ToString());
                string request_name = await accountMoveSendHeaderDb.BuildRequestName(_accountMoveSendHeader);

                _accountMoveSendHeader.request_name = request_name;
                _accountMoveSendHeader.request_status = Models.MoveStatus.ENVIANDO;
                await accountMoveSendHeaderDb.UpdateAsync(_accountMoveSendHeader);
            }

            if (DeviceInfo.Current.Platform == DevicePlatform.Android ||
                    DeviceInfo.Current.Platform == DevicePlatform.iOS)
            {
                _accountMoveSendHeader.model = DeviceInfo.Idiom.ToString() + " - " + DeviceInfo.Current.Model;
                _accountMoveSendHeader.manufacturer = DeviceInfo.Current.Manufacturer;
                _accountMoveSendHeader.autosend = "N";
            }

            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                _accountMoveSendHeader.model = DeviceInfo.Idiom.ToString() + " - " + "-";
                _accountMoveSendHeader.manufacturer = "-";
                _accountMoveSendHeader.autosend = "N";
            }

            //TODO: Se coloca directamente "unknow" ya que las nuevas versiones de Android
            // no permiten obtener el número de serie de los dispositivos

            _accountMoveSendHeader.serialNumber = "unknown" + "-" + Constants.Session.AppVersion;

            AccountMoveSendDb accountMoveSendDb = new AccountMoveSendDb(Constants.Session.odooConnection.DbNameSqlite);
            AccountMoveLineSendDb accountMoveLineSendDb = new AccountMoveLineSendDb(Constants.Session.odooConnection.DbNameSqlite);

            var accountMoveSendList = await accountMoveSendDb.GetByParent(_accountMoveSendHeader.id);

            foreach (var accountMoveSend in accountMoveSendList)
            {
                accountMoveSend.request_name = _accountMoveSendHeader.request_name;

                //foreach(var lineItem in SendObject.lines)
                //{
                //SendObject.invoice_line_ids
                //}

                //accountMoveSend.invoice_line_ids.Add
                
                var linesItems = await accountMoveLineSendDb.GetItemsByParentAsync(accountMoveSend.id);
                if (linesItems.Count > 0)
                {
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

                    List<object> listLines = new List<object>();
                    
                    int sequence = 1;

                    foreach (var line in linesItems)
                    {
                        line.move_id = accountMoveSend.id;
                        line.sequence = sequence;

                        listLines.Add(new List<object>
                        {
                            0,
                            0,
                            line
                        });
                        sequence++;
                    }

                    accountMoveSend.invoice_line_ids = listLines;
                    accountMoveSend.lines = linesItems.ToArray();
                }
                else
                {
                    accountMoveSend.invoice_line_ids = new List<object>();
                }

                resultTask = await SendMoveItem_Mode_Dataset(accountMoveSend);

                if (resultTask == null)
                {
                    //await Toast.Make("Error: Datos!").Show();
                    _accountMoveSendHeader.request_status = Models.MoveStatus.ERROR;
                    _accountMoveSendHeader.modification_datetime = DateTime.Now;
                    await accountMoveSendHeaderDb.UpdateAsync(_accountMoveSendHeader);
                }
                else
                {
                    if (resultTask.result>0 && resultTask.error == null)
                    {
                        //await Toast.Make("Envío de solicitud correcto").Show();
                        _accountMoveSendHeader.request_status = Models.MoveStatus.RECIBIDO;
                        _accountMoveSendHeader.modification_datetime = DateTime.Now;
                        await accountMoveSendHeaderDb.UpdateAsync(_accountMoveSendHeader);                        
                    }
                    else
                    {
                        //await Toast.Make("Error: " + resultTask.message).Show();
                        _accountMoveSendHeader.request_status = Models.MoveStatus.ERROR;
                        _accountMoveSendHeader.modification_datetime = DateTime.Now;
                        await accountMoveSendHeaderDb.UpdateAsync(_accountMoveSendHeader);
                    }
                }
            }

            bool everyThingOk = true;
            if (everyThingOk)
            {
                //List<AccountPaymentSend> accountMoveSendList = new List<AccountPaymentSend>();

                //AccountPaymentSend[] paymentSend = new AccountPaymentSend[] { };


                //HubAccountPaymentHeader accountPaymentHeader = new HubAccountPaymentHeader(App.Session);
                string jsonSerialized = JsonConvert.SerializeObject(_accountMoveSendHeader);
                AccountMoveSendHeader objSend = JsonConvert.DeserializeObject<AccountMoveSendHeader>(jsonSerialized);
                
                List<account_move_send> accountMoveSendListNew = new List<account_move_send>();

                foreach (var accountMoveItem in accountMoveSendList)
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    settings.ContractResolver = new IncludeJsonIgnoreResolver();

                    Console.WriteLine(accountMoveItem);

                    string jsonPaymentItem = JsonConvert.SerializeObject(accountMoveItem, settings);
                    account_move_send objPaymentSend = JsonConvert.DeserializeObject<account_move_send>(jsonPaymentItem, settings);

                    string JsonAccountPaymentSendLines = JsonConvert.SerializeObject(accountMoveItem.lines, settings);
                    objPaymentSend.lines = JsonConvert.DeserializeObject<account_move_line_send[]>(JsonAccountPaymentSendLines);
                    accountMoveSendListNew.Add(objPaymentSend);
                }

                //Se guarda cabecera de solicitudes en Odoo solo cuando se han enviado correctamente las anteriores
                HubAccountMoveSendHeader hubAccountMoveHeader = new HubAccountMoveSendHeader(Constants.Session);
                //string jsonSerialized = JsonConvert.SerializeObject(_accountMoveSendHeader);
                //Acco objSend = JsonConvert.DeserializeObject<AccountPaymentDaily>(jsonSerialized);

                ApiResponseOdooRpcT<int> headerResult = await hubAccountMoveHeader.SendHeader(objSend);
                //var headerResult = await hubAccountMoveHeader.SendHeader(objSend);
                Debug.WriteLine(headerResult);

                if (headerResult != null && headerResult.result > 0)
                {
                    foreach (var accountMoveItem in accountMoveSendListNew.ToArray())
                    {
                        var lines = accountMoveItem.lines;
                       
                        accountMoveItem.lines = Array.Empty<account_move_line_send>();
                        accountMoveItem.invoice_line_ids = new  List<object>();
                        accountMoveItem.parent_id = headerResult.result;
                        var hubSendAccountMoveSend = new HubAccountMoveSend(Constants.Session);
                        var paymentsResult = await hubSendAccountMoveSend.SendAccountMove(accountMoveItem);

                        if (paymentsResult != null && paymentsResult.result > 0)
                        {
                            if (lines != null)
                            {
                                foreach (var paymentLine in lines)
                                {
                                    paymentLine.parent_move_id = paymentsResult.result;
                                    var hubSendAccountMoveLineSend = new HubAccountMoveLineSend(Constants.Session);
                                    var lineResult = await hubSendAccountMoveLineSend.SendAccountMoveLine(paymentLine);
                                    Console.WriteLine(lineResult);
                                }
                            }
                        }
                    }
                }

            }            

            return resultTask;
        }

        public static async Task<ApiResponseOdooRpcT<int>?> SendMoveItem_Mode_Dataset(account_move_send _account_move_send)
        {
            //TODO: Agregar validaciones
            HubAccountMoveRefund hubAccountMoveRefund = new HubAccountMoveRefund(Constants.Session);

            var resultTask = await hubAccountMoveRefund.SendHeader_Mode_Dataset(_account_move_send);
            if (resultTask.result > 0 && resultTask.error == null)
            {
                AccountMoveSendDb accountMoveSendDb = new AccountMoveSendDb(Constants.Session.odooConnection.DbNameSqlite);
                //Primera actualización de la cabecera
                _account_move_send.doc_status = "sended";
                _account_move_send.send_date = DateTime.Now;
                await accountMoveSendDb.UpdateAsync(_account_move_send);

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

        public static async Task<ApiResponseOdooRpcT<int>> SendMoveItem(account_move_send _account_move_send)
        {
            //TODO: Agregar validaciones
            HubAccountMoveRefund hubAccountMoveRefund = new HubAccountMoveRefund(Constants.Session);

            var resultTask = await hubAccountMoveRefund.SendHeader(_account_move_send);
            if (resultTask.result > 0)
            {
                AccountMoveSendDb accountMoveSendDb = new AccountMoveSendDb(Constants.Session.odooConnection.DbNameSqlite);
                //Primera actualización de la cabecera
                _account_move_send.doc_status = "sended";
                await accountMoveSendDb.UpdateAsync(_account_move_send);

                AccountMoveLineSendDb accountMoveLineSendDb = new AccountMoveLineSendDb(Constants.Session.odooConnection.DbNameSqlite);
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
