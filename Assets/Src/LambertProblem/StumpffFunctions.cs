using System;

namespace Src.LambertProblem
{
	public static class StumpffFunctions
	{
		private static double TOLERANCE = 1e-7;

		/// <summary>
		/// Second derivative of C(z) by z.
		/// </summary>
		/// <param name="z"></param>
		/// <returns></returns>
		public static double CPrimeII(double z)
		{
			if (z > -TOLERANCE && z < TOLERANCE)
			{
				return (1d / 360) - z / 6720;
			}

			return (z * CPrimeI(z) - 3 * z * SPrimeI(z) - C(z) + 3 * S(z)) / (2 * z * z);
		}

		/// <summary>
		/// Second derivative of S(z) by z.
		/// </summary>
		/// <param name="z"></param>
		/// <returns></returns>
		public static double SPrimeII(double z)
		{
			if (z > - TOLERANCE && z < TOLERANCE)
			{
				return (1d / 2520) - z/60480;
			}

			return (-z * z * SPrimeI(z) - 2 * z * CPrimeI(z) + 2 * C(z) - 1) / (2 * z * z);
		}
		
		/// <summary>
		/// First derivative of S(z) by z.
		/// </summary>
		/// <param name="z"></param>
		/// <returns></returns>
		public static double SPrimeI(double z)
		{
			if (z > -TOLERANCE && z < TOLERANCE)
			{
				//First two terms of the series expansion fo dS/dz
				return -(1d / 120) + z / 2520;
			}
			return (1 - z *S(z)-2*C(z))/2*z;
		}

		/// <summary>
		/// First derivative fo C(z) by z.
		/// </summary>
		/// <param name="z"></param>
		/// <returns></returns>
		public static double CPrimeI(double z)
		{
			if (z > -TOLERANCE && z < TOLERANCE)
			{
				//First two terms of the series expansion for dC/dz
				return -(1d / 24) + z / 360;
			}
			return (C(z) - 3*S(z))/2*z;
		}
		
		public static double C(double z)
		{
			if (z > TOLERANCE)
			{
				return (1 - Math.Cos(Math.Sqrt(z))) / z;
			}
			else if (z < -TOLERANCE)
			{
				return (Math.Cosh(Math.Sqrt(-z)) - 1) / (-z);
			}
			else
			{
				return 0.5 - z / 24;
			}
		}

		public static double S(double z)
		{
			if (z > TOLERANCE)
			{
				return (Math.Sqrt(z) - Math.Sin(Math.Sqrt(z))) / Math.Pow(Math.Sqrt(z), 3);
			}
			else if (z < -TOLERANCE)
			{
				return (Math.Sinh(Math.Sqrt(-z)) - Math.Sqrt(-z)) / Math.Pow(-z, 1.5);
			}
			else
			{
				return (1d / 6) - z / 120;
			}
		}
	}
}