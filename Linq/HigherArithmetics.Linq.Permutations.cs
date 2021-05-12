using System;
using System.Collections.Generic;
using System.Linq;


namespace HigherArithmetics.Linq {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Permutations 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static partial class EnumerableExtensions {
    #region Public

    /// <summary>
    /// Enumerate permutations starting from items
    /// </summary>
    /// <param name="items">initial permutation to use</param>
    /// <param name="comparer">comparer to compare items</param>
    public static IEnumerable<T[]> Permutations<T>(this IEnumerable<T> items, IComparer<T> comparer) {
      if (items is null)
        throw new ArgumentNullException(nameof(items));

      if ((comparer ??= Comparer<T>.Default) is null)
        throw new ArgumentNullException(nameof(comparer), $"Type {typeof(T).Name} has no default comparer.");

      T[] data = items.ToArray();
      T[] initial = data.ToArray();

      if (data.Length <= 0)
        yield break;
      if (data.Length == 1) {
        yield return data;

        yield break;
      }

      do {
        yield return data.ToArray();

        int pivotIndex = -1;

        for (int i = data.Length - 2; i >= 0; --i) {
          if (comparer.Compare(data[i], data[i + 1]) < 0) {
            pivotIndex = i;

            break;
          }
        }

        if (pivotIndex < 0)
          Array.Sort(data, comparer);
        else {
          int minIndex = -1;
          T pivot = data[pivotIndex];
          T min = default;

          for (int i = data.Length - 1; i > pivotIndex; --i) {
            T item = data[i];

            if (comparer.Compare(item, pivot) > 0) {
              if (minIndex < 0 || comparer.Compare(item, min) < 0) {
                min = item;
                minIndex = i;
              }
            }
          }

          T h = data[pivotIndex];
          data[pivotIndex] = data[minIndex];
          data[minIndex] = h;

          for (int i = 0; i < (data.Length - pivotIndex) / 2; ++i) {
            h = data[pivotIndex + 1 + i];
            data[pivotIndex + 1 + i] = data[data.Length - 1 - i];
            data[data.Length - 1 - i] = h;
          }
        }
      }
      while (Enumerable.Range(0, initial.Length).Any(i => comparer.Compare(initial[i], data[i]) != 0));
    }

    /// <summary>
    /// Enumerate permutations starting from items
    /// </summary>
    /// <param name="items">initial permutation to use</param>
    /// <param name="comparer">comparer to compare items</param>
    public static IEnumerable<T[]> Permutations<T>(this IEnumerable<T> items, IEqualityComparer<T> comparer) {
      if (items is null)
        throw new ArgumentNullException(nameof(items));

      comparer ??= EqualityComparer<T>.Default;

      T[] data = items.ToArray();
      T[] initial = data.ToArray();

      Dictionary<T, int> dict = new(comparer);

      foreach (T item in data)
        if (!dict.ContainsKey(item))
          dict.Add(item, dict.Count);

      if (data.Length <= 0)
        yield break;
      if (data.Length == 1) {
        yield return data;

        yield break;
      }

      do {
        yield return data.ToArray();

        int pivotIndex = -1;

        for (int i = data.Length - 2; i >= 0; --i) {
          if (dict[data[i]] < dict[data[i + 1]]) {
            pivotIndex = i;

            break;
          }
        }

        if (pivotIndex < 0)
          Array.Sort(data, (a, b) => dict[a].CompareTo(dict[b]));
        else {
          int minIndex = -1;
          T pivot = data[pivotIndex];
          T min = default;

          for (int i = data.Length - 1; i > pivotIndex; --i) {
            T item = data[i];

            if (dict[item] > dict[pivot]) {
              if (minIndex < 0 || dict[item] < dict[min]) {
                min = item;
                minIndex = i;
              }
            }
          }

          T h = data[pivotIndex];
          data[pivotIndex] = data[minIndex];
          data[minIndex] = h;

          for (int i = 0; i < (data.Length - pivotIndex) / 2; ++i) {
            h = data[pivotIndex + 1 + i];
            data[pivotIndex + 1 + i] = data[data.Length - 1 - i];
            data[data.Length - 1 - i] = h;
          }
        }
      }
      while (Enumerable.Range(0, initial.Length).Any(i => dict[initial[i]] != dict[data[i]]));
    }

    /// <summary>
    /// Enumerate permutations starting from items
    /// </summary>
    /// <param name="items">initial permutation to use</param>
    public static IEnumerable<T[]> Permutations<T>(this IEnumerable<T> items) =>
      Permutations(items, EqualityComparer<T>.Default);

    #endregion Public
  }

}
