using MehaMath.Math.Components;
using MehaMath.Math.Components.Json;
using Newtonsoft.Json;
using Src.Model;
using Src.SpacecraftDynamics.CentralBodyDynamics;
using UnityEngine;

namespace Src.ManualTests
{
	public class PropagationTest : MonoBehaviour
	{
		private void Start()
		{
			var spacecraftIo = new JsonIO<Spacecraft>
			{
				Converters = new JsonConverter[] { new VectorJsonConverter() },
				FileName = "spacecraft.json"
			};
			var spacecraft = spacecraftIo.Load();

			var timePeriod = 10d;
			var timeStep = 0.1d;

			var centralBodyPosition = new Vector(0, 0, 0);
			var gravitationalParameter = 398600.4418;
			var rkfDynamics = new Rkf45Dynamics()
			{
				CentralBodyPosition = centralBodyPosition,
				GravitationalParameter = gravitationalParameter
			};
			var keplerianDynamics = new KeplerianDynamics()
			{
				CentralBodyPosition = centralBodyPosition,
				GravitationalParameter = gravitationalParameter
			};

			var rkfState = spacecraft.Clone();
			var elapsedTime = 0d;
			while (elapsedTime <= timePeriod)
			{
				rkfState = rkfDynamics.PropagateState(rkfState, timeStep);
				elapsedTime += timeStep;
			}
			Debug.Log("Final elapsed time: " + elapsedTime);

			var keplerianState = keplerianDynamics.PropagateState(spacecraft.Clone(), elapsedTime);

			var velocityDiff = rkfState.Velocity - keplerianState.Velocity;
			var posDiff = rkfState.Position - keplerianState.Position;
			Debug.Log("Velocity difference: " + velocityDiff);
			Debug.Log("Position difference: " + posDiff);
		}
	}
}