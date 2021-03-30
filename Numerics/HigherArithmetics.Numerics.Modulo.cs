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
    /// Normalize value for given modulo
    /// Ensure, that value in [0 .. mod - 1] range
    /// </summary>
    /// <param name="value">Value</param>
    /// <param name="modulus">Modulus</param>
    /// <returns></returns>
    public static BigInteger Normalize(this BigInteger value, BigInteger modulus) {
      if (modulus <= 0)
        throw new ArgumentOutOfRangeException(nameof(modulus));

      return (value >= 0 && value < modulus)
        ? value
        : ((value % modulus) + value) % value;
    }

    /// <summary>
    /// Remiainder (Mod)
    /// </summary>
    public static BigInteger Remainder(this BigInteger value, BigInteger modulus) {
      if (modulus <= 0)
        throw new ArgumentOutOfRangeException(nameof(modulus), $"mod == {modulus} must be positive.");

      if (value >= 0 && value < modulus)
        return value;

      value %= modulus;

      if (value >= 0)
        return value;
      else
        return value + modulus;
    }

    /// <summary>
    /// -value (mod mod)
    /// </summary>
    public static BigInteger ModNegate(this BigInteger value, BigInteger modulus) =>
      Remainder(-value, modulus);

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
    public static BigInteger ModInversion(this BigInteger value, BigInteger modulus) {
      var egcd = Egcd(value, modulus);

      if (egcd.Gcd != 1)
        throw new ArgumentException("Invalid modulo", nameof(modulus));

      BigInteger result = egcd.LeftFactor;

      if (result < 0)
        result += modulus;

      return result % modulus;
    }

    /// <summary>
    /// Mod Division
    /// </summary>
    public static BigInteger ModDivision(this BigInteger left, BigInteger right, BigInteger modulus) =>
      (left * ModInversion(right, modulus)) % modulus;

    /// <summary>
    /// Chinese Reminder Theorem Solution
    ///    x = value[0] (mod mods[0])
    ///    x = value[1] (mod mods[1])
    ///    ...
    ///    x = value[n] (mod mods[n])
    /// </summary>
    /// <param name="values">Values</param>
    /// <param name="moduluses">Corresponding mods</param>
    /// <returns></returns>
    /// <example>BigInteger x = Modulo.Crt(new BigInteger[] { 1, 2, 6}, new BigInteger[] { 2, 3, 7});</example>
    public static BigInteger Crt(IEnumerable<BigInteger> values, IEnumerable<BigInteger> moduluses) {
      if (values is null)
        throw new ArgumentNullException(nameof(values));
      else if (moduluses is null)
        throw new ArgumentNullException(nameof(moduluses));

      BigInteger[] r = values.ToArray();

      if (r.Length <= 0)
        throw new ArgumentOutOfRangeException(nameof(values), $"{nameof(values)} must not be empty.");

      BigInteger[] a = moduluses.ToArray();

      if (r.Length != a.Length)
        throw new ArgumentOutOfRangeException(nameof(moduluses), $"{nameof(moduluses)} must be of the same length as {nameof(values)}.");

      BigInteger M = a.Aggregate(BigInteger.One, (ss, item) => ss * item);

      BigInteger result = 0;

      for (int i = 0; i < a.Length; ++i) {
        BigInteger m = M / a[i];
        BigInteger m_1 = ModInversion(m, a[i]);

        result += r[i] * m * m_1;
      }

      return Remainder(result, M);
    }

    /// <summary>
    /// Discrete logarithm
    /// This fuction can be very time consuming!
    /// </summary>
    /// <param name="value">Value</param>
    /// <param name="logBase">Logarithm base</param>
    /// <param name="modulus">Modulus</param>
    /// <returns></returns>
    public static BigInteger Log(this BigInteger value, BigInteger logBase, BigInteger modulus) {
      if (logBase <= 1)
        throw new ArgumentOutOfRangeException(nameof(logBase));
      if (modulus <= 1)
        throw new ArgumentOutOfRangeException(nameof(modulus));

      if (value == 1)
        return 0;

      value = Normalize(value, modulus);

      for (BigInteger x = 1; x < modulus; x += 1)
        if (BigInteger.ModPow(logBase, x, modulus) == value)
          return x;

      throw new ArgumentException("Logarithm doesn't exist", nameof(modulus));
    }

    #endregion Public
  }

}
