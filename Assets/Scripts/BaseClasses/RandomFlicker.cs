using System.Collections;
using UnityEngine;

public class RandomFlicker : MonoBehaviour
{
    private SpriteRenderer sr;

    [SerializeField] private float minTime;
    [SerializeField] private float maxTime;

    public bool doFlicker;


    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();    
    }

    private void OnEnable()
    {
        if (doFlicker)
        {
            StartCoroutine(FlickerRoutine());
        }
    }

    private IEnumerator FlickerRoutine()
    {
        while (enabled)
        {
            sr.enabled = !sr.enabled;
            float waitTime = Random.Range(minTime, maxTime);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
