using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    public class SplinePath : MonoBehaviour
    {
        [SerializeField] private Vector3 m_Normal = new Vector3(0, 0, -1);
        [field: SerializeField] public bool IsLoopClosed { get; private set; }
        [field: SerializeField] public List<Anchor> Anchors { get; set; } = new List<Anchor>();

        private List<Point> m_Points;
        private float m_SplineLength;
        private float m_PointAmountInCurve;
        private const float m_PointAmountPerUnitInCurve = 2f;

        public event Action OnDirty;

        public void Init()
        {
            m_SplineLength = GetSplineLength();
            if (m_Points == null)
                SetupPointList();
            else
                UpdatePointList();
        }

        /// <summary>
        /// Returns the position on the spline based on time
        /// </summary>
        /// <param name="time">Value used to interpolate between a and b.</param>
        /// <returns>The position on spline based on time and the neighboring anchors.</returns>
        public SplineInfo GetPositionAt(float time)
        {
            SplineInfo splineInfo = new SplineInfo();

            // if at the end of the spline
            if (time == 1)
            {
                Anchor anchorA;
                Anchor anchorB;

                if (IsLoopClosed)
                {
                    // last position and first position for closed loop
                    anchorA = Anchors[Anchors.Count - 1];
                    anchorB = Anchors[0];
                }
                else
                {
                    // second last and last position for open spline
                    anchorA = Anchors[Anchors.Count - 2];
                    anchorB = Anchors[Anchors.Count - 1];
                }

                splineInfo.LastAnchor = anchorA;
                splineInfo.CurrentAnchor = anchorB;
                splineInfo.NextAnchor = IsLoopClosed ? Anchors[0] : anchorB = Anchors[Anchors.Count - 1];
                splineInfo.Position = Spline.CubicLerp(anchorA.transform.position, transform.position + anchorA.HandleBPosition, transform.position + anchorB.HandleAPosition, anchorB.transform.position, time);

                return splineInfo;
            }
            else
            {
                int addClosedLoop = IsLoopClosed ? 1 : 0;

                // calculate the index of the two anchors to interpolate between
                float tFull = time * (Anchors.Count - 1 + addClosedLoop);
                int anchorIndex = Mathf.FloorToInt(tFull);
                float tAnchor = tFull - anchorIndex;

                Anchor anchorA;
                Anchor anchorB;

                if (anchorIndex < Anchors.Count - 1)
                {
                    // get the two anchors to interpolate between
                    anchorA = Anchors[anchorIndex + 0];
                    anchorB = Anchors[anchorIndex + 1];
                    splineInfo.LastAnchor = anchorA;
                    splineInfo.CurrentAnchor = anchorB;
                    splineInfo.NextAnchor = Anchors[anchorIndex + 1];
                }
                else
                {
                    // if at the end of the spline, either link to "next" one or loop back to the first
                    if (IsLoopClosed)
                    {
                        anchorA = Anchors[Anchors.Count - 1];
                        anchorB = Anchors[0];
                        splineInfo.LastAnchor = anchorA;
                        splineInfo.CurrentAnchor = anchorB;
                        splineInfo.NextAnchor = anchorB;
                    }
                    else
                    {
                        anchorA = Anchors[anchorIndex - 1];
                        anchorB = Anchors[anchorIndex + 0];
                        tAnchor = 1f;
                        splineInfo.LastAnchor = anchorA;
                        splineInfo.CurrentAnchor = anchorB;
                        splineInfo.NextAnchor = null;
                    }
                }

                splineInfo.Position = Spline.CubicLerp(anchorA.transform.position, transform.position + anchorA.HandleBPosition, transform.position + anchorB.HandleAPosition, anchorB.transform.position, tAnchor);

                return splineInfo;
            }
        }

        /// <summary>
        /// Gets the forward direction of an object at a specific time by linearly interpolating between the previous and next point's forward directions.
        /// </summary>
        /// <param name="time">The time at which to get the forward direction.</param>
        /// <returns>The forward direction of the object at the specified time.</returns>
        public Vector3 GetForwardAt(float time)
        {
            Point pointA = GetPreviousPoint(time);
            int pointBIndex = (m_Points.IndexOf(pointA) + 1) % m_Points.Count;
            Point pointB = m_Points[pointBIndex];

            return Vector3.Lerp(pointA.Forward, pointB.Forward, (time - pointA.Time) / Mathf.Abs(pointA.Time - pointB.Time));
        }

        /// <summary>
        /// Returns the previous point in the collection of points with a time less than the specified time.
        /// </summary>
        /// <param name="time">The time used to find the previous point.</param>
        /// <returns>The previous point in the collection of points.</returns>
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
        /// <returns>The position on the spline at the specified distance and the next anchor.</returns>
        public SplineInfo GetPositionAtUnits(float unitDistance, float stepSize = 0.01f)
        {
            SplineInfo splineInfo = new SplineInfo();
            float splineUnitDistance = 0f;
            Vector3 lastPosition = GetPositionAt(0f).Position;
            float incrementAmount = stepSize;

            // iterate over the spline, moving incrementally along it
            for (float t = 0; t < 1f; t += incrementAmount)
            {
                // update spline unit distance and last position
                splineUnitDistance += Vector3.Distance(lastPosition, GetPositionAt(t).Position);
                lastPosition = GetPositionAt(t).Position;

                // check if target unit distance has been reached
                if (splineUnitDistance >= unitDistance)
                {
                    // calculate the position at the target unit distance along the spline
                    Vector3 direction = (GetPositionAt(t).Position - GetPositionAt(t - incrementAmount).Position).normalized;

                    splineInfo.Position = GetPositionAt(t).Position + direction * (unitDistance - splineUnitDistance);
                    splineInfo.LastAnchor = GetPositionAt(t - incrementAmount).LastAnchor;
                    splineInfo.CurrentAnchor = GetPositionAt(t).CurrentAnchor;
                    splineInfo.NextAnchor = GetPositionAt(t).NextAnchor;

                    return splineInfo;
                }
            }

            // if the target unit distance was not reached, interpolate along the entire spline
            Anchor anchorA = Anchors[0];
            Anchor anchorB = Anchors[1];

            splineInfo.Position = Spline.CubicLerp(anchorA.transform.position, anchorA.HandleBPosition, anchorB.HandleAPosition, anchorB.transform.position, unitDistance / m_SplineLength);
            splineInfo.LastAnchor = anchorA;
            splineInfo.CurrentAnchor = anchorB;
            splineInfo.NextAnchor = anchorB.NextAnchor;

            return splineInfo;
        }


        /// <summary>
        /// Returns the forward direction of an object at a distance along the spline by linearly interpolating between the previous and next point's forward directions.
        /// </summary>
        /// <param name="unitDistance">The distance along the spline at which to get the forward direction.</param>
        /// <param name="stepSize">The step size used to increment along the spline.</param>
        /// <returns>The forward direction of the object at the specified distance along the spline.</returns>
        public Vector3 GetForwardAtUnits(float unitDistance, float stepSize = 0.01f)
        {
            float splineUnitDistance = 0f;
            Vector3 lastPosition = GetPositionAt(0f).Position;

            float incrementAmount = stepSize;
            for (float t = 0; t < 1f; t += incrementAmount)
            {
                float lastDistance = Vector3.Distance(lastPosition, GetPositionAt(t).Position);
                splineUnitDistance += lastDistance;

                lastPosition = GetPositionAt(t).Position;

                if (splineUnitDistance >= unitDistance)
                {
                    float remainingDistance = splineUnitDistance - unitDistance;
                    return GetForwardAt(t - ((remainingDistance / lastDistance) * incrementAmount));
                }
            }

            // Default
            Anchor anchorA = Anchors[0];
            Anchor anchorB = Anchors[1];
            return Spline.CubicLerp(anchorA.transform.position, anchorA.HandleBPosition, anchorB.HandleAPosition, anchorB.transform.position, unitDistance / m_SplineLength);
        }

        public float GetPreCalculatedSplineLength() { return m_SplineLength; }

        /// <summary>
        /// Get the size of the spline.
        /// </summary>
        /// <param name="stepSize">How many iterations to check. Lower is more accurate.</param>
        /// <returns>The size of the spline.</returns>
        public float GetSplineLength(float stepSize = 0.01f)
        {
            float splineLength = 0;
            Vector3 lastPosition = GetPositionAt(0).Position;

            // for the size of the spline increments
            for (float t = 0; t < 1f; t += stepSize)
            {
                splineLength += Vector3.Distance(lastPosition, GetPositionAt(t).Position);
                lastPosition = GetPositionAt(t).Position;
            }

            // adds the distance between the last position and the last position in the spline
            splineLength += Vector3.Distance(lastPosition, GetPositionAt(1f).Position);
            return splineLength;
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

        public void SetupPointList()
        {
            m_Points = new List<Point>();
            m_PointAmountInCurve = m_PointAmountPerUnitInCurve * m_SplineLength;
            for (float t = 0; t < 1f; t += 1f / m_PointAmountInCurve)
            {
                m_Points.Add(new Point
                {
                    Time = t,
                    Position = GetPositionAt(t).Position,
                    Normal = m_Normal,
                });
            }

            m_Points.Add(new Point
            {
                Time = 1f,
                Position = GetPositionAt(1f).Position,
            });

            UpdateForwardVectors();
        }

        private void UpdatePointList()
        {
            if (m_Points == null || m_Points.Count <= 0) return;

            foreach (Point point in m_Points)
            {
                point.Position = GetPositionAt(point.Time).Position;
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
            if (IsLoopClosed)
            {
                m_Points[m_Points.Count - 1].Forward = m_Points[0].Forward;
            }
            else
            {
                m_Points[m_Points.Count - 1].Forward = m_Points[m_Points.Count - 2].Forward;
            }
        }

        [Serializable]
        public class Point
        {
            public float Time;
            public Vector3 Position;
            public Vector3 Forward;
            public Vector3 Normal;
        }

        public class SplineInfo
        {
            public Vector3 Position;
            public Anchor CurrentAnchor;
            public Anchor LastAnchor;
            public Anchor NextAnchor;
        }

        private void OnDrawGizmosSelected()
        {
            Anchor previous = null;

            foreach (Anchor anchor in Anchors)
            {
                if (previous != null)
                {
                    Gizmos.DrawLine(previous.transform.position, anchor.transform.position);
                }
                previous = anchor;
            }
        }
    }
}