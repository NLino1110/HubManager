namespace DMSA.Models.Odoo.Accounting
{
    public static class AccountMoveDocumentDisplay
    {
        public const string InvoicePrefix = "FACT #";
        public const string DebitNotePrefix = "NDBI #";
        public const string CreditNotePrefix = "NCRE #";
        public const string AdvancePrefix = "ANT #";

        /// <summary>
        /// Tipos de account.move que bajan sync masiva y sync por cliente.
        /// </summary>
        public static readonly object[] SyncMoveTypes = { "out_invoice", "out_refund", "advance" };

        /// <summary>
        /// Referencia UI/sync: documentos incluidos en la descarga.
        /// </summary>
        public const string SyncDocumentsSummary =
            "facturas y notas de débito con saldo pendiente";

        public const string SyncDocumentsShortLabel = "facturas y notas de débito";

        public const string BulkSyncHeadersProgressLabel =
            "Documentos (" + SyncDocumentsShortLabel + ")";

        public const string BulkSyncDetailsProgressLabel =
            "Det. documentos (" + SyncDocumentsShortLabel + ")";

        public const string PartnerSyncDocumentsSummary = SyncDocumentsSummary;

        public const string PartnerSyncConfirmMessage =
            "Se descargarán " + SyncDocumentsSummary + " del cliente. ¿Desea continuar?";

        public const string PartnerSyncProgressMessage =
            "Actualizando facturas y notas de débito del cliente...";

        public static bool IsAdvanceMoveType(string? moveType) =>
            string.Equals(moveType, "advance", StringComparison.OrdinalIgnoreCase);

        public static bool IsCreditLikeMoveType(string? moveType) =>
            string.Equals(moveType, "out_refund", StringComparison.OrdinalIgnoreCase);

        public static bool IsNegativeBalanceDocument(string? moveType) =>
            IsCreditLikeMoveType(moveType) || IsAdvanceMoveType(moveType);

        /// <summary>
        /// out_invoice: saldo &gt; 0. out_refund / advance: saldo &lt; 0.
        /// </summary>
        public static bool HasOpenBalanceForBalanceView(string? moveType, decimal amountResidual)
        {
            if (string.IsNullOrWhiteSpace(moveType))
                return false;

            if (string.Equals(moveType, "out_invoice", StringComparison.OrdinalIgnoreCase))
                return amountResidual > 0;

            if (IsNegativeBalanceDocument(moveType))
                return amountResidual < 0;

            return false;
        }

        public static bool HasOpenBalanceForBalanceView(account_move move) =>
            move != null && HasOpenBalanceForBalanceView(move.move_type, move.amount_residual);

        /// <summary>
        /// Dominio Odoo: out_invoice, o out_refund/advance solo con amount_residual &lt; 0.
        /// Por ahora solo out_invoice (out_refund/advance comentados).
        /// </summary>
        public static object[] BuildSyncMoveTypeDomain(string moveTypeField = "move_type")
        {
            // var amountResidualField = moveTypeField == "move_type"
            //     ? "amount_residual"
            //     : "move_id.amount_residual";

            return new object[]
            {
                // "|", "|",
                new object[] { moveTypeField, "=", "out_invoice" },
                // "&",
                // new object[] { moveTypeField, "=", "out_refund" },
                // new object[] { amountResidualField, "<", 0 },
                // "&",
                // new object[] { moveTypeField, "=", "advance" },
                // new object[] { amountResidualField, "<", 0 },
            };
        }

        public static string GetDocumentReferenceLabel(
            string? docnumMask,
            bool isNotaDebito)
            => GetDocumentReferenceLabel(docnumMask, moveType: null, isNotaDebito);

        public static string GetDocumentReferenceLabel(
            string? docnumMask,
            string? moveType,
            bool isNotaDebito = false)
        {
            if (string.IsNullOrWhiteSpace(docnumMask))
                return string.Empty;

            if (string.Equals(moveType, "out_refund", StringComparison.OrdinalIgnoreCase))
                return $"{CreditNotePrefix}{docnumMask.Trim()}";

            if (IsAdvanceMoveType(moveType))
                return $"{AdvancePrefix}{docnumMask.Trim()}";

            if (isNotaDebito)
                return $"{DebitNotePrefix}{docnumMask.Trim()}";

            return $"{InvoicePrefix}{docnumMask.Trim()}";
        }
    }
}
