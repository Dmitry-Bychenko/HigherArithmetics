using System;
using System.Numerics;

namespace HigherArithmetics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class NumericCodes {
    #region Public

    /// <summary>
    /// To (binary) Gray code
    /// </summary>
    public static BigInteger ToGrayCode(BigInteger value) => value >= 0
      ? value ^ (value >> 1)
      : throw new ArgumentOutOfRangeException(nameof(value));

    /// <summary>
    /// From (binary) Gray code
    /// </summary>
    public static BigInteger FromGrayCode(BigInteger value) {
      if (value < 0)
        throw new ArgumentOutOfRangeException(nameof(value));

      BigInteger result = value;

      for (BigInteger v = result >> 1; v > 0; v >>= 1)
        result ^= v;

      return result;
    }

    #endregion Public
  }


}
