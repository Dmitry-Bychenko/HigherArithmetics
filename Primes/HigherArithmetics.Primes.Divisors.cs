using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using HigherArithmetics.Numerics;

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

      List<BigInteger> result = new List<BigInteger>();

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
    public static IEnumerable<BigInteger> PrimeDivisorsFlat(this BigInteger value) {
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

      foreach (BigInteger p in PrimeDivisorsFlat(value)) {
        if (p == prior)
          count += 1;
        else {
          if (prior > 1)
            yield return (prior, count);

          prior = p;
          count = 1;
        }
      }

      if (count > 1)
        yield return (prior, count);
    }

    /// <summary>
    /// Prime Divisors
    /// </summary>
    public static IEnumerable<BigInteger> DistinctPrimeDivisors(this BigInteger value) {
      BigInteger prior = 0;

      foreach (BigInteger p in PrimeDivisorsFlat(value)) {
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

      return CoreAllDivisors(PrimeDivisorsFlat(value));
    }

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

      foreach (BigInteger p in PrimeDivisorsFlat(value)) {
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

      foreach (BigInteger div in DistinctPrimeDivisors(value))
        result = (result / div) * (div - 1);

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

      List<BigInteger> divisors = number.DistinctPrimeDivisors().Take(2).ToList();

      if (divisors.Count == 2)
        return 0;

      BigInteger fi = value;

      if (number != value)
        divisors.Add(2);

      foreach (BigInteger div in divisors)
        fi = fi / div * (div - 1);

      List<BigInteger> ps = fi.DistinctPrimeDivisors().ToList();
      ps.Add(1);

      HashSet<BigInteger> hs = new HashSet<BigInteger>();

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

      List<BigInteger> divisors = number.DistinctPrimeDivisors().Take(2).ToList();

      if (divisors.Count == 2)
        return false;

      BigInteger fi = value;

      if (number != value)
        divisors.Add(2);

      foreach (BigInteger div in divisors)
        fi = fi / div * (div - 1);

      List<BigInteger> ps = fi.DistinctPrimeDivisors().ToList();
      ps.Add(1);

      HashSet<BigInteger> hs = new HashSet<BigInteger>();

      hs.Clear();

      foreach (BigInteger p in ps) {
        var z = BigInteger.ModPow(root, fi / p, value);

        if (!hs.Add(z))
          return false;
      }

      return true;
    }

    #endregion Public
  }

}
