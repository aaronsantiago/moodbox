using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace ToonBoom.Harmony
{
    [CustomEditor(typeof(HarmonyAnchor))]
    public class HarmonyAnchorEditor : Editor
    {
        private class Internal
        {
            [DllImport("HarmonyRenderer")]
            public static extern bool CalculateLocatorTransform(string projectFolder, string clipName, float frame, string locatorName, [In, Out] float[] position, [In, Out] float[] rotation, [In, Out] float[] scale);
        }

        SerializedProperty targetNodeName;
        string[] anchorNames;

        void OnEnable()
        {
            //  Prop reference in project.
            targetNodeName = serializedObject.FindProperty(nameof(HarmonyAnchor.NodeName));
            anchorNames = null;
        }

        public override void OnInspectorGUI()
        {
            HarmonyAnchor anchor = target as HarmonyAnchor;
            if (anchor == null || !anchor.IsValid())
                return;

            HarmonyRenderer renderer = anchor.GetComponentInParent<HarmonyRenderer>();
            if (anchorNames == null)
            {
                anchorNames = renderer.Project.AnchorsMeta.Select(x => x.NodeName).OrderBy(x => x).ToArray();
            }

            int index = EditorGUILayout.Popup("name", Array.IndexOf(anchorNames, anchor.NodeName), anchorNames);
            targetNodeName.stringValue = anchorNames[index >= 0 && index < anchorNames.Length ? index : 0];

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        static void RenderSelectedAnchorControls(Transform objectTransform, GizmoType gizmoType)
        {
            RenderAnchorControls(objectTransform, gizmoType, true);
        }


        static void RenderAnchorControls(Transform objectTransform, GizmoType gizmoType, bool selected)
        {
            GameObject gameObject = objectTransform.gameObject;
            HarmonyAnchor harmonyAnchor = gameObject.GetComponent<HarmonyAnchor>();
            if (harmonyAnchor == null || !harmonyAnchor.IsValid())
                return;


            float arrowLength = HandleUtility.GetHandleSize(objectTransform.position) * 0.3f;
            float circleRadius = arrowLength * 0.5f;

            Handles.color = selected ? Color.cyan : Color.red;

            Handles.DrawWireDisc(objectTransform.position,
                                 Vector3.forward,
                                 circleRadius);

            Handles.ArrowHandleCap(0, objectTransform.position - objectTransform.right * circleRadius, Quaternion.LookRotation(objectTransform.right), arrowLength * 1.25f, EventType.Repaint);
        }
    }
}
