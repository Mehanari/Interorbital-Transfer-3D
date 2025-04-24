using MehaMath.Math.Components;
using Src.Model;
using UnityEngine;

namespace Src.EditorTools
{
	public class SpacecraftParameters : MonoBehaviour
	{
		[SerializeField] private GameObject spacecraftGo;
		[Tooltip("Velocity is measured in kilometers per second")]
		[SerializeField] private Vector3 spacecraftInitialVelocityKmS;
		[SerializeField] private double spacecraftMassKg;
		[SerializeField] private double fuelMassKg;
		[SerializeField] private Vector3 exhaustDirection;
		[Tooltip("Measured in meters per second")]
		[SerializeField] private double exhaustVelocityModuleMs;
		[Tooltip("Fuel consumption rate is measured in kilograms per second")]
		[SerializeField] private double fuelConsumptionRateKgS;
		[SerializeField] private double maxFuelConsumptionRateKgS;
		
		public GameObject SpacecraftGo => spacecraftGo;
		public Vector3 SpacecraftInitialVelocityKmS => spacecraftInitialVelocityKmS;
		public double SpacecraftMassKg => spacecraftMassKg;
		public double FuelMassKg => fuelMassKg;
		public Vector3 ExhaustDirection => exhaustDirection;
		public double ExhaustVelocityModuleMs => exhaustVelocityModuleMs;
		public double FuelConsumptionRateKgS => fuelConsumptionRateKgS;
		public double MaxFuelConsumptionRateKgS => maxFuelConsumptionRateKgS;

		public Spacecraft GetSpacecraft(double kilometersPerUnit)
		{
			return new Spacecraft(){
				Velocity = new Vector(spacecraftInitialVelocityKmS),
				Position = new Vector(spacecraftGo.transform.position*(float)kilometersPerUnit),
				FuelMass = fuelMassKg,
				FuelConsumptionRate = fuelConsumptionRateKgS,
				Mass = spacecraftMassKg,
				ExhaustDirection = new Vector(exhaustDirection),
				ExhaustVelocityModule = exhaustVelocityModuleMs,
				ExhaustVelocityConversionRate = 1000,
				MaxFuelConsumptionRate = maxFuelConsumptionRateKgS
			};
		}
	}
}