using UnityEditor;
using UnityEngine;

namespace TrafficSystem
{
    [CustomEditor(typeof(Vehicle))]
    public class VehicleEditor : Editor
    {
        private SplinePath m_DefaultSpline;

        private void Awake()
        {
            Vehicle vehicle = (Vehicle)target;
            m_DefaultSpline = vehicle.m_Spline;
        }

        public override void OnInspectorGUI()
        {
            Vehicle vehicle = (Vehicle)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Reset Car"))
            {
                vehicle.m_MoveAmount = 0;
                vehicle.m_Spline = m_DefaultSpline;
                vehicle.m_MaxMoveAmount = m_DefaultSpline.GetSplineLength();
            }
        }
    }
}