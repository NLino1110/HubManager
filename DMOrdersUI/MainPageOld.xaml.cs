using DMOrdersUI.Pages.Sys;
using DMSA.Models.Odoo.Native;
using InputKit.Shared.Controls;
using UraniumUI.Pages;

namespace DMOrdersUI
{
    public partial class MainPageOld : UraniumContentPage
    {
        public string StringCompanyName { get; set; } = "Compañia";
        public MainPageOld()
        {
            SelectionView.GlobalSetting.CornerRadius = 0;
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var headerTemplate = await Task.Run(() =>
                    BuildTabHeaderSpecial(App.Session.res_Company.name, App.Session.res_Store.name));

                tabViewMain.Tabs[0].HeaderTemplate = headerTemplate;
                tabViewMain.SelectedTab = tabViewMain.Tabs[1];
            });

            //StringCompanyName = App.Session.res_Company.name;
            //lblStore.Text = App.Session.res_Store.name;            
        }

        private void ShowBottomSheet(object sender, EventArgs e)
        {            
            bottomSheet.IsPresented = true;
        }

        private DataTemplate BuildTabHeaderSpecial(string textLabel1, string textLabel2)
        {
            var headerTemplate = new DataTemplate(() =>
            {
                // Imagen
                var image = new Image
                {
                    Source = "logo_macronegocios.png",
                    WidthRequest = 45,
                    HeightRequest = 45
                };

                // Label de la compañía con Binding
                var labelCompany = new Label
                {
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black,
                    Text = textLabel1,
                };                

                // Label estático
                var labelStore = new Label
                {
                    Text = textLabel2,
                    FontSize = 12,
                    TextColor = Colors.Gray
                };

                var verticalLayout = new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children = { labelCompany, labelStore }
                };

                var horizontalLayout = new HorizontalStackLayout
                {
                    IsEnabled = false,
                    HorizontalOptions = LayoutOptions.Center,
                    Spacing = 3,
                    Padding = new Thickness(2),
                    Margin = new Thickness(10, 0, 20, 0),
                    Children = { image, verticalLayout }
                };

                return horizontalLayout;
            });

            return headerTemplate;

        }

        //private async void BtnTopTools_OnClicked_Clicked(object sender, EventArgs e)
        //{
        //    var fntSrc = (FontImageSource)btnTopTools.ImageSource;

        //    int unicodevalue = char.ConvertToUtf32(fntSrc.Glyph, 0);

        //    if (unicodevalue == 61641)
        //    {
        //        await btnTopTools.RotateTo(90, 200);
        //        btnTopTools.Rotation = 0;
        //        fntSrc.Glyph = "\uf00d";
        //        TopTools.IsVisible = true;
        //    }
        //    else
        //    {
        //        await btnTopTools.RotateTo(-90, 200);
        //        btnTopTools.Rotation = 0;
        //        fntSrc.Glyph = "\uf0c9";
        //        TopTools.IsVisible = false;
        //    }
        //}

        private async void btnSave_Clicked(object sender, EventArgs e)
        {
        }

        private async void btnUpdate_Clicked(object sender, EventArgs e)
        {
            //BtnTopTools_OnClicked_Clicked(sender, e);

            //UpdateData obj = new UpdateData();
            //obj.Disappearing += UpdateData_Disappearing;


            //await Navigation.PushModalAsync(obj);
        }

        private async void btnExit_Clicked(object sender, EventArgs e)
        {            
            //App.Current.MainPage = new Login();
        }

        private async void ViewCell_Tapped_Update(object sender, EventArgs e)
        {
            bottomSheet.IsPresented = false;
            UpdateData obj = new UpdateData();

            //obj.Sel_Company_Id = new res_company()
            //{
            //    id = se.id,
            //    name = se.name
            //};

            obj.Disappearing += UpdateData_Disappearing;

            //await Navigation.PushAsync(obj, false);

            await Navigation.PushModalAsync(obj);
        }

        private async void ViewCell_Tapped_Exit(object sender, EventArgs e)
        {
            //App.Current.MainPage = new Login();
        }

        private void UpdateData_Disappearing(object? sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private async void ViewCell_Add_Sale(object sender, EventArgs e)
        {  

        }

        private async void ViewCell_Add_Task(object sender, EventArgs e)
        {
            
        }
    }
}