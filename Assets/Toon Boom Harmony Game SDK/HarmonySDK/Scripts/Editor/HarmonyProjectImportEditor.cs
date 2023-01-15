using UnityEditor;
using UnityEngine;

namespace ToonBoom.Harmony
{
    [CustomEditor(typeof(HarmonyProjectImport))]
    public class HarmonyProjectImportEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            do
            {
                using (new EditorGUI.DisabledScope(iterator.propertyPath == "m_Script"))
                    EditorGUILayout.PropertyField(iterator, true);

                if (iterator.propertyPath == nameof(HarmonyProjectImport.HarmonyXMLProjectPath))
                {
                    if (GUILayout.Button("Browse..."))
                    {
                        string harmonyProjectExportPath = EditorUtility.OpenFolderPanel("Select Harmony Exported Project Folder", "", "");
                        if (!string.IsNullOrEmpty(harmonyProjectExportPath))
                        {
                            string relativePath = FileUtil.GetProjectRelativePath(harmonyProjectExportPath);
                            iterator.stringValue = string.IsNullOrEmpty(relativePath) ? harmonyProjectExportPath : relativePath;
                        }
                    }
                }
                else if (iterator.propertyPath == nameof(HarmonyProjectImport.OutputPath))
                {
                    if (GUILayout.Button("Browse..."))
                    {
                        string outputPath = EditorUtility.SaveFilePanelInProject("Save Harmony Project", target.name + "Project", "asset", "Save Harmony Project");
                        if (!string.IsNullOrEmpty(outputPath))
                        {
                            iterator.stringValue = outputPath;
                        }
                    }
                }
            }
            while (iterator.NextVisible(false));

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Import"))
            {
                HarmonyProjectImport import = target as HarmonyProjectImport;
                import.DoImport();
            }
        }
    }
}