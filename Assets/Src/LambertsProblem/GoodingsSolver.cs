using System;
using MehaMath.Math.Components;
using UnityEngine;

namespace Src.LambertsProblem
{
    /// <summary>
    /// The algorithm is based on article "A procedure for the solution of Lambert's orbital boundary-value problem" by Gooding R. H.
    /// </summary>
    public class GoodingsSolver : MonoBehaviour
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
        public bool CalculateTransfer(Vector start, Vector destination, double transferTime, out Vector v1, out Vector v2)
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
            var s = (r1 + r2 + chord) / 2; //Semi-perimeter of the triangle ABC where A is the central body center, B is start and C is destination.
            var cosTheta = Vector.DotProduct(start, destination) / (r1 * r2);
            cosTheta = Math.Max(-1, Math.Min(1, cosTheta));
            var theta = Math.Acos(cosTheta) + Math.PI*2*Revolutions; //Angular separation between start and destination
            if (LongPath)
            {
                theta = Math.PI*2 - theta + Math.PI*2*Revolutions;
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
            if (y1*y2>0)
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
            
            var aMin_n = s_n/ 2;
            var a_n = aMin_n / (1 - Math.Pow(x, 2)); //Normalized semi-major axis that corresponds to energy x
            if (a_n < 0)
                return double.PositiveInfinity;

            var q = lambda / Math.Sqrt(s_n);
            var u = (-2 * x * q) / (1 - Math.Pow(x, 2)); //Scaled universal variable
            
            var z = Math.Pow(u, 2) / a_n;
            //Stumpff function 
            var S_z = StumpffFunctions.S(z);
            //Intermediate terms
            var T_0 = s_n - chord_n;
            var T_1 = 2 * q * Math.Sqrt(s_n) * u;
            var T_2 = 4 * Math.Pow(q, 2) * s_n * S_z;
            var T_3 = Math.Sqrt(s_n) * (1 - Math.Pow(x, 2)) * u * (1 - z * S_z);
            var ttof = (T_0 + T_1 + T_2 + T_3) / (1 - Math.Pow(x, 2)); //Normalized time of flight between
            ttof += (2 * Math.PI * Revolutions * Math.Pow(a_n, 1.5)) / (1 - x*x);
            return ttof > 0 ? ttof : double.PositiveInfinity;
        }
    }
}
