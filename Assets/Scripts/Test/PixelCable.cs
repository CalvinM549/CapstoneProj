using System.Collections.Generic;
using UnityEngine;

public class PixelCable : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    [Header("Cable Settings")]
    public int segments = 15;
    public float cableLength = 5f;
    public float gravityMult = 1;

    public const float GRAVITY = -9.81f;

    public int constraintIterations = 5;

    [Header("Reaction Settings")]
    public Vector2 windForce;

    [Header("Initialisation")]
    [Tooltip("Relaxation steps run before the first frame so the cable starts at rest. Raise for very long or slack cables.")]
    public int prewarmSteps = 120;

    private List<RopeNode> nodes = new List<RopeNode>();
    private float segmentLength;
    private bool initialized;

    public struct RopeNode
    {
        public Vector2 currentPos;
        public Vector2 oldPos;
    }

    void Start()
    {
        Initialize();
    }

    void OnEnable()
    {
        // Pooled objects (e.g. rooms) may have moved while disabled
        if (initialized) SettleToRest();
    }

    public void Initialize()
    {
        if (startPoint == null || endPoint == null || segments < 2)
        {
            Debug.LogWarning($"[PixelCable] '{name}' needs both anchors and at least 2 segments");
            return;
        }

        segmentLength = cableLength / segments;

        nodes.Clear();
        for (int i = 0; i < segments; i++)
            nodes.Add(default);

        initialized = true;
        SettleToRest();
    }

    /// <summary> Places the cable in its hanging shape and relaxes it, leaving no motion. </summary>
    public void SettleToRest()
    {
        if (!initialized) return;

        Vector2 start = startPoint.position;
        Vector2 end = endPoint.position;

        Vector2 force = GetForce();
        Vector2 hangDir = force.sqrMagnitude > 0.0001f ? force.normalized : Vector2.zero;

        // Parabola approximation of the sag for this length: L ~ D + 8s^2 / 3D
        float span = Vector2.Distance(start, end);
        float restLength = segmentLength * (segments - 1);
        float sag;

        if (span < 0.0001f)
            sag = restLength * 0.5f;
        else if (restLength > span)
            sag = Mathf.Sqrt(3f * span * (restLength - span) / 8f);
        else
            sag = 0f; // taut

        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1);
            Vector2 pos = Vector2.Lerp(start, end, t) + hangDir * (4f * sag * t * (1f - t));
            nodes[i] = new RopeNode { currentPos = pos, oldPos = pos };
        }

        // Relax into the exact resting shape, cancelling velocity each step so it settles instead of swinging
        for (int step = 0; step < prewarmSteps; step++)
        {
            Simulate();
            ApplyConstraints();
            ClearVelocity();
        }
    }

    void FixedUpdate()
    {
        if (!initialized) return;

        Simulate();
        ApplyConstraints();
    }

    private void ClearVelocity()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            RopeNode node = nodes[i];
            node.oldPos = node.currentPos;
            nodes[i] = node;
        }
    }

    private Vector2 GetForce() => new Vector2(0, (gravityMult * GRAVITY)) + windForce;

    void Simulate()
    {
        Vector2 force = GetForce();
        for (int i = 0; i < segments; i++)
        {
            // Keep start and end fixed to anchors
            if (i == 0 || i == segments - 1) continue;

            RopeNode node = nodes[i];
            Vector2 velocity = node.currentPos - node.oldPos;
            node.oldPos = node.currentPos;
            node.currentPos += velocity + force * Time.fixedDeltaTime * Time.fixedDeltaTime;
            nodes[i] = node;
        }
    }

    void ApplyConstraints()
    {
        for (int iteration = 0; iteration < constraintIterations; iteration++)
        {
            // Lock ends to target points
            RopeNode first = nodes[0]; first.currentPos = startPoint.position; nodes[0] = first;
            RopeNode last = nodes[segments - 1]; last.currentPos = endPoint.position; nodes[segments - 1] = last;

            for (int i = 0; i < segments - 1; i++)
            {
                RopeNode nodeA = nodes[i];
                RopeNode nodeB = nodes[i + 1];

                float dist = Vector2.Distance(nodeA.currentPos, nodeB.currentPos);
                float error = segmentLength - dist;
                Vector2 changeDir = (nodeA.currentPos - nodeB.currentPos).normalized;
                Vector2 changeAmount = changeDir * error * 0.5f;

                if (i != 0) nodeA.currentPos += changeAmount;
                if (i + 1 != segments - 1) nodeB.currentPos -= changeAmount;

                nodes[i] = nodeA;
                nodes[i + 1] = nodeB;
            }
        }
    }

    public List<RopeNode> GetNodes() => nodes;
}
