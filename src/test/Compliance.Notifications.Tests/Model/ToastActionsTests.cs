using Compliance.Notifications.Applic;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Tests.Common;
using LanguageExt;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Model
{
    [TestFixture()]
    public class ToastActionsTests
    {
        public class TestData
        {
            public string Arguments;
            public bool ExpectedReturnIsSome;

            public TestData(string arguments, bool expectedReturnIsSome)
            {
                Arguments = arguments;
                ExpectedReturnIsSome = expectedReturnIsSome;
            }
        }

        public static object[] TestDataSource =
        {
            new object[] {$"Arguments are null. Return None.", new TestData(null,false)},
            new object[] {$"Arguments are empty string. Return None.", new TestData(string.Empty,false)},
            new object[] {$"Arguments are some random string. Return None.", new TestData("srt3ij8",false)},
            new object[] {$"Incorrectly spelled action. Return None.", new TestData("action=Restart",false)},
            new object[] {$"Correctly spelled action. Return Some.", new TestData("action=restart",true)},
            new object[] {$"Incorrectly spelled action parameter. Return None.", new TestData("action1=restart",false)},
            new object[] {$"action={ToastActions.Restart}. Return Some.", new TestData($"action={ToastActions.Restart}",true)},
            new object[] {$"action={ToastActions.DiskCleanup}. Return Some.", new TestData($"action={ToastActions.DiskCleanup}",true)},
            new object[] {$"action={ToastActions.DiskAutoCleanup}. Return Some.", new TestData($"action={ToastActions.DiskAutoCleanup}",true)},
            new object[] {$"action={ToastActions.ChangePassword}. Return Some.", new TestData($"action={ToastActions.ChangePassword}",true)},
        };

        [Test, TestCaseSource("TestDataSource")]
        [Category(TestCategory.UnitTests)]
        public void ParseToastActionArgumentsTest(string description, object data)
        {
            var testData = data as TestData;
            Assert.NotNull(testData,"Test data is null");
            var actual = ToastActions.ParseToastActionArguments(testData.Arguments);
            actual.Match(func =>
            {
                Assert.IsTrue(testData.ExpectedReturnIsSome, "Expected None but was Some function");
                return Option<Unit>.None;
            }, () =>
            {
                Assert.IsFalse(testData.ExpectedReturnIsSome, "Expected Some function but was None");
                return Option<Unit>.None;
            });
        }


        public static object[] TestDataGroupSource =
        {
            new object[] {$"Arguments are null. Return None.", new TestData(null,false)},
            new object[] {$"Arguments are empty string. Return None.", new TestData(string.Empty,false)},
            new object[] {$"Arguments are some random string. Return None.", new TestData("srt3ij8",false)},
            new object[] {$"Incorrectly spelled key name. Return None.", new TestData("group1=Restart",false)},
            new object[] {$"Correctly spelled key name. Return Some.", new TestData("group=restart",true)},
        };

        [Test, TestCaseSource("TestDataGroupSource")]
        [Category(TestCategory.UnitTests)]
        public void ParseToastGropArgumentsTest(string description, object data)
        {
            var testData = data as TestData;
            Assert.NotNull(testData, "Test data is null");
            var actual = ToastGroups.ParseToastGroupArguments(testData.Arguments);
            actual.Match(func =>
            {
                Assert.IsTrue(testData.ExpectedReturnIsSome, "Expected None but was Some function");
                return Option<Unit>.None;
            }, () =>
            {
                Assert.IsFalse(testData.ExpectedReturnIsSome, "Expected Some function but was None");
                return Option<Unit>.None;
            });
        }
    }
}