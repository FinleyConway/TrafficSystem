using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TrafficSystem
{
    [CustomEditor(typeof(SplinePath))]
    public class SplinePathEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SplinePath spline = (SplinePath)target;

            // button functionaility for adding a new anchor
            if (GUILayout.Button("Add Anchor"))
            {
                Undo.RecordObject(spline, "Add Anchor");
                spline.AddAnchor();
                spline.SetDirty();
                serializedObject.Update();
            }

            // button functionaility for removing the last anchor
            if (GUILayout.Button("Remove Last Anchor"))
            {
                if (spline.GetAnchors().Count > 1) return;

                Undo.RecordObject(spline, "Remove Last Anchor");
                spline.RemoveLastAnchor();
                spline.SetDirty();
                serializedObject.Update();
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HandleOffsetA"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HandleOffsetB"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_IsLoopClosed"));

            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            SplinePath spline = (SplinePath)target;

            Vector3 transformPosition = spline.transform.position;

            List<SplinePath.Anchor> anchorList = spline.GetAnchors();
            if (anchorList != null)
            {
                foreach (SplinePath.Anchor anchor in spline.GetAnchors())
                {
                    Handles.color = Color.white;
                    Handles.DrawWireCube(transformPosition + anchor.Position, Vector3.one * .5f);

                    EditorGUI.BeginChangeCheck();
                    Vector3 newPosition = Handles.PositionHandle(transformPosition + anchor.Position, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(spline, "Change Anchor Position");
                        anchor.Position = newPosition - transformPosition;
                        spline.SetDirty();
                        serializedObject.Update();
                    }



                    Handles.color = Color.green;
                    Handles.SphereHandleCap(0, transformPosition + anchor.HandleAPosition, Quaternion.identity, .5f, EventType.Repaint);

                    EditorGUI.BeginChangeCheck();
                    newPosition = Handles.PositionHandle(transformPosition + anchor.HandleAPosition, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(spline, "Change Anchor Handle A Position");
                        anchor.HandleAPosition = newPosition - transformPosition;
                        spline.SetDirty();
                        serializedObject.Update();
                    }



                    Handles.color = Color.blue;
                    Handles.SphereHandleCap(0, transformPosition + anchor.HandleBPosition, Quaternion.identity, .5f, EventType.Repaint);

                    EditorGUI.BeginChangeCheck();
                    newPosition = Handles.PositionHandle(transformPosition + anchor.HandleBPosition, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(spline, "Change Anchor Handle B Position");
                        anchor.HandleBPosition = newPosition - transformPosition;
                        spline.SetDirty();
                        serializedObject.Update();
                    }

                    Handles.color = Color.white;
                    Handles.DrawLine(transformPosition + anchor.Position, transformPosition + anchor.HandleAPosition);
                    Handles.DrawLine(transformPosition + anchor.Position, transformPosition + anchor.HandleBPosition);
                }



                // Draw Bezier
                for (int i = 0; i < spline.GetAnchors().Count - 1; i++)
                {
                    SplinePath.Anchor anchor = spline.GetAnchors()[i];
                    SplinePath.Anchor nextAnchor = spline.GetAnchors()[i + 1];
                    Handles.DrawBezier(transformPosition + anchor.Position, transformPosition + nextAnchor.Position, transformPosition + anchor.HandleBPosition, transformPosition + nextAnchor.HandleAPosition, Color.grey, null, 3f);
                }

                if (spline.IsLoopClosed())
                {
                    // Spline is Closed Loop
                    SplinePath.Anchor anchor = spline.GetAnchors()[spline.GetAnchors().Count - 1];
                    SplinePath.Anchor nextAnchor = spline.GetAnchors()[0];
                    Handles.DrawBezier(transformPosition + anchor.Position, transformPosition + nextAnchor.Position, transformPosition + anchor.HandleBPosition, transformPosition + nextAnchor.HandleAPosition, Color.grey, null, 3f);
                }
            }
        }
    }
}