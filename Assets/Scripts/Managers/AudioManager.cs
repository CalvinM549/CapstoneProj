using System;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundType
{
    SFX,
    UI,
    Music
}



public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Serializable]
    private class AudioEntry
    {
        public string soundName;
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume;
        public bool loop;
        public bool doPitchVariation;
        public float pitchVariationAmount;

        [HideInInspector] public AudioSource source;
    }

    //private AudioSource sourcePrefab;
    //private ObjectPool<AudioSource> sourcePool;

    [SerializeField] private AudioEntry[] sfxLibrary;
    [SerializeField] private AudioEntry[] musicLibrary;

    public AudioMixerGroup sfxMixer;
    public AudioMixerGroup musicMixer;

    [Range(0f, 1f)]
    public float sfxVolume;

    [Range(0f, 1f)]
    public float musicVolume;

    public AudioSource sfxSource;
    public AudioSource musicSourceA;
    public AudioSource musicSourceB;

    private AudioSource activeMusicSource;
    private AudioSource inactiveMusicSource;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        foreach (AudioEntry sound in sfxLibrary)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.loop = sound.loop;
            sound.source.volume = sound.volume * sfxVolume;

            sound.source.playOnAwake = false;
            sound.source.outputAudioMixerGroup = sfxMixer;
        }

        musicSourceA = gameObject.AddComponent<AudioSource>();
        musicSourceA.outputAudioMixerGroup = musicMixer;
        musicSourceB = gameObject.AddComponent<AudioSource>();
        musicSourceB.outputAudioMixerGroup = musicMixer;
    }

    public void PlayMusicTrack(string name, bool doFade)
    {
        // find music entry

        if (doFade)
        {
            // Do stuff
        }
    }

    public void PlaySFX(string name)
    {
        AudioEntry sound = Array.Find(sfxLibrary, sound => sound.soundName == name);
        if (sound == null)
            Debug.LogError("No sound with that name");

        if(sound.source.isPlaying)
            return; // already playing

        if (sound.doPitchVariation)
        {
            // setup pitch shift
        }

        sound.source.volume = sound.volume * sfxVolume;
        sound.source.Play();
    }



}
