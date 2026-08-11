using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    /// <summary>
    /// Log de fases de sincronización (Output / Debug).
    /// Un aviso por fase: INICIO → INGRESO AL PROCESO → (FALLO si aplica) → FIN.
    /// No registra cada RPC individual (evita ruido en search_read paginado).
    /// </summary>
    public static class SyncActivityLog
    {
        /// <summary>
        /// Ejecuta la acción y escribe INICIO / INGRESO / FIN.
        /// Si falla: FALLO + FIN con estado error. No relanza si <paramref name="swallowErrors"/> es true.
        /// </summary>
        public static async Task<(bool Ok, Exception? Error)> RunAsync(
            string syncType,
            Func<Task> action,
            string? detail = null,
            bool swallowErrors = false)
        {
            var extra = FormatExtra(detail);
            var sw = Stopwatch.StartNew();

            Write($"INICIO | {syncType}{extra}");
            Write($"INGRESO AL PROCESO | {syncType}{extra}");

            try
            {
                await action();
                sw.Stop();
                Write($"FIN | {syncType}{extra} | OK | {sw.ElapsedMilliseconds} ms");
                return (true, null);
            }
            catch (Exception ex)
            {
                sw.Stop();
                Write($"FALLO | {syncType}{extra} | {ex.GetType().Name}: {ex.Message}");
                Write($"FIN | {syncType}{extra} | FALLO | {sw.ElapsedMilliseconds} ms");

                if (!swallowErrors)
                    throw;

                return (false, ex);
            }
        }

        /// <summary>
        /// Scope para bloques compuestos (p. ej. actualización manual completa).
        /// Escribe INICIO + INGRESO al crear; FIN al disponer. Usar <see cref="SyncLogScope.MarkFailed"/> ante error.
        /// </summary>
        public static SyncLogScope Begin(string syncType, string? detail = null)
        {
            var scope = new SyncLogScope(syncType, detail);
            scope.WriteStart();
            return scope;
        }

        private static string FormatExtra(string? detail) =>
            string.IsNullOrWhiteSpace(detail) ? "" : $" | {detail}";

        internal static void Write(string body)
        {
            var msg = $"[DMOrders Sync] {body} | {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
            Debug.WriteLine(msg);
            Console.WriteLine(msg);
        }

        public sealed class SyncLogScope : IDisposable
        {
            private readonly string _syncType;
            private readonly string? _detail;
            private readonly Stopwatch _sw = Stopwatch.StartNew();
            private bool _disposed;
            private bool _failed;

            public SyncLogScope(string syncType, string? detail)
            {
                _syncType = syncType;
                _detail = detail;
            }

            internal void WriteStart()
            {
                var extra = FormatExtra(_detail);
                Write($"INICIO | {_syncType}{extra}");
                Write($"INGRESO AL PROCESO | {_syncType}{extra}");
            }

            public void MarkFailed(Exception ex)
            {
                _failed = true;
                var extra = FormatExtra(_detail);
                Write($"FALLO | {_syncType}{extra} | {ex.GetType().Name}: {ex.Message}");
            }

            public void Dispose()
            {
                if (_disposed)
                    return;
                _disposed = true;
                _sw.Stop();
                var extra = FormatExtra(_detail);
                var status = _failed ? "FALLO" : "OK";
                Write($"FIN | {_syncType}{extra} | {status} | {_sw.ElapsedMilliseconds} ms");
            }
        }
    }
}
