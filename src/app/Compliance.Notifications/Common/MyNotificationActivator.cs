using System.Runtime.InteropServices;
using Compliance.Notifications.Model;
using Compliance.Notifications.Resources;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.QueryStringDotNET;

namespace Compliance.Notifications.Common
{
    // The GUID CLSID must be unique to your app. Create a new GUID if copying this code.
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid("2DE9CA80-6322-4AE2-8FB5-3435F6976711"), ComVisible(true)]
    public class MyNotificationActivator : NotificationActivator
    {
        public override void OnActivated(string arguments, NotificationUserInputCollection userInputCollection, string appUserModelId)
        { 
           Logging.DefaultLogger.Info($"{strings.YouActivatedTheToast}:{arguments}:{appUserModelId}:{userInputCollection?.ObjectToString()}");
           var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
           Logging.DefaultLogger.Info($"Process Name: {currentProcess.ProcessName}, {currentProcess.Id}");
           Messenger.Default.Send(new ExitApplicationMessage());
            if (string.IsNullOrWhiteSpace(arguments))
            {
                Logging.DefaultLogger.Info("Toast was activated without arguments. Do nothing.");
                return;
            }
            var args = QueryString.Parse(arguments);
            if (args.Contains("action"))
            {
                var action = args["action"];
                switch (action)
                {
                    case "restart":
                        Logging.DefaultLogger.Warn("TODO: Open Windows 10 Shutdown dialog with restart as option.");
                        F.OpenRestartDialog();
                        break;
                    default:
                        Logging.DefaultLogger.Info("Unknown action. Do nothing.");
                        break;
                }
            }
            else
            {
                Logging.DefaultLogger.Warn("Toast was activated without 'action' argument.");
            }

        }
    }
}
