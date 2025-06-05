using System;
using Src.Model;
using Src.OptimizationFramework;
using Src.OptimizationFramework.DataModels;
using UnityEngine;

namespace Src
{
	[Serializable]
	public class OrbitParametersEditor
	{
		[SerializeField] private double semiMajorAxis;
		[SerializeField] private double eccentricity;
		[SerializeField] private double inclination;
		[SerializeField] private double perigeeArgument;
		[SerializeField] private double ascendingNodeLongitude;
		[SerializeField] private double trueAnomaly;
	
		public double SemiMajorAxis => semiMajorAxis;
		public double Eccentricity => eccentricity;
		public double Inclination => inclination;
		public double PerigeeArgument => perigeeArgument;
		public double AscendingNodeLongitude => ascendingNodeLongitude;
		public double TrueAnomaly => trueAnomaly;

		public Orbit Orbit => new Orbit
		{
			SemiMajorAxis = semiMajorAxis,
			Eccentricity = eccentricity,
			Inclination = inclination,
			PerigeeArgument = perigeeArgument,
			AscendingNodeLongitude = ascendingNodeLongitude,
			TrueAnomaly = trueAnomaly
		};
	}
}