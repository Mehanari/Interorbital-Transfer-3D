using System;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using MehaMath.Math.Components;
using Src.FinalComponents;
using Unity.Mathematics;
using UnityEngine;

namespace Src.LambertProblem
{
    /// <summary>
    /// The algorithm is based on article "A procedure for the solution of Lambert's orbital boundary-value problem" by Gooding R. H.
    /// </summary>
    public class GoodingSolver
    {
        private const double TOLERANCE = 1e-11;
        private const double HALLEY_TOLERANCE = 1e-7; //Tolerance for Halley iterations step.
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
        /// Also, returns false if orbit is invalid (crushing into central body for example)
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

            //Halley iterations
            var x = x1;
            var x_new = x;
            var iter = 0;
            while (Math.Abs(LogErr(x, s_n, chord_n, lambda, time_n)) > HALLEY_TOLERANCE && iter < MAX_ITERATIONS)
            {
                iter++;
                var err = LogErr(x, s_n, chord_n, lambda, time_n);
                var dErr = LogErrPrimeI(x, s_n, chord_n, lambda);
                var ddErr = LogErrPrimeII(x, s_n, chord_n, lambda);
                var diff = (2 * dErr * err) / (2 * dErr * dErr - err * ddErr);
                x_new = x - diff;
                x = x_new;
            }

            var eta = (x / Math.Sqrt(1 - x * x)) * Math.Sqrt(s_n); //The eccentric anomaly difference in the universal variable formulation
            //Lagrange coefficients for calculating the final velocity
            var f = 1 - (s_n / r1_n) * (1 - Math.Cos(eta));
            var g = s_n * Math.Sqrt(s_n) * (eta - Math.Sin(eta));
            var df = (-Math.Sqrt(s_n) / (r1_n * r2_n)) * Math.Sin(eta);
            var dg = 1 - (s_n / r2_n) * (1 - Math.Cos(eta));
            
            //Calculating the velocities
            v1 = (destination - start * f) / g;
            v2 = (destination * dg - start) / g;
            //Converting into physical units
            v1 *= Math.Sqrt(GravitationalParameter / r1);
            v2 *= Math.Sqrt(GravitationalParameter / r1);
            
            //Checking if the obtained transfer orbit goes through the central body
            var h = Vector.CrossProduct3D(start, v1); //Specific angular momentum
            var e = (Vector.CrossProduct3D(v1, h) / GravitationalParameter) - (start / r1); //Eccentricity vector
            var energy = (v1.MagnitudeSquare() / 2) - GravitationalParameter / r1;
            var a = -GravitationalParameter / (2 * energy); //Semi-major axis
            var rp = a * (1 - e.Magnitude()); //Pericenter distance
            if (rp <= CentralBodyRadius)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Computes the logarithmic error between the time of flight (ttof) obtained from given x and the desired time of flight (desiredTtof).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="chord_n"></param>
        /// <param name="lambda"></param>
        /// <param name="desiredTtof"></param>
        /// <returns></returns>
        private double LogErr(double x, double s_n, double chord_n, double lambda, double desiredTtof)
        {
            var ttof = TimeOfFlight(x, s_n, chord_n, lambda);
            return Math.Log(ttof) - Math.Log(desiredTtof);
        }

        /// <summary>
        /// Computes the first derivative of logarithmic time of flight error with respect to x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="chord_n"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        private double LogErrPrimeI(double x, double s_n, double chord_n, double lambda)
        {
            var ttof = TimeOfFlight(x, s_n, chord_n, lambda);
            var dttof = TimeOfFlightPrimeI(x, s_n, chord_n, lambda);
            return dttof / ttof;
        }

        /// <summary>
        /// Computes the second derivative of logarithmic time of flight error with respect to x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="chord_n"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        private double LogErrPrimeII(double x, double s_n, double chord_n, double lambda)
        {
            var ttof = TimeOfFlight(x, s_n, chord_n, lambda);
            var dttof = TimeOfFlightPrimeI(x, s_n, chord_n, lambda);
            var ddttof = TimeOfFlightPrimeII(x, s_n, chord_n, lambda);
            return (ttof*ddttof-dttof*dttof)/(ttof * ttof);
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

        /// <summary>
        /// Calculates the second derivative of the time of flight with respect to x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="chord_n"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        private double TimeOfFlightPrimeII(double x, double s_n, double chord_n, double lambda)
        {
            if (Math.Abs(x) > MAX_X_MODULE) // Near-parabolic case
                return double.PositiveInfinity;

            var T = this.T(x, s_n, chord_n, lambda);
            var dT = TPrimeI(x, s_n, lambda);
            var ddT = TPrimeII(x, s_n, lambda);
            var denominator = Math.Pow((1 - x * x), 3);
            var numerator = (x * x - 1) * ((x * x - 1) * ddT - 4 * x * dT) + ((6 * x * x + 2) * T);
            var ddTrev = TrevPrimeII(x, s_n, Revolutions);
            return (numerator / denominator) + ddTrev;
        }

        #region Helper functions

        /// <summary>
        /// Calculates the second derivative of the T component with respect to x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        private double TPrimeII(double x, double s_n, double lambda)
        {
            var q = lambda / Math.Sqrt(s_n);
            var du = UPrimeI(x, q);
            var ddu = UPrimeII(x, q);
            var z = Z(x, s_n, q);
            var dz = ZPrimeI(x, s_n, q);
            var ddz = ZPrimeII(x, s_n, q);
            var dSz = StumpffFunctions.SPrimeI(z);
            var ddSz = StumpffFunctions.SPrimeII(z);

            var ddT_1 = 2 * q * Math.Sqrt(s_n) * ddu;
            var ddT_2 = 4 * q * q * s_n * (dz * dz * ddSz + ddz * dSz);
            var ddT_3 = T3PrimeII(x, s_n, lambda);
            return ddT_1 + ddT_2 + ddT_3;
        }
        
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
        /// Calculates the second derivative of the third intermediate term of the normalized time of flight.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s_n"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        private static double T3PrimeII(double x, double s_n, double lambda)
        {
            var q = lambda / Math.Sqrt(s_n);
            //All the values needed to calculate the ddT3
            var u = U(x, q);
            var du = UPrimeI(x, q);
            var ddu = UPrimeII(x, q);
            var z = Z(x, s_n, q);
            var dz = ZPrimeI(x, s_n, q);
            var ddz = ZPrimeII(x, s_n, q);
            var Sz = StumpffFunctions.S(z);
            var dSz = StumpffFunctions.SPrimeI(z);
            var ddSz = StumpffFunctions.SPrimeII(z);
            
            //The ddT3 is calculates as mult*(dA + dB + dC), each term of dA, dB and dC can be represented as sum of sub-terms.
            //We start with mult.
            var mult = Math.Sqrt(s_n);
            //Calculating the dA. 
            var dA_1 = (x * x - 1) * du * dz * (z * dSz + Sz);
            var dA_2 = (x * x - 1) * u * ddz * (z * dSz + Sz);
            var dA_3 = 2 * x * u * dz * (z * dSz + Sz);
            var dA_4 = (x * x - 1) * u * dz * dz * (z * ddSz + 2 * dSz);
            var dA = dA_1 + dA_2 + dA_3 + dA_4;
            
            //Calculating the dB
            var dB_1 = (x * x - 1) * du * dz * (z * dSz + Sz);
            var dB_2 = (x * x - 1) * (z * Sz - 1) * ddu;
            var dB_3 = 2 * x * (z * Sz - 1) * du;
            var dB = dB_1 + dB_2 + dB_3;
            
            //Calculating the dC
            var dC_mult = 2;
            var dC_1 = u * (x * z * dz * dSz + Sz * (x * dz + z) - 1);
            var dC_2 = x * (z * Sz - 1) * du;
            var dC = dC_mult * (dC_1 + dC_2);

            return mult * (dA + dB + dC);
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
        /// TODO: Check correctness.
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
