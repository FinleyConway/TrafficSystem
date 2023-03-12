using PlasticPipe.PlasticProtocol.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TrafficSystem
{
    public class WaypointWindowEditor : EditorWindow
    {
        [MenuItem("Tools/Waypoint Editor")]
        public static void Open()
        {
            GetWindow(typeof(WaypointWindowEditor));
        }

        public Transform WaypointRoot;

        private void OnGUI()
        {
            SerializedObject obj = new SerializedObject(this);

            EditorGUILayout.PropertyField(obj.FindProperty("WaypointRoot"));

            if (WaypointRoot == null) 
            {
                EditorGUILayout.HelpBox("Root transform must be selected. Please assign a root transform", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.BeginVertical("box");
                DrawButton();
                EditorGUILayout.EndVertical();
            }

            obj.ApplyModifiedProperties();
        }

        private void DrawButton()
        {
            if (GUILayout.Button("Create Waypoint"))
            {
                CreateWaypoint();
            }
            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Waypoint>()) 
            {
                if (GUILayout.Button("Add Branch Waypoint"))
                {
                    CreateBranch();
                }
                if (GUILayout.Button("Create Waypoint Before"))
                {
                    CreateWaypointBefore();
                }
                if (GUILayout.Button("Create Waypoint After"))
                {
                    CreateWaypointAfter();
                }
                if (GUILayout.Button("Remvoe Waypoint"))
                {
                    RemoveWaypoint();
                }
            }
        }

        private void CreateWaypoint()
        {
            GameObject waypointObject = new GameObject("Waypoint " + WaypointRoot.childCount, typeof(Waypoint));
            waypointObject.transform.SetParent(WaypointRoot, false);

            Waypoint waypoint = waypointObject.GetComponent<Waypoint>();
            if (WaypointRoot.childCount > 1)
            {
                waypoint.PreviousWaypoint = WaypointRoot.GetChild(WaypointRoot.childCount - 2).GetComponent<Waypoint>();
                waypoint.PreviousWaypoint.NextWaypoint = waypoint;

                // place the waypoint at the last position
                waypoint.transform.position = waypoint.PreviousWaypoint.transform.position;
                waypoint.transform.forward = waypoint.PreviousWaypoint.transform.forward;
            }

            Selection.activeGameObject = waypoint.gameObject;
        }

        private void CreateWaypointBefore()
        {
            GameObject waypointObject = new GameObject("Waypoint " + WaypointRoot.childCount, typeof(Waypoint));
            waypointObject.transform.SetParent(WaypointRoot, false);

            Waypoint newWaypoint = waypointObject.GetComponent<Waypoint>();
            Waypoint selectedWaypoint = Selection.activeGameObject.GetComponent<Waypoint>();

            waypointObject.transform.position = selectedWaypoint.transform.position;
            waypointObject.transform.forward = selectedWaypoint.transform.forward;

            if (selectedWaypoint.PreviousWaypoint != null)
            {
                newWaypoint.PreviousWaypoint = selectedWaypoint.PreviousWaypoint;
                selectedWaypoint.PreviousWaypoint.NextWaypoint = newWaypoint;
            }

            newWaypoint.NextWaypoint = selectedWaypoint;
            selectedWaypoint.PreviousWaypoint = newWaypoint;

            newWaypoint.transform.SetSiblingIndex(selectedWaypoint.transform.GetSiblingIndex());
            Selection.activeGameObject = newWaypoint.gameObject;
        }

        private void CreateWaypointAfter()
        {
            GameObject waypointObject = new GameObject("Waypoint " + WaypointRoot.childCount, typeof(Waypoint));
            waypointObject.transform.SetParent(WaypointRoot, false);

            Waypoint newWaypoint = waypointObject.GetComponent<Waypoint>();
            Waypoint selectedWaypoint = Selection.activeGameObject.GetComponent<Waypoint>();

            waypointObject.transform.position = selectedWaypoint.transform.position;
            waypointObject.transform.forward = selectedWaypoint.transform.forward;

            newWaypoint.PreviousWaypoint = selectedWaypoint;

            if (selectedWaypoint.NextWaypoint != null)
            {
                selectedWaypoint.NextWaypoint.PreviousWaypoint = newWaypoint;
                newWaypoint.NextWaypoint = selectedWaypoint.NextWaypoint;
            }

            selectedWaypoint.NextWaypoint = newWaypoint;
            newWaypoint.transform.SetSiblingIndex(selectedWaypoint.transform.GetSiblingIndex());
            Selection.activeGameObject = newWaypoint.gameObject;
        }

        private void RemoveWaypoint()
        {
            Waypoint selectedWaypoint = Selection.activeGameObject.GetComponent<Waypoint>();

            if (selectedWaypoint != null)
            {
                selectedWaypoint.NextWaypoint.PreviousWaypoint = selectedWaypoint.PreviousWaypoint;
            }
            if (selectedWaypoint.PreviousWaypoint != null)
            {
                selectedWaypoint.PreviousWaypoint.NextWaypoint = selectedWaypoint.NextWaypoint;
                Selection.activeGameObject = selectedWaypoint.PreviousWaypoint.gameObject;
            }

            DestroyImmediate(selectedWaypoint.gameObject);
        }

        private void CreateBranch()
        {
            GameObject waypointObject = new GameObject("Waypoint " + WaypointRoot.childCount, typeof(Waypoint));
            waypointObject.transform.SetParent(WaypointRoot, false);

            Waypoint newWaypoint = waypointObject.GetComponent<Waypoint>();
            Waypoint branchFrom = Selection.activeGameObject.GetComponent<Waypoint>();
            branchFrom.branches.Add(newWaypoint);

            newWaypoint.transform.position = branchFrom.transform.position;
            newWaypoint.transform.forward = branchFrom.transform.forward;

            Selection.activeGameObject = newWaypoint.gameObject;
        }
    }
}
