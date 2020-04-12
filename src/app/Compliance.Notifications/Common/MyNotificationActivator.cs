using System.Runtime.InteropServices;
using Compliance.Notifications.Model;
using GalaSoft.MvvmLight.Messaging;
using LanguageExt;

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
           Logging.DefaultLogger.Info($"Toast notification was activated with arguments: '{arguments}'. Source app:{appUserModelId}");
           ToastActions.ParseToastActionArguments(arguments).Match(func =>
           {
               func().Match(unit => Unit.Default, exception =>
               {
                   Logging.DefaultLogger.Error($"Failed to execute action registered to action arguments: '{arguments}'. {exception.ToExceptionMessage()}");
                   return Unit.Default;
               });
           }, () =>
           {
               Logging.DefaultLogger.Warn($"No action registered to action arguments: '{arguments}'.");
           });
           ToastActions.ParseToastGroupArguments(arguments).Match(group =>
           {
               Logging.DefaultLogger.Info($"Requesting application exit after activation of {group}");
               Messenger.Default.Send(new ExitApplicationMessage(group));
           }, () =>
           {
               Logging.DefaultLogger.Warn("Did not send application exit message due to missing toast notification group name in toast activation arguments.");
           });
        }
    }
}
