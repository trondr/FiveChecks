using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Compliance.Notifications.Applic.Common;
using LanguageExt;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Applic.Common
{
    [TestFixture()]
    public class DoubleCheckTests
    {
        public class DoubleCheckTestData
        {
            public string ActionName { get; set; }
            public DateTime Now { get; set; }
            public TimeSpan Threshold { get; set; } = new TimeSpan(0,0,60);
            public SortedDictionary<string, DateTime> TimeStampDictionary { get; set; }
            public bool ExpectedShouldRunDoubleCheck { get; set; }
        }

        public static DateTime Now = new DateTime(2020, 5, 1, 20, 05, 30);
        public static DateTime OutSideThreshold = Now.AddSeconds(-120);
        public static DateTime InSideThreshold = Now.AddSeconds(-25);
        private static SortedDictionary<string, DateTime> EmptyTimeStampDictionary = new SortedDictionary<string, DateTime>();
        private static SortedDictionary<string, DateTime> WithInThresholdTimeStampDictionary = new SortedDictionary<string, DateTime> {{"TestAction",InSideThreshold}};
        private static SortedDictionary<string, DateTime> OutsideThresholdTimeStampDictionary = new SortedDictionary<string, DateTime> { { "TestAction", OutSideThreshold } };


        public static object[] DoubleCheckTestDataSource = 
        {
            new object[]{"Empty TimeStamp Dictionary", new DoubleCheckTestData {ActionName = "TestAction", Now = Now, TimeStampDictionary = EmptyTimeStampDictionary, ExpectedShouldRunDoubleCheck = true} },

            new object[]{"Within threshold TimeStamp Dictionary", new DoubleCheckTestData {ActionName = "TestAction", Now = Now, TimeStampDictionary = WithInThresholdTimeStampDictionary, ExpectedShouldRunDoubleCheck = false} },

            new object[]{"Outside threshold TimeStamp Dictionary", new DoubleCheckTestData {ActionName = "TestAction", Now = Now, TimeStampDictionary = OutsideThresholdTimeStampDictionary, ExpectedShouldRunDoubleCheck = true} }
        };
        
        [Test()]
        [Category(TestCategory.UnitTests)]
        [TestCaseSource("DoubleCheckTestDataSource")]
        public void ShouldRunDoubleCheckPureTest(string description, object testDataObject)
        {
            var testData = testDataObject as DoubleCheckTestData;
            Assert.IsNotNull(testData,"Test data is null");
            var actual = DoubleCheck.ShouldRunDoubleCheckPure(testData.ActionName, testData.TimeStampDictionary.ToImmutableDictionary(), testData.Now, testData.Threshold);
            Assert.AreEqual(testData.ExpectedShouldRunDoubleCheck, actual, "Should run double check.");
        }
    }

    
}