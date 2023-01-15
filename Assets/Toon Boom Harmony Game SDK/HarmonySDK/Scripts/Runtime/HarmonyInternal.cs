using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ToonBoom.Harmony
{
    internal class HarmonyInternal
    {
#if (UNITY_IOS || UNITY_TVOS || UNITY_XBOX360) && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern float GetClipLength(int projectId, string clipName);

        [DllImport("__Internal")]
        public static extern int LoadProject(int projectId, IntPtr projectData, int projectDataLength);

        [DllImport("__Internal")]
        public static extern void ReloadSpreadsheets(int projectId, IntPtr spreadsheetData, int spreadsheetDataLength, int spreadsheetCount);

        [DllImport("__Internal")]
        public static extern void UnloadProject(int projectId);

        [DllImport("__Internal")]
        public static extern int UpdateRenderScript(int scriptId, int projectId, string clipName, string sheetResolution, float frame, int discretizationStep);

        [DllImport("__Internal")]
        public static extern int CreateRenderScript(int projectId, string clipName);

        [DllImport("__Internal")]
        public static extern void UpdateSkins(int scriptId, uint[] skins, int skinCount);

        [DllImport("__Internal")]
        public static extern void UnloadRenderScript(int scriptId);

        [DllImport("__Internal")]
        public static extern bool GetModelData(int scriptId, out IntPtr vertexData, out int vertexCount, out IntPtr indexData, out int indexCount, out IntPtr textureData, out int textureCount);

        [DllImport("__Internal")]
        public static extern bool GetBoneData(int scriptId, ref IntPtr boneData, ref int boneCount);
#elif UNITY_SWITCH && !UNITY_EDITOR
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern float GetClipLength(int projectId, string clipName);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LoadProject(int projectId, IntPtr projectData, int projectDataLength);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReloadSpreadsheets(int projectId, IntPtr spreadsheetData, int spreadsheetDataLength, int spreadsheetCount);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UnloadProject(int projectId);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern int UpdateRenderScript(int scriptId, int projectId, string clipName, string sheetResolution, float frame, int discretizationStep);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateRenderScript(int projectId, string clipName);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UpdateSkins(int scriptId, uint[] skins, int skinCount);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UnloadRenderScript(int scriptId);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetModelData(int scriptId, out IntPtr vertexData, out int vertexCount, out IntPtr indexData, out int indexCount, out IntPtr textureData, out int textureCount);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetBoneData(int scriptId, ref IntPtr boneData, ref int boneCount);
#else
        [DllImport("HarmonyRenderer")]
        public static extern float GetClipLength(int projectId, string clipName);

        [DllImport("HarmonyRenderer")]
        public static extern int LoadProject(int projectId, IntPtr projectData, int projectDataLength);

        [DllImport("HarmonyRenderer")]
        public static extern void ReloadSpreadsheets(int projectId, IntPtr spreadsheetData, int spreadsheetDataLength, int spreadsheetCount);

        [DllImport("HarmonyRenderer")]
        public static extern void UnloadProject(int projectId);

        [DllImport("HarmonyRenderer")]
        public static extern int UpdateRenderScript(int scriptId, int projectId, string clipName, string sheetResolution, float frame, int discretizationStep);

        [DllImport("HarmonyRenderer")]
        public static extern int CreateRenderScript(int projectId, string clipName);

        [DllImport("HarmonyRenderer")]
        public static extern void UpdateSkins(int scriptId, uint[] skins, int skinCount);

        [DllImport("HarmonyRenderer")]
        public static extern void UnloadRenderScript(int scriptId);

        [DllImport("HarmonyRenderer")]
        public static extern bool GetModelData(int scriptId, out IntPtr vertexData, out int vertexCount, out IntPtr indexData, out int indexCount, out IntPtr textureData, out int textureCount);

        [DllImport("HarmonyRenderer")]
        public static extern bool GetBoneData(int scriptId, ref IntPtr boneData, ref int boneCount);
#endif

        [StructLayout(LayoutKind.Sequential)]
        internal struct Header
        {
            //#pragma warning disable CS0649
            public readonly int Magicbyte;
            public readonly int MajorVersion;
            public readonly int MinorVersion;
            public readonly int Pad0;

            public Entry Clipnames;
            public Entry Sheetnames;

            public Entry Nodes;
            public Entry Skins;

            public Entry Groups;
            public Entry Clips;

            public Entry Spritesheets;
            public Entry Props;

            public Entry Anchors;
            public Entry Metas;

            public Entry CustomSprites;
            //#pragma warning restore

            [StructLayout(LayoutKind.Sequential, Size = 16)]
            public readonly struct Entry
            {
                public readonly long Count;
                public readonly long Offset;

                public Entry(long offset, long count)
                {
                    Count = count;
                    Offset = offset;
                }
            }

            internal static void FromBinary(byte[] binaryFile, out Header header)
            {
                GCHandle binaryFileHandle = GCHandle.Alloc(binaryFile, GCHandleType.Pinned);
                try
                {
                    header = Marshal.PtrToStructure<Header>(binaryFileHandle.AddrOfPinnedObject());
                }
                finally
                {
                    binaryFileHandle.Free();
                }
            }
        }

        public readonly struct BinaryTextureData
        {
            public readonly int TextureId;
            public readonly int Width;
            public readonly int Height;

            public BinaryTextureData(int textureId, int width, int height)
            {
                this.TextureId = textureId;
                this.Width = width;
                this.Height = height;
            }

            public static int GetMarshalSize()
            {
                int size = 0;
                size += sizeof(int); //Texture Id
                size += sizeof(int); //Texture Width
                size += sizeof(int); //Texture Height
                return size;
            }

            public void WriteToStream(BinaryWriter writer)
            {
                writer.Write(TextureId);
                writer.Write(Width);
                writer.Write(Height);
            }
        }

        public struct BinarySpriteData
        {
            public Rect Rect;
            public int TextureId;
            public double[] Matrix;

            public BinarySpriteData(Rect rect, int textureId, double[] matrix)
            {
                this.Rect = rect;
                this.TextureId = textureId;
                this.Matrix = matrix;
            }

            public static int GetMarshalSize()
            {
                int size = 0;
                size += sizeof(int) * 4; //Sprite Rect
                size += sizeof(int); //Sprite TextureId
                size += sizeof(double) * 16; //Sprite Matrix
                return size;
            }

            public void WriteToStream(BinaryWriter writer)
            {
                writer.Write((int)Rect.x);
                writer.Write((int)Rect.y);
                writer.Write((int)Rect.width);
                writer.Write((int)Rect.height);

                writer.Write(TextureId);

                HarmonyUtils.WriteBinaryMatrix(writer, Matrix);
            }
        }
    }
}


