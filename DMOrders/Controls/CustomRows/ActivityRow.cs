using CommunityToolkit.Maui.Behaviors;
using DMSA.Models.Odoo.DMOrders;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ActivityRow : BaseRow<ActivityHeader>
    {
        Label labelId { get; set; }
        Label labelSellerName { get; set; }
        Label labelPlanningDate { get; set; }
        Label labelWriteDate { get; set; }
        Label labelUser { get; set; }
        Label labelState { get; set; }
        
        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public static readonly BindableProperty EditCommandProperty =
            BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(ActivityRow), null);

        public ActivityRow()
        { 

        }

        protected override void BuildLeftGridContent(Grid leftGrid)
        {
            labelId = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TextColor = Colors.Black, FontSize = 10, BackgroundColor = Colors.Transparent, Padding = new Thickness(3), Margin = new Thickness(0) };
            
            labelSellerName = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, FontSize = 13, BackgroundColor = Colors.Transparent };
            labelPlanningDate = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, TextColor = Colors.DarkSlateGray, FontSize = 12 };
            labelWriteDate = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, TextColor = Colors.OrangeRed, FontSize = 10 };
            labelUser = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, TextColor = Colors.Green, FontSize = 10 };
            labelState = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, TextColor = Colors.Green, FontSize = 10 };

            labelId.SetBinding(Label.TextProperty, new Binding(nameof(Item.id), source: Item));
            labelSellerName.SetBinding(Label.TextProperty, new Binding(nameof(Item.seller_name), source: Item));
            labelPlanningDate.SetBinding(Label.TextProperty, new Binding(nameof(Item.planning_date), source: Item));
            labelWriteDate.SetBinding(Label.TextProperty, new Binding(nameof(Item.write_date), source: Item));
            labelUser.SetBinding(Label.TextProperty, new Binding(nameof(Item.user_name), source: Item));
            //labelState.SetBinding(Label.TextProperty, new Binding(nameof(Item.status), source: this));

            //leftGrid.Children.Add(labelId);
            //Grid.SetRow(labelId, 0);
            //Grid.SetColumn(labelId, 0);
                        
            AddCell(CreateCell(labelId), "left", 0, 0);
            AddCell(CreateCell(labelSellerName), "left", 0, 2);
            AddCell(CreateCell(labelPlanningDate), "left", 0, 3);
            AddCell(CreateCell(labelWriteDate), "left", 0, 4);
            AddCell(CreateCell(labelUser), "left", 0, 5);
            AddCell(CreateCell(labelState), "left", 0, 6);            
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

            buttonEdit.SetBinding(Button.CommandProperty, new Binding("EditCommand", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(ActivityRow))));
            buttonEdit.SetBinding(Button.CommandParameterProperty, new Binding("Item", source: this));

            var buttonDelete = new Button
            {                
                Command = EditCommand,
                CommandParameter = "",
                HeightRequest = 35,
                WidthRequest = 35,
                BackgroundColor = Colors.OrangeRed,
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
                    Glyph = "\uf2ed"
                },
                Padding = new Thickness(3),
                Margin = new Thickness(2),
            };

            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Margin = new Thickness(0),
                BackgroundColor = Colors.Transparent
            };

            stackLayout.Children.Add(buttonEdit);
            stackLayout.Children.Add(buttonDelete);

            toolGrid.Children.Add(stackLayout);
            Grid.SetRow(stackLayout, 0);
            Grid.SetRowSpan(stackLayout, 2);
            Grid.SetColumn(stackLayout, 4);
        }
    }
}
