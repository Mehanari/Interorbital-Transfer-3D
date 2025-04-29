using System;
using MehaMath.Math.Components;
using MehaMath.Math.Components.Json;
using Newtonsoft.Json;
using Src.EditorTools;
using Src.Helpers;
using Src.Model;
using Src.OrbitTransferControlGeneration;
using Src.SpacecraftDynamics;
using Src.SpacecraftDynamics.CentralBodyDynamics;
using Src.Visualisation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Src
{
	public class BakedSimulation : MonoBehaviour
	{
		[SerializeField] private SingleSpacecraftSimulationParameters parameters;
		[SerializeField] private int samplesCount;
		[SerializeField] private double timeStep;
		[SerializeField] private float minTimeScale = 1;
		[SerializeField] private float maxTimeScale = 100;
		[Header("UI")] 
		[SerializeField] private Slider timeScaleSlider;
		[SerializeField] private Button playButton;
		[SerializeField] private Slider simulationStepSlider;

		[Header("Visual features")] 
		[SerializeField] private OrbitDrawer orbitDrawer;
		[SerializeField] private GameObject flame;

		private Orbit _goalOrbit;
		private ControlData _controlData;
		
		private Vector3 _shipLocalRotation;
		private bool _isPlaying = false;
		private float _elapsedTime;
		private float _currentTimeScale;
		private PolynomialThrustControl _control;
		private ISpacecraftDynamics _dynamics;
		private Spacecraft _initialState;
		private Spacecraft[] _states;

		private float TotalTime => samplesCount * (float)timeStep;

		private void Start()
		{
			var controlIo = new JsonIO<ControlData>
			{
				FileName = "control.json"
			};
			var orbitIo = new JsonIO<Orbit>
			{
				FileName = "goalOrbit.json"
			};
			var spacecraftIo = new JsonIO<Spacecraft>
			{
				Converters = new JsonConverter[] { new VectorJsonConverter() },
				FileName = "spacecraft.json"
			};
			var spacecraft = spacecraftIo.Load();
			_goalOrbit = orbitIo.Load();
			_controlData = controlIo.Load();
			var currentOrbit = OrbitHelper.GetOrbit(spacecraft.Velocity, spacecraft.Position,
				parameters.GravitationalParameter);
			currentOrbit.TrueAnomaly = _controlData.IgnitionTrueAnomaly;
			var (startPosition, startVelocity) =
				OrbitHelper.GetPositionAndVelocity(currentOrbit, parameters.GravitationalParameter);
			spacecraft.Velocity = startVelocity;
			spacecraft.Position = startPosition;
			
			_shipLocalRotation = parameters.SpacecraftGo.transform.eulerAngles;
			_currentTimeScale = minTimeScale;
			_states = new Spacecraft[samplesCount];
			_initialState = spacecraft;
			
			_control = new PolynomialThrustControl
			(
				alphaPolynomial: new Polynomial(_controlData.AlphaPolynomialCoefficients),
				betaPolynomial: new Polynomial(_controlData.BetaPolynomialCoefficients),
				gammaPolynomial: new Polynomial(_controlData.GammaPolynomialCoefficients)
			);
			_dynamics = new Rkf45Dynamics()
			{
				GravitationalParameter = parameters.GravitationalParameter,
				CentralBodyPosition = new Vector(parameters.EarthGo.transform.position * (float)parameters.KilometersPerUnit)
			};
			DrawOrbits();
			SetUpUI();
			CalculateStates();
		}

		private void DrawOrbits()
		{
			//Drawing start orbit
			var orbit = OrbitHelper.GetOrbit(_initialState.Velocity, _initialState.Position,
				parameters.GravitationalParameter);
			orbitDrawer.DrawOrbit(orbit, parameters.EarthGo.transform.position, 1000, new OrbitLineParameters
			{
				Name = "Start orbit",
				LineColor = Color.red,
				LineWidth = 0.01f
			});
			
			//Drawing the goal orbit
			orbitDrawer.DrawOrbit(_goalOrbit, parameters.EarthGo.transform.position, 1000, new OrbitLineParameters
			{
				Name = "Goal orbit",
				LineColor = Color.green,
				LineWidth = 0.01f
			});
		}

		private void Update()
		{
			if(!_isPlaying) return;

			_elapsedTime += Time.deltaTime * _currentTimeScale;
			var index = TimeToIndex(_elapsedTime);
			var state = _states[index];
			UpdateView(state);
		}

		private void UpdateView(Spacecraft state)
		{
			Vector3 newVelocity = state.Velocity.ToVector3();
			var thrustDirection = (state.ExhaustDirection * (-1)).ToVector3();

			if (state.FuelConsumptionRate <= 0)
			{
				flame.transform.localScale = Vector3.zero;
			}
			else
			{
				flame.transform.localScale = Vector3.one;
			}
			_shipLocalRotation = ToEuler(thrustDirection);
			parameters.SpacecraftGo.transform.eulerAngles = _shipLocalRotation;
			parameters.SpacecraftGo.transform.position = (state.Position / parameters.KilometersPerUnit).ToVector3();
		}

		private Vector3 ToEuler(Vector3 direction)
		{
;
			var angle = Vector3.Angle(Vector3.forward, direction);
			var axis = Vector3.Cross(Vector3.forward, direction).normalized;
			var rotation = Quaternion.AngleAxis(angle, axis);
			var euler = rotation.eulerAngles;
			return euler;
		}

		private void CalculateStates()
		{
			_states[0] = _initialState;
			var elapsedTime = 0d;
			for (int i = 1; i < samplesCount; i++)
			{
				var current = _states[i - 1];
				if (elapsedTime < _controlData.BurnTime)
				{
					current.FuelConsumptionRate = _control.FuelConsumptionRatePercent(timeStep * (i-1));
					current.ExhaustDirection = _control.ThrustDirection(timeStep * (i - 1)) * -1;
				}
				else
				{
					current.FuelConsumptionRate = 0;
				}
				
				var nextState = _dynamics.PropagateState(current, timeStep);
				_states[i] = nextState;
				elapsedTime += timeStep;
			}
		}

		private void SetUpUI()
		{
			//Time scale
			timeScaleSlider.minValue = (float) minTimeScale;
			timeScaleSlider.maxValue = (float)maxTimeScale;
			timeScaleSlider.onValueChanged.AddListener(OnTimeScaleChanged);
			
			//Play button
			playButton.onClick.AddListener(OnPlay);
			
			//Step slider
			simulationStepSlider.minValue = 0;
			simulationStepSlider.maxValue = samplesCount - 1;
			simulationStepSlider.wholeNumbers = true;
			simulationStepSlider.onValueChanged.AddListener(OnStepChanged);
		}

		private void OnStepChanged(float index)
		{
			_isPlaying = false;
			var indexInt = (int) Math.Clamp((int)index, 0, samplesCount - 1);
			_elapsedTime = IndexToTime(indexInt);
			var state = _states[indexInt];
			UpdateView(state);
		}

		private void OnPlay()
		{
			_isPlaying = true;
		}

		private void OnTimeScaleChanged(float scale)
		{
			_currentTimeScale = scale;
		}

		private int TimeToIndex(float elapsedTime)
		{
			var percent = Mathf.Lerp(0, 1, elapsedTime / TotalTime);
			var index = (int) (percent * samplesCount);
			index = Math.Clamp(index, 0, samplesCount - 1);
			return index;
		}

		private float IndexToTime(int index)
		{
			var percent = (float)index / (float)samplesCount;
			var time = percent * TotalTime;
			time = Mathf.Clamp(time, 0, TotalTime);
			return time;
		}
	}
}