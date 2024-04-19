using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTreeController
{
    private Node rootNode;

    private bool isRunning;

    [SerializeField] private float tickInterval = 0.5f; // Adjust this value as needed
    private float timer = 0f;

    public BehaviorTreeController(Node rootNode)
    {
        this.rootNode = rootNode;
    }

    public void StartBehaviorTree()
    {
        isRunning = true;
    }

    public void StopBehaviorTree()
    {
        isRunning = false;
    }

    public void Tick()
    {
        if (isRunning)
        {
            timer += Time.deltaTime;
            if (timer >= tickInterval)
            {
                rootNode.Tick();
                timer = 0f;
            }
        }
    }
}

public abstract class Node
{
    public abstract NodeState Tick();
}

public class Fallback : Node
{
    private List<Node> children;

    public Fallback(List<Node> children)
    {
        this.children = children;
    }

    public override NodeState Tick()
    {
        foreach (Node child in children)
        {
            NodeState childState = child.Tick();
            if (childState != NodeState.Failure)
            {
                return childState;
            }
        }
        return NodeState.Failure;
    }
}

public class Sequence : Node
{
    private List<Node> children;

    public Sequence(List<Node> children)
    {
        this.children = children;
    }

    public override NodeState Tick()
    {
        foreach (Node child in children)
        {
            NodeState childState = child.Tick();
            if (childState != NodeState.Success)
            {
                return childState;
            }
        }
        return NodeState.Success;
    }
}

public class Condition : Node
{
    private System.Func<bool> condition;

    public Condition(System.Func<bool> condition)
    {
        this.condition = condition;
    }

    public override NodeState Tick()
    {
        bool result = condition();
        return result ? NodeState.Success : NodeState.Failure;
    }
}

public class Action : Node
{
    private System.Action action;

    public Action(System.Action action)
    {
        this.action = action;
    }

    public override NodeState Tick()
    {
        action();
        return NodeState.Success;
    }
}

public enum NodeState
{
    Running,
    Success,
    Failure
}
