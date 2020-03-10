using System;
using System.Runtime.InteropServices;
using Windows.UI.Popups;

namespace Compliance.Notifications.Helper
{
    // The GUID CLSID must be unique to your app. Create a new GUID if copying this code.
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid("2DE9CA80-6322-4AE2-8FB5-3435F6976711"), ComVisible(true)]
    public class MyNotificationActivator : NotificationActivator
    {
        public override void OnActivated(string invokedArgs, NotificationUserInput userInput, string appUserModelId)
        { 
           Console.WriteLine("You activated the toast.");
           Console.ReadLine();
        }
    }
}
