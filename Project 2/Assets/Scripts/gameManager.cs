using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine.SceneManagement;


public class gameManager : MonoBehaviour
{
    public bool gameStarted;
    public List<Transform> playersInZone = new List<Transform>();
    public Player[] players;
    public Transform respawnT;
    private int respawnTries;
    public float timer;
    public TextMeshProUGUI timerLabel;
    public Color[] playerColors;
    public nameManager nameManagerScript;
    private List<Transform> sortedList = new List<Transform>();
    private Camera cam;
    private CameeraFollow camFollowScript;
    public TMP_InputField nameField;
    private string username;
    public Transform loadingImg;
    public TextMeshProUGUI playerCountLabel;
    public TextMeshProUGUI hintLabel;
    public string[] hints;
    public GameObject menuPanel;
    public GameObject matchMakingPanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;
    public GameObject respawnPanel;
    public Transform[] bots;
    public Transform zone;
    public Vector3 inGameCamPos;
    public Vector3 matchMakingCamPos;
    public Transform[] gameOverScoreCard;
    public TextMeshProUGUI gameOverPosLabel;
    public TextMeshProUGUI respawnLabel;


    void Start()
    {
        cam = Camera.main;
        camFollowScript = cam.GetComponent<CameeraFollow>();
     
        if(PlayerPrefs.HasKey("username"))
        {
            username = PlayerPrefs.GetString("username");
        }
        else
        {
            username = "Player";
        }
        nameField.text = username;
        loadingImg.DORotate(new UnityEngine.Vector3(0, 0, -1), .005f).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
        setupPlayer();

        hintLabel.text = hints[Random.Range(0, hints.Length)];
    }


    void Update()
    {

       


        foreach(Player p in players )
        {
            if (playersInZone.Contains(p.transform))
            {
                if(!p.isInZone)
                {
                    p.isInZone = true;
                }
                else
                {
                    if (p.isInZone)
                    {
                        p.isInZone = false;
                    }
                }
            }
        }

        if(gameStarted)
        {
            if (timer > 0)
            {
                if(timer<10 && timerLabel.color != Color.red)
                {
                    timerLabel.color = Color.red;
                }
                timer -= Time.deltaTime;
                timerLabel.text = (int)timer / 60 + ":" + ((int)timer % 60).ToString("00");

                updateScores();
            }
            else
            {
                StartCoroutine(gameOver());
            }
        }
    }

    public IEnumerator respawn(Transform t, float delay)
    {
        if (!t.GetComponent<Player>().isAi)
        {
            respawnPanel.SetActive(true);
            gamePanel.SetActive(false);

            respawnLabel.text = "3";
            yield return new WaitForSeconds(delay / 3);
            respawnLabel.text = "2";
            yield return new WaitForSeconds(delay / 3);
            respawnLabel.text = "1";
            yield return new WaitForSeconds(delay / 3);
        }
        else
        {
            yield return new WaitForSeconds(delay);
        }
           

        if (!t.GetComponent<Player>().isAi)
        {
            respawnPanel.SetActive(false);
            gamePanel.SetActive(true);

        }

        if (gameStarted)
            spawn(t);
    }

    private void spawn(Transform t)
    {
        respawnT.eulerAngles = new UnityEngine.Vector3(0, Random.Range(0, 359), 0);
        Collider[] cols = Physics.OverlapSphere(respawnT.GetChild(0).position, 5);

        bool playerNearby = false;

        foreach(Collider c in cols)
        {
            if (c.CompareTag("Player"))
                playerNearby = true;
        }

        if(!playerNearby)
        {
            t.position = respawnT.GetChild(0).position;
            t.gameObject.SetActive(true);
            respawnTries = 0;

        }
        else
        {
            if(respawnTries<10)
            {
                respawnTries++;
                spawn(t);
            }
            else
            {
                t.position = respawnT.GetChild(0).position;
                t.gameObject.SetActive(true);
                respawnTries = 0;
            }
        }
    }

    private IEnumerator gameOver()
    {
        gameStarted = false;

        for(int i=0;i<sortedList.Count;i++)
        {
            if(i==0)
            {
                camFollowScript.focusOnWinner(sortedList[i]);
                sortedList[i].GetComponent<Player>().stop(true);
            }
            else
            {
                sortedList[i].GetComponent<Player>().stop(false);
            }
        }

       for(int i=0;i<players.Length;i++)
        {
            Player tempPlayerScript = sortedList[i].GetComponent<Player>();
            gameOverScoreCard[i].GetChild(0).GetComponent<TextMeshProUGUI>().text = tempPlayerScript.scoreCard.GetSiblingIndex() + 1 + ".";
            gameOverScoreCard[i].GetChild(1).GetComponent<TextMeshProUGUI>().text = tempPlayerScript.playerName.text;
            gameOverScoreCard[i].GetChild(2).GetComponent<TextMeshProUGUI>().text = "- " + tempPlayerScript.score;
            gameOverScoreCard[i].GetComponent<Image>().color = tempPlayerScript.scoreCard.GetComponent<Image>().color;


            if(!tempPlayerScript.isAi)
            {
                gameOverPosLabel.text = "You finished " + (i+1) + getRankOrdinal(i+1);
            }

        }

       

        yield return new WaitForSeconds(5);

        gameOverPanel.SetActive(true);

    }

    public void restartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void updateScores()
    {
        sortedList.Clear();
        Dictionary<Transform, int> unsortedDic = new Dictionary<Transform, int>();

        foreach(Player p in players)
        {
            unsortedDic.Add(p.transform, p.score);
        }

        foreach(var item in unsortedDic.OrderByDescending(i => i.Value))
        {
            sortedList.Add(item.Key);
        }

        for(int i=0; i<players.Length; i++)
        {
            Player tempPlayerScript = sortedList[i].GetComponent<Player>();

            if(i==0)
            {
                tempPlayerScript.crown.SetActive(true);
            }
            else
            {
                tempPlayerScript.crown.SetActive(false);
            }

            tempPlayerScript.scoreCard.SetSiblingIndex(i);
            tempPlayerScript.scoreCard.GetChild(0).GetComponent<TextMeshProUGUI>().text = (i + 1) + ".";
            tempPlayerScript.scoreCard.GetChild(2).GetComponent<TextMeshProUGUI>().text = "" + tempPlayerScript.score;
        }

    }

    private void setupPlayer()
    {
        int[] index = { 0, 1, 2, 3, 4 };
        System.Random rnd = new System.Random();
        int[] randomIndex = index.OrderBy(x => rnd.Next()).ToArray();

        for(int i=0; i<players.Length;i++)
        {
            string name = "";
           if(i==0)
            {
                name = username;
            }
           else
            { 
                name = nameManagerScript.names[Random.Range(0, nameManagerScript.names.Length)];
            }

            

            players[i].scoreCard.GetComponent<Image>().color = playerColors[i];
            players[i].scoreCard.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (i + 1) + ".";
            players[i].scoreCard.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = name;
            players[i].scoreCard.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "- 0";

            Material playerMat = players[i].transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material;
            playerMat.color = playerColors[i];
            players[i].playerName.text = name;
            players[i].playerName.color = playerColors[i];
        }
    }

    public void saveUserName()
    {
        username = nameField.text;
        PlayerPrefs.GetString("username", username);
        players[0].playerName.text = username;
        players[0].scoreCard.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = username;
    }


    private IEnumerator doMatchMacking()
    {


        cam.transform.DOMove(matchMakingCamPos, .5f);

        playerCountLabel.text = "1/5";
        yield return new WaitForSeconds(Random.Range(.25f, .7f));
        playerCountLabel.text = "2/5";
        bots[0].gameObject.SetActive(true);
        yield return new WaitForSeconds(Random.Range(.25f, .7f));
        playerCountLabel.text = "3/5";
        bots[1].gameObject.SetActive(true);
        yield return new WaitForSeconds(Random.Range(.25f, .7f));
        playerCountLabel.text = "4/5";
        bots[2].gameObject.SetActive(true);
        yield return new WaitForSeconds(Random.Range(.25f, .7f));
        playerCountLabel.text = "5/5";
        bots[3].gameObject.SetActive(true);
        yield return new WaitForSeconds(Random.Range(.25f, .7f));
        playerCountLabel.text = "Starting Game...";
        yield return new WaitForSeconds(Random.Range(.25f, .7f));
        zone.gameObject.SetActive(true);
        bots[4].gameObject.SetActive(true);
        matchMakingPanel.SetActive(false);
        gamePanel.SetActive(true);

        cam.transform.DOMove(inGameCamPos, .5f).OnComplete(() =>
         {
             camFollowScript.enabled = true;
         });

    }

    public void searchMatch()
    {
        
        menuPanel.SetActive(false);
        matchMakingPanel.SetActive(true);
        StartCoroutine(doMatchMacking());
    }

    public void startGame()
    {
        gameStarted = true;
    }

    private string getRankOrdinal(int rank)
    {
        string ordinal = "";

        switch(rank)
        {
            case 1:
                ordinal = "st";
                break;
            case 2:
                ordinal = "nd";
                break;
            case 3:
                ordinal = "rd";
                break;
            case 4:
                ordinal = "th";
                break;
            case 5:
                ordinal = "th";
                break;
            
        }

        return ordinal;
    }

}







