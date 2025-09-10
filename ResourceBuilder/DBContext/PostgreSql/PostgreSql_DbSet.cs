using DataSourceManager.Tools;
using Microsoft.EntityFrameworkCore;
using ResourceBuilder.Data.Structs.DJango;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceBuilder.DBContext.PostgreSql
{

    public partial class PostgreSqlContext : DbContext
    {
        //public virtual DbSet<User> SecurityUser { set; get; }
        //public virtual DbSet<Role> SecurityRole { set; get; }
        //public virtual DbSet<CompanyAccountT> CompanyAccount { set; get; }
        //public virtual DbSet<RefreshToken> SecurityRefreshTokens { get; set; }


        public DbSet<CatalogPrice> CatalogPrice { get; set; }
        public DbSet<CatalogSku> CatalogSku { get; set; }
        public DbSet<CatalogProduct> CatalogProduct { get; set; }
        public DbSet<CatalogBrand> CatalogBrand { get; set; }
        public DbSet<CatalogCategory> CatalogCategory { get; set; }
        public DbSet<OmsStore> OmsStore { get; set; }
        public DbSet<SettingsMerchant> SettingsMerchant { get; set; }
        public DbSet<SettingsChannel> SettingsChannel { get; set; }
        public DbSet<SettingsIdentity> SettingsIdentity { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SettingsIdentity>(entity =>
            {
                entity.ToTable("Settings_identity", "public");

                entity.HasKey(e => e.Id).HasName("Settings_identity_pkey");

                entity.HasIndex(e => e.App).HasDatabaseName("Settings_identity_app_1e979a4e");
                entity.HasIndex(e => e.ExternalId).HasDatabaseName("idx_settings_identity_external_id");
                entity.HasIndex(e => new { e.App, e.ContentTypeId, e.ObjectId }).HasDatabaseName("idx_settings_identity_lookup");

                entity.Property(e => e.App).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ExternalId).IsRequired().HasMaxLength(50);
            });

            // ================= Catalog_price =================
            modelBuilder.Entity<CatalogPrice>(entity =>
            {
                entity.ToTable("Catalog_price", "public");

                entity.HasKey(e => e.Id)
                      .HasName("Catalog_price_pkey");

                entity.HasIndex(e => e.ModifiedAt).HasDatabaseName("Catalog_price_modified_at_afe35847");
                entity.HasIndex(e => e.SkuId).HasDatabaseName("Catalog_price_sku_id_e6466565");
                entity.HasIndex(e => e.StoreId).HasDatabaseName("Catalog_price_store_id_87e294dd");
                entity.HasIndex(e => new { e.SkuId, e.StoreId }).HasDatabaseName("idx_catalog_price_sku_store");

                entity.Property(e => e.Type).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Status).IsRequired();

                // Relación obligatoria con Sku
                entity.HasOne(e => e.Sku)
                      .WithMany(s => s.Prices)
                      .HasForeignKey(e => e.SkuId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                // Relación opcional con Store
                entity.HasOne(e => e.Store)
                      .WithMany(s => s.Prices)
                      .HasForeignKey(e => e.StoreId)
                      .IsRequired(false)  // <--- Esto permite que store_id sea NULL
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });


            // ================= Catalog_sku =================
            modelBuilder.Entity<CatalogSku>(entity =>
            {
                entity.ToTable("Catalog_sku", "public");

                entity.HasKey(e => e.Id).HasName("Catalog_sku_pkey");

                entity.HasIndex(e => e.ModifiedAt).HasDatabaseName("Catalog_sku_modified_at_42376375");
                entity.HasIndex(e => e.ProductId).HasDatabaseName("Catalog_sku_product_id_a3a04de6");
                entity.HasIndex(e => e.Reference).HasDatabaseName("idx_sku_reference");

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Skus)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // ================= Catalog_product =================
            modelBuilder.Entity<CatalogProduct>(entity =>
            {
                entity.ToTable("Catalog_product", "public");

                entity.HasKey(e => e.Id).HasName("Catalog_product_pkey");

                entity.HasIndex(e => e.BrandId).HasDatabaseName("Catalog_product_brand_id_d6116e2d");
                entity.HasIndex(e => e.CategoryId).HasDatabaseName("Catalog_product_category_id_0383929c");
                entity.HasIndex(e => e.MerchantId).HasDatabaseName("Catalog_product_merchant_id_699feb4f");
                entity.HasIndex(e => e.ModifiedAt).HasDatabaseName("Catalog_product_modified_at_6d14ba82");
                entity.HasIndex(e => e.Reference).HasDatabaseName("idx_product_reference");

                entity.HasOne(e => e.Brand)
                      .WithMany(b => b.Products)
                      .HasForeignKey(e => e.BrandId);

                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(e => e.CategoryId);

                entity.HasOne(e => e.Merchant)
                      .WithMany(m => m.Products)
                      .HasForeignKey(e => e.MerchantId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // ================= Catalog_brand =================
            modelBuilder.Entity<CatalogBrand>(entity =>
            {
                entity.ToTable("Catalog_brand", "public");

                entity.HasKey(e => e.Id).HasName("Catalog_brand_pkey");

                entity.HasIndex(e => e.MerchantId).HasDatabaseName("Catalog_brand_merchant_id_f6ca573d");
                entity.HasIndex(e => e.ModifiedAt).HasDatabaseName("Catalog_brand_modified_at_a33d3c1a");

                entity.HasOne(e => e.Merchant)
                      .WithMany(m => m.Brands)
                      .HasForeignKey(e => e.MerchantId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // ================= Catalog_category =================
            modelBuilder.Entity<CatalogCategory>(entity =>
            {
                entity.ToTable("Catalog_category", "public");

                entity.HasKey(e => e.Id).HasName("Catalog_category_pkey");

                entity.HasIndex(e => e.MainId).HasDatabaseName("Catalog_category_main_id_477d253a");
                entity.HasIndex(e => e.MerchantId).HasDatabaseName("Catalog_category_merchant_id_1e2e586d");
                entity.HasIndex(e => e.ModifiedAt).HasDatabaseName("Catalog_category_modified_at_83b9a4aa");

                entity.HasOne(e => e.MainCategory)
                      .WithMany(c => c.SubCategories)
                      .HasForeignKey(e => e.MainId);

                entity.HasOne(e => e.Merchant)
                      .WithMany(m => m.Categories)
                      .HasForeignKey(e => e.MerchantId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // ================= Settings_merchant =================
            modelBuilder.Entity<SettingsMerchant>(entity =>
            {
                entity.ToTable("Settings_merchant", "public");

                entity.HasKey(e => e.Id).HasName("Settings_merchant_pkey");

                entity.HasIndex(e => e.MainId).HasDatabaseName("Settings_merchant_main_id_29c17d40");
                entity.HasIndex(e => e.Name).HasDatabaseName("Settings_merchant_name_3dea0156_like");

                entity.HasOne(e => e.MainStore)
                      .WithMany()
                      .HasForeignKey(e => e.MainId);

                entity.HasMany(e => e.Channels)
                      .WithOne(c => c.Merchant)
                      .HasForeignKey(c => c.MerchantId);
            });

            // ================= Settings_channel =================
            modelBuilder.Entity<SettingsChannel>(entity =>
            {
                entity.ToTable("Settings_channel", "public");

                entity.HasKey(e => e.Id).HasName("Settings_channel_pkey");

                entity.HasIndex(e => e.MerchantId).HasDatabaseName("Settings_channel_merchant_id_583a8980");

                entity.HasOne(e => e.Merchant)
                      .WithMany(m => m.Channels)
                      .HasForeignKey(e => e.MerchantId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // ================= OMS_store =================
            modelBuilder.Entity<OmsStore>(entity =>
            {
                entity.ToTable("OMS_store", "public");

                entity.HasKey(e => e.Id).HasName("OMS_store_pkey");

                entity.HasIndex(e => e.ChannelId).HasDatabaseName("OMS_store_channel_id_33a72d75");
                entity.HasIndex(e => e.MainId).HasDatabaseName("OMS_store_main_id_ce250551");
                entity.HasIndex(e => e.MerchantId).HasDatabaseName("OMS_store_merchant_id_8d68b876");
                entity.HasIndex(e => e.ModifiedAt).HasDatabaseName("OMS_store_modified_at_ce8999be");

                entity.HasOne(e => e.Channel)
                      .WithMany(c => c.Stores)
                      .HasForeignKey(e => e.ChannelId);

                entity.HasOne(e => e.MainStore)
                      .WithMany(s => s.SubStores)
                      .HasForeignKey(e => e.MainId);

                entity.HasOne(e => e.Merchant)
                      .WithMany(m => m.Stores)
                      .HasForeignKey(e => e.MerchantId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });
        }
    }

}