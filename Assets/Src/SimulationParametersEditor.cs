using System;
using UnityEngine;

namespace Src
{
	[Serializable]
	public class SimulationParametersEditor 
	{
		[Header("Simulation parameters")]
		[Tooltip("This value is used to convert objects simulation positions to Unity's scene positions and vice versa.")]
		[SerializeField] private double kilometersPerUnit = 100;
		[Tooltip("How to convert Unity's time into simulation time. Use it to speed up the simulation")]
		[SerializeField] private float timeStepMultiplier;
		[SerializeField] private GameObject earthGo;
		[Tooltip("Measured in km^3/s^2")]
		[SerializeField] private double gravitationalParameter;
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

		public double KilometersPerUnit => kilometersPerUnit;
		public float TimeStepMultiplier => timeStepMultiplier;
		public GameObject EarthGo => earthGo;
		public double GravitationalParameter => gravitationalParameter;
		public GameObject SpacecraftGo => spacecraftGo;
		public Vector3 SpacecraftInitialVelocityKmS => spacecraftInitialVelocityKmS;
		public double SpacecraftMassKg => spacecraftMassKg;
		public double FuelMassKg => fuelMassKg;
		public Vector3 ExhaustDirection => exhaustDirection;
		public double ExhaustVelocityModuleMs => exhaustVelocityModuleMs;
		public double FuelConsumptionRateKgS => fuelConsumptionRateKgS;
	}
}