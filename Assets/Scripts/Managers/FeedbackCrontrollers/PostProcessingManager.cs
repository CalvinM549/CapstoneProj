using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum PPVolume
{
    Global,
    Hit,
    Death
}

public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager Instance;

    [Serializable]
    public class VolumeEntry
    {
        public PPVolume type;
        public Volume volume;

        [Range(0f, 1f)]
        public float defaultWeight;

        [NonSerialized]
        public Tween activeTween;
    }

    [Header("Config")]
    public float entryDuration;
    public float releaseDuration;

    public Ease entryEase;
    public Ease releaseEase;

    [SerializeField] private VolumeEntry[] effectVolumes;

    private readonly Dictionary<PPVolume, VolumeEntry> volumes = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        SetupVolumes();
    }

    private void SetupVolumes()
    {
        volumes.Clear();

        foreach (var entry in effectVolumes)
        {
            if (entry.volume == null)
            {
                Debug.LogWarning($"[PostProcessingManager] No volume assigned to {entry.type}");
                continue;
            }

            if (volumes.ContainsKey(entry.type))
            {
                Debug.LogWarning($"[PostProcessingManager] Duplicate entry for {entry.type}");
                continue;
            }

            volumes[entry.type] = entry;
            entry.volume.weight = entry.defaultWeight;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerTookDamage += HandlePlayerHit;
        GameEvents.OnPlayerDeath += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerTookDamage -= HandlePlayerHit;

        foreach (var entry in effectVolumes)
            entry.activeTween?.Kill();
    }

    private void HandlePlayerHit(HitData hit)
    {
        PulseVolume(PPVolume.Hit, hit.hitstunTime * 3f, 1f);
    }

    private void HandlePlayerDeath()
    {
        SetVolume(PPVolume.Death, 1.0f, 5f);
    }

    private void PulseVolume(PPVolume type, float duration, float targetWeight)
    {
        var volumeRef = volumes[type];

        volumeRef.activeTween?.Kill();

        volumeRef.activeTween = DOTween.Sequence()
            .Append(TweenWeight(volumeRef, targetWeight, entryDuration, entryEase))
            .Append(TweenWeight(volumeRef, volumeRef.defaultWeight, duration, releaseEase));

    }

    private void SetVolume(PPVolume type, float targetWeight, float fadeDuration)
    {
        var volumeRef = volumes[type]
            ;
        volumeRef.activeTween?.Kill();

        volumeRef.activeTween = TweenWeight(volumeRef, targetWeight, fadeDuration, entryEase);
    }

    private void ResetVolume(PPVolume type)
    {
        var volumeRef = volumes[type];

        volumeRef.activeTween?.Kill();

        volumeRef.activeTween = TweenWeight(volumeRef, volumeRef.defaultWeight, releaseDuration, releaseEase);
    }

    private void ResetVolumeImmediate(PPVolume type)
    {
        var volumeRef = volumes[type];

        volumeRef.activeTween?.Kill();

        volumeRef.volume.weight = volumeRef.defaultWeight;
    }

    private static Tween TweenWeight(VolumeEntry volume, float target, float duration, Ease ease)
    {
        return DOTween.To(
            () => volume.volume.weight,
            x => volume.volume.weight = x,
            target, duration
            ).SetEase(ease);
    }



}
