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
    public static AudioManager Instance;

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
    }

    //private AudioSource sourcePrefab;
    //private ObjectPool<AudioSource> sourcePool;

    [SerializeField] private AudioEntry[] sfxLibrary;
    [SerializeField] private AudioEntry[] musicLibrary;

    public AudioMixerGroup sfxMixer;
    public AudioMixerGroup musicMixer;

    private float globalVolume = 1;

    [Range(0f, 1f)]
    public float sfxVolume;

    [Range(0f, 1f)]
    public float musicVolume;

    [SerializeField] private PooledSFX sfxSourcePrefab;
    private ObjectPool<PooledSFX> sfxSourcePool;

    public AudioSource musicSourceA;
    public AudioSource musicSourceB;

    private AudioSource activeMusicSource;
    private AudioSource inactiveMusicSource;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        musicSourceA.outputAudioMixerGroup = musicMixer;
        musicSourceB.outputAudioMixerGroup = musicMixer;

        BuildPools();
    }

    private void BuildPools()
    {
        sfxSourcePool = new ObjectPool<PooledSFX>(
            sfxSourcePrefab, 
            10, 
            transform
            );
    }

    public void PlayMusicTrack(string name, bool doFade)
    {
        AudioEntry track = Array.Find(musicLibrary, sound => sound.soundName == name);
        if (track == null)
            Debug.LogError($"[AudioManager] No track with name: {name}");

        if (doFade)
        {
            // Do stuff
        }
    }

    public void PlaySFX(string name, Vector3 position)
    {
        AudioEntry sound = Array.Find(sfxLibrary, sound => sound.soundName == name);
        if (sound == null)
            Debug.LogError($"[AudioManager] No SFX with name: {name}");

        float adjustedVolume = globalVolume * sfxVolume * sound.volume;

        var avaliableSource = sfxSourcePool.Get();
        avaliableSource.Play(sound.clip, sfxSourcePool.ReturnToPool, position, adjustedVolume, 1f);
    }
}
