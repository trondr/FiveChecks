using System;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using LanguageExt;
using log4net.Core;
using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Compliance.Notifications.Applic.ToastTemplates
{
    public static class ActionDismissToastContent
    {
        private static async Task<string> GetHeroImage()
        {
            return (await 
                    F.CacheFolder
                        .Match(
                        async cacheFolder => await F.GetRandomImageFromCache(cacheFolder).ConfigureAwait(false),
                        async () => await Task.FromResult(Option<string>.None).ConfigureAwait(false)
                     ).ConfigureAwait(false))
                .Match(imagePath => imagePath, () => string.Empty);
        }


        public static async Task<ToastContent> CreateToastContent(ActionDismissToastContentInfo contentInfo)
        {
            if (contentInfo == null) throw new ArgumentNullException(nameof(contentInfo));
            // Construct the visuals of the toast (using Notifications library)
            var action = contentInfo.ActionActivationType == ToastActivationType.Protocol
                ? contentInfo.Action
                : new QueryString {{"action", contentInfo.Action},{"group",contentInfo.GroupName}}.ToString();

            var notNowAction = new QueryString {{"action", contentInfo.NotNowAction}, {"group", contentInfo.GroupName}}.ToString();

            var heroImage = await GetHeroImage().ConfigureAwait(false);
            
            var toastContent = new ToastContent
            {

                // Arguments when the user taps body of toast
                Launch = action,
                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        HeroImage = new ToastGenericHeroImage
                        {
                            Source = heroImage
                        },
                        AppLogoOverride = new ToastGenericAppLogo
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
                                                        Text = contentInfo.Title, HintStyle = AdaptiveTextStyle.Title, 
                                                        HintWrap = true
                                                    },
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
                    Buttons =
                            {
                                // Note that there's no reason to specify background activation, since our COM
                                // activator decides whether to process in background or launch foreground window
                                new ToastButton(contentInfo.ActionButtonContent, action){ActivationType = contentInfo.ActionActivationType},
                                new ToastButton(contentInfo.NotNowButtonContent, notNowAction){ActivationType = ToastActivationType.Background},
                            }
                }
            };
            contentInfo.ContentSection3.IfSome(
                c3 =>
                {
                    toastContent.Visual.BindingGeneric.Children.Add(new AdaptiveGroup
                    {
                        Children =
                        {
                            new AdaptiveSubgroup
                            {
                                Children =
                                {
                                    new AdaptiveText
                                    {
                                        Text = c3,
                                        HintWrap = true
                                    }
                                }
                            }

                        }
                    });
                });
            return toastContent;
        }
    }
}
