using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using DMOrders.Controls;
using DMOrders.Controls.Tools;
using DMOrders.Shared;
using DMSA.Models.Odoo.Tareas;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using DMSA.Sync.Core.Database.Sqlite.tareas;
using DMSA.Sync.Core.Update.Pusher;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Activities;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Details : ContentPage, IBackButtonHandler
{
    public ProjectTask CurrentProjectTask { get; set; }
    public ICommand EditCommand { get; set; }
    public ICommand DeleteCommand { get; set; }

    public Details(ProjectTask _CurrentActivityHeader)
    {
        InitializeComponent();

        CurrentProjectTask = _CurrentActivityHeader;

        if (CurrentProjectTask != null)
        {
            lblMainTitle.Text = $"Actividades Diarias No. {CurrentProjectTask.id}   Fecha {CurrentProjectTask.name}";
        }

        BindingContext = new DetailsViewModel(CurrentProjectTask);
        ConfigureCommandsAndFooter();
    }

    private void ConfigureCommandsAndFooter()
    {
        if (IsTaskFullySynced())
        {
            EditCommand = null;
            DeleteCommand = null;
        }
        else
        {
            EditCommand = new Command(EditItem);
            DeleteCommand = new Command(async (obj) => await DeleteItemAsync(obj));
        }

        ApplyFooterState();
        UpdateSyncMessageLabel();
    }

    private bool IsTaskFullySynced() =>
        CurrentProjectTask?.IsFullySynced == true;

    private bool CanReprocess()
    {
        if (CurrentProjectTask == null)
            return false;

        if (CurrentProjectTask.HasPendingDetails)
            return true;

        return string.Equals(CurrentProjectTask.sync_status, ProjectTaskSyncStatus.Partial, StringComparison.OrdinalIgnoreCase)
            || string.Equals(CurrentProjectTask.sync_status, ProjectTaskSyncStatus.Error, StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyFooterState()
    {
        bool fullySynced = IsTaskFullySynced();
        bool canReprocess = CanReprocess();

        if (btnNuevo != null)
            btnNuevo.IsVisible = !fullySynced;

        if (btnEnviar != null)
            btnEnviar.IsVisible = !fullySynced && !canReprocess;

        if (btnReprocesar != null)
            btnReprocesar.IsVisible = !fullySynced && canReprocess;
    }

    private void UpdateSyncMessageLabel()
    {
        if (lblSyncMessage == null || CurrentProjectTask == null)
            return;

        var message = CurrentProjectTask.sync_message?.Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            lblSyncMessage.IsVisible = false;
            lblSyncMessage.Text = string.Empty;
            return;
        }

        lblSyncMessage.Text = message;
        lblSyncMessage.IsVisible = true;
    }

    private async Task ReloadCurrentTaskAsync()
    {
        var dbName = App.Session?.odooConnection?.DbNameSqlite;
        if (CurrentProjectTask == null || string.IsNullOrWhiteSpace(dbName))
            return;

        var projectDb = new ProjectTaskDb(dbName);
        var refreshed = await projectDb.GetItem(CurrentProjectTask.id);
        if (refreshed == null)
            return;

        CurrentProjectTask = refreshed;
        ConfigureCommandsAndFooter();
    }

    private async void EditItem(object obj)
    {
        if (CurrentProjectTask != null && IsTaskFullySynced())
        {
            await DisplayAlertAsync("Atenci�n", "No se puede editar una actividad sincronizada.", "Aceptar");
            return;
        }

        var ItemForEdit = obj as AccountAnalyticLine;
        if (ItemForEdit == null)
            return;

        if (ItemForEdit.is_synchronized
            && ProjectTaskSyncValidation.IsLineEffectivelySynced(ItemForEdit, CurrentProjectTask?.id_sync ?? 0))
        {
            await DisplayAlertAsync(
                "Atenci�n",
                "No se puede editar un detalle que ya fue sincronizado con el ERP.",
                "Aceptar");
            return;
        }

        PopupSizeConstants popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);

        var returnResultPopup = new PopupAccountAnalyticLine(popupSizeConstants, CurrentProjectTask, ItemForEdit);
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;
        await this.ShowPopupAsync(returnResultPopup);

        await ((DetailsViewModel)BindingContext).PublicLoadActivities();
    }

    public async Task<bool> OnBackButtonPressedAsync()
    {
        bool result = await DisplayAlertAsync("Confirmaci�n", "Minimizar la aplicaci�n, �Desea continuar?", "S�", "No");
        if (result)
        {
#if ANDROID
            Platform.CurrentActivity?.MoveTaskToBack(true);
#endif
        }
        return !result;
    }

    protected override bool OnBackButtonPressed()
    {
        Dispatcher.Dispatch(async () =>
        {
            await Navigation.PopModalAsync();
        });

        return true;
    }

    private async void ButtonClose_Clicked(object sender, EventArgs e)
    {
        SendBackButtonPressed();
    }

    private async void ButtonNew_Clicked(object sender, EventArgs e)
    {
        if (CurrentProjectTask != null && IsTaskFullySynced())
        {
            await DisplayAlertAsync("Atenci�n", "No puede agregar nuevas actividades a una tarea sincronizada.", "Aceptar");
            return;
        }

        PopupSizeConstants popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        var returnResultPopup = new PopupAccountAnalyticLine(popupSizeConstants, CurrentProjectTask, null);
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        await this.ShowPopupAsync(returnResultPopup);

        await ((DetailsViewModel)BindingContext).PublicLoadActivities();
    }

    private async void ButtonSave_Clicked(object sender, EventArgs e)
    {
        await Toast.Make("Datos almacenados").Show();
        await Navigation.PopModalAsync();
    }

    private async void ButtonSync_Clicked(object sender, EventArgs e)
    {
        await ExecuteSendAsync(allowAutoRetry: true, confirmTitle: "Enviar", confirmMessage: "�Desea enviar esta actividad al ERP?");
    }

    private async void ButtonReprocess_Clicked(object sender, EventArgs e)
    {
        await ExecuteSendAsync(allowAutoRetry: false, confirmTitle: "Reprocesar", confirmMessage: "�Desea reprocesar los detalles pendientes?");
    }

    private async Task ExecuteSendAsync(bool allowAutoRetry, string confirmTitle, string confirmMessage)
    {
        var dbName = App.Session?.odooConnection?.DbNameSqlite;
        if (CurrentProjectTask == null || string.IsNullOrWhiteSpace(dbName))
        {
            await DisplayAlertAsync(
                "Atenci�n",
                "Es necesario agregar actividades para realizar el env�o al ERP debido a que actualmente se encuentra vac�o.",
                "Aceptar");
            return;
        }

        var accountAnalyticLineDb = new AccountAnalyticLineDb(dbName);
        var activityLines = await accountAnalyticLineDb.GetItemsAsync(CurrentProjectTask);
        if (activityLines == null || activityLines.Count == 0)
        {
            await DisplayAlertAsync(
                "Atenci�n",
                "Es necesario agregar actividades para realizar el env�o al ERP debido a que actualmente se encuentra vac�o.",
                "Aceptar");
            return;
        }

        if (!allowAutoRetry)
        {
            var pending = await accountAnalyticLineDb.GetPendingItemsAsync(CurrentProjectTask);
            var needsHeaderRelink = ProjectTaskSyncValidation.NeedsHeaderRelink(CurrentProjectTask, activityLines);

            if ((pending == null || pending.Count == 0) && !needsHeaderRelink)
            {
                await DisplayAlertAsync("Atenci�n", "No hay detalles pendientes por reprocesar.", "Aceptar");
                return;
            }
        }

        var leave = await DisplayAlertAsync(confirmTitle, confirmMessage, "S�", "No");
        if (!leave)
            return;

        var serverPusher = new SaleOrders();
        bool popupShown = false;

        try
        {
            try
            {
                await UITools.ShowLoadingPopup(this);
                await UITools.SetNotifyLoadingPopup(allowAutoRetry ? "Enviando actividad..." : "Reprocesando pendientes...");
                popupShown = true;
            }
            catch (Exception exPopup)
            {
                Debug.WriteLine("[Details] No se pudo mostrar popup: " + exPopup);
            }

            ProjectTaskSendResult sendResult = allowAutoRetry
                ? await serverPusher.SendProjectTask(CurrentProjectTask, allowAutoRetry: true)
                : await serverPusher.ReprocessPendingProjectTask(CurrentProjectTask);

            await ReloadCurrentTaskAsync();

            if (BindingContext is DetailsViewModel vm)
                await vm.PublicLoadActivities();

            if (popupShown)
            {
                try { await UITools.HideLoadingPopup(); } catch { }
            }

            string alertTitle = sendResult.Ok ? "Env�o completado" : "Atenci�n";
            await DisplayAlertAsync(alertTitle, sendResult.Message, "Aceptar");

            if (sendResult.Ok)
            {
                Dispatcher.Dispatch(async () =>
                {
                    try { await Navigation.PopModalAsync(); } catch (Exception navEx) { Debug.WriteLine("[Details] PopModalAsync failed: " + navEx); }
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[Details] Error al sincronizar tarea: " + ex);
            try { if (popupShown) await UITools.HideLoadingPopup(); } catch { }

            await DisplayAlertAsync("Atenci�n", "Error al enviar tarea: " + ex.Message, "Cerrar");
        }
    }

    private async Task DeleteItemAsync(object obj)
    {
        if (obj == null)
            return;

        if (CurrentProjectTask != null && IsTaskFullySynced())
        {
            await Toast.Make("No puede eliminar actividades de una tarea sincronizada").Show();
            return;
        }

        try
        {
            var analytic = obj as AccountAnalyticLine;
            if (analytic == null)
                return;

            if (analytic.is_synchronized
                && ProjectTaskSyncValidation.IsLineEffectivelySynced(analytic, CurrentProjectTask?.id_sync ?? 0))
            {
                await DisplayAlertAsync(
                    "Atenci�n",
                    "No se puede eliminar un detalle que ya fue sincronizado con el ERP.",
                    "Aceptar");
                return;
            }

            bool confirm = await DisplayAlertAsync("Confirmaci�n", "�Desea eliminar esta actividad?", "S�", "No");
            if (!confirm) return;

            var db = new AccountAnalyticLineDb(App.Session?.odooConnection?.DbNameSqlite);
            int deleted = await db.DeleteAsync(analytic);

            if (deleted <= 0)
                deleted = await db.DeleteByIdAsync(analytic.id);

            if (BindingContext is DetailsViewModel vm)
                await vm.PublicLoadActivities();

            await Toast.Make(deleted > 0 ? "Actividad eliminada" : "No se encontr� registro para eliminar").Show();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Activities.Details] DeleteItemAsync error: {ex}");
            await Toast.Make("Error al eliminar actividad").Show();
        }
    }
}
