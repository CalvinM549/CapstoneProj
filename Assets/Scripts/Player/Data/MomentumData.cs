using UnityEngine;

[CreateAssetMenu(fileName = "MomentumData", menuName = "Player/NewMomentumData", order = 3)]
public class MomentumData : ScriptableObject
{
    public float capacity;

    public float emptyThreshold;
    public float lowThreshold;
    public float highThreshold;

    [Header("Gain Values")]
    public float lightAttackGain;
    public float heavyAttackGain;
    public float dashAttackGain;
    public float dashCancelGain;
    public float killGain;
    // Any more

    [Header("Drain Values")]
    public float hitTakenDrain;
    public float heavyWhiffDrain;
}
