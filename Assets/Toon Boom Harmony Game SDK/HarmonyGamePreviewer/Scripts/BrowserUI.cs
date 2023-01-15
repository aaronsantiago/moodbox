using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace ToonBoom.Harmony.Previewer
{
    /// <summary>
    /// File browser for the game previewer
    /// </summary>
    public class BrowserUI : MonoBehaviour
    {
        public HarmonyPreviewer Previewer;
        public string RootFolder;

        private const int TOP_SPACING = 40;
        private const int BOTTOM_SPACING = 10;
        private const int LEFT_SPACING = 10;
        private const int RIGHT_SPACING = 10;

        private const int BUTTON_SPACING = 10;

        private enum StateButton
        {
            Pressed,
            Released,
            None
        }

        private StateButton _buttonState;
        private Vector2 _beginTouch;
        private float _deltaTouch;
        private float _deltaOld;
        private bool _isFirstTouch;

        private List<string> _projectFolders = new List<string>();

        protected void OnEnable()
        {
            _isFirstTouch = true;
            _buttonState = StateButton.None;
            _deltaOld = TOP_SPACING;
            _deltaTouch = 0;

            _projectFolders.Clear();
            string testPath = Path.Combine(Directory.GetCurrentDirectory(), RootFolder);
            if (Directory.Exists(RootFolder))
            {
                Debug.Log(RootFolder);
                _projectFolders.AddRange(Directory.GetDirectories(RootFolder));
            }
            else
            {

                string altProjectFolder = Path.Combine(Application.persistentDataPath, RootFolder);
                if (Directory.Exists(altProjectFolder))
                {
                    _projectFolders.AddRange(Directory.GetDirectories(altProjectFolder));
                }

                altProjectFolder = Path.Combine(Application.streamingAssetsPath, RootFolder);
                if (Directory.Exists(altProjectFolder))
                {
                    _projectFolders.AddRange(Directory.GetDirectories(altProjectFolder));
                }
            }

            if(_projectFolders.Count == 0)
            {
                Debug.LogError("Cannot find folders at : " + Path.Combine(Directory.GetCurrentDirectory(), RootFolder));
                Debug.LogError("Cannot find folders at : " + Path.Combine(Application.persistentDataPath, RootFolder));
                Debug.LogError("Cannot find folders at : " + Path.Combine(Application.streamingAssetsPath, RootFolder));
            }
        }

        private void Update()
        {
            if (!Input.mousePresent)
            {
                Touch[] touch = Input.touches;

                if (touch.Length <= 0)
                {
                    if (!_isFirstTouch)
                    {
                        _deltaOld += _deltaTouch;
                        _deltaTouch = 0;
                    }
                    _isFirstTouch = true;
                    return;
                }

                _deltaTouch -= touch[0].deltaPosition.y;

                if (_isFirstTouch)
                {
                    _isFirstTouch = false;
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _buttonState = StateButton.Pressed;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    _buttonState = StateButton.Released;
                }
                switch (_buttonState)
                {
                    case StateButton.Pressed:
                        if (_isFirstTouch)
                        {
                            _beginTouch = Input.mousePosition;
                            _isFirstTouch = false;
                        }

                        Vector2 pos = Input.mousePosition;
                        _deltaTouch = -pos.y + _beginTouch.y;
                        break;
                    case StateButton.Released:
                        if (!_isFirstTouch)
                        {
                            _deltaOld += _deltaTouch;
                            _deltaTouch = 0;
                        }
                        _isFirstTouch = true;
                        break;
                    case StateButton.None:
                        break;
                }
            }
        }

        protected void OnGUI()
        {
            Camera mainCamera = Camera.main;

            int width = mainCamera.pixelWidth;
            int height = mainCamera.pixelHeight;

            int nProjects = _projectFolders.Count;

            float fontMult = 1.0f / 120.0f;
            int fontSize = 15;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.GetStyle("button"));
            buttonStyle.fontSize = (Screen.dpi != 0) ? (int)(Screen.dpi * fontMult * fontSize) : fontSize;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.GetStyle("box"));
            boxStyle.fontSize = (Screen.dpi != 0) ? (int)(Screen.dpi * fontMult * fontSize) : fontSize;

            int boxWidth = width / 3;

            int buttonWidth = boxWidth - LEFT_SPACING - RIGHT_SPACING;
            int buttonHeight = buttonWidth / 5;

            int boxHeight = TOP_SPACING + (buttonHeight * nProjects) + (BUTTON_SPACING * nProjects - 1) + BOTTOM_SPACING;

            // Top of the menu
            if ((_deltaTouch + _deltaOld) > TOP_SPACING)
            {
                _deltaOld = TOP_SPACING;
                _deltaTouch = 0;
            }

            //Bottom of the menu
            if (-(_deltaTouch + _deltaOld) > boxHeight + BOTTOM_SPACING - height)
            {
                _deltaOld = -(boxHeight + BOTTOM_SPACING - height);
                _deltaTouch = 0;
            }

            Rect boxRect = new Rect((width - boxWidth) / 2, _deltaTouch + _deltaOld, boxWidth, boxHeight);

            //  Make a background box.
            GUIContent boxText = new GUIContent("Loader Menu");
            GUI.Box(boxRect, boxText, boxStyle);

            for (int i = 0; i < nProjects; ++i)
            {
                Rect buttonRect = new Rect(boxRect.x + LEFT_SPACING,
                                            boxRect.y + TOP_SPACING + i * (buttonHeight + BUTTON_SPACING),
                                            buttonWidth,
                                            buttonHeight);


                GUIContent buttonText = new GUIContent(new DirectoryInfo(_projectFolders[i]).Name);

                if (GUI.Button(buttonRect, buttonText, buttonStyle))
                {
                    if (Directory.Exists(_projectFolders[i]))
                    {
                        Previewer.LoadProject(_projectFolders[i]);
                    }
                }
            }
        }
    }
}