using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace PACommon
{
    /// <summary>
    /// Arbitrary precision decimal.
    /// All operations are exact, except for division. Division never determines more digits than the given precision.
    /// Source: https://gist.github.com/JcBernack/0b4eef59ca97ee931a2f45542b9ff06d
    /// Based on https://stackoverflow.com/a/4524254
    /// Author: Jan Christoph Bernack (contact: jc.bernack at gmail.com)
    /// License: public domain
    /// </summary>
    public struct BigDecimal : IComparable, IComparable<BigDecimal>
    {
        #region Static properties
        /// <summary>
        /// Specifies whether the significant digits should be truncated to the given precision after each operation.
        /// </summary>
        public static bool AlwaysTruncate = false;

        /// <summary>
        /// Sets the maximum precision of division operations.
        /// If AlwaysTruncate is set to true all operations are affected.
        /// </summary>
        public static int Precision = 50;
        #endregion

        #region Public properties
        public BigInteger Mantissa { get; set; }
        public int Exponent { get; set; }
        #endregion

        #region Constructors
        public BigDecimal(BigInteger mantissa, int exponent)
            : this()
        {
            Mantissa = mantissa;
            Exponent = exponent;
            Normalize();
            if (AlwaysTruncate)
            {
                Truncate();
            }
        }
        #endregion

        #region Convertesrs
        public static implicit operator BigDecimal(int value)
        {
            return new BigDecimal(value, 0);
        }

        public static implicit operator BigDecimal(double value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            double scaleFactor = 1;
            while (Math.Abs(value * scaleFactor - (double)mantissa) > 0)
            {
                exponent -= 1;
                scaleFactor *= 10;
                mantissa = (BigInteger)(value * scaleFactor);
            }
            return new BigDecimal(mantissa, exponent);
        }

        public static implicit operator BigDecimal(decimal value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            decimal scaleFactor = 1;
            while ((decimal)mantissa != value * scaleFactor)
            {
                exponent -= 1;
                scaleFactor *= 10;
                mantissa = (BigInteger)(value * scaleFactor);
            }
            return new BigDecimal(mantissa, exponent);
        }

        public static explicit operator double(BigDecimal value)
        {
            return (double)value.Mantissa * Math.Pow(10, value.Exponent);
        }

        public static explicit operator float(BigDecimal value)
        {
            return Convert.ToSingle((double)value);
        }

        public static explicit operator decimal(BigDecimal value)
        {
            return (decimal)value.Mantissa * (decimal)Math.Pow(10, value.Exponent);
        }

        public static explicit operator int(BigDecimal value)
        {
            return (int)(value.Mantissa * BigInteger.Pow(10, value.Exponent));
        }

        public static explicit operator uint(BigDecimal value)
        {
            return (uint)(value.Mantissa * BigInteger.Pow(10, value.Exponent));
        }
        #endregion

        #region Operators
        public static BigDecimal operator +(BigDecimal value)
        {
            return value;
        }

        public static BigDecimal operator -(BigDecimal value)
        {
            value.Mantissa *= -1;
            return value;
        }

        public static BigDecimal operator ++(BigDecimal value)
        {
            return value + 1;
        }

        public static BigDecimal operator --(BigDecimal value)
        {
            return value - 1;
        }

        public static BigDecimal operator +(BigDecimal left, BigDecimal right)
        {
            return Add(left, right);
        }

        public static BigDecimal operator -(BigDecimal left, BigDecimal right)
        {
            return Add(left, -right);
        }

        public static BigDecimal operator *(BigDecimal left, BigDecimal right)
        {
            return new BigDecimal(left.Mantissa * right.Mantissa, left.Exponent + right.Exponent);
        }

        public static BigDecimal operator /(BigDecimal dividend, BigDecimal divisor)
        {
            var exponentChange = Precision - (NumberOfDigits(dividend.Mantissa) - NumberOfDigits(divisor.Mantissa));
            if (exponentChange < 0)
            {
                exponentChange = 0;
            }
            dividend.Mantissa *= BigInteger.Pow(10, exponentChange);
            return new BigDecimal(dividend.Mantissa / divisor.Mantissa, dividend.Exponent - divisor.Exponent - exponentChange);
        }

        public static BigDecimal operator %(BigDecimal left, BigDecimal right)
        {
            return left - right * (left / right).Floor();
        }

        public static bool operator ==(BigDecimal left, BigDecimal right)
        {
            return left.Exponent == right.Exponent && left.Mantissa == right.Mantissa;
        }

        public static bool operator !=(BigDecimal left, BigDecimal right)
        {
            return left.Exponent != right.Exponent || left.Mantissa != right.Mantissa;
        }

        public static bool operator <(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) < right.Mantissa : left.Mantissa < AlignExponent(right, left);
        }

        public static bool operator >(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) > right.Mantissa : left.Mantissa > AlignExponent(right, left);
        }

        public static bool operator <=(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) <= right.Mantissa : left.Mantissa <= AlignExponent(right, left);
        }

        public static bool operator >=(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent ? AlignExponent(left, right) >= right.Mantissa : left.Mantissa >= AlignExponent(right, left);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Removes trailing zeros on the mantissa.
        /// </summary>
        public void Normalize()
        {
            if (Mantissa.IsZero)
            {
                Exponent = 0;
                return;
            }

            BigInteger remainder = 0;
            while (remainder == 0)
            {
                var shortened = BigInteger.DivRem(Mantissa, 10, out remainder);
                if (remainder == 0)
                {
                    Mantissa = shortened;
                    Exponent++;
                }
            }
        }

        /// <summary>
        /// Truncate the number to the given precision by removing the least significant digits.
        /// </summary>
        /// <returns>The truncated number</returns>
        public readonly BigDecimal Truncate(int precision)
        {
            // copy this instance (remember it's a struct)
            var shortened = this;
            // save some time because the number of digits is not needed to remove trailing zeros
            shortened.Normalize();
            // remove the least significant digits, as long as the number of digits is higher than the given Precision
            while (NumberOfDigits(shortened.Mantissa) > precision)
            {
                shortened.Mantissa /= 10;
                shortened.Exponent++;
            }
            // normalize again to make sure there are no trailing zeros left
            shortened.Normalize();
            return shortened;
        }

        public readonly BigDecimal Truncate()
        {
            return Truncate(Precision);
        }

        public readonly BigDecimal Floor()
        {
            return Truncate(NumberOfDigits(Mantissa) + Exponent);
        }

        public readonly string ToStringWithExponent(uint fractionsToDisplay = 0, int digitsToDisplay = -1, NumberFormatInfo? numberFormat = null)
        {
            if (Mantissa.IsZero)
            {
                return "0";
            }

            numberFormat ??= NumberFormatInfo.CurrentInfo;
            var mantissaStr = Mantissa.ToString(numberFormat);
            var actualExponent = Exponent;

            var isLimitedDigits = digitsToDisplay > -1;
            var decimalNumPre = actualExponent + digitsToDisplay < 0
                ? (int)Math.Min(fractionsToDisplay, (actualExponent + digitsToDisplay) * -1)
                : 0;
            var displayNum = digitsToDisplay + decimalNumPre;
            var oneIfNegative = BigInteger.IsNegative(Mantissa) ? 1 : 0;
            var mantissaNumLen = mantissaStr.Length - oneIfNegative;
            if (isLimitedDigits && mantissaNumLen > displayNum)
            {
                actualExponent += mantissaNumLen - displayNum;
                mantissaStr = mantissaStr[..(displayNum + oneIfNegative)];
            }

            if (actualExponent == 0)
            {
                return mantissaStr;
            }

            if (fractionsToDisplay != 0 && actualExponent < 0)
            {
                var decimalNum = (int)Math.Min(fractionsToDisplay, actualExponent * -1);
                actualExponent += decimalNum;
                mantissaStr = mantissaStr[..^decimalNum] +
                    numberFormat.PercentDecimalSeparator +
                    mantissaStr[^decimalNum..];
            }

            if (actualExponent == 0)
            {
                return mantissaStr;
            }

            return $"{mantissaStr}E{actualExponent}";
        }

        public readonly string ToStringFull(NumberFormatInfo? numberFormat = null)
        {
            if (Mantissa.IsZero)
            {
                return "0";
            }

            var txt = new StringBuilder();
            numberFormat ??= NumberFormatInfo.CurrentInfo;
            if (BigInteger.IsNegative(Mantissa))
            {
                txt.Append(numberFormat.NegativeSign);
            }

            var digitCount = NumberOfDigits(Mantissa);
            var divisor = digitCount > 1 ? BigInteger.Pow(10, digitCount - 1) : BigInteger.One;

            var dotWritten = false;
            var expRev = Exponent * -1;
            if (expRev >= digitCount)
            {
                txt.Append("0" + numberFormat.NumberDecimalSeparator);
                dotWritten = true;

                var decimalIndexFromEnd = expRev - 1;
                while (decimalIndexFromEnd >= digitCount)
                {
                    txt.Append('0');
                    decimalIndexFromEnd--;
                }
            }

            var divisorIndex = digitCount - expRev;

            for (var x = 0; x < digitCount; x++)
            {
                if (!dotWritten && x == divisorIndex)
                {
                    txt.Append(numberFormat.NumberDecimalSeparator);
                }

                var digitValue = BigInteger.Abs(Mantissa) / divisor % 10;
                txt.Append((char)(digitValue + '0'));
                divisor /= 10;
            }
            return txt.ToString();
        }

        public override readonly string ToString()
        {
            return ToStringWithExponent();
        }

        public readonly bool Equals(BigDecimal other)
        {
            return other.Mantissa.Equals(Mantissa) && other.Exponent == Exponent;
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }
            return obj is BigDecimal bigDec && Equals(bigDec);
        }

        public override readonly int GetHashCode()
        {
            unchecked
            {
                return (Mantissa.GetHashCode() * 397) ^ Exponent;
            }
        }

        public readonly int CompareTo(object? obj)
        {
            if (obj is null || obj is not BigDecimal)
            {
                throw new ArgumentException(null, nameof(obj));
            }
            return CompareTo((BigDecimal)obj);
        }

        public readonly int CompareTo(BigDecimal other)
        {
            return this < other ? -1 : (this > other ? 1 : 0);
        }
        #endregion

        #region Public functions
        public static int NumberOfDigits(BigInteger value)
        {
            return (int)Math.Ceiling(BigInteger.Log10(value * value.Sign));
        }

        public static BigDecimal Exp(double exponent)
        {
            var tmp = (BigDecimal)1;
            while (Math.Abs(exponent) > 100)
            {
                var diff = exponent > 0 ? 100 : -100;
                tmp *= Math.Exp(diff);
                exponent -= diff;
            }
            return tmp * Math.Exp(exponent);
        }

        public static BigDecimal Pow(double basis, double exponent)
        {
            var tmp = (BigDecimal)1;
            while (Math.Abs(exponent) > 100)
            {
                var diff = exponent > 0 ? 100 : -100;
                tmp *= Math.Pow(basis, diff);
                exponent -= diff;
            }
            return tmp * Math.Pow(basis, exponent);
        }

        public static bool TryParse(string value, [NotNullWhen(true)] out BigDecimal? result, NumberFormatInfo? numberFormat = null)
        {
            result = default;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            numberFormat ??= NumberFormatInfo.CurrentInfo;
            if (!TryParseMantissa(value, out var x, out var mantissa, out var exponent, out var hasExponent, numberFormat))
            {
                return false;
            }

            if (hasExponent)
            {
                if (
                    x + 1 >= value.Length ||
                    !int.TryParse(value[(x + 1)..], numberFormat, out var moreExponent)
                )
                {
                    return false;
                }
                exponent += moreExponent;
            }

            result = new BigDecimal(mantissa, exponent);
            return true;
        }

        public static BigDecimal Parse(string value, NumberFormatInfo? numberFormat = null)
        {
            return (BigDecimal)(TryParse(value, out var result, numberFormat)
                ? result
                : throw new ArgumentException(null, nameof(value)));
        }
        #endregion

        #region Private functions
        private static BigDecimal Add(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent
                ? new BigDecimal(AlignExponent(left, right) + right.Mantissa, right.Exponent)
                : new BigDecimal(AlignExponent(right, left) + left.Mantissa, left.Exponent);
        }

        /// <summary>
        /// Returns the mantissa of value, aligned to the exponent of reference.
        /// Assumes the exponent of value is larger than of reference.
        /// </summary>
        private static BigInteger AlignExponent(BigDecimal value, BigDecimal reference)
        {
            return value.Mantissa * BigInteger.Pow(10, value.Exponent - reference.Exponent);
        }

        private static bool TryParseMantissa(
            string value,
            out int x,
            out BigInteger mantissa,
            out int exponent,
            out bool hasExponent, NumberFormatInfo numberFormat
        )
        {
            hasExponent = false;
            mantissa = 0;
            exponent = 0;

            x = 0;
            var isNegative = false;
            if (value[x].ToString() == numberFormat.NegativeSign)
            {
                isNegative = true;
                x++;
            }

            var length = value.Length;
            while (x < length)
            {
                var c = value[x];
                if (c != '0')
                {
                    break;
                }
                x++;
            }

            while (x < length)
            {
                var c = value[x];
                if (c.ToString().Equals("E", StringComparison.CurrentCultureIgnoreCase))
                {
                    hasExponent = true;
                    return true;
                }
                if (c.ToString() == numberFormat.NumberDecimalSeparator)
                {
                    break;
                }

                if (c < '0' || c > '9')
                {
                    return false;
                }

                mantissa *= 10;
                mantissa += c - '0';
                x++;
            }

            x++;
            while (x < length)
            {
                var c = value[x];
                if (c.ToString().Equals("E", StringComparison.CurrentCultureIgnoreCase))
                {
                    hasExponent = true;
                    return true;
                }

                if (c < '0' || c > '9')
                {
                    return false;
                }

                mantissa *= 10;
                mantissa += c - '0';
                exponent--;
                x++;
            }

            if (isNegative)
            {
                mantissa = -mantissa;
            }
            return true;
        }
        #endregion
    }
}
