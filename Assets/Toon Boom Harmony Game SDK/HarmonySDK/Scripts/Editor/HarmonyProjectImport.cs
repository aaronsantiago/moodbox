using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using ContextMenu = UnityEngine.ContextMenu;

namespace ToonBoom.Harmony
{
    /// <summary>
    /// A scriptable object holding all the relevant import settings for a harmony project.
    /// This makes it easy to re-import existing projects to the game.
    /// </summary>
    [CreateAssetMenu]
    public class HarmonyProjectImport : ScriptableObject
    {
        private static readonly string[] _mandatoryFiles = new string[]
        {
        "animation.xml",
        "drawingAnimation.xml",
        "skeleton.xml",
        "stage.xml"
        };

        private const float TOTAL_IMPORT_STEPS = 8;

        [Header("Harmony Project")]
        public string HarmonyXMLProjectPath;
        public string OutputPath;
        [Header("Prefab Generation")]
        public bool GeneratePrefab = true;
        public GameObject ExistingPrefab;
        [Header("Animation Generation")]
        public HarmonyAnimationSettings AnimationSettings;
        [Tooltip("The folder relative to the current asset where animations will be created if they do not exist.")]
        public string AnimationFolderName = "Animations";

        public string HarmonyXMLProjectPathClean
        {
            get
            {
                string cleanPath = HarmonyXMLProjectPath.Replace('\\', '/');
                if (!cleanPath.EndsWith("/"))
                {
                    cleanPath += '/';
                }

                return cleanPath;
            }
        }

        public string ProjectName
        {
            get { return Path.GetFileNameWithoutExtension(OutputPath); }
        }

        public string ProjectFolder
        {
            get { return Path.GetDirectoryName(OutputPath).Replace('\\', '/'); }
        }

        public string HarmonySpritesPath
        {
            get { return HarmonyXMLProjectPathClean + "sprites/"; }
        }

        public string HarmonySpritesheetPath
        {
            get { return HarmonyXMLProjectPathClean + "spriteSheets/"; }
        }

        [ContextMenu("Do Import")]
        public void DoImport()
        {
            try
            {
                ValidateXMLFolder();

                //Run Xml2Bin and import bytes
                byte[] bytes = RunXML2Bin(HarmonyXMLProjectPathClean);

                //Make project file
                HarmonyProjectBinary project = CreateProject(bytes);

                // NOTE: Need to do this to prevent Unity from clearing it out during sprite import
                project.hideFlags = HideFlags.DontUnloadUnusedAsset;

                // Import textures
                ImportSprites(project);

                // Reset the hideflags from above
                project.hideFlags = 0;

                SaveProjectAsset(project);

                if (GeneratePrefab)
                {
                    //Make or update prefab
                    GameObject prefab = BuildOrUpdatePrefabAsset(project);
                    Selection.activeObject = prefab;
                }
                else
                {
                    Selection.activeObject = project;
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private bool ValidateXMLFolder()
        {
            if (string.IsNullOrEmpty(HarmonyXMLProjectPath))
            {
                EditorUtility.DisplayDialog("Error loading Harmony Project",
                        "Harmony XML Project Path is blank",
                        "OK");
                return false;
            }

            for (int i = 0, len = _mandatoryFiles.Length; i < len; i++)
            {
                if (!File.Exists(HarmonyXMLProjectPathClean + _mandatoryFiles[i]))
                {
                    EditorUtility.DisplayDialog("Error loading Harmony Project",
                        "Couldn't find project file \"" + _mandatoryFiles[i] + "\" at path \"" + HarmonyXMLProjectPath + "\", aborting.",
                        "OK");
                    return false;
                }
            }

            return true;
        }

        private void SaveProjectAsset(HarmonyProject project)
        {
            if (!Directory.Exists(ProjectFolder))
                Directory.CreateDirectory(ProjectFolder);

            AssetDatabase.CreateAsset(project, OutputPath);
        }

        private void ImportSprites(HarmonyProject project)
        {
            EditorUtility.DisplayProgressBar("Project Import Progress", "Importing textures...", 2.0f / TOTAL_IMPORT_STEPS);

            bool useSprites = Directory.Exists(HarmonySpritesPath);
            if (useSprites)
            {
                FileUtil.ReplaceDirectory(HarmonySpritesPath, ProjectFolder + "/sprites/");
            }
            else
            {
                FileUtil.ReplaceDirectory(HarmonySpritesheetPath, ProjectFolder + "/spriteSheets/");
            }

            //Pair atlases to resolutions
            for (int i = 0, len = project.SheetResolutions.Count; i < len; i++)
            {
                Spritesheet resolution = project.SheetResolutions[i];
                resolution.Sprites.Clear();

                if (useSprites)
                {
                    string resolutionFolder = ProjectFolder + "/sprites/" + resolution.SheetName + "/" + resolution.ResolutionName;
                    foreach (string file in Directory.GetFiles(resolutionFolder, "*.png"))
                    {
                        Sprite importSprite = ImportSprite(file);
                        if (importSprite != null)
                        {
                            resolution.Sprites.Add(importSprite);
                        }
                    }

                    AssetDatabase.ImportAsset(resolutionFolder);
                    DefaultAsset folderObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(resolutionFolder);
                    CreateSpriteAtlas(resolutionFolder, folderObject);
                }
                else
                {
                    string resolutionFile = ProjectFolder + "/spriteSheets/" + resolution.SheetName + "-" + resolution.ResolutionName + ".png";
                    Sprite importSprite = ImportSprite(resolutionFile);
                    if (importSprite != null)
                    {
                        resolution.Sprites.Add(importSprite);
                    }
                }
            }

            SpriteAtlasUtility.PackAllAtlases(BuildTarget.StandaloneWindows64); // Pack specific atlases crashes in 2018 LTS
        }

        private Sprite ImportSprite(string path)
        {
            AssetDatabase.ImportAsset(path);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter == null)
                return null;

            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.textureShape = TextureImporterShape.Texture2D;
            textureImporter.alphaIsTransparency = true;
            TextureImporterSettings textureSettings = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(textureSettings);
            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
            textureSettings.spriteMode = (int)SpriteImportMode.Single;
            textureSettings.spriteExtrude = 0;
            textureImporter.SetTextureSettings(textureSettings);
            textureImporter.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private SpriteAtlas CreateSpriteAtlas(string atlasPath, UnityEngine.Object obj)
        {
            SpriteAtlas atlas = new SpriteAtlas();
            atlas.Add(new UnityEngine.Object[] { obj });
            SpriteAtlasPackingSettings saps = atlas.GetPackingSettings();
            saps.enableRotation = false;
            saps.enableTightPacking = false;
            saps.padding = 0;
            atlas.SetPackingSettings(saps);
            AssetDatabase.CreateAsset(atlas, atlasPath + ".spriteatlas");

            return atlas;
        }

        private byte[] RunXML2Bin(string projectPath)
        {
            EditorUtility.DisplayProgressBar("Project Import Progress", "Importing xml files...", 3.0f / TOTAL_IMPORT_STEPS);
            return Xml2Bin.ConvertToMemory(projectPath);
        }

        private HarmonyProjectBinary CreateProject(byte[] projectBytes)
        {
            EditorUtility.DisplayProgressBar("Project Import Progress", "Making Unity asset for Harmony project...", 4.0f / TOTAL_IMPORT_STEPS);

            HarmonyProjectBinary project = HarmonyProjectBinary.CreateFromBytes(projectBytes);
            project.name = ProjectName;

            return project;
        }

        private GameObject BuildOrUpdatePrefabAsset(HarmonyProjectBinary project)
        {
            EditorUtility.DisplayProgressBar("Project Import Progress", "Making prefab...", 5.0f / TOTAL_IMPORT_STEPS);

            GameObject prefab;
            if (ExistingPrefab == null)
            {
                prefab = new GameObject(ProjectName);
            }
            else
            {
                prefab = Instantiate(ExistingPrefab);
            }

            HarmonyRenderer harmonyRenderer = prefab.GetComponent<HarmonyRenderer>();
            harmonyRenderer = harmonyRenderer ? harmonyRenderer : prefab.AddComponent<HarmonyRenderer>();

            harmonyRenderer.Project = project;
            harmonyRenderer.AnimationSettings = AnimationSettings.Clone();
            harmonyRenderer.enabled = true;

            //Make animation controller
            EditorUtility.DisplayProgressBar("Project Import Progress", "Importing animations...", 6.0f / TOTAL_IMPORT_STEPS);
            AnimationSettings.UpdateAnimationAssets(harmonyRenderer, newAnimationControllerPath: ProjectFolder + "/" + AnimationFolderName + "/" + ProjectName + ".controller");

            //Create metadata
            EditorUtility.DisplayProgressBar("Project Import Progress", "Importing metadata...", 7.0f / TOTAL_IMPORT_STEPS);
            CreateOrUpdateAnchorsFromMetadata(prefab, harmonyRenderer);

            ExistingPrefab = PrefabUtility.SaveAsPrefabAsset(prefab, ProjectFolder + "/" + ProjectName + ".prefab");
            DestroyImmediate(prefab);

            // We changed our reference to ExistingPrefab, mark it dirty
            EditorUtility.SetDirty(this);

            return ExistingPrefab;
        }

        public static void CreateOrUpdateAnchorsFromMetadata(GameObject rootObject, HarmonyRenderer renderer)
        {
            if (renderer == null || renderer.Project == null)
                return;

            //  Load anchors metadata from XML
            if (renderer.Project.AnchorsMeta.Count > 0)
            {
                HarmonyAnchor[] anchorComponents = rootObject.GetComponentsInChildren<HarmonyAnchor>(includeInactive: true);

                foreach (AnchorMeta xmlAnchor in renderer.Project.AnchorsMeta)
                {
                    if (!Array.Exists(anchorComponents, anchor => (anchor.NodeName == xmlAnchor.NodeName)))
                    {
                        GameObject anchorObject = new GameObject(xmlAnchor.NodeName);
                        anchorObject.transform.parent = rootObject.transform;

                        HarmonyAnchor anchorComponent = anchorObject.AddComponent<HarmonyAnchor>();

                        anchorComponent.NodeName = xmlAnchor.NodeName;
                    }
                }
            }
        }

        [MenuItem("Assets/Harmony/Import Selected Harmony Projects")]
        public static void ImportSelectedProjects()
        {
            DoImport(Selection.objects.OfType<HarmonyProjectImport>());
        }

        [MenuItem("Assets/Harmony/Import All Harmony Projects")]
        public static void ImportAllProjects()
        {
            string[] projectImportGuids = AssetDatabase.FindAssets("t:HarmonyProjectImport");
            DoImport(projectImportGuids.Select(guid => AssetDatabase.LoadAssetAtPath<HarmonyProjectImport>(AssetDatabase.GUIDToAssetPath(guid))));
        }

        private static void DoImport(IEnumerable<HarmonyProjectImport> projectImportAssets)
        {
            foreach (HarmonyProjectImport projectImport in projectImportAssets)
            {
                projectImport.DoImport();
            }
        }
    }
}