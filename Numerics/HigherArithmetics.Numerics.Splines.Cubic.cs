using System.Collections.Generic;

namespace HigherArithmetics.Numerics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Standard Cubic Spline  
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class CubicRationalSpline : RationalSpline {
    #region Private Data

    private BigRational[] m_A;
    private BigRational[] m_B;
    private BigRational[] m_C;
    private BigRational[] m_D;

    #endregion Private Data

    #region Algorithm

    private bool CoreBuildSpecial() {
      if (m_X.Count > 4)
        return false;

      if (m_X.Count == 0) {
        m_A = new BigRational[] { BigRational.NaN };
        m_B = new BigRational[] { 0 };
        m_C = new BigRational[] { 0 };
        m_D = new BigRational[] { 0 };
      }
      else if (m_X.Count == 1) {
        m_A = new BigRational[] { m_Y[0] };
        m_B = new BigRational[] { 0 };
        m_C = new BigRational[] { 0 };
        m_D = new BigRational[] { 0 };
      }
      else if (m_X.Count == 2) {
        m_A = new BigRational[] { (m_Y[0] * m_X[1] - m_Y[1] * m_X[0]) / (m_X[1] - m_X[0]) };
        m_B = new BigRational[] { (m_Y[1] - m_Y[0]) / (m_X[1] - m_X[0]) };
        m_C = new BigRational[] { 0 };
        m_D = new BigRational[] { 0 };
      }
      else if (m_X.Count == 3) {
        BigRational[] s = MatrixLowLevel.Solve(new BigRational[][] {
          new BigRational[] { 1, m_X[0], m_X[0] * m_X[0], m_Y[0]},
          new BigRational[] { 1, m_X[1], m_X[1] * m_X[1], m_Y[1]},
          new BigRational[] { 1, m_X[2], m_X[2] * m_X[2], m_Y[2]},
        });

        m_A = new BigRational[] { s[0] };
        m_B = new BigRational[] { s[1] };
        m_C = new BigRational[] { s[2] };
        m_D = new BigRational[] { 0 };
      }
      else if (m_X.Count == 4) {
        BigRational[] s = MatrixLowLevel.Solve(new BigRational[][] {
          new BigRational[] { 1, m_X[0], m_X[0] * m_X[0], m_X[0] * m_X[0] * m_X[0], m_Y[0]},
          new BigRational[] { 1, m_X[1], m_X[1] * m_X[1], m_X[1] * m_X[1] * m_X[1], m_Y[1]},
          new BigRational[] { 1, m_X[2], m_X[2] * m_X[2], m_X[2] * m_X[2] * m_X[2], m_Y[2]},
          new BigRational[] { 1, m_X[3], m_X[3] * m_X[3], m_X[3] * m_X[3] * m_X[3], m_Y[3]},
        });

        m_A = new BigRational[] { s[0] };
        m_B = new BigRational[] { s[1] };
        m_C = new BigRational[] { s[2] };
        m_D = new BigRational[] { s[3] };
      }

      return true;
    }

    // http://www.astro.tsu.ru/OsChMet/7_7.html
    private void CoreBuild() {
      if (CoreBuildSpecial())
        return;

      int N = m_X.Count - 1;

      m_A = new BigRational[N + 1];

      for (int i = 0; i < m_A.Length; ++i)
        m_A[i] = m_Y[i];

      BigRational[] h = new BigRational[N];

      for (int i = 0; i < N; ++i)
        h[i] = m_X[i + 1] - m_X[i];

      BigRational[] delta = new BigRational[N - 1];

      for (int i = 0; i < delta.Length; ++i)
        delta[i] = 6 * ((m_Y[i + 2] - m_Y[i + 1]) / h[i + 1] - (m_Y[i + 1] - m_Y[i]) / h[i]);

      (BigRational a, BigRational b, BigRational c)[] data = new (BigRational a, BigRational b, BigRational c)[N - 1];

      for (int i = 0; i < data.Length; ++i)
        data[i] = (h[i], 2 * (h[i] + h[i + 1]), h[i + 1]);

      TriDiagonalRationalMatrix matrix = new(data);

      BigRational[] s = matrix.Solve(delta);

      m_C = new BigRational[N + 1];

      for (int i = 0; i < s.Length; ++i)
        m_C[i + 1] = s[i];

      m_D = new BigRational[N + 1];

      for (int i = 0; i < N; ++i)
        m_D[i] = (m_C[i + 1] - m_C[i]) / h[i];

      m_B = new BigRational[N + 1];

      for (int i = 0; i < N; ++i)
        m_B[i] = (m_Y[i + 1] - m_Y[i]) / h[i] - m_C[i] * h[i] / 2 - (m_C[i + 1] - m_C[i]) * h[i] / 6;

      for (int i = 0; i < m_C.Length; ++i)
        m_C[i] /= 2;

      for (int i = 0; i < m_D.Length; ++i)
        m_D[i] /= 6;

      BigRational x = m_X[N] - m_X[N - 1];

      m_B[N] = m_B[N - 1] + 2 * m_C[N - 1] * x + 3 * m_D[N - 1] * x * x;
    }

    #endregion Algorithm

    #region Create

    /// <summary>
    /// Standard Constructor
    /// </summary>
    public CubicRationalSpline(IEnumerable<(BigRational x, BigRational y)> source)
      : base(source) {

      CoreBuild();
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Range
    /// </summary>
    public override (BigRational from, BigRational to) Range(BigRational x) {
      if (m_X.Count <= 4)
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
    /// Polynom At 
    /// </summary>
    public override RationalPolynom PolynomAt(BigRational x) {
      if (m_X.Count <= 4)
        return new RationalPolynom(new BigRational[] { m_A[0], m_B[0], m_C[0], m_D[0] });

      int index = Index(x);

      RationalPolynom poly;

      if (index < 0) {
        poly = new RationalPolynom(new BigRational[] { m_A[0], m_B[0] });

        return poly.WithShift(-m_X[0]);
      }
      else if (index >= m_X.Count - 1) {
        poly = new RationalPolynom(new BigRational[] { m_A[m_X.Count - 1], m_B[m_X.Count - 1] });

        return poly.WithShift(-m_X[^1]);
      }

      poly = new RationalPolynom(new BigRational[] { m_A[index], m_B[index], m_C[index], m_D[index] });

      return poly.WithShift(-m_X[index]);
    }

    /// <summary>
    /// Compute At 
    /// </summary>
    public override BigRational At(BigRational x) {
      if (m_X.Count <= 4)
        return m_A[0] + x * (m_B[0] + x * (m_C[0] + x * m_D[0]));

      int index = Index(x);

      if (index < 0)
        return m_A[0] + (x - m_X[0]) * (m_B[0]);
      else if (index >= m_X.Count)
        return m_A[^1] + (x - m_X[m_A.Length - 1]) * (m_B[m_A.Length - 1]);

      BigRational v = (x - m_X[index]);

      return m_A[index] + v * (m_B[index] + v * (m_C[index] + v * m_D[index]));
    }

    /// <summary>
    /// Derivative At
    /// </summary>
    public override BigRational DerivativeAt(BigRational x) {
      if (m_X.Count <= 4)
        return m_B[0] + 2 * x * m_C[0] + 3 * x * x * m_D[0];

      int index = Index(x);

      if (index < 0)
        return m_B[0];
      else if (index >= m_X.Count)
        return m_B[m_A.Length - 1];

      BigRational v = (x - m_X[index]);

      return m_B[index] + 2 * v * m_C[index] + 3 * v * v * m_D[index];
    }

    #endregion Public
  }

}
