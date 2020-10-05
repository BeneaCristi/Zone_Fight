using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class punchTrigger : MonoBehaviour
{

    public Player playerScript;
    void Start()
    {
        
    }


    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerScript.punch(other.transform);
        }
    }
}
