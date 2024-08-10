using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{

    public GameManager gameManager;

    public GameObject currentNode;
    public float speed = 4f;
    public string direction = "";
    public string lastMovingDirection = "";

    public bool canWarp = true;

    public bool isGhost = false;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.gameIsRunning)     return;
        
        NodeController currentNodeController = currentNode.GetComponent<NodeController>();

        transform.position = Vector2.MoveTowards(transform.position, currentNode.transform.position,speed* Time.deltaTime);

        bool reverseDirection = false;
        if (
            (direction == "left" && lastMovingDirection == "right")
            || (direction == "right" && lastMovingDirection == "left")
            || (direction == "up" && lastMovingDirection == "down")
            || (direction == "down" && lastMovingDirection == "up")
        ) reverseDirection = true;

        //figure out if we are at the center of the node
        if (transform.position.x == currentNode.transform.position.x && transform.position.y == currentNode.transform.position.y || reverseDirection)
        {
            //if its the enemy
            if (isGhost)
            {
                GetComponent<EnemyController>().ReachedCenterOfTheNode(currentNodeController);
            }
            //if we are at any of the 2 warp node we get trasported to the other one (if and elseif)
            if (currentNodeController.isWarpLeftNode && canWarp)
            {
                currentNode = gameManager.rightWarpNode;
                direction = "left";
                lastMovingDirection = "left";
                transform.position = currentNode.transform.position;
                canWarp = false;
            }
            else if (currentNodeController.isWarpRightNode && canWarp)
            {
                currentNode = gameManager.leftWarpNode;
                direction = "right";
                lastMovingDirection = "right";
                transform.position = currentNode.transform.position;
                canWarp = false;
            }
            //otherwise, find next node we will be moving towards
            else
            {
                //if: we are at starting node & direction is "down" & is pacman and not the Ghost ? if ghost then not at respwaning state -> STOP
                if 
                (
                    currentNodeController.isGhostStartingNode && direction == "down" 
                    && (!isGhost || GetComponent<EnemyController>().ghostNodeStates != EnemyController.GhostNodeStatesEnum.respwaning )
                )
                {
                    direction = lastMovingDirection;
                }

                //get next node  from node folder usiing current direction
                GameObject newNode = currentNodeController.GetNodeFromDirection(direction);
                //if we can move in that direction
                if (newNode != null)
                {
                    currentNode = newNode;
                    lastMovingDirection = direction;
                } //cant move in that direction -> keep moving in old direction
                else{
                    direction = lastMovingDirection;
                    newNode = currentNodeController.GetNodeFromDirection(direction);
                    if(newNode != null)
                    {
                        currentNode = newNode;
                    }
                }
            }
        }
        //not at the center of the node
        else
        {
            canWarp = true;
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    //to get the new direction to go to
    public void SetDirection(string newDirection)
    {
        direction = newDirection;
    }
}
