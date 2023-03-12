using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrafficSystem
{
    public class Waypoint : MonoBehaviour
    {
        public Waypoint PreviousWaypoint;
        public Waypoint NextWaypoint;

        [Range(0f, 5f)] public float Width = 1f;

        public List<Waypoint> branches;
        [Range(0, 1)] public float BranchRatio = 0.5f;

        // Get a random point based on the width 
        public Vector3 GetPosition()
        {
            Vector3 minBound = transform.position + transform.right * Width / 2f;
            Vector3 maxBound = transform.position - transform.right * Width / 2f;

            return Vector3.Lerp(minBound, maxBound, Random.Range(0, 1f));
        }
    }
}
