using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    public class Vehicle : MonoBehaviour, IVehicle
    {
        [Header("Path Finding")]
        [SerializeField] private PathFinding.PathType m_PathType;
        [field: SerializeField] public SplinePath Path { get; private set; }
        public Anchor StartAnchor { get; set; }
        public Anchor EndAnchor { get; set; }
        public Anchor CurrentAnchor { get; private set; }
        private bool m_ShouldGenPath = false;

        [Header("Stopping")]
        [SerializeField] private float m_DecelerationRate = 5f;
        [SerializeField] private Transform m_StopDetection;
        [SerializeField] private LayerMask m_VehicleMask;
        public bool ShouldStop { get; set; } = false;

        [Header("Moving")]
        [SerializeField] private float m_AccelerationRate = 0.5f;
        [field: SerializeField] public int CurrentSpeedLimit { get; set; }
        public float MetresPerSecond { get; set; }

        private float m_MoveAmount = 0;
        private float m_MaxMoveAmount = 0;

        private const int c_MaxMilesPerHour = 160;
        private static readonly Dictionary<int, float> s_BrakingDistances = new Dictionary<int, float>();

        private void Awake()
        {
            if (s_BrakingDistances.Count <= 0 || s_BrakingDistances == null)
                CalculateBrakingDistances();

            MetresPerSecond = MphToMs(CurrentSpeedLimit);
        }

        public void Init()
        {
            PathRequestManager.RequestPath(StartAnchor, EndAnchor, GeneratePath);
        }

        private void GeneratePath(Anchor[] path, bool success)
        {
            if (success)
            {
                Path.Anchors = new List<Anchor>(path);

                StartAnchor = Path.Anchors[0];
                EndAnchor = Path.Anchors[Path.Anchors.Count - 1];

                Path.Init();
                m_MaxMoveAmount = Path.GetSplineLength();
                m_MoveAmount = 0;
                m_ShouldGenPath = false;
            }
        }

        private void Update()
        {
            if (Path.Anchors.Count > 0)
            {
                m_MoveAmount += Time.deltaTime * MetresPerSecond;

                if (m_MoveAmount < m_MaxMoveAmount)
                {
                    MovementHandler();

                    m_ShouldGenPath = false;
                    SplinePath.SplineInfo info = Path.GetPositionAtUnits(m_MoveAmount);

                    CurrentAnchor = info.CurrentAnchor;
                    transform.position = info.Position;
                    transform.forward = Path.GetForwardAtUnits(m_MoveAmount);
                }
                else
                {
                    if (!m_ShouldGenPath)
                    {
                        PathRequestManager.RequestPath(EndAnchor, CarPlacer.instance.GetRandomAnchor(), GeneratePath);
                        m_ShouldGenPath = true;
                    }
                }
            }
        }

        private void MovementHandler()
        {
            float speedDistance = GetBrakingDistance(MsToMph(MetresPerSecond));

            // Detect if there's a vehicle in front of the current vehicle
            if (Physics.Raycast(m_StopDetection.position, m_StopDetection.forward, out RaycastHit hit, speedDistance, m_VehicleMask))
            {
                // Detect if theres a car in front
                if (hit.transform.TryGetComponent(out IVehicle vehicle))
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

                // Detect if theres a give way approaching
                if (hit.transform.TryGetComponent(out IControlCar giveWay))
                {
                    if (IsCarInOppositeDirection(hit)) return;

                    if (hit.transform.TryGetComponent(out SpeedChange speed))
                    {
                        float distanceFromSign = hit.distance;

                        // only slow down if the speed is lower then the current speed
                        if (CurrentSpeedLimit > speed.ExpectedMph)
                        {
                            // sharper the braking the closer the car is
                            float lerpTime = 1 / distanceFromSign; // inverse distance
                            lerpTime = Mathf.Clamp(lerpTime, 0.1f, 1f);
                            MetresPerSecond = Mathf.Lerp(MetresPerSecond, MphToMs(speed.ExpectedMph), lerpTime);
                            CurrentSpeedLimit = speed.ExpectedMph;
                        }
                    }

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

                else if (hit.transform.TryGetComponent(out SpeedChange speedChange))
                {
                    if (IsCarInOppositeDirection(hit)) return;

                    float distanceFromSign = hit.distance;

                    // only slow down if the speed is lower then the current speed
                    if (CurrentSpeedLimit > speedChange.ExpectedMph)
                    {
                        // sharper the braking the closer the car is
                        float lerpTime = 1 / distanceFromSign; // inverse distance
                        lerpTime = Mathf.Clamp(lerpTime, 0.1f, 1f);
                        MetresPerSecond = Mathf.Lerp(MetresPerSecond, MphToMs(speedChange.ExpectedMph), lerpTime);
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