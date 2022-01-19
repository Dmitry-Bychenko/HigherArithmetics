using HigherArithmetics.Numerics;
using System.Collections.Generic;

namespace HigherArithmetics.Linq.Generators {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Fractions Generator
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class FractionsGenerator {
    #region Public

    /// <summary>
    /// Enumerate all fractions
    /// </summary>
    public static IEnumerable<BigRational> EnumerateAll() {
      yield return BigRational.Zero;

      for (long radius = 1; ; ++radius) {
        for (long i = -radius; i < radius; ++i)
          if (radius.Gcd(i) == 1)
            yield return new BigRational(radius, i);

        for (long i = radius; i > -radius; --i)
          if (radius.Gcd(i) == 1)
            yield return new BigRational(i, radius);

        for (long i = radius; i > -radius; --i)
          if (radius.Gcd(i) == 1)
            yield return new BigRational(-radius, i);

        for (long i = -radius; i < radius; ++i)
          if (radius.Gcd(i) == 1)
            yield return new BigRational(i, -radius);
      }
    }

    /// <summary>
    /// Enumerate positive proper fractions
    /// </summary>
    public static IEnumerable<BigRational> EnumerateProper() {
      for (long b = 2; ; ++b)
        for (long a = 1; a < b; ++a)
          if (a.Gcd(b) == 1)
            yield return new BigRational(a, b);
    }

    #endregion Public
  }

}
