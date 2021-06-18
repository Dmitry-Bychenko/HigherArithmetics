using System.Collections.Generic;

namespace HigherArithmetics.Numerics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Akima Spline (cubic) 
  /// </summary>
  /// <see cref="https://en.wikipedia.org/wiki/Akima_spline"/>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class AkimaRationalSpline : RationalSpline {
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

    // http://web.snauka.ru/issues/2015/05/53846
    // https://en.wikipedia.org/wiki/Akima_spline
    private void CoreBuild() {
      if (CoreBuildSpecial())
        return;

      int N = m_X.Count - 1;

      Dictionary<int, BigRational> m = new();

      for (int i = 0; i < N; ++i)
        m.Add(i, (m_Y[i + 1] - m_Y[i]) / (m_X[i + 1] - m_X[i]));

      m.Add(-2, 3 * m[0] - 2 * m[1]);
      m.Add(-1, 2 * m[0] - 2 * m[1]);
      m.Add(N, 2 * m[N - 1] - 2 * m[N - 2]);
      m.Add(N + 1, 3 * m[N - 1] - 2 * m[N - 2]);

      BigRational[] s = new BigRational[N + 1];

      for (int i = 0; i <= N; ++i)
        s[i] = ((m[i + 1] - m[i]).Abs() * m[i - 1] +
                (m[i - 1] - m[i - 2]).Abs() * m[i]) /
                ((m[i + 1] - m[i]).Abs() + (m[i - 1] - m[i - 2]).Abs());

      for (int i = 0; i < N; ++i) {
        BigRational[][] mt = new BigRational[][] {
          new BigRational[] { 1, m_X[i], m_X[i] * m_X[i], m_X[i] * m_X[i] * m_X[i], m_Y[i]},
          new BigRational[] { 1, m_X[i + 1], m_X[i + 1] * m_X[i + 1], m_X[i + 1] * m_X[i + 1] * m_X[i + 1], m_Y[i + 1]},
          new BigRational[] { 0, 1, 2 * m_X[i], 3 * m_X[i] * m_X[i], s[i]},
          new BigRational[] { 0, 1, 2 * m_X[i + 1], 3 * m_X[i + 1] * m_X[i + 1], s[i + 1]},
        };

        BigRational[] z = MatrixLowLevel.Solve(mt);

        m_A[i] = z[0];
        m_B[i] = z[1];
        m_C[i] = z[2];
        m_D[i] = z[3];
      }
    }

    #endregion Algorithm

    #region Create

    /// <summary>
    /// Standard Constructor
    /// </summary>
    public AkimaRationalSpline(IEnumerable<(BigRational x, BigRational y)> source)
      : base(source) {

      m_A = new BigRational[m_Y.Count];
      m_B = new BigRational[m_Y.Count];
      m_C = new BigRational[m_Y.Count];
      m_D = new BigRational[m_Y.Count];

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
    /// Compute At 
    /// </summary>
    public override BigRational At(BigRational x) {
      if (m_X.Count <= 4)
        return m_A[0] + x * (m_B[0] + x * (m_C[0] + x * m_D[0]));

      int index = Index(x);

      if (index < 0)
        index = 0;
      else if (index >= m_X.Count - 1)
        index = m_X.Count - 2;

      return m_A[index] + x * (m_B[index] + x * (m_C[index] + x * m_D[index]));
    }

    /// <summary>
    /// Polynom At 
    /// </summary>
    public override RationalPolynom PolynomAt(BigRational x) {
      if (m_X.Count <= 4)
        return new RationalPolynom(new BigRational[] { m_A[0], m_B[0], m_C[0], m_D[0] });

      int index = Index(x);

      if (index < 0)
        index = 0;
      else if (index >= m_X.Count - 1)
        index = m_X.Count - 2;

      return new RationalPolynom(new BigRational[] { m_A[index], m_B[index], m_C[index], m_D[index] });
    }

    /// <summary>
    /// Derivative At
    /// </summary>
    public override BigRational DerivativeAt(BigRational x) {
      if (m_X.Count <= 4)
        return m_B[0] + 2 * x * m_C[0] + 3 * x * x * m_D[0];

      int index = Index(x);

      if (index < 0)
        index = 0;
      else if (index >= m_X.Count - 1)
        index = m_X.Count - 2;

      return m_B[index] + 2 * x * m_C[index] + 3 * x * x * m_D[index];
    }

    #endregion Public
  }

}
