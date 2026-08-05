using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.ParticleSystem;

public class PooledSFX : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    private AudioMixerGroup mixer;


    private Action<PooledSFX> returnToPool;
    private Coroutine returnRoutine;

    private void Awake()
    {
        mixer = AudioManager.Instance.sfxMixer;
        source.outputAudioMixerGroup = mixer;
    }

    public void Play(AudioClip clip, Action<PooledSFX> returnAction, Vector3 position, float volume, float pitch)
    {
        returnToPool = returnAction;
        transform.position = position;

        if (source == null)
        {
            Debug.LogWarning("[PooledSFX] no source set");
            return;
        }

        source.clip = clip;
        source.volume = volume;
        source.Play();

        if (returnRoutine != null)
            StopCoroutine(returnRoutine);
        returnRoutine = StartCoroutine(ReturnAfterDelay());
    }

    private IEnumerator ReturnAfterDelay()
    {
        if (source.isPlaying)
        {
            yield return new WaitUntil(() => !source.isPlaying);
        }

        source.Stop();
        source.clip = null;

        returnToPool?.Invoke(this);
    }

    public void ForceReturn()
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        source.Stop();
        source.clip = null;

        returnToPool?.Invoke(this);
    }
}
