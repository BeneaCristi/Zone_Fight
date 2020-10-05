using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Skybox : MonoBehaviour
{

    public float speed; 
    void Start()
    {
        animate();
    }

    void animate()
    {
        transform.DORotate(new Vector3(0, 1, 0), speed).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
    }
}
