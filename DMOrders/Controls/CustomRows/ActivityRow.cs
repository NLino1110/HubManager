using CommunityToolkit.Maui.Behaviors;
using DMOrders.Controls.Base;
using DMOrders.Converters;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.DMOrders.tareas;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows
{    
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ActivityRow : RowAdvance<ProjectTask>
    {
        private bool _built;

        Label labelId { get; set; }
        Label labelName { get; set; }
        Label labelSellerName { get; set; }
        Label labelPlanningDate { get; set; }
        Label labelWriteDate { get; set; }
        Label labelUser { get; set; }
        Label labelState { get; set; }

        Button buttonEdit;
        Button buttonDelete;

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
            if (_built) return;

            LeftGrid.ColumnDefinitions = new ColumnDefinitionCollection()
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(2.5, GridUnitType.Star) },
                //new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
            };

            labelId = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TextColor = Colors.Black, FontSize = 12, BackgroundColor = Colors.Transparent, Padding = new Thickness(20,0,20,0), Margin = new Thickness(0) };

            labelName = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, FontSize = 12, BackgroundColor = Colors.Transparent };
            labelSellerName = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, FontSize = 12, BackgroundColor = Colors.Red };
            labelPlanningDate = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, TextColor = Colors.Black, FontSize = 12 };
            labelWriteDate = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, TextColor = Colors.Black, FontSize = 12 };
            labelUser = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, TextColor = Colors.Black, FontSize = 12 };
            labelState = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, TextColor = Colors.Black, FontSize = 12 };

            labelId.SetBinding(Label.TextProperty, new Binding(nameof(Item.id), source: Item));
            labelName.SetBinding(Label.TextProperty, new Binding(nameof(Item.display_username), source: Item));
            labelSellerName.SetBinding(Label.TextProperty, new Binding(nameof(Item.display_username), source: Item));
            labelPlanningDate.SetBinding(Label.TextProperty, new Binding(nameof(Item.date_assign), source: Item));
            labelWriteDate.SetBinding(Label.TextProperty, 
                new Binding(nameof(Item.date_synchronized), 
                source: Item, 
                converter: new DateToDashConverter()
                ));
            labelUser.SetBinding(Label.TextProperty, new Binding(nameof(Item.create_user), source: Item));
            labelState.SetBinding(Label.TextProperty, new Binding(nameof(Item.state_view), source: Item));

            //leftGrid.Children.Add(labelId);
            //Grid.SetRow(labelId, 0);
            //Grid.SetColumn(labelId, 0);
                        
            AddCell(CreateCell(labelId), "left", 0, 0);
            AddCell(CreateCell(labelName), "left", 0, 1);
            //AddCell(CreateCell(labelSellerName), "left", 0, 2);
            AddCell(CreateCell(labelPlanningDate), "left", 0, 2);
            AddCell(CreateCell(labelWriteDate), "left", 0, 3);
            AddCell(CreateCell(labelUser), "left", 0, 4);
            AddCell(CreateCell(labelState), "left", 0, 5);

            _built = true;
        }

        protected override void BuildToolGridContent(Grid toolGrid)
        {
            if (buttonEdit != null) return;

            buttonEdit = new Button
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

            buttonDelete = new Button
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
                IsVisible = false,
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
