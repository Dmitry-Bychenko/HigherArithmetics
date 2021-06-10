using HigherArithmetics.Numerics;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace HigherArithmetics.Linq {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Linq for IEnumerable(BigInteger)
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static partial class BigIntegerEnumerableExtensions {
    #region Public

    /// <summary>
    /// Sum 
    /// </summary>
    public static BigInteger Sum(this IEnumerable<BigInteger> source) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));

      BigInteger result = 0;

      foreach (BigInteger item in source)
        result += item;

      return result;
    }

    /// <summary>
    /// Sum 
    /// </summary>
    public static BigInteger Sum<T>(this IEnumerable<T> source, Func<T, BigInteger> selector) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));
      if (selector is null)
        throw new ArgumentNullException(nameof(selector));

      BigInteger result = 0;

      foreach (T item in source)
        result += selector(item);

      return result;
    }

    /// <summary>
    /// Average 
    /// </summary>
    public static BigRational Average(this IEnumerable<BigInteger> source) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));

      BigInteger sum = 0;
      long count = 0;

      foreach (BigInteger item in source) {
        sum += item;
        count += 1;
      }

      return new BigRational(sum, count);
    }

    /// <summary>
    /// Average 
    /// </summary>
    public static BigRational Average<T>(this IEnumerable<T> source, Func<T, BigInteger> selector) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));
      if (selector is null)
        throw new ArgumentNullException(nameof(selector));

      BigInteger sum = 0;
      long count = 0;

      foreach (T item in source) {
        sum += selector(item);
        count += 1;
      }

      return new BigRational(sum, count);
    }

    #endregion Public
  }

}
