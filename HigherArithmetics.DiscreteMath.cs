﻿using HigherArithmetics.Numerics;
using HigherArithmetics.Primes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace HigherArithmetics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Discrete Math
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class DiscreteMath {
    #region Private Data

    private static readonly ConcurrentDictionary<int, BigInteger> s_Factorials = new();

    #endregion Private Data

    #region Public

    #region General

    /// <summary>
    /// Factorial
    /// </summary>
    public static BigInteger Factorial(int value) {
      if (value < 0)
        throw new ArgumentOutOfRangeException(nameof(value));
      if (0 == value)
        return 1;
      if (value < 3)
        return value;

      if (s_Factorials.TryGetValue(value, out BigInteger known))
        return known;

      BigInteger result = 1;

      for (int i = 2; i <= value; ++i) {
        result *= i;

        if (i < 1000)
          s_Factorials.TryAdd(i, result);
      }

      return result;
    }

    /// <summary>
    /// Double Factorial
    /// </summary>
    public static BigInteger DoubleFactorial(int value) {
      if (value < 0)
        throw new ArgumentOutOfRangeException(nameof(value));
      if (value < 3)
        return value;

      if (value % 2 == 0)
        return Factorial(value / 2) * BigInteger.Pow(2, value / 2);

      if (value < 1000)
        return Factorial(value) / BigInteger.Pow(2, value / 2) / Factorial(value / 2);

      BigInteger result = 1;

      for (int d = 3; d <= value; d += 2)
        result *= 3;

      return result;
    }

    // !n = (n - 1)(!(n - 1) + !(n - 2)); !0 = 1; !1 = 0 
    /// <summary>
    /// Subfactorial !value
    /// </summary>
    public static BigInteger SubFactorial(int value) {
      if (value < 0)
        throw new ArgumentOutOfRangeException(nameof(value));

      if (value == 0)
        return 1;
      if (value == 1)
        return 0;

      BigInteger a = 1;
      BigInteger b = 0;

      for (int n = 2; n <= value; ++n) {
        BigInteger z = (n - 1) * (b + a);
        a = b;
        b = z;
      }

      return b;
    }

    /// <summary>
    /// Binomial coefficient A
    /// </summary>
    public static BigInteger A(int take, int from) {
      if (take < 0)
        throw new ArgumentOutOfRangeException(nameof(take));
      if (from < 0)
        throw new ArgumentOutOfRangeException(nameof(from));

      if (take > from)
        return 0;
      if (take == from)
        return 1;

      BigInteger result = 1;

      for (int d = from; d > from - take; --d)
        result *= d;

      return result;
    }

    /// <summary>
    /// Binomial coefficient C
    /// </summary>
    public static BigInteger C(int take, int from) {
      if (take < 0)
        throw new ArgumentOutOfRangeException(nameof(take));
      if (from < 0)
        throw new ArgumentOutOfRangeException(nameof(from));

      if (take > from)
        return 0;
      if (take == from)
        return 1;

      BigInteger result = 1;

      for (int d = from; d > from - take; --d)
        result *= d;

      return result / Factorial(take);
    }

    /// <summary>
    /// Distributions Count, i.e.
    /// </summary>
    /// <param name="take">Take take items</param>
    /// <param name="from">From from total items</param>
    /// <param name="withRepetitions">With or without repetitions</param>
    /// <param name="ordered">If order matters or not (i.e. if [a, b] =/!= [b, a])</param>
    public static BigInteger CombinationsCount(
      int take,
      int from,
      bool withRepetitions,
      bool ordered) {

      if (take < 0)
        throw new ArgumentOutOfRangeException(nameof(take));
      else if (from < 0)
        throw new ArgumentOutOfRangeException(nameof(from));

      if (withRepetitions) {
        if (ordered)
          return from == 0 && take == 0
            ? 1
            : BigInteger.Pow(from, take);

        // C(take, take + from - 1)
        return C(take, take + from - 1);
      }
      else {
        if (take > from)
          return 0;

        if (ordered)
          return A(take, from);
        else
          return C(take, from);
      }
    }

    /// <summary>
    ///  Distributions Count, i.e.
    /// </summary>
    /// <param name="source">items to count</param>
    /// <param name="take">Take take items</param>
    /// <param name="withRepetitions">With or without repetitions</param>
    /// <param name="orderMatters">If order matters or not (i.e. if [a, b] =/!= [b, a])</param>
    /// <param name="comparer">Comparer if required</param>
    public static BigInteger CombinationsCount<T>(
      IEnumerable<T> source,
      int take,
      bool withRepetitions,
      bool orderMatters,
      IEqualityComparer<T> comparer) {

      if (source is null)
        throw new ArgumentNullException(nameof(source));
      if (take < 0)
        throw new ArgumentOutOfRangeException(nameof(take));

      comparer ??= EqualityComparer<T>.Default;

      if (comparer is null)
        throw new ArgumentNullException(nameof(comparer),
          $"{nameof(comparer)} is not provided and there are no default equality comparer for {typeof(T).Name} type");

      if (withRepetitions) {
        int from = source.Distinct(comparer).Count();

        if (orderMatters)
          return from == 0 && take == 0
            ? 1
            : BigInteger.Pow(from, take);

        return C(take, take + from - 1);
      }
      else {
        var data = source
          .GroupBy(item => item, comparer);

        int from = 0;
        BigInteger denominator = 1;

        foreach (var group in data) {
          from += group.Count();

          denominator *= Factorial(group.Count());
        }

        if (take > from)
          return 0;

        if (orderMatters)
          return A(take, from) / denominator;
        else
          return C(take, from) / denominator;
      }
    }

    /// <summary>
    ///  Distributions Count, i.e.
    /// </summary>
    /// <param name="source">items to count</param>
    /// <param name="take">Take take items</param>
    /// <param name="withRepetitions">With or without repetitions</param>
    /// <param name="orderMatters">If order matters or not (i.e. if [a, b] =/!= [b, a])</param>
    /// <param name="comparer">Comparer if required</param>
    public static BigInteger CombinationsCount<T>(
      IEnumerable<T> source,
      int take,
      bool withRepetitions,
      bool orderMatters) where T : IComparable<T> =>
      CombinationsCount(source, take, withRepetitions, orderMatters, null);

    /// <summary>
    /// Parity
    /// </summary>
    /// <see cref="https://mathworld.wolfram.com/Parity.html"/>
    public static int Parity(BigInteger value) {
      int result = 0;

      for (BigInteger v = value < 0 ? -value : value; v > 0; v /= 2)
        result ^= (int)(v % 2);

      return result;
    }

    #endregion General

    #region Number Theory

    /// <summary>
    /// Euler–Mascheroni constant 
    /// </summary>
    public const double Gamma = 0.57721566490153286060651209008240243104215933593992;

    /// <summary>
    /// Moebius function
    /// https://en.wikipedia.org/wiki/Moebius_function
    /// </summary>
    public static int Moebius(this BigInteger value) {
      if (value <= 0)
        throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(value)} must be a positive number.");

      if (value == 1)
        return 1;

      int count = 0;
      BigInteger prior = 0;

      foreach (BigInteger p in Divisors.PrimeDivisorsExpanded(value)) {
        if (p == prior)
          return 0;

        prior = p;
        count += 1;
      }

      return count % 2 == 0 ? 1 : -1;
    }

    /// <summary>
    /// Euler Totient (fi) function
    /// </summary>
    public static BigInteger Totient(this BigInteger value) {
      BigInteger result = value;

      foreach (BigInteger div in Divisors.PrimeDivisorsDistinct(value))
        result = (result / div) * (div - 1);

      return result;
    }

    /// <summary>
    /// Divisor function
    /// </summary>
    /// <see cref="https://en.wikipedia.org/wiki/Divisor_function"/>
    public static BigInteger Sigma(this BigInteger value, int index) {
      if (index < 0)
        throw new ArgumentOutOfRangeException(nameof(index));

      if (value <= 0)
        return 0;
      if (value == 1)
        return 1;

      BigInteger result = 0;

      foreach (BigInteger d in Divisors.AllDivisors(value))
        result += BigInteger.Pow(d, index);

      return result;
    }

    /// <summary>
    /// Minimum Primitive root
    /// </summary>
    /// <returns>Minimum primitive root; 0 if primitive root doesn't exist</returns>
    public static BigInteger PrimitiveRoot(this BigInteger value) {
      if (value <= 0)
        return 0;
      if (value == 1)
        return 1;
      if (value <= 4)
        return value - 1;

      if (value % 4 == 0)
        return 0;

      BigInteger number = value % 2 == 0
        ? value / 2
        : value;

      List<BigInteger> divisors = number.PrimeDivisorsDistinct().Take(2).ToList();

      if (divisors.Count == 2)
        return 0;

      BigInteger fi = value;

      if (number != value)
        divisors.Add(2);

      foreach (BigInteger div in divisors)
        fi = fi / div * (div - 1);

      List<BigInteger> ps = fi.PrimeDivisorsDistinct().ToList();
      ps.Add(1);

      HashSet<BigInteger> hs = new();

      for (BigInteger g = 1; g < value; ++g) {
        if (g.Gcd(value) != 1)
          continue;

        bool isOK = true;

        hs.Clear();

        foreach (BigInteger p in ps) {
          var z = BigInteger.ModPow(g, fi / p, value);

          if (!hs.Add(z)) {
            isOK = false;

            break;
          }
        }

        if (isOK)
          return g;
      }

      return 0;
    }

    /// <summary>
    /// If root is primitive root of value
    /// </summary>
    public static bool IsPrimitiveRoot(this BigInteger value, BigInteger root) {
      if (value <= 0)
        return false;
      if (value == 1)
        return root == 1;
      if (value <= 4)
        return root == value - 1;

      if (value % 4 == 0)
        return false;

      if (root <= 1 || root > value)
        return false;

      if (root.Gcd(value) != 1)
        return false;

      BigInteger number = value % 2 == 0
        ? value / 2
        : value;

      List<BigInteger> divisors = number.PrimeDivisorsDistinct().Take(2).ToList();

      if (divisors.Count == 2)
        return false;

      BigInteger fi = value;

      if (number != value)
        divisors.Add(2);

      foreach (BigInteger div in divisors)
        fi = fi / div * (div - 1);

      List<BigInteger> ps = fi.PrimeDivisorsDistinct().ToList();
      ps.Add(1);

      HashSet<BigInteger> hs = new();

      hs.Clear();

      foreach (BigInteger p in ps) {
        var z = BigInteger.ModPow(root, fi / p, value);

        if (!hs.Add(z))
          return false;
      }

      return true;
    }

    /// <summary>
    /// Kroneker Symbol (a, b)
    /// </summary>
    /// <see cref="https://en.wikipedia.org/wiki/Kronecker_symbol"/>
    public static int Kronecker(BigInteger a, BigInteger b) {
      int result = 1;

      if (b == -1)
        return a < 0 ? -1 : 1;
      if (b == 0)
        return a == 1 || a == -1 ? 1 : 0;
      if (b == 1)
        return 1;

      int sign = b < 0 ? -1 : 1;

      b = BigInteger.Abs(b);

      foreach (var (prime, power) in b.PrimeDivisors()) {
        BigInteger rem = (a % prime);

        if (rem == 0)
          return 0;

        if (power % 2 == 0)
          continue;

        if (prime == 2) {
          if (((a * a - 1) / 8) % 2 != 0)
            result = -result;
        }
        else {
          BigInteger mod = BigInteger.ModPow(a, (prime - 1) / 2, prime);

          if (mod < 0)
            mod = (mod + prime) % prime;

          if (mod != 1)
            result = -result;
        }
      }

      return result * sign;
    }

    #endregion Number Theory

    #endregion Public
  }

}
