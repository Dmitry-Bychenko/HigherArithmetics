using System;
using System.Numerics;

namespace HigherArithmetics.Numerics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// BigInteger extensions
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static partial class BigIntegerExtensions {
    #region Algorithm

    private static Boolean CoreIsSqrt(BigInteger n, BigInteger root) {
      BigInteger lowerBound = root * root;
      BigInteger upperBound = (root + 1) * (root + 1);

      return (n >= lowerBound && n < upperBound);
    }

    #endregion Algorithm

    #region Public

    /// <summary>
    /// Sum of digits
    /// </summary>
    /// <param name="value">value</param>
    /// <returns>sum of digits</returns>
    public static int SumOfDigits(this BigInteger value) {
      int result = 0;

      for (; !value.IsZero; value /= 10)
        result += (int)(value % 10);

      return result < 0 ? -result : result;
    }

    /// <summary>
    /// Digit Root
    /// </summary>
    public static int DigitRoot(this BigInteger value) {
      int root = (int)(value >= 0 ? value % 9 : -value % 9);

      return root == 0 ? 9 : root;
    }

    /// <summary>
    /// Sqrt (lower bound)
    /// </summary>
    public static BigInteger Sqrt(this BigInteger value) {
      if (value == 0)
        return 0;

      if (value > 0) {
        int bitLength = Convert.ToInt32(Math.Ceiling(BigInteger.Log(value, 2)));

        BigInteger root = BigInteger.One << (bitLength / 2);

        while (!CoreIsSqrt(value, root)) {
          root += value / root;
          root /= 2;
        }

        return root;
      }

      throw new ArithmeticException("NaN");
    }

    /// <summary>
    /// Dijkstra Fusc function
    /// </summary>
    /// <see cref="https://en.wikipedia.org/wiki/Calkin%E2%80%93Wilf_tree"/>
    public static BigInteger Fusc(this BigInteger value) {
      BigInteger result = 0;

      for (BigInteger n = value, a = 1; n != 0; n /= 2)
        if (n % 2 == 0)
          a += result;
        else
          result += a;

      return result;
    }

    /// <summary>
    /// First digits
    /// </summary>
    public static BigInteger FirstDigits(this BigInteger value, int digits) {
      if (digits <= 0)
        throw new ArgumentOutOfRangeException(nameof(digits));

      if (value.IsZero)
        return value;

      int exp = (int)(value.GetBitLength() * 0.30102999566398 - digits);

      if (exp <= 0)
        return value < 0 ? -value : value;

      BigInteger result = value / BigInteger.Pow(10, exp);

      if (result < 0)
        result = -result;

      BigInteger power = BigInteger.Pow(10, digits);

      while (result >= power)
        result /= 10;

      return result;
    }

    /// <summary>
    /// Last digits
    /// </summary>
    public static BigInteger LastDigits(this BigInteger value, int digits) {
      if (digits <= 0)
        throw new ArgumentOutOfRangeException(nameof(digits));

      BigInteger result = value % BigInteger.Pow(10, digits);

      return result < 0 ? -result : result;
    }

    /// <summary>
    /// Number Of Digits
    /// </summary>
    public static int NumberOfDigits(this BigInteger value) {
      if (value.IsZero)
        return 1;

      int exp = (int)(value.GetBitLength() * 0.30102999566398);

      int result = exp;

      BigInteger power = BigInteger.Pow(10, exp);

      value /= power;

      while (value != 0) {
        value /= 10;
        result += 1;
      }

      return result;
    }

    #endregion Public
  }

}
