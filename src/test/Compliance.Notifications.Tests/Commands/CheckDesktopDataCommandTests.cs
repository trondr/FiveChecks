﻿using System;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.DesktopDataCheck;
using Compliance.Notifications.Applic.PasswordExpiryCheck;
using Compliance.Notifications.Tests.Common;
using LanguageExt.Common;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Commands
{
    [TestFixture()]
    [Category(TestCategory.UnitTests)]
    public class CheckDesktopDataCommandTests
    {
        public class TestData
        {
            public bool HasDesktopData { get; set; }
            public int ExpectedLoadCount { get; set; }
            public int ExpectedShowCount { get; set; }
            public int ExpectedHideCount { get; set; }
        }

        private static DateTime expiryDate = new DateTime(2020, 01, 13, 14, 49, 05);
        public static object[] TestDataSource =
        {
            new object[] {"Has desktop data.", new TestData {ExpectedLoadCount = 1,ExpectedShowCount = 0,ExpectedHideCount = 1,HasDesktopData = false}},
            new object[] {"Has no desktop data.", new TestData {ExpectedLoadCount = 1,ExpectedShowCount = 1,ExpectedHideCount = 0,HasDesktopData = true}},
        };

        [Test]
        [Category(TestCategory.UnitTests)]
        [TestCaseSource("TestDataSource")]
        public void CheckDesktopDataTest(string description, object data)
        {
            var testData = data as TestData;
            Assert.IsNotNull(testData, "testdata is null");
            var loadCount = 0;
            var showCount = 0;
            var hideCount = 0;
            var actual =
                CheckDesktopDataCommand.CheckDesktopDataPure(() =>
                {
                    loadCount++;
                    return Task.FromResult(new DesktopDataInfo(){HasDesktopData = testData.HasDesktopData,NumberOfFiles = 1,TotalSizeInBytes = 1});
                }, (time, s) =>
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