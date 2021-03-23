using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace HigherArithmetics.Numerics {


  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Modulo operations
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static partial class Modulo {
    #region Public

    /// <summary>
    /// Mod
    /// </summary>
    public static BigInteger Remainder(this BigInteger value, BigInteger mod) {
      if (mod <= 0)
        throw new ArgumentOutOfRangeException(nameof(mod), $"mod == {mod} must be positive.");

      if (value >= 0 && value < mod)
        return value;

      value %= mod;

      if (value >= 0)
        return value;
      else
        return value + mod;
    }

    /// <summary>
    /// -value (mod mod)
    /// </summary>
    public static BigInteger ModNegate(this BigInteger value, BigInteger mod) =>
      Remainder(-value, mod);

    /// <summary>
    /// Greatest Common Divisor
    /// </summary>
    public static BigInteger Gcd(this BigInteger left, BigInteger right) =>
      BigInteger.GreatestCommonDivisor(left, right);

    /// <summary>
    /// Least Common Multiply
    /// </summary>
    public static BigInteger Lcm(this BigInteger left, BigInteger right) =>
      BigInteger.Abs(left / BigInteger.GreatestCommonDivisor(left, right) * right);

    /// <summary>
    /// Extended Euclid Algorithm
    /// </summary>
    public static (BigInteger LeftFactor,
                   BigInteger RightFactor,
                   BigInteger Gcd) Egcd(this BigInteger left, BigInteger right) {
      BigInteger leftFactor = 0;
      BigInteger rightFactor = 1;

      BigInteger u = 1;
      BigInteger v = 0;
      BigInteger gcd = 0;

      while (left != 0) {
        BigInteger q = right / left;
        BigInteger r = right % left;

        BigInteger m = leftFactor - u * q;
        BigInteger n = rightFactor - v * q;

        right = left;
        left = r;
        leftFactor = u;
        rightFactor = v;
        u = m;
        v = n;

        gcd = right;
      }

      return (LeftFactor: leftFactor,
              RightFactor: rightFactor,
              Gcd: gcd);
    }

    /// <summary>
    /// Mod Inversion
    /// </summary>
    public static BigInteger ModInversion(this BigInteger value, BigInteger modulo) {
      var egcd = Egcd(value, modulo);

      if (egcd.Gcd != 1)
        throw new ArgumentException("Invalid modulo", nameof(modulo));

      BigInteger result = egcd.LeftFactor;

      if (result < 0)
        result += modulo;

      return result % modulo;
    }

    /// <summary>
    /// Mod Division
    /// </summary>
    public static BigInteger ModDivision(this BigInteger left, BigInteger right, BigInteger modulo) =>
      (left * ModInversion(right, modulo)) % modulo;

    /// <summary>
    /// Chinese Reminder Theorem Solution
    ///    x = value[0] (mod mods[0])
    ///    x = value[1] (mod mods[1])
    ///    ...
    ///    x = value[n] (mod mods[n])
    /// </summary>
    /// <param name="values">Values</param>
    /// <param name="mods">Corresponding mods</param>
    /// <returns></returns>
    /// <example>BigInteger x = Modular.Crt(new BigInteger[] { 1, 2, 6}, new BigInteger[] { 2, 3, 7});</example>
    public static BigInteger Crt(IEnumerable<BigInteger> values, IEnumerable<BigInteger> mods) {
      if (values is null)
        throw new ArgumentNullException(nameof(values));
      else if (mods is null)
        throw new ArgumentNullException(nameof(mods));

      BigInteger[] r = values.ToArray();

      if (r.Length <= 0)
        throw new ArgumentOutOfRangeException(nameof(values), $"{nameof(values)} must not be empty.");

      BigInteger[] a = mods.ToArray();

      if (r.Length != a.Length)
        throw new ArgumentOutOfRangeException(nameof(mods), $"{nameof(mods)} must be of the same length as {nameof(values)}.");

      BigInteger M = a.Aggregate(BigInteger.One, (ss, item) => ss * item);

      BigInteger result = 0;

      for (int i = 0; i < a.Length; ++i) {
        BigInteger m = M / a[i];
        BigInteger m_1 = ModInversion(m, a[i]);

        result += r[i] * m * m_1;
      }

      return Remainder(result, M);
    }

    #endregion Public
  }

}
