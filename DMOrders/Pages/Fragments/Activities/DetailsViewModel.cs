using DMOrders.Models; // Asegúrate de que aquí esté la definición de tu modelo Activity
using DMSA.Models.Odoo.DMOrders;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Activities
{
    public class DetailsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PlanningSlot> _activities;
        private PlanningSlot _selectedItem;

        public ObservableCollection<PlanningSlot> Activities
        {
            get => _activities;
            set
            {
                _activities = value;
                OnPropertyChanged();
            }
        }

        public PlanningSlot SelectedItem
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

        public DetailsViewModel()
        {
            Activities = new ObservableCollection<PlanningSlot>();
            LoadActivities();

            CloseCommand = new Command(OnClose);
            NewCommand = new Command(OnNew);
            SaveCommand = new Command(OnSave);
            SyncCommand = new Command(OnSync);
        }

        private void LoadActivities()
        {
            // Aquí deberías cargar tus actividades desde el servicio o base de datos
            Activities.Add(new PlanningSlot { id = 1, name = "Reunión diaria", res_company_display="Macronegocios", res_partner_display="Cliente", start_datetime = DateTime.Now.TimeOfDay, end_datetime = DateTime.Now.TimeOfDay  });
            Activities.Add(new PlanningSlot { id = 2, name = "Revisión", res_company_display = "Macronegocios", res_partner_display = "Cliente", start_datetime = DateTime.Now.TimeOfDay, end_datetime = DateTime.Now.TimeOfDay });
        }

        private void OnClose()
        {
            // Lógica para cerrar o navegar atrás
        }

        private void OnNew()
        {
            var newActivity = new PlanningSlot { id = 0, name = "Nueva actividad" };
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
