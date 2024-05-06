using System;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTreeController : MonoBehaviour
{
    private Node rootNode;

    private bool isRunning;

    [SerializeField] private float tickInterval = 0.5f; // Adjust this value as needed
    private float timer = 0f;

    private void Update()
    {
        Tick();
    }

    public void StartBehaviorTree(Node root)
    {
        rootNode = root;
        isRunning = true;
    }

    public void StopBehaviorTree()
    {
        isRunning = false;
    }

    public void Tick()
    {
        if (isRunning && rootNode != null)
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
    private Func<bool> condition;

    public Condition(Func<bool> condition)
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
    private System.Action actionDelegate;

    public Action(System.Action action)
    {
        this.actionDelegate = action;
    }

    public override NodeState Tick()
    {
        try
        {
            actionDelegate();
            return NodeState.Success;
        }
        catch (Exception e)
        {
            Debug.LogError("Error executing action: " + e.Message);
            return NodeState.Failure;
        }
    }
}

public enum NodeState
{
    Running,
    Success,
    Failure
}
