using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TrafficSystem
{
    public class Vehicle : MonoBehaviour
    {
        [Header("Path Finding")]
        [SerializeField] private Anchor m_StartAnchor;
        [SerializeField] private Anchor m_EndAnchor;
        [SerializeField] private PathFinding.PathType m_PathType;
        [SerializeField] private SplinePath m_FollowPath;
        private PathFinding m_Path;


        [Header("Stopping")]
        [SerializeField] private float m_DecelerationRate = 5f;
        [SerializeField] private Transform m_StopDetection;
        [SerializeField] private LayerMask m_VehicleMask;

        [SerializeField, Range(0, c_MaxMilesPerHour)] private int m_CurrentSpeed;

        [field: Header("Moving")]
        [field: SerializeField, Range(0, 71.72f)] public float MetresPerSecond { get; private set; }
        [SerializeField] private float m_AccelerationRate = 0.5f;
        private float m_MoveAmount;
        private float m_MaxMoveAmount;
        private bool m_IsBraking = false;


        private const int c_MaxMilesPerHour = 160;
        private static readonly Dictionary<int, float> s_BrakingDistances = new Dictionary<int, float>();


        private void Awake()
        {
            m_Path = new PathFinding();
            m_FollowPath.Anchors = m_Path.FindPath(m_StartAnchor, m_EndAnchor, m_PathType);

            if (s_BrakingDistances.Count <= 0 || s_BrakingDistances == null)
                CalculateBrakingDistances();
        }

        private void Start()
        {
            m_MaxMoveAmount = m_FollowPath.GetSplineLength();
        }

        private void Update()
        {
            MovementHandler();

            m_MoveAmount = (m_MoveAmount + (Time.deltaTime * MetresPerSecond)) % m_MaxMoveAmount;

            transform.position = m_FollowPath.GetPositionAtUnits(m_MoveAmount).Position;
            transform.forward = m_FollowPath.GetForwardAtUnits(m_MoveAmount);
        }

        private void MovementHandler()
        {
            float brakingDistance = GetBrakingDistance(MsToMph(MetresPerSecond));

            // Detect if there's a vehicle in front of the current vehicle
            if (Physics.Raycast(m_StopDetection.position, m_StopDetection.forward, out RaycastHit hit, brakingDistance, m_VehicleMask))
            {
                if (!m_IsBraking && MetresPerSecond > 0)
                {
                    StartCoroutine(DecelerateCar(brakingDistance, hit.distance));
                }
            }
            else
            {
                MetresPerSecond = Mathf.Lerp(MetresPerSecond, MphToMs(m_CurrentSpeed), m_AccelerationRate * Time.deltaTime);
            }
        }

        /// <summary>
        /// Decelerate Car based on the braking distance.
        /// </summary>
        private IEnumerator DecelerateCar(float brakingDistance, float distanceToOther)
        {
            float initialVelocity = MetresPerSecond;
            float adjustedBrakingDistance = Mathf.Max(0f, brakingDistance - distanceToOther);
            float adjustedInitialVelocity = Mathf.Sqrt(2f * adjustedBrakingDistance * m_DecelerationRate);

            m_IsBraking = true;

            float time = (initialVelocity - adjustedInitialVelocity) / m_DecelerationRate;
            float elapsedTime = 0f;

            while (elapsedTime < time)
            {
                // Calculate the current speed based on elapsed time and deceleration
                float currentSpeed = initialVelocity - m_DecelerationRate * elapsedTime;

                // Update the car's speed
                MetresPerSecond = currentSpeed;

                // Calculate the current distance to the obstacle based on elapsed time and current speed
                float currentDistance = distanceToOther + currentSpeed * elapsedTime;

                // If the current distance is negative, set it to zero and stop the car
                if (currentDistance <= 0f)
                {
                    MetresPerSecond = 0f;
                    break;
                }

                // Increment the elapsed time
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            MetresPerSecond = 0f;
            m_IsBraking = false;
        }

        /// <summary>
        /// Get the braking distance based on the vehicle speed.
        /// </summary>
        private float GetBrakingDistance(int mph)
        {
            int defaultMetres = 5;

            // If speed is between 10 and max
            if (mph >= 10 && mph <= c_MaxMilesPerHour)
            {
                // Rounding mph to a multiple of 10
                int roundedMph = Mathf.RoundToInt(mph / 10f) * 10; ;
                return s_BrakingDistances[roundedMph];
            }
            else
            {
                return defaultMetres;
            }
        }

        /// <summary>
        /// Calculate braking distances based on the UK DVLA.
        /// </summary>
        private static void CalculateBrakingDistances()
        {
            int mph = 10;
            float distance = 1.5f;

            while (mph <= c_MaxMilesPerHour)
            {
                float feet = mph * distance;
                float metres = Mathf.RoundToInt(feet / 3.2808f); // Convert feet to metres

                s_BrakingDistances.Add(mph, metres);

                mph += 10;
                distance += 0.5f;
            }
        }

        /// <summary>
        /// Convert m/s to mph
        /// </summary>
        public static int MsToMph(float metresPerSecond)
        {
            return Mathf.RoundToInt(metresPerSecond * 2.23694f);
        }

        /// <summary>
        /// Converts mph to m/s
        /// </summary>
        public static float MphToMs(int mph)
        {
            return mph * 0.44704f;
        }

        private void OnDrawGizmosSelected()
        {
            if (m_StopDetection != null && s_BrakingDistances.Count > 0)
            {
                Gizmos.DrawRay(m_StopDetection.position, m_StopDetection.forward * GetBrakingDistance(MsToMph(MetresPerSecond)));
            }
        }
    }
}