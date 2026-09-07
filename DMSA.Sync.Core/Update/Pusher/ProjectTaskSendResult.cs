using System.Linq;

namespace DMSA.Sync.Core.Update.Pusher
{
    public sealed class ProjectTaskLineFailure
    {
        public int LocalId { get; init; }
        public string Label { get; init; } = string.Empty;
        public string ErrorMessage { get; init; } = string.Empty;
    }

    public sealed class ProjectTaskSendResult
    {
        public bool Ok { get; init; }
        public bool IsPartial { get; init; }
        public bool HeaderFailed { get; init; }
        public bool AutoRetried { get; init; }
        public int TotalCount { get; init; }
        public int SyncedCount { get; init; }
        public int FailedCount { get; init; }
        public int PendingCount => Math.Max(0, TotalCount - SyncedCount);
        public string SyncStatus { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string TaskLabel { get; init; } = string.Empty;
        public List<ProjectTaskLineFailure> Failures { get; init; } = new();

        public static ProjectTaskSendResult Build(
            string taskLabel,
            int total,
            int synced,
            int failed,
            bool headerFailed,
            bool autoRetried,
            List<ProjectTaskLineFailure>? failures = null)
        {
            failures ??= new List<ProjectTaskLineFailure>();
            var pending = Math.Max(0, total - synced);
            string syncStatus;
            bool ok;
            bool isPartial;

            if (headerFailed)
            {
                syncStatus = DMSA.Models.Odoo.Tareas.ProjectTaskSyncStatus.Error;
                ok = false;
                isPartial = false;
            }
            else if (total == 0)
            {
                syncStatus = DMSA.Models.Odoo.Tareas.ProjectTaskSyncStatus.Pending;
                ok = false;
                isPartial = false;
            }
            else if (synced >= total)
            {
                syncStatus = DMSA.Models.Odoo.Tareas.ProjectTaskSyncStatus.Complete;
                ok = true;
                isPartial = false;
            }
            else if (synced > 0)
            {
                syncStatus = DMSA.Models.Odoo.Tareas.ProjectTaskSyncStatus.Partial;
                ok = false;
                isPartial = true;
            }
            else
            {
                syncStatus = DMSA.Models.Odoo.Tareas.ProjectTaskSyncStatus.Error;
                ok = false;
                isPartial = false;
            }

            var message = BuildMessage(taskLabel, synced, total, pending, headerFailed, autoRetried, failures);

            return new ProjectTaskSendResult
            {
                Ok = ok,
                IsPartial = isPartial,
                HeaderFailed = headerFailed,
                AutoRetried = autoRetried,
                TotalCount = total,
                SyncedCount = synced,
                FailedCount = failed,
                SyncStatus = syncStatus,
                Message = message,
                TaskLabel = taskLabel,
                Failures = failures
            };
        }

        private static string BuildMessage(
            string taskLabel,
            int synced,
            int total,
            int pending,
            bool headerFailed,
            bool autoRetried,
            List<ProjectTaskLineFailure> failures)
        {
            if (headerFailed)
            {
                var headerError = failures.FirstOrDefault()?.ErrorMessage;
                return string.IsNullOrWhiteSpace(headerError)
                    ? $"No se pudo enviar la cabecera de la actividad {taskLabel}."
                    : $"No se pudo enviar la cabecera de la actividad {taskLabel}: {headerError}";
            }

            if (total == 0)
                return "No hay detalles para enviar.";

            if (synced >= total)
            {
                return autoRetried
                    ? $"Actividad {taskLabel} enviada correctamente tras reintento automático. {synced} de {total} detalles sincronizados."
                    : $"Actividad {taskLabel} enviada correctamente. {synced} de {total} detalles sincronizados.";
            }

            var retryHint = autoRetried
                ? " Tras el reintento automático aún quedan pendientes; use Reprocesar pendientes."
                : " Use Reprocesar pendientes para reintentar los registros faltantes.";

            return $"Advertencia: no se sincronizaron todos los detalles de {taskLabel} ({synced} de {total}). "
                   + $"Pendientes: {pending}.{retryHint}";
        }
    }
}
