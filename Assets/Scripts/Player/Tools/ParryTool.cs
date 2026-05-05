using UnityEngine;

[CreateAssetMenu(fileName = "ParryTool", menuName = "Player/Tools/ParryTool")]
public class ParryTool : PlayerTool // Setup dash parry cancel
{
    public float parryAngle; // width of the parry cone in degrees
    public float parryDuration; // how long the parry is active


    private bool parryActive;
    private float parryTimer = 0f;
    private Vector2 parryDirection;

    protected override bool OnUse(Vector2 direction)
    {
        if (parryActive) return false;

        parryActive = true;
        parryDirection = direction;
        parryTimer = parryDuration;

        GameEvents.PlayerParryStart();
        return true;
    }

    public override bool TryIntercept(HitData incoming)
    {
        if(!parryActive) return false;

        Vector2 toIncoming = incoming.sourcePos - (Vector2)player.transform.position;
        float angleToIncoming = Vector2.Angle(parryDirection, toIncoming);

        if (angleToIncoming > parryAngle / 2) return false;

        OnParrySuccess();
        return true;
    }

    private void OnParrySuccess()
    {
        parryActive = false;
        parryTimer = 0f;

        //Do Feedback stuff

        // Ping Event
        GameEvents.PlayerParryEnd();
        ReduceCooldown(Cooldown * 0.5f);
    }

    protected override void OnUpdate()
    {
        if (!parryActive) return;

        parryTimer -= Time.deltaTime;

        if (parryTimer <= 0f)
        {

            parryActive = false;
            GameEvents.PlayerParryEnd();
        }
    }

    protected override void OnReset()
    {
        parryActive = false;
        parryTimer = 0f;
    }
}
