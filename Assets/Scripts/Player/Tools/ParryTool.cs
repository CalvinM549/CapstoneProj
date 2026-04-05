using UnityEngine;

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
        return true;
    }

    public override bool TryIntercept(HitData incoming)
    {
        if (!parryActive) return false; // can't intercept if parry isn't active
        Vector2 toIncoming = incoming.sourcePos - (Vector2)PlayerCore.Instance.transform.position;
        float angleToIncoming = Vector2.Angle(parryDirection, toIncoming);
        if (angleToIncoming <= parryAngle / 2f)
        {
            // Successful parry
            parryActive = false; // end parry immediately after a successful block
            cooldownTimer = cooldown; // start cooldown
            return true;
        }
        return false; // incoming attack is outside of parry cone
    }

    public override void OnEquip()
    {
        // Optional: Add any initialization logic when the tool is equipped
    }

    public override void OnUnequip()
    {
        // Optional: Add any cleanup logic when the tool is unequipped
    }

    public override void UpdateTool()
    {
        if (parryActive)
        {
            parryTimer -= Time.deltaTime;
            if (parryTimer <= 0f)
            {
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
