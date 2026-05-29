using System;
using System.Collections.Generic;

public enum NodeStatus
{
    Success,
    Failure,
    Running
}

public abstract class BTNode
{
    public string Name;

    protected BTNode(string name = "")
    {
        Name = name;
    }

    public abstract NodeStatus Tick();
}

public class SelectorNode : BTNode
{
    private readonly List<BTNode> children = new List<BTNode>();

    public SelectorNode(string name, params BTNode[] nodes) : base(name)
    {
        children.AddRange(nodes);
    }

    public override NodeStatus Tick()
    {
        foreach (BTNode child in children)
        {
            NodeStatus status = child.Tick();

            if (status == NodeStatus.Success || status == NodeStatus.Running)
                return status;
        }

        return NodeStatus.Failure;
    }
}

public class SequenceNode : BTNode
{
    private readonly List<BTNode> children = new List<BTNode>();

    public SequenceNode(string name, params BTNode[] nodes) : base(name)
    {
        children.AddRange(nodes);
    }

    public override NodeStatus Tick()
    {
        foreach (BTNode child in children)
        {
            NodeStatus status = child.Tick();

            if (status == NodeStatus.Failure || status == NodeStatus.Running)
                return status;
        }

        return NodeStatus.Success;
    }
}

public class ConditionNode : BTNode
{
    private readonly Func<bool> condition;

    public ConditionNode(string name, Func<bool> condition) : base(name)
    {
        this.condition = condition;
    }

    public override NodeStatus Tick()
    {
        return condition.Invoke() ? NodeStatus.Success : NodeStatus.Failure;
    }
}

public class ActionNode : BTNode
{
    private readonly Func<NodeStatus> action;

    public ActionNode(string name, Func<NodeStatus> action) : base(name)
    {
        this.action = action;
    }

    public override NodeStatus Tick()
    {
        return action.Invoke();
    }
}