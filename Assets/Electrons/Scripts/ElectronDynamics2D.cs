using System;
using MehaMath.Math.Components;

namespace Electrons.Scripts
{
	public class ElectronDynamics2D
	{
		private Rkf45 _propagator = new();
		
		public ElectronState Propagate(ElectronState initialState, double deltaT)
		{
			var stateVector = initialState.ToStateVector();
			if (stateVector.Length != 4)
			{
				throw new InvalidOperationException("Probably invalid position dimensions.");
			}

			var nextState = _propagator.PropagateState(stateVector, deltaT, Dynamics);
			return ElectronState.FromStateVector(nextState, 2);
		}

		/// <summary>
		/// Time since is how much time passed since the object was in its initial initialState.
		/// </summary>
		/// <param name="initialState"></param>
		/// <param name="timeSince"></param>
		/// <returns></returns>
		private Vector Dynamics(Vector initialState)
		{
			var x0 = initialState[0];
			var y0 = initialState[1];
			var pos0 = new Vector(x0, y0); //Initial position
			var r0 = pos0.Magnitude(); //Initial radius (distance to the central body)
			var leaveSpeed = initialState[2];
			var omega0 = initialState[3];
			var angle0 = Math.Atan2(y0, x0);
			var dx = -Math.Sin(angle0) * omega0 + Math.Cos(angle0) * leaveSpeed;
			var dy = Math.Cos(angle0) * omega0 + Math.Sin(angle0) * leaveSpeed;
			var baseAngularVelocity = omega0 * r0;
			var dOmega = -leaveSpeed * baseAngularVelocity / (r0 * r0);
			return new Vector(dx, dy, 0, dOmega);
		} 
	}
}