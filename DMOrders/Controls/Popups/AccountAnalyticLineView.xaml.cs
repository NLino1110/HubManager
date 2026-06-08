using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tareas;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.tareas;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMOrders.Controls.Popups;

public partial class AccountAnalyticLineView : Popup
{
    public ProjectTask projectTask { get; set; }

    public AccountAnalyticLine analyticLine { get; set; }

    private res_partner Sel_Res_Partner { get; set; }

    public AccountAnalyticLineView()
	{
		InitializeComponent();
        PrepareForm();
    }

    async void PopupResPartner(object sender, EventArgs e)
    {
        var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        popupSizeConstants.CalculateSizes(DeviceDisplay.Current);        

        var returnResultPopup = new PopupSelectPartnerSingle(popupSizeConstants);

        returnResultPopup.Company = new res_company()
        {
            id = 1,
            name = "Macronegocios",
        };

        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        var result = await PopupExtensions.ShowPopupAsync<res_partner>(App.Current.MainPage, returnResultPopup);
        
        if (result.Result != null)
        {
            Sel_Res_Partner = (res_partner)result.Result;
            _inputResPartner.Text = Sel_Res_Partner.id.ToString() + " - " + Sel_Res_Partner.name;            
        }
    }

    private void PrepareForm()
    {
        Task.Run(async () =>
        {
            try
            {
                Debug.WriteLine("Cargando los datos...");

                if (projectTask != null)
                {
                    ObservableCollection<ProjectTask> lplanning = new ObservableCollection<ProjectTask>();
                    lplanning.Add(projectTask);
                    _pickerPlanningSlot.ItemsSource = lplanning;
                    //_pickerPartner.ItemDisplayBinding = new Binding("name");

                    _pickerPlanningSlot.SelectedIndex = 0;
                    //labelArticulo.SetBinding(Label.TextProperty, new Binding(nameof(facturaItem.ARTICULO), source: this));
                }
                else
                {
                    //Si no se ha enviado el partner de origen no se permitirá el ingreso del dato
                    await App.Current.MainPage.DisplayAlert("Nueva actividad",
                                        $"Se requiere que se especifique la actividad principal.",
                                        "Continuar");

                    //await CloseAsync(default(AccountAnalyticLine));

                    return;
                }

                ObservableCollection<MotivoActividadDiaria> lplanning_reason = new ObservableCollection<MotivoActividadDiaria>();

                MotivoActividadDiariaDb motivoActividadDiariaDb = new MotivoActividadDiariaDb(App.Session.odooConnection.DbNameSqlite);
                lplanning_reason = new ObservableCollection<MotivoActividadDiaria>((await motivoActividadDiariaDb.GetItemsAsync()).OrderBy(i => i.name));

                _pickerPlanningReason.ItemsSource = lplanning_reason;
                _pickerPlanningReason.ItemDisplayBinding = new Binding("name");
                //_pickerPlanningReason.SelectedItem = 0;

                ObservableCollection<res_company> lcompany = new ObservableCollection<res_company>();
                CompanyDb companyDb = new CompanyDb(App.Session.odooConnection.DbNameSqlite);
                lcompany = new ObservableCollection<res_company>((await companyDb.GetItemsAsync()).OrderBy(i => i.name));

                //_pickerCompany.ItemsSource = lcompany;
                //_pickerCompany.ItemDisplayBinding = new Binding("name");

                if (analyticLine != null)
                {                    
                    _inputReview.Text = analyticLine.name;

                    ResPartnerDb resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);

                    if (analyticLine.partner_id != null)
                    {
                        Sel_Res_Partner = await resPartnerDb.GetItemsAsync(analyticLine.company_id, analyticLine.partner_id.Value);
                        if (Sel_Res_Partner != null)
                        {
                            _inputResPartner.Text = Sel_Res_Partner.id.ToString() + " - " + Sel_Res_Partner.name;
                        }
                    }

                    var motivo_selected = lplanning_reason.Where(i => i.id == analyticLine.motivo).FirstOrDefault();
                    if (motivo_selected != null)
                    {
                        _pickerPlanningReason.SelectedItem = motivo_selected;
                    }
                    var time_start = TimeSpan.FromHours((double)analyticLine.hour_start);
                    _timePickerStart.Time = time_start;
                    //_timePickerStart.TimePickerView.SetValue(TimePickerField.TimeProperty, time_start);
                    var time_end = TimeSpan.FromHours((double)analyticLine.hour_end);
                    _timePickerEnd.Time = time_end;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en PrepareForm: {ex}");
            }
        });
    }

    private async void OnBtnSave_Clicked(object sender, EventArgs e)
    {
        if (_inputReview.Text == null || _inputReview.Text == "")
        {
            

            _inputReview.Focus();
            
            await Toast.Make("Llene el campo de observaciones").Show();
            return;
        }

        var motivo = (MotivoActividadDiaria)_pickerPlanningReason.SelectedItem;

        var time_start = _timePickerStart.Time.Value;
        double hour_start = time_start.Hours + (time_start.Minutes / 60.0) + (time_start.Seconds / 3600.0);

        var time_end = _timePickerEnd.Time.Value;
        double hour_end = time_end.Hours + (time_end.Minutes / 60.0) + (time_end.Seconds / 3600.0);

        if (hour_start >= hour_end)
        {
            await Toast.Make("La hora final debe ser mayor a la hora de inicio").Show();
            return;
        }

        try
        {
            bool isNew = true;
            string title_save = "Guardar nueva actividad";
            string message_save = $"¿Desea guardar la nueva actividad para el cliente {_inputResPartner.Text}?";

            if (analyticLine != null)
            {
                isNew = false;

                title_save = "Modificar actividad";
                message_save = $"¿Desea guardar los cambios de la actividad para el cliente {_inputResPartner.Text}?";
            }
            else
            {
                analyticLine = new AccountAnalyticLine();
            }

            bool answer = await App.Current.MainPage.DisplayAlert(title_save,
                    message_save,
                    "Continuar", "Cerrar");

            if (!answer)
            {
                return;
            }

            //new_PlanningSlot.id = 1;
            //analyticLine.name = $"VISITA {Sel_Res_Partner.name}";
            analyticLine.name = _inputReview.Text;
            if (Sel_Res_Partner != null)
            {
                analyticLine.partner_id = Sel_Res_Partner.id;
            }

            analyticLine.company_id = App.Session.res_Company.id;
            analyticLine.project_id = projectTask.project_id_;
            analyticLine.task_id = projectTask.id;
            analyticLine.date = DateTime.Now;
            analyticLine.motivo = motivo.id;

            analyticLine.hour_start = (decimal)hour_start;
            analyticLine.hour_end = (decimal)hour_end;
            analyticLine.duration = analyticLine.hour_end - analyticLine.hour_start;

            AccountAnalyticLineDb accountAnalyticLineDb = new AccountAnalyticLineDb(App.Session.odooConnection.DbNameSqlite);
            if (isNew)
            {
                await accountAnalyticLineDb.InsertAsync(analyticLine);
            }
            else
            {
                await accountAnalyticLineDb.UpdateAsync(analyticLine);
            }

            await Toast.Make("Actividad guardada correctamente " + analyticLine.id.ToString()).Show();
        }
        catch (Exception ex)
        {
            await Toast.Make("Error al guardar actividad: " + ex.Message).Show();
        }

        //await CloseAsync(analyticLine);
    }

    private async void OnBtnClose_Clicked(object sender, EventArgs e)
    {        
        //await CloseAsync(default(AccountAnalyticLine));
    }

    private async void OnBtnCancel_Clicked(object sender, EventArgs e)
    {        
        //await CloseAsync(default(AccountAnalyticLine));
    }

    void OnEntryTapped(object sender, EventArgs e)
    {     
        PopupResPartner(sender, e);
    }
}