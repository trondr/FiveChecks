using System;
using System.Collections.Concurrent;
using System.Windows;
using FiveChecks.Applic;
using FiveChecks.Applic.Common;
using GalaSoft.MvvmLight.Messaging;

namespace FiveChecks
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ConcurrentDictionary<string,string> _notificationGroups = new ConcurrentDictionary<string, string>();

        public int ExitCode { get; set; }
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Logging.DefaultLogger.Info($"Starting application. {Environment.CommandLine}");
            Messenger.Default.Register<RegisterToastNotificationMessage>(this, message =>
                {
                    Logging.DefaultLogger.Info($"Registering toast notification group '{message.NotificationGroup}'...");
                    if (!_notificationGroups.ContainsKey(message.NotificationGroup))
                    {
                        _notificationGroups.GetOrAdd(message.NotificationGroup, message.NotificationGroup);
                    }
                });
            Messenger.Default.Register<UnRegisterToastNotificationMessage>(this, message =>
            {
                Logging.DefaultLogger.Info($"Unregister toast notification group '{message.NotificationGroup}'...");
                DesktopNotificationManagerCompat.History.RemoveGroup(message.NotificationGroup);
                _notificationGroups.TryRemove(message.NotificationGroup, out var value);
                if (_notificationGroups.Count == 0)
                {
                    Logging.DefaultLogger.Info($"No more toast notification groups registered, request application exit...");
                    Messenger.Default.Send(new ExitApplicationMessage());
                }
            });
            Messenger.Default.Register<ExitApplicationMessage>(this, message =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    Logging.DefaultLogger.Info($"Shutting down application...");
                    Application.Current.Shutdown(this.ExitCode);
                });
            });
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Logging.DefaultLogger.Info($"Exiting application. {Environment.CommandLine}");
            Messenger.Default.Unregister<ExitApplicationMessage>(this);
        }

        private void App_OnActivated(object sender, EventArgs e)
        {
            Logging.DefaultLogger.Info($"Application is activated.");
        }

        public static void RunApplicationOnStart(StartupEventHandler onStart)
        {
            var application = new App();
            application.InitializeComponent();
            application.Startup += onStart;
            application.Run();
        }
    }
}
