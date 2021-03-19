using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using HigherArithmetics.Numerics;

namespace HigherArithmetics.Primes {
  
  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Prime Numbers
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class PrimeNumbers {
    #region Algorithm

    private static bool CoreRabinMiller(BigInteger value, int certainty) {
      BigInteger d = value - 1;
      int s = 0;

      while (d % 2 == 0) {
        d /= 2;
        s += 1;
      }

      byte[] bytes = new byte[value.ToByteArray().Length];
      BigInteger a;

      bool result = true;

      Parallel.For(0, certainty, (i, state) => {
        using (RNGCryptoServiceProvider provider = new()) {
          do {
            provider.GetBytes(bytes);

            a = new BigInteger(bytes);
          }
          while (a < 2 || a >= value - 2);
        }

        BigInteger x = BigInteger.ModPow(a, d, value);

        if (x != 1 && x != value - 1) {
          for (int r = 1; r < s; r++) {
            x = BigInteger.ModPow(x, 2, value);

            if (x == 1) {
              result = false;

              break;
            }
            else if (x == value - 1)
              break;
          }

          if (x != value - 1)
            result = false;

          if (!result)
            state.Stop();
        }

      });

      return result;
    }

    #endregion Algorithm

    #region Public

    /// <summary>
    /// Primes
    /// </summary>
    public static IEnumerable<BigInteger> Generate() {
      List<BigInteger> primes = new ();

      yield return 2;
      yield return 3;
      yield return 5;
      yield return 7;

      for (BigInteger p = 11; ; p += 2) {
        if (p % 3 == 0 || p % 5 == 0 || p % 7 == 0)
          continue;

        bool isPrime = true;

        BigInteger n = (BigInteger)(Math.Sqrt((double)p + 1) + 1);

        for (int i = 0; i < primes.Count; ++i) {
          BigInteger div = primes[i];

          if (div > n)
            break;
          else if (p % div == 0) {
            isPrime = false;

            break;
          }
        }

        if (!isPrime)
          continue;

        if (p < 50000)
          primes.Add(p);

        yield return p;
      }
    }

    /// <summary>
    /// Is Prime
    /// </summary>
    public static bool IsPrime(this BigInteger value) {
      if (value <= 1)
        return false;

      if ((value % 2) == 0)
        return (value == 2);
      if ((value % 3) == 0)
        return (value == 3);
      if ((value % 5) == 0)
        return (value == 5);
      if ((value % 7) == 0)
        return (value == 7);
      if ((value % 11) == 0)
        return (value == 11);

      long max = 11;

      if (value < long.MaxValue) {
        long lv = (long)value;
        max = 11;
        long n = (long) (Math.Sqrt(lv) + 1000);

        foreach (int v in KnownPrimes.Primes) {
          if (v > n || v >= lv)
            return true;
          if (lv % v == 0)
            return false;

          max = v;
        }

        for (long d = max + 2; d <= n; d += 2)
          if (lv % d == 0)
            return false;

        return true;
      }

      foreach (int v in KnownPrimes.Primes) { 
        if (value % v == 0)
          return false;

        max = v;
      }

      for (long d = max + 2; d < long.MaxValue - 3; d += 2)
        if (value % d == 0)
          return false;

      throw new InvalidOperationException("Probably Prime");
    }

    /// <summary>
    /// Rabin Miller test
    /// </summary>
    public static bool RabinMillesPrimeTest(BigInteger value, int certainty) {
      if (value <= 1)
        return false;

      if (certainty <= 0)
        throw new ArgumentOutOfRangeException(nameof(certainty), "certainty should be a positive number.");

      if ((value % 2) == 0)
        return (value == 2);
      if ((value % 3) == 0)
        return (value == 3);
      if ((value % 5) == 0)
        return (value == 5);
      if ((value % 7) == 0)
        return (value == 7);
      if ((value % 11) == 0)
        return (value == 11);

      return CoreRabinMiller(value, certainty);
    }

    /// <summary>
    /// Rabin Miller test
    /// </summary>
    public static bool RabinMillesPrimeTest(BigInteger value) =>
      RabinMillesPrimeTest(value, 10);

    /// <summary>
    /// Is probable Prime
    /// </summary>
    /// <param name="value">Value to test</param>
    /// <param name="certainty">Certainty (number of tests)</param>
    /// <returns>False if not prime, true if probably prime</returns>
    public static bool IsProbablePrime(this BigInteger value, int certainty) {
      if (value <= 1)
        return false;

      if (certainty <= 0)
        throw new ArgumentOutOfRangeException(nameof(certainty), "certainty should be a positive number.");

      if ((value % 2) == 0)
        return (value == 2);
      if ((value % 3) == 0)
        return (value == 3);
      if ((value % 5) == 0)
        return (value == 5);
      if ((value % 7) == 0)
        return (value == 7);
      if ((value % 11) == 0)
        return (value == 11);

      if (value < 1_000_000_000_000L) {
        int nn = (int)(Math.Sqrt((double)value) + 1);
        long lv = (long)value;

        foreach (int v in KnownPrimes.Primes) {
          if (v > nn)
            return true;
          if (lv % v == 0)
            return false;
        }
      }

      return CoreRabinMiller(value, certainty);
    }

    /// <summary>
    /// Rabin-Milles prime test (with certainty = 10)
    /// </summary>
    /// <param name="value">Value to test</param>
    /// <returns>False if not prime, true if probably prime</returns>
    public static bool IsProbablePrime(this BigInteger value) => IsProbablePrime(value, 10);
    
    #endregion Public 

  }

}
