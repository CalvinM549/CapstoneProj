using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera defaultCamera;

    private CinemachineBrain brain;

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

    private void CameraShake(float magnitude, float duration = 0.2f)
    {
        if (impulseSource == null) return;

        //impulseSource.ImpulseDefinition.ImpulseDuration = duration;

        impulseSource.GenerateImpulse(magnitude);
    }

    private void ReturnToDefault()
    {
        SetCamera(defaultCamera);
    }

    private void SetCamera(CinemachineCamera camera)
    {
        if (currentCamera != null)
            currentCamera.Priority = 0;

        currentCamera = camera;
        currentCamera.Priority = 10;
    }

    private IEnumerator CameraMoveEnd()
    {
        yield return new WaitUntil(() => !brain.IsBlending);
        // Send Event
    }


}
