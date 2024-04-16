using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cheese : MonoBehaviour
{
    // This tag is used to identify cheese objects in the FindNearestCheese method of the MouseBehaviour script
    public string cheeseTag = "Cheese";

    public bool isCollected = false;


    // This method is called when the cheese is collected by the mouse
    public void OnCollected()
    {
        // Implement logic to handle the cheese being collected
        // For example, you might want to play a sound, increase a score, or destroy the cheese object
        if (!isCollected)
        {
            Debug.Log("Cheese collected!");
            isCollected = true; // Mark the cheese as collected
            Destroy(gameObject); // Destroy the cheese object after it's collected
        }

    }

    void Start()
    {
        // Ensure the cheese object has the correct tag
        gameObject.tag = cheeseTag;
    }

}
