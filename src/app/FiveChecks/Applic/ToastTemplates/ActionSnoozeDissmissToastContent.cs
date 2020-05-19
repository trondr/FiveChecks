using System;
using System.Threading.Tasks;
using FiveChecks.Applic.Common;
using Microsoft.Toolkit.Uwp.Notifications;

namespace FiveChecks.Applic.ToastTemplates
{
    public static class ActionSnoozeDismissToastContent
    {
        public static async Task<ToastContent> CreateToastContent(ActionSnoozeDismissToastContentInfo contentInfo)
        {
            if (contentInfo == null) throw new ArgumentNullException(nameof(contentInfo));
            // Construct the visuals of the toast (using Notifications library)
            ToastContent toastContent = new ToastContent
            {

                // Arguments when the user taps body of toast
                Launch = contentInfo.Action,

                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        HeroImage = new ToastGenericHeroImage() { Source = await F.DownloadImage(contentInfo.ImageUri).ConfigureAwait(false) },
                        AppLogoOverride = new ToastGenericAppLogo() 
                            {
                                Source = await F.DownloadImage(contentInfo.AppLogoImageUri).ConfigureAwait(false),
                                HintCrop = ToastGenericAppLogoCrop.Circle
                            },
                        Attribution = new ToastGenericAttributionText { Text = contentInfo.CompanyName },
                        Children =
                                {
                                    new AdaptiveText
                                    {
                                        Text = contentInfo.Greeting,
                                        HintStyle = AdaptiveTextStyle.Title
                                    },
                                    
                                    new AdaptiveGroup()
                                    {
                                        Children =
                                        {
                                            new AdaptiveSubgroup()
                                            {
                                                Children =
                                                {
                                                    new AdaptiveText(){Text = contentInfo.Title, HintStyle = AdaptiveTextStyle.Title, HintWrap = true },
                                                }
                                            }
                                        }
                                    },

                                    new AdaptiveGroup
                                    {
                                        Children =
                                            {
                                                new AdaptiveSubgroup
                                                {
                                                    Children =
                                                    {
                                                        new AdaptiveText
                                                        {
                                                            Text = contentInfo.ContentSection1,
                                                            HintWrap = true
                                                        }
                                                    }
                                                }
                                            }
                                    },

                                    new AdaptiveGroup
                                    {
                                        Children =
                                        {
                                            new AdaptiveSubgroup
                                            {
                                                Children =
                                                {
                                                    new AdaptiveText
                                                    {
                                                        Text = contentInfo.ContentSection2,
                                                        HintWrap = true
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }

                    }
                },

                Actions = new ToastActionsCustom
                {
                    Inputs =
                        {
                            new ToastSelectionBox("snoozeTime")
                            {
                                DefaultSelectionBoxItemId = "15",
                                Items =
                                {
                                    new ToastSelectionBoxItem("15","15 minute"),
                                    new ToastSelectionBoxItem("30","30 minutes"),
                                    new ToastSelectionBoxItem("60","1 hour"),
                                    new ToastSelectionBoxItem("240","4 hours"),
                                    new ToastSelectionBoxItem("480","8 hours"),
                                }
                            }
                        },

                    Buttons =
                            {
                                // Note that there's no reason to specify background activation, since our COM
                                // activator decides whether to process in background or launch foreground window
                                new ToastButton(contentInfo.ActionButtonContent, contentInfo.Action){ActivationType = ToastActivationType.Protocol},
                                new ToastButton(contentInfo.SnoozeButtonContent, contentInfo.SnoozeAction){HintActionId = "snoozeTime",ActivationType = ToastActivationType.Background},
                                new ToastButton(contentInfo.NotNowButtonContent, contentInfo.NotNowAction){ActivationType = ToastActivationType.Background},
                            }
                }
            };
            return toastContent;
        }
    }
}
