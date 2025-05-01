using System;
using MehaMath.Math.Components;

namespace Electrons.Scripts
{
	public struct ElectronState
	{
		public Vector Position { get; set; }
		public double LeaveSpeed { get; set; }
		public double AngularVelocity { get; set; }

		/// <summary>
		/// Returns a vector where first n values are position, last two values are leave speed and angular velocity respectively.
		/// </summary>
		/// <returns></returns>
		public Vector ToStateVector()
		{
			return new Vector(new Vector(Position, LeaveSpeed), AngularVelocity);
		}

		public static ElectronState FromStateVector(Vector stateVector, int positionDimensions)
		{
			var position = stateVector.LeftPart(positionDimensions);
			var leaveSpeed = stateVector[positionDimensions];
			var angularVelocity = stateVector[positionDimensions + 1];
			return new ElectronState()
			{
				Position = position,
				LeaveSpeed = leaveSpeed,
				AngularVelocity = angularVelocity
			};
		}

		public Vector GetVelocity2D()
		{
			var angle = Math.Atan2(Position[1], Position[0]);
			var dx = -Math.Sin(angle) * AngularVelocity + Math.Cos(angle) * LeaveSpeed;
			var dy = Math.Cos(angle) * AngularVelocity + Math.Sin(angle) * LeaveSpeed;
			return new Vector(dx, dy);
		}
	}
}