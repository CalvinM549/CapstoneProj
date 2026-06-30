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
    [Serializable]
    public class EffectVolume
    {
        public string id;

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

    [SerializeField] private List<EffectVolume> effectVolumes = new();

    private readonly Dictionary<string, EffectVolume> volumeLookup = new();

    private void Awake()
    {
        foreach (var entry in effectVolumes)
        {
            if(entry.volume == null) continue;

            volumeLookup[entry.id] = entry;
            entry.volume.weight = entry.defaultWeight;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerHit += HandlePlayerHit;
        GameEvents.OnPlayerDeath += HandlePlayerDeath;

    }

    private void OnDisable()
    {
        GameEvents.OnPlayerHit -= HandlePlayerHit;

        foreach (var entry in effectVolumes)
            entry.activeTween?.Kill();
    }

    private void HandlePlayerHit(HitData hit)
    {
        DoPulse("hitVolume", hit.hitstunTime * 3f, 1f);
    }

    private void HandlePlayerDeath()
    {
        DoHold("deathVolume", 1.0f, 5f);
    }

    private void DoPulse(string volume, float duration, float maxWeight)
    {
        var volumeRef = volumeLookup[volume];
        volumeRef.activeTween?.Kill();

        volumeRef.activeTween = DOTween.Sequence()
            .Append(TweenWeight(volumeRef, maxWeight, entryDuration, entryEase))
            .Append(TweenWeight(volumeRef, volumeRef.defaultWeight, duration, releaseEase));

    }

    private void DoHold(string volume, float targetWeight, float fadeDuration)
    {
        var volumeRef = volumeLookup[volume];
        volumeRef.activeTween?.Kill();

        volumeRef.activeTween = TweenWeight(volumeRef, targetWeight, fadeDuration, entryEase);
    }

    private void DoRelease(string volume)
    {

        var volumeRef = volumeLookup[volume];
        volumeRef.activeTween?.Kill();

        volumeRef.activeTween = TweenWeight(volumeRef, volumeRef.defaultWeight, releaseDuration, releaseEase);
    }

    private void DoSnap(string volume)
    {
        var volumeRef = volumeLookup[volume];
        volumeRef.activeTween?.Kill();

        volumeRef.volume.weight = volumeRef.defaultWeight;
    }

    private static Tween TweenWeight(EffectVolume volume, float target, float duration, Ease ease)
    {
        return DOTween.To(
            () => volume.volume.weight,
            x => volume.volume.weight = x,
            target, duration
            ).SetEase(ease);
    }



}
