using UnityEngine;

namespace TrafficSystem
{
    [RequireComponent(typeof(BoxCollider))]
    public class SpeedChange : MonoBehaviour
    {
        [field: SerializeField] public int ExpectedMph { get; private set; }

        [SerializeField] private bool m_ShouldChangeSpeedWhenPassed = false;
        [SerializeField] private int m_PassedMph;
        private BoxCollider m_Collider;

        private void Awake()
        {
            m_Collider = GetComponent<BoxCollider>();
            m_Collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IVehicle vehicle))
            {
                vehicle.CurrentSpeedLimit = ExpectedMph;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!m_ShouldChangeSpeedWhenPassed) return;

            if (other.TryGetComponent(out IVehicle vehicle))
            {
                vehicle.CurrentSpeedLimit = m_PassedMph;
            }
        }
    }
}
