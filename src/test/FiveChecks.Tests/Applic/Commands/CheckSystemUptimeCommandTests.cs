using System;
using System.Threading.Tasks;
using FiveChecks.Applic.Common;
using FiveChecks.Applic.SystemUptimeCheck;
using LanguageExt.Common;
using NUnit.Framework;

namespace FiveChecks.Tests.Applic.Commands
{
    [TestFixture]
    public class CheckSystemUptimeCommandTests
    {
        public class TestData
        {
            public int ExpectedLoadCount { get; set; }
            public int ExpectedShowCount { get; set; }
            public int ExpectedHideCount { get; set; }
            public bool IsNonCompliant { get; set; }
            public DateTime LastRestart { get; set; }
            public TimeSpan UpTime { get; set; }
            public bool IsDisabled { get; set; }
        }

        public static object[] TestDataSource =
        {
            new object[] {"Compliant", new TestData {IsNonCompliant = false,ExpectedLoadCount = 1,ExpectedShowCount = 0,ExpectedHideCount = 1, IsDisabled = false}},
            new object[] {"NonCompliant", new TestData { IsNonCompliant = true, ExpectedLoadCount = 1,ExpectedShowCount = 1,ExpectedHideCount = 0,IsDisabled = false}},
        };

        [Test]
        [Category(TestCategory.UnitTests)]
        [TestCaseSource("TestDataSource")]
        public void CheckSystemUptimeTest(string description, object data)
        {
            var testData = data as TestData;
            Assert.IsNotNull(testData, "testdata is null");
            var loadCount = 0;
            var showCount = 0;
            var hideCount = 0;
            var actual =
                CheckSystemUptimeCommand.CheckSystemUptimePure(() =>
                    {
                        loadCount++;
                        return Task.FromResult(new SystemUptimeInfo(){LastRestart = testData.LastRestart,Uptime = testData.UpTime});
                    }, info => testData.IsNonCompliant
                    , (info) =>
                    {
                        showCount++;
                        return Task.FromResult(new Result<ToastNotificationVisibility>(ToastNotificationVisibility.Show));
                    }, () =>
                    {
                        hideCount++;
                        return Task.FromResult(new Result<ToastNotificationVisibility>(ToastNotificationVisibility.Hide));
                    },testData.IsDisabled);
            Assert.AreEqual(testData.ExpectedLoadCount, loadCount, "LoadCount");
            Assert.AreEqual(testData.ExpectedShowCount, showCount, "ShowCount");
            Assert.AreEqual(testData.ExpectedHideCount, hideCount, "HideCount");
        }

        [Test]
        [Category(TestCategory.ManualTests)]
        public void IsDisabledTest()
        {
            var actual = Profile.IsNotificationDisabled(false,typeof(CheckSystemUptimeCommand));
            Assert.AreEqual(true, actual, @"Value is not set: [HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Policies\FiveChecks\FiveChecks\SystemUptimeCheck]Disabled=1");
        }
    }
}