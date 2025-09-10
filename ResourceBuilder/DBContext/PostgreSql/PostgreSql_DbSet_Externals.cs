using DataSourceManager.Tools;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ResourceBuilder.DBContext.PostgreSql
{

    public partial class PostgreSqlContext : DbContext
    {
        //public virtual DbSet<Saldo> SacSaldo { set; get; }

        //public virtual DbSet<SacDocument> SacDocument { set; get; }

        //public virtual DbSet<CustomerData> CustomerData { set; get; }
        
        //public virtual DbSet<SkuServiceValue> skuServiceValue { set; get; }
        //public virtual DbSet<SkuService> skuService { set; get; }

        ////Extras temporales
        //public virtual DbSet<Tmp_004> tmp_004 { set; get; }
    }
}