using System;
using LanguageExt;

namespace Compliance.Notifications.Commands.CheckDiskSpace
{
    public class ActionSnoozeDismissToastContentInfo: Record<ActionSnoozeDismissToastContentInfo>
    {
        public string Title { get; }
        public string CompanyName { get; }
        public string ContentSection1 { get; }
        public string ContentSection2 { get; }
        public string Action { get; }
        public Uri ImageUri { get; }
        public Uri AppLogoImageUri { get; }

        public ActionSnoozeDismissToastContentInfo(string title, string companyName, string contentSection1, string contentSection2, string action, Uri imageUri, Uri appLogoImageUri)
        {
            Title = title;
            CompanyName = companyName;
            ContentSection1 = contentSection1;
            ContentSection2 = contentSection2;
            Action = action;
            ImageUri = imageUri;
            AppLogoImageUri = appLogoImageUri;
        }
    }
}