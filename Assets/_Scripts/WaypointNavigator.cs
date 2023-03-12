using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TrafficSystem
{
    public class WaypointNavigator : MonoBehaviour
    {
        private Character m_Controller;
        public Waypoint CurrentWaypoint;

        private int m_Direction;

        private void Awake()
        {
            m_Controller = GetComponent<Character>();
            m_Controller.SetDestination(CurrentWaypoint.GetPosition());

            m_Direction = Mathf.RoundToInt(Random.Range(0, 1));
        }

        private void Update()
        {
            if (m_Controller.ReachedDestination)
            {
                bool shouldBranch = false;

                if (CurrentWaypoint.branches != null && CurrentWaypoint.branches.Count > 0)
                {
                    shouldBranch = Random.Range(0, 1) <= CurrentWaypoint.BranchRatio ? true : false;
                }

                if (shouldBranch)
                {
                    CurrentWaypoint = CurrentWaypoint.branches[Random.Range(0, CurrentWaypoint.branches.Count - 1)];
                }
                else
                {
                    if (m_Direction == 0)
                    {
                        if (CurrentWaypoint.NextWaypoint != null)
                        {
                            CurrentWaypoint = CurrentWaypoint.NextWaypoint;
                        }
                        else
                        {
                            CurrentWaypoint = CurrentWaypoint.PreviousWaypoint;
                            m_Direction = 1;
                        }
                    }
                    else if (m_Direction == 1)
                    {
                        if (CurrentWaypoint.PreviousWaypoint != null)
                        {
                            CurrentWaypoint = CurrentWaypoint.PreviousWaypoint;
                        }
                        else
                        {
                            CurrentWaypoint = CurrentWaypoint.NextWaypoint;
                            m_Direction = 0;
                        }
                    }
                }

                m_Controller.SetDestination(CurrentWaypoint.GetPosition());
            }
        }
    }
}
