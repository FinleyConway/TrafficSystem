using UnityEngine;
using UnityEditor;

namespace TrafficSystem
{
    [CustomEditor(typeof(PathFinding))]
    public class PathFindingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PathFinding path = (PathFinding)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Choose Random Spot"))
            {
                Anchor end = path.Anchors[Random.Range(0, path.Anchors.Count)];
                path.Follow.Anchors.Clear();
                path.FindPath(path.Start, end);
                path.Vehicle.MaxMoveAmount = path.Follow.GetPreCalculatedSplineLength();
            }
        }
    }
}