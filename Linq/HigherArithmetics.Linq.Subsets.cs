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
    /// [Multi]subset from multiset
    /// [1, 1, 2] => [], [2], [1], [1, 2], [1, 1], [1, 1, 2]
    /// </summary>
    /// <param name="items">Initial multiset</param>
    /// <param name="comparer">Comparer to use</param>
    /// <returns>[Multi]subsets</returns>
    public static IEnumerable<T[]> Subsets<T>(IEnumerable<T> items, IEqualityComparer<T> comparer) {
      if (items is null)
        throw new ArgumentNullException(nameof(items));

      comparer ??= EqualityComparer<T>.Default;

      Dictionary<T, int> dict = new (comparer);

      foreach (T item in items)
        if (dict.TryGetValue(item, out int count))
          dict[item] = count + 1;
        else
          dict.Add(item, 1);

      if (dict.Count <= 0) {
        yield return Array.Empty<T>();

        yield break;
      }

      var alphabet = dict.ToArray();

      int[] current = new int[alphabet.Length];

      List<T> window = new ();

      do {
        window.Clear();

        for (int index = 0; index < current.Length; ++index) {
          int count = current[index];

          for (int i = 0; i < count; ++i)
            window.Add(alphabet[index].Key);
        }

        yield return window.ToArray();

        for (int i = current.Length - 1; i >= 0; --i) {
          if (current[i] == alphabet[i].Value)
            current[i] = 0;
          else {
            current[i] += 1;

            break;
          }
        }
      }
      while (current.Any(item => item != 0));
    }

    /// <summary>
    /// [Multi]subset from multiset
    /// </summary>
    /// <param name="items">Initial multiset</param>
    /// <returns>[Multi]subsets</returns>
    public static IEnumerable<T[]> Subsets<T>(IEnumerable<T> items) => Subsets(items, EqualityComparer<T>.Default);

    #endregion Public
  }

}
