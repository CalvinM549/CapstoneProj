using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance;

    [SerializeField] private Light2D globalLight;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Set(Color color, float intensity)
    {
        globalLight.color = color;
        globalLight.intensity = intensity;
    }
}
