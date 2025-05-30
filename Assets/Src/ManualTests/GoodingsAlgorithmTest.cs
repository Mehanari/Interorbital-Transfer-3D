using System;
using MehaMath.Math.Components;
using MehaMath.VisualisationTools;
using Src.Helpers;
using Src.LambertProblem;
using Src.OptimizationFramework;
using Src.Visualisation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Src
{
    public class GoodingsAlgorithmTest : MonoBehaviour
    {
        [SerializeField] private Button increaseTofButton;
        [SerializeField] private Button decreaseTofButton;
        [SerializeField] private TextMeshProUGUI tofTextMesh;
        [SerializeField] private TextMeshProUGUI fuelTextMesh;
        [SerializeField] private TextMeshProUGUI totalCostTextMesh;
        [SerializeField] private OrbitDrawer orbitDrawer;
        [SerializeField] private GameObject shipMarker;
        [SerializeField] private GameObject satelliteMarker;
        [SerializeField] private GameObject rendezvousMarker;
        [SerializeField] private GameObject startMarker;
        [SerializeField] private Arrow3D startDirection;
        
        //Constants
        private double KM_PER_UNIT = 1000;
        private double Mu = 398600.4418d;
        private double g0 = 9.80665; //Standard gravitational acceleration
        private double MDry = 100; //Mass of the spacecraft without fuel
        private double Isp = 300; //Engine specific impulse
        
        //Ship initial state
        private Vector R1 = new Vector(8000.0, 1000.0, 2000.0); //Initial position of the ship
        private Vector V1 = new Vector(-1.5, 7.0, 2.5); //Initial velocity of the ship
        
        //Satellite initial state
        private Vector V2 = new Vector(-5.3, -3.2, 4.1); //Satellite initial velocity
        private Vector R2 = new Vector(-5000.0, 7000.0, -3000.0); //Satellite initial position

        //Time parameters
        private double DriftTime = 1361.16524526005;
        private double TimeOfFlight =  6644.2036702756;
        private double TimeStep = 100d;
        
        
        private Vector V1t = new Vector(0, 0, 0); //Transfer orbit velocity vector on start point
        private Vector V2t = new Vector(0, 0, 0); //Transfer orbit velocity vector in the end point
        private Vector RendezvousV = new Vector(0, 0, 0);
        private Vector StartV = new Vector(0, 0, 0);


        private void Start()
        {
            //Setting up the UI
            increaseTofButton.onClick.AddListener(OnIncreaseTof);
            decreaseTofButton.onClick.AddListener(OnDecreaseTof);
            tofTextMesh.text = "Transfer time: " + TimeOfFlight + " seconds";
            
            //Placing satellite and ship markers
            var shipPos = (R1 / KM_PER_UNIT).ToVector3();
            var satellitePos = (R2 / KM_PER_UNIT).ToVector3();
            shipMarker.transform.position = shipPos;
            satelliteMarker.transform.position = satellitePos;
            
            
            //Drawing the orbits
            var startOrbit = OrbitHelper.GetOrbit(V1, R1, Mu);
            var endOrbit = OrbitHelper.GetOrbit(V2, R2, Mu);
            orbitDrawer.DrawOrbit(startOrbit, Vector3.zero, 1000, new OrbitLineParameters
            {
                LineColor = Color.yellow,
                LineWidth = 0.01f,
                Name = "Start orbit"
            });
            orbitDrawer.DrawOrbit(endOrbit, Vector3.zero, 1000, new OrbitLineParameters
            {
                LineColor = Color.cyan,
                LineWidth = 0.01f,
                Name = "End orbit"
            });
            
            UpdateTransferOrbit();
            UpdateRequiredFuelAndCost();
        }

        private void UpdateRequiredFuelAndCost()
        {
            var deltaV1 = V1t - StartV;
            var deltaV2 = V2t - RendezvousV;
            var deltaVTotal = deltaV1.Magnitude() + deltaV2.Magnitude();
            var deltaVTotalMs = deltaVTotal * 1000;
            var mMin = MDry * (Math.Exp(deltaVTotalMs / (Isp * g0)) - 1);
            var surplus = mMin * 0.2;
            var mTotal = mMin + surplus;
            fuelTextMesh.text = "Fuel required: " + Math.Round(mTotal, 2) + " kg.";
            var totalCost = mTotal + TimeOfFlight*2 + DriftTime*2 + 10*2;
            totalCostTextMesh.text = "Transfer cost: " + Math.Round(totalCost, 2) + "$";
        }

        private void OnDecreaseTof()
        {
            TimeOfFlight -= TimeStep;
            UpdateTransferOrbit();
            tofTextMesh.text = "Transfer time: " + TimeOfFlight + " seconds";
            UpdateRequiredFuelAndCost();
        }

        private void OnIncreaseTof()
        {
            TimeOfFlight += TimeStep;
            UpdateTransferOrbit();
            tofTextMesh.text = "Transfer time: " + TimeOfFlight + " seconds";
            UpdateRequiredFuelAndCost();
        }

        private void UpdateTransferOrbit()
        {
            //First: determine where satellite will be after drift and transfer times
            var satelliteStartOrbit = OrbitHelper.GetOrbit(V2, R2, Mu);
            var keplerianPropagation = new KeplerianPropagation()
            {
                CentralBodyPosition = new Vector(0, 0, 0),
                GravitationalParameter = Mu
            };
            var rendezvousOrbit = keplerianPropagation.PropagateState(satelliteStartOrbit, TimeOfFlight + DriftTime);
            var (rendezvousPosition, rendezvousVelocity) = OrbitHelper.GetPositionAndVelocity(rendezvousOrbit, Mu);
            RendezvousV = rendezvousVelocity;

            //Second: determine where the ship will be after the drift time'
            var shipStartOrbit = OrbitHelper.GetOrbit(V1, R1, Mu);
            shipStartOrbit = keplerianPropagation.PropagateState(shipStartOrbit, DriftTime);
            var (shipStartPosition, shipStartVelocity) = OrbitHelper.GetPositionAndVelocity(shipStartOrbit, Mu);
            StartV = shipStartVelocity;
            
            //Apply Gooding's algorithm
            (V1t, V2t ) = Gooding1990.FindTransfer(Mu, shipStartPosition, rendezvousPosition, TimeOfFlight, revolutions: 0, prograde: true, lowPath: true);
            
            //Place markers
            var rendezvousScenePosition = (rendezvousPosition / KM_PER_UNIT).ToVector3();
            rendezvousMarker.transform.position = rendezvousScenePosition;
            var startScenePosition = (shipStartPosition / KM_PER_UNIT).ToVector3();
            startMarker.transform.position = startScenePosition;
            
            //Placing an arrow
            startDirection.transform.position = startScenePosition;
            startDirection.SetDirection(V1t.ToVector3());
            
            var transferOrbit = OrbitHelper.GetOrbit(V1t, shipStartPosition, Mu);
            orbitDrawer.DrawOrbit(transferOrbit, Vector3.zero, 1000, new OrbitLineParameters
            {
                LineColor = Color.green,
                LineWidth = 0.01f,
                Name = "Transfer orbit"
            });
        }
    }
}
