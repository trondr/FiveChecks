using System;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Win32;

namespace Compliance.Notifications.Applic.Common
{
    public sealed class TemporaryRegistryValue : IDisposable
    {
        
        private readonly RegistryKey _baseKey;
        private readonly string _subKeyPath;
        private readonly string _valueName;
        private Option<object> _existingValue;
        private RegistryValueKind _existingValueKind = RegistryValueKind.None;

        private TemporaryRegistryValue(Some<RegistryKey> baseKey, Some<string> subKeyPath, Some<string> valueName, RegistryValueKind registryValueKind, Option<object> existingValue)
        {
            _baseKey = baseKey;
            _subKeyPath = subKeyPath;
            _valueName = valueName;
            _existingValueKind = registryValueKind;
            _existingValue = existingValue;
        }
        
        private void ReleaseUnmanagedResources()
        {
            TemporaryRegistryValue.ReleaseTemporaryRegistryValue(_baseKey, _subKeyPath, _valueName, _existingValue, _existingValueKind);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~TemporaryRegistryValue()
        {
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Create new temporary registry value. Testable version.
        /// </summary>
        /// <param name="baseKey"></param>
        /// <param name="valueName"></param>
        /// <param name="valueKind"></param>
        /// <param name="value"></param>
        /// <param name="getValue"></param>
        /// <param name="getValueKind"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public static Result<TemporaryRegistryValue> NewTemporaryRegistryValuePure(Some<RegistryKey> baseKey, Some<string> subKeyPath,Some<string> valueName, RegistryValueKind valueKind, Some<object> value, Func<string,object,object> getValue,Func<string,RegistryValueKind> getValueKind, Action<string,object,RegistryValueKind> setValue)
        {
            Try<TemporaryRegistryValue> TryNewTemporaryRegistryValueF() => () =>
            {
                if (getValue == null) throw new ArgumentNullException(nameof(getValue));
                if (setValue == null) throw new ArgumentNullException(nameof(setValue));
                Option<object> existingValue = getValue(valueName.Value, null);
                return existingValue.Match(o =>
                {
                    //If existing registry value has a different kind, return error.
                    var existingValueKind = getValueKind(valueName.Value);
                    if (existingValueKind != valueKind)
                        return new Result<TemporaryRegistryValue>(new ArgumentException(
                            $"The existing registry value '[{baseKey.Value}\\{subKeyPath}]{valueName}' has different value kind: '{existingValueKind}!={valueKind}'",
                            nameof(valueKind)));
                    else
                    {
                        setValue(valueName.Value, value.Value, valueKind);
                        return new Result<TemporaryRegistryValue>(new TemporaryRegistryValue(baseKey, subKeyPath, valueName, valueKind, existingValue));
                    }
                }, () =>
                {
                    setValue(valueName.Value, value.Value, valueKind);
                    return new Result<TemporaryRegistryValue>(new TemporaryRegistryValue(baseKey, subKeyPath, valueName, valueKind, existingValue));
                });
            };
            return TryNewTemporaryRegistryValueF().Try();
        }

        /// <summary>
        /// Create new temporary registry value.
        /// </summary>
        /// <param name="baseKey"></param>
        /// <param name="subKeyPath"></param>
        /// <param name="valueName"></param>
        /// <param name="registryValueKind"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Result<TemporaryRegistryValue> NewTemporaryRegistryValue(Some<RegistryKey> baseKey, Some<string> subKeyPath, Some<string> valueName, RegistryValueKind registryValueKind, Some<object> value)
        {
            Try<TemporaryRegistryValue> TryNewTemporaryRegistryValue() => () =>
            {
                using (var subKey = baseKey.Value.OpenSubKey(subKeyPath.Value,true))
                {
                    return subKey == null ?
                        new Result<TemporaryRegistryValue>(new ArgumentException($"Sub key '{baseKey.Value.Name}\\{subKeyPath.Value}' does not exist.")) :
                        NewTemporaryRegistryValuePure(baseKey, subKeyPath, valueName, registryValueKind, value, subKey.GetValue, subKey.GetValueKind, subKey.SetValue);
                }
            };
            return TryNewTemporaryRegistryValue().Try();
        }
        
        internal static Unit ReleaseTemporaryRegistryValuePure(Some<string> valueName, Option<object> existingValue, RegistryValueKind existingValueKind, Action<string, object, RegistryValueKind> setValue, Action<string> deleteValue)
        {
            return existingValue.Match(
            o =>
            {
                setValue(valueName.Value, o, existingValueKind);
                return Unit.Default;
            }, () =>
            {
                deleteValue(valueName);
                return Unit.Default;
            });
        }

        internal static void ReleaseTemporaryRegistryValue(Some<RegistryKey> baseKey, Some<string> subKeyPath, Some<string> valueName, Option<object> existingValue, RegistryValueKind existingValueKind)
        {
            Try<Unit> TryReleaseTemporaryRegistryValue() => () =>
            {
                using (var subKey = baseKey.Value.OpenSubKey(subKeyPath.Value,true))
                {
                    if (subKey == null)
                        throw new ArgumentException($"Sub key '{baseKey.Value.Name}\\{subKeyPath.Value}' does not exist.");
                    return ReleaseTemporaryRegistryValuePure(valueName, existingValue, existingValueKind,
                        subKey.SetValue, subKey.DeleteValue);
                }
            };
            TryReleaseTemporaryRegistryValue().Try().Match(unit => unit, exception =>
            {
                Logging.DefaultLogger.Warn($"Failed to release temporary registry value. {exception.ToExceptionMessage()}");
                return Unit.Default;
            });
        }
    }
}