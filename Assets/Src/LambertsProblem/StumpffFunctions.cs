using System;

namespace Src.LambertsProblem
{
	public static class StumpffFunctions
	{
		private static double TOLERANCE = 1e-7;
		
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