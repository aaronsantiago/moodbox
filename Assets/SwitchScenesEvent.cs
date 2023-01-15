using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class SwitchScenesEvent : SequenceItem
{
    public string NextScene = "";
    protected override void OnActivate()
    {
        SceneManager.LoadScene(NextScene);
    }
}
