using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    /// <summary>
    /// Handles nearby vehicles and tells the give way line which road is free or not.
    /// </summary>
    public class GiveWayNotifier : MonoBehaviour
    {
        private readonly HashSet<IVehicle> m_VehiclesInTriggerZone = new HashSet<IVehicle>();

        public event Action<IVehicle, bool> OnGiveWay;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IVehicle vehicle))
            {
                if (m_VehiclesInTriggerZone.Add(vehicle))
                {
                    OnGiveWay?.Invoke(vehicle, true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IVehicle vehicle))
            {
                if (m_VehiclesInTriggerZone.Remove(vehicle))
                {
                    OnGiveWay?.Invoke(vehicle, false);
                }
            }
        }
    }
}
