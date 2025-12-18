using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using DMOrders.Controls;
using DMOrders.Controls.Tools;
using DMOrders.Services.Database.Sqlite;
using DMOrders.Shared;
using DMSA.Models.Odoo.DMOrders.tareas;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using DMSA.Sync.Core.Database.Sqlite.tareas;
using DMSA.Sync.Core.Update.Pusher;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Activities;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Details : ContentPage, IBackButtonHandler
{
    public ProjectTask CurrentProjectTask { get; set; }
    public ICommand EditCommand { get; set; }

    // Restaurada: DeleteCommand para que AccountAnalyticLineRow pueda invocarla por binding/reflection
    public ICommand DeleteCommand { get; set; }

    private async void EditItem(object obj)
    {
        Debug.WriteLine("EditItem");
        var ItemForEdit = (AccountAnalyticLine)obj;
        PopupSizeConstants popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);

        var returnResultPopup = new PopupAccountAnalyticLine(popupSizeConstants, CurrentProjectTask, ItemForEdit);
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;
        var result = await this.ShowPopupAsync(returnResultPopup);

        if (result != null)
        {

        }
        else
        {
            // stackAccountInfo.IsVisible = false;
        }

        await ((DetailsViewModel)BindingContext).PublicLoadActivities();
    }

    private void ViewObj_Disappearing(object? sender, EventArgs e)
    {
        Debug.WriteLine("ViewObj_Disappearing");
    }

    public Details(ProjectTask _CurrentActivityHeader)
    {
        InitializeComponent();

        CurrentProjectTask = _CurrentActivityHeader;

        if (CurrentProjectTask != null)
        {
            lblMainTitle.Text = $"Actividades Diarias No. {CurrentProjectTask.id}   Fecha {CurrentProjectTask.name}";
        }

        BindingContext = new DetailsViewModel(CurrentProjectTask);
        EditCommand = new Command(EditItem);

        // Inicializar DeleteCommand para que AccountAnalyticLineRow pueda ejecutarlo (restaurado)
        DeleteCommand = new Command(async (obj) => await DeleteItemAsync(obj));
    }

    public async Task<bool> OnBackButtonPressedAsync()
    {
        bool result = await DisplayAlert("Confirmación", "Minimizar la aplicación, ¿Desea continuar?", "Sí", "No");
        if (result)
        {
#if ANDROID
            Platform.CurrentActivity?.MoveTaskToBack(true);
#endif
        }
        return !result; // true => lo manejo yo y no cierro la app; false => dejar cerrar
    }

    protected override bool OnBackButtonPressed()
    {
        Dispatcher.Dispatch(async () =>
        {
            await Navigation.PopModalAsync();
        });

        return true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private async void ButtonClose_Clicked(object sender, EventArgs e)
    {
        SendBackButtonPressed();
    }

    private async void ButtonNew_Clicked(object sender, EventArgs e)
    {
        PopupSizeConstants popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);

        var returnResultPopup = new PopupAccountAnalyticLine(popupSizeConstants, CurrentProjectTask, null);
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        var result = await this.ShowPopupAsync(returnResultPopup);

        await ((DetailsViewModel)BindingContext).PublicLoadActivities();
    }

    private async void ButtonSave_Clicked(object sender, EventArgs e)
    {
        //Save data
        await Toast.Make("Datos almacenados").Show();
        await Navigation.PopModalAsync();
    }

    private async void ButtonSync_Clicked(object sender, EventArgs e)
    {
        var serverPusher = new ServerPusher();
        bool popupShown = false;

        try
        {
            try
            {
                await UITools.ShowLoadingPopup(this);
                await UITools.SetNotifyLoadingPopup("Enviando tarea...");
                popupShown = true;
            }
            catch (Exception exPopup)
            {
                Debug.WriteLine("[Details] No se pudo mostrar popup: " + exPopup);
            }

            // Enviar tarea al servidor (ServerPusher puede crear/actualizar id_sync internamente)
            await serverPusher.SendProjectTask(CurrentProjectTask);

            // Marcar como sincronizada en la instancia local
            if (CurrentProjectTask != null)
            {
                CurrentProjectTask.is_synchronized = true;
                CurrentProjectTask.date_synchronized = DateTime.Now;
            }

            // Persistir el cambio en la BD usada por la UI
            try
            {
                var dbName = App.Session?.odooConnection?.DbNameSqlite;
                if (!string.IsNullOrWhiteSpace(dbName))
                {
                    var projectDb = new ProjectTaskDb(dbName);
                    await projectDb.UpdateAsync(CurrentProjectTask);
                }
                else
                {
                    Debug.WriteLine("[Details] App.Session.odooConnection.DbNameSqlite es nulo o vacío; no se actualiza BD local.");
                }
            }
            catch (Exception dbEx)
            {
                Debug.WriteLine("[Details] Error actualizando ProjectTaskDb: " + dbEx);
            }

            // Forzar recarga del ViewModel de detalles
            try
            {
                if (BindingContext is DetailsViewModel vm)
                    await vm.PublicLoadActivities();
            }
            catch (Exception vmEx)
            {
                Debug.WriteLine("[Details] Error recargando ViewModel: " + vmEx);
            }

            // Notificar a otros componentes que la tarea fue sincronizada
            try
            {
                MessagingCenter.Send(this, "ProjectTaskSynced", CurrentProjectTask?.id ?? 0);
            }
            catch (Exception msgEx)
            {
                Debug.WriteLine("[Details] MessagingCenter.Send falló: " + msgEx);
            }

            if (popupShown)
            {
                try { await UITools.HideLoadingPopup(); } catch { }
            }

            // Navegar atrás en hilo UI
            Dispatcher.Dispatch(async () =>
            {
                try { await Navigation.PopModalAsync(); } catch (Exception navEx) { Debug.WriteLine("[Details] PopModalAsync failed: " + navEx); }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[Details] Error al sincronizar tarea: " + ex);
            try { if (popupShown) await UITools.HideLoadingPopup(); } catch { }
            await Toast.Make("Error al enviar tarea: " + ex.Message).Show();
        }
    }

    // Eliminación persistente y recarga del ViewModel (restaurado)
    private async Task DeleteItemAsync(object obj)
    {
        Debug.WriteLine("[Activities.Details] DeleteItemAsync invoked");
        try
        {
            if (obj == null)
            {
                Debug.WriteLine("[Activities.Details] parametro null");
                return;
            }

            // intentar extraer AccountAnalyticLine directamente o desde propiedad Item
            var analytic = obj as AccountAnalyticLine;
            if (analytic == null)
            {
                var itemProp = obj.GetType().GetProperty("Item");
                analytic = itemProp?.GetValue(obj) as AccountAnalyticLine;
            }

            if (analytic == null)
            {
                Debug.WriteLine("[Activities.Details] parámetro no es AccountAnalyticLine");
                await Toast.Make("Elemento no válido para eliminar").Show();
                return;
            }

            bool confirm = await DisplayAlert("Confirmación", "¿Desea eliminar esta actividad?", "Sí", "No");
            if (!confirm) return;

            // Usar nombre de BD desde la sesión
            string dbFile = App.Session?.odooConnection?.DbNameSqlite ?? string.Empty;
            Debug.WriteLine($"[Activities.Details] usando DB '{dbFile}' para eliminar id={analytic.id}");

            var db = new AccountAnalyticLineDb(dbFile);
            int deleted = await db.DeleteAsync(analytic);
            Debug.WriteLine($"[Activities.Details] DeleteAsync returned={deleted} for id={analytic.id}");

            if (deleted <= 0)
            {
                deleted = await db.DeleteByIdAsync(analytic.id);
                Debug.WriteLine($"[Activities.Details] DeleteByIdAsync returned={deleted} for id={analytic.id}");
            }

            // Recargar desde la DB para asegurar consistencia
            if (BindingContext is DetailsViewModel vm)
            {
                await vm.PublicLoadActivities();
            }

            await Toast.Make(deleted > 0 ? "Actividad eliminada" : "No se encontró registro para eliminar").Show();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Activities.Details] DeleteItemAsync error: {ex}");
            await Toast.Make("Error al eliminar actividad").Show();
        }
    }
}