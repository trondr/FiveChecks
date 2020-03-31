using System.Runtime.InteropServices;
using Compliance.Notifications.Model;
using Compliance.Notifications.Resources;
using GalaSoft.MvvmLight.Messaging;

namespace Compliance.Notifications.Common
{
    // The GUID CLSID must be unique to your app. Create a new GUID if copying this code.
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid("2DE9CA80-6322-4AE2-8FB5-3435F6976711"), ComVisible(true)]
    public class MyNotificationActivator : NotificationActivator
    {
        public override void OnActivated(string invokedArgs, NotificationUserInputCollection userInputCollection, string appUserModelId)
        { 
           Logging.DefaultLogger.Info($"{strings.YouActivatedTheToast}:{invokedArgs}:{appUserModelId}:{userInputCollection?.ObjectToString()}");
           var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
           Logging.DefaultLogger.Info($"Process Name: {currentProcess.ProcessName}, {currentProcess.Id}");
           Messenger.Default.Send(new ExitApplicationMessage());
        }
    }
}
