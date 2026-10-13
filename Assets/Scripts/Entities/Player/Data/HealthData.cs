using UnityEngine;


[CreateAssetMenu(fileName = "HealthData", menuName = "Player/NewHealthData")]
public class HealthData : ScriptableObject
{
    public float hitIFrameDuration;

    public float segmentBaseCount;
    public float segmentMaxHealth;
}