using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Src.Visualisation
{
	public struct TrajectoryCalculationJob : IJob
	{
		public Vector3 earthPosition;
		public float gravitationalParameter;
		public Vector3 initialPosition;
		public Vector3 initialVelocity;
		public float deltaTime;
		public NativeArray<Vector3> results;
        
		public void Execute()
		{
			var currentPosition = initialPosition;
			var currentVelocity = initialVelocity;

			for (int i = 0; i < results.Length; i++)
			{
				(currentPosition, currentVelocity) = SimulateNextState(currentPosition, currentVelocity);
				results[i] = currentPosition;
			}
		}

		private (Vector3 position, Vector3 velocity) SimulateNextState(Vector3 spacecraftPosition, Vector3 spacecraftVelocity)
		{
			//Beg - beginning
			//Acc - acceleration
			//Mod - module
			var displaceBeg = earthPosition - spacecraftPosition;
			var accModBeg = gravitationalParameter / (displaceBeg).sqrMagnitude;
			var accBeg = displaceBeg.normalized * accModBeg;
			var pos1 = spacecraftPosition + spacecraftVelocity * deltaTime;
			var accModEnd = gravitationalParameter/ (earthPosition - pos1).sqrMagnitude;
			var displaceEnd = earthPosition - pos1;
			var accEnd = displaceEnd.normalized * accModEnd;
			var accSpeed = (accEnd - accBeg) / deltaTime;

			var newVelocity = spacecraftVelocity + accBeg * deltaTime + accSpeed * deltaTime * deltaTime / 2;
			var newPosition = spacecraftPosition + spacecraftVelocity * deltaTime +
			                  accBeg * deltaTime * deltaTime / 2 + accSpeed * deltaTime * deltaTime * deltaTime / 6;

			return (newPosition, newVelocity);
		}
	}
}