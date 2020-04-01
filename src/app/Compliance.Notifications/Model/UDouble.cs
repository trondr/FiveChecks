using System;
using System.Globalization;
using Compliance.Notifications.Resources;

namespace Compliance.Notifications.Model
{
    public struct UDouble : IEquatable<UDouble>
    {
        /// <summary>
        /// Equivalent to <see cref="double.Epsilon"/>.
        /// </summary>
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static UDouble Epsilon = double.Epsilon;

        /// <summary>
        /// Represents the smallest possible value of <see cref="UDouble"/> (0).
        /// </summary>
        public static UDouble MinValue = 0d;

        /// <summary>
        /// Represents the largest possible value of <see cref="UDouble"/> (equivalent to <see cref="double.MaxValue"/>).
        /// </summary>
        public static UDouble MaxValue = double.MaxValue;

        /// <summary>
        /// Equivalent to <see cref="double.NaN"/>.
        /// </summary>
        public static UDouble NaN = double.NaN;

        /// <summary>
        /// Equivalent to <see cref="double.PositiveInfinity"/>.
        /// </summary>
        public static UDouble PositiveInfinity = double.PositiveInfinity;

        readonly double _value;

        public UDouble(double value)
        {
            if (double.IsNegativeInfinity(value)|| value < 0)
                throw new ArgumentException(strings.ValueNeedsToBePositive);
            _value = value;
        }

#pragma warning disable CA2225 // Operator overloads have named alternates
        public static implicit operator double(UDouble d)
        {
            return d._value;
        }

        public static implicit operator UDouble(double d)
        {
            return new UDouble(d);
        }

        public static bool operator <(UDouble a, UDouble b)
        {
            return a._value < b._value;
        }

        public static bool operator >(UDouble a, UDouble b)
        {
            return a._value > b._value;
        }

        public static bool operator ==(UDouble a, UDouble b)
        {
            return Math.Abs(a._value - b._value) < Epsilon;
        }

        public static bool operator !=(UDouble a, UDouble b)
        {
            return Math.Abs(a._value - b._value) > Epsilon;
        }

        public static bool operator <=(UDouble a, UDouble b)
        {
            return a._value <= b._value;
        }

        public static bool operator >=(UDouble a, UDouble b)
        {
            return a._value >= b._value;
        }

        public override bool Equals(object a)
        {
            return a is UDouble uDouble && this == uDouble;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public bool Equals(UDouble other)
        {
            return (Math.Abs(this._value - other._value) < Epsilon);
        }

        public override string ToString()
        {
            return _value.ToString(CultureInfo.InvariantCulture);
        }
#pragma warning restore CA2225 // Operator overloads have named alternates
#pragma warning restore CA2211 // Non-constant fields should not be visible
    }
}