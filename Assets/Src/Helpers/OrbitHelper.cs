using System;
using MehaMath.Math.Components;
using Src.Model;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.Video;

namespace Src.Helpers
{
	/// <summary>
	/// Not: methods here only work in 3D space. I guess its fine for the project called "Interorbital transfer 3D"
	/// </summary>
	public static class OrbitHelper
	{
		private const double Tolerance = 1e-10;

		/// <summary>
		/// Checks if given spacecraft is on parabolic orbit around central body.
		/// </summary>
		/// <param name="spacecraft"></param>
		/// <param name="gravitationalConstant"></param>
		/// <param name="centralBodyMass"></param>
		/// <returns></returns>
		public static bool IsParabolic(Spacecraft spacecraft, double mu)
		{
			if (mu < Tolerance)
				throw new ArgumentException("Gravitational parameter mu must be positive.");
			var velocity = spacecraft.Velocity;
			var position = spacecraft.Position;
			var energy = SpecificOrbitalEnergy(velocity, position, mu);
			return Math.Abs(energy) < Tolerance;
		}

		/// <summary>
		/// Checks if given spacecraft is on a rectilinear orbit, that is, goes on a straight line.
		/// </summary>
		/// <param name="spacecraft"></param>
		/// <returns></returns>
		public static bool IsRectilinear(Spacecraft spacecraft)
		{
			var angularMomentum = Vector.CrossProduct3D(spacecraft.Position, spacecraft.Velocity);
			return Math.Abs(angularMomentum.MagnitudeSquare()) < Tolerance;
		}

		/// <summary>
		/// Checks if given spacecraft in on a hyperbolic orbit.
		/// Returns false in case if orbit is parabolic or rectilinear.
		/// </summary>
		/// <param name="spacecraft"></param>
		/// <param name="gravitationalConstant"></param>
		/// <param name="centralBodyMass"></param>
		/// <returns></returns>
		public static bool IsHyperbolic(Spacecraft spacecraft, double mu)
		{
			if (IsParabolic(spacecraft, mu) || IsRectilinear(spacecraft))
				return false;
			var semiMajorAxis = SemiMajorAxis(spacecraft.Position, spacecraft.Velocity, mu);
			return semiMajorAxis < 0;
		}

		private static double SpecificOrbitalEnergy(Vector velocity, Vector position, double mu)
		{
			if (position.Magnitude() < Tolerance)
			{
				throw new ArgumentException("Position must be non-zero.");
			}

			return velocity.MagnitudeSquare() / 2 - mu / position.Magnitude();
		}
		
		/// <summary>
		/// Returns orbit parameters for the given velocity and position of an object.
		/// Throws InvalidOperationException if speed and position parameters infer parabolic or rectilinear orbits.
		/// Use IsParabolic and IsRectilinear methods to check object's orbit before using GetOrbit.
		/// In case if orbit is hyperbolic, returns orbit with negative semi-major axis.
		/// In case of equatorial orbit returns orbit parameters with ascending node longitude and argument of perigee set to zero, as for such orbits these parameters are undefined.
		/// In case of circular orbit returns orbit parameters with argument of perigee equal to zero.
		/// Also, for circular orbit, the true anomaly is calculated based on position and velocity of the orbiting object.
		/// </summary>
		/// <param name="spacecraft"></param>
		/// <param name="gravitationalConstant"></param>
		/// <param name="centralBodyMass"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static Orbit GetOrbit(Vector velocity, Vector position, double mu)
		{
			if (mu < Tolerance)
				throw new ArgumentException("Gravitational parameter mu must be positive.");
			var semiMajorAxis = SemiMajorAxis(position, velocity, mu);
			var angularMomentum = Vector.CrossProduct3D(position, velocity);
			if (Math.Abs(angularMomentum.MagnitudeSquare()) < Tolerance)
			{
				throw new InvalidOperationException("Rectilinear orbit detected, cannot infer orbital parameters.");
			}
			var eccentricity = Eccentricity(position, velocity, angularMomentum, mu);
			var inclination = Inclination(angularMomentum);
			var nodeVector = NodeVector(angularMomentum);
			var ascendingNodeLongitude = AscendingNodeLongitude(nodeVector);
			var argumentOfPerigee = ArgumentOfPerigee(nodeVector, eccentricity);
			var trueAnomaly = TrueAnomaly(eccentricity, position, velocity);
			return new Orbit
			{
				SemiMajorAxis = semiMajorAxis,
				Eccentricity = eccentricity.Magnitude(),
				Inclination = inclination,
				AscendingNodeLongitude = ascendingNodeLongitude,
				PerigeeArgument = argumentOfPerigee,
				TrueAnomaly = trueAnomaly
			};
		}

		private static double SemiMajorAxis(Vector position, Vector velocity, double mu)
		{
			double r = position.Magnitude();
			double v2 = velocity.MagnitudeSquare();
			double denom = 2 * mu - r * v2;
			if (Math.Abs(denom) < Tolerance)
				throw new InvalidOperationException("Parabolic orbit detected (semi-major axis undefined).");
			return mu * r / denom;
		}

		private static Vector Eccentricity(Vector position, Vector velocity, Vector angularMomentum, double mu)
		{
			var e = (Vector.CrossProduct3D(velocity, angularMomentum) / mu) - position / position.Magnitude();
			return e;
		}

		private static double Inclination(Vector angularMomentum)
		{
			double hMag = angularMomentum.Magnitude();
			if (hMag < Tolerance)
				return 0; // Rectilinear orbit, inclination undefined
			double cosI = angularMomentum[2] / hMag;
			// Clamp to [-1, 1] to avoid numerical errors
			cosI = Math.Max(-1, Math.Min(1, cosI));
			return Math.Acos(cosI);
		}

		private static Vector NodeVector(Vector angularMomentum)
		{
			var k = new Vector(0, 0, 1);
			return Vector.CrossProduct3D(k, angularMomentum);
		}

		private static double AscendingNodeLongitude(Vector nodeVector)
		{
			double nMag = nodeVector.Magnitude();
			if (nMag < Tolerance)
				return 0; // Equatorial orbit, Omega undefined
			double cosOmega = nodeVector[0] / nMag;
			cosOmega = Math.Max(-1, Math.Min(1, cosOmega));
			double Omega = Math.Acos(cosOmega);
			if (nodeVector[1] < 0)
				Omega = 2 * Math.PI - Omega;
			return Omega;
		}

		private static double ArgumentOfPerigee(Vector nodeVector, Vector eccentricity)
		{
			var eccentricityMag = eccentricity.Magnitude();
			if (eccentricityMag < Tolerance)
				return 0; // Circular orbit, omega undefined
			double nMag = nodeVector.Magnitude();
			if (nMag < Tolerance)
				return 0; // Equatorial orbit, omega undefined
			double cosOmega = Vector.DotProduct(nodeVector, eccentricity) / (nMag * eccentricityMag);
			cosOmega = Math.Max(-1, Math.Min(1, cosOmega));
			double omega = Math.Acos(cosOmega);
			if (eccentricity[2] < 0)
				omega = 2 * Math.PI - omega;
			return omega;
		}

		private static double TrueAnomaly(Vector eccentricity, Vector position, Vector velocity)
		{
			var eccentricityMag = eccentricity.Magnitude();
			var cosNu = 0d;
			var nu = 0d;
			if (eccentricityMag < Tolerance)
			{
				var h = Vector.CrossProduct3D(position, velocity);
				var posInPlane = VectorTransformations3D.TransformToNewFrame(h.Normalized(), position);
				// Circular orbit: compute angle of position vector
				cosNu = posInPlane[0] / posInPlane.Magnitude();
				nu = Math.Acos(cosNu);
				if (posInPlane[1] < 0)
					nu = 2 * Math.PI - nu;
				return nu;
			}
			cosNu = Vector.DotProduct(eccentricity, position) / (eccentricityMag * position.Magnitude());
			cosNu = Math.Max(-1, Math.Min(1, cosNu));
			nu = Math.Acos(cosNu);
			if (Vector.DotProduct(position, velocity) < 0)
				nu = 2 * Math.PI - nu;
			return nu;
		}

		/// <summary>
		/// Returns world position and velocity of an object on a given orbit.
		/// Uses true anomaly to calculate the position.
		/// mu is a gravitational parameter: G*(CentralBodyMass + OrbitingObjectMass).
		///
		/// WARNING: This method works only for elliptic (including circular) orbits.
		/// </summary>
		/// <param name="orbit"></param>
		/// <returns></returns>
		public static (Vector position, Vector velocity) GetPositionAndVelocity(Orbit orbit, double mu)
		{
			if (mu <= 0 || orbit.SemiMajorAxis <= 0 || orbit.Eccentricity < 0 || orbit.Eccentricity >= 1)
				throw new ArgumentException("Invalid orbit parameters for elliptic orbit.");
			var semilatusRectum = orbit.SemiMajorAxis * (1 - orbit.Eccentricity * orbit.Eccentricity);
			if (semilatusRectum <= 0)
				throw new ArgumentException("Semi-latus rectum must be positive.");
			var denom = 1 + orbit.Eccentricity * Math.Cos(orbit.TrueAnomaly);
			if (denom <= 0)
				throw new ArgumentException("Invalid true anomaly for elliptic orbit.");
			var distance = semilatusRectum / (1 + orbit.Eccentricity * Math.Cos(orbit.TrueAnomaly));
			var posPerifocal = new Vector(distance * Math.Cos(orbit.TrueAnomaly),
				distance * Math.Sin(orbit.TrueAnomaly), 0); //Formula from fundamentals of astrodynamics book by Bate, Mueller, and White
			var velocityPerifocal = new Vector(-Math.Sqrt(mu/semilatusRectum)*Math.Sin(orbit.TrueAnomaly), Math.Sqrt(mu/semilatusRectum)*(orbit.Eccentricity + Math.Cos(orbit.TrueAnomaly)), 0);
			var Rw = new SquareMatrix(new double[,]
			{
				{Math.Cos(orbit.PerigeeArgument), -Math.Sin(orbit.PerigeeArgument), 0},
				{Math.Sin(orbit.PerigeeArgument), Math.Cos(orbit.PerigeeArgument), 0},
				{0, 0, 1}
			}); //Rotation matrix for perigee argument
			var Ri = new SquareMatrix(new double[,]
			{
				{1, 0, 0},
				{0, Math.Cos(orbit.Inclination), -Math.Sin(orbit.Inclination)},
				{0, Math.Sin(orbit.Inclination), Math.Cos(orbit.Inclination)},
			}); //Rotation matrix for inclination
			var ROmega = new SquareMatrix(new double[,]
			{
				{Math.Cos(orbit.AscendingNodeLongitude), -Math.Sin(orbit.AscendingNodeLongitude), 0},
				{Math.Sin(orbit.AscendingNodeLongitude), Math.Cos(orbit.AscendingNodeLongitude), 0},
				{0, 0, 1}
			}); //Rotation matrix for ascending node.
			
			//Applying rotational matrices to transform position in the perifocal plane into the world position
			var posWorld = posPerifocal * Rw;
			posWorld *= Ri;
			posWorld *= ROmega;
			
			//Same for velocity
			var velocityWorld = velocityPerifocal * Rw;
			velocityWorld *= Ri;
			velocityWorld *= ROmega;

			return (posWorld, velocityWorld);
		}
	}
}