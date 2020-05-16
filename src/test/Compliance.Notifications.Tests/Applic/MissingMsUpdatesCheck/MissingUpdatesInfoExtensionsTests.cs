using NUnit.Framework;
using Compliance.Notifications.Applic.MissingMsUpdatesCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance.Notifications.Tests.Applic;

namespace Compliance.Notifications.Tests
{
    [TestFixture()]
    public class MissingUpdatesInfoExtensionsTests
    {
        public static DateTime marchDeadline = new DateTime(2020, 03, 20);
        public static DateTime changedDeadline = new DateTime(2020, 03, 25);


        public static DateTime firstMeasuredMissing = new DateTime(2020, 03, 29);
        public static DateTime now = new DateTime(2020, 05, 13);

        public static MissingMsUpdatesInfo previousInfo = new MissingMsUpdatesInfo
        {
            Updates = new List<MsUpdate>
            {
                new MsUpdate
                {
                    ArticleId = "4538156",
                    Name =
                        "2020-02 Cumulative Update for .NET Framework 3.5, 4.7.2 and 4.8 for Windows 10 Version 1809 for x64 (KB4538156)",
                    Deadline = marchDeadline, FirstMeasuredMissing = firstMeasuredMissing
                },
                new MsUpdate
                {
                    ArticleId = "4494174",
                    Name = "2020-01 Update for Windows 10 Version 1809 for x64-based Systems (KB4494174)",
                    Deadline = marchDeadline, FirstMeasuredMissing = firstMeasuredMissing
                },
                new MsUpdate
                {
                    ArticleId = "4549947",
                    Name =
                        "2020-04 Servicing Stack Update for Windows 10 Version 1809 for x64-based Systems (KB4549947)",
                    Deadline = marchDeadline, FirstMeasuredMissing = firstMeasuredMissing
                },
                new MsUpdate
                {
                    ArticleId = "4549949",
                    Name = "2020-04 Cumulative Update for Windows 10 Version 1809 for x64-based Systems (KB4549949)",
                    Deadline = marchDeadline, FirstMeasuredMissing = firstMeasuredMissing
                },
                new MsUpdate
                {
                    ArticleId = "3104046",
                    Name =
                        "Office 365 Client Update - Semi-annual Channel Version 1908 for x64 based Edition (Build 11929.20708)",
                    Deadline = marchDeadline, FirstMeasuredMissing = firstMeasuredMissing
                }
            }
        };

        public static MissingMsUpdatesInfo previousInfoEmpty = MissingMsUpdatesInfo.Default;


        [Test]
        [Category(TestCategory.UnitTests)]
        public void UpdateTestPreviousInfoNonEmptyCurrentInfoEmpty()
        {
            var currentInfo = MissingMsUpdatesInfo.Default;
            var actual = previousInfo.Update(currentInfo);
            Assert.AreEqual(0,actual.Updates.Count,"Updates count");
            Assert.IsTrue(previousInfo.Updates.Count > 0,"Previous info was mutated.");
        }
        
        [Test]
        [Category(TestCategory.UnitTests)]
        public void UpdateTestPreviousInfoEmpty()
        {
            var currentInfo = new MissingMsUpdatesInfo()
            {
                Updates = new List<MsUpdate>
                {
                    new MsUpdate
                    {
                        ArticleId = "4538156",
                        Name =
                            "2020-02 Cumulative Update for .NET Framework 3.5, 4.7.2 and 4.8 for Windows 10 Version 1809 for x64 (KB4538156)",
                        Deadline = marchDeadline, FirstMeasuredMissing = now
                    },
                    new MsUpdate
                    {
                        ArticleId = "4549947",
                        Name =
                            "2020-04 Servicing Stack Update for Windows 10 Version 1809 for x64-based Systems (KB4549947)",
                        Deadline = marchDeadline, FirstMeasuredMissing = now
                    },
                }
            };
            var actual = previousInfoEmpty.Update(currentInfo);
            Assert.AreEqual(0,previousInfoEmpty.Updates.Count, "Previous info has been mutated.");
            Assert.AreEqual(currentInfo.Updates.Count,actual.Updates.Count, "Updates count is not equal");
            Assert.AreEqual(currentInfo.Updates[0].ArticleId, actual.Updates[0].ArticleId, "ArticleId");
            Assert.AreEqual(currentInfo.Updates[0].Name, actual.Updates[0].Name, "Name");
            Assert.AreEqual(currentInfo.Updates[0].Deadline, actual.Updates[0].Deadline, "Deadline");
            Assert.AreEqual(now, actual.Updates[0].FirstMeasuredMissing, "FirstMeasuredMissing");

            Assert.AreEqual(currentInfo.Updates[1].ArticleId, actual.Updates[1].ArticleId, "ArticleId");
            Assert.AreEqual(currentInfo.Updates[1].Name, actual.Updates[1].Name, "Name");
            Assert.AreEqual(currentInfo.Updates[1].Deadline, actual.Updates[1].Deadline, "Deadline");
            Assert.AreEqual(currentInfo.Updates[1].FirstMeasuredMissing, actual.Updates[1].FirstMeasuredMissing, "FirstMeasuredMissing");
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void UpdateTestPreviousInfoNonEmpty()
        {
            var currentInfo = new MissingMsUpdatesInfo()
            {
                Updates = new List<MsUpdate>
                {
                    new MsUpdate
                    {
                        ArticleId = "4538156",
                        Name =
                            "2020-02 Cumulative Update for .NET Framework 3.5, 4.7.2 and 4.8 for Windows 10 Version 1809 for x64 (KB4538156)",
                        Deadline = marchDeadline, FirstMeasuredMissing = now
                    },
                    new MsUpdate
                    {
                        ArticleId = "4549947",
                        Name =
                            "2020-04 Servicing Stack Update for Windows 10 Version 1809 for x64-based Systems (KB4549947)",
                        Deadline = changedDeadline, FirstMeasuredMissing = now
                    },
                    new MsUpdate
                    {
                        ArticleId = "123456",
                        Name =
                            "2020-04 Some update (KB123456)",
                        Deadline = changedDeadline, FirstMeasuredMissing = now
                    },
                }
            };
            var actual = previousInfo.Update(currentInfo);
            Assert.AreEqual(5, previousInfo.Updates.Count, "Previous info has been mutated.");
            Assert.AreEqual(currentInfo.Updates.Count, actual.Updates.Count, "Updates count is not equal");
            Assert.AreEqual(currentInfo.Updates[0].ArticleId, actual.Updates[0].ArticleId, "ArticleId");
            Assert.AreEqual(currentInfo.Updates[0].Name, actual.Updates[0].Name, "Name");
            Assert.AreEqual(marchDeadline, actual.Updates[0].Deadline, "Deadline");
            Assert.AreEqual(firstMeasuredMissing, actual.Updates[0].FirstMeasuredMissing, "FirstMeasuredMissing");

            Assert.AreEqual(currentInfo.Updates[1].ArticleId, actual.Updates[1].ArticleId, "ArticleId");
            Assert.AreEqual(currentInfo.Updates[1].Name, actual.Updates[1].Name, "Name");
            Assert.AreEqual(changedDeadline, actual.Updates[1].Deadline, "Deadline");
            Assert.AreEqual(firstMeasuredMissing, actual.Updates[1].FirstMeasuredMissing, "FirstMeasuredMissing");

            Assert.AreEqual(currentInfo.Updates[2].ArticleId, actual.Updates[2].ArticleId, "ArticleId");
            Assert.AreEqual(currentInfo.Updates[2].Name, actual.Updates[2].Name, "Name");
            Assert.AreEqual(currentInfo.Updates[2].Deadline, actual.Updates[2].Deadline, "Deadline");
            Assert.AreEqual(currentInfo.Updates[2].FirstMeasuredMissing, actual.Updates[2].FirstMeasuredMissing, "FirstMeasuredMissing");
        }
    }
}