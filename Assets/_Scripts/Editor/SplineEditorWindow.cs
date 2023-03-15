using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TrafficSystem
{
    public class SplineEditorWindow : EditorWindow
    {
        [SerializeField] private SplinePath m_Spline;
        [SerializeField, NonReorderable] private List<SplinePath> m_ActiveSplines;

        [MenuItem("Tools/Spline Editor")]
        public static void Open()
        {
            GetWindow(typeof(SplineEditorWindow));
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            SerializedObject serializedObject = new SerializedObject(this);
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Spline"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ActiveSplines"));

            if (m_Spline == null)
            {
                if (GUILayout.Button("Create Spline Path"))
                {
                    GameObject path = new GameObject("Spline Path 0", typeof(SplinePath));
                    GameObject anchorObject = new GameObject("Anchor " + path.transform.childCount, typeof(Anchor));
                    anchorObject.transform.SetParent(path.transform, false);
                    m_Spline = path.GetComponent<SplinePath>();
                    m_Spline.GetAnchors().Add(anchorObject.GetComponent<Anchor>()); 
                    m_ActiveSplines.Add(m_Spline);
                    Selection.activeGameObject = anchorObject;
                }
            }
            else
            {
                EditorGUILayout.BeginVertical("box");
                DrawButtons();
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            SerializedObject serializedObject = new SerializedObject(this);

            if (m_Spline != null && m_Spline.GetAnchors().Count > 0)
            {
                Vector3 transformPosition = m_Spline.transform.position;

                foreach (SplinePath spline in m_ActiveSplines)
                {

                    // foreach anchor point
                    foreach (Anchor anchor in spline.GetAnchors())
                    {
                        // draw anchor positions
                        Handles.color = Color.red;
                        Handles.SphereHandleCap(0, transformPosition + anchor.transform.position, Quaternion.identity, 0.25f, EventType.Repaint);

                        // handle movement of positions
                        EditorGUI.BeginChangeCheck();
                        Vector3 newPosition = Handles.PositionHandle(transformPosition + anchor.transform.position, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(spline, "Change Anchor Position");
                            anchor.transform.position = newPosition - transformPosition;
                            spline.SetDirty();
                            serializedObject.Update();
                        }


                        // draw handle A
                        Handles.color = Color.blue;
                        Handles.SphereHandleCap(0, transformPosition + anchor.HandleAPosition, Quaternion.identity, 0.15f, EventType.Repaint);

                        // handle movement of positions
                        EditorGUI.BeginChangeCheck();
                        newPosition = Handles.PositionHandle(transformPosition + anchor.HandleAPosition, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(spline, "Change Anchor Handle A Position");
                            anchor.HandleAPosition = newPosition - transformPosition;
                            spline.SetDirty();
                            serializedObject.Update();
                        }


                        // draw handle B
                        Handles.color = Color.blue;
                        Handles.SphereHandleCap(0, transformPosition + anchor.HandleBPosition, Quaternion.identity, 0.15f, EventType.Repaint);

                        // handle movement of positions
                        EditorGUI.BeginChangeCheck();
                        newPosition = Handles.PositionHandle(transformPosition + anchor.HandleBPosition, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(spline, "Change Anchor Handle B Position");
                            anchor.HandleBPosition = newPosition - transformPosition;
                            spline.SetDirty();
                            serializedObject.Update();
                        }

                        // draw lines from handles to anchors
                        Handles.color = Color.black;
                        Handles.DrawLine(transformPosition + anchor.transform.position, transformPosition + anchor.HandleAPosition);
                        Handles.DrawLine(transformPosition + anchor.transform.position, transformPosition + anchor.HandleBPosition);
                    }

                    // Draw Bezier
                    for (int i = 0; i < spline.GetAnchors().Count - 1; i++)
                    {
                        Anchor anchor = spline.GetAnchors()[i];
                        Anchor nextAnchor = spline.GetAnchors()[i + 1];
                        Handles.DrawBezier(transformPosition + anchor.transform.position, transformPosition + nextAnchor.transform.position, transformPosition + anchor.HandleBPosition, transformPosition + nextAnchor.HandleAPosition, Color.green, null, 3f);
                    }

                    if (spline.IsLoopClosed() && spline.GetAnchors().Count >= 2)
                    {
                        // Spline is Closed Loop
                        Anchor anchor = spline.GetAnchors()[spline.GetAnchors().Count - 1];
                        Anchor nextAnchor = spline.GetAnchors()[0];
                        Handles.DrawBezier(transformPosition + anchor.transform.position, transformPosition + nextAnchor.transform.position, transformPosition + anchor.HandleBPosition, transformPosition + nextAnchor.HandleAPosition, Color.green, null, 3f);
                    }
                }
            }
        }

        private void DrawButtons()
        {
            if (GUILayout.Button("Add Anchor"))
            {
                AddAnchor();
            }
            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Anchor>())
            {
                if (GUILayout.Button("Remove Anchor"))
                {
                    RemoveAnchor();
                }
                if (GUILayout.Button("Add Branch Anchor"))
                {
                    AddBranch();
                }
            }
        }

        private void AddAnchor()
        {
            SplinePath parentPath = Selection.activeGameObject.transform.parent.GetComponent<SplinePath>();

            GameObject anchorObject = new GameObject("Anchor " + parentPath.transform.childCount, typeof(Anchor));
            anchorObject.transform.SetParent(parentPath.transform, false);

            Anchor anchor = anchorObject.GetComponent<Anchor>();

            if (parentPath.GetAnchors().Count >= 1)
            {
                Anchor lastAnchor = parentPath.GetAnchors()[parentPath.GetAnchors().Count - 1];

                anchor.transform.position = lastAnchor.transform.position + new Vector3(0, 0, 1);
                anchor.HandleAPosition = lastAnchor.HandleAPosition + new Vector3(0, 0, 1);
                anchor.HandleBPosition = lastAnchor.HandleBPosition + new Vector3(0, 0, 1);
            }

            parentPath.GetAnchors().Add(anchor);
            Selection.activeGameObject = anchor.gameObject;
        }

        private void RemoveAnchor()
        {
            Anchor selectedAnchor = Selection.activeGameObject.GetComponent<Anchor>();
            SplinePath parentPath = selectedAnchor.transform.parent.GetComponent<SplinePath>();

            if (parentPath.GetAnchors().Count <= 1) 
            {
                return;
            }

            parentPath.GetAnchors().Remove(selectedAnchor);
            if (parentPath.GetAnchors().Count >= 1)
            {
                Selection.activeGameObject = parentPath.GetAnchors()[parentPath.GetAnchors().Count - 1].gameObject;
            }

            DestroyImmediate(selectedAnchor.gameObject);
        }

        private void AddBranch()
        {
            GameObject parent = Selection.activeGameObject;
            GameObject splineObject = new GameObject("SplinePath Of " + parent.name, typeof(SplinePath));
            GameObject anchorObject = new GameObject("Anchor " + parent.transform.childCount, typeof(Anchor));

            anchorObject.transform.SetParent(splineObject.transform, false);

            Anchor anchor = anchorObject.GetComponent<Anchor>();

            anchor.transform.position = parent.transform.position;
            anchor.HandleAPosition = parent.GetComponent<Anchor>().HandleAPosition + new Vector3(0, 0, 1);
            anchor.HandleBPosition = parent.GetComponent<Anchor>().HandleBPosition + new Vector3(0, 0, 1);

            Anchor branchedAnchor = Selection.activeGameObject.GetComponent<Anchor>();
            branchedAnchor.Branches.Add(anchor);

            SplinePath path = splineObject.GetComponent<SplinePath>();
            path.GetAnchors().Add(anchor);

            Selection.activeGameObject.GetComponent<Anchor>().NewSpline.Add(path);
            m_ActiveSplines.Add(path);

            Selection.activeGameObject = anchor.gameObject;
        }
    }
}