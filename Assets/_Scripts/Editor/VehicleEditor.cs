using UnityEditor;

namespace TrafficSystem
{
    [CustomEditor(typeof(Vehicle)), CanEditMultipleObjects]
    public class VehicleEditor : Editor
    {
        public override void OnInspectorGUI ()
        {
            Vehicle vehicle = (Vehicle)target;

            DrawDefaultInspector();

            EditorGUILayout.LabelField("MilesPerHour: ", Vehicle.MsToMph(vehicle.MetresPerSecond).ToString());
        }
    }
}