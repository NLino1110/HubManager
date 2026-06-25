using DMSA.Models.Odoo.Native;
using Newtonsoft.Json.Linq;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using Plugin.LocalNotification.Core.Models.AndroidOption;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Reflection;
using System.Windows.Input;
using System;

namespace MauiApp100
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {

        private sale_order_line _selectedOrderLine;
        public sale_order_line SelectedOrderLine
        {
            get => _selectedOrderLine;
            set
            {
                _selectedOrderLine = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<sale_order_line> GenerarOrderLinesFake(int cantidad = 100)
        {
            var random = new Random();
            var list = new ObservableCollection<sale_order_line>();

            for (int i = 1; i <= cantidad; i++)
            {
                var qty = Math.Round((decimal)(random.NextDouble() * 10 + 1), 2);
                var price = Math.Round((decimal)(random.NextDouble() * 100 + 5), 2);
                var discount = Math.Round((decimal)(random.NextDouble() * 20), 2);

                var subtotal = qty * price;
                var discountAmount = subtotal * (discount / 100);
                var taxed = (subtotal - discountAmount) * 0.12m;
                var total = subtotal - discountAmount + taxed;

                list.Add(new sale_order_line
                {
                    id = i,
                    sequence = i,
                    product_id = random.Next(1, 50),
                    product_tmpl_id = random.Next(1, 50),

                    product_uom_qty = qty,
                    product_uom_qty_real = qty,
                    qty_to_deliver = qty,

                    price_unit = price,
                    discount = discount,
                    amount_discount = discountAmount,

                    price_subtotal = subtotal,
                    price_tax = taxed,
                    price_total = total,

                    product_uom_category_id = JToken.FromObject(new object[] { 1, "Unidad" }),

                    product_code = $"PROD-{i:000}",
                    product_display = $"Producto Fake {i}",
                    uom_category_display = "Unidad",

                    is_gift = (i % 10 == 0),
                    is_manual = false,

                    virtual_iva_percentage = 12,
                    virtual_line_subtotal = subtotal,

                    product_id_origin = 0,
                    show_delete_button = true,

                    promotion_ids = new int[] { random.Next(1, 5) },
                    rule_ids = new int[] { random.Next(1, 5) },
                    origin_gift_line_ids = Array.Empty<int>(),

                    erp_id = 0
                });
            }

            return list;
        }

        private async void Button_LoadData(object sender, EventArgs e)
        {
            OrderLines = new ObservableCollection<sale_order_line>(GenerarOrderLinesFake(100));
            OnPropertyChanged(nameof(OrderLines));
        }

        public void LoadFakeData()
        {
            OrderLines = new ObservableCollection<sale_order_line>(GenerarOrderLinesFake(100));
            OnPropertyChanged(nameof(OrderLines));
        }

        public ObservableCollection<sale_order_line> OrderLines { get; set; }

        public FileImageSource SettingsIconSource { get; private set; }

        private string _text;
        public string Text
        {
            get => _text;
            set => SetField(ref _text, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        protected bool SetField<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        int count = 0;

        public ICommand FirstCommand { get; }

        private void OnFirstCommandExecuted(string s) => Text = s;

        //public ICommand ShowMenuCommand => new Command<object>(async (item) =>
        //{
        //    // Aquí decides cómo mostrar el menú
        //    await Application.Current.MainPage.DisplayActionSheetAsync(
        //        "Opciones",
        //        "Cancelar",
        //        null,
        //        "Action 1",
        //        "Action 2"
        //    );
        //});

        public ICommand ActionCommand
        {
            get => (ICommand)GetValue(ActionCommandProperty);
            set => SetValue(ActionCommandProperty, value);
        }

        public static readonly BindableProperty ActionCommandProperty =
            BindableProperty.Create(
                nameof(ActionCommand),
                typeof(ICommand),
                typeof(MainPage),
                null);

        private void OnActionCommandExecuted(object item) => Text = $"Action executed for: {item}";

        public MainPage()
        {
            InitializeComponent();
            FirstCommand = new Command<string>((s) => OnFirstCommandExecuted(s));

            ActionCommand = new Command<object>((item) => OnActionCommandExecuted(item));

            SettingsIconSource = "outline_settings_black_24.png";
            OrderLines = new ObservableCollection<sale_order_line>(GenerarOrderLinesFake(500));
            OnPropertyChanged(nameof(OrderLines));
            BindingContext = this;
        }

        private async Task<byte[]> getUrlByte()
        {
            var httpClient = new HttpClient();

            byte[] imageBytes = [];

            try
            {
                imageBytes = await httpClient.GetByteArrayAsync(
                    "https://dmujeresec.vtexassets.com/arquivos/ids/174788-1200-1200?v=639088622488730000&width=1200&height=1200&aspect=true"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error descargando imagen: {ex.Message}");
            }
            finally
            {
                httpClient.Dispose();                
            }

            return imageBytes;
        }

        private async void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);

            byte[] imageBytes = [];
            //Assembly assembly = GetType().GetTypeInfo().Assembly;
            //Stream stream = assembly.GetManifestResourceStream("MauiApp100.Resources.Images.dotnet_bot_res.png");

            //if (stream != null)
            //{
            //    using var ms = new MemoryStream();
            //    await stream.CopyToAsync(ms);
            //    imageBytes = ms.ToArray();
            //}

            imageBytes = await getUrlByte();

            var urgentNotification = new NotificationRequest
            {
                NotificationId = 101,
                Title = "⚠️ Important Alert",
                Description = "Critical system update required",
                CategoryType = NotificationCategoryType.Status,
                Image =
                {                        
                    Binary = imageBytes
                },                
                Android =
                {
                    ChannelId = "urgent_channel",
                    Priority = AndroidPriority.High,
                    TimeoutAfter = TimeSpan.FromHours(1)
                }
            };
            await LocalNotificationCenter.Current.Show(urgentNotification);
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {            
            await Application.Current.MainPage.DisplayActionSheetAsync(
                "Opciones",
                "Cancelar",
                null,
                "Action 1",
                "Action 2"
            );
        }
    }
}
