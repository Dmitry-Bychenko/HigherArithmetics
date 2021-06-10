using HigherArithmetics.Numerics;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;

namespace HigherArithmetics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Special Numbers
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class Numbers {
    #region Private Data

    private static readonly ConcurrentDictionary<(int n, int k), BigInteger> s_KnownStirlingFirst = new();

    private static readonly ConcurrentDictionary<(int n, int k), BigInteger> s_KnownStirlingSecond = new();

    private static readonly ConcurrentDictionary<(int n, int k), BigInteger> s_KnownEulerFirst = new();

    private static readonly ConcurrentDictionary<(int n, int k), BigInteger> s_KnownEulerSecond = new();

    private static readonly ConcurrentDictionary<int, BigRational> s_KnownBernoulli = new();

    #endregion Private Data

    #region Algorithm

    private static BigInteger[] Matrix2Power(BigInteger[] matrix, int power) {
      BigInteger[] result = new BigInteger[] { 1, 0, 0, 1 };
      BigInteger[] factor = matrix.ToArray();

      for (int p = power; p != 0; p /= 2) {
        if (p % 2 != 0) {
          var b11 = result[0] * factor[0] + result[1] * factor[2];
          var b12 = result[0] * factor[1] + result[1] * factor[3];
          var b21 = result[2] * factor[0] + result[3] * factor[2];
          var b22 = result[2] * factor[1] + result[3] * factor[3];

          result[0] = b11;
          result[1] = b12;
          result[2] = b21;
          result[3] = b22;
        }

        var a11 = factor[0] * factor[0] + factor[1] * factor[2];
        var a12 = factor[0] * factor[1] + factor[1] * factor[3];
        var a21 = factor[2] * factor[0] + factor[3] * factor[2];
        var a22 = factor[2] * factor[1] + factor[3] * factor[3];

        factor[0] = a11;
        factor[1] = a12;
        factor[2] = a21;
        factor[3] = a22;
      }

      return result;
    }

    private static BigRational CoreBenoulli(int m) {
      if (m < 0)
        return 0;
      if (m % 2 != 0)
        return m == 1 ? new BigRational(1, 2) : 0;
      if (m == 0)
        return 1;

      if (s_KnownBernoulli.TryGetValue(m, out BigRational known))
        return known;

      BigRational result = 0;

      BigInteger factor = 1;

      for (int k = 0; k < m; ++k) {
        result += factor * CoreBenoulli(k) / (m - k + 1);

        factor = factor * (m - k) / (k + 1);
      }

      result = 1 - result;

      s_KnownBernoulli.TryAdd(m, result);

      return result;
    }

    #endregion Algorithm

    #region Public

    /// <summary>
    /// Fibonacci number; f(0) = 0, f(1) = 1
    /// </summary>
    public static BigInteger Fibonacci(int index) =>
      Matrix2Power(index > 0 ? new BigInteger[] { 0, 1, 1, 1 } : new BigInteger[] { -1, 1, 1, 0 }, index)[2];

    /// <summary>
    /// Catalan number
    /// </summary>
    public static BigInteger Catalan(int index) {
      if (index < 0)
        return 0;

      BigInteger result = 1;
      BigInteger d = 1;

      for (int k = 2; k <= index; ++k) {
        result *= (k + index);
        d *= k;
      }

      return result / d;
    }

    /// <summary>
    /// Stirling Number of the 1st Kind
    /// </summary>
    public static BigInteger StirlingFirst(int n, int k) {
      if (n < 0 || k < 0)
        return 0;

      if (n == k)
        return 1;

      if (k == 0 || n == 0)
        return 0;
      if (k > n)
        return 0;

      if (s_KnownStirlingFirst.TryGetValue((n, k), out BigInteger result))
        return result;

      result = StirlingFirst(n - 1, k - 1) + (n - 1) * StirlingFirst(n - 1, k);

      s_KnownStirlingFirst.TryAdd((n, k), result);

      return result;
    }

    /// <summary>
    /// Stirling Number of the 2nd Kind
    /// </summary>
    public static BigInteger StirlingSecond(int n, int k) {
      if (n < 0 || k < 0)
        return 0;

      if (n == k || k == 1)
        return 1;

      if (k == 0 || n == 0)
        return 0;
      if (k > n)
        return 0;

      if (s_KnownStirlingSecond.TryGetValue((n, k), out BigInteger result))
        return result;

      result = StirlingSecond(n - 1, k - 1) + k * StirlingSecond(n - 1, k);

      s_KnownStirlingFirst.TryAdd((n, k), result);

      return result;
    }

    /// <summary>
    /// Euler Number of the 1st Kind
    /// </summary>
    public static BigInteger EulerFirst(int n, int m) {
      if (n < 0 || m < 0)
        return 0;

      if (m == 0)
        return 1;

      if (m >= n)
        return 0;

      if (s_KnownEulerFirst.TryGetValue((n, m), out BigInteger result))
        return result;

      result = (n - m) * EulerFirst(n - 1, m - 1) + (m + 1) * EulerFirst(n - 1, m);

      s_KnownEulerFirst.TryAdd((n, m), result);

      return result;
    }

    /// <summary>
    /// Euler Number of the 2nd Kind
    /// </summary>
    public static BigInteger EulerSecond(int n, int m) {
      if (n < 0 || m < 0)
        return 0;

      if (m == 0)
        return 1;

      if (m >= n)
        return 0;

      if (s_KnownEulerSecond.TryGetValue((n, m), out BigInteger result))
        return result;

      result = (2 * n - m - 1) * EulerSecond(n - 1, m - 1) + (m + 1) * EulerSecond(n - 1, m);

      s_KnownEulerSecond.TryAdd((n, m), result);

      return result;
    }

    /// <summary>
    /// Bell Number
    /// </summary>
    public static BigInteger Bell(int n) {
      if (n < 0)
        return 0;

      BigInteger result = 0;

      for (int k = n; k >= 0; --k)
        result += StirlingSecond(n, k);

      return result;
    }

    /// <summary>
    /// Bernoulli number
    /// </summary>
    /// <param name="n">Argument</param>
    /// <param name="isPositive">If 1st number is positive 1/2 or negative -1/2</param>
    public static BigRational Bernoulli(int n, bool isPositive = true) {
      if (!isPositive && n == 1)
        return new BigRational(-1, 2);

      return CoreBenoulli(n);
    }

    #endregion Public
  }

}
