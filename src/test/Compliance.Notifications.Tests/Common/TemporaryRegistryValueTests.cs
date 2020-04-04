using System;
using System.Collections.Generic;
using NUnit.Framework;
using LanguageExt;
using Microsoft.Win32;

namespace Compliance.Notifications.Common.Tests
{
    [TestFixture]
    public class TemporaryRegistryValueTests
    {
        [Test]
        [Category(TestCategory.UnitTests)]
        public void NewTemporaryRegistryValueTest_RegistryKey_DoesNotExist_ErrorResult()
        {
            string nonExistingRegistryKeyPath = @"APathThat\Hardly\Exists";
            var actual = TemporaryRegistryValue.NewTemporaryRegistryValue(Registry.CurrentUser, nonExistingRegistryKeyPath, "SomeValueName", RegistryValueKind.DWord, 0);
            Assert.IsTrue(actual.IsFaulted);
            var expectedExceptionMessage = $"Sub key '{Registry.CurrentUser.Name}\\{nonExistingRegistryKeyPath}' does not exist.";
            var val = actual.Match(
                value =>
                {
                    Assert.IsFalse(actual.IsSuccess);
                    return Option<object>.None;
                },
                exception =>
                {
                    Assert.AreEqual(expectedExceptionMessage, exception.Message);
                    return Option<object>.None;
                });
        }


        [Test]
        [Category(TestCategory.UnitTests)]
        public void NewTemporaryRegistryValueTest_RegistryKey_Success()
        {
            var testSubKeyPath = "Compliance.Notifications.Tests";
            Registry.CurrentUser.DeleteSubKey(testSubKeyPath, false);
            using (Registry.CurrentUser.CreateSubKey(testSubKeyPath)){}

            Some<string> valueName = "SomeValueName";
            var actual = TemporaryRegistryValue.NewTemporaryRegistryValue(Registry.CurrentUser, testSubKeyPath, valueName, RegistryValueKind.DWord, 0);
            var val = actual.Match(
                value =>
                {
                    Assert.IsTrue(actual.IsSuccess,"Expected success");
                    Assert.IsTrue(F.RegistryValueExists(Registry.CurrentUser,testSubKeyPath,valueName),"Temporary value does not exist.");
                    value.Dispose();
                    Assert.IsFalse(F.RegistryValueExists(Registry.CurrentUser,testSubKeyPath,valueName.Value),"Temporary value was not deleted.");
                    return Option<object>.None;
                },
                exception =>
                {
                    Assert.IsFalse(true, exception.Message);
                    return Option<object>.None;
                });

            //Cleanup
            Registry.CurrentUser.DeleteSubKey(testSubKeyPath,false);
        }

        public static class IsSuccess
        {
            public const bool True = true;
            public const bool False = false;
        }

        public static class GetValueCallCount
        {
            public const int One = 1;
            public const int Zero = 0;
        }

        public static class SetValueCallCount
        {
            public const int One = 1;
            public const int Zero = 0;
        }

        public static class GetValueKindCallCount
        {
            public const int One = 1;
            public const int Zero = 0;
        }

        public static class DeleteValueCallCount
        {
            public const int One = 1;
            public const int Zero = 0;
        }

        public class TestData
        {
            public string ValueName;
            public object Value;
            public RegistryValueKind ValueKind;
            public object ExistingValue;
            public RegistryValueKind ExistingValueKind;
            public bool ExpectedIsSuccess;
            public string ExpectedErrorMessage;
            public int ExpectedGetValueCallCount;
            public int ExpectedGetValueKindCallCount;
            public int ExpectedSetValueCallCount;
            public int ExpectedDeleteValueCallCount;

            public TestData(string valueName, object value, RegistryValueKind valueKind, object existingValue, RegistryValueKind existingValueKind, bool expectedIsSuccess, string expectedErrorMessage, int expectedGetValueCallCount, int expectedGetValueKindCallCount, int expectedSetValueCallCount, int expectedDeleteValueCallCount)
            {
                ValueName = valueName;
                Value = value;
                ValueKind = valueKind;
                ExistingValue = existingValue;
                ExistingValueKind = existingValueKind;
                ExpectedIsSuccess = expectedIsSuccess;
                ExpectedErrorMessage = expectedErrorMessage;
                ExpectedGetValueCallCount = expectedGetValueCallCount;
                ExpectedGetValueKindCallCount = expectedGetValueKindCallCount;
                ExpectedSetValueCallCount = expectedSetValueCallCount;
                ExpectedDeleteValueCallCount = expectedDeleteValueCallCount;
            }
        }

        public static object[] TestDataSource =
        {
            new object[ ]{"Different value kind. Return error result.",new TestData("SomeTestValueName","SomeTestValue",RegistryValueKind.String, 0, RegistryValueKind.DWord, IsSuccess.False, "The existing registry value '[HKEY_CURRENT_USER\\Compliance.Notifications.Tests]SomeTestValueName' has different value kind: 'DWord!=String'\r\nParameter name: valueKind", GetValueCallCount.One, GetValueKindCallCount.One, SetValueCallCount.Zero, DeleteValueCallCount.Zero)}, 
            
            new object[]{ "Existing value, overwrite with temp value and restore original value on release", new TestData("SomeTestValueName", "SomeTestValue", RegistryValueKind.String, 0, RegistryValueKind.String, IsSuccess.True, "N/A", GetValueCallCount.One, GetValueKindCallCount.One, SetValueCallCount.One, DeleteValueCallCount.Zero) },

            new object[]{"No existing value, create temp value and delete temp value on release", new TestData("SomeTestValueName", "SomeTestValue", RegistryValueKind.String, null, RegistryValueKind.String, IsSuccess.True, "N/A", GetValueCallCount.One, GetValueKindCallCount.Zero, SetValueCallCount.One, DeleteValueCallCount.One) }
        };

        [Test, TestCaseSource("TestDataSource")]
        [Category(TestCategory.UnitTests)]
        public void NewTemporaryRegistryValueFTest(string description,object data)
        {
            var testData = data as TestData;
            Assert.NotNull(testData,"Test data was null");
            var testSubKeyPath = "Compliance.Notifications.Tests";
            Dictionary<string,object> registry = new Dictionary<string, object>();
            if(testData.ExistingValue != null)
                registry.Add(testData.ValueName, testData.ExistingValue);

            var actualGetValueCallCount = 0;
            var actualGetValueKindCallCount = 0;
            var actualSetValueCallCount = 0;
            var actualDeleteValueCallCount = 0;
            Func<string, object, object> getValue= (valueName, defaultValue) =>
            {
                
                actualGetValueCallCount++;
                if (registry.ContainsKey(valueName))
                    return registry[valueName];
                return defaultValue;
            };
            Func<string, RegistryValueKind> getValueKind= valueName =>
            {
                actualGetValueKindCallCount++;
                return testData.ExistingValueKind;
            };
            Action<string, object, RegistryValueKind> setValue= (valueName, value, valueKind) =>
            {
                actualSetValueCallCount++;
                if (registry.ContainsKey(valueName))
                    registry[valueName] = value;
                else
                {
                    registry.Add(valueName,value);
                }
            };
            Action<string> deleteValue= valueName =>
            {
                actualDeleteValueCallCount++;
                if (registry.ContainsKey(valueName))
                    registry.Remove(valueName);
            };

            var actual = TemporaryRegistryValue.NewTemporaryRegistryValueF(Registry.CurrentUser, testSubKeyPath, testData.ValueName, testData.ValueKind, testData.Value, getValue, getValueKind, setValue);
            Assert.AreEqual(testData.ExpectedGetValueCallCount, actualGetValueCallCount, "GetValue call count.");
            Assert.AreEqual(testData.ExpectedGetValueKindCallCount, actualGetValueKindCallCount, "GetValueKind call count.");
            Assert.AreEqual(testData.ExpectedSetValueCallCount, actualSetValueCallCount, "SetValue call count.");
            var res = actual.Match(
                value =>
                {
                    Assert.IsTrue(testData.ExpectedIsSuccess,"Did not expect success.");
                    Assert.AreEqual(testData.Value,getValue(testData.ValueName,null));
                    TemporaryRegistryValue.ReleaseTemporaryRegistryValueF(Registry.CurrentUser,testSubKeyPath, testData.ValueName, testData.ExistingValue, testData.ExistingValueKind,setValue,deleteValue);
                    Assert.AreEqual(testData.ExpectedDeleteValueCallCount, actualDeleteValueCallCount, "DeleteValue call count.");
                    return Option<object>.None;
                },
                exception =>
                {
                    Assert.IsTrue(!testData.ExpectedIsSuccess, exception.ToString());
                    Assert.AreEqual(testData.ExpectedErrorMessage,exception.Message);
                    return Option<object>.None;
                });
        }
    }
}