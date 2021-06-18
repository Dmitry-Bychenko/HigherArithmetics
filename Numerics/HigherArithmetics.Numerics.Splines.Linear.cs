using System.Collections.Generic;

namespace HigherArithmetics.Numerics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Linear Spline
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class LinearRationalSpline : RationalSpline {
    #region Algorithm

    private (BigRational k, BigRational b) Line(BigRational x) {
      if (m_X.Count <= 0)
        return (BigRational.NaN, BigRational.NaN);
      else if (m_X.Count == 1)
        return (m_Y[0], 0);

      int index = Index(x);

      if (index < 0)
        index = 0;
      else if (index >= m_X.Count - 1)
        index = m_X.Count - 2;

      BigRational x0 = m_X[index];
      BigRational y0 = m_Y[index];

      BigRational x1 = m_X[index + 1];
      BigRational y1 = m_Y[index + 1];

      return ((y0 - y1) / (x0 - x1), (x1 * y0 - y1 * x0) / (x1 - x0));
    }

    #endregion Algorithm

    #region Create

    /// <summary>
    /// Standard constructor
    /// </summary>
    public LinearRationalSpline(IEnumerable<(BigRational x, BigRational y)> source)
      : base(source) { }

    #endregion Create

    #region Public

    /// <summary>
    /// Compute At 
    /// </summary>
    public override BigRational At(BigRational x) {
      var (k, b) = Line(x);

      return k * x + b;
    }

    /// <summary>
    /// Polynom At 
    /// </summary>
    public override RationalPolynom PolynomAt(BigRational x) {
      var (k, b) = Line(x);

      return new RationalPolynom(new BigRational[] { b, k });
    }

    /// <summary>
    /// Derivative At
    /// </summary>
    public override BigRational DerivativeAt(BigRational x) => Line(x).k;

    #endregion Public
  }

}
