using System.Numerics;

namespace HigherArithmetics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Integer Math
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class IntegerMath {
    #region Public

    /// <summary>
    /// Floor Div 
    /// </summary>
    public static int FloorDiv(int a, int b) {
      int div = a / b;
      int mod = a % b;

      if (div >= 0 && mod >= 0)
        return div;
      else if (mod == 0)
        return div;
      else
        return div - 1;
    }

    /// <summary>
    /// Floor Div
    /// </summary>
    public static long FloorDiv(long a, long b) {
      long div = a / b;
      long mod = a % b;

      if (div >= 0 && mod >= 0)
        return div;
      else if (mod == 0)
        return div;
      else
        return div - 1;
    }

    /// <summary>
    /// Floor Div
    /// </summary>
    public static BigInteger FloorDiv(BigInteger a, BigInteger b) {
      BigInteger div = BigInteger.DivRem(a, b, out var mod);

      if (div >= 0 && mod >= 0)
        return div;
      else if (mod == 0)
        return div;
      else
        return div - 1;
    }

    /// <summary>
    /// Ceiling Div 
    /// </summary>
    public static int CeilingDiv(int a, int b) {
      int div = a / b;
      int mod = a % b;

      if (mod == 0)
        return div;

      return div >= 0 ? div + 1 : div;
    }

    /// <summary>
    /// Ceiling Div 
    /// </summary>
    public static long CeilingDiv(long a, long b) {
      long div = a / b;
      long mod = a % b;

      if (mod == 0)
        return div;

      return div >= 0 ? div + 1 : div;
    }

    /// <summary>
    /// Ceiling Div
    /// </summary>
    public static BigInteger CeilingDiv(BigInteger a, BigInteger b) {
      BigInteger div = BigInteger.DivRem(a, b, out var mod);

      if (mod == 0)
        return div;

      return div >= 0 ? div + 1 : div;
    }

    #endregion Public
  }

}
