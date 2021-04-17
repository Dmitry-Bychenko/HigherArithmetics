using System;

namespace HigherArithmetics.Numerics {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Low level algorithms on BigRational[][]
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  internal static partial class MatrixLowLevel {
    #region Algorithm

    private static BigRational[] CreateArray(int size) {
      BigRational[] result = new BigRational[size];

      for (int i = 0; i < result.Length; ++i)
        result[i] = 0;

      return result;
    }

    #endregion Algorithm

    #region Public

    // Unit square matrix
    internal static BigRational[][] Unit(int size) {
      BigRational[][] result = new BigRational[size][];

      for (int i = size - 1; i >= 0; --i)
        result[i] = CreateArray(size);

      for (int i = size - 1; i >= 0; --i)
        result[i][i] = 1;

      return result;
    }

    // Zero Square Matrix
    internal static BigRational[][] Zero(int size) {
      BigRational[][] result = new BigRational[size][];

      for (int i = size - 1; i >= 0; --i)
        result[i] = CreateArray(size);

      return result;
    }

    internal static BigRational[][] Clone(BigRational[][] source) {
      if (source is null)
        return null;

      BigRational[][] result = new BigRational[source.Length][];

      for (int i = source.Length - 1; i >= 0; --i) {
        BigRational[] src = source[i];

        if (src is null)
          continue;

        BigRational[] res = new BigRational[src.Length];

        Array.Copy(src, 0, res, 0, src.Length);

        result[i] = res;
      }

      return result;
    }

    internal static BigRational[][] Transpose(BigRational[][] value) {
      if (value is null)
        return null;

      int colCount = value.Length;
      int rowCount = value[^1].Length;

      BigRational[][] result = new BigRational[rowCount][];

      for (int i = rowCount - 1; i >= 0; --i) {
        BigRational[] line = new BigRational[colCount];
        result[i] = line;

        for (int j = colCount - 1; j >= 0; --j)
          line[j] = value[j][i];
      }

      return result;
    }

    internal static BigRational[][] Multiply(BigRational[][] left, BigRational[][] right) {
      int leftRows = left.Length;
      int leftCols = left[0].Length;

      int rightRows = right.Length;
      int rightCols = right[0].Length;

      if (leftCols != rightRows)
        throw new ArgumentOutOfRangeException(nameof(right));

      BigRational[][] result = new BigRational[leftRows][];

      for (int r = leftRows - 1; r >= 0; --r) {
        BigRational[] leftLine = left[r];
        BigRational[] line = new BigRational[rightCols];

        result[r] = line;

        for (int c = rightCols - 1; c >= 0; --c) {
          BigRational s = 0;

          for (int i = leftCols - 1; i >= 0; --i)
            s += leftLine[i] * right[i][c];

          line[c] = s;
        }
      }

      return result;
    }

    internal static BigRational Determinant(BigRational[][] value) {
      BigRational[][] m = Clone(value);

      BigRational result = 1;
      int size = m.Length;

      for (int i = 0; i < size; ++i) {
        BigRational[] line = m[i];

        // Find out a line
        if (line[i] == 0) {
          bool found = false;

          for (int j = i + 1; j < size; ++j) {
            BigRational[] newLine = m[j];

            if (newLine[i] != 0) {
              found = true;

              m[i] = newLine;
              m[j] = line;

              result = -result;  // < -1 each time!

              line = m[i];

              break;
            }
          }

          if (!found)
            return 0;
        }

        // Elimination
        BigRational mm = line[i];

        for (int j = i + 1; j < size; ++j) {
          BigRational[] curLine = m[j];
          BigRational coef = -curLine[i] / mm;

          for (int k = i + 1; k < size; ++k)
            curLine[k] += coef * line[k];
        }
      }

      // Backward
      for (int i = size - 1; i >= 0; --i)
        result *= m[i][i];

      return result;
    }

    internal static int Rank(BigRational[][] value) {
      if (value is null)
        return 0;

      BigRational[][] m = Clone(value);

      int size = m.Length;
      int sizeC = m[0].Length;

      for (int i = 0; i < size; ++i) {
        if (i >= sizeC)
          break;

        BigRational[] line = m[i];

        // Find out a line
        if (line[i] == 0) {
          Boolean found = false;

          for (int j = i + 1; j < size; ++j) {
            BigRational[] newLine = m[j];

            if (newLine[i] != 0) {
              found = true;

              m[i] = newLine;
              m[j] = line;

              line = m[i];

              break;
            }
          }

          if (!found)
            continue;
        }

        // Elimination
        BigRational mm = line[i];

        for (int j = i + 1; j < size; ++j) {
          BigRational[] curLine = m[j];
          BigRational coef = -curLine[i] / mm;

          for (int k = i; k >= 0; --k)
            curLine[k] = 0;

          for (int k = i + 1; k < sizeC; ++k)
            curLine[k] += coef * line[k];
        }
      }

      int result = 0;

      for (int i = 0; i < size; ++i) {
        BigRational[] line = m[i];
        Boolean found = false;

        for (int j = line.Length - 1; j >= 0; --j)
          if (line[j] != 0) {
            result += 1;
            found = true;

            break;
          }

        if (found)
          continue;

        return result;
      }

      return result;
    }

    internal static BigRational[][] Inverse(BigRational[][] value) {
      BigRational[][] m = Clone(value);
      int size = m.Length;
      BigRational result = 1;

      BigRational[][] inverted = Unit(size);

      for (int i = 0; i < size; ++i) {
        BigRational[] line = m[i];
        BigRational[] lineInverted = inverted[i];

        // Find out a line
        if (line[i] == 0) {
          Boolean found = false;

          for (int j = i + 1; j < size; ++j) {
            BigRational[] newLine = m[j];

            if (newLine[i] != 0) {
              found = true;

              m[i] = newLine;
              m[j] = line;

              BigRational[] h = inverted[i];
              inverted[i] = inverted[j];
              inverted[j] = h;

              result = -result;  // < -1 each time!

              line = m[i];
              lineInverted = inverted[i];

              break;
            }
          }

          if (!found)
            throw new ArgumentException("Matrix is degenerated and can't be inverted.", nameof(value));
        }

        // Elimination
        BigRational mm = line[i];

        for (int j = 0; j < size; ++j) {
          line[j] /= mm;
          lineInverted[j] /= mm;
        }

        for (int j = i + 1; j < size; ++j) {
          BigRational[] curLine = m[j];
          BigRational[] curLineInverted = inverted[j];

          BigRational coef = -curLine[i];

          for (int k = 0; k < size; ++k) {
            curLine[k] += coef * line[k];
            curLineInverted[k] += coef * lineInverted[k];
          }
        }
      }

      // Backward
      for (int r = size - 1; r >= 1; --r) {
        for (int i = r - 1; i >= 0; --i) {
          BigRational coef = -m[i][r];

          for (int j = size - 1; j >= 0; --j) {
            //m[i][j] = m[i][j] + coef * m[r][j];
            inverted[i][j] = inverted[i][j] + coef * inverted[r][j];
          }
        }
      }

      return inverted;
    }

    internal static BigRational[] Solve(BigRational[][] value) {
      if (value is null)
        return Array.Empty<BigRational>();
      else if (value.Length <= 0)
        return Array.Empty<BigRational>();
      else if ((value.Length + 1) != value[0].Length)
        return Array.Empty<BigRational>();

      BigRational[][] m = Clone(value);

      int size = m.Length;
      int sizeC = size + 1;

      for (int i = 0; i < size; ++i) {
        BigRational[] line = m[i];

        // Find out a line
        if (line[i] == 0) {
          Boolean found = false;

          for (int j = i + 1; j < size; ++j) {
            BigRational[] newLine = m[j];

            if (newLine[i] != 0) {
              found = true;

              m[i] = newLine;
              m[j] = line;

              line = m[i];

              break;
            }
          }

          if (!found)
            return Array.Empty<BigRational>();
        }

        // Elimination
        BigRational mm = line[i];

        for (int j = i + 1; j < size; ++j) {
          BigRational[] curLine = m[j];
          BigRational coef = -curLine[i] / mm;

          for (int k = i + 1; k < sizeC; ++k)
            curLine[k] += coef * line[k];
        }
      }

      BigRational[] result = new BigRational[size];

      // Backward 
      for (int i = size - 1; i >= 0; --i) {
        BigRational[] line = m[i];

        BigRational s = line[sizeC - 1];

        for (int j = i + 1; j < size; ++j)
          s -= result[j] * line[j];

        result[i] = s / line[i];
      }

      return result;
    }

    internal static Boolean IsPositiveDefined(BigRational[][] value) {
      BigRational[][] m = Clone(value);

      BigRational result = 1;
      int size = m.Length;

      for (int i = 0; i < size; ++i) {
        BigRational[] line = m[i];

        // Find out a line
        if (line[i] == 0) {
          Boolean found = false;

          for (int j = i + 1; j < size; ++j) {
            BigRational[] newLine = m[j];

            if (newLine[i] != 0) {
              found = true;

              m[i] = newLine;
              m[j] = line;

              result = -result;  // < -1 each time!

              line = m[i];

              break;
            }
          }

          if (!found)
            return false;
        }

        // Elimination
        BigRational mm = line[i];

        for (int j = i + 1; j < size; ++j) {
          BigRational[] curLine = m[j];
          BigRational coef = -curLine[i] / mm;

          for (int k = i + 1; k < size; ++k)
            curLine[k] += coef * line[k];
        }
      }

      // Backward
      for (int i = size - 1; i >= 0; --i)
        if (result * m[i][i] <= 0)
          return false;

      return true;
    }

    internal static Boolean IsNegativeDefined(BigRational[][] value) {
      BigRational[][] m = Clone(value);

      BigRational result = 1;
      int size = m.Length;

      for (int i = 0; i < size; ++i) {
        BigRational[] line = m[i];

        // Find out a line
        if (line[i] == 0) {
          Boolean found = false;

          for (int j = i + 1; j < size; ++j) {
            BigRational[] newLine = m[j];

            if (newLine[i] != 0) {
              found = true;

              m[i] = newLine;
              m[j] = line;

              result = -result;  // < -1 each time!

              line = m[i];

              break;
            }
          }

          if (!found)
            return false;
        }

        // Elimination
        BigRational mm = line[i];

        for (int j = i + 1; j < size; ++j) {
          BigRational[] curLine = m[j];
          BigRational coef = -curLine[i] / mm;

          for (int k = i + 1; k < size; ++k)
            curLine[k] += coef * line[k];
        }
      }

      // Backward
      for (int i = size - 1; i >= 0; --i) {
        BigRational v = result * m[i][i] * (-1 + 2 * (i % 2));

        if (v <= 0)
          return false;
      }

      return true;
    }

    #endregion Public
  }

}
