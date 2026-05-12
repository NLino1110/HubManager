using DMCobranzas.Models.Specials;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Security;
using DMSA.Models.Odoo.StaticData;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.DebitCollection;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using Fluid;
using Fluid.Values;
using Microsoft.Maui.Controls.Shapes;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace DMCobranzas.Services.Templates
{
    public class Processor
    {
        public Processor()
        { 

        }

        public static string CenterText(string text, int width)
        {
            text ??= string.Empty;

            if (text.Length > width)
                text = text.Substring(0, width);

            int padding = width - text.Length;
            int padLeft = padding / 2;
            int padRight = padding - padLeft;

            return new string(' ', padLeft) + text + new string(' ', padRight);
        }

        public static string LineLeftRight(string left, string right, int width)
        {
            left ??= string.Empty;
            right ??= string.Empty;

            // Si se pasa del ancho, recorta el lado izquierdo
            if (left.Length + right.Length > width)
            {
                int maxLeft = width - right.Length;
                if (maxLeft < 0) maxLeft = 0;

                left = left.Substring(0, Math.Min(left.Length, maxLeft));
            }

            int spaces = width - left.Length - right.Length;

            return left + new string(' ', spaces) + right;
        }

        private string Money(decimal value)
        {
            return $"${value:N2}";
        }

        public async Task<(byte[], string, string)> Template_MultipleCobrosInvoiceGroup(List<MultipleCobrosInvoice> itemsGroup)
        {
            if (itemsGroup == null || itemsGroup.Count == 0)
                return (Array.Empty<byte>(), "", "");

            var r = new ReceiptBuilder();

            List<MultipleCobrosInvoiceLine> allPayments = new();

            AccountPaymentDaily daily = null;
            res_company empresa = null;
            user_access user = null;
            var dailyDb = new AccountPaymentDailyDb(App.Session.odooConnection.DbNameSqlite);
            var lineDb = new MultipleCobrosInvoiceLineDb(App.Session.odooConnection.DbNameSqlite);
            var lineAiDb = new MultipleCobrosInvoiceLineAiDb(App.Session.odooConnection.DbNameSqlite);
            var userAccessDb = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
            var bankDb = new BankDb(App.Session.odooConnection.DbNameSqlite);
            
            var banks = (await bankDb.GetItemsAsync(x=>x.id > 0)).ToDictionary(b => b.id);

            foreach (var item in itemsGroup)
            {
                
                daily = (await dailyDb.GetItemsDateCutAsync(item.company_id, item.create_date));

                empresa = App.Session.CurrentUserFront.empresas
                    .FirstOrDefault(x => x.id == item.company_id);

                if(daily== null)
                {
                    return (Array.Empty<byte>(), "", "");
                }
                
                var lines = await lineDb.GetItemsAsync(x => x.MultipleCobrosInvoiceId == item.id);
                                
                user = await userAccessDb.GetItemAsync(x => x.uid == daily.uid);

                foreach (var line in lines)
                {
                    line.display_type = TipoEmision.data
                        .FirstOrDefault(x => x.code == line.Type)?.abrev;

                    var linesAi = (await lineAiDb.GetItemsAsync(x => x.partner_id == line.Id)).ToArray();
                    line.lines = linesAi;
                    allPayments.Add(line);
                }
            }

            /*
             * AGRUPAR JOURNALS
             */

            var cobrosSummary = allPayments
                .GroupBy(p => p.Type)
                .Select(g => new JournalSummary
                {
                    Type = g.Key,
                    TotalRecords = g.Count(),
                    TotalAmount = g.Sum(p => (decimal)p.Amount),
                    Lines = g.Select(p => new MultipleCobrosInvoiceLine
                    {
                        Id = p.Id,
                        BankId = p.BankId,
                        PaymentDate = p.PaymentDate,
                        Amount = p.Amount
                    }).ToArray()
                })
                .ToList();

            int countItems = allPayments.Count;

            /*
             * HEADER
             */

            r.Center().Bold().Line(empresa?.name ?? "");
            r.Line("Resumen Cobranzas");
            r.Normal();
            r.ResetStyle();

            r.Left();
            r.Line($"Dia: {daily?.closing_id}");

            r.Separator();

            r.Line($"Refer. Cierre: {daily?.payment_reference}");
            r.Line($"Cant. Recibos: {countItems}");
            r.Line("Cobrador: " + user?.name);

            r.Separator();

            /*
             * RESUMEN POR JOURNAL
             */

            foreach (var item in cobrosSummary)
            {
                var name_group = TipoEmision.data.Where(x => x.code == item.Type).FirstOrDefault().name;
                r.Columns(name_group, item.TotalRecords.ToString());

                if(item.Type == "transfer")
                {
                    foreach(var line in item.Lines)
                    {
                        string bank_name = banks[(int) line.BankId].name;
                        string right = string.IsNullOrEmpty(line.Circular) ? "-" : line.Circular;
                        r.Columns(bank_name, "Tr#" + right);
                        //r.Columns(bank_name, $"{line.Amount:0.00}");
                    }
                }

                r.Columns(
                    $"TOTAL {name_group}",
                    $"${item.TotalAmount:0.00}"
                );
            }

            r.Separator();

            /*
             * FIRMA
             */

            r.Feed(2);

            r.Center();
            r.Line("--------------------------------");
            r.Line("-Firma Vendedor-");

            r.Feed(3);
            r.Cut();

            return (r.Build(), r.BuildPreviewHtml(), r.BuildPreview());
        }

        public async Task<(byte[] bytes, string preview, string plain)> Template_MultipleCobrosInvoice(MultipleCobrosInvoice invoice)
        {
            if (invoice == null)
                return (Array.Empty<byte>(),string.Empty, string.Empty);

            int width = 32;

            List<AccountMoveSummary> accountMoveSummaries = new();
            List<MultipleCobrosInvoiceLine> lines = new();

            res_partner partner = null;
            user_access user = null;

            var resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
            partner = await resPartnerDb.GetItemsAsync(invoice.company_id, invoice.partner_id);

            var userAccessDb = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
            user = await userAccessDb.GetItemAsync(x => x.uid == invoice.create_uid);

            var lineDb = new MultipleCobrosInvoiceLineDb(App.Session.odooConnection.DbNameSqlite);
            lines = await lineDb.GetItemsAsync(x => x.MultipleCobrosInvoiceId == invoice.id);

            var accountMoveDb = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
            var bankDb = new BankDb(App.Session.odooConnection.DbNameSqlite);

            var banks = (await bankDb.GetItemsAsync(x => x.id > 0)).ToDictionary(b => b.id);

            decimal totalAmount = 0;
            decimal totalAmountApplied = 0;
            decimal totalAmountCancell = 0;
            decimal totalPending = 0;
            decimal totalAnticipo = 0;
            decimal totalFp = 0;

            totalAmount = (decimal) invoice.amount;
            totalPending = partner.saldo_total;

            foreach (var line in lines)
            {
                var aiDb = new MultipleCobrosInvoiceLineAiDb(App.Session.odooConnection.DbNameSqlite);
                var apl = await aiDb.GetItemsAsync(x => x.multiple_cobros_invoice_line_id == line.Id);

                line.lines = apl.ToArray();
                totalAmountCancell += (decimal) line.Amount;

                foreach (var ai in apl)
                {
                    totalAmountApplied += ai.amount_asigned;
                    totalFp += ai.amount_asigned;

                    var found = accountMoveSummaries
                        .FirstOrDefault(x => x.docnum_mask == ai.docnum_mask);

                    if (found == null)
                    {
                        decimal residual = 0;

                        var move = (await accountMoveDb
                            .GetItemsAsync(x => x.docnum_mask == ai.docnum_mask))
                            .FirstOrDefault();

                        if (move != null)
                        {
                            //residual = move.amount_residual_virtual;
                            residual = move.amount_residual - ai.amount_asigned;
                        }

                        accountMoveSummaries.Add(new AccountMoveSummary
                        {
                            docnum_mask = ai.docnum_mask,
                            total_amount_residual = residual,
                            total_amount_reconciled = ai.amount_asigned,
                            invoice_date = ai.invoice_date
                        });
                    }
                    else
                    {
                        found.total_amount_reconciled += ai.amount_asigned;
                        found.total_amount_residual -= ai.amount_asigned;
                    }
                }
            }

            totalAnticipo = totalAmount - totalAmountApplied;

            accountMoveSummaries = accountMoveSummaries
                .OrderBy(x => x.docnum_mask)
                .ToList();

            foreach (var doc in accountMoveSummaries)
            {
                //totalPending += doc.total_amount_residual;
                totalPending -= doc.total_amount_reconciled;
            }                

            var empresa = App.Session.CurrentUserFront.empresas
                .FirstOrDefault(x => x.id == invoice.company_id);

            var r = new ReceiptBuilder();

            /*
             * CABECERA
             */

            r.Center().Bold().Line(empresa?.name ?? "");
            r.Line($"RECIBO #{invoice.receipt_name}");
            r.Normal();
            r.ResetStyle();

            r.Left();
            //r.Small();
            r.Line($"Cliente: {invoice.partner_name}");
            r.Line($"Estado: {invoice.payment_status}");
            //r.ResetStyle();

            r.Separator();

            /*
             * FORMAS DE PAGO
             */

            r.Center().Line("F.PAGO");
            r.Left();

            foreach (var line in lines)
            {
                string type = TipoEmision.data
                    .FirstOrDefault(x => x.code == line.Type)?
                    .name?.ToUpper() ?? line.Type;

                string monto = Money(line.Amount ?? 0m);

                string bank_name = (line.BankId != null && banks != null && banks.TryGetValue((int)line.BankId, out var bank))
                    ? bank?.name ?? ""
                    : "";

                r.Columns(type, monto);

                if (line.Type == "check" || line.Type == "check_day")
                {
                    r.Line($" F.Cobro: {line.WithdrawalDate:yyyy-MM-dd}");
                    r.Line($" {bank_name}");
                    r.Line($" Ch# {line.NumberCheckText}");
                }

                if (line.Type == "credit_card")
                {
                    r.Line($" Tj. Lote# {line.LoteVoucher}");
                }

                if (line.Type == "transfer")
                {                    
                    r.Columns($" Cta. {line.AccNumber}",$"Dp# {line.Circular}");
                    r.Line($" {bank_name}");
                }

                if (line.Type == "deposito")
                {                    
                    r.Columns($" Cta. {line.AccNumber}", $"Dp# {line.Circular}");
                }
            }

            r.Separator();
            r.Columns("TOTAL F/P:", Money(totalFp));

            /*
             * DOCUMENTOS
             */

            r.Separator();
            r.Center().Line("DOCUMENTOS");
            r.Left();

            foreach (var doc in accountMoveSummaries)
            {
                r.Line($"FAC # {doc.docnum_mask}");
                r.Line($" Canc.: {Money(doc.total_amount_reconciled)}");
                r.Line($" Saldo: {Money(doc.total_amount_residual)}");
            }

            r.Separator();
                        
            r.Columns("TOTAL CANC:", Money(totalAmountCancell));
            r.Separator();            
            r.Columns("TOT. FACT. PEND.:", Money(totalPending - totalAnticipo));            

            r.Separator();

            /*
             * FOOTER
             */

            r.Line($"Email: {partner?.email}");
            r.Line($"Vnd: {user?.name}");
            r.Line($"Fecha: {invoice.create_date:dd/MM/yyyy HH:mm:ss}");

            r.Feed(2);

            r.Center().Line("----------------------------");
            r.Line("-Firma Cliente-");

            r.Feed(3);
            r.Cut();

            return (r.Build(), r.BuildPreviewHtml(), r.BuildPreview());
        }

        public async Task<(byte[], string, string)> Template_AccountMoveSendNC(CreditNoteRequestGroup header)
        {
            if (header == null)
                return (Array.Empty<byte>(), "", "");

            List<credit_note_request> requests = new();

            CreditNoteRequestDb db = new CreditNoteRequestDb(App.Session.odooConnection.DbNameSqlite);
            requests = await db.GetByParent(header.id);

            foreach (var req in requests)
            {
                CreditNoteRequestDetailDb lineDb =
                    new CreditNoteRequestDetailDb(App.Session.odooConnection.DbNameSqlite);

                var lines = await lineDb.GetItemsByParentAsync(req.id);
                req.lines = lines.ToArray();
            }

            var empresa = App.Session.CurrentUserFront.empresas
                .FirstOrDefault(x => x.id == header.company_id);

            var r = new ReceiptBuilder();

            /*
             * HEADER
             */

            r.Center().Bold().Line(empresa?.name ?? "");
            r.Line("SOLICITUD DE NOTA DE CREDITO");
            r.Normal();
            r.ResetStyle();

            r.Left();
            r.Line($"CLIENTE: {header.partner_name}");

            r.Separator();

            r.Center();
            r.Line("ESTE TICKET ES INFORMATIVO");
            r.Line("LA SOLICITUD SERA EVALUADA");
            r.Line("PARA SU APROBACION");

            r.Left();
            r.Separator();

            r.Center().Line("DETALLE");
            r.SeparatorTop();
            r.SeparatorBottom();

            /*
             * DOCUMENTOS
             */

            foreach (var req in requests)
            {                
                
                r.Line($"Doc.Ref.: {req._ref}");
                r.Separator();
                r.Left();
                r.Columns("ARTICULO","CANT");

                r.Separator();

                if (req.lines != null)
                {
                    foreach (var line in req.lines)
                    {
                        string name = line.name ?? "";

                        if (name.Length > 24)
                            name = name.Substring(0, 24);

                        string qty = line.quantity.ToString();

                        r.Columns(name, qty);
                    }
                }

                r.Separator();
            }

            /*
             * FOOTER
             */

            r.Line($"Fecha: {header.create_datetime}");

            r.Feed(2);

            r.Center();
            r.Line("------------------------------");
            r.Line("-Firma cliente-");

            r.Feed(3);
            r.Cut();

            return (r.Build(), r.BuildPreviewHtml(), r.BuildPreview());
        }

        ////public async Task<string> Template_AccountMoveSendNC_V3_Classic(CreditNoteRequestGroup _accountMoveSendHeader)
        ////{
        ////    List<credit_note_request> ls_accountMovesSend = new List<credit_note_request>();
            
        ////    if (_accountMoveSendHeader != null)
        ////    {
        ////        CreditNoteRequestDb _accountMoveSendDb = new CreditNoteRequestDb(App.Session.odooConnection.DbNameSqlite);
        ////        ls_accountMovesSend = await _accountMoveSendDb.GetByParent(_accountMoveSendHeader.id);

        ////        foreach(var  accountMoveSend in ls_accountMovesSend)
        ////        {
        ////            CreditNoteRequestDetailDb _accountMoveSendLineDb = new CreditNoteRequestDetailDb(App.Session.odooConnection.DbNameSqlite);
        ////            var lineItem = await _accountMoveSendLineDb.GetItemsByParentAsync(accountMoveSend.id);

        ////            accountMoveSend.lines = lineItem.ToArray();
        ////        }
        ////    }

        ////    res_company[] Empresas = null;
        ////    Empresas = App.Session.CurrentUserFront.empresas;
        ////    var empresaI = Empresas.ToList().Where(i => i.id == _accountMoveSendHeader.company_id).FirstOrDefault();

        ////    string resourceName = "DMCobranzas.Resources.Raw.liq_req_movSendHead_ncr.txt";

        ////    var assembly = Assembly.GetExecutingAssembly();

        ////    Stream stream = assembly.GetManifestResourceStream(resourceName);
        ////    StreamReader reader = new StreamReader(stream);
        ////    string strTemplate = reader.ReadToEnd();

        ////    string result = "";

        ////    var parser = new FluidParser();

        ////    var model = new
        ////    {
        ////        _accountMoveSendHeader = _accountMoveSendHeader,
        ////        _accountMovesSend = ls_accountMovesSend,
        ////        _company = empresaI
        ////    };

        ////    var options = new TemplateOptions();
        ////    options.MemberAccessStrategy.Register<CreditNoteRequestGroup>();
        ////    options.MemberAccessStrategy.Register<credit_note_request>();
        ////    options.MemberAccessStrategy.Register<credit_note_request_detail>();
        ////    options.MemberAccessStrategy.Register<res_company>();

        ////    if (parser.TryParse(strTemplate, out var templateF, out var error))
        ////    {
        ////        var context = new TemplateContext(model, options);
        ////        result = templateF.Render(context);
        ////    }
        ////    else
        ////    {
        ////        Debug.WriteLine($"Error: {error}");
        ////    }

        ////    return result;
        ////}
    }
}
