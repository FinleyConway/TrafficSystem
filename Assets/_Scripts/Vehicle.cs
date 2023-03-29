using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TrafficSystem
{
    public class Vehicle : MonoBehaviour
    {
        [SerializeField] private Transform m_StopDetection;
        [SerializeField] private LayerMask m_VehicleMask;

        [SerializeField, Range(0, c_MaxMilesPerHour)] private int m_MilesPerHour = 60; // 1 unity unity * 2.23694f (m/s -> mph)
        private float m_MoveAmount;
        public float MaxMoveAmount { get; set; }
        private bool m_IsBraking = false;
        private const int c_MaxMilesPerHour = 160;
        private static readonly Dictionary<int, float> s_BrakingDistances = new Dictionary<int, float>();

        [SerializeField] private SplinePath m_Spline;

        private void Start()
        {
            CalculateBrakingDistances();
            MaxMoveAmount = m_Spline.GetSplineLength();
        }

        private void Update()
        {
            MovementHandler();

            m_MoveAmount = (m_MoveAmount + (Time.deltaTime * m_MilesPerHour)) % MaxMoveAmount;

            transform.position = m_Spline.GetPositionAtUnits(m_MoveAmount).Position;
            transform.forward = m_Spline.GetForwardAtUnits(m_MoveAmount);
        }

        private void MovementHandler()
        {
            float brakingDistance = GetBrakingDistance(m_MilesPerHour);

            // Detect if there's a vehicle in front of the current vehicle
            if (Physics.Raycast(m_StopDetection.position, m_StopDetection.forward, out RaycastHit hit, brakingDistance, m_VehicleMask))
            {
                if (!m_IsBraking && m_MilesPerHour > 0)
                {
                    StartCoroutine(DecelerateCar(brakingDistance));
                }
            }
        }

        /// <summary>
        /// Decelerate Car based on the braking distance.
        /// </summary>
        private IEnumerator DecelerateCar(float brakingDistance)
        {
            float currentVelocity = m_MilesPerHour * 0.44704f; // Convert mph to m/s
            float deceleration = currentVelocity * currentVelocity / (2 * brakingDistance);
            float currentTime = 0f;
            float timeStep = 0.01f;

            m_IsBraking = true;

            while (brakingDistance > 0f)
            {
                // Update the velocity based on the deceleration
                currentVelocity -= deceleration * timeStep;
                m_MilesPerHour = (int)(currentVelocity / 0.44704f); // Convert m/s back to mph

                // Make sure the speed doesn't go below 0
                if (m_MilesPerHour < 0)
                {
                    m_MilesPerHour = 0;
                    break;
                }

                // Subtract the distance traveled (which is equal to the current velocity multiplied by the time step) from the braking distance
                brakingDistance -= currentVelocity * timeStep;

                currentTime += timeStep;
                yield return null;
            }

            m_MilesPerHour = 0;
            m_IsBraking = false;
        }

        /// <summary>
        /// Get the braking distance based on the vehicle speed.
        /// </summary>
        private float GetBrakingDistance(int mph)
        {
            int defaultMetres = 5;

            if (mph >= 10 && mph <= c_MaxMilesPerHour)
            {
                // rounding mph to a multiple of 10
                int roundedMph = Mathf.RoundToInt(mph / 10) * 10;
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

        private void OnDrawGizmosSelected()
        {
            if (m_StopDetection != null && s_BrakingDistances.Count > 0)
            {
                Gizmos.DrawRay(m_StopDetection.position, m_StopDetection.forward * GetBrakingDistance(m_MilesPerHour));
            }
        }
    }
}