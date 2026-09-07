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

        private static string FormatDocumentNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "-")
                return "-";

            return $"N\u00B0 {value.Trim()}";
        }

        private static string FormatSummaryDetailLine(string label, string documentNumber)
        {
            return $"{label}  -  {FormatDocumentNumber(documentNumber)}";
        }

        private static string GetBankName(
            MultipleCobrosInvoiceLine line,
            Dictionary<int, ResBank> banks)
        {
            if (line.BankId != null && banks != null && banks.TryGetValue((int)line.BankId, out var bank))
                return bank?.name ?? "-";

            return "-";
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

            Dictionary<int, ResBank> banks = null;

            if (bankDb != null)
            {
                banks = (await bankDb.GetItemsAsync(x => x.id > 0)).ToDictionary(b => b.id);
            }                

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
                    Lines = g.ToArray()
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
                r.Columns(name_group, $"Cant. {item.TotalRecords}");

                if (item.Type is "transfer" or "deposito" or "check" or "check_day")
                {
                    foreach (var line in item.Lines)
                    {
                        switch (item.Type)
                        {
                            case "transfer":
                                r.Line(FormatSummaryDetailLine(
                                    GetBankName(line, banks),
                                    string.IsNullOrWhiteSpace(line.Circular) ? "-" : line.Circular.Trim()));
                                break;
                            case "deposito":
                                r.Line(FormatSummaryDetailLine(
                                    string.IsNullOrWhiteSpace(line.journal_name) ? "-" : line.journal_name.Trim(),
                                    string.IsNullOrWhiteSpace(line.Circular) ? "-" : line.Circular.Trim()));
                                break;
                            case "check":
                            case "check_day":
                                r.Line(FormatSummaryDetailLine(
                                    GetBankName(line, banks),
                                    string.IsNullOrWhiteSpace(line.NumberCheckText) ? "-" : line.NumberCheckText.Trim()));
                                break;
                        }
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

        private static async Task<bool> ResolveIsNotaDebitoAsync(
            MultipleCobrosInvoiceLineAi ai,
            AccountMoveDb accountMoveDb,
            Dictionary<int, bool> cache)
        {
            if (ai.is_nota_debito)
                return true;

            if (ai.invoice_id <= 0)
                return false;

            if (cache.TryGetValue(ai.invoice_id, out var cached))
                return cached;

            var move = await accountMoveDb.GetItemAsync(x => x.id == ai.invoice_id);
            bool value = move?.is_nota_debito ?? false;
            cache[ai.invoice_id] = value;
            return value;
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

            /*
             * Ticket recibo individual — saldos congelados al registrar el cobro.
             *
             * Fuente: multiple_cobros_invoice_line_ai (amount_residual, amount_asigned).
             * No usa account_move ni partner.saldo_total: un recibo posterior sobre la
             * misma factura no altera lo impreso en tickets anteriores.
             *
             * | Campo            | Cálculo                                                          |
             * |------------------|------------------------------------------------------------------|
             * | TOTAL F/P        | Σ Amount de formas de pago (total cobrado en el recibo)          |
             * | Canc.            | amount_asigned por factura en este recibo                        |
             * | Saldo            | amount_residual (registro) − Σ amount_asigned del doc/recibo   |
             * | TOTAL CANC.      | Σ amount_asigned (total aplicado a documentos)                   |
             * | ANTICIPO         | TOTAL F/P − TOTAL CANC. (si > 0)                                 |
             * | TOT. FACT. PEND. | Σ saldos de facturas pendientes del cliente;                    |
             * |                  | las del recibo usan snapshot, las demás amount_residual actual  |
             *
             * Estado en ticket: done/APLICADO se muestra como PROCESADO.
             */

            decimal totalAmountApplied = 0;
            decimal totalCollected = lines.Sum(l => l.Amount ?? 0m);
            if (totalCollected <= 0 && invoice.amount > 0)
                totalCollected = (decimal)invoice.amount;

            var notaDebitoCache = new Dictionary<int, bool>();

            foreach (var line in lines)
            {
                var aiDb = new MultipleCobrosInvoiceLineAiDb(App.Session.odooConnection.DbNameSqlite);
                var apl = await aiDb.GetItemsAsync(x => x.multiple_cobros_invoice_line_id == line.Id);

                line.lines = apl.ToArray();

                foreach (var ai in apl)
                {
                    totalAmountApplied += ai.amount_asigned;

                    bool isNotaDebito = await ResolveIsNotaDebitoAsync(ai, accountMoveDb, notaDebitoCache);

                    var found = accountMoveSummaries
                        .FirstOrDefault(x => x.docnum_mask == ai.docnum_mask);

                    if (found == null)
                    {
                        accountMoveSummaries.Add(new AccountMoveSummary
                        {
                            docnum_mask = ai.docnum_mask,
                            is_nota_debito = isNotaDebito,
                            total_amount_residual = ai.amount_residual - ai.amount_asigned,
                            total_amount_reconciled = ai.amount_asigned,
                            invoice_date = ai.invoice_date
                        });
                    }
                    else
                    {
                        found.is_nota_debito = found.is_nota_debito || isNotaDebito;
                        found.total_amount_reconciled += ai.amount_asigned;
                        found.total_amount_residual -= ai.amount_asigned;
                    }
                }
            }

            accountMoveSummaries = accountMoveSummaries
                .OrderBy(x => x.docnum_mask)
                .ToList();

            decimal anticipoAmount = totalCollected - totalAmountApplied;
            if (anticipoAmount < 0)
                anticipoAmount = 0;

            var receiptSaldoByDoc = accountMoveSummaries.ToDictionary(
                x => x.docnum_mask,
                x => x.total_amount_residual);

            var pendingInvoices = await accountMoveDb.GetItemsWithBalanceByPartnerAsync(
                invoice.company_id,
                invoice.partner_id);

            decimal totFactPend = 0;
            var countedDocs = new HashSet<string>();

            foreach (var move in pendingInvoices)
            {
                if (string.IsNullOrWhiteSpace(move.docnum_mask))
                    continue;

                if (receiptSaldoByDoc.TryGetValue(move.docnum_mask, out var receiptSaldo))
                    totFactPend += receiptSaldo;
                else
                    totFactPend += move.amount_residual;

                countedDocs.Add(move.docnum_mask);
            }

            // Facturas solo en este recibo con saldo snapshot > 0 y aún no contadas.
            foreach (var doc in accountMoveSummaries)
            {
                if (!countedDocs.Contains(doc.docnum_mask) && doc.total_amount_residual > 0)
                    totFactPend += doc.total_amount_residual;
            }

            string ticketEstado = invoice.payment_status;
            if (string.Equals(invoice.state, "done", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ticketEstado, CobrosEstados.APLICADO, StringComparison.Ordinal))
            {
                ticketEstado = CobrosEstados.PROCESADO;
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
            r.Line($"Estado: {ticketEstado}");
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

                string bank_name_target = line.journal_name;

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
                    r.Line($" {bank_name_target}");
                }

                if (line.Type == "deposito")
                {                    
                    r.Columns($" Cta. {line.AccNumber}", $"Dp# {line.Circular}");
                    r.Line($" {bank_name_target}");
                }
            }

            r.Separator();
            r.Columns("TOTAL F/P:", Money(totalCollected));

            /*
             * DOCUMENTOS — saldos del snapshot registrado en este recibo.
             */

            r.Separator();
            r.Center().Line("DOCUMENTOS");
            r.Left();

            if (accountMoveSummaries.Count == 0 && anticipoAmount > 0)
            {
                string anticipoLabel = lines
                    .Select(l => l.Resumen)
                    .FirstOrDefault(r => !string.IsNullOrWhiteSpace(r))
                    ?? "ANTICIPO DE CLIENTE";

                r.Line(anticipoLabel);
            }
            else
            {
                foreach (var doc in accountMoveSummaries)
                {
                    r.Line(doc.document_reference_label);
                    r.Line($" Canc.: {Money(doc.total_amount_reconciled)}");
                    r.Line($" Saldo: {Money(doc.total_amount_residual)}");
                }
            }

            r.Separator();

            r.Columns("TOTAL CANC:", Money(totalAmountApplied));
            if (anticipoAmount > 0)
                r.Columns("ANTICIPO:", Money(anticipoAmount));
            r.Separator();
            r.Columns("TOT. FACT. PEND.:", Money(totFactPend));            

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
