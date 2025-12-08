using DMCobranzas.Models;
using DMCobranzas.Models.Specials;
using DMCobranzas.Settings.helpers;
using DMSA.Models.Odoo.DMApps;
using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using Fluid;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static DMCobranzas.Services.Templates.Processor;

namespace DMCobranzas.Services.Templates
{
    public class Processor
    {
        public Processor()
        { 

        }

        public async Task<string> Template_ItemsGroup(ItemsGroup _itemsGroup)
        {
            string TicketHeaderString = "";

            string TicketString = "";
            int _count = 0;
            string strResult = string.Empty;

            List<AccountPayment> itemsCobReciboDet = new List<AccountPayment>();

            foreach (var _cobReciboCab in _itemsGroup)
            {
                _count++;

                //_cobReciboCab = ncobReciboCab;

                res_company[] Empresas = null;

                if (App.Session.CurrentUserFront.empresas != null)
                {
                    Empresas = App.Session.CurrentUserFront.empresas;
                    var empresaI = Empresas.ToList().Where(i => i.id == _cobReciboCab.company_id).FirstOrDefault();

                    if (empresaI != null)
                    {
                        if (_count == 1)
                        {
                            TicketHeaderString += $"{empresaI.name}" + Environment.NewLine;

                            TicketHeaderString += "Resumen Cobranzas" + Environment.NewLine;
                            AccountPaymentDailyDb cobCierreDb = new AccountPaymentDailyDb(App.Session.odooConnection.DbNameSqlite);
                            var itemsCierre = await cobCierreDb.GetItemsDateCutAsync(_cobReciboCab.company_id, _cobReciboCab.create_datetime);
                            
                            if(itemsCierre!= null)
                            {
                                TicketHeaderString += "Día: (" + itemsCierre.closing_id.Substring(0,10) + ")" + Environment.NewLine;
                                //Cabecera Impresion
                                TicketHeaderString += "================================" + Environment.NewLine;
                                TicketHeaderString += "Refer. Cierre: " + itemsCierre.payment_reference + Environment.NewLine;
                                TicketHeaderString += "Cant. Recibos: " + itemsCierre.closing_amount + Environment.NewLine;
                                TicketHeaderString += "Cobrador: " + App.Session.CurrentUser.nombres + Environment.NewLine;
                                TicketHeaderString += "================================" + Environment.NewLine;
                            }
                        }

                        //if (_cobReciboCab.account_payment_json != null)
                        //{
                        //    AccountPayment[] cobReciboDet = JsonConvert.DeserializeObject<List<AccountPayment>>(_cobReciboCab.account_payment_json).ToArray();
                        //    itemsCobReciboDet.AddRange(cobReciboDet);
                        //}

                        double valor = 0;
                        //valor = _cobReciboCab.VALORPAGO;
                        double totalAplicado = 0;
                        double saldoDocumento = 0;
                    }
                }
            }

            //Detalles de pagos
            //var resultado = itemsCobReciboDet
            //                    .GroupBy(detalle => detalle.descformapago)
            //                    .Select(grupo => new
            //                    {
            //                        FormaPago = grupo.Key,
            //                        SumaValor = grupo.Sum(detalle => detalle.valor),
            //                        Contador = grupo.Count()
            //                    });

            //// Mostrar los resultados
            //foreach (var item in resultado)
            //{
            //    Console.WriteLine($"Forma de Pago: {item.FormaPago}, Suma de Valores: {item.SumaValor}, Cantidad: {item.Contador}");
            //    TicketString += $"==== {item.FormaPago} ({item.Contador})====" + Environment.NewLine;
            //    TicketString += $"TOTAL {item.FormaPago,-15}" + $"$ {item.SumaValor}" + Environment.NewLine;
            //}

            strResult += TicketHeaderString + TicketString;

            strResult += Environment.NewLine;
            strResult += Environment.NewLine;
            strResult += Environment.NewLine;
            strResult += "______________________________" + Environment.NewLine;
            strResult += "        -Firma Vendedor-" + Environment.NewLine;
            strResult += Environment.NewLine;
            strResult += Environment.NewLine;

            return strResult;
        }
        

        public async Task<string> Template_ItemsGroup_V2(ItemsGroup _itemsGroup)
        {
            List<AccountPaymentHeader> _accountPaymentHeaders = new List<AccountPaymentHeader>();
            List<AccountPayment> _accountPayments = new List<AccountPayment>();

            string result = "";
            foreach (var _itemGroup in _itemsGroup)
            {
                _accountPaymentHeaders.Add(_itemGroup);
                AccountPaymentDailyDb _accountPaymentDailyDb = new AccountPaymentDailyDb(App.Session.odooConnection.DbNameSqlite);
                var _accountPaymentDaily = await _accountPaymentDailyDb.GetItemsDateCutAsync(_itemGroup.company_id, _itemGroup.create_datetime);

                res_company[] Empresas = null;
                Empresas = App.Session.CurrentUserFront.empresas;
                var empresaI = Empresas.ToList().Where(i => i.id == _itemGroup.company_id).FirstOrDefault();

                AccountPaymentDb accountPaymentDb = new AccountPaymentDb(App.Session.odooConnection.DbNameSqlite);
                //DateTime dateTime = DateTime.Parse(_itemGroup.create_datetime);
                var _accountPaymentGroup = await accountPaymentDb.GetByParent(_itemGroup.id);

                _accountPayments.AddRange(_accountPaymentGroup);

                /*Agrupa para los totales*/
                var _journalSummary = _accountPaymentGroup.GroupBy(p => p.journal_name)
                                     .Select(g => new JournalSummary
                                     {
                                         JournalName = g.Key,
                                         TotalRecords = g.Count(),
                                         TotalAmount = g.Sum(p => p.amount)
                                     })
                                     .ToList();


                string resourceName = "DMCobranzas.Resources.Raw.liq_ticket_closing.txt";

                var assembly = Assembly.GetExecutingAssembly();

                Stream stream = assembly.GetManifestResourceStream(resourceName);
                StreamReader reader = new StreamReader(stream);
                string strTemplate = reader.ReadToEnd();

                var parser = new FluidParser();

                var model = new
                {
                    _accountPaymentDaily = _accountPaymentDaily,
                    _company = empresaI,
                    _accountPaymentHeaders = _accountPaymentHeaders,
                    _accountPayments = _accountPayments,
                    _journalSummary = _journalSummary
                };

                var options = new TemplateOptions();
                options.MemberAccessStrategy.Register<AccountPaymentDaily>();
                options.MemberAccessStrategy.Register<AccountPaymentHeader>();
                options.MemberAccessStrategy.Register<AccountPayment>();
                options.MemberAccessStrategy.Register<res_company>();                
                options.MemberAccessStrategy.Register<user_access>();
                options.MemberAccessStrategy.Register<JournalSummary>();

                if (parser.TryParse(strTemplate, out var templateF, out var error))
                {
                    var context = new TemplateContext(model, options);

                    //Debug.WriteLine(templateF.Render(context));

                    result = templateF.Render(context);
                }
                else
                {
                    Debug.WriteLine($"Error: {error}");
                }
            }

            return result;
        }

        public async Task<string> Template_AccountPaymentHeader_v2(AccountPaymentHeader _accountPaymentHeader)
        {
            List<AccountPayment> ls_accountPayments = new List<AccountPayment>();
            res_partner _res_partner = null;
            user_access _user_Access = null;

            if (_accountPaymentHeader != null)
            {
                ResPartnerDb resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
                _res_partner = await resPartnerDb.GetItemsAsync(_accountPaymentHeader.company_id, _accountPaymentHeader.partner_id);

                UserAccessDb userAccessDb = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
                _user_Access = await userAccessDb.GetItemAsync(_accountPaymentHeader.uid);

                AccountPaymentDb accountPaymentDb = new AccountPaymentDb(App.Session.odooConnection.DbNameSqlite);
                ls_accountPayments = await accountPaymentDb.GetByParent(_accountPaymentHeader.id);
             
                foreach (var accountPayment in ls_accountPayments)
                {
                    AccountPaymentInvoiceLineDb accountPaymentLines = new AccountPaymentInvoiceLineDb(App.Session.odooConnection.DbNameSqlite);
                    var apl = await accountPaymentLines.GetItemsAsync(accountPayment);

                    if (apl.Count() > 0)
                    {
                        accountPayment.lines = apl.ToArray();
                    }
                }
            }

            res_company[] Empresas = null;
            Empresas = App.Session.CurrentUserFront.empresas;
            var empresaI = Empresas.ToList().Where(i => i.id == _accountPaymentHeader.company_id).FirstOrDefault();

            //string resourceName = "DMCobranzas.Resources.Raw.liq_ticket_small.txt";
            string resourceName = "DMCobranzas.Resources.Raw.liq_ticket_small.liquid";
            
            var assembly = Assembly.GetExecutingAssembly();
            
            Stream stream = assembly.GetManifestResourceStream(resourceName);
            StreamReader reader = new StreamReader(stream);
            string strTemplate = reader.ReadToEnd();

            string result = "";

            var parser = new FluidParser();

            var model = new
            {
                _accountPaymentHeader = _accountPaymentHeader,
                _accountPayments = ls_accountPayments,
                _company = empresaI,
                _res_partner = _res_partner,
                _user_access = _user_Access
            };

            var options = new TemplateOptions();
            options.MemberAccessStrategy.Register<AccountPaymentHeader>();
            options.MemberAccessStrategy.Register<AccountPayment>();
            options.MemberAccessStrategy.Register<AccountPaymentInvoiceLine>();
            options.MemberAccessStrategy.Register<res_company>();
            options.MemberAccessStrategy.Register<res_partner>();
            options.MemberAccessStrategy.Register<user_access>();

            if (parser.TryParse(strTemplate, out var templateF, out var error))
            {
                var context = new TemplateContext(model,options);
                
                //Debug.WriteLine(templateF.Render(context));
                
                result = templateF.Render(context);
            }
            else
            {
                Debug.WriteLine($"Error: {error}");
            }

            return result;
        }

        //public List<object> ToListObject<T>(List<T> objectList)
        //{            
        //    var resultItems = new List<object>();
        //    foreach (var _itemList in objectList)
        //    {
        //        var _dictObject = ConvertToDictionary(_itemList);
        //        resultItems.Add(_dictObject);
        //    }

        //    return resultItems;
        //}

        //public static Dictionary<string, object> ConvertToDictionary(object obj)
        //{
        //    Dictionary<string, object> dictionary = new Dictionary<string, object>();

        //    if (obj != null)
        //    {
        //        Type type = obj.GetType();
        //        PropertyInfo[] properties = type.GetProperties();

        //        foreach (PropertyInfo property in properties)
        //        {
        //            string propertyName = property.Name;
        //            object propertyValue = property.GetValue(obj, null);
        //            dictionary.Add(propertyName, propertyValue);
        //        }
        //    }

        //    return dictionary;
        //}

        public async Task<string> Template_CobReciboCab(AccountPaymentHeader _accountPaymentHeader)
        {
            //_cobReciboCab.TOTALDEUDAACTUAL = _cobReciboCab.TOTALDEUDAACTUAL.Replace(".",",");
            List<AccountPayment> ls_accountPayments = new List<AccountPayment>();

            if (_accountPaymentHeader != null)
            {
                //dataItems = new CobReciboDet[0];
                //var ls_dataItems = JsonConvert.DeserializeObject<List<AccountPayment>>(cobReciboCab.DETALLESPAGO);
                AccountPaymentDb accountPaymentDb = new AccountPaymentDb(App.Session.odooConnection.DbNameSqlite);
                ls_accountPayments = await accountPaymentDb.GetByParent(_accountPaymentHeader.id);
                //accountPayments = ls_accountPayments.ToArray();

                AccountPaymentInvoiceLineDb accountPaymentLines = new AccountPaymentInvoiceLineDb(App.Session.odooConnection.DbNameSqlite);

                foreach (var accountPayment in ls_accountPayments)
                {
                    var apl = await accountPaymentLines.GetItemsAsync(accountPayment);

                    if (apl.Count() > 0)
                    {
                        accountPayment.lines = apl.ToArray();
                    }
                }
            }

            string strResult = string.Empty;
            //_cobReciboCab = ncobReciboCab;

            res_company[] Empresas = null;

            if (App.Session.CurrentUserFront.empresas != null)
            {
                Empresas = App.Session.CurrentUserFront.empresas;
                var empresaI = Empresas.ToList().Where(i => i.id == _accountPaymentHeader.company_id).FirstOrDefault();

                if (empresaI != null)
                {
                    string TicketString = "";
                    TicketString += $"{empresaI.name}" + Environment.NewLine;
                    TicketString += $"RECIBO # {_accountPaymentHeader.recipe_name}" + Environment.NewLine;
                    TicketString += $"CLIENTE: ({_accountPaymentHeader.partner_id}) {_accountPaymentHeader.partner_name}" + Environment.NewLine;

                    if (_accountPaymentHeader.payment_status == DMSA.Models.CobrosEstados.PENDIENTE || _accountPaymentHeader.payment_status == DMSA.Models.CobrosEstados.ENVIANDO)
                        TicketString += "Estado: NO PROCESADO" + Environment.NewLine;
                    else if (_accountPaymentHeader.payment_status == DMSA.Models.CobrosEstados.RECIBIDO)
                        TicketString += "Estado: PROCESADO" + Environment.NewLine;
                    else
                        TicketString += "Estado: " + _accountPaymentHeader.payment_status + Environment.NewLine;

                    //TicketString += $"Estado: {_cobReciboCab.CODESTADO}" + Environment.NewLine;
                    TicketString += $"============F.PAGO===========" + Environment.NewLine;

                    if (ls_accountPayments != null && ls_accountPayments.Count()> 0)
                    {
                        //AccountPayment[] cobReciboDet = JsonConvert.DeserializeObject<List<AccountPayment>>(_cobReciboCab.DETALLESPAGO).ToArray(); //new CobReciboDet[5];
                                                                                                                                                   //Detalles de forma de pago
                        foreach (var itemDet in ls_accountPayments)
                        {
                            //itemDet.valor = ParseTool.StringToDouble(itemDet.valor).ToString("N2", App.Session.ApplicationCultureInfo);
                            //itemDet.amount = itemDet.amount;

                            string amount = itemDet.amount.ToString("N2", App.Session.ApplicationCultureInfo);

                            //itemDet.descformapago.PadRight(15);
                            TicketString += $"{itemDet.journal_name}" + $"         $ {amount}" + Environment.NewLine;
                            //var re = "BANCO";
                            //switch (itemDet.idformapago)
                            //{
                            //    case "EF":
                            //        {
                            //            //TicketString += $" ";
                            //        }
                            //        break;
                            //    case "CH":
                            //        {
                            //            TicketString += $" F.Cobro: {itemDet.fcobrocheque,+28}" + Environment.NewLine;
                            //            TicketString += $" Bco. {itemDet.descbanco.ToUpper().Replace(re, "").Trim(),+15}  Ch# {itemDet.numero_cheque,-10}" + Environment.NewLine;
                            //        }
                            //        break;
                            //    case "DP":
                            //    case "TRANBAN":
                            //        {
                            //            TicketString += $" Cta. {itemDet.descctacia.ToUpper().Replace(re, "").Trim(),+15}  Dp# {itemDet.numerodeposito,-10}" + Environment.NewLine;
                            //        }
                            //        break;
                            //    case "TJ":
                            //        {
                            //            TicketString += $" Tj. {itemDet.desctarjeta,+15}  t# {itemDet.numero_lote}, -10)" + Environment.NewLine;
                            //        }
                            //        break;
                            //    case "NC":
                            //        {

                            //        }
                            //        break;
                            //    case "RF":
                            //        {

                            //        }
                            //        break;
                            //}

                            TicketString += $"===========DOCUMENTOS=========" + Environment.NewLine;
                            //Lineas de pago (FACTURAS)
                            if (itemDet.lines != null)
                            {
                                foreach (var itemLine in itemDet.lines)
                                {
                                    string reconcile_amount = itemLine.reconcile_amount.ToString();
                                    TicketString += $"{itemLine.invoice_line_id_name}" + $"         $ {reconcile_amount}" + Environment.NewLine;
                                }
                            }
                        }
                    }

                    //_cobReciboCab.VALORPAGO = ParseTool.StringToDouble(_cobReciboCab.VALORPAGO).ToString("N2", App.Session.ApplicationCultureInfo);
                    //_accountPaymentHeader.VALORPAGO = _accountPaymentHeader.VALORPAGO;

                    TicketString += $"________________" + Environment.NewLine;
                    TicketString += $"TOTAL F/P:     $ " + _accountPaymentHeader.payment_amount + Environment.NewLine;
                    TicketString += $"" + Environment.NewLine;

                    decimal valor = 0;
                    //valor = _cobReciboCab.VALORPAGO;
                    decimal totalAplicado = 0;
                    decimal saldoDocumento = 0;

                    //////if (_accountPaymentHeader.account_payment_invoice_json != null)
                    //////{
                    //////    detallesDocumentos[] _detallesDocumentos = JsonConvert.DeserializeObject<List<detallesDocumentos>>(_accountPaymentHeader.account_payment_invoice_json).ToArray();
                    //////    //Detalles de documentos
                    //////    foreach (var itemDet in _detallesDocumentos)
                    //////    {
                    //////        //itemDet.VALORSALDO = itemDet.VALORSALDO.Replace(".",",");
                    //////        //itemDet.VALORXAPLICAR = itemDet.VALORXAPLICAR.Replace(".", ",");
                    //////        //itemDet.VALORCUOTA = itemDet.VALORCUOTA.Replace(".", ",");

                    //////        //itemDet.VALORSALDO = ParseTool.StringToDouble(itemDet.VALORSALDO).ToString("N2", App.Session.ApplicationCultureInfo);
                    //////        //itemDet.VALORXAPLICAR = ParseTool.StringToDouble(itemDet.VALORXAPLICAR).ToString("N2", App.Session.ApplicationCultureInfo);
                    //////        //itemDet.VALORCUOTA = ParseTool.StringToDouble(itemDet.VALORCUOTA).ToString("N2", App.Session.ApplicationCultureInfo);

                    //////        itemDet.VALORSALDO = itemDet.VALORSALDO;
                    //////        itemDet.VALORXAPLICAR = itemDet.VALORXAPLICAR;
                    //////        itemDet.VALORCUOTA = itemDet.VALORCUOTA;

                    //////        //valor = ParseTool.StringToDouble( itemDet.VALORXAPLICAR );
                    //////        //saldoDocumento = ParseTool.StringToDouble( itemDet.VALORSALDO ); //APLICAR ACUMULADOR +=


                    //////        valor = itemDet.VALORXAPLICAR;
                    //////        saldoDocumento = itemDet.VALORSALDO; //APLICAR ACUMULADOR +=

                    //////        //Obtengo saldo Real del Documento (Con la Aplicación)   
                    //////        saldoDocumento = saldoDocumento - valor;

                    //////        TicketString += $"===========DOCUMENTOS=========" + Environment.NewLine;
                    //////        TicketString += $"{itemDet.NUMDOCUMENTO}" + Environment.NewLine;
                    //////        //TicketString += $"FAC # {itemDet.REFERENCIA}" + Environment.NewLine;
                    //////        //TicketString += $"Abono:     $ {itemDet.REFERENCIA}" + Environment.NewLine;

                    //////        //textoImprimir = textoImprimir + " " + this.pad((saldoDocumento <= parseFloat("0").toFixed(2) ? "Canc.: $" : "Abono: $") + this.formatNumber(valor, 2), 31, "L") + "\n";

                    //////        if (saldoDocumento <= 0)
                    //////        {
                    //////            TicketString += " Canc.: $";
                    //////        }
                    //////        else
                    //////        {
                    //////            TicketString += "Abono:  $";
                    //////        }

                    //////        TicketString += valor.ToString("N2", App.Session.ApplicationCultureInfo) + Environment.NewLine;

                    //////        TicketString += $"Saldo:     $ " + saldoDocumento.ToString("N2", App.Session.ApplicationCultureInfo) + Environment.NewLine;
                    //////        //totalAplicado += ParseTool.StringToDouble(itemDet.VALORXAPLICAR);
                    //////        totalAplicado += itemDet.VALORXAPLICAR;
                    //////    }
                    //////}

                    TicketString += $"TOTAL CANC:     $ " + valor.ToString("N2", App.Session.ApplicationCultureInfo) + Environment.NewLine;

                    decimal diferenciaValor = 0;
                    //_detallesDocumentos
                    //totalAplicado = totalAplicado + (+detallesDocumentos[i].VALORXAPLICAR);
                    //diferenciaValor = parseFloat("" + entidadCobro.TOTALDEUDAACTUAL) - parseFloat("" + totalAplicado);

                    //if(_cobReciboCab.TOTALDEUDAACTUAL == "")
                    //{
                    //    _cobReciboCab.TOTALDEUDAACTUAL = "0";
                    //}

                    //diferenciaValor = ParseTool.StringToDouble(_cobReciboCab.TOTALDEUDAACTUAL) - totalAplicado;

                    diferenciaValor = _accountPaymentHeader.total_due - totalAplicado;

                    TicketString += $"================================" + Environment.NewLine;
                    TicketString += $"TOTAL FACT. Pendientes: $ " + diferenciaValor.ToString("N2", App.Session.ApplicationCultureInfo) + Environment.NewLine;
                    TicketString += $"================================" + Environment.NewLine;
                    TicketString += $"Email: {_accountPaymentHeader.EMAILCLIENTE}" + Environment.NewLine;
                    TicketString += $"Vnd: {_accountPaymentHeader.NOMBREUSUARIO}" + Environment.NewLine;
                    TicketString += $"Fecha: {_accountPaymentHeader.create_datetime}" + Environment.NewLine;
                    TicketString += Environment.NewLine;                    
                    TicketString += Environment.NewLine;
                    TicketString += "________________________________" + Environment.NewLine;
                    TicketString += "             -Firma Cliente-" + Environment.NewLine;
                    TicketString += Environment.NewLine;
                    TicketString += Environment.NewLine;

                    strResult = TicketString;
                }
            }

            return strResult;
        }

        [Obsolete]
        public async Task<string> Template_AccountMoveSendNC(account_move_send _account_move_send)
        {
            string strResult = string.Empty;
            //_cobReciboCab = ncobReciboCab;

            res_company[] Empresas = null;

            if (App.Session.CurrentUserFront.empresas != null)
            {
                Empresas = App.Session.CurrentUserFront.empresas;
                var empresaI = Empresas.ToList().Where(i => i.id == _account_move_send.company_id).FirstOrDefault();

                ResPartnerDb resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
                var resPartner = await resPartnerDb.GetItemsAsync(1,1);

                AccountMoveLineSendDb accountMoveLineSendDb = new AccountMoveLineSendDb(App.Session.odooConnection.DbNameSqlite);
                var movesLine = await accountMoveLineSendDb.GetItemsByParentAsync(_account_move_send.id);

                if (empresaI != null)
                {
                    string TicketString = "";
                    TicketString += $"{empresaI.name}" + Environment.NewLine;
                    TicketString += $"=== SOLICITUD DE NOTA DE CREDITO ===\n" + Environment.NewLine;
                    TicketString += $"CLIENTE: {resPartner.name}" + Environment.NewLine;
                    TicketString += $"Doc.Ref.: {_account_move_send._ref}\n";
                    TicketString += ".______________________________.\n";
                    TicketString += "| ESTE TICKET ES INFORMATIVO.**|\n";
                    TicketString += "| LA SOLICITUD SERA EVALUADA   |\n";
                    TicketString += "| PARA SU APROBACION.**********|\n";
                    TicketString += ".______________________________.\n\n";                    
                    TicketString += "=========== DETALLE ===========\n";

                    if (movesLine != null && movesLine.Count > 0)
                    {
                        TicketString += $"ARTICULO                                      CANTIDAD" + Environment.NewLine;
                        TicketString += $"--------------------------------------------------------------" + Environment.NewLine;
                        foreach (var itemDet in movesLine)
                        {
                            //itemDet.descformapago.PadRight(15);
                            //TicketString += $"{itemDet.NUMDOCUMENTO,+15}" + Environment.NewLine;
                            TicketString += $"{itemDet.name}                {itemDet.quantity}" + Environment.NewLine;
                            //TicketString += $"_____________________________" + Environment.NewLine;
                        }
                    }

                    TicketString += $"\n\n================================\n";                    
                    //TicketString += "Cod. Vnd: " + _account_move_send.uid + "\n";
                    TicketString += $"Fecha:     {_account_move_send.create_date.ToShortDateString()}" + Environment.NewLine;
                    TicketString += Environment.NewLine;
                    TicketString += Environment.NewLine;                    
                    TicketString += "______________________________" + Environment.NewLine;
                    TicketString += "-Firma cliente-" + Environment.NewLine;
                    TicketString += Environment.NewLine;
                    TicketString += Environment.NewLine;

                    strResult = TicketString;
                }
            }

            return strResult;
        }

        public async Task<string> Template_AccountMoveSendNC_V2(account_move_send _account_move_send)
        {
            List<account_move_line_send> ls_accountMoveSendLines = new List<account_move_line_send>();

            if (_account_move_send != null)
            {
                AccountMoveLineSendDb _accountMoveSendLineDb = new AccountMoveLineSendDb(App.Session.odooConnection.DbNameSqlite);
                ls_accountMoveSendLines = await _accountMoveSendLineDb.GetItemsByParentAsync(_account_move_send.id);
            }

            res_company[] Empresas = null;
            Empresas = App.Session.CurrentUserFront.empresas;
            var empresaI = Empresas.ToList().Where(i => i.id == _account_move_send.company_id).FirstOrDefault();

            string resourceName = "DMCobranzas.Resources.Raw.liq_refund_req_ncr.txt";

            var assembly = Assembly.GetExecutingAssembly();

            Stream stream = assembly.GetManifestResourceStream(resourceName);
            StreamReader reader = new StreamReader(stream);
            string strTemplate = reader.ReadToEnd();

            string result = "";

            var parser = new FluidParser();

            var model = new
            {
                _account_move_send = _account_move_send,
                _accountMoveSendLines = ls_accountMoveSendLines,
                _company = empresaI
            };

            var options = new TemplateOptions();
            options.MemberAccessStrategy.Register<account_move_send>();
            options.MemberAccessStrategy.Register<account_move_line_send>();            
            options.MemberAccessStrategy.Register<res_company>();

            if (parser.TryParse(strTemplate, out var templateF, out var error))
            {
                var context = new TemplateContext(model, options);
                result = templateF.Render(context);
            }
            else
            {
                Debug.WriteLine($"Error: {error}");
            }

            return result;
        }

        public async Task<string> Template_AccountMoveSendNC_V3(AccountMoveSendHeader _accountMoveSendHeader)
        {
            List<account_move_send> ls_accountMovesSend = new List<account_move_send>();
            
            if (_accountMoveSendHeader != null)
            {
                AccountMoveSendDb _accountMoveSendDb = new AccountMoveSendDb(App.Session.odooConnection.DbNameSqlite);
                ls_accountMovesSend = await _accountMoveSendDb.GetByParent(_accountMoveSendHeader.id);

                foreach(var  accountMoveSend in ls_accountMovesSend)
                {
                    AccountMoveLineSendDb _accountMoveSendLineDb = new AccountMoveLineSendDb(App.Session.odooConnection.DbNameSqlite);
                    var lineItem = await _accountMoveSendLineDb.GetItemsByParentAsync(accountMoveSend.id);

                    accountMoveSend.lines = lineItem.ToArray();
                }
            }

            res_company[] Empresas = null;
            Empresas = App.Session.CurrentUserFront.empresas;
            var empresaI = Empresas.ToList().Where(i => i.id == _accountMoveSendHeader.company_id).FirstOrDefault();

            string resourceName = "DMCobranzas.Resources.Raw.liq_req_movSendHead_ncr.txt";

            var assembly = Assembly.GetExecutingAssembly();

            Stream stream = assembly.GetManifestResourceStream(resourceName);
            StreamReader reader = new StreamReader(stream);
            string strTemplate = reader.ReadToEnd();

            string result = "";

            var parser = new FluidParser();

            var model = new
            {
                _accountMoveSendHeader = _accountMoveSendHeader,
                _accountMovesSend = ls_accountMovesSend,
                _company = empresaI
            };

            var options = new TemplateOptions();
            options.MemberAccessStrategy.Register<AccountMoveSendHeader>();
            options.MemberAccessStrategy.Register<account_move_send>();
            options.MemberAccessStrategy.Register<account_move_line_send>();
            options.MemberAccessStrategy.Register<res_company>();

            if (parser.TryParse(strTemplate, out var templateF, out var error))
            {
                var context = new TemplateContext(model, options);
                result = templateF.Render(context);
            }
            else
            {
                Debug.WriteLine($"Error: {error}");
            }

            return result;
        }
    }
}
