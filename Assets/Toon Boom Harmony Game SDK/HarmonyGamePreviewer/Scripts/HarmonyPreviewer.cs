
using UnityEngine;

namespace ToonBoom.Harmony.Previewer
{
    /// <summary>
    /// Scene settings for the game previewer
    /// </summary>
    public class HarmonyPreviewer : MonoBehaviour
    {
        public bool InBetweenFrames = false;
        public bool IsPlaying = true;
        public float FrameRate = 30.0f;
        public float CurrentFrame = 0.0f;
        public HarmonyRenderer Renderer;

        public GameObject ViewerGroup;
        public GameObject BrowserGroup;

        public void LoadProject(string projectFolder)
        {
            var project = HarmonyProjectBinary.CreateFromFile(projectFolder);
            project.LoadSprites(projectFolder);

            Destroy(Renderer.Project);
            Renderer.Project = project;

            ViewerGroup.SetActive(true);
            BrowserGroup.SetActive(false);

            // Update the renderer so we can get valid bounds
            Renderer.UpdateRenderer();
            //  Adjust renderer object size to fit in camera.
            Bounds box = Renderer.GetComponent<MeshFilter>().sharedMesh.bounds;

            float scaleFactor = 5.0f / Mathf.Max(box.size.x, box.size.y);
            Renderer.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1.0f);
        }

        public void UnloadProject()
        {
            ViewerGroup.SetActive(false);
            BrowserGroup.SetActive(true);

            if (Renderer.Project != null)
            {
                Destroy(Renderer.Project);
                Renderer.Project = null;
            }
        }

        protected void Update()
        {
            if (IsPlaying && Renderer.Project != null)
            {
                if (Renderer.CurrentClip.FrameCount == 0.0f)
                {
                    Renderer.CurrentFrame = 0.0f;
                }
                else
                {
                    CurrentFrame += FrameRate * Time.deltaTime;
                    CurrentFrame %= Renderer.CurrentClip.FrameCount;

                    Renderer.CurrentFrame = InBetweenFrames ? CurrentFrame : Mathf.Floor(CurrentFrame);
                }
            }
        }
    }
}