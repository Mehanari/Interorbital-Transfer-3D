using System;
using MehaMath.Math.Components;

namespace Src.LambertProblem
{
    /// <summary>
    /// This is a C# translation of gooding1990 implementation from: https://github.com/jorgepiloto/lamberthub/blob/main/src/lamberthub/universal_solvers/gooding.py
    /// </summary>
    public static class Gooding1990 
    {
        /// <summary>
        /// Lambert's problem solver using the method proposed by R. H. Gooding in 1990.
        /// </summary>
        /// <param name="mu">Gravitational parameter.</param>
        /// <param name="r1">Initial position vector.</param>
        /// <param name="r2">Final position vector.</param>
        /// <param name="tof">Desired time of flight.</param>
        /// <param name="revolutions">Number of revolutions. Must be equal or greater than 0 value.</param>
        /// <param name="prograde">If `True`, specifies prograde motion. Otherwise, retrograde motion is imposed.</param>
        /// <param name="lowPath">If two solutions are available, it selects between high or low path.</param>
        /// <param name="maxiter">Maximum number of iterations.</param>
        /// <param name="atol">Absolute tolerance.</param>
        /// <param name="rtol">Relative tolerance.</param>
        /// <returns></returns>
        public static (Vector initialVelocity, Vector finalVelocity) FindTransfer(double mu, Vector r1, Vector r2,
            double tof, int revolutions = 0, bool prograde = true, bool lowPath = true, int maxiter = 35,
            double atol = 1e-5, double rtol = 1e-7)
        {
            ValidateParameters(mu, r1, r2, tof, revolutions);

            var r1Norm = r1.Magnitude();
            var r2Norm = r2.Magnitude();
            var ir1 = r1 / r1Norm;
            var ir2 = r2 / r2Norm;

            var theta = GetTransferAngle(r1, r2, prograde);
            if (theta == 0)
            {
                throw new ArgumentException("Transfer angle was found to be zero!");
            }

            //Theta + additional revolutions
            var dtheta = 2 * Math.PI * revolutions + theta;

            //Vector normal to orbit plane, parallel to angular momentum.
            //Needed to specify orbit motions sense.
            var ih = GetOrbitNormalVector(r1, r2, prograde);

            //Tangential unitary vectors at initial and final position vectors respectively.
            var it1 = Vector.CrossProduct3D(ih, r1);
            var it2 = Vector.CrossProduct3D(ih, r2);
        }

        /// <summary>
        /// Computes a unitary normal vector aligned with the specific angular momentum of the orbit.
        /// </summary>
        /// <param name="r1">Initial position vector.</param>
        /// <param name="r2">Final position vector.</param>
        /// <param name="prograde">If True, it assumes prograde motion, otherwise assumes retrograde.</param>
        /// <returns>Unitary vector aligned with orbit specific angular momentum.</returns>
        private static Vector GetOrbitNormalVector(Vector r1, Vector r2, bool prograde)
        {
            var cross = Vector.CrossProduct3D(r1, r2);
            var ih = cross / cross.Magnitude();
            var alpha = Vector.DotProduct(new Vector(0, 0, 1), ih);

            //A prograde orbit always has a positive vertical component of its specific
            //angular momentum. Therefore, we just need to check for this condition
            if (prograde)
            {
                if (alpha > 0)
                {
                    return ih;
                }
                return ih * -1;
            }

            if (alpha < 0)
            {
                return ih;
            }

            return ih * -1;
        }

        /// <summary>
        /// Compute the transfer angle of the trajectory.
        /// Initial and final position vectors are required together with the direction of motion.
        /// </summary>
        /// <param name="r1">Initial position vector.</param>
        /// <param name="r2">Final position vector.</param>
        /// <param name="prograde">True if prograde motion, false otherwise</param>
        /// <returns>Transfer angle in radians.</returns>
        private static double GetTransferAngle(Vector r1, Vector r2, bool prograde)
        {
            var cross = Vector.CrossProduct3D(r1, r2);
            
            //If vectors are collinear, then the angle between them is either 0 or pi.
            if (cross.All((v) => v == 0))
            {
                for (int i = 0; i < r1.Length; i++)
                {
                    if (Math.Sign(r1[i]) != Math.Sign(r2[i]))
                    {
                        return Math.PI;
                    }

                    return 0;
                }
            }

            var h = cross / cross.Magnitude();
            var alpha = Vector.DotProduct(new Vector(0, 0, 1), h);

            var r1Norm = r1.Magnitude();
            var r2Norm = r2.Magnitude();
            var theta0 = Math.Acos(Vector.DotProduct(r1, r2) / (r1Norm * r2Norm));

            if (prograde)
            {
                if (alpha > 0)
                {
                    return theta0;
                }
                return Math.PI * 2 - theta0;
            }

            if (alpha < 0)
            {
                return theta0;
            }
            return Math.PI * 2 - theta0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mu">Gravitational parameter.</param>
        /// <param name="r1">Initial position vector.</param>
        /// <param name="r2">Final position vector.</param>
        /// <param name="tof">Time of flight.</param>
        /// <param name="revolutions">Number of revolutions.</param>
        private static void ValidateParameters(double mu, Vector r1, Vector r2, double tof, int revolutions)
        {
            if (mu <= 0)
            {
                throw new ArgumentException("Gravitational parameter must be positive!");
            }
            if (r1.Length != 3 || r2.Length != 3)
            {
                throw new ArgumentException("Position vectors must be three-dimensional!");
            }
            if (r1.Equals(r2))
            {
                throw new ArgumentException("Initial and final position vectors cannot be equal!");
            }
            if (tof <= 0)
            {
                throw new ArgumentException("Time of flight must be positive!");
            }

            if (revolutions <= 0)
            {
                throw new ArgumentException("Number of revolutions must be equal or greater than zero!");
            }
        }
    }
}
