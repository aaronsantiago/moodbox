using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ToonBoom.Harmony
{
    /*!
     *  @class HarmonyRenderer
     *  Main Harmony game object component.
     *  This script will calculate the mesh, uvs, bones data
     *  and send it the the shader
     */
    [ExecuteInEditMode]
    [AddComponentMenu("Harmony/Core/HarmonyRenderer")]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public partial class HarmonyRenderer : MonoBehaviour
    {
        private const int INVALID_RENDER_SCRIPT = -1;

        // script id used for call to native code
        private int _nativeRenderScriptId = INVALID_RENDER_SCRIPT;

        public HarmonyProject Project;
        private HarmonyProject _loadedProject;

        [Header("Rendering")]
        public int ResolutionIndex;
        private int _lastResolutionIndex;

        [FormerlySerializedAs("material")]
        public Material Material;

        //  Color of the layer.  Will be multiplied with the texture color.
        [FormerlySerializedAs("color")]
        public Color Color = Color.white;
        private Color _lastColor = Color.white;

        // set the nb of discretization step, will add definition to the mesh to help with bone animation
        [FormerlySerializedAs("discretizationStep")]
        [Range(1, 50)]
        public int DiscretizationStep = 4;
        private int _lastDiscretizationStep = 0;

        [Header("Animation")]
        [FormerlySerializedAs("AnimatedClipIndex")]
        public int CurrentClipIndex = 0;
        private int _lastClip = -1;

        private bool bypassUpdateSkins = false;

        public ClipData CurrentClip
        {
            get => Project.GetClipByIndex(CurrentClipIndex);
        }

        private MaterialPropertyBlock _globalPropertyBlock;

        // current frame where at, will mostly be driven by the animator
        [FormerlySerializedAs("Frame")]
        public float CurrentFrame = 1;
        private float _lastFrame = -1;

        [Header("Animation Generation")]
        public HarmonyAnimationSettings AnimationSettings;

        protected void OnEnable()
        {
            if (Project == null || !Project.TryToLoadNative())
            {
                enabled = false;
                return;
            }

            if (_meshRenderer == null)
            {
                AwakeMesh();
            }

            _meshRenderer.enabled = true;

            _globalPropertyBlock = new MaterialPropertyBlock();


            // make sure the native code is initialized for this instance
            CreateRenderScript();
        }

        protected void OnDisable()
        {
            if (_meshRenderer != null)
            {
                _meshRenderer.enabled = false;
            }

            DestroyRenderScript();
        }

        protected void CreateRenderScript()
        {
            if (_nativeRenderScriptId == INVALID_RENDER_SCRIPT) //-1 Should always be the case, potentially remove
            {
                _nativeRenderScriptId = Project.CreateRenderScript(CurrentClipIndex);
            }

            // Pre-warm some stuff
            if (Application.isPlaying)
            {
                Project.UpdateRenderScript(_nativeRenderScriptId, CurrentClipIndex, ResolutionIndex, CurrentFrame, DiscretizationStep);
            }

            _loadedProject = Project;

            MarkDirty();
        }

        protected void DestroyRenderScript()
        {
            //  Free render scripts.  They'll be recreated if the game
            //  object is reactivated.  Clips and sprite sheet still
            //  remain loaded in memory.
            if (_nativeRenderScriptId != INVALID_RENDER_SCRIPT)
            {
                HarmonyProject.UnloadRenderScript(_nativeRenderScriptId);
                _nativeRenderScriptId = INVALID_RENDER_SCRIPT;
            }
        }

        protected void OnDestroy()
        {
            DestroyMesh();
        }

        protected void LateUpdate()
        {
            if (Project != _loadedProject)
            {
                Project.TryToLoadNative();

                DestroyRenderScript();
                CreateRenderScript();
            }

            UpdateRenderer();
        }

        public void MarkDirty()
        {
            _lastFrame = CurrentFrame - 1;
        }

        public void bypassForSkins()
        {
          bypassUpdateSkins = true;
        }

        public void UpdateRenderer()
        {
            bool scriptIsDirty = false;

            // update native skins values
            if (UpdateSkins() || bypassUpdateSkins)
            {
                scriptIsDirty = true;
                bypassUpdateSkins = false;
                HarmonyInternal.UpdateSkins(_nativeRenderScriptId, _skins, _skins.Length);
            }

            scriptIsDirty = scriptIsDirty ||
                Color != _lastColor ||
                CurrentFrame != _lastFrame ||
                CurrentClipIndex != _lastClip ||
                ResolutionIndex != _lastResolutionIndex ||
                DiscretizationStep != _lastDiscretizationStep;


            if (scriptIsDirty)
            {
                float duration = Project.GetClipByIndex(CurrentClipIndex).FrameCount;
                CurrentFrame = Mathf.Clamp(CurrentFrame, 1, (int)duration);

                Project.UpdateRenderScript(_nativeRenderScriptId, CurrentClipIndex, ResolutionIndex, CurrentFrame, DiscretizationStep);
                _lastColor = Color;
                _lastFrame = CurrentFrame;
                _lastClip = CurrentClipIndex;
                _lastResolutionIndex = ResolutionIndex;
                _lastDiscretizationStep = DiscretizationStep;
                UpdateMesh();
                UpdateBones();
            }

            _meshRenderer.GetPropertyBlock(_globalPropertyBlock);
            List<Material> materials = UpdateBuffers.GetMaterialBuffer();
            _meshRenderer.GetSharedMaterials(materials);
            for (int i = 0, len = materials.Count; i < len; i++)
            {
                _globalPropertyBlock.SetMatrixArray("_Bones", _boneMatrixArray);
                _globalPropertyBlock.SetTexture("_MainTex", _orderedTextures[i]);
                _globalPropertyBlock.SetTexture("_MaskTex", _orderedMasks[i]);
                _meshRenderer.SetPropertyBlock(_globalPropertyBlock, i);
            }
            materials.Clear();
        }
    }
}
