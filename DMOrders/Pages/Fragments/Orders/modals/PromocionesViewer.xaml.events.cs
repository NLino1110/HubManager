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
        if (!await ConfirmIncompleteManualGiftsIfNeeded())
            return;

        await ApplyPendingDiscountsAsync();

        var resultPopup = new PromoResultPopup();
        resultPopup.benefits = ItemsData.ToList();
        resultPopup.manualGifts = wholeRealApplied?.ToList() ?? new List<sale_order_line>();
        resultPopup.ActionResult = 1;
        ClosePopupAction?.Invoke(resultPopup);
    }

    private async void OnApplyAndContinueButtonClicked(object sender, EventArgs e)
    {
        if (!await ConfirmIncompleteManualGiftsIfNeeded())
            return;

        await ApplyPendingDiscountsAsync();

        var resultPopup = new PromoResultPopup();
        resultPopup.benefits = ItemsData.ToList();
        resultPopup.manualGifts = wholeRealApplied?.ToList() ?? new List<sale_order_line>();
        resultPopup.ActionResult = 2;
        ClosePopupAction?.Invoke(resultPopup);
    }

    /// <summary>
    /// Solo advierte regalos incompletos si la promo seleccionada es regalo manual.
    /// Evita el falso aviso al aplicar solo descuentos.
    /// </summary>
    private async Task<bool> ConfirmIncompleteManualGiftsIfNeeded()
    {
        var selected = selectedPromoRuleEvalItem;
        bool isManualGiftSelected = selected != null
            && selected.promotion_type_id == 2
            && selected.selection_type_id == 2;

        // Descuento u otra promo: no exigir cupo global de regalos
        if (selected != null && !isManualGiftSelected)
            return true;

        if (!isManualGiftSelected)
        {
            bool anyManualGift = _ItemsDataBenefitsRules?.Any(b =>
                b.promotion_type_id == 2 && b.selection_type_id == 2) == true;

            if (!anyManualGift || GlobalTotalManualGiftsAllowed <= 0)
                return true;

            // Sin selección de regalo y sin nada aplicado: no bloquear
            if (GlobalTotalManualGiftsApplied <= 0
                && (promoGifts == null || promoGifts.Sum(p => p.qty_gift) <= 0))
                return true;
        }

        int allowed = isManualGiftSelected
            ? GetSelectedManualGiftAllowed()
            : GlobalTotalManualGiftsAllowed;

        int applied = isManualGiftSelected
            ? (promoGifts?.Sum(p => p.qty_gift) ?? 0)
            : Math.Max(GlobalTotalManualGiftsApplied, promoGifts?.Sum(p => p.qty_gift) ?? 0);

        if (allowed > 0 && applied < allowed)
        {
            var leave = await App.Current.Windows[0].Page.DisplayAlertAsync(
                "¿Desea continuar?",
                $"No se han aplicado todos los {allowed} regalos de los bonificados manuales (aplicados: {applied}).",
                "Si",
                "No");

            return leave;
        }

        return true;
    }

    private int GetSelectedManualGiftAllowed()
    {
        if (selectedPromoRuleEvalItem == null)
            return GlobalTotalManualGiftsAllowed;

        var benefit = _itemsFullPromos?
            .Where(p => p.Items != null)
            .SelectMany(p => p.Items)
            .FirstOrDefault(b =>
                b.Promotion != null
                && b.Promotion.id == selectedPromoRuleEvalItem.promo_id
                && b.Promotion._promotion_type_id == 2
                && b.Promotion._selection_type_id == 2);

        if (benefit != null && benefit.MaxAllowedGifts > 0)
            return benefit.MaxAllowedGifts;

        return selectedPromoRuleEvalItem.AllowedGifts > 0
            ? selectedPromoRuleEvalItem.AllowedGifts
            : GlobalTotalManualGiftsAllowed;
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
            rule.discount = 0;
            await App.Current.Windows[0].Page.DisplayAlertAsync(
                "⚠️ Confirmación requerida",
                "El descuento ingresado supera el máximo permitido (" + rule.discount_base + "%) o es inválido.\n\nSe restableció a 0. Ingrese un valor válido e intente de nuevo.",
                "Aceptar");

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

