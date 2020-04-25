using System.Collections.Generic;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.PendingRebootCheck;
using Compliance.Notifications.Tests.Common;
using LanguageExt.Common;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Commands
{
    [TestFixture()]
    [Category(TestCategory.UnitTests)]
    public class CheckPendingRebootCommandTests
    {
        public static class ShowPendingRebootToastNotificationCallCount
        {
            public const int One = 1;
            public const int Zero = 0;
        }

        public static class RemovePendingRebootToastNotificationCallCount
        {
            public const int One = 1;
            public const int Zero = 0;
        }

        public static class LoadPendingRebootCallCount
        {
            public const int One = 1;
            public const int Zero = 0;
        }

        public static class PendingReboot
        {
            public const bool True = true;
            public const bool False = false;
        }

        [Test]
        [TestCase(PendingReboot.True, LoadPendingRebootCallCount.One, ShowPendingRebootToastNotificationCallCount.One, RemovePendingRebootToastNotificationCallCount.Zero,true, Description = "Pending reboot is true")]
        [TestCase(PendingReboot.False, LoadPendingRebootCallCount.One, ShowPendingRebootToastNotificationCallCount.Zero, RemovePendingRebootToastNotificationCallCount.One,false, Description = "Pending reboot is false")]
        public void CheckPendingRebootTest(bool isPendingReboot,int expectedLoadPendingRebootCallCount, int expectedShowPendingRebootToastNotificationCount, int expectedRemovePendingRebootToastNotificationCallCount, bool isNonCompliant)
        {
            var actualLoadPendingRebootCallCount = 0;
            var actualShowPendingRebootToastNotificationCount = 0;
            var actualRemovePendingRebootToastNotificationCount = 0;
            var actual = 
                    CheckPendingRebootCommand.CheckPendingRebootPure(
                        async () =>
                        {
                            actualLoadPendingRebootCallCount++;
                            await Task.CompletedTask;
                            return new PendingRebootInfo {RebootIsPending = isPendingReboot,Sources=new List<RebootSource>()};
                        },info => isNonCompliant
                        ,
                        (info,s) => { 
                            actualShowPendingRebootToastNotificationCount++;
                            return Task.FromResult(new Result<ToastNotificationVisibility>(ToastNotificationVisibility.Show));
                        }, () =>
                        {
                            actualRemovePendingRebootToastNotificationCount++;
                            return Task.FromResult(new Result<ToastNotificationVisibility>(ToastNotificationVisibility.Hide));
                        });

            Assert.AreEqual(expectedLoadPendingRebootCallCount, actualLoadPendingRebootCallCount, "LoadPendingRebootResult");
            Assert.AreEqual(expectedShowPendingRebootToastNotificationCount, actualShowPendingRebootToastNotificationCount,"ShowPendingRebootToastNotification");
            Assert.AreEqual(expectedRemovePendingRebootToastNotificationCallCount, actualRemovePendingRebootToastNotificationCount, "RemovePendingRebootToastNotification");
        }
    }
}