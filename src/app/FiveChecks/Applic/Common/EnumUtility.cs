using System;
using System.ComponentModel;
using System.Linq;
using LanguageExt;

namespace FiveChecks.Applic.Common
{
    public static class EnumUtility
    {
        public static Option<string> StringValueOf(Enum value)
        {
            var enumType = value.GetType();
            var stringValue = value.ToString();
            var fieldInfo = enumType.GetField(stringValue);
            if (fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes)
            {
                return attributes.Select(attribute => attribute.Description).First();
            }
            return Option<string>.None;
        }
    }
}