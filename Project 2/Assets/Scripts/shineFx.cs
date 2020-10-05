using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class shineFx : MonoBehaviour
{

    public Transform shineT;
    public float offset;
    public float speed;
    public float minDelay;
    public float maxDelay;


    void Start()
    {
        animate();
    }


    void animate()
    {
        shineT.DOLocalMoveX(offset, speed).SetDelay(Random.Range(minDelay, maxDelay)).SetEase(Ease.Linear).OnComplete(() =>
        {
            shineT.DOLocalMoveX(-offset,0);
            animate();
        });
    }
}
