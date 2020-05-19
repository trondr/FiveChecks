using System;
using LanguageExt;
using Microsoft.Toolkit.Uwp.Notifications;

namespace FiveChecks.Applic.ToastTemplates
{
    public class ActionSnoozeDismissToastContentInfo: Record<ActionSnoozeDismissToastContentInfo>
    {
        public BindableString Greeting { get; }
        public string Title { get; }
        public string CompanyName { get; }
        public string ContentSection1 { get; }
        public string ContentSection2 { get; }
        public string Action { get; }
        public Uri ImageUri { get; }
        public Uri AppLogoImageUri { get; }
        public string ActionButtonContent { get; }
        public string NotNowButtonContent { get; }
        public string NotNowAction { get; }
        public string SnoozeAction { get; }
        public string SnoozeButtonContent { get; }
        
        public ActionSnoozeDismissToastContentInfo(BindableString greeting, string title, string companyName, string contentSection1, string contentSection2, string action, Uri imageUri, Uri appLogoImageUri, string actionButtonContent, string notNowButtonContent, string notNowAction, string snoozeButtonContent, string snoozeAction)
        {
            Greeting = greeting;
            Title = title;
            CompanyName = companyName;
            ContentSection1 = contentSection1;
            ContentSection2 = contentSection2;
            Action = action;
            ImageUri = imageUri;
            AppLogoImageUri = appLogoImageUri;
            ActionButtonContent = actionButtonContent;
            NotNowButtonContent = notNowButtonContent;
            NotNowAction = notNowAction;
            SnoozeButtonContent = snoozeButtonContent;
            SnoozeAction = snoozeAction;
        }
    }
}