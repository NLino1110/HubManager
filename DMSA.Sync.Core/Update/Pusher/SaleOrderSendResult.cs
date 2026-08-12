namespace DMSA.Sync.Core.Update.Pusher
{
    public sealed class SaleOrderSendResult
    {
        public bool Ok { get; init; }
        public bool IsDuplicateExternalGuid { get; init; }
        public bool LinkedExistingOrder { get; init; }
        public string ErpName { get; init; } = string.Empty;
        public string ErrorMessage { get; init; } = string.Empty;
        public string OrderLabel { get; init; } = string.Empty;

        public static SaleOrderSendResult Success(bool linkedExisting = false, string erpName = "") =>
            new()
            {
                Ok = true,
                LinkedExistingOrder = linkedExisting,
                ErpName = erpName ?? string.Empty
            };

        public static SaleOrderSendResult Fail(
            string message,
            bool duplicateGuid = false,
            string orderLabel = "") =>
            new()
            {
                Ok = false,
                ErrorMessage = message,
                IsDuplicateExternalGuid = duplicateGuid,
                OrderLabel = orderLabel
            };
    }

    internal static class SaleOrderSyncErrorParser
    {
        internal static bool IsDuplicateExternalGuid(string? message, string? debug)
        {
            var text = $"{message} {debug}".ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return text.Contains("external guid")
                || text.Contains("external_guid")
                || (text.Contains("already exists") && text.Contains("guid"));
        }

        internal static string BuildUserMessage(string? serverMessage, bool duplicateGuid)
        {
            if (duplicateGuid)
            {
                return "No se puede completar la operación: el External GUID ya existe en otra orden de venta en el ERP.\n\n"
                    + "Suele ocurrir si el pedido ya se envió pero no se guardó el número ERP en el dispositivo.\n\n"
                    + "Use Recuperar pedido en ERP para traer el número. Use Reintentar envío solo si confirma que el pedido no existe en el ERP.";
            }

            return string.IsNullOrWhiteSpace(serverMessage)
                ? "No se pudo sincronizar el pedido con el ERP."
                : serverMessage;
        }
    }
}
