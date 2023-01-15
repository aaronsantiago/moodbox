using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ToonBoom.Harmony
{
    public class HarmonyProjectBinary : HarmonyProject
    {
        public static HarmonyProjectBinary CreateFromFile(string projectFolder)
        {
            byte[] bytes = Xml2Bin.ConvertToMemory(Path.GetFullPath(projectFolder));
            return CreateFromBytes(bytes);
        }
        public static HarmonyProjectBinary CreateFromBytes(byte[] projectBytes)
        {
            HarmonyProjectBinary project = CreateInstance<HarmonyProjectBinary>();
            HarmonyBinaryUtil.FillProjectFromBinary(project, projectBytes);
            project.ProjectBytes = projectBytes;

            return project;
        }

        [HideInInspector]
        public byte[] ProjectBytes;

        [ContextMenu("Load")]
        protected override void LoadFromSourceProject()
        {
            HarmonyBinaryUtil.FillProjectFromBinary(this, ProjectBytes);
            if(IsValid())
            {
                LoadProjectInNative();
            }
        }

        protected override void LoadProjectInNative()
        {
            if(IsLoadedInNative())
            {
                UnloadProjectInNative();
            }

            Debug.Log("Loading bytes project in native");
            byte[] bytes = ProjectBytes;
            int size = sizeof(byte) * bytes.Length;
            int id = GetNativeProjectId();
            IntPtr pointerToData = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, pointerToData, bytes.Length);
                HarmonyInternal.LoadProject(id, pointerToData, size);
            }
            finally
            {
                Marshal.FreeHGlobal(pointerToData);
            }

            _isLoadedInNative = true;
        }

        protected override void UnloadProjectInNative()
        {
            Debug.Log("Unloading bytes project in native");
            HarmonyInternal.UnloadProject(GetNativeProjectId());

            _isLoadedInNative = false;
        }
    }
}
