using DG.Tweening;
using System;
using System.Collections;
using System.Security.Authentication.ExtendedProtection;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public enum CameraEffect
    {
        PositionalShake,
        ImpulseShake,
        DirectionalShake,
        ZoomPunch,
        VCamSwitch
    }

    [SerializeField] private CinemachineCamera defaultCamera;

    private CinemachineBrain brain;
    private Transform player;

    [SerializeField] private BoxCollider2D roomBounds;
    [SerializeField] private int maxCameraShakes;

    private CinemachineImpulseSource impulseSource;

    public bool IsBlending => brain.IsBlending;

    private CinemachineCamera currentCamera;
    private int currentShakeCount;


    [Space]
    [SerializeField] private float lightHitShakeMag = 0.05f;
    [SerializeField] private float heavyHitShakeMag = 0.2f;
    [SerializeField] private float dashHitShakeMag = 0.1f;

    private void Awake()
    {
        brain = GetComponent<CinemachineBrain>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        player = GameObject.FindWithTag("Player").transform;
    }

    private void OnDestroy()
    {
        
    }

    private void OnEnable()
    {
        GameEvents.OnHitConfirmed += HandleHitShake;
        GameEvents.OnPlayerHit += HandlePlayerHit;
    }

    private void OnDisable()
    {
        GameEvents.OnHitConfirmed -= HandleHitShake;
        GameEvents.OnPlayerHit -= HandlePlayerHit;
    }

    #region Event Handlers

    private void HandleHitShake(HitData hit)
    {
        switch (hit.attackType)
        {
            case AttackType.Light:
            case AttackType.Secondary:
                CameraShake(lightHitShakeMag);
                break;
            case AttackType.Heavy:
                CameraShake(heavyHitShakeMag);
                break;
            case AttackType.DashAttack:
                CameraShake(dashHitShakeMag);
                break;
        }
    }

    private void HandlePlayerHit(HitData hit)
    {
        CameraShake(hit.hitstunTime);
    }

    #endregion

    #region CameraShake

    private void CameraShake(float magnitude, float duration = 0.2f)
    {
        if (impulseSource == null) return;

        //impulseSource.ImpulseDefinition.ImpulseDuration = duration;

        impulseSource.GenerateImpulse(magnitude);
    }

    #endregion
    #region Camera View Changing



    private void SetCamera(CinemachineCamera camera)
    {
        if (currentCamera != null)
            currentCamera.Priority = 0;

        currentCamera = camera;
        currentCamera.Priority = 10;
    }

    private void ReturnToDefault()
    {
        SetCamera(defaultCamera);
        SetTarget(player);
    }

    private void SetTarget(Transform target, Action triggeredEvent = null)
    {
        currentCamera.Follow = target;

        if (triggeredEvent != null)
            CameraMoveEnd(triggeredEvent);
    }


    private IEnumerator CameraMoveEnd(Action triggeredEvent)
    {
        yield return new WaitUntil(() => !brain.IsBlending);
        triggeredEvent?.Invoke();
    }

    #endregion
    //private void DoPositionalShake()
    //{
    //    if (brain == null) return;
    //    if (currentShakeCount >= maxCameraShakes) return;

    //    float strength = b.baseStrength * p.intensity;
    //    float dur = p.duration > 0f ? p.duration : b.duration;

    //    currentShakeCount++;
    //    brain.transform
    //        .DOShakePosition(dur, strength, b.vibrato, b.randomness, snapping: false, fadeOut: true)
    //        .OnComplete(() => currentShakeCount--);
    //}

}
