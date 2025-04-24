using System;
using MehaMath.Math.Components;
using Src.Model;
using UnityEngine;

namespace Src.EditorTools
{
	[Serializable]
	public class SingleSpacecraftSimulationParameters 
	{
		[Header("Simulation parameters")]
		[Tooltip("This value is used to convert objects simulation positions to Unity's scene positions and vice versa.")]
		[SerializeField] private double kilometersPerUnit = 100;
		[SerializeField] private GameObject earthGo;
		[Tooltip("Measured in km^3/s^2")]
		[SerializeField] private double gravitationalParameter;
		[SerializeField] private SpacecraftParameters spacecraftParameters;
		public double KilometersPerUnit => kilometersPerUnit;
		public GameObject EarthGo => earthGo;
		public double GravitationalParameter => gravitationalParameter;
		public GameObject SpacecraftGo => spacecraftParameters.SpacecraftGo;
		public Vector3 SpacecraftInitialVelocityKmS => spacecraftParameters.SpacecraftInitialVelocityKmS;
		public double SpacecraftMassKg => spacecraftParameters.SpacecraftMassKg;
		public double FuelMassKg => spacecraftParameters.FuelMassKg;
		public Vector3 ExhaustDirection => spacecraftParameters.ExhaustDirection;
		public double ExhaustVelocityModuleMs => spacecraftParameters.ExhaustVelocityModuleMs;
		public double FuelConsumptionRateKgS => spacecraftParameters.FuelConsumptionRateKgS;
		public double MaxFuelConsumptionRateKgS => spacecraftParameters.MaxFuelConsumptionRateKgS;

		public Spacecraft Spacecraft => spacecraftParameters.GetSpacecraft(kilometersPerUnit);
	}
}