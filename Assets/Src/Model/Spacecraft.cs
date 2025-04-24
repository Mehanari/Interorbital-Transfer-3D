using System;
using MehaMath.Math.Components;
using Newtonsoft.Json;

namespace Src.Model
{
	[Serializable]
	public struct Spacecraft
	{
		private Vector _exhaustDirection;
		[JsonProperty("mass")]
		public double Mass { get; set; }
		[JsonProperty("fuelMass")]
		public double FuelMass { get; set; }
		/// <summary>
		/// This variable controls the magnitude of the engine acceleration.
		/// </summary>
		[JsonProperty("fuelConsumptionRate")]
		public double FuelConsumptionRate { get; set; }
		[JsonIgnore]
		public double TotalMass => Mass + FuelMass;
		[JsonProperty("position")]
		public Vector Position { get; set; }
		[JsonProperty("velocity")]
		public Vector Velocity { get; set; }
		/// <summary>
		/// Note: this value is constant for most rocket engines.
		/// You don't have to keep it constant thought, but doing it would make simulation more realistic.
		/// </summary>
		[JsonProperty("exhaustVelocityModule")]
		public double ExhaustVelocityModule { get; set; }

		/// <summary>
		/// The spacecraft's velocity and it's engine exhaust velocity are often measured in different units.
		/// For example, first is measured in km/s, second is measured in m/s.
		/// Because of this we need to specify how to convert exhaust velocity units to spacecraft's velocity units and vice versa.
		/// This parameter is here for that.
		/// It answers the question "How many times spacecraft's velocity units are bigger than it's engine exhaust velocity units?"
		/// </summary>
		[JsonProperty("exhaustVelocityConversionRate")]
		public double ExhaustVelocityConversionRate { get; set; }
		[JsonProperty("maxFuelConsumptionRate")]
		public double MaxFuelConsumptionRate { get; set; }

		/// <summary>
		/// One of the main spacecraft control variables which determines in which direction the spaceship will accelerate.
		/// </summary>
		[JsonProperty("exhaustDirection")]
		public Vector ExhaustDirection
		{
			get => _exhaustDirection;
			set => _exhaustDirection = value.Normalized();
		}

		/// <summary>
		/// Returns a vector representation of the spacecraft state. The first n values are position, next n values are velocity, the last value is the fuel mass.
		/// This state only includes values that are supposed to change during the simulation, excluding the control variables.
		/// n is number of space dimensions.
		/// </summary>
		/// <returns></returns>
		public Vector ToStateVector()
		{
			return Vector.Combine(Position, Velocity, new Vector(FuelMass));
		}

		public Spacecraft FromStateVector(Vector stateVector, int spaceDimensions)
		{
			var position = new Vector(spaceDimensions);
			var velocity = new Vector(spaceDimensions);
			var fuelMass = 0d;
			for (int i = 0; i < 2*spaceDimensions + 1; i++)
			{
				if (i <= spaceDimensions - 1)
				{
					position[i] = stateVector[i];
				}
				else if (i >= spaceDimensions && i <= spaceDimensions*2 - 1 )
				{
					velocity[i-spaceDimensions] = stateVector[i];
				}
				else
				{
					fuelMass = stateVector[i];
				}
			}

			var result = this.Clone();
			result.Position = position;
			result.Velocity = velocity;
			result.FuelMass = fuelMass;
			return result;
		}

		public Spacecraft Clone()
		{
			return new Spacecraft
			{
				ExhaustDirection = this.ExhaustDirection,
				ExhaustVelocityModule = this.ExhaustVelocityModule,
				FuelMass = this.FuelMass,
				FuelConsumptionRate = this.FuelConsumptionRate,
				Position = this.Position,
				Velocity = this.Velocity,
				Mass = this.Mass,
				ExhaustVelocityConversionRate = this.ExhaustVelocityConversionRate,
				MaxFuelConsumptionRate = this.MaxFuelConsumptionRate
			};
		}
	}
}
