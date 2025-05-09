using System;
using System.Net.NetworkInformation;
using MehaMath.Math.Components;
using UnityEngine;

namespace Src.LambertProblem
{
    /// <summary>
    /// The algorithm is based on article "A procedure for the solution of Lambert's orbital boundary-value problem" by Gooding R. H.
    /// </summary>
    public class GoodingSolver : MonoBehaviour
    {
        private const double TOLERANCE = 1e-11;
        private const int MAX_ITERATIONS = 30;
        private const double MAX_X_MODULE = 0.99999;

        public double GravitationalParameter { get; set; }
        public double CentralBodyRadius { get; set; }
        public bool LongPath { get; set; }

        /// <summary>
        /// Zero revolution means we perform our transfer within one revolution.
        /// If Revolutions > 0, then found transfer orbit will perform this amount of revolutions before reaching the destination.
        /// </summary>
        public int Revolutions { get; set; } = 0;

        /// <summary>
        /// Calculates the transfer orbit connecting start to destination in transferTime.
        /// Returns true if a solution is found, false otherwise.
        /// Outputs velocity vectors v1 (at start) and v2 (at destination).
        /// </summary>
        public bool CalculateTransfer(Vector start, Vector destination, double transferTime, out Vector v1,
            out Vector v2)
        {
            v1 = new Vector(3);
            v2 = new Vector(3);
            if (transferTime <= 0 || start.Equals(destination))
                return false;

            var r1 = start.Magnitude();
            var r2 = destination.Magnitude();

            if (r1 < CentralBodyRadius || r2 < CentralBodyRadius)
            {
                //We don't want to fly into our planet.
                return false;
            }

            if (GravitationalParameter < 0)
            {
                return false;
            }

            var chord = (destination - start).Magnitude();
            var s = (r1 + r2 + chord) /
                    2; //Semi-perimeter of the triangle ABC where A is the central body center, B is start and C is destination.
            var cosTheta = Vector.DotProduct(start, destination) / (r1 * r2);
            cosTheta = Math.Max(-1, Math.Min(1, cosTheta));
            var theta = Math.Acos(cosTheta) +
                        Math.PI * 2 * Revolutions; //Angular separation between start and destination
            if (LongPath)
            {
                theta = Math.PI * 2 - theta + Math.PI * 2 * Revolutions;
            }

            //Converting to non-dimensional units to improve numerical stability.
            //Suggested by Gooding in his article.
            //We will use magnitude of initial position (r1) as the length unit.
            //_n in the end of variable name means non-dimensional
            var r1_n = 1d;
            var r2_n = r2 / r1;
            var chord_n = chord / r1;
            var s_n = s / r1;
            var time_n = transferTime / Math.Sqrt(r1 * r1 * r1 / GravitationalParameter);
            var lambda = Math.Sqrt(r2_n / s_n) * Math.Cos(theta / 2);
            var aMin = s / 2; //Semi-major axis of minimum-energy orbit
            var aMin_n = aMin / r1;

            //Making initial guesses for x (universal variable)
            //Magic numbers come from Gooding's calculations and correspond to orbits with different energy deviations
            //x1 - elliptical orbit (negative energy deviation)
            //x2 - hyperbolic orbit (positive energy deviation)
            //So, x1 and x2 cover a wide spectrum of possible transfer orbits.
            //z1 and z2 are just transformed universal variables
            var z1 = Math.Log(0.4767);
            var z2 = Math.Log(1.5233);
            var x1 = Math.Exp(z1) - 1;
            var x2 = Math.Exp(z2) - 1;

            //y1 and y2 are logarithmic errors between the computed time of flight and desired time in non-dimensional forms.
            var y1 = Math.Log(TimeOfFlight(x1, s_n, chord_n, lambda)) - Math.Log(time_n);
            var y2 = Math.Log(TimeOfFlight(x2, s_n, chord_n, lambda)) - Math.Log(time_n);

            //Intermediate Value Theorem: if y1 and y2 have opposite signs, a root exists between x1 and x2.
            if (y1 * y2 > 0)
            {
                //If this happened, then Halley's method may converge into an incorrect root.
                Debug.LogWarning("Initial guess may not bracket the solution.");
            }

            //Helley iterations
            var x = x1;
            var y = y1;
            var x_new = x;
            var iter = 0;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Computes the non-dimensional time of flight for a given x.
        /// Implemented using universal variables and Stumpff functions.
        /// _n in the end of parameters and variables means "non-dimensional"
        /// </summary>
        private double TimeOfFlight(double x, double s_n, double chord_n, double lambda)
        {
            if (Math.Abs(x) > MAX_X_MODULE) // Near-parabolic case
                return double.PositiveInfinity;

            var a_n = An(x, s_n); //Normalized seme-major axis corresponding to energy x.
            if (a_n < 0)
                return double.PositiveInfinity;

            var T = this.T(x, s_n, chord_n, lambda);
            var Trev = GoodingSolver.Trev(x, s_n, Revolutions);
            var ttof = T / (1 - Math.Pow(x, 2)); 
            ttof += Trev;
            return ttof > 0 ? ttof : double.PositiveInfinity;
        }

        /// <summary>
        /// Calculates the first derivative of the time of flight with respect to x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="chord_n"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        private double TimeOfFlightPrimeI(double x, double s_n, double chord_n, double lambda)
        {
            if (Math.Abs(x) > MAX_X_MODULE) // Near-parabolic case
                return double.PositiveInfinity;
            
            var T = this.T(x, s_n, chord_n, lambda);
            var dT = TPrimeI(x, s_n, lambda);
            var dTrev = TrevPrimeI(x, s_n, Revolutions);
            var numerator = dT * (1 - x * x) + 2 * x * T;
            var denominator = Math.Pow((1 - x * x), 2);
            return (numerator / denominator) + dTrev;
        }

        #region Helper functions

        /// <summary>
        /// Calculates the first derivative of the T component with respect to x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        private double TPrimeI(double x, double s_n, double lambda)
        {
            var q = lambda / Math.Sqrt(s_n);
            var du = UPrimeI(x, q);
            var z = Z(x, s_n, q);
            var dz = ZPrimeI(x, s_n, q);
            var dT_1 = 2 * q * Math.Sqrt(s_n) * du;
            var dT_2 = 4 * q * q * s_n * dz * StumpffFunctions.SPrimeI(z);
            var dT_3 = T3PrimeI(x, s_n, lambda);
            return dT_1 + dT_2 + dT_3;
        }

        /// <summary>
        /// Calculates the T component of the normalized time of flight.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="chord_n"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        private double T(double x, double s_n, double chord_n, double lambda)
        {
            var q = lambda / Math.Sqrt(s_n);
            var u = U(x, q);
            var z = Z(x, s_n, q);

            var S_z = StumpffFunctions.S(z);
            var T_0 = s_n - chord_n;
            var T_1 = 2 * q * Math.Sqrt(s_n) * u;
            var T_2 = 4 * Math.Pow(q, 2) * s_n * S_z;
            var T_3 = Math.Sqrt(s_n) * (1 - Math.Pow(x, 2)) * u * (1 - z * S_z);
            return T_0 + T_1 + T_2 + T_3;
        }

        /// <summary>
        /// Calculates the first derivative of the third intermediate term of the normalized time of flight.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        private static double T3PrimeI(double x, double s_n, double lambda)
        {
            var q = lambda / Math.Sqrt(s_n);
            var u = U(x, q);
            var du = UPrimeI(x, q);
            var z = Z(x, s_n, q);
            var dz = ZPrimeI(x, s_n, q);
            var Sz = StumpffFunctions.S(z);
            var dSz = StumpffFunctions.SPrimeI(z);
            var multiplier = Math.Sqrt(s_n);
            var firstTerm = (x * x - 1) * u * dz * (z * dSz + Sz);
            var secondTerm = (x * x - 1) * du * (z * Sz - 1);
            var thirdTerm = 2 * x * u * (z * Sz - 1);
            return multiplier * (firstTerm + secondTerm + thirdTerm);
        }

        /// <summary>
        /// Calculates the normalized time of multiple revolutions, if there is any.
        /// N is a number of revolutions.
        /// </summary>
        /// <returns></returns>
        private static double Trev(double x, double s_n, int N)
        {
            var a_n = An(x, s_n);
            return 2 * Math.PI * N * Math.Pow(a_n, 3d / 2) / (1 - x * x);
        }

        /// <summary>
        /// Calculates the first derivative of Trev with respect to x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        private static double TrevPrimeI(double x, double s_n, int N)
        {
            var a_n = An(x, s_n);
            var da_n = AnPrimeI(x, s_n);
            var numerator = N * Math.PI * Math.Sqrt(a_n) * (4 * x * a_n - 3 * (x * x - 1) * da_n);
            var denominator = Math.Pow((1 - x * x), 2);
            return numerator / denominator;
        }

        /// <summary>
        /// Calculates the second derivative of Trev with respect to x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        private static double TrevPrimeII(double x, double s_n, int N)
        {
            var a_n = An(x, s_n);
            var da_n = AnPrimeI(x, s_n);
            var dda_n = AnPrimeII(x, s_n);
            var multiplier = (N*Math.PI) / (2 * Math.Pow((1 - x * x), 3) * Math.Sqrt(a_n));
            var firstTerm = 6 * (x * x - 1) * a_n * (-2 * x * da_n + (x * x - 1) * dda_n - 2 * x * da_n);
            var secondTerm = 3 * Math.Pow((x * x - 1), 2) * da_n * da_n;
            var thirdTerm = 8 * (3 * x * x + 1) * a_n * a_n;
            return multiplier * (firstTerm + secondTerm + thirdTerm);
        }

        /// <summary>
        /// Calculates the normalized semi-major axis that corresponds to energy x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <returns></returns>
        private static double An(double x, double s_n)
        {
            var aMin_n = s_n / 2;
            return aMin_n / (1 - Math.Pow(x, 2)); 
        }

        /// <summary>
        /// Calculates the first derivative of a_n with respect to x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <returns></returns>
        private static double AnPrimeI(double x, double s_n)
        {
            return (s_n * x) / Math.Pow((1 - x * x), 2);
        }

        /// <summary>
        /// Calculates the second derivative of a_n with respect to x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <returns></returns>
        private static double AnPrimeII(double x, double s_n)
        {
            return s_n * (3 * x * x + 1) / Math.Pow((1 - x * x), 3);
        }

        /// <summary>
        /// Calculates the scaled universal variable.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="lambda"></param>
        /// <param name="s_n"></param>
        /// <returns></returns>
        private static double U(double x, double q)
        {
            return (-2 * x * q) / (1 - Math.Pow(x, 2));
        }

        /// <summary>
        /// Calculates the first derivative of u with respect to x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private static double UPrimeI(double x, double q)
        {
            return -2 * q * (1 + x * x) / Math.Pow((1 - x * x), 2);
        }

        /// <summary>
        /// Calculates the second derivative of u with respect to x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private static double UPrimeII(double x, double q)
        {
            return 4 * q * x * (x * x + 3) / Math.Pow((x * x - 1), 3);
        }

        /// <summary>
        /// Calculates the non-dimensional parameter.
        /// It is related to the orbit's geometry and energy.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private static double Z(double x, double s_n, double q)
        {
            var u = U(x, q);
            var a_n = An(x, s_n);
            return u * u / a_n;
        }

        /// <summary>
        /// Calculates the first derivative of z.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private static double ZPrimeI(double x, double s_n, double q)
        {
            var u = U(x, q);
            var du = UPrimeI(x, q);
            var a_n = An(x, s_n);
            var da_n = AnPrimeI(x, s_n);
            var firstTerm = 2 * u * du / a_n;
            var secondTerm = -1 * u * u * da_n / (a_n * a_n);
            return firstTerm + secondTerm;
        }

        /// <summary>
        /// Calculates the second derivative of z.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private static double ZPrimeII(double x, double s_n, double q)
        {
            var u = U(x, q);
            var du = UPrimeI(x, q);
            var ddu = UPrimeII(x, q);
            var a_n = An(x, s_n);
            var da_n = AnPrimeI(x, s_n);
            var dda_n = AnPrimeII(x, s_n);
            var firstTerm = (1 / (a_n * a_n)) * (2 * (du * du + u * ddu) * a_n - 2 * u * du * da_n);
            var secondTerm = (-1 / Math.Pow(a_n, 4)) * (a_n * a_n * (2 * u * du * da_n + u * u * dda_n) -
                                                        2 * a_n * da_n * (2 * u * du * a_n - u * u * da_n));
            return firstTerm + secondTerm;
        }

        #endregion
    }
}
