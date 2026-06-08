using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using Plugin.LocalNotification.Core.Models.AndroidOption;
using Plugin.LocalNotification.Core.Models.AppleOption;

namespace MauiApp100
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseLocalNotification(config =>
                {
                    config.AddCategory(new NotificationCategory(NotificationCategoryType.Status)
                    {
                        ActionList =
                    [
                        new(100)
                        {
                            Title = "Hello",
                            Android =
                            {
                                LaunchAppWhenTapped = true,
                                IconName =
                                {
                                    ResourceName = "i2"
                                }
                            },
                            Apple =
                            {
                                Action = AppleActionType.Foreground
                            }
                        },
                        new(101)
                        {
                            Title = "Close",
                            Android =
                            {
                                LaunchAppWhenTapped = false,
                                IconName =
                                {
                                    ResourceName = "i3"
                                }
                            },
                            Apple =
                            {
                                Action = AppleActionType.Destructive
                            }
                        }
                    ]
                    })
                    .AddAndroid(android =>
                    {
                        // Default channel for general notifications
                        android.AddChannel(new AndroidNotificationChannelRequest
                        {
                            Id = "general_channel",
                            Name = "General",
                            Description = "General notifications",
                            Importance = AndroidImportance.Default
                        });

                        // High-priority channel for important alerts
                        android.AddChannel(new AndroidNotificationChannelRequest
                        {
                            Id = "urgent_channel",
                            Name = "Important Alerts",
                            Description = "Critical notifications that require immediate attention",
                            Importance = AndroidImportance.High,
                            EnableVibration = true,
                            EnableLights = true,
                            LightColor = new AndroidColor { ResourceName = "red" }
                        });

                        // Silent channel for background updates
                        android.AddChannel(new AndroidNotificationChannelRequest
                        {
                            Id = "silent_channel",
                            Name = "Silent Updates",
                            Description = "Background updates and non-urgent information",
                            Importance = AndroidImportance.Low,
                            EnableVibration = false,
                            EnableSound = false
                        });
                    });

                });


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
