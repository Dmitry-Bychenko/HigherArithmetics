using HigherArithmetics.Numerics;
using System.Collections.Generic;

namespace HigherArithmetics.Linq.Generators {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Pythagorean Triplets
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class PythagoreanGenerator {
    #region Public

    /// <summary>
    /// Primitive triplets 
    /// </summary>
    public static IEnumerable<(long a, long b, long c)> EnumeratePrimitive() {
      for (long m = 2; ; m += 1)
        for (long n = 1; n < m; ++n) {
          if (m % 2 != 0 && n % 2 != 0)
            continue;

          if (m.Gcd(n) != 1)
            continue;

          yield return (m * m - n * n, 2 * m * n, m * m + n * n);
        }
    }

    #endregion Public
  }

}
