using LanguageExt;
using LanguageExt.Common;
using Microsoft.Win32;

namespace Compliance.Notifications.Common
{
    public static class RegistryOperations
    {
        public static Result<Unit> SetRegistryValue(Some<RegistryKey> baseKey, Some<string> subKeyPath, Some<string> valueName, Some<object> value, RegistryValueKind valueKind)
        {
            return F.TryFunc<Unit>(() =>
            {
                using (var key = baseKey.Value.OpenSubKey(subKeyPath.Value, true))
                {
                    key?.SetValue(valueName.Value, value.Value, valueKind);
                }
                return new Result<Unit>(Unit.Default);
            });
        }

        public static Result<Unit> DeleteRegistryValue(Some<RegistryKey> baseKey, Some<string> subKeyPath, Some<string> valueName)
        {
            return F.TryFunc<Unit>(() =>
            {
                if (RegistryValueExists(baseKey, subKeyPath, valueName))
                {
                    using (var key = baseKey.Value.OpenSubKey(subKeyPath.Value, true))
                    {
                        key?.DeleteValue(valueName.Value);
                    }
                }
                return new Result<Unit>(Unit.Default);
            });
        }

        /// <summary>
        /// Check if a registry key exists.
        /// </summary>
        /// <param name="baseKey">Example: Registry.LocalMachine</param>
        /// <param name="subKeyPath"></param>
        /// <returns></returns>
        public static bool RegistryKeyExists(Some<RegistryKey> baseKey, string subKeyPath)
        {
            using (var key = baseKey.Value.OpenSubKey(subKeyPath))
            {
                return key != null;
            }
        }

        /// <summary>
        /// Check if a registry value exists.
        /// </summary>
        /// <param name="baseKey"></param>
        /// <param name="subKeyPath"></param>
        /// <param name="valueName"></param>
        /// <returns></returns>
        public static bool RegistryValueExists(Some<RegistryKey> baseKey, Some<string> subKeyPath, Some<string> valueName)
        {
            using (var regKey = baseKey.Value.OpenSubKey(subKeyPath.Value))
            {
                return regKey?.GetValue(valueName.Value, null) != null;
            }
        }

        public static bool MultiStringRegistryValueExistsAndHasStrings(Some<RegistryKey> baseKey, Some<string> subKeyPath, Some<string> valueName)
        {
            using (var key = baseKey.Value.OpenSubKey(subKeyPath.Value))
            {
                var value = key?.GetValue(valueName);
                if (value == null) return false;
                if (value is string[] stringArray)
                {
                    return stringArray.Length > 0;
                }
            }
            return false;
        }
    }
}
