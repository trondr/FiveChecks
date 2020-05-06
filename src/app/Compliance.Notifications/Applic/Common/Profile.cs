using System;
using System.Globalization;
using System.IO;
using System.Security;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Win32;

namespace Compliance.Notifications.Applic.Common
{
    public enum Context
    {
        Machine,
        User
    }

    public static class Profile
    {
        public static object GetProfileValue(Context context, Option<string> category, string valueName, object defaultValue)
        {
            var hive = context.ContextToRegistryHive();
            var policySubKeyPath = GetProfileSubKeyPath(category);
            using (var key = hive.OpenSubKey(policySubKeyPath))
            {
                var value = key?.GetValue(valueName, defaultValue);
                return value ?? defaultValue;
            }
        }

        public static string GetStringProfileValue(Context context, Option<string> category, string valueName, string defaultValue)
        {
            var value = GetProfileValue(context, category, valueName, defaultValue);
            return ObjectValueToString(value, defaultValue);
        }

        public static Result<Unit> SetStringProfileValue(Context context, Option<string> category, string valueName, string value)
        {
            return SetProfileValue(context, category, valueName, ObjectValueToString(value, value));
        }

        public static Result<Unit> SetProfileValue(Context context, Option<string> category, string valueName, object value)
        {
            try
            {
                var hive = context.ContextToRegistryHive();
                var policySubKeyPath = GetProfileSubKeyPath(category);
                using (var key = hive.CreateSubKey(policySubKeyPath))
                {
                    key?.SetValue(valueName, value);
                }
                return new Result<Unit>(Unit.Default);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException || ex is ObjectDisposedException ||
                    ex is UnauthorizedAccessException || ex is SecurityException || ex is IOException)
                {
                    return new Result<Unit>(new Exception($"Failed to set profile value '{valueName}'='{value}'", ex));
                }
                throw;
            }
        }

        public static string GetProfileSubKeyPath(Option<string> category)
        {
            return category.Match(
                cat => $"Software\\{ApplicationInfo.ApplicationCompanyName}\\{ApplicationInfo.ApplicationProductName}\\{cat}",
                () => $"Software\\{ApplicationInfo.ApplicationCompanyName}\\{ApplicationInfo.ApplicationProductName}");
        }

        public static object GetPolicyValue(Context context, Option<string> category, string valueName, object defaultValue)
        {
            var hive = context.ContextToRegistryHive();
            var policySubKeyPath = GetPolicySubKeyPath(category);
            using (var key = hive.OpenSubKey(policySubKeyPath))
            {
                var value = key?.GetValue(valueName, defaultValue);
                return value ?? defaultValue;
            }
        }

        public static string GetPolicySubKeyPath(Option<string> category)
        {
            return category.Match(
                cat => $"Software\\Policies\\{ApplicationInfo.ApplicationCompanyName}\\{ApplicationInfo.ApplicationProductName}\\{cat}",
                () => $"Software\\Policies\\{ApplicationInfo.ApplicationCompanyName}\\{ApplicationInfo.ApplicationProductName}");
        }

        public static bool GetBooleanPolicyValue(Context context, Option<string> category, string valueName, bool defaultValue)
        {
            var value = GetPolicyValue(context, category, valueName, defaultValue);
            return ObjectValueToBoolean(value, defaultValue);
        }

        public static int GetIntegerPolicyValue(Context context, Option<string> category, string valueName, int defaultValue)
        {
            var value = GetPolicyValue(context, category, valueName, defaultValue);
            return ObjectValueToInteger(value, defaultValue);
        }

        public static string GetStringPolicyValue(Context context, Option<string> category, string valueName, string defaultValue)
        {
            var value = GetPolicyValue(context, category, valueName, defaultValue);
            return ObjectValueToString(value, defaultValue);
        }

        private static string ObjectValueToString(object value, string defaultValue)
        {
            try
            {
                var intValue = value != null ? Convert.ToString(value, CultureInfo.InvariantCulture) : defaultValue;
                return intValue;
            }
            catch (ArgumentException e)
            {
                Logging.DefaultLogger.Debug($"Failed to convert object value {value} to string. {e.ToExceptionMessage()}");
                return defaultValue;
            }
        }

        private static int ObjectValueToInteger(object value, int defaultValue)
        {
            try
            {
                var intValue = value != null ? Convert.ToInt32(value, CultureInfo.InvariantCulture) : defaultValue;
                return intValue;
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
                {
                    Logging.DefaultLogger.Debug($"Failed to convert object value {value} to integer. {ex.ToExceptionMessage()}");
                    return defaultValue;
                }
                throw;
            }
        }

        public static bool ObjectValueToBoolean(object value, bool defaultValue)
        {
            try
            {
                var booleanValue = value != null ? Convert.ToBoolean(value, CultureInfo.InvariantCulture) : defaultValue;
                return booleanValue;
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is InvalidCastException)
                {
                    Logging.DefaultLogger.Debug($"Failed to convert object value {value} to boolean. {ex.ToExceptionMessage()}");
                    return defaultValue;
                }
                throw;
            }
        }

        public static bool ToBoolean(this int i)
        {
            return i != 0;
        }

        public static Option<string> GetPolicyCategory(this Type type)
        {
            if (type == null) return Option<string>.None;
            if (type.Namespace == null) return Option<string>.None;
            var segments = type.Namespace.Split('.');
            return segments[segments.Length - 1];
        }

        public static RegistryKey ContextToRegistryHive(this Context context)
        {
            switch (context)
            {
                case Context.Machine:
                    return Registry.LocalMachine;
                case Context.User:
                    return Registry.CurrentUser;
                default:
                    throw new ArgumentOutOfRangeException(nameof(context), context, null);
            }
        }

        public static bool PolicyCategoryIsDisabled(Option<string> policyCategory, ComplianceAction complianceAction, bool defaultValue)
        {
            var valueName = ComplianceActionToDisabledValueName(complianceAction);
            var isDisabled = GetBooleanPolicyValue(Context.Machine, policyCategory, valueName, defaultValue);
            return isDisabled;
        }

        private static string ComplianceActionToDisabledValueName(ComplianceAction complianceAction)
        {
            switch (complianceAction)
            {
                case ComplianceAction.Notification:
                    return "DisableNotification";
                case ComplianceAction.Measurement:
                    return "DisableMeasurement";
                default:
                    throw new ArgumentOutOfRangeException(nameof(complianceAction), complianceAction, null);
            }
        }

        public static string GetCompanyName()
        {
            var companyName = Profile.GetStringPolicyValue(Context.Machine, Option<string>.None, "CompanyName", "My Company AS");
            return companyName;
        }

        public static bool IsNotificationDisabled(bool defaultValue, Some<Type> checkCommandType)
        {
            var policyCategory = checkCommandType.Value.GetPolicyCategory();
            var isDisabled = Profile.PolicyCategoryIsDisabled(policyCategory, ComplianceAction.Notification, defaultValue);
            if (isDisabled) Logging.DefaultLogger.Info($"Notification {checkCommandType.Value.Name} is disabled.");
            return isDisabled;
        }

        public static bool IsMeasurementDisabled(bool defaultValue, Some<Type> checkCommandType)
        {
            var policyCategory = checkCommandType.Value.GetPolicyCategory();
            var isDisabled = Profile.PolicyCategoryIsDisabled(policyCategory, ComplianceAction.Measurement, defaultValue);
            if (isDisabled) Logging.DefaultLogger.Info($"Measurement {checkCommandType.Value.Name} is disabled.");
            return isDisabled;
        }

        public static bool IsCheckDisabled(bool defaultValue, Type checkCommandType)
        {
            return IsMeasurementDisabled(defaultValue, checkCommandType) || IsNotificationDisabled(defaultValue, checkCommandType);
        }
    }
}
