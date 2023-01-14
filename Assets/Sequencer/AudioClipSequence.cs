using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioClipSequence : OrderedSequence
{

    public List<AudioClip> clips;
    public AudioSource audioSource;

    int currentClip = 0;

    protected override void OnActivate()
    {
        currentClip = 0;
        audioSource.clip = clips[currentClip];
        base.OnActivate();
    }

    protected override void Update()
    {
        base.Update();
        if (base.IsResponseSatisfied())
        {
            if (currentClip < clips.Count)
            {
                currentClip++;
                if (currentClip < clips.Count)
                {
                    audioSource.clip = clips[currentClip];
                    base.OnActivate();
                }
            }
        }
    }

    public override bool IsResponseSatisfied()
    {
        return base.IsResponseSatisfied() && currentClip == clips.Count;
    }
}
