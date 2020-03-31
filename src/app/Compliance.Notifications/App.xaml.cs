using System;
using System.Windows;
using Compliance.Notifications.Common;
using Compliance.Notifications.Model;
using GalaSoft.MvvmLight.Messaging;

namespace Compliance.Notifications
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public int ExitCode { get; set; }
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Logging.DefaultLogger.Info($"Starting application. {Environment.CommandLine}");
            Messenger.Default.Register<ExitApplicationMessage>(this, message =>
            {
                Logging.DefaultLogger.Info($"Shutting down application...");
                this.Dispatcher.Invoke(() =>
                {
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
