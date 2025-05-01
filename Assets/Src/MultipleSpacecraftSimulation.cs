using System;
using MehaMath.Math.Components;
using MehaMath.Math.Components.Json;
using Newtonsoft.Json;
using Src.EditorTools;
using Src.Helpers;
using Src.Model;
using Src.SpacecraftDynamics;
using Src.SpacecraftDynamics.CentralBodyDynamics;
using Src.Visualisation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ColorUtility = Src.Helpers.ColorUtility;

namespace Src
{
	public class MultipleSpacecraftSimulation : MonoBehaviour
	{
		private class SpacecraftData
		{
			public Vector3 LocalRotation { get; set; }
			public Spacecraft InitialState { get; set; }
			public Spacecraft[] States { get; set; }
			public GameObject Go { get; set; }
		}
		
		[SerializeField] private MultiSpacecraftsSimulationParameters parameters;
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
		
		private bool _isPlaying = false;
		private float _elapsedTime;
		private float _currentTimeScale;
		private ISpacecraftDynamics _dynamics;

		private SpacecraftData[] _spacecrafts;

		private float TotalTime => samplesCount * (float)timeStep;

		private void Start()
		{
			var spacecraftIo = new JsonIO<Spacecraft>()
			{
				Converters = new JsonConverter[]{new VectorJsonConverter()}
			};
			
			_currentTimeScale = minTimeScale;
			_dynamics = new Rkf45Dynamics()
			{
				GravitationalParameter = parameters.GravitationalParameter,
				CentralBodyPosition = new Vector(parameters.EarthGo.transform.position * (float)parameters.KilometersPerUnit)
			};

			_spacecrafts = new SpacecraftData[parameters.SpacecraftsParameters.Count];
			for (int i = 0; i < parameters.SpacecraftsParameters.Count; i++)
			{
				var spacecraftParameters = parameters.SpacecraftsParameters[i];
				var spacecraft = spacecraftParameters.GetSpacecraft(parameters.KilometersPerUnit);
				if (i == 0)
				{
					spacecraftIo.FileName = "Rendezvous/carrier.json";
					spacecraftIo.Save(spacecraft);
				}
				else
				{
					spacecraftIo.FileName = "Rendezvous/satellite.json";
					spacecraftIo.Save(spacecraft);
				}
				_spacecrafts[i] = new SpacecraftData
				{
					LocalRotation = spacecraftParameters.SpacecraftGo.transform.eulerAngles,
					Go = spacecraftParameters.SpacecraftGo,
					InitialState = spacecraft
				};
			}
			
			SetUpUI();
			DrawOrbits();
			CalculateStates();
		}

		private void DrawOrbits()
		{
			for (int i = 0; i < _spacecrafts.Length; i++)
			{
				var spacecraft = _spacecrafts[i];
				var initialState = spacecraft.InitialState;
				var orbit = OrbitHelper.GetOrbit(initialState.Velocity, initialState.Position,
					parameters.GravitationalParameter);
				orbitDrawer.DrawOrbit(orbit, parameters.EarthGo.transform.position, 1000, new OrbitLineParameters
				{
					Name = "Spacecraft #" + (i+1) + " orbit",
					LineColor = ColorUtility.GetRandomBrightColor(),
					LineWidth = 0.01f
				});
			}
		}

		private void Update()
		{
			if(!_isPlaying) return;

			_elapsedTime += Time.deltaTime * _currentTimeScale;
			var index = TimeToIndex(_elapsedTime);
			UpdateView(index);
		}

		private void UpdateView(int stateIndex)
		{
			for (int i = 0; i < _spacecrafts.Length; i++)
			{
				var spacecraft = _spacecrafts[i];
				var state = spacecraft.States[stateIndex];
				Vector3 newVelocity = state.Velocity.ToVector3();
				var shipLocalRotation = spacecraft.LocalRotation;
				shipLocalRotation = ToEuler(state.ExhaustDirection.ToVector3());
				spacecraft.Go.transform.eulerAngles = ToEuler(newVelocity) + shipLocalRotation;
				spacecraft.Go.transform.position = (state.Position / parameters.KilometersPerUnit).ToVector3();
			}
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
			for (int i = 0; i < _spacecrafts.Length; i++)
			{
				var spacecraft = _spacecrafts[i];
				var initialState = spacecraft.InitialState;
				spacecraft.States = new Spacecraft[samplesCount];
				spacecraft.States[0] = initialState;
				for (int j = 1; j < samplesCount; j++)
				{
					var current = spacecraft.States[j - 1];
					var nextState = _dynamics.PropagateState(current, timeStep);
					spacecraft.States[j] = nextState;
				}
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
			UpdateView(indexInt);
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