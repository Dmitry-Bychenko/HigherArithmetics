using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

    private static List<int> m_Primes = new () {
      2, 3, 5, 7, 11, 13, 17, 19
    };

    #endregion Private Data

    #region Algorithm

    private static void CoreUpdate(int maxPrime) {
      List<int> current = null;

      Interlocked.Exchange(ref current, m_Primes);

      if (current[^1] >= maxPrime)
        return;

      List<int> next = current.ToList();

      for (int v = next[^1]; v < maxPrime; v += 2) {
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

      var was = Interlocked.CompareExchange(ref m_Primes, next, current);

      if (ReferenceEquals(was, current))
        return;

      lock (m_Primes) {
        if (m_Primes.Count < next.Count)
          Interlocked.Exchange(ref m_Primes, next);
      }
    }

    #endregion Algorithm

    #region Public

    /// <summary>
    /// Known (Cached) Prime Numbers
    /// </summary>
    public static IReadOnlyList<int> Primes {
      get {
        List<int> result = null;

        Interlocked.Exchange(ref result, m_Primes);

        return result;
      }
    }

    /// <summary>
    /// Update (get more primes)
    /// </summary>
    public static void Update(int maxPrimeNumber) => CoreUpdate(maxPrimeNumber);

    /// <summary>
    /// Maximum Known Prime
    /// </summary>
    public static int MaxKnownPrime {
      get {
        var current = m_Primes;

        return current[^1];
      }
      set {
        CoreUpdate(value);
      }
    }

    #endregion Public
  }

}
