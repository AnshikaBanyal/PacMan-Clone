using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    MovementController movementController;
    GameManager gameManager;

    public Animator animator;
    public SpriteRenderer sprite;

    public GameObject StartNode;
    public Vector2 startPos;

    public bool isDead = false;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        movementController = GetComponent<MovementController>();

        startPos = new Vector2(-0.06f, -2.4f);
        movementController.lastMovingDirection = "left";
        StartNode = movementController.currentNode;

        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }
    public void SetUp()
    {
        isDead = false;

        animator.SetBool("dead", false);
        animator.SetBool("moving", false);

        movementController.currentNode = StartNode;
        movementController.direction = "left";
        movementController.lastMovingDirection = "left";
        transform.position = startPos;
        sprite.flipX = false;
        animator.speed = 1;

    }

    public void Stop()
    {
        animator.speed = 0;
    }

    public void Death()
    {
        animator.speed = 1;
        animator.SetBool("moving", false);
        animator.SetBool("dead", true);
        isDead = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.gameIsRunning)     
        {
            if (!isDead)        animator.speed = 0;
            return;
        }

        animator.speed = 1;
        animator.SetBool("moving", true);
        
        if (Input.GetKey(KeyCode.LeftArrow)) movementController.SetDirection("left");
        if (Input.GetKey(KeyCode.RightArrow)) movementController.SetDirection("right"); 
        if (Input.GetKey(KeyCode.UpArrow)) movementController.SetDirection("up");
        if (Input.GetKey(KeyCode.DownArrow)) movementController.SetDirection("down");

        bool flipX = false;
        bool flipY = false;

        if (movementController.lastMovingDirection == "left") 
        {
            animator.SetInteger("direction", 0);
        }
        else if (movementController.lastMovingDirection=="right")
        {
            animator.SetInteger("direction", 0);
            flipX = true;
        }
        else if (movementController.lastMovingDirection=="up")
        {
            animator.SetInteger("direction", 1);
        }
        else if (movementController.lastMovingDirection=="down")
        {
            animator.SetInteger("direction", 1);
            flipY = true;
        }
        sprite.flipY = flipY;
        sprite.flipX = flipX;
    }
}
