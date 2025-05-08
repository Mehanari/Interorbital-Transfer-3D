using System;
using MehaMath.Math.Components;

namespace MehaMath.Math.RootsFinding
{
    public static class Algorithms
    {
        public static FuncVector PartialDerivatives(Func<Vector, double> func, int paramsCount, double derivationDelta = 0.0000001d)
        {
            var derivs = new Func<Vector, double>[paramsCount];
            for (int i = 0; i < paramsCount; i++)
            {
                var index = i;
                derivs[i] = (v) =>
                {
                    var perturb = new Vector(paramsCount);
                    perturb[index] += derivationDelta;
                    var input = v + perturb;
                    return (func(input) - func(v))/derivationDelta;
                };
            }

            return new FuncVector(derivs);
        }

        /// <summary>
        /// Find such a vector of input values, so that values of objective function with this input are zeros. 
        /// </summary>
        /// <param name="objective"></param>
        /// <param name="guess"></param>
        /// <param name="derivationDelta"></param>
        /// <param name="tolerance"></param>
        /// <param name="iterationsLimit"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public static Vector NewtonRaphson(FuncVector objective, Vector guess,
            double derivationDelta = 0.0000001d, double tolerance = 0.00001d, int iterationsLimit = 1000, double lambda = 1d)
        {
            var iteration = 0;
            var distance = objective.Calculate(guess).MagnitudeSquare();
            while (distance > tolerance && iteration < iterationsLimit)
            {
                iteration++;
                var J = SquareJacobian(objective, guess, derivationDelta);
                if (System.Math.Abs(J.Determinant()) < 0.00001f)
                {
                    var I = SquareMatrix.I(guess.Length); 
                    J += I * lambda; //Addition of identity matrix multiplied by lambda is needed to avoid uninversible jacobians.
                }

                var fX = objective.Calculate(guess);
                var dfXInverse = J.Inverse();
                guess = guess - fX*dfXInverse;
                distance = objective.Calculate(guess).Magnitude();
            }

            return guess;
        }

        /// <summary>
        /// Find (or tries to find, at least) zeros of a function by applying shooting method in a function gradient direction.
        /// </summary>
        /// <param name="objective"></param>
        /// <param name="initialGuess"></param>
        /// <param name="derivationDelta"></param>
        /// <param name="tolerance"></param>
        /// <param name="iterationsLimit"></param>
        /// <returns></returns>
        public static Vector GradientShooting(Func<Vector, double> objective, Vector initialGuess,
            double derivationDelta = 0.00001d, double tolerance = 0.0001d, int iterationsLimit = 1000)
        {
            var guess = initialGuess;
            var iteration = 0;
            var height = objective(guess);
            var gradientFunction = PartialDerivatives(objective, initialGuess.Length, derivationDelta);
            while (System.Math.Abs(height) > tolerance && iteration < iterationsLimit)
            {
                iteration++;
                var gradient = gradientFunction.Calculate(guess);
                var derivative = DirectionalDerivative(objective, guess, gradient, derivationDelta);
                var pivot = new Vector(guess, height);
                var direction = new Vector(gradient.Normalized(), derivative);
                var distanceToZero = height / derivative;
                var zeroPoint = pivot - direction * distanceToZero;
                guess = zeroPoint.LeftPart(guess.Length);
                height = objective(guess);
            }

            return guess;
        }

        /// <summary>
        /// Tries to find such an input vector that minimizes the objective function.
        /// May stuck in the local minima.
        /// Adjust step size and initial guess to improve performance.
        /// </summary>
        /// <param name="objective"></param>
        /// <param name="initialGuess"></param>
        /// <param name="step"></param>
        /// <param name="derivationDelta"></param>
        /// <param name="tolerance"></param>
        /// <param name="iterationsLimit"></param>
        /// <returns></returns>
        public static Vector GradientDescent(Func<Vector, double> objective, Vector initialGuess, double step = 0.1d,
            double derivationDelta = 0.00001d, double tolerance = 0.0001d, int iterationsLimit = 1000)
        {
            var improvement = double.MaxValue; //How much the new result differs from the last one.
            var guess = initialGuess;
            var height = objective(guess);
            var partialDerivatives = PartialDerivatives(objective, initialGuess.Length, derivationDelta);
            var iteration = 0;
            while (improvement > tolerance && iteration < iterationsLimit)
            {
                iteration++;
                var gradient = partialDerivatives.Calculate(guess).Normalized();
                var nextGuess = guess - gradient * step;
                var nextHeight = objective(nextGuess);
                improvement = System.Math.Abs(nextHeight - height);
                guess = nextGuess;
            }

            return guess;
        }

        public static Vector MomentumGradientDescent(Func<Vector, double> objective, Vector initialGuess,
            Vector initialVelocity, double step = 0.1d, double lambda = 0.9d, double derivationDelta = 0.00001d,
            double tolerance = 0.0001d, int iterationsLimit = 1000)
        {
            var improvement = double.MaxValue;
            var guess = initialGuess;
            var velocity = initialVelocity;
            var height = objective(guess);
            var partialDerivatives = PartialDerivatives(objective, initialGuess.Length, derivationDelta);
            var iteration = 0;
            while (improvement > tolerance && iteration < iterationsLimit)
            {
                iteration++;
                var gradient = partialDerivatives.Calculate(guess);
                var nextVelocity = velocity*lambda - gradient*step;
                var nextGuess = guess + nextVelocity;
                var nextHeight = objective(nextGuess);
                improvement = System.Math.Abs(nextHeight - height);
                guess = nextGuess;
                velocity = nextVelocity;
            }

            return guess;
        }

        /// <summary>
        /// Tries to find zero's of objective system of equations.
        /// Transforms objective func vector into single-output function and applies gradient descent to it.
        /// Mentioned single-output function returns a square magnitude of the output of objective func vector.
        /// </summary>
        /// <param name="objective"></param>
        /// <param name="initialGuess"></param>
        /// <param name="step"></param>
        /// <param name="derivationDelta"></param>
        /// <param name="tolerance"></param>
        /// <param name="iterationsLimit"></param>
        /// <returns></returns>
        public static Vector LeastSquaresGradientDescent(FuncVector objective, Vector initialGuess, double step = 0.1d,
            double derivationDelta = 0.00001d, double tolerance = 0.0001d, int iterationsLimit = 1000)
        {
            Func<Vector, double> leastSquaresObjective = (vector) =>
            {
                return objective.Calculate(vector).Magnitude();
            };
            return GradientDescent(leastSquaresObjective, initialGuess, step, derivationDelta, tolerance,
                iterationsLimit);
        }

        /// <summary>
        /// Tells you the slope of a multi-variable function f in a given direction.
        /// The lengths of pos, direction and input of f vectors must be equal. 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="pos"></param>
        /// <param name="direction"></param>
        /// <param name="derivationDelta"></param>
        /// <returns></returns>
        public static double DirectionalDerivative(Func<Vector, double> f, Vector pos, Vector direction, double derivationDelta = 0.00001d)
        {
            direction = direction.Normalized();
            var offset = direction * derivationDelta;
            var fPos = f(pos);
            var fNext = f(pos + offset);
            var derivative = (fNext - fPos) / derivationDelta;
            return derivative;
        }
        
        /// <summary>
        /// Create a Jacobian approximation for a given list of multivariable input functions.
        /// </summary>
        /// <param name="objective"></param>
        /// <param name="state"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static SquareMatrix SquareJacobian(FuncVector objective, Vector state, double delta = 0.0000001d)
        {
            if (state.Length != objective.Count)
            {
                throw new InvalidOperationException(
                    "Variables count and objectives functions count do not match. Cannot create a square Jacobian.");
            }

            var size = state.Length;
            var result = new SquareMatrix(size);
            for (int i = 0; i < size; i++)
            {
                var func = objective[i];
                var noPerturbResult = func(state);
                for (int j = 0; j < size; j++)
                {
                    var perturb = new Vector(size);
                    perturb[j] += delta;
                    var input = state + perturb;
                    result[i, j] = (func(input) - noPerturbResult) / delta;
                }
            }

            return result;
        }
    }
}
