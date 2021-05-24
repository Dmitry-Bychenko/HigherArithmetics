using System;
using System.Numerics;

namespace HigherArithmetics {

  //-------------------------------------------------------------------------------------------------------------------
  // 
  /// <summary>
  /// Decimal Utilities
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class Decimals {

    private static readonly BigInteger Two96 = BigInteger.Pow(2, 96);

    #region Public

    /// <summary>
    /// Deconstruct Decimal into mantissa * 10^exponent
    /// </summary>
    public static (BigInteger mantissa, int exponent) DeconstructDecimal(decimal value) {
      int[] bits = decimal.GetBits(value);

      int exponent;
      BigInteger mantissa = 0;

      unchecked {
        mantissa += (uint)bits[2];
        mantissa <<= 32;

        mantissa += (uint)bits[1];
        mantissa <<= 32;

        mantissa += (uint)bits[0];

        if (((bits[3] >> 31) & 1) != 0)
          mantissa = -mantissa;

        exponent = (bits[3] >> 16) & 0b111111;
      }

      return (mantissa, -exponent);
    }

    /// <summary>
    /// Construct Decimal from mantissa * 10^exponent
    /// </summary>
    public static decimal ConstructDecimal(BigInteger mantissa, int exponent) {
      if (0 == mantissa)
        return 0m;

      if (exponent < -1000 || exponent > 1000)
        throw new ArgumentNullException(nameof(exponent));

      int sign = mantissa < 0 ? 1 : 0;

      mantissa = BigInteger.Abs(mantissa);

      if (exponent > 0) {
        mantissa *= BigInteger.Pow(10, exponent);

        exponent = 0;
      }
      else if (exponent < -28) {
        BigInteger factor = BigInteger.Pow(10, -28 - exponent);

        BigInteger mod = mantissa / factor;
        BigInteger rem = mantissa % factor;

        if (rem > factor / 2 || rem == factor / 2 && ((mod % 10) % 2 != 0))
          mod += 1;

        mantissa = mod;

        exponent = -28;
      }

      if (mantissa >= Two96)
        throw new ArgumentOutOfRangeException(nameof(mantissa));

      int[] bits = new int[4];

      unchecked {
        bits[0] = (int)(uint)(mantissa & 0xFFFFFFFF);
        mantissa >>= 32;
        bits[1] = (int)(uint)(mantissa & 0xFFFFFFFF);
        mantissa >>= 32;
        bits[2] = (int)(uint)(mantissa & 0xFFFFFFFF);

        bits[3] = (sign << 31) | (-exponent << 16);
      }

      return new decimal(bits);
    }

    /// <summary>
    /// Construct Decimal from mantissa * 10^exponent
    /// </summary>
    public static decimal ConstructDecimal((BigInteger mantissa, int exponent) value) =>
      ConstructDecimal(value.mantissa, value.exponent);

    #endregion Public
  }

}
