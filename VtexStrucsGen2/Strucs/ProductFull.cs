using System;
using System.Collections.Generic;
using System.Text;

namespace VtexStrucs.Strucs
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Dimension
    {
        public double cubicweight { get; set; }
        public double height { get; set; }
        public double length { get; set; }
        public double weight { get; set; }
        public double width { get; set; }
    }

    public class RealDimension
    {
        public double realCubicWeight { get; set; }
        public double realHeight { get; set; }
        public double realLength { get; set; }
        public double realWeight { get; set; }
        public double realWidth { get; set; }
    }

    public class Option
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PriceName { get; set; }
        public double ListPrice { get; set; }
        public double Price { get; set; }
    }

    public class Service
    {
        public int Id { get; set; }
        public int ServiceTypeId { get; set; }
        public string Name { get; set; }
        public bool IsFile { get; set; }
        public bool IsGiftCard { get; set; }
        public bool IsRequired { get; set; }
        public List<Option> Options { get; set; }
        public List<object> Attachments { get; set; }
    }

    public class SkuSeller
    {
        public string SellerId { get; set; }
        public int StockKeepingUnitId { get; set; }
        public string SellerStockKeepingUnitId { get; set; }
        public bool IsActive { get; set; }
        public double FreightCommissionPercentage { get; set; }
        public double ProductCommissionPercentage { get; set; }
    }

    public class Image
    {
        public string ImageUrl { get; set; }
        public string ImageName { get; set; }
        public int FileId { get; set; }
    }

    public class PositionsInClusters
    {
        public int _140 { get; set; }
        public int _141 { get; set; }
        public int _157 { get; set; }
        public int _164 { get; set; }
        public int _166 { get; set; }
        public int _176 { get; set; }
        public int _178 { get; set; }
        public int _186 { get; set; }
        public int _187 { get; set; }
        public int _200 { get; set; }
    }

    public class ProductClusterNames
    {
        public string _140 { get; set; }
        public string _141 { get; set; }
        public string _157 { get; set; }
        public string _164 { get; set; }
        public string _166 { get; set; }
        public string _176 { get; set; }
        public string _178 { get; set; }
        public string _186 { get; set; }
        public string _187 { get; set; }
        public string _200 { get; set; }
    }

    public class ProductClusterHighlights
    {
    }

    public class ProductCategories
    {
        public string _51 { get; set; }
        public string _16 { get; set; }
        public string _3 { get; set; }
    }

    public class AlternateIds
    {
        public string RefId { get; set; }
    }

    public class ProductFull
    {
        public string account_name { get; set; }
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string NameComplete { get; set; }
        public string ComplementName { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductRefId { get; set; }
        public string TaxCode { get; set; }
        public string SkuName { get; set; }
        public bool IsActive { get; set; }
        public bool IsTransported { get; set; }
        public bool IsInventoried { get; set; }
        public bool IsGiftCardRecharge { get; set; }
        public string ImageUrl { get; set; }
        public string DetailUrl { get; set; }
        public object CSCIdentification { get; set; }
        public string BrandId { get; set; }
        public string BrandName { get; set; }
        public bool IsBrandActive { get; set; }
        public Dimension Dimension { get; set; }
        public RealDimension RealDimension { get; set; }
        public string ManufacturerCode { get; set; }
        public bool IsKit { get; set; }
        public List<object> KitItems { get; set; }
        public List<Service> Services { get; set; }
        public List<object> Categories { get; set; }
        public List<string> CategoriesFullPath { get; set; }
        public List<object> Attachments { get; set; }
        public List<object> Collections { get; set; }
        public List<SkuSeller> SkuSellers { get; set; }
        public List<int> SalesChannels { get; set; }
        public List<Image> Images { get; set; }
        public List<object> Videos { get; set; }
        public List<object> SkuSpecifications { get; set; }
        public List<object> ProductSpecifications { get; set; }
        public string ProductClustersIds { get; set; }
        public PositionsInClusters PositionsInClusters { get; set; }
        public ProductClusterNames ProductClusterNames { get; set; }
        public ProductClusterHighlights ProductClusterHighlights { get; set; }
        public string ProductCategoryIds { get; set; }
        public bool IsDirectCategoryActive { get; set; }
        public int ProductGlobalCategoryId { get; set; }
        //public ProductCategories ProductCategories { get; set; }
        //public Object ProductCategories { get; set; }
        public Dictionary<string, string> ProductCategories { get; set; } = new();
        public int CommercialConditionId { get; set; }
        public double RewardValue { get; set; }
        public AlternateIds AlternateIds { get; set; }
        public List<string> AlternateIdValues { get; set; }
        public object EstimatedDateArrival { get; set; }
        public string MeasurementUnit { get; set; }
        public double UnitMultiplier { get; set; }
        public string InformationSource { get; set; }
        public object ModalType { get; set; }
        public string KeyWords { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool ProductIsVisible { get; set; }
        public bool ShowIfNotAvailable { get; set; }
        public bool IsProductActive { get; set; }
        public int ProductFinalScore { get; set; }
    }


}
