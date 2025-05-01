using System;
using System.Collections.Generic;
using UnityEngine;

namespace Electrons.Scripts
{
    public class ElectronsSimulation : MonoBehaviour
    {
        [SerializeField] private ElectronParameters2D ControllableElectron;
        [SerializeField] private List<Target> TargetElectrons;

        private ElectronState _controllableState;
        private readonly List<ElectronState> _targetsStates = new();
        private ElectronDynamics2D _dynamics;
        
        private void Awake()
        {
            _dynamics = new ElectronDynamics2D();
            foreach (var target in TargetElectrons)
            {
                var state = target.ElectronParameters.GetInitialState();
                _targetsStates.Add(state);
            }

            _controllableState = ControllableElectron.GetInitialState();
            
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.A))
            {
                _controllableState.LeaveSpeed = -1;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                _controllableState.LeaveSpeed = 1;
            }
            else
            {
                _controllableState.LeaveSpeed = 0;
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < TargetElectrons.Count; i++)
            {
                _targetsStates[i] = UpdateElectron(_targetsStates[i], TargetElectrons[i].ElectronParameters);
                TargetElectrons[i].SetTargetState(_targetsStates[i]);
            }

            _controllableState = UpdateElectron(_controllableState, ControllableElectron);

            for (int i = 0; i < TargetElectrons.Count; i++)
            {
                TargetElectrons[i].SetCarrierState(_controllableState);
                TargetElectrons[i].UpdateRingColor();
            }
        }

        private ElectronState UpdateElectron(ElectronState state, ElectronParameters2D parameters)
        {
            var go = parameters.GetGo();
            var orbitDrawer = parameters.GetOrbitDrawer();
            var newState = _dynamics.Propagate(state, Time.fixedDeltaTime);
            go.transform.position = newState.Position.ToVector2();
            orbitDrawer.DrawOrbit(newState);
            return newState;
        }
    }
}
