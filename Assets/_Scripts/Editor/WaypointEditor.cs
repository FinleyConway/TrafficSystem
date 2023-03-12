using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TrafficSystem
{
    [InitializeOnLoad]
    public class WaypointEditor
    {
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
        public static void OnDrawSceneGizmo(Waypoint waypoint, GizmoType gizmoType) 
        { 
            if ((gizmoType & GizmoType.Selected) != 0)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = Color.yellow * 0.5f;   
            }

            Gizmos.DrawSphere(waypoint.transform.position, 0.1f);
            Gizmos.DrawLine(waypoint.transform.position + (waypoint.transform.right * waypoint.Width / 2f),
                waypoint.transform.position - (waypoint.transform.right * waypoint.Width / 2f));

            if (waypoint.PreviousWaypoint != null)
            {
                Gizmos.color = Color.red;
                Vector3 offset = waypoint.transform.right * waypoint.Width / 2f;
                Vector3 offsetTo = waypoint.PreviousWaypoint.transform.right * waypoint.PreviousWaypoint.Width / 2f;

                Gizmos.DrawLine(waypoint.transform.position + offset, waypoint.PreviousWaypoint.transform.position + offsetTo);
            }
            if (waypoint.NextWaypoint != null)
            {
                Gizmos.color = Color.green;
                Vector3 offset = waypoint.transform.right * -waypoint.Width / 2f;
                Vector3 offsetTo = waypoint.NextWaypoint.transform.right * -waypoint.NextWaypoint.Width / 2f;

                Gizmos.DrawLine(waypoint.transform.position + offset, waypoint.NextWaypoint.transform.position + offsetTo);
            }

            if (waypoint.branches != null)
            {
                foreach (Waypoint branchin in waypoint.branches)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(waypoint.transform.position, branchin.transform.position);
                }
            }
        }
    }
}
