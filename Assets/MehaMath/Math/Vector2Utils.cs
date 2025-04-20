using UnityEngine;

namespace MehaMath.Math
{
    public abstract class Vector2Utils
    {
        public static Vector2 Rotate(Vector2 v, float delta) {
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }

        public static float MinAngleRad(Vector2 a, Vector2 b)
        {
            var cos = Vector2.Dot(a, b) / (a.magnitude * b.magnitude);
            return Mathf.Acos(cos);
        }
    }
}