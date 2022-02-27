using HigherArithmetics.Primes;
using System;

namespace HigherArithmetics.Numerics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Long (Int64) extensions
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static partial class LongExtensions {
    #region Public

    /// <summary>
    /// Greatest Common Divisor
    /// </summary>
    /// <param name="left">Left value</param>
    /// <param name="right">Right value</param>
    /// <returns></returns>
    public static long Gcd(this long left, long right) {
      if (left < 0)
        left = -left;
      if (right < 0)
        right = -right;

      int shift;

      // GCD(0,v) == v; GCD(u,0) == u, GCD(0,0) == 0 
      if (left == 0)
        return right;

      if (right == 0)
        return left;

      // Let shift := lg K, where K is the greatest power of 2 dividing both u and v. 
      for (shift = 0; ((left | right) & 1) == 0; ++shift) {
        left >>= 1;
        right >>= 1;
      }

      while ((left & 1) == 0)
        left >>= 1;

      // From here on, u is always odd. 
      do {
        // remove all factors of 2 in v - they are not common 
        // note: v is not zero, so while will terminate
        while ((right & 1) == 0)  /* Loop X */
          right >>= 1;

        // Now u and v are both odd. Swap if necessary so u <= v,
        // then set v = v - u (which is even). For bignums, the
        // swapping is just pointer movement, and the subtraction
        // can be done in-place. 
        if (left > right) 
          (left, right) = (right, left);
          
        // Here v >= u.
        right -= left;

      } while (right != 0);

      // restore common factors of 2 
      return left << shift;
    }

    /// <summary>
    /// Extended Euclid Algorithm
    /// </summary>
    public static (long LeftFactor,
                   long RightFactor,
                   long Gcd) Egcd(this long left, long right) {
      long leftFactor = 0;
      long rightFactor = 1;

      long u = 1;
      long v = 0;
      long gcd = 0;

      while (left != 0) {
        long q = right / left;
        long r = right % left;

        long m = leftFactor - u * q;
        long n = rightFactor - v * q;

        right = left;
        left = r;
        leftFactor = u;
        rightFactor = v;
        u = m;
        v = n;

        gcd = right;
      }

      return (LeftFactor: leftFactor,
              RightFactor: rightFactor,
              Gcd: gcd);
    }

    /// <summary>
    /// Mod Inversion
    /// </summary>
    public static long ModInversion(this long value, long modulo) {
      var egcd = Egcd(value, modulo);

      if (egcd.Gcd != 1)
        throw new ArgumentException("Invalid modulo", nameof(modulo));

      long result = egcd.LeftFactor;

      if (result < 0)
        result += modulo;

      return result % modulo;
    }

    /// <summary>
    /// Mod Division
    /// </summary>
    public static long ModDivision(this long left, long right, long modulo) =>
      (left * ModInversion(right, modulo)) % modulo;

    /// <summary>
    /// Is Prime
    /// </summary>
    public static bool IsPrime(this long value) {
      if (value <= 1)
        return false;

      if ((value % 2) == 0)
        return (value == 2);
      if ((value % 3) == 0)
        return (value == 3);
      if ((value % 5) == 0)
        return (value == 5);
      if ((value % 7) == 0)
        return (value == 7);
      if ((value % 11) == 0)
        return (value == 11);

      if (value <= KnownPrimes.MaxKnownPrime)
        return KnownPrimes.IsKnownPrime((int)value);

      if (value < int.MaxValue) {
        long start = KnownPrimes.MaxKnownPrime;

        start += start % 2 == 0 ? 1 : 0;
        start = start < 3 ? 3 : start;

        long stop = value > (1L << 52)
          ? Math.Min((long)(Math.Sqrt(value) + 100), value - 1)
          : (long)(Math.Sqrt(value) + 1);

        for (long d = start; d <= stop; d += 2)
          if (value % d == 0)
            return false;

        return true;
      }

      return PrimeNumbers.RabinMillesPrimeTest(value, 5);
    }

    #endregion Public
  }

}
