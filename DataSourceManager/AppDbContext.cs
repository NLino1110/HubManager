using DataSourceManager.Tools;
using Microsoft.EntityFrameworkCore;
using Models.DMSA.Mbw.Core;
//using Entidades.Externals;
//using Entidades.Security;
//using Entidades.SyncTask;
//using Models.DMSA.Mbw.Clientes;
//using Models.DMSA.Mbw.Especiales;
using Models.DMSA.Mbw.Inventario;
using Models.DMSA.Mbw.Sales;
using Models.DMSA.Shared.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
namespace DataSourceManager
{

    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {

        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            //Debug.WriteLine("");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //DataConnection connection = new DataConnection();
            ////string Driver = ConfigurationHelper.GetValue($"DataServers:Mainapp:Driver");

            //string Driver = ConfigurationHelper.GetAppSettings().profile.DataServers[0].Driver;

            //switch (Driver.ToLower())
            //{
            //    case "mysql":
            //        {
            //            optionsBuilder.UseMySql(
            //                connection.GetConnectionString("Mainapp"),
            //                new MySqlServerVersion(new Version(8, 0, 21))
            //            );
            //        }
            //        break;
            //    case "oracle":
            //        {
            //            //TODO:
            //            //Parametrizar la versión de Oracle desde el appsettings.json
            //            optionsBuilder.UseOracle(connection.GetConnectionString("Mainapp"), 
            //                b => b.UseOracleSQLCompatibility("11"));            
            //        }
            //        break;
            //}

            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsBuilder.EnableDetailedErrors(true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            //modelBuilder.Entity<User>().Ignore(t => t.refreshTokens);
            //modelBuilder.Entity<User>().MapToStoredProcedures();
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Role>().Metadata.GetSeedData(true);

            //modelBuilder.Entity<User>(entity =>
            //{
            //    entity.HasOne(u => u.role).
            //    WithMany(y => y.Users).
            //    HasForeignKey(f => f.roleId);
            //});

            //modelBuilder.Entity<Role>().

            //modelBuilder.Entity<User>()
            //    .Navigation(b => b.role).
            //    IsRequired(false).
            //    AutoInclude().
            //    UsePropertyAccessMode( PropertyAccessMode.Property );

            //modelBuilder.Entity<ProductSync>()
            //.HasKey(a => new { a.prod_vtex_id, a.prod_vtex_sku, a.account_name });

            //modelBuilder.Entity<NotaCreditoDet>()
            //.HasKey(a => new { a.CODAGENCIA, 
            //    a.CODTIPOCMPR, 
            //    a.NUMCMPRVENTA,
            //    a.NUMCMPRVENTADET
            //});

            modelBuilder.Entity<GenParametros>()
            .HasKey(a => new {
                a.CodEmpresa,
                a.CodParametro
            });

            modelBuilder.Entity<GenMarca>()
            .HasKey(gm => new { 
                gm.CodMarca, 
                gm.CodEmpresaMarca 
            });

            modelBuilder.Entity<FacNivelesPrecios>()
            .HasKey(gm => new {
                gm.CodEmpresa,
                gm.CodNivel
            });

            modelBuilder.Entity<GenVendedores>()
            .HasKey(gm => new {
                gm.CodEmpresa,
                gm.CodVendedor
            });

            modelBuilder.Entity<Faccmprventa>()
            .HasKey(gm => new {
                gm.CodAgencia,
                gm.CodTipoCmpr,
                gm.NumCmprVenta
            });

            modelBuilder.Entity<FacPromociones>()
            .HasKey(gm => new {
                gm.CodEmpresa,
                gm.CodPromocion
            });

            modelBuilder.Entity<FacRangoDescuentoLote>()
            .HasKey(gm => new {
                gm.CodRangoLote,
                gm.CodEmpresa
            });

            modelBuilder.Entity<FacPedido>()
            .HasKey(gm => new {
                gm.CodAgencia,
                gm.TipoPedido,
                gm.NumPedido
            });

            modelBuilder.Entity<FacPedidoDet>()
            .HasKey(gm => new {
                gm.CodAgencia,
                gm.TipoPedido,
                gm.NumPedido,
                gm.NumPedidoDet
            });

            modelBuilder.Entity<InvStock>()
            .HasKey(gm => new {
                gm.CodBodegaAgencia,
                gm.CodArticulo
            });

            modelBuilder.Entity<InvstockLotes>()
            .HasKey(gm => new {
                gm.CodEmpresa,
                gm.CodBodegaAgencia,
                gm.CodArticulo,
                gm.CodigoLote
            });

            modelBuilder.Entity<CajCajas>()
            .HasKey(gm => new {
                gm.CodAgencia,
                gm.NumCaja
            });

            //modelBuilder.Entity<ArticulosXEmpresa>()
            //.HasKey(a => new { a.CodArticuloEmpresa, a.CodMarca, a.CodEmpresaMarca });
            
            //modelBuilder.Entity<ArticulosXEmpresa>()
            //    .HasOne(a => a.Articulo)
            //    .WithMany()
            //    .HasForeignKey(a => new { a.CodArticulo });

            modelBuilder.Entity<ArticulosXEmpresa>()
                .HasOne(a => a.Marca)
                .WithMany()
                .HasForeignKey(a => new { a.CodMarca, a.CodEmpresaMarca });

            modelBuilder.Entity<FacPreciosVenta>()
                .HasKey(pv => new { pv.CodAgencia, pv.NumPrecioVenta });

            modelBuilder.Entity<FacPreciosAlmacen>()
                .HasKey(pb => new { pb.CodEmpresa, pb.CodArticulo , pb.CodEmpresaNivel, pb.CodNivel, pb.CodUnidadMedida});

            modelBuilder.Entity<FacBonificadosXArticulo>(entity =>
            {
                entity.HasOne(d => d.FacNivelesPreciosFk)
                      .WithMany(p => p.FacBonificadosxArticuloFk)
                      .HasForeignKey(d => new { d.CodNivel, d.CodEmpresa })  // 🔹 clave compuesta
                      .HasPrincipalKey(p => new { p.CodNivel, p.CodEmpresa }); // 🔹 clave compuesta en FacNivelesPrecios
            });

            //modelBuilder.Entity<ClienteAprobacion>()
            //.HasKey(a => new { a.I, a.prod_vtex_sku, a.account_name });

            //DbContextOptionsBuilder.EnableSensitiveDataLogging
            //modelBuilder.Entity<SkuServiceValue>().
            //HasKey(a => new { a.Id, a.SkuServiceTypeId });
            //.UsePropertyAccessMode(PropertyAccessMode.Property);               

            //modelBuilder.Entity<Saldo>().HasData(
            //    new List<Saldo>()
            //    {
            //        new Saldo(){ cid="0919826951", valorPagar= 666.77m, fechaMaxPago= DateTime.Now, estado="BUENO"  },
            //        new Saldo(){ cid="0919826941", valorPagar= 777.66m, fechaMaxPago= DateTime.Now, estado="MALO"  },
            //    }
            //    );


            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties()
                    .Where(p => p.ClrType == typeof(string)))
                {
                    property.SetIsUnicode(false);  // 🔑 Fuerza VARCHAR2
                }
            }
        }
    }
}