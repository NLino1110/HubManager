using DataSourceManager.MySql.Struct;
using DataSourceManager.Tools;
//using Entidades.Externals;
//using Entidades.Sac;
//using Entidades.SyncTask;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceBuilder.DBContext.PostgreSql
{

    public partial class PostgreSqlContext : DbContext
    {
        //public virtual DbSet<ProductSync> product_sync { set; get; }
        //public virtual DbSet<SacDocument> SacDocument { set; get; }
    }
}