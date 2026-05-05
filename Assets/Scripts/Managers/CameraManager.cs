using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Make Instance?

    [SerializeField] private CinemachineCamera defaultCamera;

    private CinemachineBrain brain;
    private Transform player;

    [SerializeField] private BoxCollider2D roomBounds;
    
    private CinemachineImpulseSource impulseSource;

    public bool IsBlending => brain.IsBlending;

    private CinemachineCamera currentCamera;

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

    private void OnEnable()
    {
        GameEvents.OnHitConfirmed += HandleHitShake;
    }

    private void OnDisable()
    {
        GameEvents.OnHitConfirmed -= HandleHitShake;
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

}
