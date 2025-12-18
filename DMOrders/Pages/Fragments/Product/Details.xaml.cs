using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using DMOrders.Controls;
using DMOrders.Services.Database.Sqlite;
using DMOrders.Services.Helpers;
using DMOrders.Shared;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.DMOrders.tareas;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite.tareas;
using MPowerKit.VirtualizeListView;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Product;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Details : ContentPage, IBackButtonHandler
{
    public ProjectTask CurrentActivityHeader { get; set; }

    public ICommand DeleteCommand { get; set; }

    public Details()
    {
        InitializeComponent();
        BindingContext = new DetailsViewModel();

        // Exponer el DeleteCommand para que los Row templates puedan enlazarlo
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
        var tcs = new TaskCompletionSource<bool>();

        Dispatcher.Dispatch(async () =>
        {

            var leave = await DisplayAlert("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");

            if (leave)
            {
                await Navigation.PopAsync();
            }
        });

        return true;
        //return base.OnBackButtonPressed();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        //Debug.WriteLine("Details page is disappearing.");

        //if (true)
        //{ 
        //    // Si no se permite el cierre, evitar que la página se cierre
        //    Navigation.PopModalAsync(false);
        //}
    }

    private async void ButtonClose_Clicked(object sender, EventArgs e)
    {
        SendBackButtonPressed();
        //await Navigation.PopAsync();
    }

    private async void ButtonNew_Clicked(object sender, EventArgs e)
    {
        PopupSizeConstants popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);

        var returnResultPopup = new PopupAccountAnalyticLine(popupSizeConstants, null, null);
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;
        returnResultPopup.projectTask = CurrentActivityHeader;

        //if (!isWindows)
        //    returnResultPopup.Size = this.popupSizeConstants.Large;

        var result = await this.ShowPopupAsync(returnResultPopup);

        if (result != null)
        {
            //...
        }
        else
        {
            //...
        }
    }

    private async void ButtonSave_Clicked(object sender, EventArgs e)
    {
        //Save data
        await Toast.Make("Datos almacenados").Show();
        await Navigation.PopAsync();
    }

    private void ButtonSync_Clicked(object sender, EventArgs e)
    {

    }

    private async Task DeleteItemAsync(object obj)
    {
        Debug.WriteLine("DeleteItemAsync (Product Details) - intentando eliminar elemento");
        try
        {
            if (obj == null)
            {
                Debug.WriteLine("Parametro null");
                return;
            }

            // Intentar obtener AccountAnalyticLine directamente o desde una propiedad 'Item'
            AccountAnalyticLine analytic = obj as AccountAnalyticLine;
            if (analytic == null)
            {
                var itemProp = obj.GetType().GetProperty("Item");
                analytic = itemProp?.GetValue(obj) as AccountAnalyticLine;
            }

            // Si es una AccountAnalyticLine entonces eliminar en BD y refrescar (caso Activities)
            if (analytic != null)
            {
                bool confirm = await DisplayAlert("Confirmación", "¿Desea eliminar esta actividad?", "Sí", "No");
                if (!confirm) return;

                var db = new AccountAnalyticLineDb(App.Session.odooConnection.DbNameSqlite);
                await db.DeleteAsync(analytic);
                Debug.WriteLine($"AccountAnalyticLine id={analytic.id} eliminado de DB.");

                // Si el BindingContext es el ViewModel de Activities, recargar desde DB
                if (BindingContext is DMOrders.Pages.Fragments.Activities.DetailsViewModel activitiesVm)
                {
                    await activitiesVm.PublicLoadActivities();
                }
                else
                {
                    // fallback: intentar eliminar de la colección 'Activities' del BindingContext actual
                    RemoveFromBindingContextActivities(analytic);
                }

                await Toast.Make("Actividad eliminada").Show();
                return;
            }

            // Si no era AccountAnalyticLine, intentar eliminar genérico de Activities (por ejemplo PlanningSlot en Producto)
            if (BindingContext == null)
            {
                Debug.WriteLine("BindingContext es null");
                return;
            }

            var activitiesProp = BindingContext.GetType().GetProperty("Activities");
            if (activitiesProp == null)
            {
                Debug.WriteLine("No se encontró la propiedad 'Activities' en el BindingContext");
                return;
            }

            var activitiesObj = activitiesProp.GetValue(BindingContext);
            if (activitiesObj is not IList list)
            {
                Debug.WriteLine("La propiedad 'Activities' no es una colección IList");
                return;
            }

            // Si el parámetro es la misma instancia que aparece en la lista, eliminar por referencia
            if (list.Contains(obj))
            {
                list.Remove(obj);
                Debug.WriteLine("Elemento eliminado por referencia de Activities");
                await Toast.Make("Elemento eliminado").Show();
                return;
            }

            // Intentar eliminar por id si existe
            var objIdProp = obj?.GetType().GetProperty("id");
            if (objIdProp != null)
            {
                var objId = objIdProp.GetValue(obj);
                object toRemove = null;
                foreach (var item in list)
                {
                    var itemIdProp = item?.GetType().GetProperty("id");
                    if (itemIdProp == null) continue;
                    var itemId = itemIdProp.GetValue(item);
                    if (itemId != null && itemId.Equals(objId))
                    {
                        toRemove = item;
                        break;
                    }
                }
                if (toRemove != null)
                {
                    list.Remove(toRemove);
                    Debug.WriteLine("Elemento eliminado por id de Activities");
                    await Toast.Make("Elemento eliminado").Show();
                    return;
                }
            }

            Debug.WriteLine("No se encontró elemento coincidente para eliminar");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DeleteItemAsync error: {ex.Message}");
            await Toast.Make("Error al eliminar elemento").Show();
        }
    }

    void RemoveFromBindingContextActivities(object param)
    {
        try
        {
            var vm = BindingContext;
            if (vm == null)
            {
                Debug.WriteLine("BindingContext null en RemoveFromBindingContextActivities");
                return;
            }

            var activitiesProp = vm.GetType().GetProperty("Activities", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (activitiesProp == null)
            {
                Debug.WriteLine("ViewModel no expone 'Activities'");
                return;
            }

            var activitiesObj = activitiesProp.GetValue(vm);
            if (activitiesObj is not IList list)
            {
                Debug.WriteLine("'Activities' no es IList");
                return;
            }

            if (param != null && list.Contains(param))
            {
                list.Remove(param);
                Debug.WriteLine("Removed item by reference from Activities (fallback)");
                return;
            }

            var idProp = param?.GetType().GetProperty("id");
            if (idProp != null)
            {
                var idVal = idProp.GetValue(param);
                object toRemove = null;
                foreach (var it in list)
                {
                    var itIdProp = it?.GetType().GetProperty("id");
                    if (itIdProp == null) continue;
                    var itId = itIdProp.GetValue(it);
                    if (idVal != null && idVal.Equals(itId))
                    {
                        toRemove = it;
                        break;
                    }
                }
                if (toRemove != null)
                {
                    list.Remove(toRemove);
                    Debug.WriteLine("Removed item by id from Activities (fallback)");
                    return;
                }
            }

            Debug.WriteLine("No matching item found to remove (fallback)");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error removing item from Activities: {ex.Message}");
        }
    }
}