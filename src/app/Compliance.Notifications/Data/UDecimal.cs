using System;
using System.Globalization;
using Compliance.Notifications.Resources;

namespace Compliance.Notifications.Data
{
    public struct UDecimal : IEquatable<UDecimal>
    {
        
#pragma warning disable CA2211 // Non-constant fields should not be visible
        /// <summary>
        /// Represents the smallest possible value of <see cref="Compliance.Notifications.Data.UDecimal"/> (0).
        /// </summary>
        public static UDecimal MinValue = 0M;

        /// <summary>
        /// Represents the largest possible value of <see cref="Compliance.Notifications.Data.UDecimal"/> (equivalent to <see cref="decimal.MaxValue"/>).
        /// </summary>
        public static UDecimal MaxValue = decimal.MaxValue;

        readonly decimal _value;

        public UDecimal(decimal value)
        {
            if (value < 0)
                throw new ArgumentException(Resource_Strings.ValueNeedsToBePositive);
            _value = value;
        }

#pragma warning disable CA2225 // Operator overloads have named alternates
        public static implicit operator decimal(UDecimal d)
        {
            return d._value;
        }

        public static implicit operator UDecimal(decimal d)
        {
            return new UDecimal(d);
        }

        public static bool operator <(UDecimal a, UDecimal b)
        {
            return a._value < b._value;
        }

        public static bool operator >(UDecimal a, UDecimal b)
        {
            return a._value > b._value;
        }

        public static bool operator ==(UDecimal a, UDecimal b)
        {
            return a._value == b._value;
        }

        public static bool operator !=(UDecimal a, UDecimal b)
        {
            return a._value != b._value;
        }

        public static bool operator <=(UDecimal a, UDecimal b)
        {
            return a._value <= b._value;
        }

        public static bool operator >=(UDecimal a, UDecimal b)
        {
            return a._value >= b._value;
        }

        public override bool Equals(object a)
        {
            return a is UDecimal uDecimal && this == uDecimal;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public bool Equals(UDecimal other)
        {
            return this._value == other._value;
        }

        public override string ToString()
        {
            return _value.ToString(CultureInfo.CurrentCulture);
        }
#pragma warning restore CA2225 // Operator overloads have named alternates
#pragma warning restore CA2211 // Non-constant fields should not be visible
    }
}
