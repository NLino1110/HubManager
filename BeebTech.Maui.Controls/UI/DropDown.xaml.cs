using System.Collections.ObjectModel;

namespace BeebTech.Maui.Controls.UI;

public partial class DropDown : ContentView
{
    public ObservableCollection<string> Items { get; set; } = new();

    public event Action<string> OnItemSelected;

    bool isOpen = false;

    public DropDown()
    {
        InitializeComponent();

        Items.Add("Opción 1");
        Items.Add("Opción 2");
        Items.Add("Opción 3");

        Build();
    }

    void Build()
    {
        ItemsHost.Children.Clear();

        foreach (var item in Items)
        {
            var lbl = new Label
            {
                Text = item,
                Padding = 8
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                MainButton.Text = item;
                DropPanel.IsVisible = false;
                isOpen = false;
                OnItemSelected?.Invoke(item);
            };

            lbl.GestureRecognizers.Add(tap);

            ItemsHost.Children.Add(lbl);
        }

        this.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() =>
            {
                if (isOpen)
                {
                    DropPanel.IsVisible = false;
                    isOpen = false;
                }
            })
        });
    }

    void OnClicked(object sender, EventArgs e)
    {
        isOpen = !isOpen;
        DropPanel.IsVisible = isOpen;
    }

}