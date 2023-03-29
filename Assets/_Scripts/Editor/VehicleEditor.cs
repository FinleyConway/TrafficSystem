using UnityEditor;
using UnityEngine;

namespace TrafficSystem
{
    [CustomEditor(typeof(Vehicle))]
    public class VehicleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Vehicle vehicle = target as Vehicle;

            DrawDefaultInspector();
        }
    }
}