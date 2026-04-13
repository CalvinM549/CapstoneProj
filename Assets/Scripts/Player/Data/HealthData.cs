using UnityEngine;


[CreateAssetMenu(fileName = "HealthData", menuName = "Player/NewHealthData")]
public class HealthData : ScriptableObject
{
    public int maxHealth;

    public float hitIFrameDuration;

    public int integrityBaseCount;
    public int integrityBaseHealth;
}
