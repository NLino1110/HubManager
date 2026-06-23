using CommunityToolkit.Maui.Alerts;
using DMOrders.Services.Promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales.promotions.abstractCustom;
using DMSA.Sync.Core.Database.Sqlite;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMOrders.Pages.Fragments.Orders.modals;

public partial class PromocionesViewer
{


    private async void OnApplyButtonClicked(object sender, EventArgs e)
    {
        if (GlobalTotalManualGiftsApplied < GlobalTotalManualGiftsAllowed)
        {
            var leave = await App.Current.Windows[0].Page.DisplayAlertAsync($"¿Desea continuar?", $"No se han aplicado todos los {GlobalTotalManualGiftsAllowed} regalos de los bonificados manuales", "Si", "No");

            if (!leave)
            {
                return;
            }
        }

        var resultPopup = new PromoResultPopup();
        resultPopup.benefits = ItemsData.ToList();
        resultPopup.manualGifts = wholeRealApplied?.ToList() ?? new List<sale_order_line>();
        resultPopup.ActionResult = 1;
        ClosePopupAction?.Invoke(resultPopup);
    }

    private async void OnApplyAndContinueButtonClicked(object sender, EventArgs e)
    {
        if (GlobalTotalManualGiftsApplied < GlobalTotalManualGiftsAllowed)
        {
            var leave = await App.Current.Windows[0].Page.DisplayAlertAsync($"¿Desea continuar?", $"No se han aplicado todos los {GlobalTotalManualGiftsAllowed} regalos de los bonificados manuales", "Si", "No");

            if (!leave)
            {
                return;
            }
        }

        var resultPopup = new PromoResultPopup();
        resultPopup.benefits = ItemsData.ToList();
        resultPopup.manualGifts = wholeRealApplied?.ToList() ?? new List<sale_order_line>();
        resultPopup.ActionResult = 2;
        ClosePopupAction?.Invoke(resultPopup);
    }


    private async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        var resultPopup = new PromoResultPopup();
        resultPopup.benefits = ItemsData.ToList();
        resultPopup.manualGifts = wholeRealApplied?.ToList() ?? new List<sale_order_line>();
        resultPopup.ActionResult = 0;
        ClosePopupAction?.Invoke(resultPopup);
    }

    private readonly Dictionary<Entry, CancellationTokenSource> _entryTokens = new();

    private async void Qty_Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsLoading)
            return;

        if (sender is not Entry entry)
            return;

        try
        {
            bool ShouldSaveToo = false;

            if (_entryTokens.TryGetValue(entry, out var existingCts))
            {
                existingCts.Cancel();
                existingCts.Dispose();
            }

            var cts = new CancellationTokenSource();
            _entryTokens[entry] = cts;
            var token = cts.Token;

            await Task.Delay(400, token);

            if (token.IsCancellationRequested)
                return;

            if (entry.BindingContext is not product_product product)
                return;

            await ChangeQtyEvent(product, ShouldSaveToo);
        }
        catch (TaskCanceledException)
        {

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error Qty_Entry_TextChanged: {ex}");
        }
    }

    private async void Disc_Entry_Completed(object sender, EventArgs e)
    {
        bool ShouldSaveToo = false;
        Button viewObject = (Button)sender;
        PromoRuleMatch ruleMatch = (PromoRuleMatch)viewObject.BindingContext;
        if (await ChangeDiscountEvent(ruleMatch, false))
            await ApplyDiscountRule(SaleOrder, ruleMatch);
    }

    private async Task<bool> ChangeDiscountEvent(PromoRuleMatch rule, bool ShouldSaveToo)
    {
        if (rule.discount > rule.discount_base || rule.discount < 0)
        {
            rule.discount = rule.discount_base;
            await Toast.Make("El descuento no puede ser mayor a " + rule.discount_base + " ni negativo").Show();

            OnPropertyChanged(nameof(promoDiscounts));
            return false;
        }
        else
        {
            await Toast.Make("El descuento del " + rule.discount + "% aplicado!").Show();
        }
        return true;
    }

    private async Task ChangeQtyEvent(product_product product, bool ShouldSaveToo)
    {
        if (product.qty_gift_virtual < 0)
        {
            product.qty_gift_virtual = product.qty_gift;
            return;
        }

        int previousQty = product.qty_gift;
        int delta = product.qty_gift_virtual - product.qty_gift;

        Debug.WriteLine("ChangeQtyEvent - before UpdateGiftIsolated");
        bool success = await UpdateGiftIsolated(product, ShouldSaveToo, selectedPromoRuleEvalItem);
        Debug.WriteLine("ChangeQtyEvent - pass UpdateGiftIsolated");

        if (!success)
        {
            product.qty_gift_virtual = previousQty;
            OnPropertyChanged(nameof(product.qty_gift_virtual));
        }

        OnPropertyChanged(nameof(ComputeTotal));
        OnPropertyChanged(nameof(ComputeTotalQty));
    }


    private void FiltroArtPromo_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue.Length < 3 && !e.NewTextValue.Trim().Equals(""))
        {
            return;
        }
        ApplyFilterGifts(e.NewTextValue);
    }


    private async void detail_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.Count > 0)
        {
            IsLoading = true;
            var selected = (PromoRuleItem)e.CurrentSelection[0];
            await HandlePromotionSelection(selected);

            await Task.Delay(400);
            IsLoading = false;
        }
    }

    private async void _detail_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.Count > 0)
        {
            IsLoading = true;
            var selected = (PromotionEvalItem)e.CurrentSelection[0];
            await HandlePromotionSelection(selected);

            await Task.Delay(400);
            IsLoading = false;
        }
    }

}

