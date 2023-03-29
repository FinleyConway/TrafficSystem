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


        }
    }
}