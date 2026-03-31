using UnityEngine;
using Unity.Cinemachine;


// MERGE WITH CAMERA MANAGER?
public class ShakeManager : MonoBehaviour
{
    public static ShakeManager Instance;

    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        
    }

    public void Shake(float amplitude, float duration)
    {
        if (impulseSource == null) return;

        impulseSource.ImpulseDefinition.ImpulseDuration = duration;
        Vector3 velocity = GetShakeVelocity(amplitude);
        impulseSource.GenerateImpulse(velocity);
    }

    private Vector3 GetShakeVelocity(float amplitude)
    {
        float angle = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Sin(angle), -Mathf.Cos(angle), 0f);
        return dir * amplitude;
    }
}
