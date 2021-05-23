using System;

namespace HigherArithmetics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Floating Point Utilities
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class FloatingPoint {
    #region Public

    /// <summary>
    /// Deconstructs Double into mantissa * 2^exponent
    /// </summary>
    public static (long mantissa, int exponent) DeconstructDouble(double value) {
      byte[] bits = BitConverter.GetBytes(value);

      int sign = 1 - ((bits[7] >> 7) & 1) * 2;

      bits[7] = (byte)((bits[7] | 0x80) ^ 0x80);

      int exp = ((bits[7] << 4) | (bits[6] >> 4)) - 1023;

      long mantissa = (bits[6] & 0xF) + 16;

      mantissa <<= 48;

      for (int i = 5; i >= 0; --i) {
        int item = bits[i];

        mantissa += ((long)item) << (i * 8);
      }

      return (mantissa * sign, exp - 52);
    }

    /// <summary>
    /// Constructs Double from mantissa * 2^exponent representation
    /// </summary>
    public static double ConstructDouble(long mantissa, int exponent) {
      if (mantissa == 0)
        return 0.0;

      int sign = 0;

      if (exponent < -1200 || exponent > 1200)
        throw new ArgumentOutOfRangeException(nameof(exponent));

      if (mantissa < 0) {
        sign = 1;

        if (mantissa == long.MinValue)
          mantissa = long.MaxValue;
        else
          mantissa = unchecked(-mantissa);
      }

      while (mantissa < 4503599627370496L) {
        mantissa <<= 1;
        exponent -= 1;
      }

      while (mantissa > 9007199254740991L) {
        mantissa >>= 1;
        exponent += 1;
      }

      if (exponent < -1075 || exponent > 972)
        throw new ArgumentOutOfRangeException(nameof(exponent));
      if (mantissa < 4503599627370496L)
        throw new ArgumentOutOfRangeException(nameof(mantissa));
      if (mantissa > 9007199254740991L)
        throw new ArgumentOutOfRangeException(nameof(mantissa));

      byte[] bits = new byte[8];

      exponent = (exponent + 52 + 1023);

      bits[7] = (byte)((sign << 7) | (exponent >> 4));
      bits[6] |= (byte)((exponent & 0xF) << 4);

      bits[6] |= (byte)((mantissa >> 48) & 0xF);

      for (int i = 0; i <= 5; ++i)
        bits[i] = (byte)((mantissa >> (i * 8)) & 0xFF);

      return BitConverter.ToDouble(bits);
    }

    /// <summary>
    /// Constructs Double from mantissa * 2^exponent representation
    /// </summary>
    public static double ConstructDouble((long mantissa, int exponent) value) =>
      ConstructDouble(value.mantissa, value.exponent);

    /// <summary>
    /// Deconstructs Single into mantissa * 2^exponent
    /// </summary>
    public static (long mantissa, int exponent) DeconstructSingle(float value) {
      byte[] bits = BitConverter.GetBytes(value);

      int sign = 1 - ((bits[3] >> 7) & 1) * 2;

      bits[3] = (byte)((bits[3] | 0x80) ^ 0x80);

      int exp = ((bits[3] << 1) | (bits[2] >> 7)) - 127;

      long mantissa = bits[2] | 128;

      mantissa <<= 16;

      for (int i = 1; i >= 0; --i) {
        int item = bits[i];

        mantissa += ((long)item) << (i * 8);
      }

      return (mantissa * sign, exp - 23);
    }

    /// <summary>
    /// Constructs Single from mantissa * 2^exponent representation
    /// </summary>
    public static float ConstructSingle(long mantissa, int exponent) {
      if (mantissa == 0)
        return 0.0f;

      int sign = 0;

      if (exponent < -1200 || exponent > 1200)
        throw new ArgumentOutOfRangeException(nameof(exponent));

      if (mantissa < 0) {
        sign = 1;

        if (mantissa == long.MinValue)
          mantissa = long.MaxValue;
        else
          mantissa = unchecked(-mantissa);
      }

      while (mantissa < 8388608L) {
        mantissa <<= 1;
        exponent -= 1;
      }

      while (mantissa > 16777215L) {
        mantissa >>= 1;
        exponent += 1;
      }

      if (exponent < -150 || exponent > 105)
        throw new ArgumentOutOfRangeException(nameof(exponent));
      if (mantissa < 8388608L)
        throw new ArgumentOutOfRangeException(nameof(mantissa));
      if (mantissa > 16777215L)
        throw new ArgumentOutOfRangeException(nameof(mantissa));

      byte[] bits = new byte[4];

      exponent = (exponent + 23 + 127);

      bits[3] = (byte)((sign << 7) | (exponent >> 1));

      bits[2] |= (byte)((exponent & 1) << 7);
      bits[2] |= (byte)((mantissa >> 16) & 127);

      for (int i = 0; i <= 1; ++i)
        bits[i] = (byte)((mantissa >> (i * 8)) & 0xFF);

      return BitConverter.ToSingle(bits);
    }

    /// <summary>
    /// Constructs Single from mantissa * 2^exponent representation
    /// </summary>
    public static float ConstructSingle((long mantissa, int exponent) value) =>
      ConstructSingle(value.mantissa, value.exponent);

    #endregion Public
  }

}
