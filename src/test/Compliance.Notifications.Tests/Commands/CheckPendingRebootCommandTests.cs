using System.Collections.Generic;
using System.Threading.Tasks;
using Compliance.Notifications.Commands;
using Compliance.Notifications.Common;
using Compliance.Notifications.Common.Tests;
using Compliance.Notifications.Model;
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
        [TestCase(PendingReboot.True, LoadPendingRebootCallCount.One, ShowPendingRebootToastNotificationCallCount.One, RemovePendingRebootToastNotificationCallCount.Zero, Description = "Pending reboot is true")]
        [TestCase(PendingReboot.False, LoadPendingRebootCallCount.One, ShowPendingRebootToastNotificationCallCount.Zero, RemovePendingRebootToastNotificationCallCount.One, Description = "Pending reboot is false")]
        public void CheckPendingRebootTest(bool isPendingReboot,int expectedLoadPendingRebootCallCount, int expectedShowPendingRebootToastNotificationCount, int expectedRemovePendingRebootToastNotificationCallCount)
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
                            return new PendingRebootInfo {RebootIsPending = isPendingReboot,Source=new List<RebootSource>()};
                        },
                        (s) => { 
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