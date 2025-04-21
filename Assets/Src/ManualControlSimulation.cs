using MehaMath.Math.Components;
using Src.Helpers;
using Src.Model;
using Src.SpacecraftDynamics;
using Src.SpacecraftDynamics.CentralBodyDynamics;
using Src.Visualisation;
using UnityEngine;
using UnityEngine.UI;

namespace Src
{
	public class ManualControlSimulation : MonoBehaviour
	{
		[SerializeField] private SimulationParametersEditor simulationParameters;
		[Tooltip("How to convert Unity's time into simulation time. Use it to speed up the simulation")]
		[SerializeField] private float timeStepMultiplier;
		[SerializeField] private GameObject spacecraftNose;
		[Header("Control parameters")] 
		[SerializeField] private double maxFuelConsumptionKgS;
		[SerializeField] private double fuelConsumptionChangeSpeed;
		[SerializeField] private double rotationSpeedModule;
		[Header("Visualisation parameters")]
		[SerializeField] private TrajectoryRenderer trajectoryRenderer;
		[SerializeField] private OrbitDrawer orbitDrawer;
		[SerializeField] private Image fuelPercentage;
		[SerializeField] private GameObject flame;
		[Header("UI")] 
		[SerializeField] private Button snapshotOrbit;
		private SatelliteModel _model;
		private ISpacecraftDynamics _spacecraftDynamics;
		private double _initialFuelMass;
		private Vector3 _shipRotation;
		private JsonIO<Orbit> _orbitIo;

		private void Start()
		{
			var spacecraft = new Spacecraft()
			{
				Velocity = new Vector(simulationParameters.SpacecraftInitialVelocityKmS),
				Position = new Vector(simulationParameters.SpacecraftGo.transform.position*(float)simulationParameters.KilometersPerUnit),
				FuelMass = simulationParameters.FuelMassKg,
				FuelConsumptionRate = simulationParameters.FuelConsumptionRateKgS,
				Mass = simulationParameters.SpacecraftMassKg,
				ExhaustDirection = new Vector(simulationParameters.ExhaustDirection),
				ExhaustVelocityModule = simulationParameters.ExhaustVelocityModuleMs,
				ExhaustVelocityConversionRate = 1000
			};
			_model = new SatelliteModel
			{
				Spacecraft = spacecraft,
				EarthPosition = new Vector(simulationParameters.EarthGo.transform.position*(float)simulationParameters.KilometersPerUnit),
				GravitationalParameter = simulationParameters.GravitationalParameter
			};
			_spacecraftDynamics = new Rkf45Dynamics()
			{
				GravitationalParameter = simulationParameters.GravitationalParameter,
				CentralBodyPosition = new Vector(simulationParameters.EarthGo.transform.position * (float)simulationParameters.KilometersPerUnit)
			};
			trajectoryRenderer.SetModel(_model);

			_shipRotation = simulationParameters.SpacecraftGo.transform.eulerAngles;
			_initialFuelMass = simulationParameters.FuelMassKg;
			_orbitIo = new JsonIO<Orbit>
			{
				FileName = "goalOrbit.json"
			};
			var startOrbit = OrbitHelper.GetOrbit(spacecraft.Velocity, spacecraft.Position,
				simulationParameters.GravitationalParameter);
			orbitDrawer.DrawOrbit(startOrbit, simulationParameters.EarthGo.transform.position, 1000, new OrbitLineParameters
			{
				Name = "Start Orbit",
				LineColor = Color.red,
				LineWidth = 0.01f
			});
			snapshotOrbit.onClick.AddListener(OnOrbitSnapshot);
		}

		private void OnOrbitSnapshot()
		{
			var spacecraft = _model.Spacecraft;
			var orbit = OrbitHelper.GetOrbit(spacecraft.Velocity, spacecraft.Position,
				simulationParameters.GravitationalParameter);
			_orbitIo.Save(orbit);
			orbitDrawer.DrawOrbit(orbit, simulationParameters.EarthGo.transform.position, 1000, new OrbitLineParameters
			{
				Name = "Goal orbit",
				LineColor = Color.green,
				LineWidth = 0.01f
			});
		}

		private void Update()
		{
			HandleRotationInputs();
			HandleFuelConsumptionRateInputs();
			UpdateExhaustDirection();
			
			var newState = _spacecraftDynamics.PropagateState(_model.Spacecraft, Time.deltaTime*timeStepMultiplier);
			_model.Spacecraft = newState;
			simulationParameters.SpacecraftGo.transform.position = (newState.Position/simulationParameters.KilometersPerUnit).ToVector3();
			
			UpdateSpacecraftModelDirection(newState);
			UpdateVisualEffects();
		}

		private void UpdateVisualEffects()
		{
			var newState = _model.Spacecraft;
			if (_initialFuelMass > 0)
			{
				fuelPercentage.fillAmount = (float)(newState.FuelMass / _initialFuelMass);
			}
			else
			{
				fuelPercentage.fillAmount = 0f;
			}

			// Flame scale (y-component: 0.5 at 0 fuel consumption, 2 at maxFuelConsumptionKgS)
			float flameScaleY = Mathf.Lerp(0.5f, 2f, (float)(newState.FuelConsumptionRate / maxFuelConsumptionKgS));
			if (maxFuelConsumptionKgS == 0 || newState.FuelMass <= 0)
			{
				flameScaleY = 0f;
			}
			Vector3 flameScale = flame.transform.localScale;
			flameScale.y = flameScaleY;
			flame.transform.localScale = flameScale;
		}

		private void HandleRotationInputs()
		{
			float rotationSpeed = (float)rotationSpeedModule * Time.deltaTime;
			var currentRotation = simulationParameters.SpacecraftGo.transform.eulerAngles;
			simulationParameters.SpacecraftGo.transform.eulerAngles = _shipRotation;
			if (Input.GetKey(KeyCode.Q))
			{
				simulationParameters.SpacecraftGo.transform.Rotate(Vector3.forward, rotationSpeed, Space.Self); // Roll left
			}
			if (Input.GetKey(KeyCode.E))
			{
				simulationParameters.SpacecraftGo.transform.Rotate(Vector3.forward, -rotationSpeed, Space.Self); // Roll right
			}
			if (Input.GetKey(KeyCode.W))
			{
				simulationParameters.SpacecraftGo.transform.Rotate(Vector3.right, rotationSpeed, Space.Self); // Pitch forward
			}
			if (Input.GetKey(KeyCode.S))
			{
				simulationParameters.SpacecraftGo.transform.Rotate(Vector3.right, -rotationSpeed, Space.Self); // Pitch backward
			}
			if (Input.GetKey(KeyCode.A))
			{
				simulationParameters.SpacecraftGo.transform.Rotate(Vector3.up, -rotationSpeed, Space.Self); // Yaw left
			}
			if (Input.GetKey(KeyCode.D))
			{
				simulationParameters.SpacecraftGo.transform.Rotate(Vector3.up, rotationSpeed, Space.Self); // Yaw right
			}

			_shipRotation = simulationParameters.SpacecraftGo.transform.eulerAngles;
			simulationParameters.SpacecraftGo.transform.eulerAngles = currentRotation;
		}

		private void HandleFuelConsumptionRateInputs()
		{
			double fuelRateChange = fuelConsumptionChangeSpeed * Time.deltaTime;
			var spacecraft = _model.Spacecraft;
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				spacecraft.FuelConsumptionRate += fuelRateChange;
			}
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				spacecraft.FuelConsumptionRate -= fuelRateChange;
			}
			spacecraft.FuelConsumptionRate = Mathf.Clamp((float)spacecraft.FuelConsumptionRate, 0f, (float)maxFuelConsumptionKgS);
			_model.Spacecraft = spacecraft;
		}

		private void UpdateExhaustDirection()
		{
			// Update exhaust direction based on spacecraftGo to spacecraftNose vector
			var spacecraft= _model.Spacecraft;
			Vector3 noseDirection = (spacecraftNose.transform.position - simulationParameters.SpacecraftGo.transform.position).normalized;
			spacecraft.ExhaustDirection = new Vector(-noseDirection.x, -noseDirection.y, -noseDirection.z).Normalized();
			_model.Spacecraft = spacecraft;
		}
		
		private void UpdateSpacecraftModelDirection(Spacecraft newState)
		{
			Vector3 newVelocity = newState.Velocity.ToVector3();
			
			var angle = Vector3.Angle(Vector3.forward, newVelocity);
			var axis = Vector3.Cross(Vector3.forward, newVelocity).normalized;
			var rotation = Quaternion.AngleAxis(angle, axis);
			var euler = rotation.eulerAngles;
			simulationParameters.SpacecraftGo.transform.eulerAngles = euler + _shipRotation;
		}

		private void OnDrawGizmos()
		{
			if (simulationParameters.SpacecraftGo is null)
			{
				return;
			}

			var spacecraftPosition = simulationParameters.SpacecraftGo.transform.position;
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(spacecraftPosition, spacecraftPosition + simulationParameters.SpacecraftInitialVelocityKmS);
			Gizmos.color =Color.red;
			Gizmos.DrawLine(spacecraftPosition, spacecraftPosition + simulationParameters.ExhaustDirection);
		}
	}
}