using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales.promotions.abstractCustom;
using DMSA.Sync.Core.Database.Sqlite;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMOrders.Pages.Fragments.Orders.modals;

public partial class PromocionesViewer
{
    /// <summary>
    /// Regalos manuales: una línea por (product_id, promo_id).
    /// Revertir: false y/o git checkout de prepare.cs + UpdateGiftIsolated en items.cs.
    /// </summary>
    private const bool ManualGiftsSeparateLinePerPromo = true;

    /// <summary>
    /// Regalos automáticos (bonificación auto y NxN): una línea por (product_id, promo_id, rule_id).
    /// Con false vuelven a fusionarse en una sola línea por producto, que perdía cantidades
    /// (la segunda promo hacía qty++ en vez de aportar sus propios regalos) y dejaba
    /// origin_gift_line_ids_offline con el total_allowed_gifts de la primera promo.
    /// </summary>
    private const bool AutoGiftsSeparateLinePerPromo = true;

    private int GlobalTotalManualGiftsAllowed = 0;
    private int GlobalTotalManualGiftsApplied = 0;
    public int GlobalTotalManualGiftsForRemove = 0;
    public int GlobalTotalManualGiftsRemoved = 0;

    public bool RequiredRemoveItems
    {
        get
        {
            return GlobalTotalManualGiftsForRemove > GlobalTotalManualGiftsRemoved;
        }
    }

    public int TotalGiftsForRemove
    {
        get
        {
            return GlobalTotalManualGiftsForRemove;
        }
    }

    public int TotalGiftsRemoved
    {
        get
        {
            return GlobalTotalManualGiftsRemoved;
        }
    }

    private List<sale_order_line> realApplied { get; set; }
    private List<sale_order_line> wholeRealApplied { get; set; }
    private List<OrderLineWrapper> SaleOrdersLinesTmp { get; set; }

    ProductProductDb productDb { get; set; }

    public List<SaleOrderPromotions> saleOrderPromotions { get; set; }

    public ObservableCollection<product_product> promoGiftsAuto
    {
        get => _promoGiftsAuto;
        set
        {
            _promoGiftsAuto = value;
            OnPropertyChanged(nameof(promoGiftsAuto));
        }
    }

    public bool BenefitsForShow { get; set; }

    private ObservableCollection<product_product> _promoGiftsAuto;

    private sale_order SaleOrder { get; set; }
    private PromotionEvalItem selectedPromoEvalItem { get; set; }

    private PromoRuleItem selectedPromoRuleEvalItem { get; set; }

    public ObservableCollection<PromotionEvalResult> ItemsData
    {
        get => _itemsFullPromos;
        set
        {
            _itemsFullPromos = value;

            if (_ItemsDataBenefits != null)
            {
                _ItemsDataBenefits.Clear();
                _promoGiftsAuto.Clear();
            }
            else
            {
                _ItemsDataBenefits = new ObservableCollection<PromotionEvalItem>();
                _promoGiftsAuto = new ObservableCollection<product_product>();
                _ItemsDataBenefitsRules = new ObservableCollection<PromoRuleItem>();
                //_ItemsDataBenefits = new ObservableCollection<PromotionBenefit>();
            }

            OnPropertyChanged(nameof(ItemsData));
            OnPropertyChanged(nameof(ItemsDataBenefits));
            OnPropertyChanged(nameof(ItemsDataBenefitsRules));
            OnPropertyChanged(nameof(ComputeTotal));
            OnPropertyChanged(nameof(ComputeTotalQty));
        }
    }

    public ObservableCollection<PromoRuleItem> ItemsDataBenefitsRules
    {
        get => _ItemsDataBenefitsRules;
        set
        {
            _ItemsDataBenefitsRules = value;
            OnPropertyChanged(nameof(ItemsDataBenefitsRules));
        }
    }

    private ObservableCollection<PromoRuleItem> _ItemsDataBenefitsRules;


    private ObservableCollection<PromotionEvalResult> _itemsFullPromos;

    public ObservableCollection<PromotionEvalItem> ItemsDataBenefits
    {
        get => _ItemsDataBenefits;
        set
        {
            _ItemsDataBenefits = value;
            OnPropertyChanged(nameof(ItemsDataBenefits));
        }
    }

    private ObservableCollection<PromotionEvalItem> _ItemsDataBenefits;

    public ObservableCollection<product_product> promoGifts
    {
        get => _promoGifts;
        set
        {
            _promoGifts = value;
            promoGiftsFiltered = _promoGifts;
            OnPropertyChanged(nameof(promoGifts));
            OnPropertyChanged(nameof(promoGiftsFiltered));
        }
    }

    private ObservableCollection<product_product> _promoGifts;

    public ObservableCollection<product_product> promoGiftsFiltered { get; set; }

    public ObservableCollection<PromoRuleMatch> promoDiscounts { get; set; }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged(nameof(IsLoading));
            Debug.WriteLine("_isLoading");
            Debug.WriteLine(_isLoading);
        }
    }

    private bool _editQty;
    public bool EditQty
    {
        get => _editQty;
        set
        {
            _editQty = value;
            OnPropertyChanged(nameof(EditQty));
        }
    }

    public int PromosCount
    {
        get => ItemsData?.Count ?? 0;
    }


    public decimal ComputeTotal
    {
        get
        {
            if (realApplied == null) return 0m;

            decimal total = 0m;
            foreach (var a in realApplied)
            {
                total += a.product_uom_qty_real * (decimal)a.virtual_price_no_tax;
            }
            return total;
        }
    }

    public decimal ComputeTotalQty
    {
        get
        {
            if (realApplied == null) return 0m;

            decimal total = 0m;
            foreach (var a in realApplied)
            {
                total += (decimal)a.product_uom_qty_real;
            }
            return total;
        }
    }


    public Action<PromoResultPopup> ClosePopupAction { get; set; }
    public ObservableCollection<sale_order_line> OrderLines { get; internal set; }
}
