using HigherArithmetics.Numerics;
using System;
using System.Collections.Generic;

namespace HigherArithmetics.Linq {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Linq for IEnumerable(BigRationals)
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static partial class BigRationalEnumerableExtensions {
    #region Public

    /// <summary>
    /// Sum 
    /// </summary>
    public static BigRational Sum(this IEnumerable<BigRational> source) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));

      BigRational result = 0;

      foreach (BigRational item in source)
        result += item;

      return result;
    }

    /// <summary>
    /// Sum 
    /// </summary>
    public static BigRational Sum<T>(this IEnumerable<T> source, Func<T, BigRational> selector) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));
      if (selector is null)
        throw new ArgumentNullException(nameof(selector));

      BigRational result = 0;

      foreach (T item in source)
        result += selector(item);

      return result;
    }

    /// <summary>
    /// Average 
    /// </summary>
    public static BigRational Average(this IEnumerable<BigRational> source) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));

      BigRational sum = 0;
      long count = 0;

      foreach (BigRational item in source) {
        sum += item;
        count += 1;
      }

      return count == 0 ? BigRational.NaN : sum / count;
    }

    /// <summary>
    /// Average 
    /// </summary>
    public static BigRational Average<T>(this IEnumerable<T> source, Func<T, BigRational> selector) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));
      if (selector is null)
        throw new ArgumentNullException(nameof(selector));

      BigRational sum = 0;
      long count = 0;

      foreach (T item in source) {
        sum += selector(item);
        count += 1;
      }

      return count == 0 ? BigRational.NaN : sum / count;
    }

    #endregion Public
  }

}
