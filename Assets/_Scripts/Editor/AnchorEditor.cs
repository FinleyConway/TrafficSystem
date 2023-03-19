using UnityEditor;
using UnityEngine;

namespace TrafficSystem
{
    [CustomEditor(typeof(Anchor))]
    public class AnchorEditor : Editor
    {
        public override void OnInspectorGUI ()
        {
            Anchor anchor= (Anchor)target;
            DrawDefaultInspector();
            
            if (GUILayout.Button("Reset Handle Position"))
            {
                anchor.HandleAPosition = anchor.transform.localPosition + new Vector3(1, 0, 0);

                anchor.HandleBPosition = anchor.transform.localPosition + new Vector3(-1, 0, 0);
            }
        }
    }
}