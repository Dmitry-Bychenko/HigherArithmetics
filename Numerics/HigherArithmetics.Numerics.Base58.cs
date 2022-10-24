using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace HigherArithmetics.Numerics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Base 58 Possible formats
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public enum Base58Format {
    /// <summary>
    /// Bitcoin
    /// </summary>
    Bitcoin = 0,

    /// <summary>
    /// Ripple
    /// </summary>
    Ripple = 1,

    /// <summary>
    /// Flickr
    /// </summary>
    Flickr = 2,
  }

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Base 58 Encoder
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class Base58Encoder {
    #region Data

    private static readonly IReadOnlyDictionary<Base58Format, string> s_Alphabet =
      new Dictionary<Base58Format, string> {
        { Base58Format.Bitcoin, "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz" },
        { Base58Format.Ripple, "rpshnaf39wBUDNEGHJKLM4PQRST7VWXYZ2bcdeCg65jkm8oFqi1tuvAxyz" },
        { Base58Format.Flickr, "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ" },
      };

    private static readonly IReadOnlyDictionary<Base58Format, Dictionary<char, int>> s_Reversed;

    #endregion Data

    #region Create

    static Base58Encoder() {
      var reserved = new Dictionary<Base58Format, Dictionary<char, int>>();

      s_Reversed = reserved;

      foreach (var pair in s_Alphabet)
        reserved.Add(pair.Key, pair.Value.Select((c, i) => (c, i)).ToDictionary(p => p.c, p => p.i));
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="value">Value to Encode</param>
    /// <param name="format">Format to use</param>
    /// <returns>Value encoded as Base58</returns>
    public static string Encode(BigInteger value, Base58Format format) {
      var alphabet = s_Alphabet[format];

      if (value == 0)
        return alphabet[0].ToString();

      StringBuilder sb = new StringBuilder();

      for (BigInteger v = BigInteger.Abs(value); v != 0; v /= alphabet.Length)
        sb.Append(alphabet[(int)(v % alphabet.Length)]);

      if (value < 0)
        sb.Append('-');

      return string.Concat(sb.ToString().Reverse());
    }

    /// <summary>
    /// Try decode from Base58
    /// </summary>
    /// <param name="value">Value to decode</param>
    /// <param name="format">Format to use</param>
    /// <param name="result">Decoded value on success, 0 if decoding is not possible</param>
    /// <returns>True if succeeded, false otherwise</returns>
    public static bool TryDecode(string value, Base58Format format, out BigInteger result) {
      result = BigInteger.Zero;

      value = value?.Trim();

      if (string.IsNullOrEmpty(value))
        return false;

      if (value == "-")
        return false;
      if (format < Base58Format.Bitcoin || format > Base58Format.Flickr)
        return false;

      int sign = value[0] == '-' ? -1 : 1;

      var dict = s_Reversed[format];

      for (int i = value[0] == '-' ? 1 : 0; i < value.Length; ++i) {
        char c = value[i];

        if (dict.TryGetValue(c, out int d))
          result = result * dict.Count + d;
        else {
          result = 0;

          return false;
        }
      }

      result *= sign;

      return true;
    }

    /// <summary>
    /// Decode from Base58
    /// </summary>
    /// <param name="value">Value to decode</param>
    /// <param name="format">Format to use</param>
    /// <returns>Decoded value</returns>
    /// <exception cref="ArgumentNullException">When value is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">When format is out of range</exception>
    /// <exception cref="FormatException">When value doesn't fit Base 58 format</exception>
    public static BigInteger Decode(string value, Base58Format format) {
      if (value is null)
        throw new ArgumentNullException(nameof(value));
      if (format < Base58Format.Bitcoin || format > Base58Format.Flickr)
        throw new ArgumentOutOfRangeException(nameof(format));

      if (TryDecode(value, format, out BigInteger result))
        return result;

      throw new FormatException($"{nameof(value)} is not in Base58 format.");
    }

    #endregion Public
  }

}
