#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ToonBoom.Harmony
{

    /// <summary>
    /// Id name data
    /// </summary>
    [Serializable]
    public class HarmonyAnimationSettings
    {
        [Tooltip("The target frame rate of the animation clips.")]
        public float FrameRate = 30.0f;
        [Tooltip("Should the animations be generated with stepped frames. This prevents Unity from generating in-between frames.")]
        public bool Stepped = true;
        [Tooltip("A prefix to strip off each animation name.")]
        public string StripPrefix;

        public HarmonyAnimationSettings Clone()
        {
            return (HarmonyAnimationSettings)MemberwiseClone();
        }

#if UNITY_EDITOR
        public void UpdateAnimationAssets(HarmonyRenderer harmonyRenderer, bool updateOnlyCurrentClip = false, string newAnimationControllerPath = null)
        {
            Animator animator = harmonyRenderer.GetComponentsInParent<Animator>(true).FirstOrDefault();
            // Default to adding an animator straight on the renderer if we don't have one
            animator = animator ? animator : harmonyRenderer.gameObject.AddComponent<Animator>();

            AnimatorController controller = (AnimatorController)animator.runtimeAnimatorController;
            if (controller == null)
            {
                string outputFile = newAnimationControllerPath ?? EditorUtility.SaveFilePanelInProject("Save Animation Controller", animator.gameObject.name, "controller", "Save Animation Controller");
                if (!string.IsNullOrEmpty(outputFile))
                {
                    // Ensure directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(newAnimationControllerPath));
                    controller = AnimatorController.CreateAnimatorControllerAtPath(outputFile);
                }
                else
                {
                    return;
                }
            }

            string controllerPath = AssetDatabase.GetAssetPath(controller);
            string animationsFolder = Path.GetDirectoryName(controllerPath).Replace('\\', '/') + "/";

            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
            string transformPath = AnimationUtility.CalculateTransformPath(harmonyRenderer.transform, animator.transform);

            List<ClipData> clipList = harmonyRenderer.Project.Clips;
            for (int i = 0; i < clipList.Count; ++i)
            {
                if (updateOnlyCurrentClip)
                {
                    int normalizedCurrentClip = Mathf.Clamp(harmonyRenderer.CurrentClipIndex, 0, clipList.Count - 1);
                    if (normalizedCurrentClip != i)
                        continue;
                }

                ClipData clipData = clipList[i];
                string strippedName = !string.IsNullOrEmpty(StripPrefix) && clipData.FullName.StartsWith(StripPrefix) ? clipData.FullName.Substring(StripPrefix.Length) : clipData.FullName;

                AnimatorState animatorState = rootStateMachine.states.Select(c => c.state).FirstOrDefault(s => s.name == strippedName);
                if (animatorState == null)
                {
                    animatorState = rootStateMachine.AddState(strippedName);
                }

                animatorState.tag = i.ToString();

                AnimationClip clip = animatorState.motion as AnimationClip;
                if (clip == null)
                {
                    clip = new AnimationClip();
                    clip.name = strippedName;
                    string assetPath = animationsFolder + clip.name + ".anim";
                    AssetDatabase.CreateAsset(clip, assetPath);

                    // Reload asset to ensure changes to our clips still work.
                    clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);

                    animatorState.motion = clip;

                    // First init settings
                    AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                    settings.loopTime = true;
                    AnimationUtility.SetAnimationClipSettings(clip, settings);
                }
                // Settings to apply every time
                clip.frameRate = FrameRate;

                {
                    AnimationCurve clipIndexCurve = new AnimationCurve();
                    clipIndexCurve.AddKey(0, i);
                    AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(transformPath, harmonyRenderer.GetType(), nameof(harmonyRenderer.CurrentClipIndex)), clipIndexCurve);
                }

                {
                    float nFrames = clipData.FrameCount;
                    AnimationCurve frameCurve = new AnimationCurve();
                    int index = 0;
                    for (float frame = 0; frame < nFrames; ++frame)
                    {
                        Keyframe key = new Keyframe();
                        key.time = frame / clip.frameRate;
                        key.value = frame + 1;

                        frameCurve.AddKey(key);

                        if (Stepped)
                        {
                            AnimationUtility.SetKeyLeftTangentMode(frameCurve, index, AnimationUtility.TangentMode.Constant);
                            AnimationUtility.SetKeyRightTangentMode(frameCurve, index, AnimationUtility.TangentMode.Constant);
                        }
                        else
                        {
                            AnimationUtility.SetKeyLeftTangentMode(frameCurve, index, AnimationUtility.TangentMode.Linear);
                            AnimationUtility.SetKeyRightTangentMode(frameCurve, index, AnimationUtility.TangentMode.Linear);
                        }
                        index++;
                    }
                    AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(transformPath, harmonyRenderer.GetType(), nameof(harmonyRenderer.CurrentFrame)), frameCurve);
                }

                EditorUtility.SetDirty(clip);
            }
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            animator.runtimeAnimatorController = controller;
        }
#endif
    }
}