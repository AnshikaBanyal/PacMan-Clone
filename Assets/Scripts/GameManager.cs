using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    MovementController movementController;

    //to have pacman
    public GameObject pacman;

    // to have the warpingNodes
    public GameObject leftWarpNode;
    public GameObject rightWarpNode;


    //o have all the nodes at which the ghost start from
    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;

    //differnt ghost modes
    public enum GhostMode
    {
        chase,      //chases the pacman 
        scatter     // goes to the corner of the map for some time
    }
    public GhostMode currentGhostMode;
    public int[] ghostModeTimers = new int[] {7, 20, 7, 20, 5, 20, 5};
    public int ghostModeIndex;
    public float ghostModeTimer = 0;
    public bool runningTimer;
    public bool completedTimer;


    //all the Ghosts references
    public GameObject redGhost;
    public GameObject pinkGhost;
    public GameObject blueGhost;
    public GameObject orangeGhost;

    public EnemyController redGhostController;
    public EnemyController pinkGhostController;
    public EnemyController blueGhostController;
    public EnemyController orangeGhostController;
    

    public int totalPellets;
    public int pelletsLeft;
    public int pelletsCollectedOnThisLife;

    public bool hadDeathOnThisLevel = false;
    
    //audio sounds of the game
    public AudioSource siren;
    public AudioSource munch1;
    public AudioSource munch2;
    public int currentMunch;
    public AudioSource  startGameAudio;
    public AudioSource death;
    public AudioSource powerPelletAudio;
    public AudioSource respawingAudio;
    public AudioSource ghostEatenAudio;

    //for powerPettels
    public bool isPowerPelletRunning = false;
    public float currentPowerPelletTimer = 0;
    public float powerPelletTimer = 8f;
    public int powerPelletMultiplyer = 1;

    //the score record of the game
    public int score = 0;
    public Text scoreText;
    public Image blackBackground;
    public Text GameOverText;
    public Text livesText;

    public List<NodeController> nodeControllers = new List<NodeController>();

    public int lives;
    public int currentLevel;

    public bool gameIsRunning;
    public bool newGame;
    public bool clearedLevel;

    // Start is called before the first frame update
    void Awake()
    {
        ghostNodeStart.GetComponent<NodeController>().isGhostStartingNode = true;
        movementController = GetComponent<MovementController>();

        newGame = true;
        clearedLevel = false;
        blackBackground.enabled = false;

        redGhostController = redGhost.GetComponent<EnemyController>();
        pinkGhostController = pinkGhost.GetComponent<EnemyController>();
        blueGhostController = blueGhost.GetComponent<EnemyController>();
        orangeGhostController = orangeGhost.GetComponent<EnemyController>();

        StartCoroutine(SetUp());
    }
    
    public IEnumerator SetUp()
    {
        ghostModeIndex = 0;
        ghostModeTimer = 0;
    

        GameOverText.enabled = false;
        completedTimer = false;
        runningTimer = true;

        //if level Cleared -> flase a background and pause gam for 0.1 sec
        if (clearedLevel)
        {
            blackBackground.enabled = true;
            yield return new WaitForSeconds(0.1f);
            blackBackground.enabled = false;
        }
        pelletsCollectedOnThisLife = 0;
        currentGhostMode = GhostMode.scatter;
        gameIsRunning = false;

        float waitTime = 1f;

        if (clearedLevel || newGame)
        {
            waitTime = 4f;
            pelletsLeft = totalPellets;
            //pellets only repawn if -> new Game || next Level
            for (int i = 0; i < nodeControllers.Count; i++ )     nodeControllers[i].RespawnPellet(); 
        }

        if (newGame)
        {
            startGameAudio.Play();
            score = 0;
            scoreText.text = "Score: " + score.ToString();
            SetLives(3);
            currentLevel = 0;
        }

       pacman.GetComponent<PlayerController>().SetUp();

        redGhostController.SetUp();
        pinkGhostController.SetUp();
        blueGhostController.SetUp();
        orangeGhostController.SetUp();
        
        newGame = false;
        clearedLevel = false;
        currentMunch = 0;

        yield return new WaitForSeconds(waitTime);

        StartGame();
       
    }

    void SetLives(int newLives)     
    {
        lives = newLives;
        livesText.text = "Lives: " + lives.ToString();
    }

    void StartGame()
    {
        gameIsRunning = true;
        siren.Play();
    }

    void StopGame()
    {
        gameIsRunning = false;
        pacman.GetComponent<PlayerController>().Stop();
        siren.Stop();
        powerPelletAudio.Stop();
        respawingAudio.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if(!gameIsRunning)      return;

        if 
        (
            redGhostController.ghostNodeStates == EnemyController.GhostNodeStatesEnum.respwaning
            || pinkGhostController.ghostNodeStates == EnemyController.GhostNodeStatesEnum.respwaning
            || blueGhostController.ghostNodeStates == EnemyController.GhostNodeStatesEnum.respwaning
            || orangeGhostController.ghostNodeStates == EnemyController.GhostNodeStatesEnum.respwaning
        )       
        {
            if (!respawingAudio.isPlaying)      respawingAudio.Play();
        }
        else 
        {
            if (respawingAudio.isPlaying)       respawingAudio.Stop();
        }

        if(!completedTimer && runningTimer)    
        {
            ghostModeTimer += Time.deltaTime;
            if (ghostModeTimer >= ghostModeTimers[ghostModeIndex])
            {
                ghostModeTimer = 0;
                ghostModeIndex++;
                if (currentGhostMode == GhostMode.chase)    currentGhostMode = GhostMode.scatter;
                else        currentGhostMode = GhostMode.chase;

                if(ghostModeIndex == ghostModeTimers.Length)
                {
                    completedTimer = true;
                    runningTimer = false;
                    currentGhostMode = GhostMode.chase;
                }
            }
        }

        if(isPowerPelletRunning)
        {
            currentPowerPelletTimer +=Time.deltaTime;
            if (currentPowerPelletTimer >= powerPelletTimer)
            {
                isPowerPelletRunning = false;
                currentPowerPelletTimer = 0;
                powerPelletMultiplyer = 1;
                siren.Play();
            }
        }
    }

    public void AddToScore (int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score.ToString();
    }

    public void GotPelletFromNodeController(NodeController nodeController)
    {
        nodeControllers.Add(nodeController);
        totalPellets++;
        pelletsLeft++;
    }

    public IEnumerator collectedPellet(NodeController nodeController)
    {
        if (currentMunch == 0) 
        {
            munch1.Play();
            currentMunch = 1;
        }
        else if (currentMunch == 1)
        {
            munch2.Play();
            currentMunch = 0;
        }

        pelletsLeft--;
        pelletsCollectedOnThisLife++;

        int requiredBluePellets = 0;
        int requiredOrangePellets = 0;

        if (hadDeathOnThisLevel)
        {
            requiredBluePellets = 12;
            requiredOrangePellets = 32;
        }
        else{
            requiredBluePellets = 30;
            requiredOrangePellets = 60;
        }
        if (pelletsCollectedOnThisLife >= requiredBluePellets && !blueGhost.GetComponent<EnemyController>().leftHomeBefore)    blueGhost.GetComponent<EnemyController>().readyToLeaveHome = true;
        if (pelletsCollectedOnThisLife > requiredOrangePellets && !orangeGhost.GetComponent<EnemyController>().leftHomeBefore)    orangeGhost.GetComponent<EnemyController>().readyToLeaveHome = true;

        //Add our score
        AddToScore(10);

        if (pelletsLeft == 0)
        {
            currentLevel++;
            clearedLevel = true;
            StopGame();
            yield return new WaitForSeconds(1);
            StartCoroutine(SetUp());
        }

        //is this a power pellet
        if (nodeController.isPowerPellet)
        {
            siren.Stop();
            powerPelletAudio.Play();
            isPowerPelletRunning = true;
            currentPowerPelletTimer = 0;
            //powerPelletMultiplyer +=1;

            redGhostController.SetFrightened(true);
            pinkGhostController.SetFrightened(true);
            blueGhostController.SetFrightened(true);
            orangeGhostController.SetFrightened(true);
            
      }
    }


    public IEnumerator PauseGame(float timeToPause)
    {
        gameIsRunning = false;
        yield return new WaitForSeconds(timeToPause);
        gameIsRunning = true;
    }

    public void GhostEaten()
    {
        ghostEatenAudio.Play();
        AddToScore(400 * powerPelletMultiplyer);
        powerPelletMultiplyer++;
        StartCoroutine(PauseGame(1));
    }    

    public IEnumerator PlayerEaten()
    {
        hadDeathOnThisLevel = true;
        StopGame();
        yield return new WaitForSeconds(1);

        redGhostController.SetVisible(false);
        pinkGhostController.SetVisible(false);
        blueGhostController.SetVisible(false);
        orangeGhostController.SetVisible(false);
        
        pacman.GetComponent<PlayerController>().Death();
        death.Play();

        yield return new WaitForSeconds(3);

        SetLives(lives - 1);
        if (lives <= 0)
        {
            newGame = true;
            GameOverText.enabled = true;

            yield return new WaitForSeconds(3);
        }
        StartCoroutine(SetUp());
    }

   
}
