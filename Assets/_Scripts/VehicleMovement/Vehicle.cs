using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TrafficSystem
{
    public class Vehicle : MonoBehaviour, IVehicle
    {
        [Header("Path Finding")]
        [SerializeField] private Anchor m_StartAnchor;
        [SerializeField] private Anchor m_EndAnchor;
        [SerializeField] private PathFinding.PathType m_PathType;
        [field: SerializeField] public SplinePath FollowPath { get; private set; }
        public Anchor CurrentAnchor { get; private set; }

        private PathFinding m_Path;

        [Header("Stopping")]
        [SerializeField] private float m_DecelerationRate = 5f;
        [SerializeField] private Transform m_StopDetection;
        [SerializeField] private LayerMask m_VehicleMask;
        public bool ShouldStop { get; set; } = false;


        [Header("Moving")]
        [SerializeField] private float m_AccelerationRate = 0.5f;
        [field: SerializeField] public int CurrentSpeedLimit { get; set; }
        public float MetresPerSecond { get; set; }

        private float m_MoveAmount;
        private float m_MaxMoveAmount;

        private const int c_MaxMilesPerHour = 160;
        private static readonly Dictionary<int, float> s_BrakingDistances = new Dictionary<int, float>();


        private void Awake()
        {
            m_Path = new PathFinding();
            FollowPath.Anchors = m_Path.FindPath(m_StartAnchor, m_EndAnchor, m_PathType);

            if (s_BrakingDistances.Count <= 0 || s_BrakingDistances == null)
                CalculateBrakingDistances();

            MetresPerSecond = MphToMs(CurrentSpeedLimit);
        }

        private void Start()
        {
            m_MaxMoveAmount = FollowPath.GetSplineLength();
        }

        private void Update()
        {
            MovementHandler();

            m_MoveAmount = (m_MoveAmount + (Time.deltaTime * MetresPerSecond)) % m_MaxMoveAmount;

            SplinePath.SplineInfo info = FollowPath.GetPositionAtUnits(m_MoveAmount);

            CurrentAnchor = info.CurrentAnchor;
            transform.position = info.Position;
            transform.forward = FollowPath.GetForwardAtUnits(m_MoveAmount);
        }

        private void MovementHandler()
        {
            float speedDistance = GetBrakingDistance(MsToMph(MetresPerSecond));

            // Detect if there's a vehicle in front of the current vehicle
            if (Physics.Raycast(m_StopDetection.position, m_StopDetection.forward, out RaycastHit hit, speedDistance, m_VehicleMask))
            {
                // Detect if theres a give way approaching
                if (hit.transform.parent.TryGetComponent(out GiveWay giveWay))
                {
                    // Tell give way to manage this car
                    giveWay.CurrentCar = this;

                    // stop car at give way line
                    float stopDistance = 1.5f;
                    if (hit.distance < stopDistance && ShouldStop)
                    {
                        // slow down and stop the car
                        MetresPerSecond = Mathf.Lerp(MetresPerSecond, 0f, m_DecelerationRate * Time.deltaTime);
                    }
                    else if (!ShouldStop)
                    {
                        MetresPerSecond = Mathf.Lerp(MetresPerSecond, MphToMs(CurrentSpeedLimit), m_DecelerationRate * Time.deltaTime);
                    }

                }
                // Detect if theres a car in front
                else if (hit.transform.TryGetComponent(out IVehicle vehicle))
                {
                    if (IsCarInOppositeDirection(hit)) return;

                    float carAheadSpeed = vehicle.MetresPerSecond;

                    // emergency stop
                    float minDistanceFromCar = 2f;

                    if (hit.distance < minDistanceFromCar)
                    {
                        MetresPerSecond = 0;
                    }
                    // if this car is faster then the car ahead
                    // the speed distance shoud take care of the 2 rule gap
                    else if (MetresPerSecond >= carAheadSpeed)
                    {
                        // try to maintain speed 
                        MetresPerSecond = Mathf.Lerp(MetresPerSecond, carAheadSpeed, m_DecelerationRate * Time.deltaTime);
                    }
                }
            }
            else
            {
                MetresPerSecond = Mathf.Lerp(MetresPerSecond, MphToMs(CurrentSpeedLimit), m_AccelerationRate * Time.deltaTime);
            }
        }

        /// <summary>
        /// Checks if the car that the raycast hits is driving in the same direction.
        /// Fixes issue of when going round corners
        /// </summary>
        private bool IsCarInOppositeDirection(RaycastHit hit)
        {
            Vector3 carDirection = hit.transform.forward;
            float directionFrom = Vector3.Dot(transform.forward, carDirection);

            return directionFrom < 0;
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