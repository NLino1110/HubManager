using DataSourceManager.Tools;
//using Entidades.Externals;
//using Entidades.Security;
//using Entidades.SyncTask;
using Microsoft.EntityFrameworkCore;
using Renci.SshNet;
using ResourceBuilder.Data.Structs.SyncTask;
using System.Diagnostics;

namespace ResourceBuilder.DBContext.PostgreSql
{
    public partial class PostgreSqlContext : DbContext
    {
        private static SshClient _sshClient;
        private static ForwardedPortLocal _portForwarded;

        public PostgreSqlContext()
        {
            EnsureSshTunnel();
        }

        public PostgreSqlContext(DbContextOptions<PostgreSqlContext> options): base(options)
        {
            EnsureSshTunnel();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            //optionsBuilder.EnableDetailedErrors(true);

            if (!optionsBuilder.IsConfigured)
            {
                // Conexión al puerto local del túnel
                var connectionString = "Host=127.0.0.1;Port=5433;Username=postgres;Password=tu_password;Database=mi_basedatos";
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
            
        //    base.OnModelCreating(modelBuilder);

        //    modelBuilder.Entity<ProductSync>()
        //    .HasKey(a => new { a.prod_vtex_id, a.prod_vtex_sku, a.account_name });

        //}

        private void EnsureSshTunnel()
        {
            if (_sshClient != null && _sshClient.IsConnected)
            {
                Debug.WriteLine("Conectado al tunel SSH");
                return;
            }

            string sshHost = "admin.dmujeres.ec";
            int sshPort = 22;
            string sshUser = "proyectos";
            string sshPassword = "MN@m2ddl2w4r3.26";

            // Crear cliente SSH
            _sshClient = new SshClient(sshHost, sshPort, sshUser, sshPassword);
            _sshClient.Connect();

            // Redirigir localhost:5434 -> servidor:5432
            _portForwarded = new ForwardedPortLocal("127.0.0.1", 5434, "127.0.0.1", 5432);
            _sshClient.AddForwardedPort(_portForwarded);
            _portForwarded.Start();
        }

        public override void Dispose()
        {
            base.Dispose();
            _portForwarded?.Stop();
            _sshClient?.Disconnect();
        }
    }
}