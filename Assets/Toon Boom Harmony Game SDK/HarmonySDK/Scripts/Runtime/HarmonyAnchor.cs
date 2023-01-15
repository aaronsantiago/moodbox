using UnityEngine;
using System.Runtime.InteropServices;

namespace ToonBoom.Harmony
{
    /*!
     *  @class HarmonyAnchor
     *  Extract position from animation node.
     *  To be used in conjunction with HarmonyRenderer.
     */
    [ExecuteInEditMode]
    [AddComponentMenu("Harmony/Core/HarmonyAnchor")]
    public class HarmonyAnchor : MonoBehaviour
    {
        public string NodeName;

        private HarmonyRenderer _harmonyRenderer = null;
        private float _lastFrame = -1.0f;
        private string _lastClipName = string.Empty;

        private class Internal
        {
#if (UNITY_IPHONE || UNITY_XBOX360) && !UNITY_EDITOR
            [DllImport ("__Internal")]
            public static extern bool CalculateLocatorTransform( int projectId, string clipName, float frame, string locatorName, [In, Out] float[] position, [In, Out] float[] rotation, [In, Out] float[] scale );
#else
            [DllImport("HarmonyRenderer")]
            public static extern bool CalculateLocatorTransform(int projectId, string clipName, float frame, string locatorName, [In, Out] float[] position, [In, Out] float[] rotation, [In, Out] float[] scale);
#endif
        }

        protected void OnEnable()
        {
            _harmonyRenderer = GetComponentInParent<HarmonyRenderer>();
        }

        public bool IsValid()
        {
            return _harmonyRenderer != null && _harmonyRenderer.Project != null && _harmonyRenderer.CurrentClip.Name != null;
        }

        private void LateUpdate()
        {
            if (!IsValid())
                return;

            var clipData = _harmonyRenderer.CurrentClip;

            if (_lastFrame != _harmonyRenderer.CurrentFrame
                || _lastClipName != clipData.FullName)
            {
                //  Update HarmonyLocator position during LateUpdate to make sure that parent HarmonyRenderer component
                //  has finished updating the rendering script.  The locator must be a child of a Game Object with a
                //  HarmonyRenderer component.  It will inherit the game object transform and append specified bone transform.
                float[] position = new float[3];
                float[] rotation = new float[3];
                float[] scale = new float[3];

                if (Internal.CalculateLocatorTransform(_harmonyRenderer.Project.GetNativeProjectId(), clipData.FullName, _harmonyRenderer.CurrentFrame, NodeName, position, rotation, scale))
                {
                    transform.localPosition = new Vector3(position[0], position[1], position[2]);
                    transform.localRotation = Quaternion.Euler(rotation[0], rotation[1], rotation[2]);
                    transform.localScale = new Vector3(scale[0], scale[1], scale[2]);
                }

                _lastFrame = _harmonyRenderer.CurrentFrame;
                _lastClipName = clipData.FullName;
            }
        }
    }
}