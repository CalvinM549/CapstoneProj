using System.Collections;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class LaserTelegraph : MonoBehaviour
{
    private readonly RaycastHit2D[] laserHitBuffer = new RaycastHit2D[1];

    private EnemyAIController aiController;
    private Transform targetPos;
    private Vector2 adjustment;

    [SerializeField] private LineRenderer line;

    private Coroutine targetRoutine;

    private void Awake()
    {
        aiController = GetComponentInParent<EnemyAIController>();
        line.enabled = false;
        line.SetPosition(1, transform.position);
    }

    public void DoTarget(Transform target, float startWidth, float duration)
    {
        targetPos = target;

        adjustment = Random.insideUnitCircle * 0.5f;

        if (!aiController.isActiveAndEnabled) return;
        targetRoutine = aiController.StartCoroutine(TargetRoutine(startWidth, duration));
    }

    public void CancelTarget()
    {
        if(targetRoutine != null)
            aiController.StopCoroutine(targetRoutine);
        line.enabled = false;
    }

    private IEnumerator TargetRoutine(float startWidth, float duration)
    {
        line.enabled = true;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;
            float distanceOffset = Mathf.Lerp(startWidth, 0, t);

            Vector2 origin = transform.position;
            Vector2 dirToTarget = (((Vector2)targetPos.position + adjustment) - origin).normalized;
            Vector2 perpendicular = Vector2.Perpendicular(dirToTarget).normalized;

            Vector2 targetPoint1 = (Vector2)targetPos.position + adjustment + (perpendicular * distanceOffset);
            Vector2 targetPoint2 = (Vector2)targetPos.position + adjustment - (perpendicular * distanceOffset);

            Vector2 toTarget1 = targetPoint1 - origin;
            Vector2 toTarget2 = targetPoint2 - origin;

            float distance1 = toTarget1.magnitude;
            float distance2 = toTarget2.magnitude;

            Vector2 endPoint1 = targetPoint1;
            Vector2 endPoint2 = targetPoint2;

            if (distance1 > 0.0001f && distanceOffset > 0.0001f)
            {
                Vector2 direction1 = toTarget1 / distance1;
                Vector2 direction2 = toTarget2 / distance2;


                int hits1 = Physics2D.RaycastNonAlloc(origin, direction1, laserHitBuffer, distance1, aiController.profile.obstacleLayer);

                if (hits1 > 0)
                    endPoint1 = laserHitBuffer[0].point;

                int hits2 = Physics2D.RaycastNonAlloc(origin, direction2, laserHitBuffer, distance2, aiController.profile.obstacleLayer);

                if (hits2 > 0)
                    endPoint1 = laserHitBuffer[0].point;
            }

            line.SetPosition(0, endPoint1);
            line.SetPosition(1, origin);
            line.SetPosition(2, endPoint2);

            yield return null;
        }

        line.enabled = false;
    }
}
