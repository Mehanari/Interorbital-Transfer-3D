using Newtonsoft.Json;
using UnityEngine;

namespace MehaMath.Math.OptimalControlProblems
{
    /// <summary>
    /// This is a control which for each moment in time produces a control input
    /// equal to linear interpolation between the closest control samples for a given moment.
    /// If time given in ControlInput is out of control time limit, the produced input will be 0f.
    /// </summary>
    public class LerpControl : Control
    {
        public double Time { get; }

        private readonly double[] _controlSamples;

        private double TimeStep => Time / _controlSamples.Length;

        public LerpControl(double time, double[] controlSamples)
        {
            Time = time;
            _controlSamples = controlSamples;
        }

        /// <summary>
        /// If given time is between 0 and Time, this method will return a linear control interpolation from given control samples.
        /// Otherwise returns 0f.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override double ControlInput(double time)
        {
            var normalized = time / Time;
            if (normalized > 1 || normalized < 0)
            {
                return 0f;
            }

            var lIndex = (int)System.Math.Floor(_controlSamples.Length * normalized);
            var rIndex = (int)System.Math.Ceiling(_controlSamples.Length * normalized);
            //For the case of double error
            if (rIndex >= _controlSamples.Length)
            {
                rIndex = _controlSamples.Length - 1;
            }
            if (lIndex < 0)
            {
                lIndex = 0;
            }

            if (lIndex == rIndex)
            {
                return _controlSamples[lIndex];
            }

            var lTime = TimeStep * lIndex;
            var rTime = TimeStep * rIndex;
            var alpha = (time - lTime) / (rTime - lTime);
            var lVal = _controlSamples[lIndex];
            var rVal = _controlSamples[rIndex];
            var interpolated = alpha*(rVal - lVal) + lVal;
            if (interpolated is double.NaN)
            {
                Debug.Log("Wtf");
            }

            return interpolated;
        }

        public override string ToJson()
        {
            var data = new DiscreteControlData()
            {
                Time = (float)Time,
                ControlSamples = _controlSamples
            };
            return JsonConvert.SerializeObject(data);
        }
    }
}