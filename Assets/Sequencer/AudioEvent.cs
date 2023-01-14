using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioEvent : SequenceItem
{
    public bool WaitForAudioCompletion = false;
    private bool audioFinished = false;
    protected override void OnActivate()
    {
        audioFinished = false;
        IEnumerator coroutine = PlayAudio();
        StartCoroutine(coroutine);
    }

    public override bool IsResponseSatisfied()
    {
        if (!WaitForAudioCompletion) return true;
        return audioFinished;
    }

    // every 2 seconds perform the print()
    private IEnumerator PlayAudio()
    {

        AudioSource audio = GetComponent<AudioSource>();
        audio.Play();
        yield return new WaitForSeconds(audio.clip.length);
        audioFinished = true;
    }
}
