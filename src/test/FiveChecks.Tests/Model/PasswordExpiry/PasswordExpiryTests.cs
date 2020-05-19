using System;
using System.Threading.Tasks;
using FiveChecks.Applic.PasswordExpiryCheck;
using FiveChecks.Tests.Applic;
using LanguageExt;
using NUnit.Framework;

namespace FiveChecks.Tests.Model.PasswordExpiry
{
    [TestFixture()]
    public class PasswordExpiryTests
    {
        public class TestData
        {
            public TestData(UserPasswordInfo userPasswordInfo, bool isRemoteSession, DateTime now, double expiryWarningDays, UserPasswordExpiryStatusInfo expectedUserPasswordExpiryStatusInfo)
            {
                UserPasswordInfo = userPasswordInfo;
                IsRemoteSession = isRemoteSession;
                Now = now;
                ExpiryWarningDays = expiryWarningDays;
                ExpectedUserPasswordExpiryStatusInfo = expectedUserPasswordExpiryStatusInfo;
            }

            public UserPasswordInfo UserPasswordInfo { get; }
            public bool IsRemoteSession { get; }
            public DateTime Now { get; }
            public double ExpiryWarningDays { get; }
            public UserPasswordExpiryStatusInfo ExpectedUserPasswordExpiryStatusInfo { get; }
        }

        private static readonly DateTime passwordExpirationDate = new DateTime(2020, 3, 11, 10, 33, 44);
        private static readonly DateTime nowLongBeforeExpirationDate = new DateTime(2020, 2, 10, 10, 33, 44);
        private static readonly DateTime nowBeforeExpirationDate = new DateTime(2020, 3, 10, 10, 33, 44);
        private static readonly DateTime nowAfterExpirationDate = new DateTime(2020, 3, 12, 10, 33, 44);
        private static readonly UserPasswordInfo userPasswordInfo = new UserPasswordInfo("someUser", passwordExpirationDate);

        public static readonly UserPasswordInfo userPasswordInfo2 = new UserPasswordInfo("someUser", new DateTime(2020, 07, 13, 10, 20, 08));
        public static object[] TestDataSource =
        {
            new object[] {"Password has expired.", new TestData(userPasswordInfo, false,nowAfterExpirationDate,11, new UserPasswordExpiryStatusInfo(userPasswordInfo,false,PasswordExpiryStatus.HasExpired) )},
            new object[] {"Password has not expired, but is expiring soon.", new TestData(userPasswordInfo, false,nowBeforeExpirationDate,11, new UserPasswordExpiryStatusInfo(userPasswordInfo,false,PasswordExpiryStatus.ExpiringSoon) )},
            new object[] {"Password has not expired.", new TestData(userPasswordInfo, false,nowLongBeforeExpirationDate,11, new UserPasswordExpiryStatusInfo(userPasswordInfo,false,PasswordExpiryStatus.NotExpiring) )},
            new object[] {"Password expires 2020-07-13.", new TestData(userPasswordInfo2, false,new DateTime(2020,05,01), 75, new UserPasswordExpiryStatusInfo(userPasswordInfo2, false,PasswordExpiryStatus.ExpiringSoon) )}
        };

        [Test]
        [Category(TestCategory.UnitTests)]
        [TestCaseSource("TestDataSource")]
        public async Task GetPasswordExpiryStatusFTest(string description, object data)
        {
            var testData = data as TestData;
            Assert.IsNotNull(testData,"Test data object is null");
            UserPasswordInfo GetUserPasswordInfo(string s) => testData.UserPasswordInfo;
            bool GetIsRemoteSession() => testData.IsRemoteSession;
            DateTime GetNow() => testData.Now;
            double GetExpiryWarningDays() => testData.ExpiryWarningDays;
            var actual = await PasswordExpire.GetPasswordExpiryStatusPure("someUserId", GetUserPasswordInfo, GetIsRemoteSession, GetNow, GetExpiryWarningDays);
            Assert.AreEqual(testData.ExpectedUserPasswordExpiryStatusInfo,actual);
        }
    }
}