﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace HigherArithmetics.Numerics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  ///     Big Rational Number
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public readonly struct BigRational :
      IEquatable<BigRational>,
      IComparable<BigRational>,
      ISerializable,
      IFormattable,
      ISpanFormattable,
      IParsable<BigRational>,
      ISpanParsable<BigRational>,
      IConvertible,
      IComparable {
    #region Create Algorithm

    private static readonly Regex s_ParseRegex = 
      new (@"^\s*(?<sign>[+-])?\s*(?<int>[0-9]+)?(\.(?<fraction>[0-9]+)?(?<period>\([0-9]+\))?)?([eE](?<exp>[+-]?[0-9]+)?)?\s*$");

    private static bool TryParseDecimal(string? source, NumberStyles styles, IFormatProvider? formatProvider,
        out BigRational result) {
      result = NaN;

      if (string.IsNullOrWhiteSpace(source))
        return false;

      var match = s_ParseRegex.Match(source);

      if (!match.Success)
        return false;

      string signPart = match.Groups["sign"].Value;
      string intPart = match.Groups["int"].Value;
      string fractionPart = match.Groups["fraction"].Value;
      string periodPart = match.Groups["period"].Value.Trim('(', ')');
      string expPart = match.Groups["exp"].Value;

      if (string.IsNullOrEmpty(intPart) &&
          string.IsNullOrEmpty(fractionPart) &&
          string.IsNullOrEmpty(periodPart))
        return false;

      result = 0;

      if (!string.IsNullOrEmpty(intPart))
        result += BigInteger.Parse(intPart, styles, formatProvider);

      if (!string.IsNullOrEmpty(fractionPart))
        result += new BigRational(BigInteger.Parse(fractionPart, styles, formatProvider),
            BigInteger.Pow(10, fractionPart.Length));

      if (!string.IsNullOrEmpty(periodPart))
        result += new BigRational(BigInteger.Parse(periodPart, styles, formatProvider),
                      BigInteger.Pow(10, periodPart.Length) - 1) /
                  BigInteger.Pow(10, fractionPart.Length);

      if (!string.IsNullOrEmpty(expPart)) {
        if (!int.TryParse(expPart, out int exp) && exp > 1_000_000_000) {
          result = NaN;

          return false;
        }

        var factor = BigInteger.Pow(10, Math.Abs(exp));

        if (exp < 0)
          result /= factor;
        else
          result *= factor;
      }

      if (signPart == "-")
        result = -result;

      return true;
    }

    private static bool TryParseNatural(string? value, NumberStyles styles, IFormatProvider? formatProvider,
        out BigRational result) {
      result = NaN;

      if (string.IsNullOrWhiteSpace(value))
        return false;

      value = value.Trim();

      if ("NaN".Equals(value, StringComparison.OrdinalIgnoreCase)) {
        result = NaN;

        return true;
      }

      if ("+Inf".Equals(value, StringComparison.OrdinalIgnoreCase)) {
        result = PositiveInfinity;

        return true;
      }

      if ("-Inf".Equals(value, StringComparison.OrdinalIgnoreCase)) {
        result = NegativeInfinity;

        return true;
      }

      var parts = value.Split('/', '\\', ':');

      if (parts.Length > 2)
        return false;

      if (parts.Length == 1) {
        if (BigInteger.TryParse(value, styles, formatProvider, out var v)) {
          result = new BigRational(v);

          return true;
        }

        return false;
      }

      if (BigInteger.TryParse(parts[0], styles, formatProvider, out var a) &&
          BigInteger.TryParse(parts[1], styles, formatProvider, out var b)) {
        result = new BigRational(a, b);

        return true;
      }

      return false;
    }

    private static bool TryCast(object? value, out BigRational result) {
      result = NaN;

      if (value is null)
        return false;

      if (value is BigRational exact) {
        result = exact;

        return true;
      }

      if (value is bool flag) {
        result = flag ? One : Zero;

        return true;
      }

      if (value is sbyte i8) {
        result = i8;

        return true;
      }

      if (value is short i16) {
        result = i16;

        return true;
      }

      if (value is int i32) {
        result = i32;

        return true;
      }

      if (value is int i64) {
        result = i64;

        return true;
      }

      if (value is byte ui8) {
        result = ui8;

        return true;
      }

      if (value is ushort ui16) {
        result = ui16;

        return true;
      }

      if (value is uint ui32) {
        result = ui32;

        return true;
      }

      if (value is uint ui64) {
        result = ui64;

        return true;
      }

      if (value is BigInteger bi) {
        result = bi;

        return true;
      }

      if (value is float fp32) {
        result = fp32;

        return true;
      }

      if (value is float fp64) {
        result = fp64;

        return true;
      }

      if (value is decimal deValue) {
        result = deValue;

        return true;
      }

      return TryParse(value.ToString(), out result);
    }

    #endregion Create Algorithm

    #region Create

    // Deserialization
    private BigRational(SerializationInfo info, StreamingContext context) {
      if (info is null)
        throw new ArgumentNullException(nameof(info));

      var numerator = BigInteger.Parse(info.GetString("Numerator") ?? "");
      var denominator = BigInteger.Parse(info.GetString("Denominator") ?? "");

      if (numerator < 0 && denominator < 0) {
        numerator = -numerator;
        denominator = -denominator;
      }
      else if (numerator == 0) {
        if (denominator != 0)
          denominator = 1;
      }
      else if (denominator == 0) {
        if (numerator < 0)
          numerator = -1;
        else
          numerator = 1;
      }
      else if (denominator < 0) {
        numerator = -numerator;
        denominator = -denominator;
      }

      if (denominator == 0) {
        Numerator = numerator;
        Denominator = denominator;
      }
      else {
        var gcd = BigInteger.GreatestCommonDivisor(denominator, numerator < 0 ? -numerator : numerator);

        Numerator = numerator / gcd;
        Denominator = denominator / gcd;
      }
    }

    /// <summary>
    ///     Standard rational constructor
    /// </summary>
    /// <param name="numerator">numerator</param>
    /// <param name="denominator">denominator</param>
    public BigRational(BigInteger numerator, BigInteger denominator) {
      if (numerator < 0 && denominator < 0) {
        numerator = -numerator;
        denominator = -denominator;
      }
      else if (numerator == 0) {
        if (denominator != 0)
          denominator = 1;
      }
      else if (denominator == 0) {
        if (numerator < 0)
          numerator = -1;
        else
          numerator = 1;
      }
      else if (denominator < 0) {
        numerator = -numerator;
        denominator = -denominator;
      }

      if (denominator == 0) {
        Numerator = numerator;
        Denominator = denominator;
      }
      else {
        var gcd = BigInteger.GreatestCommonDivisor(denominator, numerator < 0 ? -numerator : numerator);

        Numerator = numerator / gcd;
        Denominator = denominator / gcd;
      }
    }

    /// <summary>
    ///     Standard rational constructor
    /// </summary>
    /// <param name="pair">tuple with numerator and denominator</param>
    public BigRational((BigInteger numerator, BigInteger denominator) pair)
        : this(pair.numerator, pair.denominator) {
    }

    /// <summary>
    ///     Standard rational constructor (from integer)
    /// </summary>
    /// <param name="value">integer value</param>
    public BigRational(BigInteger value) : this(value, 1) {
    }

    /// <summary>
    ///     Standard rational constructor (from integer)
    /// </summary>
    /// <param name="value">integer value</param>
    public BigRational(int value) : this(value, 1) {
    }

    /// <summary>
    ///     Standard rational constructor (from integer)
    /// </summary>
    /// <param name="value">integer value</param>
    public BigRational(long value) : this(value, 1) {
    }

    /// <summary>
    ///     From character numeric value ⅝ => 5 / 8
    /// </summary>
    public BigRational(char value) {
      if (value == '∞') {
        Numerator = 1;
        Denominator = 0;

        return;
      }

      if (char.GetNumericValue(value) < 0) {
        Numerator = 0;
        Denominator = 0;

        return;
      }

      long factor = 1_260_000;

      BigInteger numerator = (long)(char.GetNumericValue(value) * factor + 0.5);
      BigInteger denominator = factor;

      var gcd = BigInteger.GreatestCommonDivisor(denominator, numerator < 0 ? -numerator : numerator);

      Numerator = numerator / gcd;
      Denominator = denominator / gcd;
    }

    /// <summary>
    ///     From Decimal value
    /// </summary>
    /// <param name="value">Decimal value</param>
    public BigRational(decimal value) {
      var (m, e) = Decimals.DeconstructDecimal(value);

      var nom = m;
      BigInteger den = 1;

      if (e > 0)
        nom *= BigInteger.Pow(10, e);
      else
        den = BigInteger.Pow(10, -e);

      var gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(den), nom);

      Numerator = nom / gcd;
      Denominator = den / gcd;
    }

    /// <summary>
    ///     From Float value
    /// </summary>
    /// <param name="value">Float value</param>
    public BigRational(float value) {
      if (0 == value) {
        Numerator = 0;
        Denominator = 1;

        return;
      }

      if (float.IsPositiveInfinity(value)) {
        Numerator = 1;
        Denominator = 0;

        return;
      }

      if (float.IsNegativeInfinity(value)) {
        Numerator = -1;
        Denominator = 0;

        return;
      }

      if (float.IsNaN(value)) {
        Numerator = 0;
        Denominator = 0;

        return;
      }

      byte[] bits = BitConverter.GetBytes(value);

      int sign = 1 - ((bits[3] >> 7) & 1) * 2;

      bits[3] = (byte)((bits[3] | 0x80) ^ 0x80);

      int exp = ((bits[3] << 1) | (bits[2] >> 7)) - 127;

      BigInteger mantissa = bits[2] | 128;

      mantissa <<= 16;

      for (int i = 1; i >= 0; --i) {
        int item = bits[i];

        BigInteger term = (BigInteger)item << (i * 8);

        mantissa += term; //((BigInteger)item) << (i * 8);
      }

      mantissa *= sign;

      exp = 23 - exp;

      if (exp < 0) {
        Numerator = mantissa * BigInteger.Pow(2, -exp);
        Denominator = 1;
      }
      else {
        BigInteger factor = BigInteger.Pow(2, exp);

        var gcd = BigInteger.GreatestCommonDivisor(factor, mantissa < 0 ? -mantissa : mantissa);

        Numerator = mantissa / gcd;
        Denominator = factor / gcd;
      }
    }

    /// <summary>
    ///     From Double value
    /// </summary>
    /// <param name="value">Double value</param>
    public BigRational(double value) {
      if (0 == value) {
        Numerator = 0;
        Denominator = 1;

        return;
      }

      if (double.IsPositiveInfinity(value)) {
        Numerator = 1;
        Denominator = 0;

        return;
      }

      if (double.IsNegativeInfinity(value)) {
        Numerator = -1;
        Denominator = 0;

        return;
      }

      if (double.IsNaN(value)) {
        Numerator = 0;
        Denominator = 0;

        return;
      }

      byte[] bits = BitConverter.GetBytes(value);

      int sign = 1 - ((bits[7] >> 7) & 1) * 2;

      bits[7] = (byte)((bits[7] | 0x80) ^ 0x80);

      int exp = ((bits[7] << 4) | (bits[6] >> 4)) - 1023;

      BigInteger mantissa = (bits[6] & 0xF) + 16;

      mantissa <<= 48;

      for (int i = 5; i >= 0; --i) {
        int item = bits[i];

        mantissa += (BigInteger)item << (i * 8);
      }

      mantissa *= sign;

      exp = 52 - exp;

      if (exp < 0) {
        Numerator = mantissa * BigInteger.Pow(2, -exp);
        Denominator = 1;
      }
      else {
        BigInteger factor = BigInteger.Pow(2, exp);

        var gcd = BigInteger.GreatestCommonDivisor(factor, mantissa < 0 ? -mantissa : mantissa);

        Numerator = mantissa / gcd;
        Denominator = factor / gcd;
      }
    }

    /// <summary>
    ///     Deconstruction into tuple
    /// </summary>
    /// <param name="numerator">Numerator</param>
    /// <param name="denominator">Denominator</param>
    public void Deconstruct(out BigInteger numerator, out BigInteger denominator) {
      numerator = Numerator;
      denominator = Denominator;
    }

    /// <summary>
    ///     From Continued Fraction
    /// </summary>
    /// <param name="terms"></param>
    /// <returns></returns>
    public static BigRational FromContinuedFraction(IEnumerable<BigInteger> terms) {
      if (terms is null)
        throw new ArgumentNullException(nameof(terms));

      BigRational result = PositiveInfinity;

      foreach (BigInteger term in terms.Reverse())
        result = term + 1 / result;

      return result;
    }

    /// <summary>
    ///     From radix representation
    ///     E.g. -3.26A0FF4 (Hex)
    /// </summary>
    /// <param name="value">Value in int.fractional format</param>
    /// <param name="radix">Radix to use</param>
    public static BigRational FromRadix(IEnumerable<char> value, int radix) {
      if (value is null)
        throw new ArgumentNullException(nameof(value));

      if (radix < 2 || radix > 36)
        throw new ArgumentOutOfRangeException(nameof(radix));

      static int DigitFromChar(char c) {
        if ((c >= '0') & (c <= '9'))
          return c - '0';
        if ((c >= 'a') & (c <= 'z'))
          return c - 'a' + 10;
        if ((c >= 'A') & (c <= 'Z'))
          return c - 'A' + 10;

        return -1;
      }

      int sign = 1;

      bool first = true;

      BigInteger nom = 0;
      BigInteger den = 1;

      bool isFractional = false;

      foreach (var c in value) {
        if (char.IsWhiteSpace(c) || c == '_')
          continue;

        if (c == '-') {
          sign = -sign;

          if (!first)
            throw new FormatException("Invalid Rational Value Format");

          continue;
        }

        first = false;

        if (c == ',' || c == '.') {
          if (isFractional)
            throw new FormatException("Invalid Rational Value Format");

          isFractional = true;

          continue;
        }

        if ((c >= '0') & (c <= '9') || (c >= 'a') & (c <= 'z') || (c >= 'A') & (c <= 'Z')) {
          int d = DigitFromChar(c);

          if (d < 0 || d >= radix)
            throw new FormatException("Invalid Rational Value Format");

          nom = nom * radix + d;

          if (isFractional)
            den *= radix;
        }
        else {
          throw new FormatException("Invalid Rational Value Format");
        }
      }

      return new BigRational(nom * sign, den);
    }

    /// <summary>
    ///     Zero
    /// </summary>
    public static BigRational Zero => new(0, 1);

    /// <summary>
    ///     One
    /// </summary>
    public static BigRational One => new(1, 1);

    /// <summary>
    ///     Minus One
    /// </summary>
    public static BigRational MinusOne => new(-1, 1);

    /// <summary>
    ///     NaN
    /// </summary>
    public static BigRational NaN => new(0, 0);

    /// <summary>
    ///     Positive infinity
    /// </summary>
    public static BigRational PositiveInfinity => new(1, 0);

    /// <summary>
    ///     Negative infinity
    /// </summary>
    public static BigRational NegativeInfinity => new(-1, 0);

    #endregion Create

    #region Public

    /// <summary>
    ///     Farey sequence
    /// </summary>
    // see https://en.wikipedia.org/wiki/Farey_sequence" 
    public static IEnumerable<BigRational> Farey(BigInteger n) {
      if (n <= 0)
        throw new ArgumentOutOfRangeException(nameof(n), "Parameter must be a positive value");

      BigInteger a = 0;
      BigInteger b = 1;
      BigInteger c = 1;
      BigInteger d = n;

      yield return new BigRational(a, b);
      yield return new BigRational(c, d);

      if (n == 1)
        yield break;

      while (true) {
        BigInteger z = (n + b) / d;

        BigInteger p = z * c - a;
        BigInteger q = z * d - b;

        yield return new BigRational(p, q);

        if (p == 1 && q == 1)
          break;

        a = c;
        b = d;
        c = p;
        d = q;
      }
    }

    /// <summary>
    ///     Compare
    /// </summary>
    public static int Compare(BigRational left, BigRational right) {
      var value = left.Numerator * right.Denominator - left.Denominator * right.Numerator;

      return value < 0
          ? -1
          : value > 0
              ? +1
              : 0;
    }

    /// <summary>
    ///     Min
    /// </summary>
    public static BigRational Min(BigRational left, BigRational right) {
      return left <= right ? left : right;
    }

    /// <summary>
    ///     Max
    /// </summary>
    public static BigRational Max(BigRational left, BigRational right) {
      return left >= right ? left : right;
    }

    /// <summary>
    ///     Numerator
    /// </summary>
    public BigInteger Numerator { get; }

    /// <summary>
    ///     Denominator
    /// </summary>
    public BigInteger Denominator { get; }

    /// <summary>
    ///     Parity
    /// </summary>
    public Parity Parity {
      get {
        if (Denominator.IsEven)
          return Parity.None;

        return Numerator.IsEven ? Parity.Even : Parity.Odd;
      }
    }

    /// <summary>
    ///     Is NaN
    /// </summary>
    public bool IsNaN => Numerator == 0 && Denominator == 0;

    /// <summary>
    ///     Is Infinity (either positive or negative)
    /// </summary>
    public bool IsInfinity => Numerator != 0 && Denominator == 0;

    /// <summary>
    ///     Is Positive Infinity
    /// </summary>
    public bool IsPositiveInfinity => Numerator > 0 && Denominator == 0;

    /// <summary>
    ///     Is Negative Infinity
    /// </summary>
    public bool IsNegativeInfinity => Numerator < 0 && Denominator == 0;

    /// <summary>
    ///     Is Finite (not NaN and not Infinity)
    /// </summary>
    public bool IsFinite => Denominator != 0;

    /// <summary>
    ///     Is One (1)
    /// </summary>
    public bool IsOne => Numerator.IsOne && Denominator.IsOne;

    /// <summary>
    ///     Is Zero (0)
    /// </summary>
    public bool IsZero => Numerator.IsZero && Denominator.IsOne;

    /// <summary>
    ///     Is Power Of Two
    /// </summary>
    public bool IsPowerOfTwo => (Numerator.IsOne && Denominator.IsPowerOfTwo) ||
                                (Numerator.IsPowerOfTwo && Denominator.IsOne);

    /// <summary>
    ///     Is Integer (no fractional part)
    /// </summary>
    public bool IsInteger => Denominator == 1;

    /// <summary>
    ///     Is Proper Fraction
    /// </summary>
    public bool IsProperFraction => BigInteger.Abs(Numerator) < BigInteger.Abs(Denominator);

    /// <summary>
    ///     Absolute Value
    /// </summary>
    public BigRational Abs() {
      return Numerator < 0
          ? new BigRational(-Numerator, Denominator)
          : this;
    }

    /// <summary>
    ///     Sign (-1, +1, 0)
    /// </summary>
    public int Sign() {
      return Numerator > 0 ? 1
          : Numerator < 0 ? -1
          : 0;
    }

    /// <summary>
    ///     Clamp
    /// </summary>
    /// <param name="min">Minimum Border</param>
    /// <param name="max">Maximum Border</param>
    public BigRational Clamp(BigRational min, BigRational max) {
      if (min > max)
        throw new ArgumentException($"Min value {min} can't be greater than max {max}.");

      if (IsNaN)
        return this;

      return this < min ? min
          : this > max ? max
          : this;
    }

    /// <summary>
    ///     Power
    /// </summary>
    public BigRational Pow(int exponent) {
      if (Denominator == 0)
        return this;
      if (0 == exponent)
        return One;
      if (0 == Numerator)
        return Zero;

      if (exponent > 0)
        return new BigRational(BigInteger.Pow(Numerator, exponent), BigInteger.Pow(Denominator, exponent));
      if (exponent == int.MinValue)
        throw new ArgumentOutOfRangeException(nameof(exponent));
      return new BigRational(BigInteger.Pow(Denominator, -exponent), BigInteger.Pow(Numerator, -exponent));
    }

    /// <summary>
    ///     Log10
    /// </summary>
    public double Log10() {
      if (Numerator < 0)
        return double.NaN;
      if (Denominator == 0)
        return Numerator > 0
            ? double.PositiveInfinity
            : double.NaN;

      if (Numerator == 0)
        return double.NegativeInfinity;

      return BigInteger.Log10(Numerator) - BigInteger.Log10(Denominator);
    }

    /// <summary>
    ///     Log (natural)
    /// </summary>
    public double Log() {
      if (Numerator < 0)
        return double.NaN;
      if (Denominator == 0)
        return Numerator > 0
            ? double.PositiveInfinity
            : double.NaN;

      if (Numerator == 0)
        return double.NegativeInfinity;

      return BigInteger.Log(Numerator) - BigInteger.Log(Denominator);
    }

    /// <summary>
    ///     Log
    /// </summary>
    public double Log(double baseValue) {
      if (Numerator < 0)
        return double.NaN;
      if (Denominator == 0) {
        if (Numerator > 0)
          return double.PositiveInfinity;

        return double.NaN;
      }

      if (Numerator == 0)
        return double.NegativeInfinity;

      return BigInteger.Log(Numerator, baseValue) - BigInteger.Log(Denominator, baseValue);
    }

    /// <summary>
    ///     Truncate (integer part)
    /// </summary>
    public BigRational Trunc() {
      return Denominator == 0
          ? this
          : new BigRational(Numerator / Denominator, 1);
    }

    /// <summary>
    ///     Fractional part
    /// </summary>
    public BigRational Frac() {
      return Denominator == 0
          ? this
          : new BigRational(Numerator % Denominator, Denominator);
    }

    /// <summary>
    ///     Floor
    /// </summary>
    public BigRational Floor() {
      if (Denominator == 0)
        return this;

      if (Numerator >= 0)
        return Numerator / Denominator;

      return Numerator / Denominator - (Numerator % Denominator == 0 ? 0 : 1);
    }

    /// <summary>
    ///     Ceiling
    /// </summary>
    public BigRational Ceiling() {
      if (Denominator == 0)
        return this;

      if (Numerator <= 0)
        return Numerator / Denominator;

      return Numerator / Denominator + (Numerator % Denominator == 0 ? 0 : 1);
    }

    /// <summary>
    ///     Round
    /// </summary>
    public BigRational Round(MidpointRounding mode) {
      if (Denominator == 0)
        return this;

      var integer = Numerator / Denominator;
      var fractional = 2 * ((Numerator < 0 ? -Numerator : Numerator) % Denominator);

      var sign = Numerator < 0 ? -1 : 1;

      if (fractional < Denominator)
        return integer;
      if (fractional > Denominator)
        return sign < 0 ? integer - 1 : integer + 1;

      switch (mode) {
        case MidpointRounding.AwayFromZero:
          return sign < 0 ? integer - 1 : integer + 1;
        case MidpointRounding.ToZero:
          return integer;
        case MidpointRounding.ToNegativeInfinity:
          return integer - 1;
        case MidpointRounding.ToPositiveInfinity:
          return integer + 1;
        case MidpointRounding.ToEven:
        default: {
            if (integer % 2 == 0)
              return integer;

            return sign < 0 ? integer - 1 : integer + 1;
          }
      }
    }

    /// <summary>
    ///     Integer Division
    /// </summary>
    public BigInteger Div(BigRational value) {
      return Numerator * value.Denominator / (value.Numerator * Denominator);
    }

    /// <summary>
    ///     Remainder
    /// </summary>
    public BigRational Rem(BigRational value) {
      return new(Numerator * value.Denominator % (value.Numerator * Denominator),
          Denominator * value.Denominator);
    }

    /// <summary>
    ///     Round
    /// </summary>
    public BigRational Round() {
      return Round(MidpointRounding.ToEven);
    }

    /// <summary>
    ///     Fractional digits
    ///     e.g. 1/7 returns 1, 4, 2, 8, 5, 7, 1, 4, 2 ...
    /// </summary>
    public IEnumerable<int> FractionalDigits() {
      if (Denominator <= 1)
        yield break;

      for (var value = (Numerator < 0 ? -Numerator : Numerator) % Denominator * 10;
           value != 0;
           value = value % Denominator * 10)
        yield return (int)(value / Denominator);
    }

    /// <summary>
    ///     To Continued Fraction
    /// </summary>
    public IEnumerable<BigInteger> ToContinuedFraction() {
      var num = Numerator;
      var den = Denominator;

      while (den != 0) {
        yield return BigInteger.DivRem(num, den, out num);

        (num, den) = (den, num);
      }
    }

    /// <summary>
    ///     To radix representation (eg. binary, hexadecimal etc.)
    /// </summary>
    /// <param name="radix">Radix in [2..36] range</param>
    /// <returns></returns>
    public IEnumerable<char> ToRadix(int radix) {
      if (radix < 2 || radix > 36)
        throw new ArgumentOutOfRangeException(nameof(radix));

      if (IsInfinity || IsNaN)
        foreach (char c in ToString())
          yield return c;

      static char DigitToChar(int v) {
        return (char)(v < 10 ? '0' + v : 'a' + v - 10);
      }

      var value = this;

      if (value < 0) {
        yield return '-';

        value = -value;
      }

      var intPart = value.Numerator / value.Denominator;

      if (intPart == 0) {
        yield return '0';
      }
      else {
        Stack<char> digits = new();

        for (; intPart > 0; intPart /= radix)
          digits.Push(DigitToChar((int)(intPart % radix)));

        while (digits.Count > 0)
          yield return digits.Pop();
      }

      var fractionalPart = value.Frac();

      if (fractionalPart == 0)
        yield break;

      yield return '.';

      while (fractionalPart > 0) {
        fractionalPart *= radix;

        yield return DigitToChar((int)(fractionalPart.Numerator / fractionalPart.Denominator));

        fractionalPart = fractionalPart.Frac();
      }
    }

    #endregion Public

    #region Operators

    #region Cast

    /// <summary>
    ///     From (numerator, denominator) tuple
    /// </summary>
    /// <param name="pair">Tuple</param>
    public static implicit operator BigRational((BigInteger numerator, BigInteger denominator) pair) {
      return new(pair);
    }

    /// <summary>
    ///     To (numerator, denominator) tuple
    /// </summary>
    public static implicit operator (BigInteger numerator, BigInteger denominator)(BigRational value) {
      return (value.Numerator, value.Denominator);
    }

    /// <summary>
    ///     From Character Numeric Value
    /// </summary>
    public static implicit operator BigRational(char value) {
      return new(value);
    }

    /// <summary>
    ///     From Integer
    /// </summary>
    public static implicit operator BigRational(BigInteger value) {
      return new(value);
    }

    /// <summary>
    ///     To Integer
    /// </summary>
    public static explicit operator BigInteger(BigRational value) {
      return value.IsNaN || value.IsInfinity
          ? throw new OverflowException()
          : value.Numerator / value.Denominator;
    }

    /// <summary>
    ///     From Integer
    /// </summary>
    public static implicit operator BigRational(int value) {
      return new(value, 1);
    }

    /// <summary>
    ///     To Integer
    /// </summary>
    public static explicit operator int(BigRational value) {
      if (value.IsNaN || value.IsInfinity)
        throw new OverflowException();

      BigInteger result = value.Numerator / value.Denominator;

      if (result < int.MinValue || result > int.MaxValue)
        throw new OverflowException();

      return (int)result;
    }

    /// <summary>
    ///     From bool
    /// </summary>
    public static explicit operator BigRational(bool value) {
      return value ? One : Zero;
    }

    /// <summary>
    ///     From Signed Byte
    /// </summary>
    [CLSCompliant(false)]
    public static implicit operator BigRational(sbyte value) {
      return new(value, 1);
    }

    /// <summary>
    ///     To Signed Byte
    /// </summary>
    [CLSCompliant(false)]
    public static explicit operator sbyte(BigRational value) {
      if (value.IsNaN || value.IsInfinity)
        throw new OverflowException();

      BigInteger result = value.Numerator / value.Denominator;

      if (result < sbyte.MinValue || result > sbyte.MaxValue)
        throw new OverflowException();

      return (sbyte)result;
    }

    /// <summary>
    ///     From Byte
    /// </summary>
    public static implicit operator BigRational(byte value) {
      return new(value, 1);
    }

    /// <summary>
    ///     To Byte
    /// </summary>
    public static explicit operator byte(BigRational value) {
      if (value.IsNaN || value.IsInfinity)
        throw new OverflowException();

      BigInteger result = value.Numerator / value.Denominator;

      if (result < byte.MinValue || result > byte.MaxValue)
        throw new OverflowException();

      return (byte)result;
    }

    /// <summary>
    ///     From Short
    /// </summary>
    public static implicit operator BigRational(short value) {
      return new(value, 1);
    }

    /// <summary>
    ///     To Short
    /// </summary>
    public static explicit operator short(BigRational value) {
      if (value.IsNaN || value.IsInfinity)
        throw new OverflowException();

      BigInteger result = value.Numerator / value.Denominator;

      if (result < short.MinValue || result > short.MaxValue)
        throw new OverflowException();

      return (short)result;
    }

    /// <summary>
    ///     From UInt16
    /// </summary>
    [CLSCompliant(false)]
    public static implicit operator BigRational(ushort value) {
      return new(value, 1);
    }

    /// <summary>
    ///     To UInt16
    /// </summary>
    [CLSCompliant(false)]
    public static explicit operator ushort(BigRational value) {
      if (value.IsNaN || value.IsInfinity)
        throw new OverflowException();

      BigInteger result = value.Numerator / value.Denominator;

      if (result < ushort.MinValue || result > ushort.MaxValue)
        throw new OverflowException();

      return (ushort)result;
    }

    /// <summary>
    ///     From UInt32
    /// </summary>
    [CLSCompliant(false)]
    public static implicit operator BigRational(uint value) {
      return new(value, 1);
    }

    /// <summary>
    ///     To UInt32
    /// </summary>
    [CLSCompliant(false)]
    public static explicit operator uint(BigRational value) {
      if (value.IsNaN || value.IsInfinity)
        throw new OverflowException();

      BigInteger result = value.Numerator / value.Denominator;

      if (result < uint.MinValue || result > uint.MaxValue)
        throw new OverflowException();

      return (uint)result;
    }

    /// <summary>
    ///     From UInt64
    /// </summary>
    [CLSCompliant(false)]
    public static implicit operator BigRational(ulong value) {
      return new(value, 1);
    }

    /// <summary>
    ///     To UInt64
    /// </summary>
    [CLSCompliant(false)]
    public static explicit operator ulong(BigRational value) {
      if (value.IsNaN || value.IsInfinity)
        throw new OverflowException();

      BigInteger result = value.Numerator / value.Denominator;

      if (result < ulong.MinValue || result > ulong.MaxValue)
        throw new OverflowException();

      return (ulong)result;
    }

    /// <summary>
    ///     From Integer
    /// </summary>
    public static implicit operator BigRational(long value) {
      return new(value, 1);
    }

    /// <summary>
    ///     To Integer
    /// </summary>
    public static explicit operator long(BigRational value) {
      if (value.IsNaN || value.IsInfinity)
        throw new OverflowException();

      BigInteger result = value.Numerator / value.Denominator;

      if (result < long.MinValue || result > long.MaxValue)
        throw new OverflowException();

      return (long)result;
    }

    /// <summary>
    ///     From Decimal
    /// </summary>
    public static implicit operator BigRational(decimal value) {
      return new(value);
    }

    /// <summary>
    ///     To Decimal
    /// </summary>
    public static explicit operator decimal(BigRational value) {
      if (!value.IsFinite)
        throw new InvalidCastException("Not a finite ratio");
      if (value == 0)
        return 0;

      long exp = value.Numerator.GetBitLength() - value.Denominator.GetBitLength();

      if (exp < -10000 || exp > 10000)
        throw new OverflowException();

      BigRational mantissa = value.Abs();

      var data = mantissa
          .ToRadix(10)
          .Select(c => c - '0')
          .Take(30);

      BigInteger m = 0;
      bool flag = false;

      int e = 0;

      foreach (int d in data) {
        if (d < 0 || d > 9) {
          flag = true;

          continue;
        }

        m = m * 10 + d;

        if (flag)
          e -= 1;
      }

      while (m > 0 && m % 10 == 0) {
        m /= 10;
        e += 1;
      }

      if (value < 0)
        m = -m;

      try {
        return Decimals.ConstructDecimal(m, e);
      }
      catch (ArgumentOutOfRangeException ex) {
        throw new OverflowException("Overflow when converting to Decimal", ex);
      }
    }

    /// <summary>
    ///     From Float (Single)
    /// </summary>
    public static implicit operator BigRational(float value) {
      return new(value);
    }

    /// <summary>
    ///     To Float (Single)
    /// </summary>
    public static explicit operator float(BigRational value) {
      if (value.IsNaN)
        return float.NaN;
      if (value.IsNegativeInfinity)
        return float.NegativeInfinity;
      if (value.IsPositiveInfinity)
        return float.PositiveInfinity;
      if (value == 0)
        return 0;
      if (value.IsInteger)
        return (float)value.Numerator;

      long exp = value.Numerator.GetBitLength() - value.Denominator.GetBitLength();

      if (exp < -10000 || exp > 10000)
        throw new OverflowException();

      int e = (int)exp;

      BigRational mantissa = e >= 0
          ? value.Abs() / BigInteger.Pow(2, e)
          : value.Abs() * BigInteger.Pow(2, -e);

      long m = mantissa
          .ToRadix(2)
          .Where(c => c == '1' || c == '0')
          .Select(c => c - '0')
          .Concat(new int[24])
          .Take(25)
          .Aggregate(0L, (s, a) => s * 2 + a);

      if (value < 0)
        m = -m;

      e -= 24;

      try {
        return FloatingPoint.ConstructSingle(m, e);
      }
      catch (ArgumentOutOfRangeException ex) {
        throw new OverflowException("Overflow when converting to single", ex);
      }
    }

    /// <summary>
    ///     From Double
    /// </summary>
    public static implicit operator BigRational(double value) {
      return new(value);
    }

    /// <summary>
    ///     To Double
    /// </summary>
    public static explicit operator double(BigRational value) {
      if (value.IsNaN)
        return double.NaN;
      if (value.IsNegativeInfinity)
        return double.NegativeInfinity;
      if (value.IsPositiveInfinity)
        return double.PositiveInfinity;
      if (value == 0)
        return 0;
      if (value.IsInteger)
        return (double)value.Numerator;

      long exp = value.Numerator.GetBitLength() - value.Denominator.GetBitLength();

      if (exp < -10000 || exp > 10000)
        throw new OverflowException();

      int e = (int)exp;

      BigRational mantissa = e >= 0
          ? value.Abs() / BigInteger.Pow(2, e)
          : value.Abs() * BigInteger.Pow(2, -e);

      long m = mantissa
          .ToRadix(2)
          .Where(c => c == '1' || c == '0')
          .Select(c => c - '0')
          .Concat(new int[52])
          .Take(53)
          .Aggregate(0L, (s, a) => s * 2 + a);

      if (value < 0)
        m = -m;

      e -= 52;

      try {
        return FloatingPoint.ConstructDouble(m, e);
      }
      catch (ArgumentOutOfRangeException ex) {
        throw new OverflowException("Overflow when converting to double", ex);
      }
    }

    #endregion Cast

    #region Comparison

    /// <summary>
    ///     Equals
    /// </summary>
    public static bool operator ==(BigRational left, BigRational right) {
      return left.Equals(right);
    }

    /// <summary>
    ///     Not Equals
    /// </summary>
    public static bool operator !=(BigRational left, BigRational right) {
      return !left.Equals(right);
    }

    /// <summary>
    ///     More
    /// </summary>
    public static bool operator >(BigRational left, BigRational right) {
      return Compare(left, right) > 0;
    }

    /// <summary>
    ///     Less
    /// </summary>
    public static bool operator <(BigRational left, BigRational right) {
      return Compare(left, right) < 0;
    }

    /// <summary>
    ///     More or Equal
    /// </summary>
    public static bool operator >=(BigRational left, BigRational right) {
      return Compare(left, right) >= 0;
    }

    /// <summary>
    ///     Less or Equal
    /// </summary>
    public static bool operator <=(BigRational left, BigRational right) {
      return Compare(left, right) <= 0;
    }

    #endregion Comparison

    #region Arithmetics

    /// <summary>
    ///     Unary +
    /// </summary>
    public static BigRational operator +(BigRational value) {
      return value;
    }

    /// <summary>
    ///     Unary -
    /// </summary>
    public static BigRational operator -(BigRational value) {
      return new(-value.Numerator, value.Denominator);
    }

    /// <summary>
    ///     Increment
    /// </summary>
    public static BigRational operator ++(BigRational value) {
      return value + 1;
    }

    /// <summary>
    ///     Decrement
    /// </summary>
    public static BigRational operator --(BigRational value) {
      return value - 1;
    }

    /// <summary>
    ///     Binary +
    /// </summary>
    public static BigRational operator +(BigRational left, BigRational right) {
      return new(left.Numerator * right.Denominator + right.Numerator * left.Denominator,
          left.Denominator * right.Denominator);
    }

    /// <summary>
    ///     Binary -
    /// </summary>
    public static BigRational operator -(BigRational left, BigRational right) {
      return new(left.Numerator * right.Denominator - right.Numerator * left.Denominator,
          left.Denominator * right.Denominator);
    }

    /// <summary>
    ///     Binary *
    /// </summary>
    public static BigRational operator *(BigRational left, BigRational right) {
      return new(left.Numerator * right.Numerator, left.Denominator * right.Denominator);
    }

    /// <summary>
    ///     Binary /
    /// </summary>
    public static BigRational operator /(BigRational left, BigRational right) {
      return new(left.Numerator * right.Denominator, left.Denominator * right.Numerator);
    }

    /// <summary>
    ///     Remainder
    /// </summary>
    public static BigRational operator %(BigRational left, BigRational right) {
      return new(left.Numerator * right.Denominator % (right.Numerator * left.Denominator),
          left.Denominator * right.Denominator);
    }

    #endregion Arithmetics

    #endregion Operators

    #region IEquatable<BigRational>

    /// <summary>
    ///     Equals
    /// </summary>
    public bool Equals(BigRational other) {
      return Numerator == other.Numerator && Denominator == other.Denominator;
    }

    /// <summary>
    ///     Equals
    /// </summary>
    public override bool Equals(object? obj) {
      return obj is BigInteger other && Equals(other);
    }

    /// <summary>
    ///     Hash Code
    /// </summary>
    public override int GetHashCode() {
      return Numerator.GetHashCode() ^ Denominator.GetHashCode();
    }

    #endregion IEquatable<BigRational>

    #region IComparable<BigRational>

    /// <summary>
    ///     CompareTo
    /// </summary>
    public int CompareTo(BigRational other) {
      return Compare(this, other);
    }

    #endregion IComparable<BigRational>

    #region ISerializable

    /// <summary>
    ///     Serialization
    /// </summary>
    public void GetObjectData(SerializationInfo info, StreamingContext context) {
      if (info is null)
        throw new ArgumentNullException(nameof(info));

      info.AddValue("Numerator", Numerator.ToString());
      info.AddValue("Denominator", Numerator.ToString());
    }

    #endregion ISerializable

    #region IFormattable

    /// <summary>
    ///     To Natural representation (e.g. "1 / 6")
    /// </summary>
    public string ToStringNatural() {
      if (Denominator == 0)
        if (Numerator == 0)
          return "NaN";
        else if (Numerator > 0)
          return "+Inf";
        else
          return "-Inf";

      return Denominator == 1
          ? Numerator.ToString()
          : $"{Numerator} / {Denominator}";
    }

    /// <summary>
    ///     To Decimal representation (e.g. "0.1(6)")
    /// </summary>
    public string ToStringDecimal(IFormatProvider? formatProvider = default) {
      if (Denominator == 0)
        if (Numerator == 0)
          return "NaN";
        else if (Numerator > 0)
          return "+Inf";
        else
          return "-Inf";
      if (Denominator == 1)
        return Numerator.ToString();

      formatProvider ??= CultureInfo.CurrentCulture;

      var format = formatProvider.GetFormat(typeof(NumberFormatInfo));

      var numerator = Numerator;

      StringBuilder sb = new();

      if (numerator < 0) {
        sb.Append('-');

        numerator = BigInteger.Abs(numerator);
      }

      sb.Append(numerator / Denominator);

      if (format is NumberFormatInfo nfi)
        sb.Append(nfi.NumberDecimalSeparator);
      else
        sb.Append('.');

      numerator %= Denominator;

      List<int> digits = new();
      Dictionary<BigInteger, int> remainders = new();

      for (int index = 0; ; ++index) {
        numerator *= 10;

        BigInteger rem = numerator % Denominator;

        int digit = (int)(numerator / Denominator);
        numerator -= digit * Denominator;

        if (rem == 0) {
          digits.Add(digit);

          sb.Append(string.Concat(digits));

          break;
        }

        if (remainders.TryGetValue(rem, out int idx)) {
          if (digit != digits[idx]) {
            idx += 1;
            digits.Add(digit);
          }

          for (int i = 0; i < idx; ++i)
            sb.Append(digits[i]);

          sb.Append('(');

          for (int i = idx; i < digits.Count; ++i)
            sb.Append(digits[i]);

          sb.Append(')');

          break;
        }

        digits.Add(digit);

        remainders.Add(rem, index);
      }

      return sb.ToString();
    }

    /// <summary>
    ///     To String
    /// </summary>
    public override string ToString() {
      if (Denominator == 0)
        if (Numerator == 0)
          return "NaN";
        else if (Numerator > 0)
          return "+Inf";
        else
          return "-Inf";
      if (Denominator == 1)
        return Numerator.ToString();

      return $"{Numerator} / {Denominator}";
    }

    /// <summary>
    ///     To String
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider) {
      if (string.IsNullOrEmpty(format) || format == "g" || format == "G")
        return ToStringNatural();
      if (format == "n" || format == "N")
        return ToStringNatural();
      if (format == "d" || format == "D")
        return ToStringDecimal(formatProvider);

      throw new FormatException("Invalid format");
    }

    /// <summary>
    ///     To String
    /// </summary>
    public string ToString(string? format) {
      return ToString(format, CultureInfo.CurrentCulture);
    }

    #endregion IFormattable

    #region ISpanFormattable

    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider) {
      var result = ToString(format.ToString());

      charsWritten = result.Length;

      if (destination.Length >= charsWritten) {
        for (int i = 0; i < charsWritten; ++i)
          destination[i] = result[i];

        return true;
      }

      return false;
    }

    #endregion ISpanFormattable

    #region IParsable<BigRational>

    /// <summary>
    ///     Try Parse (either natural, like "23 / 97" or decimal like "-1.45(913)e-6")
    /// </summary>
    public static bool TryParse(string? value, NumberStyles styles, IFormatProvider? formatProvider,
        out BigRational result) {
      if (TryParseNatural(value, styles, formatProvider, out result))
        return true;
      if (TryParseDecimal(value, styles, formatProvider, out result))
        return true;

      result = NaN;

      return false;
    }

    /// <summary>
    ///     Try Parse (either natural, like "23 / 97" or decimal like "-1.45(913)e-6")
    /// </summary>
    public static bool TryParse(string? value, IFormatProvider? formatProvider, out BigRational result) {
      NumberStyles style = NumberStyles.AllowLeadingSign |
                           NumberStyles.AllowLeadingWhite |
                           NumberStyles.AllowThousands |
                           NumberStyles.AllowTrailingWhite |
                           NumberStyles.AllowHexSpecifier;

      if (TryParseNatural(value, style, formatProvider, out result))
        return true;
      if (TryParseDecimal(value, style, formatProvider, out result))
        return true;

      result = NaN;

      return false;
    }

    /// <summary>
    ///     Try Parse (either natural, like "23 / 97" or decimal like "-1.45(913)e-6")
    /// </summary>
    public static bool TryParse(string? value, out BigRational result) {
      return TryParse(value, default, out result);
    }

    /// <summary>
    ///     Parse
    /// </summary>
    public static BigRational Parse(string? value, NumberStyles styles, IFormatProvider? formatProvider) {
      return TryParse(value, styles, formatProvider, out var result)
          ? result
          : throw new FormatException("Not a valid fraction");
    }

    /// <summary>
    ///     Parse
    /// </summary>
    public static BigRational Parse(string? value, IFormatProvider? formatProvider) {
      return TryParse(value, formatProvider, out var result)
          ? result
          : throw new FormatException("Not a valid fraction");
    }

    /// <summary>
    ///     Parse
    /// </summary>
    public static BigRational Parse(string? value) {
      return Parse(value, default);
    }

    #endregion IParsable<BigRational>

    /// <summary>
    ///     Try Parse (either natural, like "23 / 97" or decimal like "-1.45(913)e-6")
    /// </summary>
    public static bool TryParse(ReadOnlySpan<char> value, NumberStyles styles, IFormatProvider? formatProvider,
        out BigRational result) {
      if (TryParseNatural(value.ToString(), styles, formatProvider, out result))
        return true;
      if (TryParseDecimal(value.ToString(), styles, formatProvider, out result))
        return true;

      result = NaN;

      return false;
    }

    /// <summary>
    ///     Try Parse (either natural, like "23 / 97" or decimal like "-1.45(913)e-6")
    /// </summary>
    public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider? formatProvider, out BigRational result) {
      NumberStyles style = NumberStyles.AllowLeadingSign |
                           NumberStyles.AllowLeadingWhite |
                           NumberStyles.AllowThousands |
                           NumberStyles.AllowTrailingWhite |
                           NumberStyles.AllowHexSpecifier;

      if (TryParseNatural(value.ToString(), style, formatProvider, out result))
        return true;
      if (TryParseDecimal(value.ToString(), style, formatProvider, out result))
        return true;

      result = NaN;

      return false;
    }

    /// <summary>
    ///     Try Parse (either natural, like "23 / 97" or decimal like "-1.45(913)e-6")
    /// </summary>
    public static bool TryParse(ReadOnlySpan<char> value, out BigRational result) {
      return TryParse(value, default, out result);
    }

    /// <summary>
    ///     Parse
    /// </summary>
    public static BigRational Parse(ReadOnlySpan<char> value, NumberStyles styles, IFormatProvider? formatProvider) {
      return TryParse(value, styles, formatProvider, out var result)
          ? result
          : throw new FormatException("Not a valid fraction");
    }

    /// <summary>
    ///     Parse
    /// </summary>
    public static BigRational Parse(ReadOnlySpan<char> value, IFormatProvider? formatProvider) {
      return TryParse(value, formatProvider, out var result)
          ? result
          : throw new FormatException("Not a valid fraction");
    }

    /// <summary>
    ///     Parse
    /// </summary>
    public static BigRational Parse(ReadOnlySpan<char> value) {
      return Parse(value, default);
    }

    #region ISpanParsable<BigRational>

    #endregion ISpanParsable<BigRational>

    #region IConvertible

    TypeCode IConvertible.GetTypeCode() {
      return TypeCode.Object;
    }

    bool IConvertible.ToBoolean(IFormatProvider? provider) {
      return !IsZero;
    }

    byte IConvertible.ToByte(IFormatProvider? provider) {
      return (byte)this;
    }

    char IConvertible.ToChar(IFormatProvider? provider) {
      return (char)this;
    }

    DateTime IConvertible.ToDateTime(IFormatProvider? provider) {
      return Convert.ToDateTime((double)this);
    }

    decimal IConvertible.ToDecimal(IFormatProvider? provider) {
      return (decimal)this;
    }

    double IConvertible.ToDouble(IFormatProvider? provider) {
      return (double)this;
    }

    short IConvertible.ToInt16(IFormatProvider? provider) {
      return (short)this;
    }

    int IConvertible.ToInt32(IFormatProvider? provider) {
      return (int)this;
    }

    long IConvertible.ToInt64(IFormatProvider? provider) {
      return (long)this;
    }

    sbyte IConvertible.ToSByte(IFormatProvider? provider) {
      return (sbyte)this;
    }

    float IConvertible.ToSingle(IFormatProvider? provider) {
      return (float)this;
    }

    string IConvertible.ToString(IFormatProvider? provider) {
      return ToString();
    }

    object IConvertible.ToType(Type conversionType, IFormatProvider? provider) {
      return Convert.ChangeType((double)this, conversionType);
    }

    ushort IConvertible.ToUInt16(IFormatProvider? provider) {
      return (ushort)this;
    }

    uint IConvertible.ToUInt32(IFormatProvider? provider) {
      return (uint)this;
    }

    ulong IConvertible.ToUInt64(IFormatProvider? provider) {
      return (ulong)this;
    }

    #endregion IConvertible

    #region IComparable

    int IComparable.CompareTo(object? obj) {
      if (obj is null)
        return 1;

      if (TryCast(obj, out var other))
        return CompareTo(other);

      throw new ArgumentException($"Incomparable with {obj.GetType().Name}");
    }

    #endregion IComparable

    #region INumber<BigRational>

    #endregion INumber<BigRational>
  }

}
