using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAtCam : MonoBehaviour
{

    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        UnityEngine.Vector3 dir = transform.position - cam.position;
        UnityEngine.Quaternion rot = UnityEngine.Quaternion.LookRotation(dir);
        transform.rotation = rot;
    }
}
