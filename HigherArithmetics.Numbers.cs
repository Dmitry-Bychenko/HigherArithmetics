using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HigherArithmetics {
  
  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Special Numbers
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class Numbers {
    #region Algorithm

    private static BigInteger[] Matrix2Power(BigInteger[] matrix, int power) {
      BigInteger[] result = new BigInteger[] { 1, 0, 0, 1 };
      BigInteger[] factor = matrix.ToArray();

      for (int p = power; p != 0; p /= 2) {
        if (p % 2 != 0) {
          var b11 = result[0] * factor[0] + result[1] * factor[2];
          var b12 = result[0] * factor[1] + result[1] * factor[3];
          var b21 = result[2] * factor[0] + result[3] * factor[2];
          var b22 = result[2] * factor[1] + result[3] * factor[3];

          result[0] = b11;
          result[1] = b12;
          result[2] = b21;
          result[3] = b22;
        }

        var a11 = factor[0] * factor[0] + factor[1] * factor[2];
        var a12 = factor[0] * factor[1] + factor[1] * factor[3];
        var a21 = factor[2] * factor[0] + factor[3] * factor[2];
        var a22 = factor[2] * factor[1] + factor[3] * factor[3];

        factor[0] = a11;
        factor[1] = a12;
        factor[2] = a21;
        factor[3] = a22;
      }

      return result;
    }

    #endregion Algorithm

    #region Public

    /// <summary>
    /// Fibonacci number; f(0) = 0, f(1) = 1
    /// </summary>
    public static BigInteger Fibonacci(int index) =>
      Matrix2Power(index > 0 ? new BigInteger[] { 0, 1, 1, 1 } : new BigInteger[] { -1, 1, 1, 0 }, index)[2];

    /// <summary>
    /// Catalan number
    /// </summary>
    public static BigInteger Catalan(int index) {
      if (index < 0)
        return 0;

      BigInteger result = 1;
      BigInteger d = 1;

      for (int k = 2; k <= index; ++k) {
        result *= (k + index);
        d *= k;
      }

      return result / d;
    }

    #endregion Public
  }

}
