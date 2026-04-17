using UnityEngine;

[CreateAssetMenu(fileName = "ParryTool", menuName = "Player/Tools/NewParryTool")]
public class ParryTool : PlayerTool
{
    [SerializeField] private float parryAngle; // width of the parry cone in degrees
    [SerializeField] private float parryDuration; // how long the parry is active
    [SerializeField] private float cooldown; // cooldown duration after parry ends

    private bool parryActive;
    private Vector2 parryDirection;

    private float parryTimer = 0f;
    private float cooldownTimer = 0f;

    public override bool UseTool(Vector2 direction)
    {
        if (cooldownTimer > 0f || parryActive) return false; // can't parry if on cooldown or already active

        parryActive = true;
        parryDirection = direction.normalized;
        parryTimer = parryDuration;

        GameEvents.PlayerParryStart();
        return true;
    }

    public override bool TryIntercept(HitData incoming)
    {
        if (!parryActive) return false; // can't intercept if parry isn't active
        Vector2 toIncoming = incoming.sourcePos - (Vector2)playerTransform.position; 
        float angleToIncoming = Vector2.Angle(parryDirection, toIncoming);
        if (angleToIncoming <= parryAngle / 2f)
        {
            return true;
        }
        return false; // incoming attack is outside of parry cone
    }

    public override void OnEquip(Transform currentTransform)
    {
        playerTransform = currentTransform;
    }

    public override void OnUnequip()
    {
        
    }

    public override void UpdateTool()
    {
        if (parryActive)
        {
            parryTimer -= Time.deltaTime;
            if (parryTimer <= 0f)
            {
                GameEvents.PlayerParryEnd();
                parryActive = false; // end parry when duration expires
                cooldownTimer = cooldown; // start cooldown
            }
        }
        else if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime; // reduce cooldown timer
        }
    }
}
