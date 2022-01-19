using System;
using System.Collections.Generic;
using System.Linq;

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

    /// <summary>
    /// Enumrate Gray codes; ensures mininum difference
    /// </summary>
    /// <param name="length">Length</param>
    /// <param name="radix">Radix</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">On negative length or radix </exception>
    public static IEnumerable<int[]> EnumerateMinDufference(int length, int radix) {
      if (length < 0)
        throw new ArgumentOutOfRangeException(nameof(length));
      if (radix < 0)
        throw new ArgumentOutOfRangeException(nameof(radix));

      if (0 == length || 0 == radix)
        yield break;

      int[] signs = Enumerable.Repeat(1, length).ToArray();
      int[] current = new int[length];

      for (bool keep = true; keep;) {
        yield return current.ToArray();

        keep = false;

        for (int i = current.Length - 1; i >= 0; --i) {
          int d = current[i] + signs[i];

          if (d >= 0 && d < radix) {
            current[i] = d;

            for (int j = i + 1; j < signs.Length; ++j)
              signs[j] = -signs[j];

            keep = true;

            break;
          }
        }
      }
    }

    #endregion Public
  }

}
