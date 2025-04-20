using System;
using MehaMath.Math.Components;
using Src.Helpers;
using Src.Model;
using Src.Visualisation;
using UnityEngine;
using UnityEngine.UI;

namespace Src
{
	public class OrbitsTest : MonoBehaviour
	{
		[Header("Model parameters")]
		[SerializeField] private GameObject earth;
		[SerializeField] private GameObject spacecraftModel;
		[SerializeField] private double earthMass;
		[SerializeField] private double gravitationalConstant;
		[SerializeField] private Vector3 spacecraftInitialVelocity;
		[SerializeField] private double spacecraftMass;
		[SerializeField] private double fuelMass;
		[SerializeField] private Vector3 exhaustDirection;
		[SerializeField] private double exhaustVelocityModule;
		[SerializeField] private double fuelConsumptionRate;

		[Header("Orbit visualisation parameters")] 
		[SerializeField] private OrbitDrawer orbitDrawer;
		[SerializeField] private Slider slider;

		private Orbit _orbit;
		
		private void Start()
		{
			var spacecraft = new Spacecraft()
			{
				Velocity = new Vector(spacecraftInitialVelocity),
				Position = new Vector(spacecraftModel.transform.position),
				FuelMass = fuelMass,
				FuelConsumptionRate = fuelConsumptionRate,
				Mass = spacecraftMass,
				ExhaustDirection = new Vector(exhaustDirection),
				ExhaustVelocityModule = exhaustVelocityModule
			};
			var mu = earthMass * gravitationalConstant;
			var orbit1 = OrbitHelper.GetOrbit(spacecraft.Velocity, spacecraft.Position,
				mu);
			var (position, velocity) = OrbitHelper.GetPositionAndVelocity(orbit1, mu);
			var orbit2 = OrbitHelper.GetOrbit(velocity, position, mu);
			_orbit = orbit1;
			orbitDrawer.DrawOrbit(orbit1, earth.transform.position, 1000, new OrbitLineParameters
			{
				LineColor = Color.red,
				LineWidth = .01f,
				Name = "Orbit1"
			});
			orbitDrawer.DrawOrbit(orbit2, earth.transform.position, 1000, new OrbitLineParameters
			{
				LineColor = Color.blue,
				LineWidth = .01f,
				Name = "Orbit2"
			});
			slider.minValue = 0f;
			slider.maxValue = Mathf.PI * 2;
			slider.onValueChanged.AddListener(OnValueChanged);
		}

		private void OnValueChanged(float val)
		{
			_orbit.TrueAnomaly = val;
			var (pos, _) = OrbitHelper.GetPositionAndVelocity(_orbit, gravitationalConstant * earthMass);
			spacecraftModel.transform.position = pos.ToVector3();
		}
	} 
}