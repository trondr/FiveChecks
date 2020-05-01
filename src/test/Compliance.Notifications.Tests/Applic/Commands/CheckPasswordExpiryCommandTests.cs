using System;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.PasswordExpiryCheck;
using LanguageExt.Common;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Applic.Commands
{
    [TestFixture]
    [Category(TestCategory.UnitTests)]
    public class CheckPasswordExpiryCommandTests
    {
        public class TestData
        {
            public DateTime PasswordExpiryDate{ get; set; }
            public PasswordExpiryStatus PasswordExpiryStatus { get; set; }
            public bool IsRemoteSession { get; set; }
            public int ExpectedLoadCount { get; set; }
            public int ExpectedShowCount { get; set; }
            public int ExpectedHideCount { get; set; }
            public bool IsNonCompliant { get; set; }
        }

        private static DateTime expiryDate = new DateTime(2020, 01, 13, 14, 49, 05);
        public static object[] TestDataSource =
        {
            new object[] {".", new TestData {IsNonCompliant = false,ExpectedLoadCount = 1,ExpectedShowCount = 0,ExpectedHideCount = 1,IsRemoteSession = false,PasswordExpiryStatus = PasswordExpiryStatus.NotExpiring, PasswordExpiryDate = expiryDate}},
            new object[] {".", new TestData { IsNonCompliant = true, ExpectedLoadCount = 1,ExpectedShowCount = 1,ExpectedHideCount = 0,IsRemoteSession = false,PasswordExpiryStatus = PasswordExpiryStatus.ExpiringSoon, PasswordExpiryDate = expiryDate}},
        };
        
        [Test]
        [Category(TestCategory.UnitTests)]
        [TestCaseSource("TestDataSource")]
        public void CheckPasswordExpiryTest(string description, object data)
        {
            var testData = data as TestData;
            Assert.IsNotNull(testData,"testdata is null");
            var loadCount = 0;
            var showCount = 0;
            var hideCount = 0;
            var actual =
                CheckPasswordExpiryCommand.CheckPasswordExpiryPure(() =>
                {
                    loadCount++;
                    return Task.FromResult(new PasswordExpiryInfo(testData.PasswordExpiryDate,testData.PasswordExpiryStatus,testData.IsRemoteSession));
                }, info => testData.IsNonCompliant
                    ,(info) =>
                {
                            
                    showCount++;
                    return Task.FromResult(new Result<ToastNotificationVisibility>(ToastNotificationVisibility.Show));
                }, () =>
                {
                    hideCount++;
                    return Task.FromResult(new Result<ToastNotificationVisibility>(ToastNotificationVisibility.Hide));
                });
            Assert.AreEqual(testData.ExpectedLoadCount, loadCount, "LoadCount");
            Assert.AreEqual(testData.ExpectedShowCount, showCount, "ShowCount");
            Assert.AreEqual(testData.ExpectedHideCount, hideCount, "HideCount");
        }
    }
}