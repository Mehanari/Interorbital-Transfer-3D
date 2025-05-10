using System;
using MehaMath.Math.Components;
using Src.Helpers;
using Src.LambertProblem;
using Src.Visualisation;
using UnityEngine;

namespace Src
{
    public class GoodingsAlgorithmTest : MonoBehaviour
    {
        [SerializeField] private OrbitDrawer orbitDrawer;
        [SerializeField] private GameObject startMarker;
        [SerializeField] private GameObject endMarker;
        
        private double KM_PER_UNIT = 1000;
        private double mu = 398600.4375;
        
        private void Start()
        {
            var startPos = new Vector(7000, 0, 0);
            var startVelocity = new Vector(0, 0, 7.612);
            var startUnity = (startPos / KM_PER_UNIT).ToVector3();
            startMarker.transform.position = startUnity;
            var startOrbit = OrbitHelper.GetOrbit(startVelocity, startPos, mu);
            orbitDrawer.DrawOrbit(startOrbit, Vector3.zero, 1000, new OrbitLineParameters
            {
                LineColor = Color.yellow,
                LineWidth = 0.01f,
                Name = "Start"
            });

            var endPos = new Vector(7400, 0, 0);
            var endVelocity = new Vector(0, 0, 7.612);
            var endUnity = (endPos / KM_PER_UNIT).ToVector3();
            endMarker.transform.position = endUnity;
            var endOrbit = OrbitHelper.GetOrbit(endVelocity, endPos, mu);
            orbitDrawer.DrawOrbit(endOrbit, Vector3.zero, 1000, new OrbitLineParameters
            {
                LineColor = Color.cyan,
                LineWidth = 0.01f,
                Name = "End"
            });

            var endTrueAnomaly = Math.PI / 4;
            endOrbit.TrueAnomaly = endTrueAnomaly;
            (endPos, _) = OrbitHelper.GetPositionAndVelocity(endOrbit, mu);
            endUnity = (endPos / KM_PER_UNIT).ToVector3();
            endMarker.transform.position = endUnity;

            var goodingSolver = new GoodingSolver()
            {
                LongPath = false,
                GravitationalParameter = mu,
                CentralBodyRadius = 6500
            };
            goodingSolver.CalculateTransfer(startPos, endPos, 3600, out var transferVelocityStart,
                out var transferVelocityEnd);
        }
        
    }
}
