using UnityEngine;

[CreateAssetMenu(fileName = "ParryTool", menuName = "Player/Tools/ParryTool")]
public class ParryTool : PlayerTool // Setup dash parry cancel
{
    public float parryAngle; // width of the parry cone in degrees
    public float parryDuration; // how long the parry is active


    private bool parryActive;
    private float parryTimer = 0f;
    private Vector2 parryDirection;

    public override void UseTool(Vector2 direction)
    {
        parryActive = true;
        parryDirection = direction;
        parryTimer = parryDuration;

        //fire player method
    }

    public override bool CanUse()
    {
        if(parryActive) return false;
        return true;
    }

    public override bool TryIntercept(HitData incoming)
    {
        if(!parryActive) return false;

        Vector2 toIncoming = incoming.sourcePos - (Vector2)p.transform.position;
        float angleToIncoming = Vector2.Angle(parryDirection, toIncoming);

        if (angleToIncoming > parryAngle / 2) return false;

        OnParrySuccess();
        return true;
    }

    public override void OnEquip(Player player)
    {
        base.OnEquip(player);
        parryActive = false;
        parryTimer = 0f;
    }

    public override void UpdateTool()
    {
        if (!parryActive) return;

        parryTimer -= Time.deltaTime;

        if (parryTimer <= 0f)
        {

            parryActive = false;
            GameEvents.PlayerParryEnd();
        }
    }


    private void OnParrySuccess()
    {
        parryActive = false;
        parryTimer = 0f;

        //Do Feedback stuff

        // fire player method for parry end

        p.Tools.ReduceCurrentCooldown(0.5f);
    }
}
