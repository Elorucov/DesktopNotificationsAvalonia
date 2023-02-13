using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using DesktopNotifications.FreeDesktop;
using DesktopNotifications.Windows;
using System;
using System.Runtime.InteropServices;

namespace DesktopNotifications.Avalonia {
    /// <summary>
    /// Extensions for <see cref="AppBuilder"/>
    /// </summary>
    public static class AppBuilderExtensions {
        /// <summary>
        /// Setups the <see cref="INotificationManager"/> for the current platform and
        /// binds it to the service locator (<see cref="AvaloniaLocator"/>).
        /// </summary>
        /// <typeparam name="TAppBuilder"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static AppBuilder SetupDesktopNotifications(this AppBuilder builder) {
            INotificationManager manager;
            var runtimeInfo = builder.RuntimePlatform.GetRuntimeInfo();
            if (runtimeInfo.IsMobile) return builder;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                var context = WindowsApplicationContext.FromCurrentProcess();
                manager = new WindowsNotificationManager(context);
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                var context = FreeDesktopApplicationContext.FromCurrentProcess();
                manager = new FreeDesktopNotificationManager(context);
            } else {
                return builder;
            }

            //TODO Any better way of doing this?
            manager.Initialize().GetAwaiter().GetResult();

            builder.AfterSetup(b => {
                if (b.Instance.ApplicationLifetime is IControlledApplicationLifetime lifetime) {
                    lifetime.Exit += (s, e) => {
                        manager.Dispose();
                    };
                }
            });

            AvaloniaLocator.CurrentMutable.Bind<INotificationManager>().ToConstant(manager);

            return builder;
        }
    }
}