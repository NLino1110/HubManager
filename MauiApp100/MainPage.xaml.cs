using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using Plugin.LocalNotification.Core.Models.AndroidOption;
using System.Reflection;
using System.Net.Http;

namespace MauiApp100
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
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
    }
}
