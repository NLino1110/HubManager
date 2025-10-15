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
        private ObservableCollection<MailActivityPlan> _activities;
        private MailActivityPlan _selectedItem;

        public ObservableCollection<MailActivityPlan> Activities
        {
            get => _activities;
            set
            {
                _activities = value;
                OnPropertyChanged();
            }
        }

        public MailActivityPlan SelectedItem
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
            Activities = new ObservableCollection<MailActivityPlan>();
            LoadActivities();

            CloseCommand = new Command(OnClose);
            NewCommand = new Command(OnNew);
            SaveCommand = new Command(OnSave);
            SyncCommand = new Command(OnSync);
        }

        private void LoadActivities()
        {
            // Aquí deberías cargar tus actividades desde el servicio o base de datos
            Activities.Add(new MailActivityPlan { id = 1, name = "Reunión diaria",  display_name = "Reu dia", user_id = 3 });
            Activities.Add(new MailActivityPlan { id = 2, name = "Revisión", display_name = "Reu dia 2", user_id = 3 });
        }

        private void OnClose()
        {
            // Lógica para cerrar o navegar atrás
        }

        private void OnNew()
        {
            var newActivity = new MailActivityPlan { id = 0, name = "Nueva actividad" };
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
