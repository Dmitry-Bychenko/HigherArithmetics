using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace HigherArithmetics.Primes {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Known Prime Numbers
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class KnownPrimes {
    #region Private Data

    private static int s_MaxPrime = 40;

    private static ImmutableList<int> s_Primes = ImmutableList.Create(
      2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37);

    #endregion Private Data

    #region Algorithm

    private static void CoreUpdate(int maxPrime) {
      if (s_MaxPrime >= maxPrime)
        return;

      var current = s_Primes;

      List<int> next = current.ToList();

      for (int v = next[^1] + 2; v < maxPrime; v += 2) {
        if (v % 3 == 0 || v % 5 == 0 || v % 7 == 0 || v % 11 == 0)
          continue;

        int n = (int)(Math.Sqrt(v) + 1);

        for (int i = 5; i < next.Count; ++i) {
          int p = next[i];

          if (p > n) {
            next.Add(v);

            break;
          }

          if (v % p == 0)
            break;
        }
      }

      if (s_Primes.Count >= next.Count || s_MaxPrime >= maxPrime)
        return;

      ImmutableList<int> list = next.ToImmutableList();

      lock (s_Primes) {
        if (s_Primes.Count >= next.Count || s_MaxPrime >= maxPrime)
          return;

        s_MaxPrime = maxPrime;
        s_Primes = list;
      }
    }

    #endregion Algorithm

    #region Public

    /// <summary>
    /// Known (Cached) Prime Numbers
    /// </summary>
    public static IReadOnlyList<int> Primes => s_Primes;

    /// <summary>
    /// Update (get more primes)
    /// </summary>
    public static void Update(int maxPrimeNumber) => CoreUpdate(maxPrimeNumber);

    /// <summary>
    /// Maximum Known Prime
    /// </summary>
    public static int MaxKnownPrime {
      get => s_MaxPrime;
      set => CoreUpdate(value);
    }

    /// <summary>
    /// Is known prime
    /// </summary>
    public static bool IsKnownPrime(int value) {
      if (value <= 1)
        return false;
      if (value > s_MaxPrime)
        return false;
      if (value % 2 == 0)
        return value == 2;

      var primes = s_Primes;

      if (value == primes[^1])
        return true;

      int left = 0;
      int right = primes.Count - 1;

      while (right - left > 1) {
        int middle = (left + right) / 2;

        if (primes[middle] == value)
          return true;

        if (primes[middle] > value)
          right = middle;
        else
          left = middle;
      }

      return primes[left] == value || primes[right] == value;
    }

    #endregion Public
  }

}
