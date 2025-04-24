using System;
using System.Collections.Generic;
using UnityEngine;

namespace Src.EditorTools
{
	[Serializable]
	public class MultiSpacecraftsSimulationParameters
	{
		[Header("Simulation parameters")]
		[Tooltip("This value is used to convert objects simulation positions to Unity's scene positions and vice versa.")]
		[SerializeField] private double kilometersPerUnit = 100;
		[SerializeField] private GameObject earthGo;
		[Tooltip("Measured in km^3/s^2")]
		[SerializeField] private double gravitationalParameter;
		[SerializeField] private List<SpacecraftParameters> spacecrafts = new();
		
		public double KilometersPerUnit => kilometersPerUnit;
		public GameObject EarthGo => earthGo;
		public double GravitationalParameter => gravitationalParameter;
		public List<SpacecraftParameters> SpacecraftsParameters => spacecrafts;
	}
}