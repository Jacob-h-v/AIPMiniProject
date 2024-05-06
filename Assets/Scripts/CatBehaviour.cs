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
    [SerializeField] float  viewRange = 15f;
    [SerializeField] float  viewAngle = 90f;
    [SerializeField] float attackRange = 1f;

    [Header("References")]
    [SerializeField] LayerMask mouseMask;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] UnityEngine.AI.NavMeshAgent navAgent;
    [SerializeField] Transform[] waypoints;
    [SerializeField] Transform mouse;

    Vector3 mouseLastKnownPos = Vector3.zero;
    [SerializeField] BehaviorTreeController behaviorTreeController;

    // Start is called before the first frame update
    void Start()
    {
        // Load and start navAgent.
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        // Create the AI's behavior tree controller
    behaviorTreeController = new BehaviorTreeController();

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
                new Condition(ConditionMouseInAttackRange),
                new Action(CatchMouse)
            })
        })
    });
    /*Node behaviorTreeRoot = new Fallback(new List<Node>
    {
        new Sequence(new List<Node>
        {
            new Fallback(new List<Node>
            {
                new Condition(ConditionMouseFound),
                new Action(ChaseMouse)
            }),
            new Fallback(new List<Node>
            {
                new Condition(ConditionMouseInAttackRange),
                new Action(CatchMouse)
            }),
            new Action(Patrol)
        })
    });*/

    // Start the behavior tree
    behaviorTreeController.StartBehaviorTree(behaviorTreeRoot);

       // Patrol();

    }

    void Update()
    {
        // Keep polling the behaviour tree
        //(it has an internal variable to prevent it from running every node every frame)
        behaviorTreeController.Tick();
    }

    bool ConditionMouseFound()
    {
        mouseFound = false;
        // Create an overlap sphere to detect when mouse is close.
        Collider[] isMouseVisible = Physics.OverlapSphere(transform.position, viewRange, mouseMask);
        for (int i = 0; i < isMouseVisible.Length; i++)
        {
            Transform mouse = isMouseVisible[i].transform;
            Vector3 directionToMouse = (mouse.position - transform.position).normalized;
            // If mouse is in the overlap sphere and within the cat's field of view (viewAngle)
            // and also not behind cover, set mouseFound to true.
            if (Vector3.Angle(transform.forward, directionToMouse) < viewAngle / 2)
            {
                float distanceToMouse = Vector3.Distance(transform.position, mouse.position);
                if (!Physics.Raycast(transform.position, directionToMouse, distanceToMouse, obstacleMask))
                {
                    mouseFound = true;
                }
            }
        }
        return mouseFound;
    }

    void Patrol()
    {
        isPatrolling = true;
        isChasing = false;
        Move(patrolSpeed);
        // Patrol to next waypoint
        navAgent.SetDestination(waypoints[waypointIndex].position);

        // When agent approaches current waypoint, select and move to next waypoint.
        if (navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
            navAgent.SetDestination(waypoints[waypointIndex].position);
        }
    }

bool ConditionMouseInSightRange()
{
    if (Vector3.Distance(transform.position, mouse.position) <= viewRange)
        {
            mouseInViewRange = true;
        }
        else
        {
            mouseInViewRange = false;
        }
        return mouseInViewRange;
}
    bool ConditionMouseInAttackRange()
    {
        // Check whether mouse in close enough to attack it.
        if (Vector3.Distance(transform.position, mouse.position) <= attackRange)
        {
            mouseInAttackRange = true;
        }
        else
        {
            mouseInAttackRange = false;
        }
        return mouseInAttackRange;
    }

    void ChaseMouse()
    {
        isChasing = true;
        isPatrolling = false;
        Move(chaseSpeed);
        // Set destination to mouse's last known position
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
        // eatMouse();
        // Win();
        Debug.Log("Mouse Caught");

    }


    // navAgent functionality
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
