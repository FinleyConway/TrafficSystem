using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    /// <summary>
    /// Handles stopping and moving off vehicles by recieving information about the new road ahead.
    /// </summary>
    public class GiveWay : MonoBehaviour, IControlCar
    {
        [SerializeField] private List<GiveWayNotifier> m_Notifiers;
        [SerializeField] private bool m_ShouldCheckAhead = false;

        private readonly HashSet<IVehicle> m_CurrentObstructions = new HashSet<IVehicle>();
        public Vehicle CurrentCar { get; set; }
        public bool ShouldGo { get; set; } = true;

        private void OnEnable()
        {
            foreach (var notifier in m_Notifiers)
            {
                notifier.OnGiveWay += OnGiveWay;
            }
        }

        private void OnDestroy()
        {
            foreach (var notifier in m_Notifiers)
            {
                notifier.OnGiveWay -= OnGiveWay;
            }
        }

        private void Update()
        {
            // if the car is null and there are obstruction
            if (CurrentCar != null && m_CurrentObstructions.Count > 0)// && ShouldGo)
            {
                CheckAhead();
            }
            else if (CurrentCar != null && m_CurrentObstructions.Count <= 0)// && !ShouldGo)
            {
                CurrentCar.ShouldStop = false;
            }
        }

        private void OnGiveWay(IVehicle vehicle, bool isCarPresent)
        {
            // Adds to obstruction collection
            if (isCarPresent)
            {
                m_CurrentObstructions.Add(vehicle);
            }
            else
            {
                m_CurrentObstructions.Remove(vehicle);
            }
        }

        private void CheckAhead()
        {
            List<Anchor> path = CurrentCar.Path.Anchors;
            int startIndex = path.IndexOf(CurrentCar.CurrentAnchor);
            int searchAmount = 2;

            for (int i = startIndex; i < Mathf.Min(startIndex + searchAmount, path.Count); i++)
            {
                // check if the current car's path intersects with any other car's path
                foreach (IVehicle vehicle in m_CurrentObstructions)
                {
                    List<Anchor> carPath = vehicle.Path.Anchors;
                    int carStartIndex = carPath.IndexOf(vehicle.CurrentAnchor);
                    int carSearchAmount = 2;

                    for (int j = carStartIndex; j < Mathf.Min(carStartIndex + carSearchAmount, carPath.Count); j++)
                    {
                        if (path[i] == carPath[j])
                        {
                            CurrentCar.ShouldStop = true;
                            return; // exit the function as soon as an obstruction is found
                        }
                    }
                }
            }
        }
    }
}
