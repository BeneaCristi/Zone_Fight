using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CnControls;
using System.Numerics;
using DG.Tweening;
using TMPro;
using UnityEngine.AI;

public class Player : MonoBehaviour
{

    public bool active;
    public bool isAi;
    public Transform[] bones;
    public bool keepBonesStraight;
    public Transform zone;
    public Transform target;

    public float speed;
    public float turnSpeed;

    bool touched;
    UnityEngine.Vector3 lastTouchPos;

    private Rigidbody rb;

    public bool isInZone;
    public int score;

    public Transform scoreFx;

    private gameManager gameManagerScript;
    private NavMeshAgent nav;
    private Zone zoneScript;
    private float defaultSpeed;
    public float outsideOfZoneRange;
    private Animator anim;
    public ParticleSystem trailFx;

    public bool canPunch = true;
    public float punchForce;
    public GameObject punchFx;
    public bool stunned;
    public Transform puncher;
    private CameeraFollow cameraFollowScript;

    public bool isBomber;
    public bool unstable;
    public Transform explosionFx;

    public Transform scoreCard;
    public TextMeshPro playerName;
    public GameObject crown;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManagerScript = GameObject.FindWithTag("manager").GetComponent<gameManager>();
        anim = GetComponent<Animator>();
        cameraFollowScript = Camera.main.transform.GetComponent<CameeraFollow>();
        InvokeRepeating("addZoneScore", 1, 1);

        if(isAi)
        {
            nav = GetComponent<NavMeshAgent>();
            zoneScript = zone.GetComponent<Zone>();
        }
        else
        {
            defaultSpeed = speed;
        }



    }

    private void FixedUpdate()
    {
        if(active && !isAi)
        {
            rb.MovePosition(transform.position + transform.forward * speed * Time.deltaTime);
        }
        
    }

    private void OnEnable()
    {
        if(active)
        {
            canPunch = true;
            stunned = false;
            ParticleSystem.EmissionModule em = trailFx.emission;
            em.enabled = true;
            anim.SetBool("run", true);
            rb.velocity = UnityEngine.Vector3.zero;
            
            if(isAi)
            {
                nav.enabled = true;
            }
            else
            {
                cameraFollowScript.enabled = true;
                speed = defaultSpeed;
            }

            if (isBomber)
                unstable = false;


            UnityEngine.Vector3 dir = new UnityEngine.Vector3(transform.position.x, 0, transform.position.z) - new UnityEngine.Vector3(zone.position.x, 0, zone.position.z);
            transform.rotation = UnityEngine.Quaternion.LookRotation(-dir);


        }
    }

    void Update()
    {
        if(gameManagerScript.gameStarted && !active)
        {
            active = true;
            anim.SetBool("run", true);
            ParticleSystem.EmissionModule em = trailFx.emission;
            em.enabled = true;
            if(isAi)
            {
                nav.enabled = true;
            }
        }

        if (active)
        {


            if (isAi)
            {
                if (!stunned)
                {
                    if (gameManagerScript.playersInZone.Contains(transform))
                    {
                        target = GetClosestEnemyInZone(gameManagerScript.playersInZone);
                        if (target == null)
                        {
                            target = zoneScript.wayPoints[Random.Range(0, zoneScript.wayPoints.Length)];
                        }
                    }
                    else
                    {
                        target = GetClosestEnemy(gameManagerScript.players);

                        if (UnityEngine.Vector3.Distance(transform.position, target.position) > outsideOfZoneRange)
                        {
                            target = zone;
                        }
                    }
                    nav.SetDestination(target.position);
                }
               
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    UnityEngine.Vector3 v1 = UnityEngine.Vector3.zero;
                    UnityEngine.Vector3 v2 = new UnityEngine.Vector3(CnInputManager.GetAxis("Horizontal"), CnInputManager.GetAxis("Vertical"), 0);
                    v1 = v2.normalized;

                    if (v1.sqrMagnitude < 1E-05f)
                    {
                        v1 = lastTouchPos;
                    }

                    UnityEngine.Vector3 touchPos = transform.position + v1;
                    UnityEngine.Vector3 touchDir = touchPos - transform.position;
                    float angle = Mathf.Atan2(touchDir.y, touchDir.x) * Mathf.Rad2Deg;

                    if (touched)
                        angle -= 90;

                    UnityEngine.Quaternion rot = UnityEngine.Quaternion.AngleAxis(angle, UnityEngine.Vector3.down);
                    transform.rotation = rot;
                    lastTouchPos = v1;

                    if (!touched)
                        touched = true;
                }
            }

              if(transform.position.y < -1 && !stunned)
              {
                ParticleSystem.EmissionModule em = trailFx.emission;
                em.enabled = false;
                stunned = true;
                speed = 0;
                StartCoroutine(death());
              }
        }



        
    }

    private void LateUpdate()
    {
        if(keepBonesStraight)
        {
            foreach (Transform t in bones)
                t.eulerAngles = new UnityEngine.Vector3(0, t.eulerAngles.y, t.eulerAngles.z);
        }
    }

    private void addZoneScore()
    {
        
        if(isInZone)
        {
            if (isAi)
            {
                score++;
            }
            else
            {
                score++;

                Transform t = Instantiate(scoreFx, new UnityEngine.Vector3(transform.position.x, transform.position.y + 2, transform.position.z), UnityEngine.Quaternion.identity);
                TextMeshPro txt = t.GetComponent<TextMeshPro>();
                txt.DOFade(0, .5f).SetDelay(.5f);
                t.DOMoveY(t.position.y + 2, 1);
                Destroy(t.gameObject, 2);
            }
            
        }
    }

    public void addKnockOutScore()
    { 
            if (isAi)
            {
                score += 4;
            }
            else
            {
                score += 4;

                Transform t = Instantiate(scoreFx, new UnityEngine.Vector3(transform.position.x, transform.position.y + 2, transform.position.z), UnityEngine.Quaternion.identity);
                TextMeshPro txt = t.GetComponent<TextMeshPro>();
                txt.text = "+4";
                txt.color = Color.yellow;
                txt.DOFade(0, .5f).SetDelay(.5f);
                t.DOMoveY(t.position.y + 2, 1);
                t.DOPunchScale(new UnityEngine.Vector3(.5f, .5f, .5f), .8f);
                Destroy(t.gameObject, 2);
            }
    }


    private Transform GetClosestEnemyInZone(List<Transform> enemies)
    {
        Transform bestTarget = null;
        float smallestDistance = Mathf.Infinity;
        UnityEngine.Vector3 currentPosition = transform.position;

        foreach(Transform potentialTarget in enemies)
        {
            if (potentialTarget != transform)
            {
                UnityEngine.Vector3 directionToTarget = potentialTarget.position - currentPosition;
                float distance = directionToTarget.sqrMagnitude;

                if(distance < smallestDistance)
                {
                    smallestDistance = distance;
                    bestTarget = potentialTarget;
                }
            }
        }

        return bestTarget;
    }

    private Transform GetClosestEnemy(Player[] enemies)
    {
        Transform bestTarget = null;
        float smallestDistance = Mathf.Infinity;
        UnityEngine.Vector3 currentPosition = transform.position;

        foreach (Player potentialTarget in enemies)
        {
            if (potentialTarget.transform != transform)
            {
                UnityEngine.Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                float distance = directionToTarget.sqrMagnitude;

                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    bestTarget = potentialTarget.transform;
                }
            }
        }

        return bestTarget;
    }

    public void punch(Transform other)
    {
        
        if(canPunch)
        {
            if (isBomber)
            {
                if(!unstable)
                {
                    unstable = true;

                    Material mat = transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material;

                    Sequence unstableAnimation = DOTween.Sequence();
                    unstableAnimation.Append(mat.DOColor(Color.red, .25f));
                    unstableAnimation.Join(transform.GetChild(0).DOScale(1.5f, .25f));
                    unstableAnimation.Append(mat.DOColor(Color.white, .25f));
                    unstableAnimation.Join(transform.GetChild(0).DOScale(1f, .25f));
                    unstableAnimation.SetLoops(5);
                    unstableAnimation.OnComplete(() =>
                    {
                        Transform t = Instantiate(explosionFx, new UnityEngine.Vector3(transform.position.x, transform.position.y + .5f, transform.position.z), UnityEngine.Quaternion.identity);
                        Destroy(t.gameObject, 3);

                        StartCoroutine(death());

                        Collider[] cols = Physics.OverlapSphere(transform.position, 5);

                        foreach(Collider c in cols)
                        {
                            if(c.CompareTag("Player"))
                            {
                                Rigidbody _rb = c.GetComponent<Rigidbody>();
                                _rb.velocity = UnityEngine.Vector3.zero;
                                _rb.velocity = (c.transform.position - transform.position).normalized * punchForce;

                                Player tempPlayerScript = c.GetComponent<Player>();
                                tempPlayerScript.StartCoroutine(tempPlayerScript.stun());

                            }
                        }
                    });
                }
            }
            else
            {
                canPunch = false;
                anim.SetBool("attack", true);
                StartCoroutine(resetPunch());

                Player tempPlayerScript = other.GetComponent<Player>();
                tempPlayerScript.puncher = transform;
                tempPlayerScript.StartCoroutine(tempPlayerScript.stun());


                Rigidbody _rb = other.GetComponent<Rigidbody>();
                _rb.velocity = transform.forward * punchForce;
            }
        }
    }

    private IEnumerator resetPunch()
    {
        yield return new WaitForSeconds(.1f);
        keepBonesStraight = false;
        punchFx.SetActive(true);

        yield return new WaitForSeconds(.25f);
        keepBonesStraight = true;
        punchFx.SetActive(false);
        canPunch = true;
        anim.SetBool("attack", false);
    }

    public IEnumerator stun()
    {
        stunned = true;
        ParticleSystem.EmissionModule em = trailFx.emission;
        em.enabled = false;

        if(isAi)
        {
            canPunch = false;
            nav.enabled = false;
        }
        else
        {
            speed = 0;
        }

        yield return new WaitForSeconds(.25f);

        if (UnityEngine.Vector3.Distance(UnityEngine.Vector3.zero, new UnityEngine.Vector3(transform.position.x, 0, transform.position.z)) < 12)
        {
            rb.velocity = UnityEngine.Vector3.zero;
            stunned = false;
            em.enabled = true;

            if(isAi)
            {
                canPunch = true;
                nav.enabled = true;
            }
            else
            {
                speed = defaultSpeed;
            }
        }
        else
        {
            if(puncher)
            {
                puncher.GetComponent<Player>().addKnockOutScore();
                puncher = null;
            }

            StartCoroutine(death());
        }
    }

    private IEnumerator death()
    {
         if(!isAi)
         {
            cameraFollowScript.enabled = false;
         }

        yield return new WaitForSeconds(.5f);

        gameObject.SetActive(false);
        transform.position = new UnityEngine.Vector3(0, -100, 0);
        gameManagerScript.StartCoroutine(gameManagerScript.respawn(transform , 3));

    }

    public void stop(bool won)
    {
        active = false;
        ParticleSystem.EmissionModule em = trailFx.emission;
        em.enabled = false;
        anim.SetBool("run", false);

        if (isAi)
            nav.enabled = false;
        else
            speed = 0;



        if(won)
        {
            anim.SetBool("won", true);
        }
        else
        {
            anim.SetBool("lost", true);
        }
    }
}
