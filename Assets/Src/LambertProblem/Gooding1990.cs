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
            var it1 = Vector.CrossProduct3D(ih, ir1);
            var it2 = Vector.CrossProduct3D(ih, ir2);

            var (vr1, vt1, vr2, vt2) = Vlamb(mu, r1Norm, r2Norm, dtheta, tof, lowPath, maxiter, atol, rtol);

            var v1 = ir1 * vr1 + it1 * vt1;
            var v2 = ir2 * vr2 + it2 * vt2;

            return (v1, v2);
        }

        /// <summary>
        /// Auxiliary routine for computing the non-dimensional time of flight as
        /// function of the number of revolutions, the transfer parameter and the
        /// independent variable (x).
        /// </summary>
        /// <param name="revolutions">Number of revolutions.</param>
        /// <param name="q">The transfer angle parameter.</param>
        /// <param name="qsqfm1">Equivalent to 1-q*q.</param>
        /// <param name="x">The independent variable.</param>
        /// <param name="n">Number of output parameters to be returned.</param>
        /// <returns>Time of flight and its first, second and third derivatives with respect to x.</returns>
        private static (double t, double dt, double d2t, double d3t) TimeOfFlight(double revolutions, double q, double qsqfm1, double x,
            double n)
        {
            var sw = 0.4d;
            var lm1 = n == -1;
            var l1 = n >= 1;
            var l2 = n >= 2;
            var l3 = n == 3;
            var qsq = q * q;
            var xsq = x * x;
            var u = (1d - x) * (1d + x);
            var t = 0d;
            var dt = 0d;
            var d2t = 0d;
            var d3t = 0d;
            
            if (lm1 || revolutions > 0 || x < 0d || Math.Abs(u) > sw)
            {
                //Direct computation, series is not required
                var y = Math.Sqrt(Math.Abs(u));
                var z = Math.Sqrt(qsqfm1 + qsq * xsq);
                var qx = q * x;
                
                var a = 0d;
                var b = 0d;
                var aa = 0d;
                var bb = 0d;

                if (qx <= 0)
                {
                    a = z - qx;
                    b = q * z - x;
                }

                if (qx <= 0 && lm1)
                {
                    aa = qsqfm1 / a;
                    bb = qsqfm1 * (qsq * u - xsq) / b;
                }

                if ((qx == 0 && lm1) || (qx > 0))
                {
                    aa = z + qx;
                    bb = q * z + x;
                }

                if (qx > 0.0)
                {
                    a = qsqfm1 / aa;
                    b = qsqfm1 * (qsq * u - xsq) / bb;
                }

                if (lm1)
                {
                    t = 0;
                    dt = b;
                    d2t = bb;
                    d3t = aa;
                }
                else
                {
                    var g = 0d;
                    if (qx * u >= 0)
                        g = x * z + q * u;
                    else
                        g = (xsq - qsq * u) / (x * z - q * u);


                    var f = a * y;

                    if (x <= 1d)
                    {
                        t = revolutions * Math.PI + Math.Atan2(f, g);
                    }
                    else
                    {
                        if (f > sw)
                        {
                            t = Math.Log(f + g);
                        }
                        else
                        {
                            var fg1 = f / (g + 1d);
                            var term = 2 * fg1;
                            var fg1sq = fg1 * fg1;
                            t = term;
                            var twoi1 = 1d;
                            
                            //The python emulation of the FORTRAN-77 do-loop was replaced with the C# do-while loop.
                            var told = t;
                            do
                            {
                                twoi1 = twoi1 + 2;
                                term = term * fg1sq;
                                told = t;
                                t = t + term / twoi1;
                            } while (t != told);
                        }
                    }

                    t = 2d * (t / y + b) / u;

                    if (l1 && z != 0d)
                    {
                        var qz = q / z;
                        var qz2 = qz * qz;
                        qz = qz * qz2;
                        dt = (3d * x * t - 4d * (a + qx * qsqfm1) / z) / u;
                        if (l2)
                            d2t = (3d * t + 5d * x * dt + 4d * qz * qsqfm1) / u;
                        if (l3)
                            d3t = (8d * dt + 7d * x * d2t - 12d * qz * qz2 * x * qsqfm1) / u;
                    }
                }
            }
            else
            {
                //Compute by series.
                var u0i = 1d;
                var u1i = 1d;
                var u2i = 1d;
                var u3i = 1d;

                var term = 4d;
                var tq = q * qsqfm1;
                var i = 0;
                var tqsum = 0d;
                
                if (q < 0.5d)
                    tqsum = 1d - q * qsq;
                else
                    tqsum = (1 / (1 + q) + q) * qsqfm1;


                var ttmold = term / 3d;
                t = ttmold * tqsum;
                

                
                var told = t;
                
                //The python imitation of FORTRAN-77 do-loop was replaced with C# do-while loop.

                do
                {
                    i = i + 1;
                    var p = i;
                    u0i = u0i * u;

                    if (l1 && i > 1)
                        u1i = u1i * u;
                    if (l2 && i > 2)
                        u2i = u2i * u;
                    if (l3 && i > 3)
                        u3i = u3i * u;
                
                    term = term * (p - 0.5d) / p;
                    tq = tq * qsq;
                    tqsum = tqsum + tq;
                    told = t;
                    var tterm = term / (2d * p + 3d);
                    var tqterm = tterm * tqsum;
                    t = t - u0i * ((1.5d * p + 0.25d) * tqterm / (p * p - 0.25d) - ttmold * tq);
                    ttmold = tterm;
                    tqterm = tqterm * p;
                    if (l1)
                        dt = dt + tqterm * u1i;
                    if (l2)
                        d2t = d2t + tqterm * u2i * (p - 1d);
                    if (l3)
                        d3t = d3t + tqterm * u3i * (p - 1d) * (p - 2);
                } while (i < n || t != told);

                if (l3)
                    d3t = 8d * x * (1.5d * d2t - xsq * d3t);
                if (l2)
                    d2t = 2 * (2 * xsq * d2t - dt);
                if (l1)
                    dt = -2 * x * dt;

                t = t / xsq;
            }
            
            return (t, dt, d2t, d3t);
        }

        /// <summary>
        /// Auxiliary routine for finding the independent variable as function of the
        /// number of revolutions, the transfer angle parameter and the non-dimensional
        /// time of flight.
        /// </summary>
        /// <param name="revolutions">Number of revolutions.</param>
        /// <param name="q">The transfer angle parameter.</param>
        /// <param name="qsqfm1">Equivalent to 1-q*q.</param>
        /// <param name="tin">The actual non-dimensional time of flight.</param>
        /// <param name="maxiter">Maximum number of iterations.</param>
        /// <param name="atol">Desired absolute tolerance.</param>
        /// <param name="rtol">Desired relative tolerance.</param>
        /// <returns>nSol - number of solutions, x - first solution, xpl - second solution, if available.</returns>
        private static (int nSol, double x, double xpl) Xlamb(double revolutions, double q, double qsqfm1, double tin,
            int maxiter, double atol, double rtol)
        {
            //Declaring auxiliary parameters.
            var n = 0;
            var x = 0d;
            var xpl = 0d;
            var c0 = 1.7d;
            var c1 = 0.5d;
            var c2 = 0.03d;
            var c3 = 0.15;
            var c41 = 1d;
            var c42 = 0.24d;
            var thr2 = Math.Atan2(qsqfm1, 2d / q) / Math.PI;
            var tmin = 0d;
            var xm = 0d;
            var tdiffm = 0d;
            var d2t2 = 0d;

            //Boolean variables to emulate original code as max as possible.
            var goto3 = false;

            //Auxiliary function for 8th root.
            Func<double, double> d8RT = (x) => Math.Sqrt(Math.Sqrt(Math.Sqrt(x)));
            
            //Start computing the initial guess. The process is different depending on the number of revolutions.
            if (revolutions == 0)
            {
                //Single-revolution starter fromT (at x = 0)  and bilinear usually.
                n = 1;
                var (t0, dt, d2t, d3t) = TimeOfFlight(revolutions, q, qsqfm1, 0.0, 0);
                var tdiff = tin - t0;
                if (tdiff <= 0.0)
                {
                    //-4 is the value of dt for x = 0.
                    x = t0 * tdiff / (-4d * tin);
                }
                else
                {
                    x = -tdiff / (tdiff + 4d);
                    var w = x + c0 * Math.Sqrt(2d * (1 - thr2));

                    if (w < 0d)
                    {
                        x = x - Math.Sqrt(d8RT(-w)) * (x + Math.Sqrt(tdiff / (tdiff + 1.5 * t0)));
                    }

                    w = 4d / (4d + tdiff);
                    x = x * (1d + x * (c1 * w - c2 * x * Math.Sqrt(w)));
                }
            }
            else
            {
                xm = 1d / (1.5d * (revolutions + 0.5d) * Math.PI);

                if (thr2 < 0.5d)
                {
                    xm = d8RT(2d * thr2) * xm;
                }
                if (thr2 > 0.5)
                {
                    xm = (2d - d8RT(2d - 2d * thr2)) * xm;
                }
                
                //For locating Tmin, an iterative process is required. Original
                //implementation imposed 12 iterations but they were not considered to
                //be part of numerical routine as they belong to the initial guess.
                //Here, we impose it not to exceeded the number of iterations.
                var numiter = 1;
                tmin = 1d;
                var dt = 0d;
                var d2t = 0d;
                var d3t = 0d;
                for (; numiter < maxiter+1; numiter++)
                {
                    (tmin, dt, d2t, d3t) = TimeOfFlight(revolutions, q, qsqfm1, xm, 3);
                    if (d2t == 0.0)
                    {
                        break;
                    }

                    var xmold = xm;
                    xm = xm - dt * d2t / (d2t * d2t - dt * d3t / 2d);

                    //Compute the absolute and relative tolerances and check if within desired range
                    if (Math.Abs(xmold - xm) < rtol*Math.Abs(xmold) + atol)
                    {
                        break;
                    }
                }

                //Check last. 
                //P.S: I suppose this is for the case if algorithm did not converge and reached the last iteration of the previous for loop.
                if ((numiter+1) == maxiter)
                {
                    throw new InvalidOperationException("Exceeded max iterations!");
                }

                tdiffm = tin - tmin;
                if (tdiffm < 0.0)
                {
                    throw new InvalidOperationException("No feasible solution, try lower number of revolutions!");
                }
                else if (tdiffm == 0d)
                {
                    x = xm;
                    n = 1;
                    return (n, x, 0);
                }
                else
                {
                    n = 3;
                    if (d2t == 0d)
                    {
                        d2t = 6d * revolutions * Math.PI;
                    }

                    x = Math.Sqrt(tdiffm / (d2t / 2d + tdiffm / Math.Pow((1d - xm), 2)));
                    var w = xm + x;
                    w = w * 4d / (4d + tdiffm) + Math.Pow((1d - w), 2);
                    x = (
                        x
                        * (
                            1.0
                            - (1.0 + revolutions + c41 * (thr2 - 0.5))
                            / (1.0 + c3 * revolutions)
                            * x
                            * (c1 * w + c2 * x * Math.Sqrt(w))
                        )
                        + xm
                    );
                    d2t2 = d2t / 2d;

                    if (x >= 1d)
                    {
                        n = 1;
                        goto3 = true;
                    }
                }
            }
            
            //--- THE ITERATION METHOD FOR HALLEY STARTS NOW ---
            while (true)
            {
                //5: LINE OF STATEMENT
                if (!goto3)
                {
                    for (int numiter = 1; numiter < maxiter+1; numiter++)
                    {
                        //We use _ in the beginning of a variable name to keep its name in the local scope.
                        var (_t, _dt, _d2t, _d3t) = TimeOfFlight(revolutions, q, qsqfm1, x, 2);
                        _t = tin - _t;
                        
                        var xold = x;
                        if (_dt != 0d)
                        {
                            x = x + _t * _dt / (_dt * _dt + _t * _d2t / 2d);
                        }

                        var xAtol = Math.Abs(x - xold);
                        var xRtol = Math.Abs(x / (xold - 1));
                        if (xAtol <= atol && xRtol <= rtol)
                        {
                            break;
                        }
                    }

                    if (n != 3)
                    {
                        return (n, x, xpl);
                    }

                    n = 2;
                    xpl = x;
                }
                else
                {
                    goto3 = false;
                }
                
                //3: LINE OF STATEMENT
                //Second multi-rev starter
                var (t0, dt, d2t, d3t) = TimeOfFlight(revolutions, q, qsqfm1, x, 0);
                var tdiff0 = t0 - tmin;
                var tdiff = tin - t0;

                if (tdiff <= 0d)
                {
                    x = xm - Math.Sqrt(tdiffm / (d2t2 - tdiffm * (d2t2 / tdiff0 - 1.0 / Math.Pow(xm, 2))));
                }
                else
                {
                    x = -tdiff / (tdiff + 4d);
                    var w = x + c0 * Math.Sqrt(2d * (1d - thr2));

                    if (w < 0d)
                    {
                        x = x - Math.Sqrt(d8RT(-w)) * (x + Math.Sqrt(tdiff / (tdiff + 1.5d * t0)));
                    }

                    w = 4d / (4d + tdiff);
                    x = x * (
                        1.0
                        + (1.0 + revolutions + c42 * (thr2 - 0.5))
                        / (1.0 + c3 * revolutions)
                        * x
                        * (c1 * w - c2 * x * Math.Sqrt(w))
                    );

                    if (x <= -1d)
                    {
                        n = n - 1;
                        if (n == 1)
                        {
                            x = xpl;
                        }
                    }
                }
            }

            return (n, x, xpl);
        }

        /// <summary>
        ///Auxiliary routine for computing the velocity vector components, both
        ///radian and tangential ones.
        /// </summary>
        /// <param name="mu"> Gravitational parameter.</param>
        /// <param name="r1Norm">Norm of the initial position vector.</param>
        /// <param name="r2Norm">Norm of the final position vector.</param>
        /// <param name="dtheta">Transfer angle between initial and final vectors.</param>
        /// <param name="tof">Time of flight between initial and final position vectors.</param>
        /// <param name="lowPath">If two solutions are available, it selects between high or low path.</param>
        /// <param name="maxiter">Maximum number of iterations.</param>
        /// <param name="atol">Absolute tolerance.</param>
        /// <param name="rtol">Relative tolerance</param>
        /// <returns>
        /// vri - radial velocity component at the initial position vector,
        /// vti - tangential velocity component at the initial position vector,
        /// vrf - radial velocity component at the final position vector,
        /// vtf - tangential velocity component at the final position vector
        /// </returns>
        private static (double vri, double vti, double vrf, double vtf) Vlamb(double mu, double r1Norm,
            double r2Norm, double dtheta, double tof, bool lowPath, int maxiter, double atol, double rtol)
        {
            //The following yields m = 0 when th = 2pi exactly
            //Neither this nor the original code works for
            //th < 0.0
            var thr2 = dtheta;
            var m = 0;

            while (thr2 > 2*Math.PI)
            {
                thr2 = thr2 - 2 * Math.PI;
                m = m + 1;
            }

            thr2 = thr2 / 2d;
            
            //Compute auxiliary parameters
            var dr = r1Norm - r2Norm;
            var r1r2 = r1Norm * r2Norm;
            var r1r2th = 4d * r1r2 * Math.Pow(Math.Sin(thr2), 2);
            var csq = dr * dr + r1r2th;
            var c = Math.Sqrt(csq);
            var s = (r1Norm + r2Norm + c) / 2d;
            var mus = Math.Sqrt(mu * s / 2d);
            var qsqfm1 = c / s;
            var q = Math.Sqrt(r1r2) * Math.Cos(thr2) / s;
            var rho = 0d;
            var sig = 0d;

            if (c != 0d)
            {
                rho = dr / c;
                sig = r1r2th / csq;
            }
            else
            {
                rho = 0d;
                sig = 1d;
            }

            var t = 4d * mus * tof / Math.Pow(s, 2);

            var (nSol, x1Sol, x2Sol) = Xlamb(m, q, qsqfm1, t, maxiter, atol, rtol);

            var xSol = 0d;

            //Filter the solutions
            if (nSol > 1)
            {
                if (lowPath)
                {
                    xSol = Math.Max(x1Sol, x2Sol);
                }
                else
                {
                    xSol = Math.Min(x1Sol, x2Sol);
                }
            }
            else
            {
                xSol = x1Sol;
            }
            
            //Compute radial and tangential velocity components
            var (_, qzminx, qzplx, zplqx) = TimeOfFlight(m, q, qsqfm1, xSol, -1);
            var vt2 = mus * zplqx * Math.Sqrt(sig);
            var vr1 = mus * (qzminx - qzplx * rho) / r1Norm;
            var vt1 = vt2 / r1Norm;
            var vr2 = -mus * (qzminx + qzplx * rho) / r2Norm;
            vt2 = vt2 / r2Norm;

            return (vr1, vt1, vr2, vt2);
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

            if (revolutions < 0)
            {
                throw new ArgumentException("Number of revolutions must be equal or greater than zero!");
            }
        }
    }
}
