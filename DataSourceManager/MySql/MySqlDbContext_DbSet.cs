using DataSourceManager.Tools;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataSourceManager.MySql
{

    public partial class MySqlDbContext : DbContext
    {
        //public virtual DbSet<User> SecurityUser { set; get; }
        //public virtual DbSet<Role> SecurityRole { set; get; }
        //public virtual DbSet<CompanyAccountT> CompanyAccount { set; get; }
        //public virtual DbSet<RefreshToken> SecurityRefreshTokens { get; set; }
    }
}