using System;
using System.Collections.Generic;
using System.Linq;

namespace HigherArithmetics.Linq {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Enumerable Extensions
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static partial class EnumerableExtensions {
    #region Public

    /// <summary>
    /// Unordered combinations without repetions from multiset
    /// [0, 1, 2, 2] => [0, 1, 2], [0, 2, 2], [1, 2, 2]
    /// </summary>
    /// <param name="source">Multiset to get combinations from</param>
    /// <param name="size">Size of combinations</param>
    /// <param name="comparer">Comparer to use</param>
    /// <returns>Unordered combinations without repetions</returns>
    public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> source,
                                                        int size,
                                                        IEqualityComparer<T> comparer) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));
      if (size < 0)
        throw new ArgumentOutOfRangeException(nameof(size));

      comparer ??= EqualityComparer<T>.Default;

      T[] items = source
        .GroupBy(item => item, comparer)
        .SelectMany(group => group)
        .ToArray();

      if (size > items.Length)
        throw new ArgumentOutOfRangeException(nameof(size));

      if (items.Length == size) {
        yield return items;

        yield break;
      }

      T[] window = new T[size];

      for (int i = 0; i < size; ++i)
        window[i] = items[i];

      do {
        yield return window.ToArray();

        int index = -1;

        for (int i = 0; i < window.Length; ++i) {
          if (!comparer.Equals(window[window.Length - 1 - i], items[items.Length - 1 - i])) {
            index = window.Length - 1 - i;

            bool found = false;

            for (int j = 0; j < items.Length; ++j)
              if (found) {
                if (!comparer.Equals(items[j], window[index])) {
                  window[index] = items[j];

                  break;
                }
              }
              else
                found = comparer.Equals(items[j], window[index]);

            break;
          }
        }

        if (index < 0)
          break;

        int rest = 0;

        for (int i = 0; i <= index; ++i) {
          for (int j = rest; j < items.Length; ++j) {
            if (comparer.Equals(window[i], items[j])) {
              rest = j;

              break;
            }
          }
        }

        for (int i = index + 1; i < window.Length; ++i) {
          window[i] = items[++rest];
        }
      }
      while (true);
    }

    /// <summary>
    /// Unordered combinations without repetions from multiset
    /// [0, 1, 2, 2] => [0, 1, 2], [0, 2, 2], [1, 2, 2]
    /// </summary>
    /// <param name="source">Multiset to get combinations from</param>
    /// <param name="size">Size of combinations</param>
    /// <returns>Unordered combinations without repetions</returns>
    public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> source, int size) =>
      Combinations(source, size, EqualityComparer<T>.Default);

    #endregion Public 
  }

}
