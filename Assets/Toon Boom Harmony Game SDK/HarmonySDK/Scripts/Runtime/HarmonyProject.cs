using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ToonBoom.Harmony
{
    /// <summary>
    /// Project data used mostly for interface
    /// </summary>
    [Serializable]
    public abstract class HarmonyProject : ScriptableObject
    {
        public List<ClipData> Clips;
        public List<ClipData> Props;
        public List<HarmonyNode> Nodes; // all the drawings nodes data
        public List<string> Skins; // all the skins used by all the nodes
        public List<string> Groups; // all the groups used by all the nodes 
        public List<AnchorMeta> AnchorsMeta;
        public List<PropMeta> PropsMeta;
        public List<GenericMeta> GenericMeta;
        public List<Spritesheet> SheetResolutions;
        public List<CustomSpriteData> CustomSprites;

        [NonSerialized]
        protected bool _isLoadedInNative;

        [NonSerialized]
        protected int _activeResolution = -1;

        public ClipData GetClipByIndex(int index)
        {
            return Clips[Mathf.Clamp(index, 0, Clips.Count - 1)];
        }

        public Spritesheet GetResolutionByIndex(int index)
        {
            return SheetResolutions[Mathf.Clamp(index, 0, SheetResolutions.Count - 1)];
        }

        [NonSerialized]
        private List<Texture2D> _textures;
        public IReadOnlyList<Texture2D> Textures
        {
            get
            {
                return _textures;
            }
        }

        public void OnEnable()
        {
            if (IsValid() && !IsLoadedInNative())
            {
                LoadProjectInNative();
            }
        }
        public void OnDisable()
        {
            if (IsLoadedInNative())
            {
                UnloadProjectInNative();
            }
        }

        protected abstract void LoadFromSourceProject();
        protected abstract void LoadProjectInNative();
        protected abstract void UnloadProjectInNative();

        public bool IsValid()
        {
            if(Clips == null || Clips.Count <= 0 || SheetResolutions == null || SheetResolutions.Count <= 0)
            {
                return false;
            }

            return true;
        }

        public bool IsLoadedInNative()
        {
            return _isLoadedInNative;
        }

        public bool TryToLoadNative()
        {
            if (IsLoadedInNative()) return true;
            if (!IsValid()) return false;
            LoadProjectInNative();
            return _isLoadedInNative;
        }

        internal int GetNativeProjectId()
        {
            return GetInstanceID();
        }

        internal int CreateRenderScript(int currentClipIndex)
        {
            string clipName = GetClipByIndex(currentClipIndex).FullName;
            return HarmonyInternal.CreateRenderScript(GetNativeProjectId(), clipName);
        }

        internal int UpdateRenderScript(int nativeRenderScriptId, int currentClipIndex, int currentResolution, float currentFrame, int currentDiscretizationStep)
        {
            MakeSureTexturesAreUpToDate(currentResolution);
            string clipName = GetClipByIndex(currentClipIndex).FullName;
            string resolution = GetResolutionByIndex(currentResolution).ResolutionName;

            return HarmonyInternal.UpdateRenderScript(nativeRenderScriptId, GetNativeProjectId(), clipName, resolution, currentFrame, currentDiscretizationStep);
        }

        internal static void UnloadRenderScript(int nativeRenderScriptId)
        {
            HarmonyInternal.UnloadRenderScript(nativeRenderScriptId);
        }

        protected void MakeSureTexturesAreUpToDate(int activeResolution, bool force = false)
        {
            if (_textures == null || _textures.Count == 0 || activeResolution != _activeResolution || force)
            {
                Spritesheet resolution = GetResolutionByIndex(activeResolution);
                if(resolution.HasValidSprites())
                {
                    if (_textures == null)
                    {
                        _textures = new List<Texture2D>();
                    }
                    _textures.Clear();

                    for (int i = 0, len = resolution.Sprites.Count; i < len; i++)
                    {
                        Sprite sprite = resolution.Sprites[i];
                        if (!_textures.Contains(sprite.texture))
                        {
                            _textures.Add(sprite.texture);
                        }
                    }
                    _activeResolution = activeResolution;

                    ReloadNativeSpritesheetsFromCustom();
                }
            }
        }

        /// <summary>
        /// Loads sprites from a project folder at runtime.
        /// In editor, it is preferable to use a HarmonyProjectImport to automatically load assets and create sprite atlases
        /// </summary>
        /// <param name="projectFolder">The root XML project folder</param>
        public void LoadSprites(string projectFolder)
        {
            bool useSprites = Directory.Exists(projectFolder + "/sprites/");

            //Pair atlases to resolutions
            for (int i = 0, len = SheetResolutions.Count; i < len; i++)
            {
                Spritesheet resolution = SheetResolutions[i];
                resolution.Sprites.Clear();

                if (useSprites)
                {
                    string resolutionFolder = projectFolder + "/sprites/" + resolution.SheetName + "/" + resolution.ResolutionName;
                    foreach (string file in Directory.GetFiles(resolutionFolder, "*.png"))
                    {
                        Sprite importSprite = LoadSprite(file);
                        if (importSprite != null)
                        {
                            resolution.Sprites.Add(importSprite);
                        }
                    }
                }
                else
                {
                    string resolutionFile = projectFolder + "/spriteSheets/" + resolution.SheetName + "-" + resolution.ResolutionName + ".png";
                    Sprite importSprite = LoadSprite(resolutionFile);
                    if (importSprite != null)
                    {
                        resolution.Sprites.Add(importSprite);
                    }
                }
            }
        }

        private Sprite LoadSprite(string file)
        {
            if (!File.Exists(file))
                return null;

            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
            texture.LoadImage(File.ReadAllBytes(file));
            Sprite result = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            result.name = Path.GetFileName(file);

            return result;
        }

        public bool IsUsingCustomSprites()
        {
            return CustomSprites != null && CustomSprites.Count > 0;
        }

        private void ReloadNativeSpritesheetsFromCustom()
        {
            if (IsUsingCustomSprites())
            {
                List<HarmonyBinarySpriteSheet> spritesheetBuffer = Buffers.GetSpritesheetBuffer(SheetResolutions.Count);

                try
                {
                    int size = 0;
                    for (int i = 0, len = SheetResolutions.Count; i < len; i++)
                    {
                        HarmonyBinarySpriteSheet sheet = HarmonyBinarySpriteSheet.MakeFromCustomSprites(
                            SheetResolutions[i].Sprites,
                            SheetResolutions[i].SheetName,
                            SheetResolutions[i].ResolutionName,
                            CustomSprites);
                        size += sheet.GetMarshalSize();
                        spritesheetBuffer.Add(sheet);
                    }

                    byte[] sheetBytes = new byte[size];
                    using (MemoryStream stream = new MemoryStream(sheetBytes))
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        for (int i = 0, len = spritesheetBuffer.Count; i < len; i++)
                        {
                            spritesheetBuffer[i].StoreToMemory(writer);
                        }
                    }

                    IntPtr pointerToData = Marshal.AllocHGlobal(size);
                    try
                    {
                        Marshal.Copy(sheetBytes, 0, pointerToData, sheetBytes.Length);
                        HarmonyInternal.ReloadSpreadsheets(GetNativeProjectId(), pointerToData, size, spritesheetBuffer.Count);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(pointerToData);
                    }
                }
                finally
                {
                    spritesheetBuffer.Clear();
                }
            }
        }

        private static class Buffers
        {
            [ThreadStatic]
            private static List<HarmonyBinarySpriteSheet> _spritesheetBuffer;
            internal static List<HarmonyBinarySpriteSheet> GetSpritesheetBuffer(int neededCount = 8)
            {
                if (_spritesheetBuffer == null)
                {
                    _spritesheetBuffer = new List<HarmonyBinarySpriteSheet>(neededCount);
                }
                else
                {
                    _spritesheetBuffer.Clear();
                }
                return _spritesheetBuffer;
            }
        }
    }
}
