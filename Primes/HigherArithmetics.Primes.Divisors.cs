﻿using HigherArithmetics.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace HigherArithmetics.Primes {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Divisors
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class Divisors {
    #region Algorithm

    private static BigInteger[] CoreAllDivisors(IEnumerable<BigInteger> divisors) {
      var divs = divisors
        .GroupBy(x => x)
        .Select(group => (key: group.Key, count: group.Count()))
        .ToArray();

      int[] indexes = new int[divs.Length];

      List<BigInteger> result = new();

      do {
        BigInteger value = 1;

        for (int i = 0; i < indexes.Length; ++i) {
          for (int k = 0; k < indexes[i]; ++k)
            value *= divs[i].key;
        }

        result.Add(value);

        for (int i = 0; i < indexes.Length; ++i)
          if (indexes[i] < divs[i].count) {
            indexes[i] += 1;

            break;
          }
          else
            indexes[i] = 0;
      }
      while (!indexes.All(idx => idx == 0));

      result.Sort();

      return result.ToArray();
    }

    #endregion Algorithm

    #region Public

    /// <summary>
    /// Prime divisors
    /// </summary>
    public static IEnumerable<BigInteger> PrimeDivisorsExpanded(this BigInteger value) {
      if (value <= 1)
        yield break;

      long max = 3;
      BigInteger root = value.Sqrt();

      foreach (int d in KnownPrimes.Primes) {
        bool divides = false;

        while (value % d == 0) {
          yield return d;

          value /= d;
          divides = true;
        }

        if (divides)
          root = value.Sqrt();

        if (d > root) {
          if (value > 1)
            yield return value;

          yield break;
        }

        max = d;
      }

      if (PrimeNumbers.RabinMillesPrimeTest(value)) {
        yield return value;

        yield break;
      }

      for (long d = max + 2; d < long.MaxValue - 3; d += 2) {
        bool divides = false;

        while (value % d == 0) {
          yield return d;

          value /= d;
          divides = true;
        }

        if (divides)
          root = value.Sqrt();

        if (d > root) {
          if (value > 1)
            yield return value;

          yield break;
        }
      }

      throw new InvalidOperationException("Failed to find the next divisor");
    }

    /// <summary>
    /// Prime Divisors
    /// </summary>
    public static IEnumerable<(BigInteger prime, int power)> PrimeDivisors(this BigInteger value) {
      BigInteger prior = 0;
      int count = 0;

      foreach (BigInteger p in PrimeDivisorsExpanded(value)) {
        if (p == prior)
          count += 1;
        else {
          if (prior > 1)
            yield return (prior, count);

          prior = p;
          count = 1;
        }
      }

      if (count > 0)
        yield return (prior, count);
    }

    /// <summary>
    /// Prime Divisors
    /// </summary>
    public static IEnumerable<BigInteger> PrimeDivisorsDistinct(this BigInteger value) {
      BigInteger prior = 0;

      foreach (BigInteger p in PrimeDivisorsExpanded(value)) {
        if (p != prior) {
          yield return p;

          prior = p;
        }
      }
    }

    /// <summary>
    /// All Divisors
    /// </summary>
    public static BigInteger[] AllDivisors(this BigInteger value) {
      if (value < 1)
        return Array.Empty<BigInteger>();
      else if (value == 1)
        return new BigInteger[] { 1 };

      return CoreAllDivisors(PrimeDivisorsExpanded(value));
    }

    /// <summary>
    /// Sum Of Proper Divisors
    /// </summary>
    public static BigInteger SumOfDivisors(BigInteger value) {
      if (value == 0 | value == 1)
        return value;
      if (value < 0)
        return -SumOfDivisors(-value);

      var divs = PrimeDivisorsExpanded(value)
        .GroupBy(x => x)
        .Select(group => (key: group.Key, count: group.Count()))
        .ToArray();

      int[] indexes = new int[divs.Length];

      BigInteger result = -value;

      do {
        BigInteger divisor = 1;

        for (int i = 0; i < indexes.Length; ++i) {
          for (int k = 0; k < indexes[i]; ++k)
            divisor *= divs[i].key;
        }

        result += divisor;

        for (int i = 0; i < indexes.Length; ++i)
          if (indexes[i] < divs[i].count) {
            indexes[i] += 1;

            break;
          }
          else
            indexes[i] = 0;
      }
      while (!indexes.All(idx => idx == 0));

      return result;
    }

    /// <summary>
    /// Number Of Proper Divisors
    /// </summary>
    public static int NumberOfDivisors(BigInteger value) {
      if (value == 0 | value == 1)
        return 0;
      if (value < 0)
        return NumberOfDivisors(-value);

      var divs = PrimeDivisorsExpanded(value)
        .GroupBy(x => x)
        .Select(group => (key: group.Key, count: group.Count()))
        .ToArray();

      int[] indexes = new int[divs.Length];

      int result = -1;

      do {
        BigInteger divisor = 1;

        for (int i = 0; i < indexes.Length; ++i) {
          for (int k = 0; k < indexes[i]; ++k)
            divisor *= divs[i].key;
        }

        result += 1;

        for (int i = 0; i < indexes.Length; ++i)
          if (indexes[i] < divs[i].count) {
            indexes[i] += 1;

            break;
          }
          else
            indexes[i] = 0;
      }
      while (!indexes.All(idx => idx == 0));

      return result;
    }

    #endregion Public
  }

}
