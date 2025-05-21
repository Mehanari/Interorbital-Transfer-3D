using Src.FinalComponents;
using Src.Model;
using UnityEngine;

namespace Src.Visualisation
{
	public class OrbitDrawingTest : MonoBehaviour
	{
		[SerializeField] private int samplesCount = 1000;
		[SerializeField] private Orbit orbit;
		[SerializeField] private OrbitDrawer drawer;

		private void OnValidate()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (drawer is null)
			{
				return;
			}
			drawer.DrawOrbit(orbit, Vector3.zero, samplesCount, new OrbitLineParameters
			{
				Name = "Orbit",
				LineColor = Color.blue,
				LineWidth = 0.1f
			});
		}
	}
}