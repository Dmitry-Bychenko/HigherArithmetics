using System;
using System.Collections.Generic;

namespace HigherArithmetics.Linq.Generators {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Gray Codes Generator
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class GrayCodesGenerator {
    #region Public

    /// <summary>
    /// Enumrate Gray codes
    /// </summary>
    /// <param name="length">Length</param>
    /// <param name="radix">Radix</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">On negative length or radix </exception>
    public static IEnumerable<int[]> Enumerate(int length, int radix) {
      if (length < 0)
        throw new ArgumentOutOfRangeException(nameof(length));
      if (radix < 0)
        throw new ArgumentOutOfRangeException(nameof(radix));

      if (0 == length || 0 == radix)
        yield break;

      static int digit(long n, int radix, int i) =>
        (int)(Math.Floor(n / Math.Pow(radix, i)) % radix);

      double count = Math.Pow(radix, length);

      long max = count > long.MaxValue ? long.MaxValue : (long)count;

      for (long i = 0; i < max; ++i) {
        int[] result = new int[length];
        int shift = 0;

        for (int j = length - 1; j >= 0; j--) {
          var x = (digit(i, radix, j) + shift) % radix;

          shift += radix - x;
          result[length - j - 1] = x;
        }

        yield return result;
      }
    }

    #endregion Public
  }

}
