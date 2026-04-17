using DMDataSafePreview.Models;
using DMDataSafePreview.PageModels;

namespace DMDataSafePreview.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}