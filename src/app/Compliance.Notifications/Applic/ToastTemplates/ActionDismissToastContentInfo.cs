﻿using System;
using Compliance.Notifications.Applic.Common;
using LanguageExt;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Compliance.Notifications.Applic.ToastTemplates
{
    public class ActionDismissToastContentInfo : Record<ActionDismissToastContentInfo>
    {
        
        public BindableString Greeting { get; }
        public string Title { get; }

        private string _companyName;
        public string CompanyName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_companyName))
                    _companyName = F.GetCompanyName();
                return _companyName;
            }
        }
        public string ContentSection1 { get; }
        public string ContentSection2 { get; }
        public Option<string> ContentSection3 { get; }
        public string Action { get; }
        public string GroupName { get; }
        public Uri ImageUri { get; }
        public Uri AppLogoImageUri { get; }
        public string ActionButtonContent { get; }
        public string NotNowButtonContent { get; }
        public string NotNowAction { get; }
        public ToastActivationType ActionActivationType { get; }

        public ActionDismissToastContentInfo(BindableString greeting, string title,
            string contentSection1, string contentSection2, Uri imageUri, Uri appLogoImageUri, string action,
            ToastActivationType actionActivationType, string actionButtonContent, string notNowButtonContent,
            string notNowAction, string groupName, Option<string> contentSection3)
        {
            Greeting = greeting;
            Title = title;
            ContentSection1 = contentSection1;
            ContentSection2 = contentSection2;
            Action = action;
            ImageUri = imageUri;
            AppLogoImageUri = appLogoImageUri;
            ActionButtonContent = actionButtonContent;
            NotNowButtonContent = notNowButtonContent;
            NotNowAction = notNowAction;
            GroupName = groupName;
            ContentSection3 = contentSection3;
            ActionActivationType = actionActivationType;
        }
    }
}