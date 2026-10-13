using System.Collections;
using UnityEngine;

public class RandomFlicker : MonoBehaviour
{
    [SerializeField] private GameObject target;

    [SerializeField] private float minTime;
    [SerializeField] private float maxTime;

    public bool doFlicker;

    private void OnEnable()
    {
        if (doFlicker)
        {
            StartCoroutine(FlickerRoutine());
        }
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            target.SetActive(!target.activeInHierarchy);
            float waitTime = Random.Range(minTime, maxTime);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
