using System.Collections.Generic;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.DiskSpaceCheck;
using Compliance.Notifications.Applic.PendingRebootCheck;
using LanguageExt.Common;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Applic.Commands
{
    [TestFixture()]
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
        [Category(TestCategory.UnitTests)]
        [TestCase(PendingReboot.True, LoadPendingRebootCallCount.One, ShowPendingRebootToastNotificationCallCount.One, RemovePendingRebootToastNotificationCallCount.Zero,true,false, Description = "Pending reboot is true")]
        [TestCase(PendingReboot.False, LoadPendingRebootCallCount.One, ShowPendingRebootToastNotificationCallCount.Zero, RemovePendingRebootToastNotificationCallCount.One,false,false, Description = "Pending reboot is false")]
        public void CheckPendingRebootTest(bool isPendingReboot, int expectedLoadPendingRebootCallCount,
            int expectedShowPendingRebootToastNotificationCount,
            int expectedRemovePendingRebootToastNotificationCallCount, bool isNonCompliant, bool isDisabled)
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
                        },isDisabled);

            Assert.AreEqual(expectedLoadPendingRebootCallCount, actualLoadPendingRebootCallCount, "LoadPendingRebootResult");
            Assert.AreEqual(expectedShowPendingRebootToastNotificationCount, actualShowPendingRebootToastNotificationCount,"ShowPendingRebootToastNotification");
            Assert.AreEqual(expectedRemovePendingRebootToastNotificationCallCount, actualRemovePendingRebootToastNotificationCount, "RemovePendingRebootToastNotification");
        }

        [Test]
        [Category(TestCategory.ManualTests)]
        public void IsDisabledTest()
        {
            var actual = Profile.IsNotificationDisabled(false, typeof(CheckDiskSpaceCommand));
            Assert.AreEqual(true, actual, @"Value is not set: [HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Policies\FiveChecks\Compliance.Notifications\PendingRebootCheck]Disabled=1");
        }
    }
}