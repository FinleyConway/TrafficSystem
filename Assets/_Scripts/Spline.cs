using UnityEngine;

namespace TrafficSystem
{
    public static class Spline
    {
        /// <summary>
        /// Calculates a quadratic Bezier curve between 3 points.
        /// </summary>
        /// <returns>The interpolated point.</returns>
        public static Vector3 QuadraticLerp(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            Vector3 ab = Vector3.Lerp(a, b, t);
            Vector3 bc = Vector3.Lerp(b, c, t);
            return Vector3.Lerp(ab, bc, t);
        }

        /// <summary>
        /// Calculates a cubic Bezier curve between 4 points.
        /// </summary>
        /// <returns>The interpolated point.</returns>
        public static Vector3 CubicLerp(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            Vector3 abbc = QuadraticLerp(a, b, c, t);
            Vector3 bccd = QuadraticLerp(b, c, d, t);
            return Vector3.Lerp(abbc, bccd, t);
        }
    }
}