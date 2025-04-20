using System;
using Src.Model;

namespace Src.SpacecraftDynamics.CentralBodyDynamics
{
	public class CustomDynamics : CentralBodyDynamics
	{
		public override Spacecraft PropagateState(Spacecraft spacecraft, double deltaT)
		{
			//Beg - beginning
			//Acc - acceleration
			//Mod - module
			//Eng - engine (not English)
			
			//Calculating the engine acceleration
			var currentMass = spacecraft.Mass + spacecraft.FuelMass;
			var nextFuelMass = spacecraft.FuelMass - deltaT * spacecraft.FuelConsumptionRate;
			if (nextFuelMass < 0)
			{
				nextFuelMass = 0;
			}
			var nextMass = spacecraft.Mass + nextFuelMass;
			var engDeltaV = spacecraft.ExhaustDirection * (-1) * spacecraft.ExhaustVelocityModule * Math.Log(currentMass / nextMass); //How much the speed will change after deltaT
			var engAcc = engDeltaV / deltaT; //Speed rate of change caused by engine. We assume this rate of change is constant during deltaT.
            
			var displaceBeg = CentralBodyPosition - spacecraft.Position; //Radius-vector from Earth's center to the spacecraft position
			var accModBeg = GravitationalParameter / (displaceBeg).MagnitudeSquare(); //Gravitational acceleration module for the current moment
			var accBeg = displaceBeg.Normalized() * accModBeg + engAcc; //Acceleration for the current moment of time. Sum of engine acceleration and gravity acceleration.
			//accBeg does not include engine acceleration because engAcc is how much the speed of the spacecraft will change during deltaT
			var pos1 = spacecraft.Position + spacecraft.Velocity * deltaT;
			var displaceEnd = CentralBodyPosition - pos1; //Spacecraft position relative to earthGo if it (spacecraft) moves with its current velocity for deltaT time.
			var accModEnd = GravitationalParameter / (displaceEnd).MagnitudeSquare(); 
			var accEnd = displaceEnd.Normalized() * accModEnd + engAcc;
			var accSpeed = (accEnd - accBeg) / deltaT; //The engAcc cancels out here, but is fine, as it does not affect the acceleration rate of change, because of our assumption that engAcc is constant during deltaT. 

			var newVelocity = spacecraft.Velocity + accBeg * deltaT + accSpeed * deltaT * deltaT / 2; //The engAcc takes effect here because it is included in accBeg.
			var newPosition = spacecraft.Position + spacecraft.Velocity * deltaT +
			                  accBeg * deltaT * deltaT / 2 + accSpeed * deltaT * deltaT * deltaT / 6;

			spacecraft.FuelMass = nextFuelMass;
			spacecraft.Velocity = newVelocity;
			spacecraft.Position = newPosition;

			return spacecraft;
		}
	}
}