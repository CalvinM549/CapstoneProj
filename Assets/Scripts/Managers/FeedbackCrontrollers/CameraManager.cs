using DG.Tweening;
using System;
using System.Collections;
using System.Security.Authentication.ExtendedProtection;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [SerializeField] private CinemachineCamera defaultCamera;

    private CinemachineBrain brain;
    private Transform player;
    private CinemachineImpulseSource impulseSource;
    private CinemachineCamera currentCamera;

    [SerializeField] private BoxCollider2D roomBounds;
    [SerializeField] private int maxCameraShakes;


    private Tween zoomTween;

    public bool IsBlending => brain.IsBlending;


    [Space]
    [SerializeField] private float lightHitShakeMag = 0.05f;
    [SerializeField] private float heavyHitShakeMag = 0.2f;
    [SerializeField] private float dashHitShakeMag = 0.1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        brain = GetComponent<CinemachineBrain>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        currentCamera = defaultCamera;
    }

    private void Start()
    {
        //player = GameObject.FindWithTag("Player").transform;
    }

    public void SetPlayer(Transform player)
    {
        this.player = player;
        currentCamera.Follow = player;
    }

    private void OnEnable()
    {
        GameEvents.OnHitConfirmed += HandleHitShake;
        GameEvents.OnPlayerTookDamage += HandlePlayerHit;

        GameEvents.OnPlayerTransitionTeleport += HandlePlayerTransition;
    }

    private void OnDisable()
    {
        GameEvents.OnHitConfirmed -= HandleHitShake;
        GameEvents.OnPlayerTookDamage -= HandlePlayerHit;

        GameEvents.OnPlayerTransitionTeleport -= HandlePlayerTransition;
    }


    #region Event Handlers

    private void HandleHitShake(HitData hit)
    {
        switch (hit.attackType)
        {
            case AttackType.Light:
            case AttackType.Projectile:
                CameraShake(lightHitShakeMag);
                break;
            case AttackType.Heavy:
                CameraShake(heavyHitShakeMag);
                break;
        }
    }

    private void HandlePlayerHit(HitData hit)
    {
        CameraShake(hit.hitstunTime);
    }

    private void HandlePlayerTransition(Vector3 newPos)
    {
        PlayerWarp(newPos);
    }

    #endregion

    #region CameraShake

    public void CameraShake(float magnitude)
    {
        if (impulseSource == null) return;

        impulseSource.GenerateImpulse(magnitude);
    }

    #endregion

    #region Camera View Changing

    public void SetCamera(CinemachineCamera camera, Transform target = null, Action onBlendComplete = null)
    {
        if (currentCamera == null) return;

        currentCamera.Priority = 0;

        currentCamera = camera;
        currentCamera.Priority = 10;

        if (target != null)
            currentCamera.Follow = target;

        if (onBlendComplete != null)
            StartCoroutine(CameraMoveEnd(onBlendComplete));
    }

    private void PlayerWarp(Vector3 newPos)
    {
        Vector3 deltaPos = newPos - transform.position;

        currentCamera.OnTargetObjectWarped(currentCamera.Follow, deltaPos);
    }

    public void ReturnToDefault(Action onBlendComplete = null)
    {
        SetCamera(defaultCamera, player, onBlendComplete);
    }

    private IEnumerator CameraMoveEnd(Action onComplete)
    {
        yield return new WaitUntil(() => !brain.IsBlending);
        onComplete?.Invoke();
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
