using System;
using System.Text;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace MehaMath.Math.Components
{
    /// <summary>
    /// A vector of values.
    /// I need it to perform more flexible operations, like matrix multiplication.
    /// Use an appropriate constructor variant if you want to make a copy of a vector and not mess up the inner values array.
    /// </summary>
    public readonly struct Vector
    {
        public int Length => _values.Length;

        private readonly double[] _values;

        public Vector(int length)
        {
            _values = new double[length];
        }
        
        /// <summary>
        /// Creates a single-value vector.
        /// </summary>
        /// <param name="val"></param>
        public Vector(double val)
        {
            _values = new double [] { val };
        }

        /// <summary>
        /// Creates a deep copy of a vector.
        /// </summary>
        /// <param name="vector"></param>
        public Vector(Vector vector)
        {
            _values = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                _values[i] = vector[i];
            }
        }

        /// <summary>
        /// Creates a vector with a COPY of values array.
        /// </summary>
        /// <param name="values"></param>
        public Vector(params double[] values)
        {
            _values = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                _values[i] = values[i];
            }
        }

        public Vector(Vector2 vector2)
        {
            _values = new double[2];
            _values[0] = vector2.x;
            _values[1] = vector2.y;
        }

        public Vector(Vector3 vector3)
        {
            _values = new double[3];
            _values[0] = vector3.x;
            _values[1] = vector3.y;
            _values[2] = vector3.z;
        }

        /// <summary>
        /// Creates a new vector of lenght equal to baseVector.Length + 1.
        /// Puts values of baseVector in the beginning and value of tail to the end. 
        /// </summary>
        /// <param name="baseVector"></param>
        /// <param name="tail"></param>
        public Vector(Vector baseVector, double tail)
        {
            _values = new double[baseVector.Length + 1];
            for (int i = 0; i < baseVector.Length; i++)
            {
                _values[i] = baseVector[i];
            }

            _values[^1] = tail;
        }

        public double this[int i]
        {
            get => _values[i];
            set => _values[i] = value;
        }

        /// <summary>
        /// Converts this vector to a Unity Vector2.
        /// If length of this vector is bigger than 2, then remaining values will be lost.
        /// If length of this vector is less than 2, then missing values will be set to zero.
        /// </summary>
        /// <returns></returns>
        public Vector2 ToVector2()
        {
            return ToVector3();
        }

        public Vector3 ToVector3()
        {
            var coordinates = new float[]{0, 0, 0};
            for (int i = 0; i < coordinates.Length; i++)
            {
                if (i >= _values.Length)
                {
                    break;
                }

                coordinates[i] = (float)_values[i];
            }

            return new Vector3(coordinates[0], coordinates[1], coordinates[2]);
        }

        public Vector LeftPart(int length)
        {
            if (length > Length)
            {
                throw new InvalidOperationException("Cannot take left part of length " + length +
                                                    "from a vector of length " + Length);
            }

            var result = new Vector(length);
            for (int i = 0; i < length; i++)
            {
                result[i] = _values[i];
            }

            return result;
        }

        public static Vector CreateSameValueVector(int length, double value)
        {
            var vector = new Vector(length);
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = value;
            }

            return vector;
        }

        public Vector Normalized()
        {
            return this / Magnitude();
        }

        public static Vector CrossProduct3D(Vector a, Vector b)
        {
            if ( a.Length != 3 || b.Length != 3 )
            {
                throw new InvalidOperationException("CrossProduct3D is defined only for 3-dimensional vectors.");
            }

            return new Vector(a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0]);
        }

        public static double DotProduct(Vector a, Vector b)
        {
            if (a.Length != b.Length)
            {
                throw new InvalidOperationException("For dot product vectors must have equal length.");
            }

            var result = 0d;
            for (int i = 0; i < a.Length; i++)
            {
                result += a[i] * b[i];
            }

            return result;
        }

        public double MagnitudeSquare()
        {
            var sum = 0d;
            foreach (var val in _values)
            {
                sum += val * val;
            }

            return sum;
        }

        public double Magnitude()
        {
            return System.Math.Sqrt(MagnitudeSquare());
        }

        /// <summary>
        /// Returns a vector with length equal to sum of lengths of given vector.
        /// The order of values is determined by order of input vectors and values inside them.
        /// </summary>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public static Vector Combine(params Vector[] vectors)
        {
            var totalLength = 0;
            foreach (var vec in vectors)
            {
                totalLength += vec.Length;
            }

            var result = new Vector(totalLength);
            var i = 0;
            foreach (var vec in vectors)
            {
                foreach (var value in vec._values)
                {
                    result[i] = value;
                    i++;
                }
            }

            return result;
        } 

        public static Vector operator *(Vector vector, SquareMatrix matrix)
        {
            if (vector.Length != matrix.Size)
            {
                throw new InvalidOperationException(
                    "Trying to multiply vector and square matrix with different sizes!");
            }

            var result = new Vector(matrix.Size);
            for (int i = 0; i < matrix.Size; i++)
            {
                for (int j = 0; j < matrix.Size; j++)
                {
                    result[i] += vector[j] * matrix[i, j];
                }
            }

            return result;
        }

        public static Vector operator -(Vector a, Vector b)
        {
            if (a.Length != b.Length)
            {
                throw new InvalidOperationException("Cannot subtract vectors of different length.");
            }

            var result = new Vector(a.Length);
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] - b[i];
            }

            return result;
        }
        
        public static Vector operator + (Vector a, Vector b)
        {
            if (a.Length != b.Length)
            {
                throw new InvalidOperationException("Cannot add vectors of different length.");
            }

            var result = new Vector(a.Length);
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] + b[i];
            }

            return result;
        }

        public static Vector operator *(Vector vector, double num)
        {
            var result = new Vector(vector.Length);
            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = vector[i] * num;
            }

            return result;
        }

        public static Vector operator /(Vector vector, double num)
        {
            return vector * (1 / num);
        }
        

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("( ");
            for (int i = 0; i < _values.Length; i++)
            {
                builder.Append(_values[i]);
                if (i == _values.Length - 1)
                {
                    builder.Append(" )");
                }
                else
                {
                    builder.Append(", ");
                }
            }

            return builder.ToString();
        }

        public bool Equals(Vector other)
        {
            if (other.Length != Length)
            {
                return false;
            }

            for (int i = 0; i < Length; i++)
            {
                if (_values[i] != other._values[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (_values != null ? _values.GetHashCode() : 0);
        }
    }
}