using DMSA.Models.Odoo.DMOrders;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DMOrdersUI.Pages.Fragments.Activities
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


        public ListViewModel()
        {
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

            //EditCommand = new Command(EditItem);
            //EditCommand = new RelayCommand<ActivityHeader>(EditItem);
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
