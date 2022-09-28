using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
public class PlataformOneWay : MonoBehaviour
{
    private InputManager inputManager;
    public bool shouldPass = false;
    public bool inputDown = false;
    public bool inputTap = false;

    //Enum that sets up different types for one way platforms
    public enum OneWayPlatforms { GoingUp, GoingDown, Both }
    public OneWayPlatforms type = OneWayPlatforms.Both;
    //A short delay to allow the player to collide with the platform again
    [SerializeField]
    private float delayDown = .4f;

    [SerializeField]
    private float delayUp = .4f;
    //The collider on the platform
    private Collider2D col;
    //Reference of the player
    private GameObject player;
    //Reference to the collider on the player
    private Collider2D playerCollider;
    private PlayerController playerController;

    private bool goingUp = false;

    void Awake() {
        inputManager = InputManager.Instance;
    }

    private void Start()
    {
        //Grabs a reference of the current collider on the platform
        col = GetComponent<Collider2D>();
        //Less optimal way to find Player
        player = GameObject.FindGameObjectWithTag("Player");
        //More optimal way to find Player; requires some sort of script that only the active player in the scene would have
        // player = FindObjectOfType<Player>().gameObject;
        playerCollider = player.GetComponent<Collider2D>();
        playerController = player.GetComponent<PlayerController>();
    }

    void OnEnable() {
        inputManager.OnMove += CheckInput;
        inputManager.OnJump += JumpTapped;
    }

    void OnDisable() {
        inputManager.OnMove -= CheckInput;
        inputManager.OnJump -= JumpTapped;
    }    

    void CheckInput(Vector2 inputDelta) {
        inputDown = inputDelta.y <= -0.9f;
    }

    void JumpTapped() {
        shouldPass = inputDown;
    }

    private void Update() {
        // Teto
        if (playerController.isAgainstCeiling && playerController.headObject == col) {
            Debug.Log("Should pass Ceiling");
            StartIgnoringCollision(delayUp);
            goingUp = true;
        }

        // Chão
        if (shouldPass && playerController.isGrounded && playerController.footObject == col) {
            StartIgnoringCollision(delayDown);
        } else {
            shouldPass = false;
        }
    }

    //Unity event that gets called once everytime something collides with the platform
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Checks to see if the gameobject colliding with the platform is the player
        if (goingUp && playerController.footObject == col)
        {
            //Checks to see if player is not above the platform so the player can stand on the platform while jumping and then checks to see if the platform will allow the player to jump up through it;
            Physics2D.IgnoreCollision(playerCollider, col, false);

            shouldPass = false;
            goingUp = false;
            playerController.canLedgeClimb = true;
        }
    }
    
    private void OnCollisionStay2D(Collision2D other) {
        //Checks to see if the gameobject colliding with the platform is the player
        if (goingUp && playerController.footObject == col)
        {
            //Checks to see if player is not above the platform so the player can stand on the platform while jumping and then checks to see if the platform will allow the player to jump up through it;
            Physics2D.IgnoreCollision(playerCollider, col, false);

            shouldPass = false;
            goingUp = false;
            playerController.canLedgeClimb = true;
        }
    }

    private void OnCollisionExit2D(Collision2D other) {
        shouldPass = false;
        // if (playerController.footObject == col)
        // {
        //     //Checks to see if player is not above the platform so the player can stand on the platform while jumping and then checks to see if the platform will allow the player to jump up through it;
        //     Physics2D.IgnoreCollision(playerCollider, col, false);
        // }
    }

    private void StartIgnoringCollision(float delay) {
        // Debug.Log(col.name + " started ignoring collision!");
        //Sets the player as a gameobject that should ignore the platform collider so the player can pass through
        Physics2D.IgnoreCollision(playerCollider, col, true);
        //Runs coroutine to allow the player to collide with the platform again
        StartCoroutine(StopIgnoring(delay));
        playerController.canLedgeClimb = false;
    }

    //Coroutine that toggles the collider on the platform to allow the player to collide with it again
    private IEnumerator StopIgnoring(float delay)
    {
        //Waits a short delay setup at the top of this script in the variables
        yield return new WaitForSeconds(delay);

        //Sets the player as a gameobject that should collide the platform collider so the player can stand on it again
        Physics2D.IgnoreCollision(playerCollider, col, false);

        shouldPass = false;
        playerController.canLedgeClimb = true;
        // Debug.Log(col.name + " is checking collision again!");
    }

    void OnDrawGizmos() {
        float y = shouldPass ? -1f : 1f; 
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position + new Vector3(0f, y), new Vector3(0.25f, 2f));
    }
}

