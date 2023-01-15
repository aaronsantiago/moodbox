using UnityEngine;

namespace ToonBoom.Harmony.Previewer
{
    /// <summary>
    /// UI for the game previewer
    /// </summary>
    [RequireComponent(typeof(HarmonyPreviewer))]
    public class ViewerUI : MonoBehaviour
    {
        private const float FONT_MULT = 1.0f / 120.0f;
        private const int FONT_SIZE = 30;

        private GUIStyle _buttonStyle;
        private GUIStyle _labelStyle;

        private bool _initialized = false;

        public HarmonyPreviewer Previewer;
        public PanCamera PanCam;

        void OnGUI()
        {
            //  Update font size according to dpi.
            if (!_initialized)
            {
                int realFontSize = (Screen.dpi != 0) ? (int)(Screen.dpi * FONT_MULT * FONT_SIZE) : FONT_SIZE;

                //  Setup styles.
                _labelStyle = new GUIStyle(GUI.skin.GetStyle("label"));
                _labelStyle.fontSize = realFontSize;
                _labelStyle.wordWrap = false;

                _buttonStyle = new GUIStyle(GUI.skin.GetStyle("button"));
                _buttonStyle.fontSize = realFontSize;
                _buttonStyle.stretchWidth = false;

                _initialized = true;
            }

            int width = Screen.width;
            int height = Screen.height;

            //  Add button to go back to main browser scene.
            GUIContent buttonText = new GUIContent("Go Back");
            Rect buttonRect = GUILayoutUtility.GetRect(buttonText, _buttonStyle);

            int buttonWidth = (int)(buttonRect.width + 1);
            int buttonHeight = (int)(buttonRect.height + 1);

            //  Lower right corner
            buttonRect = new Rect(width - buttonWidth - 5,
                                   height - buttonHeight - 5,
                                   buttonWidth,
                                   buttonHeight);

            if (GUI.Button(buttonRect, buttonText, _buttonStyle))
            {
                //  Exit scene.  Delete controller object and enable
                //  browser group hierarchy.
                Previewer.UnloadProject();
                return;
            }

            //  If more than 1 clip, add browser for previous/next clip.
            if (Previewer.Renderer.Project != null)
            {
                DoControls();

                int nClips = Previewer.Renderer.Project.Clips.Count;

                //  Add button for previous clip.
                GUI.enabled = Previewer.Renderer.CurrentClipIndex >= 1;
                {
                    buttonText = new GUIContent("Prev");
                    buttonRect = GUILayoutUtility.GetRect(buttonText, _buttonStyle);

                    buttonWidth = (int)(buttonRect.width + 1);
                    buttonHeight = (int)(buttonRect.height + 1);

                    //  Lower middle side
                    buttonRect = new Rect((width / 2) - buttonWidth - 5,
                                           height - ((buttonHeight + 5) * 2),
                                           buttonWidth,
                                           buttonHeight);

                    if (GUI.Button(buttonRect, buttonText, _buttonStyle))
                    {
                        --Previewer.Renderer.CurrentClipIndex;
                    }
                }

                //  Add button for next clip.
                GUI.enabled = Previewer.Renderer.CurrentClipIndex < nClips - 1;
                {
                    buttonText = new GUIContent("Next");
                    buttonRect = GUILayoutUtility.GetRect(buttonText, _buttonStyle);

                    buttonWidth = (int)(buttonRect.width + 1);
                    buttonHeight = (int)(buttonRect.height + 1);

                    //  Lower middle side
                    buttonRect = new Rect((width / 2) + 5,
                                           height - ((buttonHeight + 5) * 2),
                                           buttonWidth,
                                           buttonHeight);

                    if (GUI.Button(buttonRect, buttonText, _buttonStyle))
                    {
                        ++Previewer.Renderer.CurrentClipIndex;
                    }
                }

                GUI.enabled = true;
            }

            //  Add button for play/pause animation.
            GUI.enabled = true;

            buttonText = new GUIContent(Previewer.IsPlaying ? "Pause" : "Play");
            buttonRect = GUILayoutUtility.GetRect(buttonText, _buttonStyle);

            buttonWidth = (int)(buttonRect.width + 1);
            buttonHeight = (int)(buttonRect.height + 1);

            //  Lower middle side
            buttonRect = new Rect((width - buttonWidth) / 2,
                                   height - buttonHeight - 5,
                                   buttonWidth,
                                   buttonHeight);

            if (GUI.Button(buttonRect, buttonText, _buttonStyle))
            {
                Previewer.IsPlaying = !Previewer.IsPlaying;
            }

            GUI.enabled = true;

            //  Add label for current clip.
            GUIContent labelText = new GUIContent(Previewer.Renderer.CurrentClip.DisplayName);
            Rect labelRect = GUILayoutUtility.GetRect(labelText, _labelStyle);

            int labelWidth = (int)labelRect.width;
            int labelHeight = (int)labelRect.height;

            //  Upper middle side.
            labelRect = new Rect((width - labelWidth) / 2,
                                    5,
                                    labelWidth,
                                    labelHeight);

            GUI.Label(labelRect, labelText, _labelStyle);
        }

        protected void DoControls()
        {
            using (new GUILayout.VerticalScope(GUILayout.Width(200)))
            {
                DoIntField(ref Previewer.FrameRate, "Frame Rate", 0, 120);
                Previewer.InBetweenFrames = GUILayout.Toggle(Previewer.InBetweenFrames, "Show In Between Frames");

                DoFloatField(ref Previewer.Renderer.Color.r, "Red", 0.0f, 1.0f);
                DoFloatField(ref Previewer.Renderer.Color.g, "Green", 0.0f, 1.0f);
                DoFloatField(ref Previewer.Renderer.Color.b, "Blue", 0.0f, 1.0f);
                DoFloatField(ref Previewer.Renderer.Color.a, "Alpha", 0.0f, 1.0f);

                float discretizationStep = Previewer.Renderer.DiscretizationStep;
                DoFloatField(ref discretizationStep, "Discretization Step", 1.0f, 50.0f);
                Previewer.Renderer.DiscretizationStep = (int)discretizationStep;

                if (Previewer.Renderer.Project.Skins.Count > 1)
                {
                    if (Previewer.Renderer.GroupSkins.Count == 0)
                    {
                        Previewer.Renderer.GroupSkins.Add(new GroupSkin(0, 0));
                    }

                    var groupSkin = Previewer.Renderer.GroupSkins[0];
                    float skinIndex = groupSkin.SkinId;
                    DoFloatField(ref skinIndex, "Skin", 0.0f, Previewer.Renderer.Project.Skins.Count - 1);
                    groupSkin.SkinId = (int)skinIndex;
                    Previewer.Renderer.GroupSkins[0] = groupSkin;
                    GUILayout.Label(Previewer.Renderer.Project.Skins[groupSkin.SkinId]);
                }
            }
        }

        protected void DoFloatField(ref float field, string label, float min, float max)
        {
            var oldfield = field;
            GUILayout.Label(label);
            using (new GUILayout.HorizontalScope())
            {
                string textField = GUILayout.TextField(field.ToString(), GUILayout.Width(45));
                if (float.TryParse(textField, out float newField))
                {

                    field = Mathf.Clamp(newField, min, max);
                }
                field = GUILayout.HorizontalSlider(field, min, max);
            }

            field = Mathf.Clamp(field, min, max);
            if (field != oldfield)
            {
              PanCam.isGUIUsed = true;
            }

        }

        protected void DoIntField(ref float field, string label, int min, int max)
        {
            var oldfield = field;
            GUILayout.Label(label);
            using (new GUILayout.HorizontalScope())
            {
                string textField = GUILayout.TextField(field.ToString(), GUILayout.Width(45));
                if (float.TryParse(textField, out float newField))
                {
                    if (field != newField)
                    {
                      PanCam.isGUIUsed = true;
                    }
                    field = Mathf.Clamp(newField, min, max);
                }
                field = GUILayout.HorizontalSlider(field, min, max);
            }

            field = Mathf.Clamp(field, min, max);
            field = Mathf.RoundToInt(field);
            if (field != oldfield)
            {
              PanCam.isGUIUsed = true;
            }
        }
    }
}
