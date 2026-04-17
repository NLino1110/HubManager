using DMSA.Models.Odoo.Native;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace DMOrders.Pages.Fragments.Activities;

public partial class Container : ContentView
{
    public Container()
    {
        InitializeComponent();
        filterActivities.OnSearchButtonClicked += OnSearchButtonClicked;

        if (App.Session.CurrentUser != null)
        {
            //customImageHeaderView.Title = App.Session.CurrentUser.username;
            //customImageHeaderView.Subtitle = App.Session.CurrentUser.nombres;
        }

        try
        {
            //MessagingCenter.Subscribe<Details, int>(this, "ProjectTaskSynced", (sender, taskId) =>
            //{
            //    Debug.WriteLine($"[Container] ProjectTaskSynced received id={taskId}. Reloading activities.");
            //    Dispatcher.Dispatch(() =>
            //    {
            //        try
            //        {
            //            dataActivities.LoadData(filterActivities);
            //        }
            //        catch (Exception ex)
            //        {
            //            Debug.WriteLine("[Container] Error al recargar dataActivities: " + ex);
            //        }
            //    });
            //});
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[Container] MessagingCenter subscribe failed: " + ex);
        }
    }

    private void OnSearchButtonClicked(object? sender, EventArgs e)
    {
        dataActivities.LoadData(filterActivities);
    }

    public void LoadInfo(res_partner _data)
    {
        //info.FillData(_data);
    }

    public void ReloadData()
    {
        dataActivities.LoadData(filterActivities);
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        // Si el control se quita de la visual tree, anular la suscripción para evitar memory leaks
        if (Parent == null)
        {
            try
            {
                //MessagingCenter.Unsubscribe<Details, int>(this, "ProjectTaskSynced");
                Debug.WriteLine("[Container] Unsubscribed from ProjectTaskSynced");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Container] Error unsubscribing: " + ex);
            }
        }
    }
}