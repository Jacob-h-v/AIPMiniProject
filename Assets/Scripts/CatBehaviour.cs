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

    int waypointIndex = 0;

    [Header("Cat Behaviour Modifiers")]
    [SerializeField] float timeToRotate = 2f, patrolSpeed = 5f, chaseSpeed = 8f, viewRange = 15f, viewAngle = 90f, attackRange = 1f;

    [Header("References")]
    [SerializeField] LayerMask mouseMask;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] UnityEngine.AI.NavMeshAgent navAgent;
    [SerializeField] Transform[] waypoints;
    [SerializeField] Transform mouse;

    Vector3 mouseLastKnownPos = Vector3.zero;
    BehaviorTreeController behaviorTreeController;

    // Start is called before the first frame update
    void Start()
    {
        // Load and start navAgent.
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        // Create the AI's behavior tree
        // Define your behavior tree structure
        Node behaviorTreeRoot = new Fallback(new List<Node>
        {
            new Sequence(new List<Node>
            {
                new Condition(ConditionMouseFound),
                new Action(ChaseMouse)
            }),
            new Sequence(new List<Node>
            {
                new Condition(ConditionMouseInAttackRange),
                new Action(CatchMouse)
            }),
            new Action(Patrol)
        });
        Patrol();

    }

    // Update is called once per frame
    void Update()
    {


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
