using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MouseBehaviour : MonoBehaviour
{
    public Transform catTransform; // Reference to the cat's transform
    public float safeDistance = 10f; // The distance at which the mouse is considered safe
    public bool isSafe = true;
    public float recalculateCooldown = 1f; // Cooldown period in seconds

    private bool cheeseFound = false;
    private bool cheeseClose = false;
    private bool collected = false;
    private NavMeshAgent agent;
    private float lastRecalculateTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        lastRecalculateTime = Time.time;
    }
    void Update()
    {
        float distanceToCat = Vector3.Distance(transform.position, catTransform.position);
        if (distanceToCat <= safeDistance)
        {
            isSafe = false; // The mouse is not safe
        }
        else
        {
            isSafe = true; // The mouse is safe
        }

        if (isSafe)
        {
            if (!cheeseFound)
            {
                FindCheese();
                if (!cheeseFound) // If cheese is not found, move randomly
                {
                    MoveRandomly();
                }
            }
            else if (!cheeseClose)
            {
                ApproachCheese();
            }
            else if (!collected)
            {
                CollectCheese();
            }
        }
        else
        {
            if (Time.time >= lastRecalculateTime + recalculateCooldown)
            {
                EscapeFromCat();
                lastRecalculateTime = Time.time;
            }
        }
    }

    void FindCheese()
    {
        GameObject nearestCheese = FindNearestCheese();
        if (nearestCheese != null && IsCheeseVisibleAndNotBlockedByCat(nearestCheese))
        {
            agent.SetDestination(nearestCheese.transform.position);
            cheeseFound = true;
        }
    }

    void ApproachCheese()
    {
        GameObject nearestCheese = FindNearestCheese();
        if (nearestCheese != null && IsCheeseVisibleAndNotBlockedByCat(nearestCheese))
        {
            float distanceToCheese = Vector3.Distance(transform.position, nearestCheese.transform.position);
            if (distanceToCheese <= agent.stoppingDistance)
            {
                cheeseClose = true;
            }
            else
            {
                agent.SetDestination(nearestCheese.transform.position);
            }
        }
    }

    void CollectCheese()
    {
        if (cheeseClose)
        {
            CollectCheeseAtPosition(agent.destination);
            collected = true;
        }
    }

    void CollectCheeseAtPosition(Vector3 position)
    {
        GameObject cheese = GameObject.FindGameObjectWithTag("Cheese");
        if (cheese != null)
        {
            Cheese cheeseScript = cheese.GetComponent<Cheese>();
            if (cheeseScript != null && !cheeseScript.isCollected)
            {
                cheeseScript.OnCollected();
            }
        }
    }


    void EscapeFromCat()
    {
        // Calculate the direction away from the cat
        Vector3 directionAwayFromCat = transform.position - catTransform.position;

        // Normalize the direction vector
        Vector3 normalizedDirection = directionAwayFromCat.normalized;

        // Predict the cat's future position based on its current velocity
        // Assuming the cat has a Rigidbody component attached and is using physics-based movement
        Vector3 predictedCatPosition = catTransform.position + catTransform.GetComponent<Rigidbody>().velocity * 2f; // Adjust the multiplier as needed

        // Calculate the new escape direction considering the cat's predicted position
        Vector3 escapeDirection = (transform.position - predictedCatPosition).normalized * safeDistance;

        // Attempt to find the best destination within a safe distance
        int attempts = 10;
        Vector3 bestDestination = transform.position;
        float maxDistance = 0f;

        for (int i = 0; i < attempts; i++)
        {
            // Generate a random offset around the escape direction
            Vector3 randomOffset = Random.insideUnitSphere * safeDistance;

            // Calculate a candidate position for the mouse to move to
            Vector3 candidatePosition = transform.position + escapeDirection + randomOffset;

            // Check if the candidate position is valid on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidatePosition, out hit, safeDistance, NavMesh.AllAreas))
            {
                // Calculate the distance from the candidate position to the cat's predicted position
                float distanceToPredictedCatPosition = Vector3.Distance(hit.position, predictedCatPosition);

                // Update the best destination if the candidate position is further away from the cat's predicted position
                if (distanceToPredictedCatPosition > maxDistance)
                {
                    maxDistance = distanceToPredictedCatPosition;
                    bestDestination = hit.position;
                }
            }
        }

        // Set the mouse's destination to the best calculated escape position
        agent.SetDestination(bestDestination);
    }


    GameObject FindNearestCheese()
    {
        GameObject[] cheeses = GameObject.FindGameObjectsWithTag("Cheese");
        GameObject nearestCheese = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject cheese in cheeses)
        {
            float distance = Vector3.Distance(transform.position, cheese.transform.position);
            if (distance < nearestDistance)
            {
                nearestCheese = cheese;
                nearestDistance = distance;
            }
        }

        return nearestCheese;
    }

    bool IsCheeseVisibleAndNotBlockedByCat(GameObject cheese)
    {
        Vector3 directionToCheese = cheese.transform.position - transform.position;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToCheese, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject == cheese)
            {
                // Additional check to ensure the cat is not blocking the path to the cheese
                Vector3 directionToCat = catTransform.position - transform.position;
                RaycastHit catHit;
                if (Physics.Raycast(transform.position, directionToCat, out catHit, Mathf.Infinity))
                {
                    if (catHit.collider.gameObject == catTransform.gameObject)
                    {
                        // Cat is in the way, so cheese is considered not visible
                        return false;
                    }
                }
                return true;
            }
        }
        return false;
    }

    void MoveRandomly()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10; // Adjust the range as needed
        Vector3 randomDestination = transform.position + randomDirection;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDestination, out hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
