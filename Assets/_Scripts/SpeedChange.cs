using UnityEngine;

namespace TrafficSystem
{
    [RequireComponent(typeof(BoxCollider))]
    public class SpeedChange : MonoBehaviour
    {
        [SerializeField] private int m_ExpectedMph;

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
                vehicle.CurrentSpeedLimit = m_ExpectedMph;
            }
        }
    }
}
