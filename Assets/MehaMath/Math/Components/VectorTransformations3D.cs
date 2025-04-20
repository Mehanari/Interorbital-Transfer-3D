using System;

namespace MehaMath.Math.Components
{
	public static class VectorTransformations3D
	{
		/// <summary>
		/// Returns the x-component of vector b in the reference frame where the z-axis is aligned with normalized vector a,
		/// and the x-axis is the projection of (1, 0, 0) onto the plane orthogonal to a.
		/// </summary>
		/// <param name="a">The vector defining the new z-axis (will be normalized).</param>
		/// <param name="b">The vector to transform.</param>
		/// <returns>The x-component of b in the new reference frame.</returns>
		public static Vector TransformToNewFrame(Vector a, Vector b)
		{
			if (a.Length != 3 || b.Length != 3)
			{
				throw new InvalidOperationException(
					"The lengths of vectors a and b must be 3. The class has 3D in its name for a reason.");
			}
			var zNew = a.Normalized();
			
			var xInitial = new Vector(1, 0, 0);
			var xProjScalar = Vector.DotProduct(xInitial, zNew);
			var xNew = xInitial - zNew * xProjScalar;

			//Check if new x axis is not parallel to a new z axis.
			if (xNew.Magnitude() < 1e-10)
			{
				// If (1, 0, 0) is parallel to a, choose an arbitrary vector orthogonal to a
				// First we try (0, 1, 0) and project it
				xInitial = new Vector(0, 1, 0);
				xProjScalar = Vector.DotProduct(xInitial, zNew);
				xNew = xInitial - zNew * xProjScalar;
				if (xNew.Magnitude() < 1e-10)
				{
					// Try (0, 0, 1)
					xInitial = new Vector(0, 0, 1);
					xProjScalar = Vector.DotProduct(xInitial, zNew);
					xNew = xInitial - zNew * xProjScalar;
					if (xNew.Magnitude() < 1e-10)
					{
						throw new InvalidOperationException("Unable to construct x-axis basis vector.");
					}
				}
			}

			xNew = xNew / xNew.Magnitude();
			var yNew = Vector.CrossProduct3D(zNew, xNew).Normalized();
			
			//Express b in new basis
			var bX = Vector.DotProduct(b, xNew);
			var bY = Vector.DotProduct(b, yNew);
			var bZ = Vector.DotProduct(b, zNew);
			return new Vector(bX, bY, bZ);
		}

		/// <summary>
		/// If given (1, 0, 0), return (0, 1, 0).
		/// If given (0, 1, 0), return (0, 0, 1).
		/// If given (0, 0, 1) return (1, 0, 0).
		/// If given anything else, returns (1, 0, 0).
		/// </summary>
		/// <param name="current"></param>
		/// <returns></returns>
		private static Vector GetNextBasis(Vector current)
		{
			if (current.Equals(new Vector(1, 0, 0)))
			{
				return new Vector(0, 1, 0);
			}

			if (current.Equals(new Vector(0, 1, 0)))
			{
				return new Vector(0, 0, 1);
			}

			return new Vector(1, 0, 0);
		}
	}
}