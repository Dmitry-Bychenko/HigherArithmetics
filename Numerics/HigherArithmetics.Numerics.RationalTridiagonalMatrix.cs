using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HigherArithmetics.Numerics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Tridiagonal matrix
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class TriDiagonalRationalMatrix
    : IEquatable<TriDiagonalRationalMatrix>,
      IReadOnlyList<(BigRational a, BigRational b, BigRational c)> {

    #region Private Data

    private readonly List<BigRational> m_A = new();
    private readonly List<BigRational> m_B = new();
    private readonly List<BigRational> m_C = new();

    BigRational[] m_Bs;

    private BigRational m_Determinant = BigRational.NaN;

    #endregion Private Data

    #region Algorithm

    private BigRational CoreDeterminant() {
      BigRational[] f = new BigRational[m_B.Count + 2];

      f[0] = 0;
      f[1] = 1;
      f[2] = m_B[0];

      for (int i = 1; i < m_B.Count; ++i)
        f[i + 2] = m_B[i] * f[i - 1 + 2] - m_A[i] * m_C[i - 1] * f[i - 2 + 2];

      return f[^1];
    }

    #endregion Algorothm

    #region Create

    // Standard constructor for inner use
    private TriDiagonalRationalMatrix() { }

    /// <summary>
    /// Standard constructor
    /// </summary>
    public TriDiagonalRationalMatrix(IEnumerable<(BigRational a, BigRational b, BigRational c)> lines)
      : this() {

      if (lines is null)
        throw new ArgumentNullException(nameof(lines));

      foreach (var (a, b, c) in lines) {
        m_A.Add(a);
        m_B.Add(b);
        m_C.Add(c);
      }

      if (m_A.Count > 0) {
        m_A[0] = 0.0;
        m_C[^1] = 0.0;
      }
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Left line
    /// </summary>
    public IReadOnlyList<BigRational> A => m_A;

    /// <summary>
    /// Central line
    /// </summary>
    public IReadOnlyList<BigRational> B => m_B;

    /// <summary>
    /// Right line
    /// </summary>
    public IReadOnlyList<BigRational> C => m_C;

    /// <summary>
    /// Count
    /// </summary>
    public int Count => m_B.Count;

    /// <summary>
    /// Cells
    /// </summary>
    public BigRational this[int row, int column] {
      get {
        if (row < 0 || column < 0 || row >= m_B.Count || column >= m_B.Count)
          return 0;
        else if (column == row - 1)
          return m_A[column];
        else if (column == row)
          return m_B[column];
        else if (column == row + 1)
          return m_C[column];

        return 0;
      }
    }

    /// <summary>
    /// Determinant
    /// </summary>
    public BigRational Determinant {
      get {
        if (!m_Determinant.IsNaN)
          return m_Determinant;

        m_Determinant = CoreDeterminant();

        return m_Determinant;
      }
    }

    /// <summary>
    /// Solve Matrix * x = f equation
    /// </summary>
    public BigRational[] Solve(IEnumerable<BigRational> f) {
      if (f is null)
        throw new ArgumentNullException(nameof(f));

      BigRational[] free = f.Take(m_B.Count + 1).ToArray();

      if (m_B.Count != free.Length)
        throw new ArgumentOutOfRangeException(nameof(f));

      if (m_Bs is null) {
        m_Bs = new BigRational[m_B.Count];
        m_Bs[0] = m_B[0];

        for (int i = 1; i < m_B.Count; ++i)
          m_Bs[i] = m_B[i] - m_A[i] / m_Bs[i - 1] * m_C[i - 1];
      }

      BigRational[] Fs = new BigRational[free.Length];

      Fs[0] = free[0];

      for (int i = 1; i < free.Length; ++i)
        Fs[i] = free[i] - m_A[i] / m_Bs[i - 1] * Fs[i - 1];

      BigRational[] result = new BigRational[free.Length];

      result[^1] = Fs[^1] / m_Bs[Fs.Length - 1];

      for (int i = free.Length - 2; i >= 0; --i)
        result[i] = (Fs[i] - m_C[i] * result[i + 1]) / m_Bs[i];

      return result;
    }

    /// <summary>
    /// Multiply Matrix * x
    /// </summary>
    public BigRational[] Apply(IEnumerable<BigRational> x) {
      if (x is null)
        throw new ArgumentNullException(nameof(x));

      BigRational[] v = x.Take(Count + 1).ToArray();

      if (v.Length < Count || v.Length > Count)
        throw new ArgumentOutOfRangeException(nameof(x));

      BigRational[] result = new BigRational[Count];

      if (result.Length <= 0)
        return result;
      else if (result.Length == 1) {
        result[0] = m_B[0] * v[0];

        return result;
      }
      else if (result.Length == 2) {
        result[0] = m_B[0] * v[0] + m_C[0] * v[1];
        result[1] = m_A[1] * v[0] + m_B[1] * v[1];

        return result;
      }

      result[0] = m_B[0] * v[0] + m_C[0] * v[1];
      result[^1] = m_B[result.Length - 2] * v[result.Length - 2] +
                                  m_C[result.Length - 1] * v[result.Length - 1];

      for (int r = 1; r < result.Length - 1; ++r)
        result[r] = m_A[r] * v[r - 1] + m_B[r] * v[r] + m_C[r] * v[r + 1];

      return result;
    }

    #endregion Public

    #region Operators

    #region Comparison

    /// <summary>
    /// Equals 
    /// </summary>
    public static bool operator ==(TriDiagonalRationalMatrix left, TriDiagonalRationalMatrix right) {
      if (ReferenceEquals(left, right))
        return true;
      else if (left is null || right is null)
        return false;
      else
        return left.Equals(right);
    }

    /// <summary>
    /// Not Equals 
    /// </summary>
    public static bool operator !=(TriDiagonalRationalMatrix left, TriDiagonalRationalMatrix right) {
      if (ReferenceEquals(left, right))
        return true;
      else if (left is not null || right is not null)
        return false;
      else
        return !left.Equals(right);
    }

    #endregion Comparison

    #region Arithmetics

    /// <summary>
    /// Unary + 
    /// </summary>
    public static TriDiagonalRationalMatrix operator +(TriDiagonalRationalMatrix value) {
      if (value is null)
        throw new ArgumentNullException(nameof(value));

      return value;
    }

    /// <summary>
    /// Unary - 
    /// </summary>
    public static TriDiagonalRationalMatrix operator -(TriDiagonalRationalMatrix value) {
      if (value is null)
        throw new ArgumentNullException(nameof(value));

      TriDiagonalRationalMatrix result = new();

      result.m_A.AddRange(value.m_A.Select(x => -x));
      result.m_B.AddRange(value.m_B.Select(x => -x));
      result.m_C.AddRange(value.m_C.Select(x => -x));

      return result;
    }

    /// <summary>
    /// Multiplication
    /// </summary>
    public static TriDiagonalRationalMatrix operator *(TriDiagonalRationalMatrix value, BigRational coefficient) {
      if (value is null)
        throw new ArgumentNullException(nameof(value));

      TriDiagonalRationalMatrix result = new();

      result.m_A.AddRange(value.m_A.Select(x => x * coefficient));
      result.m_B.AddRange(value.m_B.Select(x => x * coefficient));
      result.m_C.AddRange(value.m_C.Select(x => x * coefficient));

      return result;
    }

    /// <summary>
    /// Division
    /// </summary>
    public static TriDiagonalRationalMatrix operator /(TriDiagonalRationalMatrix value, BigRational coefficient) {
      if (value is null)
        throw new ArgumentNullException(nameof(value));

      TriDiagonalRationalMatrix result = new();

      result.m_A.AddRange(value.m_A.Select(x => x / coefficient));
      result.m_B.AddRange(value.m_B.Select(x => x / coefficient));
      result.m_C.AddRange(value.m_C.Select(x => x / coefficient));

      return result;
    }

    /// <summary>
    /// Multiplication
    /// </summary>
    public static TriDiagonalRationalMatrix operator *(BigRational coefficient, TriDiagonalRationalMatrix value) {
      if (value is null)
        throw new ArgumentNullException(nameof(value));

      TriDiagonalRationalMatrix result = new();

      result.m_A.AddRange(value.m_A.Select(x => x * coefficient));
      result.m_B.AddRange(value.m_B.Select(x => x * coefficient));
      result.m_C.AddRange(value.m_C.Select(x => x * coefficient));

      return result;
    }

    /// <summary>
    /// Addition
    /// </summary>
    public static TriDiagonalRationalMatrix operator +(TriDiagonalRationalMatrix left, TriDiagonalRationalMatrix right) {
      if (left is null)
        throw new ArgumentNullException(nameof(left));
      else if (right is null)
        throw new ArgumentNullException(nameof(right));
      else if (left.Count != right.Count)
        throw new ArgumentOutOfRangeException(nameof(right));

      TriDiagonalRationalMatrix result = new();

      result.m_A.AddRange(left.m_A.Zip(right.m_A, (l, r) => l + r));
      result.m_B.AddRange(left.m_B.Zip(right.m_B, (l, r) => l + r));
      result.m_C.AddRange(left.m_C.Zip(right.m_C, (l, r) => l + r));

      return result;
    }

    /// <summary>
    /// Subtraction
    /// </summary>
    public static TriDiagonalRationalMatrix operator -(TriDiagonalRationalMatrix left, TriDiagonalRationalMatrix right) {
      if (left is null)
        throw new ArgumentNullException(nameof(left));
      else if (right is null)
        throw new ArgumentNullException(nameof(right));
      else if (left.Count != right.Count)
        throw new ArgumentOutOfRangeException(nameof(right));

      TriDiagonalRationalMatrix result = new();

      result.m_A.AddRange(left.m_A.Zip(right.m_A, (l, r) => l - r));
      result.m_B.AddRange(left.m_B.Zip(right.m_B, (l, r) => l - r));
      result.m_C.AddRange(left.m_C.Zip(right.m_C, (l, r) => l - r));

      return result;
    }

    #endregion Arithmentics

    #endregion Operators

    #region IEquatable<TriDiagonalMatrix>

    /// <summary>
    /// Equals
    /// </summary>
    public bool Equals(TriDiagonalRationalMatrix other) {
      if (ReferenceEquals(this, other))
        return true;
      else if (other is null)
        return false;

      return m_A.SequenceEqual(other.m_A) &&
             m_B.SequenceEqual(other.m_B) &&
             m_C.SequenceEqual(other.m_C);
    }

    /// <summary>
    /// Equals
    /// </summary>
    public override bool Equals(object obj) => obj is TriDiagonalRationalMatrix matrix && Equals(matrix);

    /// <summary>
    /// Hash Code
    /// </summary>
    public override int GetHashCode() {
      if (m_B.Count <= 0)
        return 0;

      return m_B.Count ^ m_B[0].GetHashCode();
    }

    #endregion IEquatable<TriDiagonalMatrix>

    #region IReadOnlyList<(BigRational a, BigRational b, BigRational c)>

    /// <summary>
    /// Indexer
    /// </summary>
    public (BigRational a, BigRational b, BigRational c) this[int index] {
      get =>
        (index >= 0 && index < m_B.Count)
          ? (m_A[index], m_B[index], m_C[index])
          : throw new ArgumentOutOfRangeException(nameof(index));
    }

    /// <summary>
    /// Typed Enumerator
    /// </summary>
    public IEnumerator<(BigRational a, BigRational b, BigRational c)> GetEnumerator() => Enumerable
      .Range(0, m_B.Count)
      .Select(i => (m_A[0], m_B[i], m_C[i]))
      .GetEnumerator();

    /// <summary>
    /// Typeless Enumerator
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    #endregion IReadOnlyList<(BigRational a, BigRational b, BigRational c)>
  }


}
