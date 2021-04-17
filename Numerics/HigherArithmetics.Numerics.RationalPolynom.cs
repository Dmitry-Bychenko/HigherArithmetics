using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HigherArithmetics.Numerics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Polynom 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class RationalPolynom : IEquatable<RationalPolynom> {
    #region Private Data

    private readonly List<BigRational> m_Items;

    #endregion Private Data

    #region Algorithm

    private static BigRational C(int n, int k) {
      if (n == k || k == 0)
        return 1;

      BigRational result = 1;

      if (k > n / 2)
        k = n - k;

      for (int i = n; i > n - k; --i)
        result *= i;

      for (int i = 1; i <= k; ++i)
        result /= i;

      return result;
    }

    #endregion Algorithm

    #region Create

    /// <summary>
    /// Standard constructor
    /// </summary>
    public RationalPolynom(IEnumerable<BigRational> items) {
      m_Items = items
        ?.Reverse()
        ?.SkipWhile(x => x == 0)
        ?.Reverse()
        ?.ToList() ?? throw new ArgumentNullException(nameof(items));
    }

    /// <summary>
    /// Reconstruct polynom from its values
    /// for P(0) = 1, P(1) = ?, P(2) = 9, P(3) = 16, P(4) = 25, P(5) = ?
    /// var result = Reconstruct(0, new BigRational[] {1, BigRational.NaN, 9, 16, 25, BigRational.NaN});
    /// </summary>
    /// <param name="startAt">starting point</param>
    /// <param name="values">values (put BigRational.NaN for abscent points)</param>
    /// <returns></returns>
    public static RationalPolynom Reconstruct(int startAt, IEnumerable<BigRational> values) {
      if (values is null)
        throw new ArgumentNullException(nameof(values));

      var points = values
        .Select((v, i) => (x: (BigRational)i + startAt, y: v))
        .Where(item => !item.y.IsNaN)
        .ToArray();

      if (points.Length == 0)
        return NaN;

      BigRational[][] M = new BigRational[points.Length][];

      for (int r = 0; r < M.Length; ++r) {
        BigRational[] row = new BigRational[M.Length + 1];
        M[r] = row;

        row[M.Length] = points[r].y;

        BigRational x = points[r].x;
        BigRational v = 1;

        for (int c = 0; c < M.Length; ++c) {
          row[c] = v;

          v *= x;
        }
      }

      return new RationalPolynom(MatrixLowLevel.Solve(M));
    }

    /// <summary>
    /// Reconstruct polynom from its values
    /// for P(0) = 1, P(1) = ?, P(2) = 9, P(3) = 16, P(4) = 25, P(5) = ?
    /// var result = Reconstruct(1, BigRational.NaN, 9, 16, 25, BigRational.NaN);
    /// </summary>
    /// <param name="values">values (put BigRational.NaN for abscent points)</param>
    /// <returns></returns>
    public static RationalPolynom ReconstructZeroStarting(params BigRational[] values) =>
      Reconstruct(0, values);

    /// <summary>
    /// Reconstruct polynom from its values
    /// for P(1) = 1, P(2) = ?, P(3) = 9, P(4) = 16, P(5) = 25, P(6) = ?
    /// var result = Reconstruct(1, BigRational.NaN, 9, 16, 25, BigRational.NaN);
    /// </summary>
    /// <param name="values">values (put BigRational.NaN for abscent points)</param>
    /// <returns></returns>
    public static RationalPolynom ReconstructOneStarting(params BigRational[] values) =>
      Reconstruct(1, values);

    /// <summary>
    /// Reconstruct from (x, y) pairs by Lagrange interpolation
    /// </summary>
    public static RationalPolynom Reconstruct(IEnumerable<(BigRational x, BigRational y)> points) {
      if (points is null)
        throw new ArgumentNullException(nameof(points));

      var data = points
        .Where(p => !p.x.IsNaN && !p.y.IsNaN)
        .ToList();

      if (data.Count <= 0)
        return NaN;
      else if (data.Count == 1)
        return new RationalPolynom(new BigRational[] { data[0].x });

      BigRational[][] matrix = new BigRational[data.Count][];

      for (int r = 0; r < matrix.Length; ++r) {
        BigRational[] row = new BigRational[matrix.Length + 1];

        matrix[r] = row;

        row[^1] = data[r].y;

        BigRational v = 1;
        BigRational x = data[r].x;

        for (int c = 0; c < row.Length - 1; ++c) {
          row[c] = v;

          v *= x;
        }
      }

      return new RationalPolynom(MatrixLowLevel.Solve(matrix));
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Zero
    /// </summary>
    public static RationalPolynom Zero { get; } = new RationalPolynom(Array.Empty<BigRational>());

    /// <summary>
    /// Zero
    /// </summary>
    public static RationalPolynom One { get; } = new RationalPolynom(new BigRational[] { 1 });

    /// <summary>
    /// NaN
    /// </summary>
    public static RationalPolynom NaN { get; } = new RationalPolynom(new BigRational[] { BigRational.NaN });

    /// <summary>
    /// Compute At point x 
    /// </summary>
    public BigRational At(BigRational x) {
      BigRational value = 1;
      BigRational result = 0;

      for (int i = 0; i < m_Items.Count; ++i) {
        result += m_Items[i] * value;

        value *= x;
      }

      return result;
    }

    /// <summary>
    /// Items
    /// </summary>
    public IReadOnlyList<BigRational> Items => m_Items;

    /// <summary>
    /// Count
    /// </summary>
    public int Count => m_Items.Count;

    /// <summary>
    /// Items
    /// </summary>
    public BigRational this[int index] {
      get {
        if (index < 0 || index >= m_Items.Count)
          return 0;
        else
          return m_Items[index];
      }
    }

    /// <summary>
    /// Add Shift
    /// (x) => (x + shift)
    /// e.g. for shift = 2 we have 4 * (x + 2)**2 + 3 * (x + 2) + 1 => 4x**2 + 19x + 23
    /// </summary>
    public RationalPolynom WithShift(BigRational shift) {
      if (0 == shift)
        return this;

      BigRational[] a = Items.ToArray();
      BigRational[] r = new BigRational[a.Length];

      int n = a.Length - 1;

      for (int k = 0; k <= n; ++k) {
        BigRational coef = 0;

        for (int i = 0; i <= n - k; ++i)
          coef += a[n - i] * C(n - i, k) * shift.Pow(n - k - i);

        r[k] = coef;
      }

      return new RationalPolynom(r);
    }

    /// <summary>
    /// Add Shift
    /// (x) => (coefficient * x + shift)
    /// e.g. for shift = 2 we have 4 * (x + 2)**2 + 3 * (x + 2) + 1 => 4x**2 + 19x + 23
    /// </summary>
    public RationalPolynom WithLinear(BigRational coefficient, BigRational shift) {
      if (1 == coefficient && 0 == shift)
        return this;

      BigRational[] a = Items.ToArray();
      BigRational[] r = new BigRational[a.Length];

      int n = a.Length - 1;

      for (int k = 0; k <= n; ++k) {
        BigRational coef = 0;

        for (int i = 0; i <= n - k; ++i)
          coef += a[n - i] * C(n - i, k) * coefficient.Pow(k) * shift.Pow(n - k - i);

        r[k] = coef;
      }

      return new RationalPolynom(r);
    }

    /// <summary>
    /// To String
    /// </summary>
    public override string ToString() {
      if (m_Items.Count <= 0)
        return "0";

      StringBuilder sb = new ();

      for (int i = m_Items.Count - 1; i >= 0; --i) {
        BigRational v = m_Items[i];

        if (0 == v)
          continue;

        if (v < 0) {
          if (sb.Length > 0) {
            sb.Append(" - ");
            sb.Append(v.Abs());
          }
          else
            sb.Append(v);
        }
        else {
          if (sb.Length > 0)
            sb.Append(" + ");

          sb.Append(v);
        }

        if (i == 1)
          sb.Append(" * x");
        else if (i > 1)
          sb.Append($" * x ** {i}");
      }

      return sb.ToString();
    }

    /// <summary>
    /// Divide and find Remainder
    /// </summary>
    public RationalPolynom DivRem(RationalPolynom value, out RationalPolynom remainder) {
      if (value is null)
        throw new ArgumentNullException(nameof(value));
      else if (value.Count <= 0)
        throw new DivideByZeroException();

      if (value.Count > Count) {
        remainder = this;

        return Zero;
      }

      List<BigRational> quotinent = new ();
      List<BigRational> rem = m_Items.ToList();

      for (int i = 0; i <= Count - value.Count; ++i) {
        BigRational coef = rem[rem.Count - i - 1] / value[value.m_Items.Count - 1];

        quotinent.Add(coef);

        for (int j = 0; j < value.m_Items.Count; ++j)
          rem[rem.Count - i - j - 1] -= value.m_Items[value.m_Items.Count - j - 1] * coef;
      }

      remainder = new RationalPolynom(rem.Take(value.Count - 1));

      quotinent.Reverse();

      return new RationalPolynom(quotinent);
    }

    /// <summary>
    /// Derivative
    /// </summary>
    public RationalPolynom Derivative(int count = 1) {
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof(count));
      else if (count == 0)
        return this;
      else if (count >= Count)
        return Zero;

      List<BigRational> list = new (m_Items.Count - count);

      BigInteger coef = DiscreteMath.Factorial(count);

      for (int i = count; i < m_Items.Count; ++i) {
        list.Add(coef * m_Items[i]);

        coef = coef / (i - count + 1) * (i + 1);
      }
 
      return new RationalPolynom(list);
    }

    /// <summary>
    /// Integral
    /// </summary>
    public BigRational Integral(BigRational from, BigRational to) {
      BigRational result = 0;

      BigRational x1 = from;
      BigRational x2 = to;

      for (int i = 0; i < m_Items.Count; ++i) {
        result += m_Items[i] * (x2 - x1) / (i + 1);

        x1 *= from;
        x2 *= to;
      }

      return result;
    }

    /// <summary>
    /// Integral (indefinite)
    /// </summary>
    /// <param name="C">Integrating constant</param>
    public RationalPolynom Integral(BigRational c) {
      List<BigRational> coefs = new (m_Items.Count + 1) { c };

      for (int i = 0; i < m_Items.Count; ++i)
        coefs.Add(m_Items[i] / (i + 1));

      return new RationalPolynom(coefs);
    }

    /// <summary>
    /// GCD (Greatest Common Divisor)
    /// </summary>
    public RationalPolynom Gcd(RationalPolynom other) {
      if (other is null)
        throw new ArgumentNullException(nameof(other));

      RationalPolynom left = this;
      RationalPolynom right = other;
      RationalPolynom rem = left % right;

      while (rem.Count >= 1) {
        left = right;
        right = rem;

        rem = left % right;
      }

      return right;
    }

    #endregion Public

    #region Operators

    #region Comparison

    /// <summary>
    /// Equal
    /// </summary>
    public static bool operator ==(RationalPolynom left, RationalPolynom right) {
      if (ReferenceEquals(left, right))
        return true;
      else if (left is null || right is null)
        return false;

      return left.Equals(right);
    }

    /// <summary>
    /// Not Equal
    /// </summary>
    public static bool operator !=(RationalPolynom left, RationalPolynom right) {
      if (ReferenceEquals(left, right))
        return false;
      else if (left is null || right is null)
        return true;

      return !left.Equals(right);
    }

    #endregion Comparison

    #region Arithmetics

    /// <summary>
    /// Unary +
    /// </summary>
    public static RationalPolynom operator +(RationalPolynom value) => value;

    /// <summary>
    /// Unary -
    /// </summary>
    public static RationalPolynom operator -(RationalPolynom value) => value is null
      ? null
      : new RationalPolynom(value.m_Items.Select(x => -x));

    /// <summary>
    /// Binary + 
    /// </summary>
    public static RationalPolynom operator +(RationalPolynom left, RationalPolynom right) {
      if (left is null)
        throw new ArgumentNullException(nameof(left));
      else if (right is null)
        throw new ArgumentNullException(nameof(right));

      List<BigRational> result = new (Math.Max(left.Count, right.Count));

      for (int i = 0; i < result.Count; ++i)
        result.Add(left[i] + right[i]);

      return new RationalPolynom(result);
    }

    /// <summary>
    /// Binary - 
    /// </summary>
    public static RationalPolynom operator -(RationalPolynom left, RationalPolynom right) {
      if (left is null)
        throw new ArgumentNullException(nameof(left));
      else if (right is null)
        throw new ArgumentNullException(nameof(right));

      List<BigRational> result = new (Math.Max(left.Count, right.Count));

      for (int i = 0; i < result.Count; ++i)
        result.Add(left[i] - right[i]);

      return new RationalPolynom(result);
    }

    /// <summary>
    /// Binary * 
    /// </summary>
    public static RationalPolynom operator *(RationalPolynom value, BigRational coef) {
      if (value is null)
        throw new ArgumentNullException(nameof(coef));

      return new RationalPolynom(value.m_Items.Select(x => x * coef));
    }

    /// <summary>
    /// Binary * 
    /// </summary>
    public static RationalPolynom operator *(BigRational coef, RationalPolynom value) {
      if (value is null)
        throw new ArgumentNullException(nameof(coef));

      return new RationalPolynom(value.m_Items.Select(x => x * coef));
    }

    /// <summary>
    /// Binary / 
    /// </summary>
    public static RationalPolynom operator /(RationalPolynom value, BigRational coef) {
      if (value is null)
        throw new ArgumentNullException(nameof(coef));

      return new RationalPolynom(value.m_Items.Select(x => x / coef));
    }

    /// <summary>
    /// Binary *
    /// </summary>
    public static RationalPolynom operator *(RationalPolynom left, RationalPolynom right) {
      if (left is null)
        throw new ArgumentNullException(nameof(left));
      else if (right is null)
        throw new ArgumentNullException(nameof(right));

      if (left.Count <= 0 || right.Count <= 0)
        return Zero;

      List<BigRational> list = new (left.Count + right.Count + 1);

      for (int power = 0; power <= left.Count + right.Count; ++power) {
        BigRational s = 0;

        for (int i = power; i >= 0; --i) {
          int p1 = i;
          int p2 = power - i;

          if (p1 < left.Count && p2 < right.Count)
            s += left[p1] * right[p2];
        }

        list.Add(s);
      }

      return new RationalPolynom(list);
    }

    /// <summary>
    /// Division /
    /// </summary>
    public static RationalPolynom operator /(RationalPolynom left, RationalPolynom right) {
      if (left is null)
        throw new ArgumentNullException(nameof(left));
      else if (right is null)
        throw new ArgumentNullException(nameof(right));

      return left.DivRem(right, out var _);
    }

    /// <summary>
    /// Remainder %
    /// </summary>
    public static RationalPolynom operator %(RationalPolynom left, RationalPolynom right) {
      if (left is null)
        throw new ArgumentNullException(nameof(left));
      else if (right is null)
        throw new ArgumentNullException(nameof(right));

      left.DivRem(right, out var remainder);

      return remainder;
    }

    #endregion Arithmetics

    #endregion Operators

    #region IEquatable<Polynom>

    /// <summary>
    /// Equals
    /// </summary>
    public bool Equals(RationalPolynom other) {
      if (ReferenceEquals(this, other))
        return true;
      else if (other is null)
        return false;

      return m_Items.SequenceEqual(other.m_Items);
    }

    /// <summary>
    /// Equals
    /// </summary>
    public override bool Equals(object obj) => Equals(obj as RationalPolynom);

    /// <summary>
    /// Get Hash Code
    /// </summary>
    public override int GetHashCode() {
      return m_Items.Count <= 0
        ? 0
        : m_Items.Count ^ m_Items[0].GetHashCode();
    }

    #endregion IEquatable<Polynom>
  }

}
