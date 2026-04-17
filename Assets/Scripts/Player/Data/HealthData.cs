using UnityEngine;


[CreateAssetMenu(fileName = "HealthData", menuName = "Player/NewHealthData")]
public class HealthData : ScriptableObject
{
    public int maxHealth;

    public float hitIFrameDuration;

    public int integrityBaseCount;
    public int integrityBaseHealth;

    public float integrityBreakIFrameDuration;
}
public class Structure
{
    public int index;
    public float maxHealth;
    public float currentHealth;

    public bool isDestroyed;

    public Structure(int index, float maxHealth)
    {
        this.index = index;
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;
        this.isDestroyed = isDestroyed = false;
    }
}