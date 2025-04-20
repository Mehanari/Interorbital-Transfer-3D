using System;
using System.Collections.Generic;
using MehaMath.Math.Components;
using Src.Model;
using UnityEngine;

namespace Src.Visualisation
{
	public class OrbitDrawer : MonoBehaviour
	{
		[SerializeField] private double kilometersPerUnit = 1000;
		
		private Dictionary<string, GameObject> _orbits = new();
		
		/// <summary>
		/// Center should be defined in Unity's scene units.
		/// </summary>
		/// <param name="orbit"></param>
		/// <param name="center"></param>
		/// <param name="samplesCount"></param>
		/// <param name="lineParameters"></param>
		public void DrawOrbit(Orbit orbit, Vector3 center, int samplesCount, OrbitLineParameters lineParameters)
		{
			var points = SamplePointsScaled(orbit, center, samplesCount);
			var line = CreateLineRenderer(lineParameters.Name, lineParameters.LineColor, lineParameters.LineWidth,
				samplesCount);
			line.SetPositions(points);
			DeleteOrbit(lineParameters.Name);
			_orbits.Add(lineParameters.Name, line.gameObject);
		}

		private void DeleteOrbit(string orbitName)
		{
			if (!_orbits.TryGetValue(orbitName, out var line)) return;
			Destroy(line.gameObject);
			_orbits.Remove(orbitName);
		}

		private LineRenderer CreateLineRenderer(string orbitName, Color orbitColor, float lineWidth, int positionsCount)
		{
			var line = new GameObject(orbitName).AddComponent<LineRenderer>();
			line.transform.localPosition = Vector3.zero;
			line.positionCount = positionsCount;
			line.startWidth = lineWidth;
			line.endWidth = lineWidth;
			line.startColor = orbitColor;
			line.endColor = orbitColor;
			line.material = new Material(Shader.Find("Sprites/Default"));
			return line;
		}

		private Vector3[] SamplePointsScaled(Orbit orbit, Vector3 center, int samplesCount)
		{
			var Rw = new SquareMatrix(new double[,]
			{
				{Math.Cos(orbit.PerigeeArgument), -Math.Sin(orbit.PerigeeArgument), 0},
				{Math.Sin(orbit.PerigeeArgument), Math.Cos(orbit.PerigeeArgument), 0},
				{0, 0, 1}
			}); //Rotation matrix for perigee argument
			var Ri = new SquareMatrix(new double[,]
			{
				{1, 0, 0},
				{0, Math.Cos(orbit.Inclination), -Math.Sin(orbit.Inclination)},
				{0, Math.Sin(orbit.Inclination), Math.Cos(orbit.Inclination)},
			}); //Rotation matrix for inclination
			var ROmega = new SquareMatrix(new double[,]
			{
				{Math.Cos(orbit.AscendingNodeLongitude), -Math.Sin(orbit.AscendingNodeLongitude), 0},
				{Math.Sin(orbit.AscendingNodeLongitude), Math.Cos(orbit.AscendingNodeLongitude), 0},
				{0, 0, 1}
			}); //Rotation matrix for ascending node.
			var anomalySamples = SampleTrueAnomaly(samplesCount);
			var pointsScaled = new Vector3[samplesCount];
			for (int i = 0; i < samplesCount; i++)
			{
				var sample = anomalySamples[i];
				var r = (orbit.SemiMajorAxis * (1 - orbit.Eccentricity * orbit.Eccentricity)) /
				        (1 + orbit.Eccentricity * Math.Cos(sample));
				var xPerifocal = r * Math.Cos(sample);
				var yPerifocal = r * Math.Sin(sample);
				var posPerifocal = new Vector(xPerifocal, yPerifocal, 0);
				var posTransformed = posPerifocal;
				posTransformed *= Rw;
				posTransformed *= Ri;
				posTransformed *= ROmega;
				var posWorld = (posTransformed/kilometersPerUnit).ToVector3() + center;
				pointsScaled[i] = posWorld;
			}

			return pointsScaled;
		}

		private float[] SampleTrueAnomaly(int samplesCount)
		{
			var values = new float[samplesCount];
			var start = 0f;
			var end = Mathf.PI * 2;
			var step = (end - start) / samplesCount;
			for (int i = 0; i < samplesCount; i++)
			{
				values[i] = start + step * i;
			}

			return values;
		}
	}
}