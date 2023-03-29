using UnityEditor;
using UnityEngine;

namespace TrafficSystem
{
    [CustomEditor(typeof(Anchor)), CanEditMultipleObjects]
    public class AnchorEditor : Editor
    {
        private bool isWaitingForClick = false;

        public override void OnInspectorGUI ()
        {
            Anchor anchor = (Anchor)target;
            DrawDefaultInspector();
            
            if (GUILayout.Button("Reset Handle Position"))
            {
                anchor.HandleAPosition = anchor.transform.localPosition + new Vector3(1, 0, 0);

                anchor.HandleBPosition = anchor.transform.localPosition + new Vector3(-1, 0, 0);
            }

            if (isWaitingForClick)
            {
                if (GUILayout.Button("Cancel"))
                {
                    isWaitingForClick = false;
                }
                EditorGUILayout.HelpBox("Click on an object in the Scene view.", MessageType.Info);
            }

            if (!isWaitingForClick)
            {
                if (GUILayout.Button("Pick a anchor to branch to"))
                {
                    isWaitingForClick = true;
                }
            }
        }

        private void OnSceneGUI()
        {
            Anchor anchor = (Anchor)target;

            if (isWaitingForClick)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Event currentEvent = Event.current;
                EventType eventType = currentEvent.type;

                if (eventType == EventType.MouseDown && currentEvent.button == 0)
                {
                    Ray mouseRay = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(mouseRay, out hit))
                    {
                        if (hit.transform.TryGetComponent(out Anchor branch))
                        {
                            anchor.Branches.Add(new Anchor.Branch
                            {
                                ToAnchor = branch,
                                NextPath = branch.GetComponentInParent<SplinePath>()
                            });
                        }
                        Repaint();
                        isWaitingForClick = false;
                    }
                }

                HandleUtility.Repaint();
            }
        }
    }
}