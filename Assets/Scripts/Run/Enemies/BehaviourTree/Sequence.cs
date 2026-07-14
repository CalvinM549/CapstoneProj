using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

// Behaves like an AND gate, completing all subnodes in order
public class Sequence : Node
{
    public Sequence() : base() { }
    public Sequence(List<Node> children) : base(children) { }

    public override NodeState Evaluate()
    {
        bool anyChildIsRunning = false;

        foreach (Node node in children)
        {
            switch (node.Evaluate())
            {
                case NodeState.Failure:
                    state = NodeState.Failure;
                    return state;
                case NodeState.Success:
                    continue;
                default:
                    state = NodeState.Success;
                    return state;

            }
        }

        state = anyChildIsRunning ? NodeState.Running : NodeState.Success;
        return state;
    }
}
