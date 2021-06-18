using System;
using System.Collections.Generic;
using System.Linq;

namespace HigherArithmetics.Numerics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Abstract Interpolation Spline 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public abstract class RationalSpline {
    #region Private Data

    protected readonly List<BigRational> m_X = new();
    protected readonly List<BigRational> m_Y = new();

    #endregion Private Data

    #region Create

    /// <summary>
    /// Standard Constructor
    /// </summary>
    public RationalSpline(IEnumerable<(BigRational x, BigRational y)> source) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));

      foreach (var (x, y) in source.OrderBy(p => p.x)) {
        m_X.Add(x);
        m_Y.Add(y);
      }
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Count
    /// </summary>
    public int Count => m_X.Count;

    /// <summary>
    /// X
    /// </summary>
    public IReadOnlyList<BigRational> X => m_X;

    /// <summary>
    /// Y
    /// </summary>
    public IReadOnlyList<BigRational> Y => m_Y;

    /// <summary>
    /// Index [result..result + 1]
    /// </summary>
    public int Index(BigRational x) {
      int result = m_X.BinarySearch(x);

      if (result < 0)
        result = ~result - 1;

      return result;
    }

    /// <summary>
    /// Range
    /// </summary>
    public virtual (BigRational from, BigRational to) Range(BigRational x) {
      if (m_X.Count <= 1)
        return (BigRational.NegativeInfinity, BigRational.PositiveInfinity);

      int index = Index(x);

      if (index < 0)
        return (BigRational.NegativeInfinity, m_X[index]);
      else if (index >= m_X.Count - 1)
        return (m_X[index], BigRational.PositiveInfinity);
      else
        return (m_X[index], m_X[index + 1]);
    }

    /// <summary>
    /// Compute At 
    /// </summary>
    public abstract BigRational At(BigRational x);

    /// <summary>
    /// Polynom At 
    /// </summary>
    public abstract RationalPolynom PolynomAt(BigRational x);

    /// <summary>
    /// Derivative At
    /// </summary>
    public abstract BigRational DerivativeAt(BigRational x);

    #endregion Public
  }

}
