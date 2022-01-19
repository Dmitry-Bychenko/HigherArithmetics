using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace HigherArithmetics.Linq.Generators {
  
  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Fibonacci Generator
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class FibonacciGenerator {
    #region Public

    /// <summary>
    /// Fibonacci items
    /// </summary>
    /// <param name="topItems">Top Items</param>
    public static IEnumerable<BigInteger> Enumerate(IEnumerable<BigInteger> topItems) {
      BigInteger[] current = topItems is null 
        ? new BigInteger[] { 0, 1 } 
        : topItems.ToArray();

      if (0 == current.Length)
        while (true)
          yield return 0;

      while (true) {
        yield return current[0];

        BigInteger last = 0;

        for (int i = 0; i < current.Length; ++i) 
          last += current[i];

        for (int i = 0; i < current.Length - 1; ++i)
          current[i] = current[i + 1];

        current[^1] = last;
      }
    }

    /// <summary>
    /// Fibonacci items
    /// </summary>
    /// <param name="topItems">Top Items</param>
    public static IEnumerable<BigInteger> Enumerate(params BigInteger[] topItems) =>
      Enumerate((IEnumerable<BigInteger>) topItems);

    /// <summary>
    /// Fibonacci items
    /// </summary>
    public static IEnumerable<BigInteger> Enumerate() => Enumerate(0, 1);

    #endregion Public
  }

}
