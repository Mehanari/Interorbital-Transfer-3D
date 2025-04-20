using System;
using System.Text;

namespace MehaMath.Math.Components
{
    public class Polynomial
    {
        public Vector Coefficients => _coefficients;
        
        private readonly Vector _coefficients;

        public Polynomial(Vector coefficients)
        {
            _coefficients = new Vector(coefficients);
        }

        public Polynomial(params double[] coefficients)
        {
            _coefficients = new Vector(coefficients);
        }

        public double this[int i]
        {
            get => _coefficients[i];
            set => _coefficients[i] = value;
        }
        

        public double Compute(double x)
        {
            var sum = _coefficients[0];
            for (int i = 1; i < _coefficients.Length; i++)
            {
                sum += _coefficients[i] * System.Math.Pow(x, i);
            }

            return sum;
        }

        /// <summary>
        /// Computes the "area" under the polynomial curve in the integration region [a, b].
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public double ComputeIntegral(double a, double b)
        {
            var integral = Integral(0);
            return integral.Compute(b) - integral.Compute(a);
        }

        public Polynomial Derivative()
        {
            if (_coefficients.Length == 1)
            {
                return new Polynomial(new Vector(0d));
            }
            var newCoefficients = new Vector(_coefficients.Length - 1);
            for (int i = 1; i < _coefficients.Length; i++)
            {
                newCoefficients[i-1] = _coefficients[i] * i;
            }

            return new Polynomial(newCoefficients);
        }

        public Polynomial Integral(double constant)
        {
            var newCoefficients = new Vector(_coefficients.Length + 1);
            newCoefficients[0] = constant;
            for (int i = 1; i < newCoefficients.Length; i++)
            {
                newCoefficients[i] = _coefficients[i - 1] / i;
            }

            return new Polynomial(newCoefficients);
        }

        public Func<double, double> ToDoubleFunc()
        {
            return Compute;
        }

        public Func<float, float> ToFloatFunc()
        {
            return (x) => (float)Compute(x);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var writtenMembers = 0;
            for (int i = 0; i < _coefficients.Length; i++)
            {
                var coef = _coefficients[i];
                if (coef == 0)
                {
                    continue;
                }

                if (writtenMembers > 0)
                {
                    if (_coefficients[i] > 0)
                    {
                        builder.Append(" + ");
                    }

                    if (_coefficients[i] < 0)
                    {
                        builder.Append(" - ");
                        coef = -coef;
                    }
                }
                builder.Append(coef + " * (x^" + i + ")");
                writtenMembers++;
            }
            return builder.ToString();
        }
    }
}
