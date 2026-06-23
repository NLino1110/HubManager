using CommunityToolkit.Maui.Alerts;
using DMOrders.Services.Promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMOrders.Pages.Fragments.Orders.modals;

public partial class PromocionesViewer : ContentView
{
    public PromocionesViewer(sale_order SaleOrderParam)
	{
		InitializeComponent();
        BindingContext = this;
        SaleOrder = SaleOrderParam;
        productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
        realApplied = new List<sale_order_line>();
        wholeRealApplied = new List<sale_order_line>();
        SaleOrdersLinesTmp = SaleOrder.order_line;

        CachedPreselected();

        if(App.Session.odooConnection.IsTestMode)
        {
            btnApplyAndContinue.IsVisible = true;
        }

        BenefitsForShow = true;
    }

    private void CachedPreselected()
    {
        var giftLineForAdd = SaleOrdersLinesTmp
            .Select(x => (sale_order_line)x[2])
            .Where(l => l.is_gift && l.is_manual)
            .ToList();

        if (giftLineForAdd.Any())
        {
            foreach (var item in giftLineForAdd)
            {
                wholeRealApplied.Add(item);
            }
        }
    }

    [Obsolete("Se va a seleccionar Rule desde ahora")]
    private void AutoselectManualPromotion()
    {
        var benefit = _itemsFullPromos?
            .Where(p => p.Items != null)
            .SelectMany(p => p.Items)
            .FirstOrDefault(b =>
                b.Promotion._promotion_type_id == 2 &&
                b.Promotion._selection_type_id == 2);

        if (benefit != null)
        {
            ListaDetallesPromocion.SelectedItem = benefit;
        }
    }

    private void AutoselectManualPromotionRule()
    {
        var benefitRule = _ItemsDataBenefitsRules?            
            .FirstOrDefault(b =>
                b.promotion_type_id == 2 &&
                b.selection_type_id == 2);

        if (benefitRule != null)
        {
            ListaDetallesPromocion.SelectedItem = benefitRule;
        }
    }

    public async Task AutoApplyPromotion()
    {
        foreach (var promotionEvalResult in _itemsFullPromos)
        {
            if (promotionEvalResult.Items != null)
            {
                foreach (var benefit in promotionEvalResult.Items)
                {
                    var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

                    var promoItems = _itemsFullPromos
                        .SelectMany(x => x.Items)
                        .Where(x => x.Promotion.id == benefit.Promotion.id)
                        .ToList();

                    if (benefit.Promotion._promotion_type_id == 2)
                    {
                        if (benefit.Promotion._selection_type_id == 1)
                            await HandleGiftAutoPromotion(promoItems, productDb);
                        //else if (benefit.Promotion._selection_type_id == 2)
                        //    await HandleGiftAutoPromotion(promoItems, productDb);
                    }
                    else if (benefit.Promotion._promotion_type_id == 6) // DESCUENTO
                    {
                        HandleDiscountPromotion(promoItems);
                    }
                }
            }
        }
    }
        

    private readonly SemaphoreSlim _btnLock = new(1, 1);

    private async void AddGiftAndSave(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.BindingContext is not product_product product)
            return;

        await _btnLock.WaitAsync();
        button.IsEnabled = false;

        try
        {
            bool ShouldSaveToo = false;
            IsLoading = true;

            product.qty_gift_virtual++;
            await ChangeQtyEvent(product, ShouldSaveToo);

            IsLoading = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error AddGiftAndSave: {ex}");
        }
        finally
        {
            _btnLock.Release();
            button.IsEnabled = true;
        }
    }

    private async void SubstractGift(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.BindingContext is not product_product product)
            return;

        await _btnLock.WaitAsync();
        button.IsEnabled = false;
        try
        {
            bool ShouldSaveToo = false;

            if (product.qty_gift_virtual <= 0)
                return;

            product.qty_gift_virtual--;

            await ChangeQtyEvent(product, ShouldSaveToo);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error SubstractGift: {ex}");
        }
        finally
        {
            _btnLock.Release();
            button.IsEnabled = true;
        }
    }

    public void ApplyFilterGifts(string text)
    {
        if(promoGifts == null || promoGifts.Count == 0)
        {
            promoGiftsFiltered = new ObservableCollection<product_product>();
            OnPropertyChanged(nameof(promoGiftsFiltered));
            return;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            promoGiftsFiltered = new ObservableCollection<product_product>(promoGifts);
        }
        else
        {
            promoGiftsFiltered = new ObservableCollection<product_product>(
                promoGifts.Where(x => x.display_name.Contains(text, StringComparison.OrdinalIgnoreCase))
            );
        }

        OnPropertyChanged(nameof(promoGiftsFiltered));
    }
}