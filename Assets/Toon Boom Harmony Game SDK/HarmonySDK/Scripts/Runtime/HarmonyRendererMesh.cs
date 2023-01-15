using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

namespace ToonBoom.Harmony
{
    public partial class HarmonyRenderer
    {
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private Mesh _mesh;

        List<Texture2D> _orderedTextures = new List<Texture2D>();
        List<Texture2D> _orderedMasks = new List<Texture2D>();
        const int SHADER_ARRAY_SIZE = 32;
        Matrix4x4[] _boneMatrixArray = new Matrix4x4[SHADER_ARRAY_SIZE];

        private void AwakeMesh()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshFilter.hideFlags = HideFlags.NotEditable;

            _meshFilter.mesh = _mesh = new Mesh();
            _mesh.name = "Harmony Mesh";
            _mesh.MarkDynamic();
            _mesh.hideFlags = HideFlags.DontSave;

            _meshRenderer = GetComponent<MeshRenderer>();
            _meshRenderer.hideFlags = HideFlags.NotEditable;
        }

        /// <summary>
        /// Update Unity mesh
        /// </summary>
        private void UpdateMesh()
        {
            IntPtr vertexData, indexData, textureData;
            int vertexCount, indexCount, textureCount;
            bool gotModelData = HarmonyInternal.GetModelData(_nativeRenderScriptId, out vertexData, out vertexCount, out indexData, out indexCount, out textureData, out textureCount);
            if (gotModelData)
            {
                // read natives data
                ReadVertices(vertexData, vertexCount);
                ReadSubmeshIndicesAndTextures(indexData, indexCount, textureData, textureCount);
            }
            else
            {
                Debug.LogError("No model data", gameObject);
            }
        }

        private void DestroyMesh()
        {
            DestroyImmediate(_mesh);
        }

        /// <summary>
        /// Read mesh vertices data from native script
        /// </summary>
        private void ReadVertices(IntPtr vertexData, int vertexCount)
        {
            List<Vector3> vertices = UpdateBuffers.GetVertexBuffer();
            List<Color> colors = UpdateBuffers.GetColorBuffer();
            List<Vector2> uvs = UpdateBuffers.GetUVBuffer();
            List<Vector4> fxParams = UpdateBuffers.GetFxParamBuffer();
            List<Vector4> fxViewports = UpdateBuffers.GetFxViewportBuffer();
            List<Vector4> boneParams = UpdateBuffers.GetBoneParamBuffer();

            if (vertexCount > 0)
            {
                const int STRUCT_SIZE = 18;
                float[] nativeVertices = UpdateBuffers.GetNativeVertexBuffer(vertexCount * STRUCT_SIZE);
                Marshal.Copy(vertexData, nativeVertices, 0, vertexCount * STRUCT_SIZE);

                for (int i = 0; i < vertexCount; ++i)
                {
                    int offset = STRUCT_SIZE * i;

                    vertices.Add(new Vector3(nativeVertices[offset], nativeVertices[offset + 1], nativeVertices[offset + 2]));
                    colors.Add(new Color(Color.r, Color.g, Color.b, Color.a * nativeVertices[offset + 3]));
                    uvs.Add(new Vector2(nativeVertices[offset + 4], 1.0f - nativeVertices[offset + 5]));
                    fxParams.Add(new Vector4(nativeVertices[offset + 6], 1.0f - nativeVertices[offset + 7], nativeVertices[offset + 8], nativeVertices[offset + 9]));
                    // Next line we are inversing the UVs so we also need to inverse the order of the clamped parameters
                    fxViewports.Add(new Vector4(nativeVertices[offset + 10], 1.0f - nativeVertices[offset + 13], nativeVertices[offset + 12], 1.0f - nativeVertices[offset + 11]));
                    boneParams.Add(new Vector4(nativeVertices[offset + 14], nativeVertices[offset + 15], nativeVertices[offset + 16], nativeVertices[offset + 17]));
                }
            }

            _mesh.Clear();
            _mesh.SetVertices(vertices);
            _mesh.SetColors(colors);
            _mesh.SetUVs(0, uvs);
            _mesh.SetUVs(1, fxParams);
            _mesh.SetUVs(2, fxViewports);
            _mesh.SetUVs(3, boneParams);

            vertices.Clear();
            colors.Clear();
            uvs.Clear();
            fxParams.Clear();
            fxViewports.Clear();
            boneParams.Clear();
        }

        private static int[] ReadIndices(IntPtr indexData, int indexCount)
        {
            int[] nativeIndices = UpdateBuffers.GetNativeIndexBuffer(indexCount);
            if (indexCount > 0)
            {
                Marshal.Copy(indexData, nativeIndices, 0, indexCount);
            }
            return nativeIndices;
        }

        private static int[] ReadTextures(IntPtr textureData, int textureCount)
        {
            const int STRUCT_SIZE = 3;
            int[] nativeTextureMappingArray = UpdateBuffers.GetNativeTextureMappingBuffer(textureCount * STRUCT_SIZE);
            if (textureCount > 0)
            {
                Marshal.Copy(textureData, nativeTextureMappingArray, 0, textureCount * STRUCT_SIZE);
            }
            return nativeTextureMappingArray;
        }

        private void ReadSubmeshIndicesAndTextures(IntPtr indexData, int indexCount, IntPtr textureData, int textureCount)
        {

            int[] nativeIndices = ReadIndices(indexData, indexCount);
            int[] nativeTextureMappingArray = ReadTextures(textureData, textureCount);
            List<int> indices = UpdateBuffers.GetIndexBuffer();

            _orderedTextures.Clear();
            _orderedMasks.Clear();

            int indicesOffset = 0;
            _mesh.subMeshCount = nativeTextureMappingArray.Length / 3;

            for (int textureIndex = 0, i = 0; textureIndex < textureCount * 3; textureIndex += 3, i++)
            {
                int textureId = nativeTextureMappingArray[textureIndex];
                int maskTextureId = nativeTextureMappingArray[textureIndex + 1];
                int count = nativeTextureMappingArray[textureIndex + 2];

                _orderedTextures.Add(Project.Textures[textureId]);
                _orderedMasks.Add(Project.Textures[maskTextureId]);
                indices.Clear();

                for (int nativeIndex = 0; nativeIndex < count; nativeIndex++)
                {
                    if(indicesOffset + nativeIndex >= indexCount)
                    {
                        throw new IndexOutOfRangeException((indicesOffset + nativeIndex) + " >= " + indexCount);
                    }
                    indices.Add(nativeIndices[indicesOffset + nativeIndex]);
                }
                indicesOffset += count;

                _mesh.SetTriangles(indices, i);
            }


            List<Material> materials = UpdateBuffers.GetMaterialBuffer();
            _meshRenderer.GetSharedMaterials(materials);
            bool needReassign = false;
            if (materials.Count > _orderedTextures.Count)
            {
                needReassign = true;
                materials.RemoveRange(_orderedTextures.Count, materials.Count - _orderedTextures.Count);
            }
            else if(materials.Count < _orderedTextures.Count)
            {
                needReassign = true;
                for (int i = 0, len = _orderedTextures.Count - materials.Count; i < len; i++)
                {
                    materials.Add(Material);
                }
            }

            for (int i = 0, len = materials.Count; i < len; i++)
            {
                if (materials[i] != Material)
                {
                    materials[i] = Material;
                    needReassign = true;
                }
            }

            if(needReassign)
            {
                _meshRenderer.sharedMaterials = materials.ToArray();
            }
        }

        /// <summary>
        /// Read bones data from native and send it to shader
        /// </summary>
        private void UpdateBones()
        {
            IntPtr boneData = IntPtr.Zero;
            int boneCount = 0;
            if (HarmonyInternal.GetBoneData(_nativeRenderScriptId, ref boneData, ref boneCount))
            {
                if (boneCount > 0)
                {
                    const int STRUCT_SIZE = 16;
                    float[] nativeBonesArray = UpdateBuffers.GetNativeBoneBuffer(boneCount * STRUCT_SIZE);

                    Marshal.Copy(boneData, nativeBonesArray, 0, boneCount * STRUCT_SIZE);
                    for (int i = 0; i < boneCount && i < SHADER_ARRAY_SIZE; ++i)
                    {
                        HarmonyUtils.NativeArrayToMatrix(nativeBonesArray, i * STRUCT_SIZE, out _boneMatrixArray[i]);
                    }
                }
            }
            else
            {
                Debug.LogError("No Bones data", gameObject);
            }
        }

        private static class UpdateBuffers
        {
            [ThreadStatic]
            private static float[] _nativeVertexArray;
            internal static float[] GetNativeVertexBuffer(int neededCount)
            {
                if (_nativeVertexArray == null || _nativeVertexArray.Length < neededCount)
                {
                    _nativeVertexArray = new float[neededCount];
                }
                return _nativeVertexArray;
            }

            [ThreadStatic]
            private static int[] _nativeIndexArray;
            internal static int[] GetNativeIndexBuffer(int neededCount)
            {
                if (_nativeIndexArray == null || _nativeIndexArray.Length < neededCount)
                {
                    _nativeIndexArray = new int[neededCount];
                }
                return _nativeIndexArray;
            }

            [ThreadStatic]
            private static float[] _nativeBoneArray;
            internal static float[] GetNativeBoneBuffer(int neededCount)
            {
                if (_nativeBoneArray == null || _nativeBoneArray.Length < neededCount)
                {
                    _nativeBoneArray = new float[neededCount];
                }
                return _nativeBoneArray;
            }

            [ThreadStatic]
            private static int[] _nativeTextureMappingArray; //3 ints, i = textureId, i + 1 = maskTextureId, i + 2 = indicesCount
            internal static int[] GetNativeTextureMappingBuffer(int neededCount)
            {
                if (_nativeTextureMappingArray == null || _nativeTextureMappingArray.Length < neededCount)
                {
                    _nativeTextureMappingArray = new int[neededCount];
                }
                return _nativeTextureMappingArray;
            }

            [ThreadStatic]
            private static List<Vector3> _vertices;
            internal static List<Vector3> GetVertexBuffer(int neededCount = 8)
            {
                if (_vertices == null)
                {
                    _vertices = new List<Vector3>(neededCount);
                }
                else
                {
                    _vertices.Clear();
                }
                return _vertices;
            }

            [ThreadStatic]
            private static List<Color> _colors;
            internal static List<Color> GetColorBuffer(int neededCount = 8)
            {
                if (_colors == null)
                {
                    _colors = new List<Color>(neededCount);
                }
                else
                {
                    _colors.Clear();
                }
                return _colors;
            }

            [ThreadStatic]
            private static List<Vector2> _uvs;
            internal static List<Vector2> GetUVBuffer(int neededCount = 8)
            {
                if (_uvs == null)
                {
                    _uvs = new List<Vector2>(neededCount);
                }
                else
                {
                    _uvs.Clear();
                }
                return _uvs;
            }

            [ThreadStatic]
            private static List<Vector4> _fxParams;
            internal static List<Vector4> GetFxParamBuffer(int neededCount = 8)
            {
                if (_fxParams == null)
                {
                    _fxParams = new List<Vector4>(neededCount);
                }
                else
                {
                    _fxParams.Clear();
                }
                return _fxParams;
            }

            [ThreadStatic]
            private static List<Vector4> _fxViewports;
            internal static List<Vector4> GetFxViewportBuffer(int neededCount = 8)
            {
                if (_fxViewports == null)
                {
                    _fxViewports = new List<Vector4>(neededCount);
                }
                else
                {
                    _fxViewports.Clear();
                }
                return _fxViewports;
            }

            [ThreadStatic]
            private static List<Vector4> _boneParams;
            internal static List<Vector4> GetBoneParamBuffer(int neededCount = 8)
            {
                if (_boneParams == null)
                {
                    _boneParams = new List<Vector4>(neededCount);
                }
                else
                {
                    _boneParams.Clear();
                }
                return _boneParams;
            }

            [ThreadStatic]
            private static Matrix4x4[] _boneMatrixArray;
            internal static Matrix4x4[] GetBoneMatrixBuffer(int neededCount = 8)
            {
                if (_boneMatrixArray == null || _boneMatrixArray.Length != neededCount)
                {
                    _boneMatrixArray = new Matrix4x4[neededCount];
                }
                return _boneMatrixArray;
            }

            [ThreadStatic]
            private static List<int> _indices;
            internal static List<int> GetIndexBuffer(int neededCount = 8)
            {
                if (_indices == null)
                {
                    _indices = new List<int>(neededCount);
                }
                else
                {
                    _indices.Clear();
                }
                return _indices;
            }

            [ThreadStatic]
            private static List<Texture2D> _orderedTextures;
            internal static List<Texture2D> GetOrderedTextureBuffer(int neededCount = 8)
            {
                if (_orderedTextures == null)
                {
                    _orderedTextures = new List<Texture2D>(neededCount);
                }
                else
                {
                    _orderedTextures.Clear();
                }
                return _orderedTextures;
            }

            [ThreadStatic]
            private static List<Texture2D> _orderedMasks;
            internal static List<Texture2D> GetOrderedMaskBuffer(int neededCount = 8)
            {
                if (_orderedMasks == null)
                {
                    _orderedMasks = new List<Texture2D>(neededCount);
                }
                else
                {
                    _orderedMasks.Clear();
                }
                return _orderedMasks;
            }

            [ThreadStatic]
            private static List<Material> _materials;
            internal static List<Material> GetMaterialBuffer(int neededCount = 8)
            {
                if (_materials == null)
                {
                    _materials = new List<Material>(neededCount);
                }
                else
                {
                    _materials.Clear();
                }
                return _materials;
            }
        }
    }
}
