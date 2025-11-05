using DMOrders.Models; // Asegúrate de que aquí esté la definición de tu modelo Activity
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.DMOrders.tareas;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Activities
{
    public class DetailsViewModel : INotifyPropertyChanged
    {
        public ProjectTask ParentProjectTask { get; set; }
        private ObservableCollection<AccountAnalyticLine> _activities;
        private AccountAnalyticLine _selectedItem;

        public ObservableCollection<AccountAnalyticLine> Activities
        {
            get => _activities;
            set
            {
                _activities = value;
                OnPropertyChanged();
            }
        }

        public AccountAnalyticLine SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public ICommand CloseCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SyncCommand { get; }

        public DetailsViewModel(ProjectTask _ParentProjectTask)
        {
            Activities = new ObservableCollection<AccountAnalyticLine>();
            ParentProjectTask = _ParentProjectTask;
            LoadActivities();

            CloseCommand = new Command(OnClose);
            NewCommand = new Command(OnNew);
            SaveCommand = new Command(OnSave);
            SyncCommand = new Command(OnSync);
        }

        public async Task PublicLoadActivities()
        {
            await LoadActivities();
        }

        private async Task LoadActivities()
        {
            var companyDb = new CompanyDb();
            var motivoDb = new MotivoActividadDiariaDb();
            var resPartnerDb = new ResPartnerDb();

            var accountAnalyticDb = new AccountAnalyticLineDb();
            var items = (await accountAnalyticDb.GetItemsAsync(ParentProjectTask));
            foreach(var item in items)
            {
                var companyItem = await companyDb.GetItem(item.company_id);

                if (companyItem != null)
                    item.res_company_display = companyItem.name;

                var motivoItem = await motivoDb.GetItem(item.motivo);

                if(motivoItem != null)
                    item.motivo_display = motivoItem.name;

                var partnerItem = await resPartnerDb.GetItemsAsync(item.company_id, item.partner_id);
                
                if(partnerItem != null)
                    item.res_partner_display = partnerItem.name;
            }
            Activities = new ObservableCollection<AccountAnalyticLine>(items);
        }

        private void OnClose()
        {
            // Lógica para cerrar o navegar atrás
        }

        private void OnNew()
        {
            var newActivity = new AccountAnalyticLine { id = 0, name = "Nueva actividad" };
            Activities.Add(newActivity);
            SelectedItem = newActivity;
        }

        private void OnSave()
        {
            // Lógica para guardar la actividad actual
        }

        private async void OnSync()
        {
            // Lógica para sincronizar actividades con un servidor, por ejemplo
            await Task.Delay(1000); // Simular sincronización
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
