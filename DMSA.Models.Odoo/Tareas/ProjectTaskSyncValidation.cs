namespace DMSA.Models.Odoo.Tareas
{
    /// <summary>
    /// Validación adicional de sincronización (complementa is_synchronized).
    /// Detecta registros marcados como enviados pero sin vínculo real con el ERP.
    /// </summary>
    public static class ProjectTaskSyncValidation
    {
        public static bool IsHeaderLinkedToErp(ProjectTask task) =>
            task != null && task.id_sync > 0;

        public static bool IsLineEffectivelySynced(AccountAnalyticLine line, int headerIdSync)
        {
            if (line == null || !line.is_synchronized || line.id_sync <= 0)
                return false;

            if (line.task_id_sync <= 0)
                return false;

            if (headerIdSync > 0 && line.task_id_sync != headerIdSync)
                return false;

            return true;
        }

        public static bool IsLinePendingSync(AccountAnalyticLine line, int headerIdSync) =>
            !IsLineEffectivelySynced(line, headerIdSync);

        public static int CountEffectivelySyncedLines(IEnumerable<AccountAnalyticLine>? lines, int headerIdSync) =>
            lines?.Count(l => IsLineEffectivelySynced(l, headerIdSync)) ?? 0;

        public static bool HasBrokenLineLinkage(IEnumerable<AccountAnalyticLine>? lines, int headerIdSync) =>
            lines?.Any(l => l.is_synchronized && !IsLineEffectivelySynced(l, headerIdSync)) == true;

        public static bool NeedsHeaderRelink(ProjectTask task, IEnumerable<AccountAnalyticLine>? lines)
        {
            if (task == null)
                return false;

            if (lines == null || !lines.Any())
                return task.is_synchronized && task.id_sync <= 0;

            return task.id_sync <= 0 && lines.Any(l => l.is_synchronized || l.id_sync > 0);
        }
    }
}
