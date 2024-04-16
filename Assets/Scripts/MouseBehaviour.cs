using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MouseBehaviour : MonoBehaviour
{
    public Transform catTransform; // Reference to the cat's transform
    public float safeDistance = 10f; // The distance at which the mouse is considered safe
    public bool isSafe = true;
    
    
    private bool cheeseFound = false;
    private bool cheeseClose = false;
    private bool collected = false;

    // Reference to the NavMeshAgent component
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
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
            EscapeFromCat();
        }
    }


    void FindCheese()
    {
        GameObject nearestCheese = FindNearestCheese();
        if (nearestCheese != null && IsCheeseVisible(nearestCheese))
        {
            agent.SetDestination(nearestCheese.transform.position);
            cheeseFound = true;
        }
    }



    void ApproachCheese()
    {
        GameObject nearestCheese = FindNearestCheese();
        if (nearestCheese != null)
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
            // Assuming you have a method to collect the cheese
            CollectCheeseAtPosition(agent.destination);
            collected = true;
        }
    }

    void CollectCheeseAtPosition(Vector3 position)
    {
        // Assuming the cheese object is at the destination position
        GameObject cheese = GameObject.FindGameObjectWithTag("Cheese");
        if (cheese != null)
        {
            Cheese cheeseScript = cheese.GetComponent<Cheese>();
            if (cheeseScript != null && !cheeseScript.isCollected)
            {
                // Call the OnCollected method on the Cheese script
                cheeseScript.OnCollected();
            }
        }
    }




    void EscapeFromCat()
    {
        // Example: Set a random destination within a safe area
        Vector3 safeAreaCenter = new Vector3(0, 0, 0); // Adjust this to your safe area's center
        Vector3 randomDirection = Random.insideUnitSphere * 10; // Adjust the range as needed
        Vector3 safeDestination = safeAreaCenter + randomDirection;
        agent.SetDestination(safeDestination);
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

    bool IsCheeseVisible(GameObject cheese)
    {
        Vector3 directionToCheese = cheese.transform.position - transform.position;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToCheese, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject == cheese)
            {
                return true;
            }
        }
        return false;
    }


    void MoveRandomly()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10; // Adjust the range as needed
        Vector3 randomDestination = transform.position + randomDirection;
        agent.SetDestination(randomDestination);
    }


}
