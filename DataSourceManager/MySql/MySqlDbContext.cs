using DataSourceManager.MySql.Struct;
using DataSourceManager.Tools;
//using Entidades.Externals;
//using Entidades.Security;
//using Entidades.SyncTask;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataSourceManager.MySql
{
    public partial class MySqlDbContext : DbContext
    {
        public MySqlDbContext()
        {

        }

        public MySqlDbContext(DbContextOptions<MySqlDbContext> options): base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.EnableDetailedErrors(true);
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

            modelBuilder.Entity<ProductSync>()
            .HasKey(a => new { a.prod_vtex_id, a.prod_vtex_sku, a.account_name });

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
        }
    }
}