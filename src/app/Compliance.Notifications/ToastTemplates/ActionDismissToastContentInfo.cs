using System;
using LanguageExt;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Compliance.Notifications.ToastTemplates
{
    public class ActionDismissToastContentInfo : Record<ActionDismissToastContentInfo>
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
        public ToastActivationType ActionActivationType { get; }

        public ActionDismissToastContentInfo(BindableString greeting, string title, string companyName,
            string contentSection1, string contentSection2, Uri imageUri, Uri appLogoImageUri, string action,
            ToastActivationType actionActivationType, string actionButtonContent, string notNowButtonContent,
            string notNowAction)
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
            ActionActivationType = actionActivationType;
        }
    }
}