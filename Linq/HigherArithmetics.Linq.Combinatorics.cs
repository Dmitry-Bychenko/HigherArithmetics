using System;
using System.Collections.Generic;
using System.Linq;


namespace HigherArithmetics.Linq {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static partial class EnumerableExtensions {
    #region Algorithm

    // Combinations with replacement, order matters
    // {A, B, C}, 4 -> {A, A, A, A}, {A, A, A, B}, {A, A, A, C}, {A, A, B, A}, ... , {C, C, C, C}
    private static IEnumerable<T[]> OrderedWithReplacement<T>(IEnumerable<T> source,
                                                              int size,
                                                              IEqualityComparer<T> comparer) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));
      if (size < 0)
        throw new ArgumentOutOfRangeException(nameof(size));

      comparer ??= EqualityComparer<T>.Default;

      T[] alphabet = source.Distinct(comparer).ToArray();

      if (alphabet.Length <= 0)
        yield break;
      else if (size == 0)
        yield break;

      int[] indexes = new int[size];

      do {
        T[] window = new T[indexes.Length];

        for (int i = alphabet.Length - 1; i >= 0; --i)
          window[i] = alphabet[i];

        yield return window;

        for (int i = indexes.Length - 1; i >= 0; --i)
          if (indexes[i] >= alphabet.Length - 1)
            indexes[i] = 0;
          else {
            indexes[i] += 1;

            break;
          }
      }
      while (!indexes.All(index => index == 0));
    }

    // Combinations with replacement, order doesn't matters
    // {A, B, C}, 4 -> {A, A, A, A}, {A, A, A, B}, {A, A, A, C}, {A, A, B, B}, {A, A, B, C},... , {C, C, C, C}
    private static IEnumerable<T[]> UnOrderedWithReplacement<T>(IEnumerable<T> source,
                                                                int size,
                                                                IEqualityComparer<T> comparer) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));
      else if (size < 0)
        throw new ArgumentOutOfRangeException(nameof(size));

      comparer ??= EqualityComparer<T>.Default;

      T[] alphabet = source.Distinct(comparer).ToArray();

      if (alphabet.Length <= 0)
        yield break;
      else if (size == 0)
        yield break;

      int[] indexes = new int[size];

      do {
        T[] window = new T[indexes.Length];

        for (int i = alphabet.Length - 1; i >= 0; --i)
          window[i] = alphabet[i];

        yield return window;

        for (int i = indexes.Length - 1; i >= 0; --i)
          if (indexes[i] >= alphabet.Length - 1)
            indexes[i] = 0;
          else {
            indexes[i] += 1;

            for (int j = i + 1; j < indexes.Length; ++j)
              indexes[j] = indexes[i];

            break;
          }
      }
      while (!indexes.All(index => index == 0));
    }

    // Combinations without replacement, order matters
    // {A, B, C, C} => {A, B, C}, {A, C, B}, {A, C, C}, {B, A, C}, {B, C, A}, {B, C, C}...
    public static IEnumerable<T[]> OrderedWithoutReplacement<T>(IEnumerable<T> source,
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

      Dictionary<T, int> dict = new(comparer);

      foreach (T item in items)
        if (!dict.ContainsKey(item))
          dict.Add(item, dict.Count);

      if (items.Length == size) {
        yield return items;

        yield break;
      }

      T[] window = new T[size];

      for (int i = 0; i < size; ++i)
        window[i] = items[i];

      do {
        foreach (var outcome in LightPermutations(window, dict))
          yield return outcome;

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

        for (int i = 0; i <= index; ++i)
          for (int j = rest; j < items.Length; ++j)
            if (comparer.Equals(window[i], items[j])) {
              rest = j;

              break;
            }

        for (int i = index + 1; i < window.Length; ++i)
          window[i] = items[++rest];
      }
      while (true);
    }

    // [Multi]subset from multiset, order matters
    // {1, 1, 2} => {}, {2}, {1}, {1, 2}, {1, 1}, {2, 1}, {1, 1, 2}, {1, 2, 1}, {2, 1, 1}
    private static IEnumerable<T[]> OrderedSubsets<T>(IEnumerable<T> items, IEqualityComparer<T> comparer) {
      if (items is null)
        throw new ArgumentNullException(nameof(items));

      comparer ??= EqualityComparer<T>.Default;

      Dictionary<T, int> dict = new(comparer);

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

      Dictionary<T, int> permDict = new();

      for (int i = 0; i < alphabet.Length; ++i)
        permDict.Add(alphabet[i].Key, i);

      int[] current = new int[alphabet.Length];

      List<T> window = new();

      do {
        window.Clear();

        for (int index = 0; index < current.Length; ++index) {
          int count = current[index];

          for (int i = 0; i < count; ++i)
            window.Add(alphabet[index].Key);
        }

        foreach (T[] item in LightPermutations(window.ToArray(), permDict))
          yield return item;

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

    private static IEnumerable<T[]> OrderedSubsetsWithReplacement<T>(IEnumerable<T> items, IEqualityComparer<T> comparer) {
      if (items is null)
        throw new ArgumentNullException(nameof(items));

      T[] unique = items
        .Distinct(comparer)
        .ToArray();

      yield return Array.Empty<T>();

      if (unique.Length <= 0) 
        yield break;

      for (int size = 1; ; ++size)
        foreach (T[] result in OrderedWithReplacement(unique, size, comparer))
          yield return result;
    }

    private static IEnumerable<T[]> UnOrderedSubsetsWithReplacement<T>(IEnumerable<T> items, IEqualityComparer<T> comparer) {
      if (items is null)
        throw new ArgumentNullException(nameof(items));

      T[] unique = items
        .Distinct(comparer)
        .ToArray();

      yield return Array.Empty<T>();

      if (unique.Length <= 0)
        yield break;

      for (int size = 1; ; ++size)
        foreach (T[] result in UnOrderedWithReplacement(unique, size, comparer))
          yield return result;
    }

    // Permutations
    private static IEnumerable<T[]> LightPermutations<T>(T[] initial, IDictionary<T, int> dictionary) {
      if (initial is null)
        throw new ArgumentNullException(nameof(initial));

      T[] data = initial.ToArray();

      foreach (T item in data)
        if (!dictionary.ContainsKey(item))
          dictionary.Add(item, dictionary.Count);

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
          if (dictionary[data[i]] < dictionary[data[i + 1]]) {
            pivotIndex = i;

            break;
          }
        }

        if (pivotIndex < 0)
          Array.Sort(data, (a, b) => dictionary[a].CompareTo(dictionary[b]));
        else {
          int minIndex = -1;
          T pivot = data[pivotIndex];
          T min = default;

          for (int i = data.Length - 1; i > pivotIndex; --i) {
            T item = data[i];

            if (dictionary[item] > dictionary[pivot]) {
              if (minIndex < 0 || dictionary[item] < dictionary[min]) {
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
      while (Enumerable.Range(0, initial.Length).Any(i => dictionary[initial[i]] != dictionary[data[i]]));
    }

    #endregion Algorithm

    #region Public

    /// <summary>
    /// Combinations
    /// </summary>
    /// <param name="source">Source (multiset)</param>
    /// <param name="size">Combinations' size</param>
    /// <param name="withReplacement">With or Without replacement</param>
    /// <param name="orderMatters">Does order matter</param>
    /// <param name="comparer">Comparer to use</param>
    public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> source,
                                                        int size,
                                                        bool withReplacement,
                                                        bool orderMatters,
                                                        IEqualityComparer<T> comparer) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));
      if (size < 0)
        throw new ArgumentOutOfRangeException(nameof(size));

      if (withReplacement)
        if (orderMatters)
          return OrderedWithReplacement(source, size, comparer);
        else
          return UnOrderedWithReplacement(source, size, comparer);
      else if (orderMatters)
        return OrderedWithoutReplacement(source, size, comparer);
      else
        return Combinations(source, size, comparer);
    }

    /// <summary>
    /// Combinations
    /// </summary>
    /// <param name="source">Source (multiset)</param>
    /// <param name="size">Combinations' size</param>
    /// <param name="withReplacement">With or Without replacement</param>
    /// <param name="orderMatters">Does order matter</param>
    /// <param name="comparer">Comparer to use</param>
    public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> source,
                                                        int size,
                                                        bool withReplacement,
                                                        bool orderMatters) =>
      Combinations(source, size, withReplacement, orderMatters, EqualityComparer<T>.Default);

    /// <summary>
    /// [Multi]subset from multiset, e.g.
    /// [1, 1, 2] => [], [2], [1], [1, 2], [1, 1], [1, 1, 2]
    /// Or (order matters)
    /// [1, 1, 2] => [], [2], [1], [1, 2], [1, 1], [2, 1], [1, 1, 2], [1, 2, 1], [2, 1, 1]
    /// </summary>
    /// <param name="items">Initial multiset</param>
    /// <param name="orderMatters">If order matters or not</param>
    /// <param name="comparer">Comparer to use</param>
    /// <returns>[Multi]subsets</returns>
    public static IEnumerable<T[]> Subsets<T>(this IEnumerable<T> items,
                                                   bool withReplacement,
                                                   bool orderMatters,
                                                   IEqualityComparer<T> comparer) {
      if (items is null)
        throw new ArgumentNullException(nameof(items));

      comparer ??= EqualityComparer<T>.Default;

      if (withReplacement) {
        if (orderMatters)
          return OrderedSubsetsWithReplacement(items, comparer);
        else
          return UnOrderedSubsetsWithReplacement(items, comparer);
      }
      else if (orderMatters)
        return OrderedSubsets(items, comparer);
      else
        return Subsets(items, comparer);
    }

    /// <summary>
    /// [Multi]subset from multiset, e.g.
    /// [1, 1, 2] => [], [2], [1], [1, 2], [1, 1], [1, 1, 2]
    /// Or (order matters)
    /// [1, 1, 2] => [], [2], [1], [1, 2], [1, 1], [2, 1], [1, 1, 2], [1, 2, 1], [2, 1, 1]
    /// </summary>
    /// <param name="items">Initial multiset</param>
    /// <param name="orderMatters">If order matters or not</param>
    /// <returns>[Multi]subsets</returns>
    public static IEnumerable<T[]> Subsets<T>(this IEnumerable<T> items,
                                                   bool withReplacement,
                                                   bool orderMatters) =>
      Subsets(items, withReplacement, orderMatters, EqualityComparer<T>.Default);

    #endregion Public
  }
}
