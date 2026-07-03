using System.Collections.Generic;
using UnityEngine;

public class PixelCable : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    [Header("Cable Settings")]
    public int segments = 15;
    public float cableLength = 5f;
    public float gravity = -9.81f;
    public int constraintIterations = 5;

    [Header("Reaction Settings")]
    public Vector2 windForce;

    private List<RopeNode> nodes = new List<RopeNode>();
    private float segmentLength;

    public struct RopeNode
    {
        public Vector2 currentPos;
        public Vector2 oldPos;
    }

    void Start()
    {
        segmentLength = cableLength / segments;
        Vector2 start = startPoint.position;

        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1);
            Vector2 pos = Vector2.Lerp(start, endPoint.position, t);
            nodes.Add(new RopeNode { currentPos = pos, oldPos = pos });
        }
    }

    void FixedUpdate()
    {
        Simulate();
        ApplyConstraints();
    }

    void Simulate()
    {
        Vector2 force = new Vector2(0, gravity) + windForce;
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
