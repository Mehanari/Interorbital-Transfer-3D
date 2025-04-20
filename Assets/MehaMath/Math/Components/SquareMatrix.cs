using System;
using System.Text;

namespace MehaMath.Math.Components
{
    public readonly struct SquareMatrix
    {
        public int Size { get; }

        private readonly double[,] _values;
        
        public SquareMatrix(int size)
        {
            Size = size;
            _values = new double[size, size];
        }

        public SquareMatrix(double[,] doubleMatrix)
        {
            if (doubleMatrix.GetLength(0) != doubleMatrix.GetLength(1))
            {
                throw new InvalidOperationException(
                    "Cannot create SquareMatrix from two-dimensional array with different length and height");
            }

            Size = doubleMatrix.GetLength(0);
            _values = new double[Size, Size];
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    _values[i, j] = doubleMatrix[i, j];
                }
            }
        }

        public double this[int i, int j]
        {
            get => _values[i, j];
            set => _values[i, j] = value;
        }

        public SquareMatrix Inverse()
        {
            var determinant = Determinant();
            var adjoint = Adjoint();
            return adjoint / determinant;
        }
        
        public SquareMatrix Adjoint()
        {
            var cofactor = CofactorMatrix();
            return cofactor.Transpose();
        }

        public SquareMatrix Transpose()
        {
            var newMatrix = new SquareMatrix(Size);
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    newMatrix[j, i] = _values[i, j];
                }
            }

            return newMatrix;
        }
        
        public SquareMatrix CofactorMatrix()
        {
            var result = new SquareMatrix(Size);
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    result[i, j] = Cofactor(i, j);
                }
            }

            return result;
        }

        public double Cofactor(int i, int j)
        {
            if (i+1 > Size || j + 1 > Size)
            {
                throw new InvalidOperationException(
                    "Cannot find cofactor. Indices are out of the matrix boundaries.");
            }

            var minor = Minor(i, j);
            var sign = (i + j) % 2 == 0 ? 1 : -1;
            return sign * minor;
        }

        public double Minor(int i, int j)
        {
            return SubMatrix(i, j).Determinant();
        }

        public double Determinant()
        {
            if (Size == 1)
            {
                return _values[0, 0];
            }
            if (Size == 2)
            {
                return _values[0, 0] * _values[1, 1] - _values[0, 1] * _values[1, 0];
            }

            var determinant = 0d;
            var sign = 1;
            for (int i = 0; i < Size; i++)
            {
                var subDeterminant = _values[0, i] * SubMatrix(0, i).Determinant();
                determinant += sign * subDeterminant;
                sign = -sign;
            }

            return determinant;
        }
        

        /// <summary>
        /// Returns a matrix without i-th row and j-th column.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public SquareMatrix SubMatrix(int i, int j)
        {
            if (i + 1 > Size || j + 1 > Size)
            {
                throw new InvalidOperationException(
                    "Cannot find sub matrix. Indices are out of the matrix boundaries.");
            }

            if (Size == 1)
            {
                return I(1);
            }

            var sub = new SquareMatrix(Size - 1);
            var row = 0;
            for (int k = 0; k < Size; k++)
            {
                var col = 0;
                if(k == i) continue;
                for (int l = 0; l < Size; l++)
                {
                    if(l == j) continue;
                    sub[row, col] = _values[k, l];
                    col++;
                }

                row++;
            }

            return sub;
        }

        public static bool operator ==(SquareMatrix a, SquareMatrix b)
        {
            if (a.Size != b.Size)
            {
                return false;
            }

            for (int i = 0; i < a.Size; i++)
            {
                for (int j = 0; j < b.Size; j++)
                {
                    if (a[i, j] != b[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        
        public static bool operator !=(SquareMatrix a, SquareMatrix b)
        {
            return !(a == b);
        }

        public static SquareMatrix operator *(SquareMatrix matrix, double number)
        {
            var result = new SquareMatrix(matrix.Size);
            for (int i = 0; i < matrix.Size; i++)
            {
                for (int j = 0; j < matrix.Size; j++)
                {
                    result[i, j] = matrix[i, j] * number;
                }
            }

            return result;
        }

        public static SquareMatrix operator /(SquareMatrix matrix, double number)
        {
            return matrix * (1 / number);
        }

        public static SquareMatrix operator +(SquareMatrix a, SquareMatrix b)
        {
            if (a.Size != b.Size)
            {
                throw new InvalidOperationException(
                    "Cannot add matrices of different sizes.");
            }

            var result = new SquareMatrix(a.Size);
            for (int i = 0; i < a.Size; i++)
            {
                for (int j = 0; j < a.Size; j++)
                {
                    result[i, j] = a[i, j] + b[i, j];
                }
            }

            return result;
        }

        public static SquareMatrix operator *(SquareMatrix a, SquareMatrix b)
        {
            if (a.Size != b.Size)
            {
                throw new InvalidOperationException("Cannot multiply two square matrices of different size.");
            }

            var result = new SquareMatrix(a.Size);
            for (int i = 0; i < a.Size; i++)
            {
                for (int j = 0; j < a.Size; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < a.Size; k++)
                    {
                        result[i, j] += a[i, k] * b[k, j];
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the identity matrix of a given size
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static SquareMatrix I(int size)
        {
            var result = new SquareMatrix(size);
            for (int i = 0; i < size; i++)
            {
                result[i, i] = 1;
            }

            return result;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    builder.Append(_values[i, j] + " ");
                }

                builder.Append("\n");
            }
            return builder.ToString();
        }
    }
}