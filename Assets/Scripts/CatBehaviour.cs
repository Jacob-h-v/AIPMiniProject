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

        // Initialize BehaviorTreeController using AddComponent
        behaviorTreeController = gameObject.AddComponent<BehaviorTreeController>();

        // Define behavior tree structure
        Node behaviorTreeRoot = new Fallback(new List<Node>
        {
            new Sequence(new List<Node>
            {
                new Fallback(new List<Node>
                {
                    new Condition(ConditionMouseFound),
                    new Action(Patrol)
                }),
                new Fallback(new List<Node>
                {
                    new Condition(ConditionMouseInAttackRange),
                    new Action(ChaseMouse)
                }),
                new Fallback(new List<Node>
                {
                    new Condition(ConditionMouseCaught),
                    new Action(CatchMouse)
                })
            })
        });

        // Start the behavior tree
        behaviorTreeController.StartBehaviorTree(behaviorTreeRoot);
    }

    void Update()
    {
        // Keep polling the behaviour tree
        behaviorTreeController.Tick();
    }

    bool ConditionMouseFound()
    {
        mouseFound = false;
        Collider[] isMouseVisible = Physics.OverlapSphere(transform.position, viewRange, mouseMask);
        for (int i = 0; i < isMouseVisible.Length; i++)
        {
            Transform detectedMouse = isMouseVisible[i].transform;
            Vector3 directionToMouse = (detectedMouse.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToMouse) < viewAngle / 2)
            {
                float distanceToMouse = Vector3.Distance(transform.position, detectedMouse.position);
                if (!Physics.Raycast(transform.position, directionToMouse, distanceToMouse, obstacleMask))
                {
                    mouseFound = true;
                    mouseLastKnownPos = detectedMouse.position; // Update last known position
                }
            }
        }
        return mouseFound;
    }

    void Patrol()
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

    bool ConditionMouseInSightRange()
    {
        mouseInViewRange = Vector3.Distance(transform.position, mouse.position) <= viewRange;
        return mouseInViewRange;
    }

    bool ConditionMouseInAttackRange()
    {
        float distanceToMouse = Vector3.Distance(transform.position, mouse.position);
        Debug.Log($"Mouse Distance: {distanceToMouse}, Attack Range: {attackRange}");
        mouseInAttackRange = distanceToMouse <= attackRange;
        return mouseInAttackRange;
    }

    void ChaseMouse()
    {
        isChasing = true;
        isPatrolling = false;
        Move(chaseSpeed);
        navAgent.SetDestination(mouseLastKnownPos);
    }

    bool ConditionMouseCaught()
    {
        return mouseCaught;
    }

    void CatchMouse()
    {
        Stop();
        mouseCaught = true;
        Debug.Log("Mouse Caught");
    }

    void Move(float speed)
    {
        navAgent.isStopped = false;
        navAgent.speed = speed;
    }

    void Stop()
    {
        navAgent.isStopped = true;
        navAgent.speed = 0;
    }
}
