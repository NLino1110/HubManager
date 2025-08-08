using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VtexStrucsGen2.Strucs.Vtex.ResponseModels
{
    public class ProductSearch
    {
        //https://bosque.vtexcommercestable.com.br/api/catalog_system/pub/products/search/?ft=combo

        
            public string productId { get; set; }
            public string productName { get; set; }
            public string brand { get; set; }
            public int brandId { get; set; }
            public object brandImageUrl { get; set; }
            public string linkText { get; set; }
            public string productReference { get; set; }
            public object productReferenceCode { get; set; }
            public string categoryId { get; set; }
            public string productTitle { get; set; }
            public string metaTagDescription { get; set; }
            public DateTime releaseDate { get; set; }
            public Clusterhighlights clusterHighlights { get; set; }
            public Productclusters productClusters { get; set; }
            public Searchableclusters searchableClusters { get; set; }
            public string[] categories { get; set; }
            public string[] categoriesIds { get; set; }
            public string link { get; set; }
            public string description { get; set; }
            public Item[] items { get; set; }
        

        public class Clusterhighlights
        {
            public string _167 { get; set; }
        }

        public class Productclusters
        {
            public string _140 { get; set; }
            public string _152 { get; set; }
            public string _157 { get; set; }
            public string _167 { get; set; }
            public string _176 { get; set; }
            public string _186 { get; set; }
            public string _189 { get; set; }
            public string _193 { get; set; }
            public string _205 { get; set; }
            public string _208 { get; set; }
            public string _153 { get; set; }
            public string _166 { get; set; }
            public string _200 { get; set; }
            public string _141 { get; set; }
            public string _164 { get; set; }
        }

        public class Searchableclusters
        {
            public string _157 { get; set; }
            public string _167 { get; set; }
        }

        public class Item
        {
            public string itemId { get; set; }
            public string name { get; set; }
            public string nameComplete { get; set; }
            public string complementName { get; set; }
            public string ean { get; set; }
            public Referenceid[] referenceId { get; set; }
            public string measurementUnit { get; set; }
            public float unitMultiplier { get; set; }
            public object modalType { get; set; }
            public bool isKit { get; set; }
            public Image[] images { get; set; }
            public Seller[] sellers { get; set; }
            public object[] Videos { get; set; }
            public object estimatedDateArrival { get; set; }
        }

        public class Referenceid
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public class Image
        {
            public string imageId { get; set; }
            public string imageLabel { get; set; }
            public string imageTag { get; set; }
            public string imageUrl { get; set; }
            public string imageText { get; set; }
            public DateTime imageLastModified { get; set; }
        }

        public class Seller
        {
            public string sellerId { get; set; }
            public string sellerName { get; set; }
            public string addToCartLink { get; set; }
            public bool sellerDefault { get; set; }
            public Commertialoffer commertialOffer { get; set; }
        }

        public class Commertialoffer
        {
            public Deliveryslasamplesperregion DeliverySlaSamplesPerRegion { get; set; }
            public Installment1[] Installments { get; set; }
            public object[] DiscountHighLight { get; set; }
            public object[] GiftSkuIds { get; set; }
            public object[] Teasers { get; set; }
            public object[] BuyTogether { get; set; }
            public object[] ItemMetadataAttachment { get; set; }
            public float Price { get; set; }
            public float ListPrice { get; set; }
            public float PriceWithoutDiscount { get; set; }
            public float RewardValue { get; set; }
            public DateTime PriceValidUntil { get; set; }
            public int AvailableQuantity { get; set; }
            public bool IsAvailable { get; set; }
            public float Tax { get; set; }
            public Deliveryslasample[] DeliverySlaSamples { get; set; }
            public object GetInfoErrorMessage { get; set; }
            public string CacheVersionUsedToCallCheckout { get; set; }
            public Paymentoptions PaymentOptions { get; set; }
        }

        public class Deliveryslasamplesperregion
        {
            public _0 _0 { get; set; }
        }

        public class _0
        {
            public object[] DeliverySlaPerTypes { get; set; }
            public object Region { get; set; }
        }

        public class Paymentoptions
        {
            public Installmentoption[] installmentOptions { get; set; }
            public Paymentsystem[] paymentSystems { get; set; }
            public object[] payments { get; set; }
            public object[] giftCards { get; set; }
            public object[] giftCardMessages { get; set; }
            public object[] availableAccounts { get; set; }
            public object[] availableTokens { get; set; }
        }

        public class Installmentoption
        {
            public string paymentSystem { get; set; }
            public object bin { get; set; }
            public string paymentName { get; set; }
            public string paymentGroupName { get; set; }
            public int value { get; set; }
            public Installment[] installments { get; set; }
        }

        public class Installment
        {
            public int count { get; set; }
            public bool hasInterestRate { get; set; }
            public int interestRate { get; set; }
            public int value { get; set; }
            public int total { get; set; }
            public Sellermerchantinstallment[] sellerMerchantInstallments { get; set; }
        }

        public class Sellermerchantinstallment
        {
            public string id { get; set; }
            public int count { get; set; }
            public bool hasInterestRate { get; set; }
            public int interestRate { get; set; }
            public int value { get; set; }
            public int total { get; set; }
        }

        public class Paymentsystem
        {
            public int id { get; set; }
            public string name { get; set; }
            public string groupName { get; set; }
            public object validator { get; set; }
            public string stringId { get; set; }
            public string template { get; set; }
            public bool requiresDocument { get; set; }
            public bool isCustom { get; set; }
            public string description { get; set; }
            public bool requiresAuthentication { get; set; }
            public DateTime dueDate { get; set; }
            public object availablePayments { get; set; }
        }

        public class Installment1
        {
            public float Value { get; set; }
            public float InterestRate { get; set; }
            public float TotalValuePlusInterestRate { get; set; }
            public int NumberOfInstallments { get; set; }
            public string PaymentSystemName { get; set; }
            public string PaymentSystemGroupName { get; set; }
            public string Name { get; set; }
        }

        public class Deliveryslasample
        {
            public object[] DeliverySlaPerTypes { get; set; }
            public object Region { get; set; }
        }

    }
}
