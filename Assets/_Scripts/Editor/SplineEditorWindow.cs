using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TrafficSystem
{
    public class SplineEditorWindow : EditorWindow
    {
        [SerializeField] private SplinePath m_Spline;
        [SerializeField] private List<SplinePath> m_ActiveSplines;

        [MenuItem("Tools/Spline Editor")]
        public static void Open()
        {
            GetWindow(typeof(SplineEditorWindow), false, "Spline Manager");
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
                    // create spline and anchor
                    GameObject path = new GameObject("Spline Path Root", typeof(SplinePath));
                    GameObject anchorObject = new GameObject("Anchor " + path.transform.childCount, typeof(Anchor));

                    anchorObject.transform.SetParent(path.transform, false);

                    m_Spline = path.GetComponent<SplinePath>();
                    m_Spline.Anchors.Add(anchorObject.GetComponent<Anchor>()); 
                    m_ActiveSplines.Add(m_Spline);

                    Selection.activeGameObject = path;
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

            if (m_Spline != null && m_Spline.Anchors.Count > 0)
            {
                Vector3 transformPosition = m_Spline.transform.position;

                foreach (SplinePath spline in m_ActiveSplines)
                {
                    // foreach anchor point
                    foreach (Anchor anchor in spline.Anchors)
                    {
                        // draw anchor positions
                        Handles.color = Color.red;
                        Handles.SphereHandleCap(0, anchor.transform.position, Quaternion.identity, 0.15f, EventType.Repaint);

                        // handle movement of positions
                        EditorGUI.BeginChangeCheck();
                        Vector3 newPosition = Handles.PositionHandle(anchor.transform.position, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(spline, "Change Anchor Position");
                            anchor.transform.position = newPosition;
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
                        Handles.DrawLine(anchor.transform.position, transformPosition + anchor.HandleAPosition);
                        Handles.DrawLine(anchor.transform.position, transformPosition + anchor.HandleBPosition);

                        for (int i = 0; i < anchor.Branches.Count; i++)
                        {
                            Anchor anchorTo = anchor.Branches[i].ToAnchor;
                            Handles.DrawBezier(anchor.transform.position, anchorTo.transform.position, transformPosition + anchor.HandleBPosition, transformPosition + anchorTo.HandleAPosition, Color.green, null, 3f);
                        }
                    }

                    // Draw Bezier
                    for (int i = 0; i < spline.Anchors.Count - 1; i++)
                    {
                        Anchor anchor = spline.Anchors[i];
                        Anchor nextAnchor = spline.Anchors[i + 1];
                        Handles.DrawBezier(anchor.transform.position, nextAnchor.transform.position, transformPosition + anchor.HandleBPosition, transformPosition + nextAnchor.HandleAPosition, Color.green, null, 3f);
                    }

                    if (spline.IsLoopClosed && spline.Anchors.Count >= 2)
                    {
                        // Spline is Closed Loop
                        Anchor anchor = spline.Anchors[spline.Anchors.Count - 1];
                        Anchor nextAnchor = spline.Anchors[0];
                        Handles.DrawBezier(anchor.transform.position, nextAnchor.transform.position, transformPosition + anchor.HandleBPosition, transformPosition + nextAnchor.HandleAPosition, Color.green, null, 3f);
                    }
                }
            }
        }

        private void DrawButtons()
        {
            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Anchor>())
            {
                if (GUILayout.Button("Add Anchor"))
                {
                    AddAnchor();
                }
                if (GUILayout.Button("Remove Anchor"))
                {
                    RemoveAnchor();
                }
                if (GUILayout.Button("Add Branch"))
                {
                    AddBranch();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select an anchor to add an anchor or branch or to remove an anchor.", MessageType.Info);
            }

            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<SplinePath>())
            {
                if (GUILayout.Button("Remove Branch"))
                {
                    RemoveBranch();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select a spline to remove a branch.", MessageType.Info);
            }
        }

        // Create a spline anchor
        private void AddAnchor()
        {
            GameObject selectedObj = Selection.activeGameObject;
            SplinePath parentPath = null;

            if (selectedObj != null && selectedObj.TryGetComponent(out SplinePath path)) 
            {
                parentPath = path;
            }
            else if (selectedObj != null && selectedObj.transform.parent.TryGetComponent(out SplinePath path1))
            {
                parentPath = path1;
            }

            // create anchor object
            GameObject anchorObject = new GameObject("Anchor " + parentPath.transform.childCount, typeof(Anchor));
            anchorObject.transform.SetParent(parentPath.transform, false);

            Anchor anchor = anchorObject.GetComponent<Anchor>();

            // offset anchor if there is one or more anchor
            if (parentPath.Anchors.Count >= 1)
            {
                Anchor lastAnchor = parentPath.Anchors[parentPath.Anchors.Count - 1];

                anchor.transform.position = lastAnchor.transform.position + new Vector3(0, 0, 1);
                anchor.HandleAPosition = lastAnchor.HandleAPosition + new Vector3(0, 0, 1);
                anchor.HandleBPosition = lastAnchor.HandleBPosition + new Vector3(0, 0, 1);
            }

            parentPath.Anchors.Add(anchor);
            Selection.activeGameObject = anchor.gameObject;
        }

        // Remove anchor from spline
        private void RemoveAnchor()
        {
            Anchor selectedAnchor = Selection.activeGameObject.GetComponent<Anchor>();
            SplinePath parentPath = selectedAnchor.transform.parent.GetComponent<SplinePath>();

            // make sure that there is always 1 anchor
            if (parentPath.Anchors.Count <= 1) 
            {
                return;
            }

            // remove anchor from list and set previous anchor if any to the next selected object
            parentPath.Anchors.Remove(selectedAnchor);
            if (parentPath.Anchors.Count >= 1)
            {
                Selection.activeGameObject = parentPath.Anchors[parentPath.Anchors.Count - 1].gameObject;
            }

            // remove all branches if this anchor has any
            if (selectedAnchor.Branches.Count > 0)
            {
                foreach (Anchor.Branch branch in selectedAnchor.Branches)
                {
                    branch.NextPath.Anchors.Remove(selectedAnchor);
                }
                selectedAnchor.Branches.Clear();
            }

            DestroyImmediate(selectedAnchor.gameObject);
        }

        private void AddBranch()
        {
            GameObject branchFromAnchor = Selection.activeGameObject;

            // create scene objects
            GameObject splineObject = new GameObject("SplinePath Of " + branchFromAnchor.name, typeof(SplinePath));
            GameObject anchorObject = new GameObject("Anchor " + splineObject.transform.childCount, typeof(Anchor));

            splineObject.transform.SetParent(branchFromAnchor.transform, false);
            splineObject.transform.position = branchFromAnchor.transform.parent.position;

            anchorObject.transform.SetParent(splineObject.transform, false);

            Anchor anchor = anchorObject.GetComponent<Anchor>();
            Anchor branchedAnchor = branchFromAnchor.GetComponent<Anchor>();

            // offset anchor from branched anchor
            anchor.transform.position = branchFromAnchor.transform.position + new Vector3(0, 0, 1);
            anchor.HandleAPosition = branchedAnchor.HandleAPosition + new Vector3(0, 0, 1);
            anchor.HandleBPosition = branchedAnchor.HandleBPosition + new Vector3(0, 0, 1);

            // add branched and new anchor to spline
            SplinePath path = splineObject.GetComponent<SplinePath>();
            path.Anchors.Add(branchedAnchor);
            path.Anchors.Add(anchor);

            // set branch values to branched from anchor
            branchedAnchor.Branches.Add(new Anchor.Branch { NextPath = path, ToAnchor = anchor });
            m_ActiveSplines.Add(path);

            Selection.activeGameObject = anchor.gameObject;
        }

        private void RemoveBranch()
        {
            SplinePath path = Selection.activeGameObject.GetComponent<SplinePath>();

            Anchor startSplineAnchor = path.Anchors[1];
            Anchor branchFromAnchor = path.Anchors[0];

            foreach (Anchor.Branch branch in branchFromAnchor.Branches)
            {
                if (branch.ToAnchor == startSplineAnchor)
                {
                    branchFromAnchor.Branches.Remove(branch);
                    break;
                }
            }

            m_ActiveSplines.Remove(path);

            DestroyImmediate(path.gameObject);
        }
    }
}