using UnityEditor;
using UnityEngine;

namespace TrafficSystem
{
    [CustomEditor(typeof(Vehicle)), CanEditMultipleObjects]
    public class VehicleEditor : Editor
    {
    }
}