using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{

    private gameManager gameManagerScript;
    public Transform[] wayPoints;

    void Start()
    {
        gameManagerScript = GameObject.FindGameObjectWithTag("manager").GetComponent<gameManager>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            gameManagerScript.playersInZone.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManagerScript.playersInZone.Remove(other.transform);
        }
    }
}
