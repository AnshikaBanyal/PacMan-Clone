using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public MovementController movementController;
    public GameManager gameManager;

    public enum GhostNodeStatesEnum
    {
        respwaning,
        leftNode,
        rightNode,
        centerNode,
        startNode,
        movingNodes
    }
    public GhostNodeStatesEnum ghostNodeStates;
    public GhostNodeStatesEnum startGhostNodeState;
    public GhostNodeStatesEnum repawnState;

    public enum GhostType
    {
        red,
        blue,
        pink,
        orange
    }
    public GhostType ghostType;

    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;
    
    public GameObject startingNode;

    public bool readyToLeaveHome = false;
    public bool leftHomeBefore = false;

    public bool testRespawn = false;
    public bool isFrightened = false;  //this is not a Ghost Mode as once pacman eats a ghost the are no longer frighted but the rest are

    public bool isVisible = true;
    public SpriteRenderer ghostSprite;
    public SpriteRenderer eyesSprite;
    public Color color;

    public Animator animator;

    //an array of nodes to name them as scatterd nodes
    public GameObject[] scatterNodes; //its creates an arry of empty game objects
    public int scatterNodeIndex;

    public GameObject currentNode;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        movementController = GetComponent<MovementController>();
        animator = GetComponent<Animator>();

        ghostSprite = GetComponent<SpriteRenderer>();

        if (ghostType == GhostType.red)
        {
            startGhostNodeState = GhostNodeStatesEnum.startNode;
            startingNode = ghostNodeStart;
            repawnState = GhostNodeStatesEnum.centerNode;
        }
        else if (ghostType == GhostType.pink)
        {
            startGhostNodeState = GhostNodeStatesEnum.centerNode;
            startingNode = ghostNodeCenter;
            repawnState = GhostNodeStatesEnum.centerNode;
        }
        else if (ghostType == GhostType.blue) 
        {
            startGhostNodeState = GhostNodeStatesEnum.leftNode;
            startingNode = ghostNodeLeft;
            repawnState = GhostNodeStatesEnum.leftNode;
        }
        else if ( ghostType == GhostType.orange)
        {
            startGhostNodeState = GhostNodeStatesEnum.rightNode;
            startingNode = ghostNodeRight;
            repawnState = GhostNodeStatesEnum.rightNode;
        }
    }

    public void SetUp()
    {
        animator.SetBool("moving", false);

        ghostNodeStates = startGhostNodeState;
        readyToLeaveHome = false;
        leftHomeBefore = false;

        movementController.currentNode = startingNode;
        transform.position = startingNode.transform.position;

        movementController.direction = "";
        movementController.lastMovingDirection = "";

        if (ghostType == GhostType.red)
        {
            readyToLeaveHome = true;
            leftHomeBefore = true;
        }
        else if (ghostType == GhostType.pink)
        {
            readyToLeaveHome = true;
        }

        scatterNodeIndex = 0;
        isFrightened = false;
        SetVisible(true);

    }

    // Update is called once per frame
    void Update()
    {
        
        if (!gameManager.isPowerPelletRunning || ghostNodeStates != GhostNodeStatesEnum.movingNodes)      isFrightened = false;

        if (isVisible)
        {
            if(ghostNodeStates != GhostNodeStatesEnum.respwaning)
            {
                ghostSprite.enabled = true;
            }
            else 
            {
                ghostSprite.enabled = false;
            }
            
            eyesSprite.enabled = true;
        }
        else
        {
            ghostSprite.enabled = false;
            eyesSprite.enabled = false;

        }

        if (isFrightened)    
        {
            animator.SetBool("frightened",true);
            eyesSprite.enabled = false;
            ghostSprite.color = new Color(255, 255, 255, 255);
        }
        else    
        {
            animator.SetBool("frightened",false); 
            animator.SetBool("frightenedBlinking", false);
            ghostSprite.color = color;
        }

        if (!gameManager.gameIsRunning)     return;

        if (gameManager.powerPelletTimer - gameManager.currentPowerPelletTimer <= 3)    animator.SetBool("frightenedBlinking", true);
        else            animator.SetBool("frightenedBlinking", false);
        
        animator.SetBool("moving",true);

        if (movementController.currentNode.GetComponent<NodeController>().isSideNode)    
        {
            movementController.SetSpeed(1);
        }
        else    
        {
            if (isFrightened)    movementController.SetSpeed(1);
            else if (ghostNodeStates == GhostNodeStatesEnum.respwaning)     movementController.SetSpeed(7);
            else        movementController.SetSpeed(2);
        }

        if (testRespawn == true)
        {
            ghostNodeStates = GhostNodeStatesEnum.respwaning;
            testRespawn = false;
            readyToLeaveHome = false;
        }
        
    }

    public void SetFrightened(bool newIsFrightened)
    {
        isFrightened = newIsFrightened;
        ReverseDirection();
    }

    public void ReachedCenterOfTheNode (NodeController nodeController)
    {
        if (ghostNodeStates == GhostNodeStatesEnum.movingNodes)
        {
            leftHomeBefore = true;
            //scatter mode coding
            if (gameManager.currentGhostMode == GameManager.GhostMode.scatter)
            {
                //when we reace the scatter node chnage the index by adding one
                DetermineGhostScatterModeDirection();
                
            }
            //if ther are frightened
            else if (isFrightened)
            {
                string direction = GetRandomDirection();
                movementController.SetDirection(direction);
            }
            //Chase mode coding
            else
            {
                if (ghostType == GhostType.red)    DetermineRedGhostDirection();
                else if (ghostType == GhostType.pink)    DeterminePinkGhostDirection();
                else if  (ghostType == GhostType.blue)    DetermineBlueGhostDirection();
                else if ( ghostType == GhostType.orange)    DetermineOrangeFhostDirection();
            }

            //Dertermine next game node to go to
        }
        else if (ghostNodeStates == GhostNodeStatesEnum.respwaning)
        {
            string direction ="";
            //determine where we currently are -> reached the start node, move to centernode
            if (transform.position.x == ghostNodeStart.transform.position.x && transform.position.y == ghostNodeStart.transform.position.y) 
            {
                direction = "down";
            }
            //we are at the center node -> finish respawn or move to left/right
            else if (transform.position.x == ghostNodeCenter.transform.position.x && transform.position.y == ghostNodeCenter.transform.position.y)
            {
                if (repawnState == GhostNodeStatesEnum.centerNode)
                {
                    ghostNodeStates = repawnState;
                }
                else if (repawnState == GhostNodeStatesEnum.leftNode)
                {
                    direction = "left";
                }
                else if (repawnState ==GhostNodeStatesEnum.rightNode)
                {
                    direction = "right";
                }
            }
            //respawn state is left or right node
            else if (
                (transform.position.x == ghostNodeLeft.transform.position.x && transform.position.y == ghostNodeLeft.transform.position.y)
                || (transform.position.x == ghostNodeRight.transform.position.x && transform.position.y == ghostNodeRight.transform.position.y)
                )
            {
                ghostNodeStates = repawnState;
            }
            //still  in gameboard
            else{
                 //determine quickest direction to home
                direction =  GetClosestDirection(ghostNodeStart.transform.position);
            }
            movementController.SetDirection(direction);
        }
        else
        {
            //if we are ready to leave home
            if (readyToLeaveHome)
            {
                //left or right node -> center node
                if (ghostNodeStates == GhostNodeStatesEnum.leftNode) 
                {
                    ghostNodeStates = GhostNodeStatesEnum.centerNode;
                    movementController.SetDirection("right");
                }
                else if(ghostNodeStates == GhostNodeStatesEnum.rightNode)
                {
                    ghostNodeStates = GhostNodeStatesEnum.centerNode;
                    movementController.SetDirection("left");
                }
                //center Node -> start node
                else if (ghostNodeStates == GhostNodeStatesEnum.centerNode)
                {
                    ghostNodeStates = GhostNodeStatesEnum.startNode;
                    movementController.SetDirection("up");
                }
                //start node -> start moving in the game nodes
                else if (ghostNodeStates == GhostNodeStatesEnum.startNode)
                {
                    ghostNodeStates = GhostNodeStatesEnum.movingNodes;
                    movementController.SetDirection("left");
                }
            }
        }
    }

    void DetermineRedGhostDirection()
    {
        string direction = GetClosestDirection(gameManager.pacman.transform.position);
        movementController.SetDirection(direction);
    }

    void DeterminePinkGhostDirection()
    {
        string pacmanDirection = gameManager.pacman.GetComponent<MovementController>().lastMovingDirection;
        float distanceBetweenNodes = 0.35f;

        Vector2 target = gameManager.pacman.transform.position;
        if (pacmanDirection == "left") target.x -= distanceBetweenNodes*2;
        else if (pacmanDirection == "right") target.x += distanceBetweenNodes*2;
        else if (pacmanDirection == "up") target.y += distanceBetweenNodes*2;
        else if (pacmanDirection == "down") target.y -= distanceBetweenNodes*2;

        string direction = GetClosestDirection(target);
        movementController.SetDirection(direction);
        
    }

    void DetermineBlueGhostDirection()
    {
        string pacmanDirection = gameManager.pacman.GetComponent<MovementController>().lastMovingDirection;
        float distanceBetweenNodes = 0.35f;

        Vector2 target = gameManager.pacman.transform.position;
        if (pacmanDirection == "left") target.x -= distanceBetweenNodes*2;
        else if (pacmanDirection == "right") target.x += distanceBetweenNodes*2;
        else if (pacmanDirection == "up") target.y += distanceBetweenNodes*2;
        else if (pacmanDirection == "down") target.y -= distanceBetweenNodes*2;

        GameObject redGhost = gameManager.redGhost;
        float xDistance = target.x - redGhost.transform.position.x;
        float yDistance = target.y - redGhost.transform.position.y;

        Vector2 blueTarget = new Vector2(target.x + xDistance, target.y + yDistance);
        string direction = GetClosestDirection(blueTarget);
        movementController.SetDirection(direction);
    }

    void DetermineOrangeFhostDirection()
    {
        float distance = Vector2.Distance(gameManager.pacman.transform.position, transform.position);
        float distanceBetweenNodes = 0.35f;

        if (distance < 0)   distance *= -1;

        //if we near 8 nodes -> chase pacman
        if (distance <= distanceBetweenNodes*8) DetermineRedGhostDirection();
        //otherwise go to scatter mode
        else 
        {
            DetermineGhostScatterModeDirection();
        }     
    }

    string GetRandomDirection()
    {
        List<string>possibleDirection = new List<string>();
        NodeController nodeController = movementController.currentNode.GetComponent<NodeController>();

        if (nodeController.canMoveDown && movementController.lastMovingDirection != "up")   possibleDirection.Add("down");
        if (nodeController.canMoveUp && movementController.lastMovingDirection != "down")   possibleDirection.Add("up");
        if (nodeController.canMoveLeft && movementController.lastMovingDirection != "right")   possibleDirection.Add("left");
        if (nodeController.canMoveRight && movementController.lastMovingDirection != "left")   possibleDirection.Add("right");

        //Added Myself -> to solve the bug maing the game stop when ghost move towards any of the two Warp Nodes
        if (nodeController.isWarpLeftNode && movementController.canWarp)
            {
                currentNode = gameManager.rightWarpNode;
                possibleDirection.Add("left");
                transform.position = currentNode.transform.position;
                movementController.canWarp = false;
            }
        else if (nodeController.isWarpRightNode && movementController.canWarp)
            {
                currentNode = gameManager.leftWarpNode;
                possibleDirection.Add("right");
                transform.position = currentNode.transform.position;
                movementController.canWarp = false;
            };

        string direction = "";
        int randomDirectionIndex = Random.Range(0, possibleDirection.Count - 1);
        direction = possibleDirection[randomDirectionIndex];
        return direction;
        
    }

    void DetermineGhostScatterModeDirection()
    {
        if (transform.position.x == scatterNodes[scatterNodeIndex].transform.position.x && transform.position.y == scatterNodes[scatterNodeIndex].transform.position.y)
        {
            scatterNodeIndex++;
                    // and if the value is greater then lenght of scatterNode array then make it back to 0
                    if (scatterNodeIndex == scatterNodes.Length - 1)
                    {
                        scatterNodeIndex = 0;
                    }
        }       
                string direction = GetClosestDirection(scatterNodes[scatterNodeIndex].transform.position);
                movementController.SetDirection(direction);
}

    string GetClosestDirection(Vector2 target)
    {
        float shortestDistance = 0;
        string lastMovingDirection = movementController.lastMovingDirection;
        string newDirection = "";
        NodeController nodeController = movementController.currentNode.GetComponent<NodeController>();

        //If we can move up and we aren't reversing
        if (nodeController.canMoveUp && lastMovingDirection != "down")
        {
            //Get the node above us
            GameObject nodeup = nodeController.nodeUp;
            //get distance btw topnode and pacman
            float distance = Vector2.Distance(nodeup.transform.position, target);

            //if this is the shortest distance, set out direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "up";
            }
        }

        if (nodeController.canMoveDown && lastMovingDirection != "up")
        {
            //Get the node above us
            GameObject nodeDown = nodeController.nodeDown;
            //get distance btw topnode and pacman
            float distance = Vector2.Distance(nodeDown.transform.position, target);

            //if this is the shortest distance, set out direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "down";
            }
        }

        if (nodeController.canMoveRight && lastMovingDirection != "left")
        {
            //Get the node above us
            GameObject nodeRight = nodeController.nodeRight;
            //get distance btw topnode and pacman
            float distance = Vector2.Distance(nodeRight.transform.position, target);

            //if this is the shortest distance, set out direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "right";
            }
        }

        if (nodeController.canMoveLeft && lastMovingDirection != "right")
        {
            //Get the node above us
            GameObject nodeLeft = nodeController.nodeLeft;
            //get distance btw topnode and pacman
            float distance = Vector2.Distance(nodeLeft.transform.position, target);

            //if this is the shortest distance, set out direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "left";
            }
        }
        return newDirection;
    }



    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && ghostNodeStates != GhostNodeStatesEnum.respwaning)
        {
            //we eat ghost
            if (isFrightened)
            {
                gameManager.GhostEaten();
                ghostNodeStates =  GhostNodeStatesEnum.respwaning;
                
            }
            //ghost eat us
            else
            {
                StartCoroutine(gameManager.PlayerEaten());
            }
        }
    }

     void ReverseDirection()
{
    // Get the current direction of the ghost
    string redCurrentDirection = movementController.direction;

    string oppositeDirection = "";

    switch (redCurrentDirection)
    {
        case "up":
            oppositeDirection = "down";
            break;
        case "down":
            oppositeDirection = "up";
            break;
        case "left":
            oppositeDirection = "right";
            break;
        case "right":
            oppositeDirection = "left";
            break;
    }
    Debug.Log(oppositeDirection);

    // Set the opposite direction as the new direction for the ghost
    movementController.SetDirection(oppositeDirection);
    movementController.lastMovingDirection = oppositeDirection;
}

    public void SetVisible (bool newIsVisible)
    {
        isVisible = newIsVisible;
    }
}