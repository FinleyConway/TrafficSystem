using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    public class SplinePath : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] private float m_GizmoSize = 0.25f;
        [SerializeField] private Vector3 m_HandleOffsetA = new Vector3(1f, 0, 0.5f);
        [SerializeField] private Vector3 m_HandleOffsetB = new Vector3(1f, 0, -0.5f);
        [SerializeField] private Vector3 m_Normal = new Vector3(0, 0, -1);
        [SerializeField] private bool m_IsLoopClosed;
        [SerializeField] private List<Anchor> m_Anchors = new List<Anchor>();

        private List<Point> m_Points;
        private float m_SplineLength;
        private float m_PointAmountInCurve;
        private float m_PointAmountPerUnitInCurve = 2f;

        public event Action OnDirty;

        private void Awake()
        {
            m_SplineLength = GetSplineLength();
            SetupPointList();
        }

        /// <summary>
        /// Returns the position on the spline based on time
        /// </summary>
        /// <param name="time">Value used to interpolate between a and b.</param>
        /// <returns>The position on spline based on time.</returns>
        public Vector3 GetPositionAt(float time)
        {
            // if at the end of the spline
            if (time == 1)
            {
                Anchor anchorA;
                Anchor anchorB;

                if (m_IsLoopClosed)
                {
                    // last position and first position for closed loop
                    anchorA = m_Anchors[m_Anchors.Count - 1];
                    anchorB = m_Anchors[0];
                }
                else
                {
                    // second last and last position for open spline
                    anchorA = m_Anchors[m_Anchors.Count - 2];
                    anchorB = m_Anchors[m_Anchors.Count - 1];
                }

                return transform.position + Spline.CubicLerp(anchorA.Position, anchorA.HandleBPosition, anchorB.HandleAPosition, anchorB.Position, time);
            }
            else
            {
                int addClosedLoop = m_IsLoopClosed ? 1 : 0;

                // calculate the index of the two anchors to interpolate between
                float tFull = time * (m_Anchors.Count - 1 + addClosedLoop);
                int anchorIndex = Mathf.FloorToInt(tFull);
                float tAnchor = tFull - anchorIndex;

                Anchor anchorA;
                Anchor anchorB;

                if (anchorIndex < m_Anchors.Count - 1)
                {
                    // get the two anchors to interpolate between
                    anchorA = m_Anchors[anchorIndex + 0];
                    anchorB = m_Anchors[anchorIndex + 1];
                }
                else
                {
                    // if at the end of the spline, either link to "next" one or loop back to the first
                    if (m_IsLoopClosed)
                    {
                        anchorA = m_Anchors[m_Anchors.Count - 1];
                        anchorB = m_Anchors[0];
                    }
                    else
                    {
                        anchorA = m_Anchors[anchorIndex - 1];
                        anchorB = m_Anchors[anchorIndex + 0];
                        tAnchor = 1f;
                    }
                }

                return transform.position + Spline.CubicLerp(anchorA.Position, anchorA.HandleBPosition, anchorB.HandleAPosition, anchorB.Position, tAnchor);
            }
        }

        public Vector3 GetForwardAt(float time)
        {
            Point pointA = GetPreviousPoint(time);
            int pointBIndex = (m_Points.IndexOf(pointA) + 1) % m_Points.Count;
            Point pointB = m_Points[pointBIndex];

            return Vector3.Lerp(pointA.Forward, pointB.Forward, (time - pointA.Time) / Mathf.Abs(pointA.Time - pointB.Time));
        }

        public Point GetPreviousPoint(float time)
        {
            int previousIndex = 0;
            for (int i = 1; i < m_Points.Count; i++)
            {
                Point point = m_Points[i];
                if (time < point.Time)
                {
                    return m_Points[previousIndex];
                }
                else
                {
                    previousIndex++;
                }
            }
            return m_Points[previousIndex];
        }

        /// <summary>
        /// Returns the position on the spline at a specified distance along the curve.
        /// </summary>
        /// <param name="unitDistance">The distance in units to sample the spline at.</param>
        /// <param name="stepSize">The step size used when sampling the spline. A smaller step size will result in a more accurate distance calculation but will be slower.</param>
        /// <returns>The position on the spline at the specified distance.</returns>
        public Vector3 GetPositionAtUnits(float unitDistance, float stepSize = 0.01f)
        {
            float splineUnitDistance = 0f;
            Vector3 lastPosition = GetPositionAt(0f);
            float incrementAmount = stepSize;

            // iterate over the spline, moving incrementally along it
            for (float t = 0; t < 1f; t += incrementAmount)
            {
                // update spline unit distance and last position
                splineUnitDistance += Vector3.Distance(lastPosition, GetPositionAt(t));
                lastPosition = GetPositionAt(t);

                // check if target unit distance has been reached
                if (splineUnitDistance >= unitDistance)
                {
                    // calculate the position at the target unit distance along the spline
                    Vector3 direction = (GetPositionAt(t) - GetPositionAt(t - incrementAmount)).normalized;
                    return GetPositionAt(t) + direction * (unitDistance - splineUnitDistance);
                }
            }

            // if the target unit distance was not reached, interpolate along the entire spline
            Anchor anchorA = m_Anchors[0];
            Anchor anchorB = m_Anchors[1];
            return Spline.CubicLerp(anchorA.Position, anchorA.HandleBPosition, anchorB.HandleAPosition, anchorB.Position, unitDistance / m_SplineLength);
        }

        public Vector3 GetForwardAtUnits(float unitDistance, float stepSize = 0.01f)
        {
            float splineUnitDistance = 0f;
            Vector3 lastPosition = GetPositionAt(0f);

            float incrementAmount = stepSize;
            for (float t = 0; t < 1f; t += incrementAmount)
            {
                float lastDistance = Vector3.Distance(lastPosition, GetPositionAt(t));
                splineUnitDistance += lastDistance;

                lastPosition = GetPositionAt(t);

                if (splineUnitDistance >= unitDistance)
                {
                    float remainingDistance = splineUnitDistance - unitDistance;
                    return GetForwardAt(t - ((remainingDistance / lastDistance) * incrementAmount));
                }

            }

            // Default
            Anchor anchorA = m_Anchors[0];
            Anchor anchorB = m_Anchors[1];
            return Spline.CubicLerp(anchorA.Position, anchorA.HandleBPosition, anchorB.HandleAPosition, anchorB.Position, unitDistance / m_SplineLength);
        }

        /// <summary>
        /// Get the size of the spline.
        /// </summary>
        /// <param name="stepSize">How many iterations to check. Lower is more accurate.</param>
        /// <returns>The size of the spline.</returns>
        public float GetSplineLength(float stepSize = 0.01f)
        {
            float splineLength = 0;
            Vector3 lastPosition = GetPositionAt(0);

            // for the size of the spline increments
            for (float t = 0; t < 1f; t += stepSize)
            {
                splineLength += Vector3.Distance(lastPosition, GetPositionAt(t));
                lastPosition = GetPositionAt(t);
            }

            // adds the distance between the last position and the last position in the spline
            splineLength += Vector3.Distance(lastPosition, GetPositionAt(1f));
            return splineLength;
        }

        /// <summary>
        /// Adds an anchor to the spline.
        /// </summary>
        public void AddAnchor()
        {
            if (m_Anchors == null) { m_Anchors = new List<Anchor>(); }

            // create a new anchor is there isnt any
            if (m_Anchors.Count == 0)
            {
                m_Anchors.Add(new Anchor
                {
                    Position = new Vector3(0, 0, 0),
                    HandleAPosition = m_HandleOffsetA,
                    HandleBPosition = m_HandleOffsetB,
                });
            }

            // create a new one offsetted based on the last anchor
            Anchor lastAnchor = m_Anchors[m_Anchors.Count - 1];
            m_Anchors.Add(new Anchor
            {
                Position = lastAnchor.Position + new Vector3(1, 0, 0),
                HandleAPosition = lastAnchor.HandleAPosition + m_HandleOffsetA,
                HandleBPosition = lastAnchor.HandleBPosition + m_HandleOffsetB,
            });
        }

        /// <summary>
        /// Removes the last anchor from the spline.
        /// </summary>
        public void RemoveLastAnchor()
        {
            if (m_Anchors == null) { m_Anchors = new List<Anchor>(); }

            m_Anchors.RemoveAt(m_Anchors.Count - 1);
        }

        /// <summary>
        /// Update Spline.
        /// </summary>
        public void SetDirty()
        {
            m_SplineLength = GetSplineLength();

            UpdatePointList();

            OnDirty?.Invoke();
        }

        private void SetupPointList()
        {
            m_Points = new List<Point>();
            m_PointAmountInCurve = m_PointAmountPerUnitInCurve * m_SplineLength;
            for (float t = 0; t < 1f; t += 1f / m_PointAmountInCurve)
            {
                m_Points.Add(new Point
                {
                    Time = t,
                    Position = GetPositionAt(t),
                    Normal = m_Normal,
                });
            }

            m_Points.Add(new Point
            {
                Time = 1f,
                Position = GetPositionAt(1f),
            });

            UpdateForwardVectors();
        }

        private void UpdatePointList()
        {
            if (m_Points == null) return;

            foreach (Point point in m_Points)
            {
                point.Position = GetPositionAt(point.Time);
            }

            UpdateForwardVectors();
        }

        private void UpdateForwardVectors()
        {
            // Set forward vectors
            for (int i = 0; i < m_Points.Count - 1; i++)
            {
                m_Points[i].Forward = (m_Points[i + 1].Position - m_Points[i].Position).normalized;
            }
            // Set final forward vector
            if (m_IsLoopClosed)
            {
                m_Points[m_Points.Count - 1].Forward = m_Points[0].Forward;
            }
            else
            {
                m_Points[m_Points.Count - 1].Forward = m_Points[m_Points.Count - 2].Forward;
            }
        }

        // getters
        public List<Anchor> GetAnchors() { return m_Anchors; }
        public bool IsLoopClosed() { return m_IsLoopClosed; }
        public float GetGizmoSize() { return m_GizmoSize; }

        [Serializable]
        public class Anchor
        {
            public Vector3 Position;
            public Vector3 HandleAPosition;
            public Vector3 HandleBPosition;
        }

        [Serializable]
        public class Point
        {
            public float Time;
            public Vector3 Position;
            public Vector3 Forward;
            public Vector3 Normal;
        }
    }
}