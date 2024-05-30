using System;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTreeController : MonoBehaviour
{
    private Node rootNode;

    private bool isRunning;

    [SerializeField] private float tickInterval = 0.5f; // how often the behaviour should update.
    private float timer = 0f;

    private void Update()
    {
        Tick(); // Keep calling Tick() (which checks internally whether it should update behaviour based on tickInterval).
    }

    public void StartBehaviorTree(Node root) // Initializes the behaviour tree with a root node.
    {
        rootNode = root;
        isRunning = true;
    }

    public void StopBehaviorTree()
    {
        isRunning = false;
    }

    public void Tick() // If the behaviour tree is running and time >= tickInterval has passed, tick the root node to update behaviour.
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

public abstract class Node // Base class used to create node types.
{
    public abstract NodeState Tick(); // This is overridden in each node type to implement its state behaviour.
}

public class Fallback : Node // This class defines a node that checks each child sequentially unless one returns Success.
{
    private List<Node> children; // Stores the specific fallback node's children.

    public Fallback(List<Node> children) // This is the constructor used for initializing the fallback node with a list of child nodes as input
    {
        this.children = children;
    }

    public override NodeState Tick() // Here the default Tick() is overridden to implement the correct fallback logic.
    {
        Debug.Log("Fallback Tick called. Number of children: " + children.Count);
        foreach (Node child in children)
        {
            NodeState childState = child.Tick(); // Ticks each child node to check their state
            if (childState != NodeState.Failure) // If a child returns success or running, the fallback node stops and returns success or running.
            {
                return childState;
            }
        }
        return NodeState.Failure; // If all of the children returned a failure state, the fallback node returns failure, too.
    }
}

public class Sequence : Node // This class defines a node that checks all children sequentially unless one returns Failure.
{
    private List<Node> children;

    public Sequence(List<Node> children) // The contructor
    {
        this.children = children;
    }

    public override NodeState Tick()
    {
        Debug.Log("Sequence Tick called. Number of children: " + children.Count);
        foreach (Node child in children)
        {
            NodeState childState = child.Tick();
            if (childState != NodeState.Success) // If a child node returns running or failure, so will the Sequence node.
            {
                return childState;
            }
        }
        return NodeState.Success; // If all child nodes returned Success, so does the sequence node.
    }
}

public class Condition : Node // This class defines a leaf node that simply checks whether a condition is true or false.
{
    private Func<bool> condition; // This delegate represents the condition that the node needs to evaluate.
    // (Delegates are pretty much just types that represent a reference to a method with a specific parameter list and return type)

    public Condition(Func<bool> condition) // This constructor takes a condition to evaluate as input.
    {
        this.condition = condition;
    }

    public override NodeState Tick() // When the Condition node is ticked, it simply checks whether the given condition is true or falls and returns Success or Failure accordingly.
    {
        bool result = condition();
        return result ? NodeState.Success : NodeState.Failure;
    }
}

public class Action : Node // This class defines a leaf node that causes the agent to perform some action.
{
    private bool actionRunning;
    private System.Action actionDelegate; // This delegate represents the action that the agent needs to perform.

    public Action(System.Action action) // The constructor takes an action as input.
    {
        this.actionDelegate = action;
    }

    public override NodeState Tick() // When ticked, the action node attempts to complete the action. Returns success if it can, Failure if it can't.
    // If the action node is ticked again while it is attempting to perform an action, it returns "Running".
    {
        try
        {
            if (actionRunning)
            {
                Debug.Log("Action Already Running");
                return NodeState.Running;
            }

            Debug.Log("Action Tick called");
            actionRunning = true;
            actionDelegate();
            actionRunning = false;
            return NodeState.Success;
        }
        catch (Exception e)
        {
            Debug.LogError("Error executing action: " + e.Message + "\n" + e.StackTrace);
            actionRunning = false;
            return NodeState.Failure;
        }
    }
}

public enum NodeState // This enum represents the possible states of each node.
{
    Running,
    Success,
    Failure
}
