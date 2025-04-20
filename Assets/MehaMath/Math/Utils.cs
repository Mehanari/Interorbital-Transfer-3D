using System;
using MehaMath.Math.Components;
using UnityEngine;

namespace MehaMath.Math
{
    public static class Utils
    {
        /// <summary>
        /// Convert a function that is handy for Unity into a function that is handy for math algorithms.
        /// </summary>
        /// <param name="floatFunc"></param>
        /// <returns></returns>
        public static Func<Vector, double> ToDoubleFunc(Func<Vector2, float> floatFunc)
        {
            return (vector) =>
            {
                return floatFunc(vector.ToVector2());
            };
        }

        public static Func<Vector, double> ToDoubleFunc(Func<float, float> floatFunc)
        {
            return (vector) =>
            {
                return floatFunc((float)vector[0]);
            };
        }
    }
}