using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HigherArithmetics.Numerics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Real Value Matrix 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class RationalMatrix
    : ICloneable,
      IEquatable<RationalMatrix>,
      IFormattable,
      IEnumerable<BigRational> {

    #region Private Data

    // Items
    internal BigRational[][] m_Items;

    private BigRational m_Determinant = BigRational.NaN;

    private int m_Rank = -1;

    private BigRational[] m_LinearSolutions;

    #endregion Private Data

    #region Algorithm 
    #endregion Algorithm

    #region Create

    // Empty Constructor
    private RationalMatrix() { }

    // Low level constructor
    private RationalMatrix(BigRational[][] items)
      : this() {
      m_Items = items;
    }

    // Standard Constructor
    private RationalMatrix(int lines, int columns)
      : this() {

      if (lines <= 0)
        throw new ArgumentOutOfRangeException(nameof(lines));
      else if (columns <= 0)
        throw new ArgumentOutOfRangeException(nameof(columns));

      m_Items = Enumerable
        .Range(0, lines)
        .Select(_ => new BigRational[columns])
        .ToArray();
    }

    // Standard Constructor (square matrix)
    private RationalMatrix(int size) : this(size, size) { }

    /// <summary>
    /// Union matrix
    /// </summary>
    /// <param name="size">Size</param>
    public static RationalMatrix Union(int size) {
      RationalMatrix result = new(size);

      for (int i = result.LineCount - 1; i >= 0; --i)
        result.m_Items[i][i] = 1;

      return result;
    }

    /// <summary>
    /// Zero matrix
    /// </summary>
    /// <param name="size">Size</param>
    public static RationalMatrix Zero(int size) => new(size);

    /// <summary>
    /// Create Matrix
    /// </summary>
    /// <param name="lines">Lines</param>
    /// <param name="columns">Columns</param>
    /// <param name="createItem">Item at line and column</param>
    /// <returns></returns>
    public static RationalMatrix Create(int lines, int columns, Func<int, int, BigRational> createItem) {
      if (lines <= 0)
        throw new ArgumentOutOfRangeException(nameof(lines));
      else if (columns <= 0)
        throw new ArgumentOutOfRangeException(nameof(columns));
      else if (createItem is null)
        throw new ArgumentNullException(nameof(createItem));

      RationalMatrix result = new(lines, columns);

      for (int r = 0; r < lines; ++r)
        for (int c = 0; c < columns; ++c)
          result.m_Items[r][c] = createItem(r, c);

      return result;
    }

    /// <summary>
    /// Create Square Matrix
    /// </summary>
    /// <param name="size">Size</param>
    /// <param name="createItem">Item at line and column</param>
    /// <returns></returns>
    public static RationalMatrix Create(int size, Func<int, int, BigRational> createItem) => Create(size, size, createItem);

    /// <summary>
    /// Create
    /// </summary>
    public static RationalMatrix Create(IEnumerable<IEnumerable<BigRational>> data) {
      if (data is null)
        throw new ArgumentNullException(nameof(data));

      var copy = data
        .Select(line => line.ToArray())
        .ToArray();

      return new RationalMatrix(copy);
    }

    #endregion Create

    #region Public

    #region General

    /// <summary>
    /// Perform a func and create a new matrix
    /// </summary>
    /// <param name="func">func: value, line, column returns a new value</param>
    /// <returns>New Matrix</returns>
    public RationalMatrix Perform(Func<BigRational, int, int, BigRational> func) {
      if (func is null)
        throw new ArgumentNullException(nameof(func));

      RationalMatrix result = new(LineCount, ColumnCount);

      for (int r = 0; r < m_Items.Length; ++r)
        for (int c = 0; c < m_Items[r].Length; ++c)
          result.m_Items[r][c] = func(m_Items[r][c], r, c);

      return result;
    }

    /// <summary>
    /// Perform a func and create a new matrix
    /// </summary>
    /// <param name="func">func: line, column returns a new value</param>
    /// <returns>New Matrix</returns>
    public RationalMatrix Perform(Func<int, int, BigRational> func) {
      if (func is null)
        throw new ArgumentNullException(nameof(func));

      RationalMatrix result = new(LineCount, ColumnCount);

      for (int r = 0; r < m_Items.Length; ++r)
        for (int c = 0; c < m_Items[r].Length; ++c)
          result.m_Items[r][c] = func(r, c);

      return result;
    }

    /// <summary>
    /// Perform a func and create a new matrix
    /// </summary>
    /// <param name="func">func: value returns a new value</param>
    /// <returns>New Matrix</returns>
    public RationalMatrix Perform(Func<BigRational, BigRational> func) {
      if (func is null)
        throw new ArgumentNullException(nameof(func));

      RationalMatrix result = new(LineCount, ColumnCount);

      for (int r = 0; r < m_Items.Length; ++r)
        for (int c = 0; c < m_Items[r].Length; ++c)
          result.m_Items[r][c] = func(m_Items[r][c]);

      return result;
    }

    /// <summary>
    /// Lines
    /// </summary>
    public int LineCount => m_Items.Length;

    /// <summary>
    /// Columns
    /// </summary>
    public int ColumnCount => m_Items[0].Length;

    /// <summary>
    /// Item Count
    /// </summary>
    public int ItemCount => m_Items.Length * m_Items[0].Length;

    /// <summary>
    /// Lines
    /// </summary>
    public IEnumerable<BigRational[]> Lines {
      get {
        foreach (var line in m_Items)
          yield return line.ToArray();
      }
    }

    /// <summary>
    /// Lines
    /// </summary>
    public IEnumerable<BigRational[]> Columns {
      get {
        int N = ColumnCount;

        for (int i = 0; i < N; ++i) {
          BigRational[] result = new BigRational[m_Items.Length];

          for (int j = 0; j < result.Length; ++j)
            result[j] = m_Items[i][j];

          yield return result;
        }
      }
    }

    /// <summary>
    /// Items
    /// </summary>
    public IEnumerable<BigRational> Items {
      get {
        foreach (var line in m_Items)
          foreach (var item in line)
            yield return item;
      }
    }

    /// <summary>
    /// Cell
    /// </summary>
    /// <param name="line">Line</param>
    /// <param name="column">Column</param>
    public BigRational Cell(int line, int column) => m_Items[line][column];

    /// <summary>
    /// Cell
    /// </summary>
    /// <param name="line">Line</param>
    /// <param name="column">Column</param>
    public BigRational this[int line, int column] => Cell(line, column);

    #endregion General

    #region Standard

    /// <summary>
    /// Transpose
    /// </summary>
    public RationalMatrix Transpose() {
      RationalMatrix result = new(ColumnCount, LineCount);

      for (int r = result.m_Items.Length - 1; r >= 0; --r)
        for (int c = result.m_Items[0].Length - 1; c >= 0; --c)
          result.m_Items[r][c] = m_Items[c][r];

      return result;
    }

    /// <summary>
    /// Determinant
    /// </summary>
    public BigRational Determinant {
      get {
        if (m_Determinant.IsNaN)
          m_Determinant = MatrixLowLevel.Determinant(m_Items);

        return m_Determinant;
      }
    }

    /// <summary>
    /// Rank
    /// </summary>
    public int Rank {
      get {
        if (m_Rank < 0)
          m_Rank = MatrixLowLevel.Rank(m_Items);

        return m_Rank;
      }
    }

    /// <summary>
    /// Inverse
    /// </summary>
    public RationalMatrix Inverse() {
      if (ColumnCount != LineCount)
        throw new InvalidOperationException("Only square matrix can be inversed.");

      try {
        return new RationalMatrix(MatrixLowLevel.Inverse(m_Items));
      }
      catch (ArgumentException) {
        throw new InvalidOperationException("Degenerated matrix can't be inversed.");
      }
    }

    /// <summary>
    /// Pseudo Inverse
    /// </summary>
    public RationalMatrix PseudoInverse() {
      BigRational[][] tran = MatrixLowLevel.Transpose(m_Items);

      BigRational[][] result = MatrixLowLevel.Multiply(MatrixLowLevel.Inverse(MatrixLowLevel.Multiply(tran, m_Items)), tran);

      return new RationalMatrix(result);
    }

    /// <summary>
    /// Linear Solutions
    /// </summary>
    public BigRational[] LinearSoution {
      get {
        if (m_LinearSolutions is null)
          m_LinearSolutions = MatrixLowLevel.Solve(m_Items);

        return m_LinearSolutions;
      }
    }

    #endregion Standard

    #endregion Public

    #region Operators

    #region Comparison

    /// <summary>
    /// Equals
    /// </summary>
    public static bool operator ==(RationalMatrix left, RationalMatrix right) {
      if (ReferenceEquals(left, right))
        return true;
      else if (right is null)
        return false;
      else if (left is null)
        return false;

      return left.Equals(right);
    }

    /// <summary>
    /// Not Equals
    /// </summary>
    public static bool operator !=(RationalMatrix left, RationalMatrix right) {
      if (ReferenceEquals(left, right))
        return false;
      else if (right is null)
        return true;
      else if (left is null)
        return true;

      return !left.Equals(right);
    }

    #endregion Comparison

    #region Arithmetics

    /// <summary>
    /// Unary +
    /// </summary>
    public static RationalMatrix operator +(RationalMatrix value) {
      if (value is null)
        throw new ArgumentNullException(nameof(value));

      return value;
    }

    /// <summary>
    /// Unary -
    /// </summary>
    public static RationalMatrix operator -(RationalMatrix value) {
      if (value is null)
        throw new ArgumentNullException(nameof(value));

      RationalMatrix result = value.Clone();

      foreach (BigRational[] line in result.m_Items)
        for (int i = line.Length; i >= 0; --i)
          line[i] = -line[i];

      return result;
    }

    /// <summary>
    /// Multiplication by number
    /// </summary>
    public static RationalMatrix operator *(RationalMatrix matrix, BigRational value) {
      if (matrix is null)
        throw new ArgumentNullException(nameof(matrix));

      if (value == 1)
        return matrix;

      RationalMatrix result = matrix.Clone();

      foreach (BigRational[] line in result.m_Items)
        for (int i = line.Length; i >= 0; --i)
          line[i] = line[i] * value;

      return result;
    }

    /// <summary>
    /// Multiplication by number
    /// </summary>
    public static RationalMatrix operator *(BigRational value, RationalMatrix matrix) => matrix * value;

    /// <summary>
    /// Division by number
    /// </summary>
    public static RationalMatrix operator /(RationalMatrix matrix, BigRational value) {
      if (matrix is null)
        throw new ArgumentNullException(nameof(matrix));

      if (value == 1)
        return matrix;

      RationalMatrix result = matrix.Clone();

      foreach (BigRational[] line in result.m_Items)
        for (int i = line.Length; i >= 0; --i)
          line[i] = line[i] / value;

      return result;
    }

    /// <summary>
    /// Matrix Addition
    /// </summary>
    public static RationalMatrix operator +(RationalMatrix left, RationalMatrix right) {
      if (left is null)
        throw new ArgumentNullException(nameof(left));
      else if (right is null)
        throw new ArgumentNullException(nameof(right));

      if (left.LineCount != right.LineCount)
        throw new ArgumentException($"Right matrix must have {left.LineCount} lines, actual {right.LineCount}", nameof(right));
      else if (left.ColumnCount != right.ColumnCount)
        throw new ArgumentException($"Right matrix must have {left.ColumnCount} columns, actual {right.ColumnCount}", nameof(right));

      RationalMatrix result = new(left.LineCount, left.LineCount);

      for (int r = right.LineCount - 1; r >= 0; --r)
        for (int c = right.ColumnCount - 1; c >= 0; --c)
          result.m_Items[r][c] = left.m_Items[r][c] + right.m_Items[r][c];

      return result;
    }

    /// <summary>
    /// Matrix Subtractions
    /// </summary>
    public static RationalMatrix operator -(RationalMatrix left, RationalMatrix right) {
      if (left is null)
        throw new ArgumentNullException(nameof(left));
      else if (right is null)
        throw new ArgumentNullException(nameof(right));

      if (left.LineCount != right.LineCount)
        throw new ArgumentException($"Right matrix must have {left.LineCount} lines, actual {right.LineCount}", nameof(right));
      else if (left.ColumnCount != right.ColumnCount)
        throw new ArgumentException($"Right matrix must have {left.ColumnCount} columns, actual {right.ColumnCount}", nameof(right));

      RationalMatrix result = new(left.LineCount, left.LineCount);

      for (int r = right.LineCount - 1; r >= 0; --r)
        for (int c = right.ColumnCount - 1; c >= 0; --c)
          result.m_Items[r][c] = left.m_Items[r][c] - right.m_Items[r][c];

      return result;
    }

    /// <summary>
    /// Matrix Mutiplication
    /// </summary>
    public static RationalMatrix operator *(RationalMatrix left, RationalMatrix right) {
      if (left is null)
        throw new ArgumentNullException(nameof(left));
      else if (right is null)
        throw new ArgumentNullException(nameof(right));

      if (left.ColumnCount != right.LineCount)
        throw new ArgumentException($"Right matrix must have {left.ColumnCount} liness, actual {right.LineCount}", nameof(right));

      RationalMatrix result = new(left.LineCount, right.ColumnCount);

      for (int r = result.m_Items.Length - 1; r >= 0; --r)
        for (int c = result.m_Items[0].Length - 1; c >= 0; --c) {
          BigRational v = 0;

          for (int i = right.LineCount - 1; i >= 0; --i)
            v += left.m_Items[r][i] * right.m_Items[i][c];

          result.m_Items[r][c] = v;
        }

      return result;
    }

    /// <summary>
    /// Matrix Division
    /// </summary>
    public static RationalMatrix operator /(RationalMatrix left, RationalMatrix right) {
      if (left is null)
        throw new ArgumentNullException(nameof(left));
      else if (right is null)
        throw new ArgumentNullException(nameof(right));

      if (left.ColumnCount != right.LineCount)
        throw new ArgumentException($"Right matrix must have {left.ColumnCount} liness, actual {right.LineCount}", nameof(right));
      else if (right.ColumnCount != right.LineCount)
        throw new ArgumentException("Divisor must be a square matrix.", nameof(right));

      return new RationalMatrix(MatrixLowLevel.Multiply(left.m_Items, MatrixLowLevel.Inverse(right.m_Items)));
    }

    /// <summary>
    /// Matrix Division
    /// </summary>
    public static RationalMatrix operator /(BigRational left, RationalMatrix right) {
      if (right is null)
        throw new ArgumentNullException(nameof(right));

      if (right.ColumnCount != right.LineCount)
        throw new ArgumentException("Divisor must be a square matrix.", nameof(right));

      BigRational[][] result = MatrixLowLevel.Inverse(right.m_Items);

      for (int i = result.Length - 1; i >= 0; --i) {
        BigRational[] line = result[i];

        for (int j = line.Length - 1; j >= 0; --j)
          line[j] = left * line[j];
      }

      return new RationalMatrix(result);
    }

    #endregion Arithmetics

    #endregion Operators

    #region ICloneable

    /// <summary>
    /// Deep copy
    /// </summary>
    public RationalMatrix Clone() => new() {
      m_Items = m_Items
                   .Select(line => {
                     BigRational[] result = new BigRational[line.Length];
                     Array.Copy(line, 0, result, 0, line.Length);
                     return result;
                   })
                   .ToArray()
    };


    /// <summary>
    /// Deep Copy
    /// </summary>
    object ICloneable.Clone() => this.Clone();

    #endregion ICloneable

    #region IEquatable<Matrix>

    /// <summary>
    /// Equals
    /// </summary>
    public bool Equals(RationalMatrix other, BigRational tolerance) {
      if (tolerance < 0)
        throw new ArgumentOutOfRangeException(nameof(tolerance));

      if (ReferenceEquals(this, other))
        return true;
      else if (other is null)
        return false;

      if (this.ColumnCount != other.ColumnCount || this.LineCount != other.LineCount)
        return false;

      for (int r = LineCount - 1; r >= 0; --r)
        for (int c = ColumnCount - 1; c >= 0; --c)
          if ((m_Items[r][c] - other.m_Items[r][c]).Abs() < tolerance)
            continue; // for BigRational.NaN case or alike
          else
            return false;

      return true;
    }

    /// <summary>
    /// Equals
    /// </summary>
    public bool Equals(RationalMatrix other) {
      if (ReferenceEquals(this, other))
        return true;
      else if (other is null)
        return false;

      if (this.ColumnCount != other.ColumnCount || this.LineCount != other.LineCount)
        return false;

      return m_Items
        .Zip(other.m_Items, (left, right) => left.SequenceEqual(right))
        .All(item => item);
    }

    /// <summary>
    /// Equals
    /// </summary>
    public override bool Equals(object obj) => Equals(obj as RationalMatrix);

    /// <summary>
    /// Hash Code
    /// </summary>
    public override int GetHashCode() => unchecked((LineCount << 16) ^ ColumnCount ^ m_Items[0][0].GetHashCode());

    #endregion IEquatable<Matrix>

    #region IFormattable

    /// <summary>
    /// To String
    /// </summary>
    public string ToString(string format, IFormatProvider formatProvider) {
      if (formatProvider is null)
        formatProvider = CultureInfo.InvariantCulture;

      if (string.IsNullOrEmpty(format) || "g".Equals(format, StringComparison.OrdinalIgnoreCase))
        return ToString();

      int p = format.IndexOf('|');

      string delimiter = p >= 0
        ? format[(p + 1)..]
        : "\t";

      if (p > 0)
        format = format[0..p];

      return string.Join(Environment.NewLine, m_Items
        .Select(line => string.Join(delimiter, line.Select(item => item.ToString()))));
    }

    /// <summary>
    /// To String
    /// </summary>
    public override string ToString() => string.Join(Environment.NewLine, m_Items
        .Select(line => string.Join("\t", line.Select(item => item.ToString()))));

    #endregion IFormattable

    #region IEnumerable<BigRational>

    /// <summary>
    /// Enumerator
    /// </summary>
    /// <returns></returns>
    public IEnumerator<BigRational> GetEnumerator() => Items.GetEnumerator();

    /// <summary>
    /// Enumerator
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

    #endregion IEnumerable<BigRational>

  }

}
