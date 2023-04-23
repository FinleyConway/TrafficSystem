using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    /// <summary>
    /// Handles stopping and moving off vehicles by recieving information about the new road ahead.
    /// </summary>
    public class GiveWay : MonoBehaviour
    {
        [SerializeField] private List<GiveWayNotifier> m_Notifiers;
        [SerializeField] private GameObject m_Stopper;

        public HashSet<Anchor> CurrentObstructions { get; private set; } = new HashSet<Anchor>();
        public Vehicle CurrentCar { get; set; }

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

        #region Open Me If Get Angry About Giving Way
        /*
         * im not convinced that doing this in the event update works so im keeping this here just in case
        */
        /*
        private void Update()
        {
            // if the car is null and there are obstruction
            if (CurrentCar != null && CurrentObstructions.Count > 0)
            {
                // Is the obstruction blocking the current path?
                int startIndex = CurrentCar.FollowPath.Anchors.IndexOf(CurrentCar.CurrentAnchor);
                int searchAmount = 2;

                // loops through index and checks if its going to be in obstruction
                for (int i = startIndex; i < startIndex + searchAmount && i < CurrentCar.FollowPath.Anchors.Count; i++)
                {
                    if (CurrentObstructions.Contains(CurrentCar.FollowPath.Anchors[i]))
                    {
                        CurrentCar.ShouldStop = true;
                    }
                }
            }
            else if (CurrentCar != null && CurrentObstructions.Count <= 0)
            {
                CurrentCar.ShouldStop = false;
            }
        }
        */
        #endregion

        private void OnGiveWay(Anchor nextNode, bool isCarPresent)
        {
            // Adds to obstruction collection
            if (isCarPresent)
            {
                CurrentObstructions.Add(nextNode);

                if (CurrentCar != null && CurrentObstructions.Count > 0)
                {
                    CheckAhead();
                }
            }
            else
            {
                CurrentObstructions.Remove(nextNode);

                if (CurrentCar != null && CurrentObstructions.Count <= 0)
                {
                    CurrentCar.ShouldStop = false;
                }
            }
        }

        private void CheckAhead()
        {
            // Is the obstruction blocking the current path?
            List<Anchor> path = CurrentCar.FollowPath.Anchors;
            int startIndex = path.IndexOf(CurrentCar.CurrentAnchor);
            int searchAmount = 2;

            // loops through index and checks if its going to be in obstruction
            for (int i = startIndex; i < startIndex + searchAmount && i < path.Count; i++)
            {
                if (CurrentObstructions.Contains(path[i]))
                {
                    CurrentCar.ShouldStop = true;
                }
            }
        }
    }
}
