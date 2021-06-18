using System.Collections.Generic;

namespace HigherArithmetics.Numerics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Lagrange Spline
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class LagrangeRationalSpline : RationalSpline {
    #region Private Data

    BigRational[] m_A;

    RationalPolynom m_Polynom;

    #endregion Private Data

    #region Algorithm

    private void CoreBuild() {
      if (m_X.Count == 0) {
        m_A = new BigRational[] { BigRational.NaN };

        m_Polynom = RationalPolynom.NaN;

        return;
      }
      else if (m_X.Count == 1) {
        m_A = new BigRational[] { m_Y[0] };

        m_Polynom = new RationalPolynom(m_A);

        return;
      }

      BigRational[][] M = new BigRational[m_Y.Count][];

      for (int r = 0; r < M.Length; ++r) {
        BigRational[] row = new BigRational[M.Length + 1];

        row[M.Length] = m_Y[r];

        BigRational x = m_X[r];
        BigRational v = 1;

        for (int i = 0; i < M.Length; ++i) {
          row[i] = v;

          v *= x;
        }

        M[r] = row;
      }

      m_A = MatrixLowLevel.Solve(M);
      m_Polynom = new RationalPolynom(m_A);
    }

    #endregion Algorithm

    #region Create

    /// <summary>
    /// Standard constructor
    /// </summary>
    public LagrangeRationalSpline(IEnumerable<(BigRational x, BigRational y)> source)
      : base(source) {

      CoreBuild();
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Compute At 
    /// </summary>
    public override BigRational At(BigRational x) {
      BigRational result = 0;
      BigRational v = 1.0;

      for (int i = 0; i < m_A.Length; ++i) {
        result += m_A[i] * v;

        v *= x;
      }

      return result;
    }

    /// <summary>
    /// Range
    /// </summary>
    public override (BigRational from, BigRational to) Range(BigRational x) =>
      (BigRational.NegativeInfinity, BigRational.PositiveInfinity);

    /// <summary>
    /// Polynom At 
    /// </summary>
    public override RationalPolynom PolynomAt(BigRational x) => m_Polynom;

    /// <summary>
    /// Derivative At
    /// </summary>
    public override BigRational DerivativeAt(BigRational x) {
      BigRational result = 0;
      BigRational v = 1.0;

      for (int i = 1; i < m_A.Length; ++i) {
        result += i * m_A[i] * v;

        v *= x;
      }

      return result;
    }

    #endregion Public
  }

}
