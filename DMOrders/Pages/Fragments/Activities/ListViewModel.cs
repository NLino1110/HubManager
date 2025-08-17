using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.Native;
using Microsoft.Maui;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DMOrders.Pages.Fragments.Activities
{
    public class ListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ActivityHeader> Activities { get; set; } = new();

        private ActivityHeader _selectedItem;
        public ActivityHeader SelectedItem
        {
            get => _selectedItem;
            set
            {
                //if (_selectedActivity != value)
                //{
                //    _selectedActivity = value;
                //    OnPropertyChanged();
                //    OnPropertyChanged(nameof(SelectedActivity));
                //}

                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged();                   
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public ListViewModel()
        {
            LoadDataByTimer();
            //EditCommand = new Command(EditItem);
            //EditCommand = new RelayCommand<ActivityHeader>(EditItem);
        }

        public async Task LoadData()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                Activities = new ObservableCollection<ActivityHeader>
            {
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 2, seller_name = "Miguel Vargas" },
                new ActivityHeader { id = 3, seller_name = "Byron Freire" },
                new ActivityHeader { id = 4, seller_name = "Sergio Ruiz" },
                new ActivityHeader { id = 4, seller_name = "Sergio Ruiz" },
                new ActivityHeader { id = 4, seller_name = "Sergio Ruiz" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },
                new ActivityHeader { id = 1, seller_name = "Ronald Chonillo" },

            };

                OnPropertyChanged(nameof(Activities));
                //OnPropertyChanged(nameof(CanGoNext));
                //OnPropertyChanged(nameof(CanGoPrevious));
            }
            catch (Exception ex)
            {
                //_itemsData = new ObservableCollection<res_partner>();
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void LoadDataByTimer()
        {
            // Usamos el dispatcher global de la app para garantizar ejecución en UI
            var dispatcher = Application.Current.Dispatcher;

            var timer = dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(300); // delay corto para dejar respirar la UI
            timer.IsRepeating = false;

            timer.Tick += async (s, e) =>
            {
                try
                {
                    if (IsBusy) return; // Previene cargas simultáneas
                    await LoadData();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en LoadData: {ex}");
                }
                finally
                {
                    timer.Stop();
                }
            };

            timer.Start();
        }

        //public ICommand EditCommand { get; set; }

        //private async void EditItem(object obj)
        //{
        //    Debug.WriteLine("EditItem");
        //    Details viewObj = new Details();
        //    //objPage.Sel_AccountMoveSendHeader = (AccountMoveSendHeader)obj;
        //    //objPage.editionMode = true;
        //    //objPage.Disappearing += NewGroup_Disappearing;
        //    //await Navigation.PushAsync(viewObj, false);
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
