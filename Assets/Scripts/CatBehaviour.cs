using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatBehaviour : MonoBehaviour
{
    bool mouseFound = false;
    bool mouseInAttackRange = false;
    bool mouseCaught = false;
    bool isPatrolling = false;
    bool isChasing = false;
    bool mouseInViewRange = false;

    int waypointIndex = 0;

    [Header("Cat Behaviour Modifiers")]
    [SerializeField] float timeToRotate = 2f;
    [SerializeField] float patrolSpeed = 5f;
    [SerializeField] float chaseSpeed = 8f;
    [SerializeField] float viewRange = 15f;
    [SerializeField] float viewAngle = 90f;
    [SerializeField] float attackRange = 1f;

    [Header("References")]
    [SerializeField] LayerMask mouseMask;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] NavMeshAgent navAgent;
    [SerializeField] Transform[] waypoints;
    [SerializeField] Transform mouse;

    Vector3 mouseLastKnownPos = Vector3.zero;
    private BehaviorTreeController behaviorTreeController;

    void Start()
    {
        // Ensure NavMeshAgent is attached
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("NavMeshAgent component is missing on the GameObject.");
            return;
        }

        // Initialize BehaviorTreeController using AddComponent instead of New (can't use the "= New BehaviorTreeController" syntax for monobehaviour classes.)
        behaviorTreeController = gameObject.AddComponent<BehaviorTreeController>();

        // Define behavior tree structure and add functions to define expected behaviour for each node.
        Node behaviorTreeRoot = new Fallback(new List<Node>
        {
            new Sequence(new List<Node>
            {
                new Condition(ConditionMouseFound),
                new Fallback(new List<Node>
                {
                    new Sequence(new List<Node>
                    {
                        new Condition(ConditionMouseInAttackRange),
                        new Action(CatchMouse)
                    }),
                    new Action(ChaseMouse)
                })
            }),
            new Action(Patrol)
        });


        // Start the behavior tree
        behaviorTreeController.StartBehaviorTree(behaviorTreeRoot);
    }

    void Update()
    {
        // Keep polling the behaviour tree
        behaviorTreeController.Tick();
    }

    bool ConditionMouseFound() // Essentially checks whether mouse is visible to the cat.
    {
        mouseFound = false;
        Collider[] isMouseVisible = Physics.OverlapSphere(transform.position, viewRange, mouseMask); // Checks for nearby mice by looking for colliders tagged "mouseMask" in view range.
        for (int i = 0; i < isMouseVisible.Length; i++) // If any mice are in range, check whether they are within the cat's field of view.
        {
            Transform detectedMouse = isMouseVisible[i].transform;
            Vector3 directionToMouse = (detectedMouse.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToMouse) < viewAngle / 2)
            {
                float distanceToMouse = Vector3.Distance(transform.position, detectedMouse.position);
                if (!Physics.Raycast(transform.position, directionToMouse, distanceToMouse, obstacleMask)) // Check whether any objects are obscuring the cat's sight of the mouse using a raycast.
                {
                    mouseFound = true;
                    mouseLastKnownPos = detectedMouse.position; // Update last known mouse position.
                }
            }
        }
        return mouseFound;
    }

    void Patrol() // Patrol between waypoints (if mouse isn't being chased).
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("No waypoints set for patrolling.");
            return;
        }

        isPatrolling = true;
        isChasing = false;
        Move(patrolSpeed);

        navAgent.SetDestination(waypoints[waypointIndex].position);
        if (navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
            navAgent.SetDestination(waypoints[waypointIndex].position);
        }
    }

    bool ConditionMouseInSightRange() // Simply checks whether mouse is close enough to be in view.
    {
        mouseInViewRange = Vector3.Distance(transform.position, mouse.position) <= viewRange;
        return mouseInViewRange;
    }

    bool ConditionMouseInAttackRange() // Checks wheteher mouse is close enough for cat to catch it.
    {
        float distanceToMouse = Vector3.Distance(transform.position, mouse.position);
        Debug.Log($"Mouse Distance: {distanceToMouse}, Attack Range: {attackRange}");
        mouseInAttackRange = distanceToMouse <= attackRange;
        return mouseInAttackRange;
    }

    void ChaseMouse() // Instructs the cat's nav mesh agent to go to the last known position of the mouse.
    {
        isChasing = true;
        isPatrolling = false;
        Move(chaseSpeed);
        navAgent.SetDestination(mouseLastKnownPos);
    }

    bool ConditionMouseCaught() // Check whether the mouse has already been caught.
    {
        if (!mouseCaught)
        {
            Time.timeScale = 1f;
        }
        return mouseCaught;
    }

    void CatchMouse() // Catch the mouse and do this.
    {
        Stop();
        mouseCaught = true;
        Debug.Log("Mouse Caught");
        Time.timeScale = 0.1f;
    }

    void Move(float speed) // Sets the movement speed of the nav mesh agent.
    {
        navAgent.isStopped = false;
        navAgent.speed = speed;
    }

    void Stop() // Instructs the nav mesh agent to stand still.
    {
        navAgent.isStopped = true;
        navAgent.speed = 0;
    }
}
