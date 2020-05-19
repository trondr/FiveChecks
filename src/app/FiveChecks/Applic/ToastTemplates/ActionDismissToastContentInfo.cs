using System;
using FiveChecks.Applic.Common;
using LanguageExt;
using Microsoft.Toolkit.Uwp.Notifications;

namespace FiveChecks.Applic.ToastTemplates
{
    public class ActionDismissToastContentInfo : Record<ActionDismissToastContentInfo>
    {
        public BindableString Greeting { get; }
        public string Title { get; }
        public string CompanyName { get; }
        public string ContentSection1 { get; }
        public string ContentSection2 { get; }
        public Option<string> ContentSection3 { get; }
        public string Action { get; }
        public string GroupName { get; }
        public string ActionButtonContent { get; }
        public string NotNowButtonContent { get; }
        public string NotNowAction { get; }
        public ToastActivationType ActionActivationType { get; }

        public ActionDismissToastContentInfo(BindableString greeting, string title,
            string contentSection1, string contentSection2, string action,
            ToastActivationType actionActivationType, string actionButtonContent, string notNowButtonContent,
            string notNowAction, string groupName, Option<string> contentSection3, string companyName)
        {
            Greeting = greeting;
            Title = title;
            ContentSection1 = contentSection1;
            ContentSection2 = contentSection2;
            Action = action;
            ActionButtonContent = actionButtonContent;
            NotNowButtonContent = notNowButtonContent;
            NotNowAction = notNowAction;
            GroupName = groupName;
            ContentSection3 = contentSection3;
            CompanyName = companyName;
            ActionActivationType = actionActivationType;
        }
    }
}