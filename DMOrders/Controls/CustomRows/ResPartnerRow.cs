using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.Native;
using System.Windows.Input;


namespace DMOrders.Controls.CustomRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ResPartnerRow : BaseRow<res_partner>
    {
        Label labelId { get; set; }
        Label labelVat { get; set; }
        Label labelName { get; set; }
        Label labelEmail { get; set; }
        Label labelPhone { get; set; }
        Label labelVisits { get; set; }
        Label labelCity { get; set; }

        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public static readonly BindableProperty EditCommandProperty =
            BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(ResPartnerRow), null);
        public ResPartnerRow()
        {            
            
        }

        protected override void BuildLeftGridContent(Grid leftGrid)
        {
            labelId = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TextColor = Colors.Black, FontSize = 10, BackgroundColor = Colors.Transparent, Padding = new Thickness(3), Margin = new Thickness(0) };
            labelVat = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, FontSize = 13, BackgroundColor = Colors.Transparent };
            labelName = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, FontSize = 13, BackgroundColor = Colors.Transparent };
            labelEmail = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, FontSize = 13, BackgroundColor = Colors.Transparent };
            labelPhone = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, TextColor = Colors.DarkSlateGray, FontSize = 12 };
            labelVisits = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, TextColor = Colors.OrangeRed, FontSize = 10 };
            labelCity = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, TextColor = Colors.Green, FontSize = 10 };
            
            labelId.SetBinding(Label.TextProperty, new Binding(nameof(Item.id), source: Item));
            labelVat.SetBinding(Label.TextProperty, new Binding(nameof(Item.vat), source: Item));
            labelName.SetBinding(Label.TextProperty, new Binding(nameof(Item.name), source: Item));
            labelEmail.SetBinding(Label.TextProperty, new Binding(nameof(Item.email), source: Item));
            labelPhone.SetBinding(Label.TextProperty, new Binding(nameof(Item.phone), source: Item, stringFormat: "{0:hh\\:mm}"));
            labelVisits.SetBinding(Label.TextProperty, new Binding(nameof(Item.active), source: Item, stringFormat: "{0:hh\\:mm}"));
            labelCity.SetBinding(Label.TextProperty, new Binding(nameof(Item.city), source: Item, stringFormat: "{0:hh\\:mm}"));

            var cellGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(0),
                ColumnSpacing = 0,
                RowSpacing = 0,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },                    
                }
            };

            cellGrid.Children.Add(labelName);
            cellGrid.Children.Add(labelVat);
            Grid.SetRow(labelVat, 1);

            AddCell(CreateCell(labelId), "left", 0, 0);
            AddCell(CreateCell(cellGrid), "left", 0, 2);
            AddCell(CreateCell(labelEmail), "left", 0, 3);
            AddCell(CreateCell(labelPhone), "left", 0, 4);
            AddCell(CreateCell(labelVisits), "left", 0, 5);
            AddCell(CreateCell(labelCity), "left", 0, 6);
        }

        protected override void BuildToolGridContent(Grid toolGrid)
        {
            var buttonEdit = new Button
            {
                HeightRequest = 35,
                WidthRequest = 35,
                BackgroundColor = Colors.DodgerBlue,
                Text = "",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                IsVisible = true,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 15,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf303"
                },
                Padding = new Thickness(3),
                Margin = new Thickness(2),
            };

            buttonEdit.SetBinding(Button.CommandProperty, new Binding("EditCommand", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(ResPartnerRow))));
            buttonEdit.SetBinding(Button.CommandParameterProperty, new Binding("Item", source: this));

            //var buttonDelete = new Button
            //{
            //    //Command = EditCommand,
            //    CommandParameter = "",
            //    HeightRequest = 35,
            //    WidthRequest = 35,
            //    BackgroundColor = Colors.OrangeRed,
            //    Text = "",
            //    TextColor = Colors.White,
            //    FontAttributes = FontAttributes.Bold,
            //    FontSize = 12,
            //    HorizontalOptions = LayoutOptions.Center,
            //    IsVisible = true,
            //    ImageSource = new FontImageSource
            //    {
            //        FontFamily = "FontAwesome5Solid",
            //        Color = Colors.White,
            //        Size = 15,
            //        FontAutoScalingEnabled = true,
            //        Glyph = "\uf2ed"
            //    },
            //    Padding = new Thickness(3),
            //    Margin = new Thickness(2),
            //};

            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Margin = new Thickness(0),
                BackgroundColor = Colors.Transparent
            };

            stackLayout.Children.Add(buttonEdit);
            //stackLayout.Children.Add(buttonDelete);

            toolGrid.Children.Add(stackLayout);
            Grid.SetRow(stackLayout, 0);
            Grid.SetRowSpan(stackLayout, 2);
            Grid.SetColumn(stackLayout, 4);
        }
    }
}
