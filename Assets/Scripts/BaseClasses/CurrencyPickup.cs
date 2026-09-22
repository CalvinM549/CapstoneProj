using DG.Tweening;
using System;
using UnityEngine;

public class CurrencyPickup : MonoBehaviour
{
    public int Value { get; private set; }

    [Header("Burst")]
    [SerializeField] private float burstForceMin = 2f;
    [SerializeField] private float burstForceMax = 4f;
    [SerializeField] private float burstDrag = 4f;

    [Header("Timing")]
    [SerializeField] private float anticipationDuration = 0.35f;
    [SerializeField] private float popScale = 1.25f;

    [Header("Homing")]
    [SerializeField] private float homingAccel = 30f;
    [SerializeField] private float homingMaxSpeed = 16f;
    [SerializeField] private float collectDistance = 0.3f;

    private enum State { Bursting, Anticipating, Homing, Collected }
    private State state;

    private Vector2 velocity;
    private float stateTimer;
    private Transform target;
    private Action returnToPool;

    public void Spawn(Vector2 position, int value, Action returnToPool)
    {
        transform.position = position;
        transform.localScale = Vector3.one;
        Value = value;
        this.returnToPool = returnToPool;

        float angle = UnityEngine.Random.value * Mathf.PI * 2f;
        float force = UnityEngine.Random.Range(burstForceMin, burstForceMax);
        velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * force;

        state = State.Bursting;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        switch (state)
        {
            case State.Bursting: TickBursting(); break;
            case State.Anticipating: TickAnticipating(); break;
            case State.Homing: TickHoming(); break;
        }
    }

    private void TickBursting()
    {
        velocity = Vector2.Lerp(velocity, Vector2.zero, burstDrag * Time.deltaTime);
        transform.position += (Vector3)(velocity * Time.deltaTime);

        if (velocity.sqrMagnitude < 0.05f)
        {
            state = State.Anticipating;
            stateTimer = 0f;
            transform.DOKill();
            transform.DOPunchScale(Vector3.one * (popScale - 1f), anticipationDuration, 1, 0.5f);
        }
    }

    private void TickAnticipating()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer < anticipationDuration) return;

        target = RunManager.Instance.activePlayer.transform;
        state = State.Homing;
        stateTimer = 0f;
    }

    private void TickHoming()
    {
        stateTimer += Time.deltaTime;
        if (target == null) { Collect(); return; }

        Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
        if (toTarget.magnitude <= collectDistance) { Collect(); return; }

        velocity += toTarget.normalized * homingAccel * (stateTimer) * Time.deltaTime;
        velocity = Vector2.ClampMagnitude(velocity, homingMaxSpeed);
        transform.position += (Vector3)(velocity * Time.deltaTime);
    }

    public void ForceCollect() => Collect(); // used for the room-transition sweep below

    private void Collect()
    {
        if (state == State.Collected) return;
        state = State.Collected;
        transform.DOKill();

        RunManager.Instance.currentRun.GrantCurrency(Value);
        returnToPool?.Invoke();
    }
}