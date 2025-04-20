using MehaMath.Math.Components;
using Src.Model;

namespace Src.SpacecraftDynamics.CentralBodyDynamics
{
	public class Rk4Dynamics : CentralBodyDynamics
	{
		public override Spacecraft PropagateState(Spacecraft spacecraft, double deltaT)
		{
			//What changes:
			//Fuel mass
			//Velocity
			//Position
			var x = spacecraft.ToStateVector();
			var k1 = Derivative(spacecraft);
			var k2 = Derivative(x + k1*deltaT/2, spacecraft);
			var k3 = Derivative(x + k2 * deltaT / 2, spacecraft);
			var k4 = Derivative(x + k3 * deltaT, spacecraft);
			var result = x + (k1 + k2 * 2 + k3 * 2 + k4)*(deltaT/6);
			return spacecraft.FromStateVector(result, spacecraft.Position.Length);
		}

		private Vector Derivative(Vector stateVector, Spacecraft spacecraft)
		{
			return Derivative(spacecraft.FromStateVector(stateVector, spacecraft.Position.Length));
		}

		/// <summary>
		/// The first n values are the position rate of change. Next n values are velocity rate of change.
		/// The last value is fuel mass rate of change.
		/// </summary>
		/// <param name="spacecraft"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		private Vector Derivative(Spacecraft spacecraft)
		{
			return Vector.Combine(spacecraft.Velocity, GetAcceleration(spacecraft),
				new Vector(-spacecraft.FuelConsumptionRate));
		}

		/// <summary>
		/// The t is passed in case if some parameters depend on time, like exhaust direction or fuel consumption rate due to control.
		/// </summary>
		/// <param name="spacecraft"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		private Vector GetAcceleration(Spacecraft spacecraft)
		{
			var displacement = CentralBodyPosition - spacecraft.Position;
			var gravitationalComponent = displacement.Normalized() * GravitationalParameter / displacement.MagnitudeSquare();
			var engineComponent = spacecraft.ExhaustDirection * (-1) * spacecraft.ExhaustVelocityModule *
				spacecraft.FuelConsumptionRate / spacecraft.TotalMass;
			engineComponent /= spacecraft.ExhaustVelocityConversionRate;
			if (spacecraft.FuelMass <= 0)
			{
				engineComponent = engineComponent * 0;
			}

			return gravitationalComponent + engineComponent;
		}
	}
}