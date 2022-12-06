using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

[RequireComponent(typeof(EdgeCollider2D))]
public class PlataformOneWay : MonoBehaviour
{
  private InputManager inputManager;
  public bool shouldPass = false;
  public bool inputTap = false;

  public enum OneWayPlatforms { GoingUp, GoingDown, Both }
  public OneWayPlatforms type = OneWayPlatforms.Both;

  [SerializeField]
  private float delayDown = .4f;

  [SerializeField]
  private float delayUp = .4f;

  private Collider2D col;

  private Collider2D playerCollider;
  
  private PlayerController playerController;

  private bool goingUp = false;

  void Awake()
  {
    inputManager = InputManager.Instance;
  }

  private void Start()
  {
    col = GetComponent<Collider2D>();
    // player = GameObject.FindGameObjectWithTag("Player");
   // player = FindObjectOfType<Player>().gameObject;
    playerController = PlayerController.Instance;
    playerCollider = playerController.BodyCollider;
  }

  void OnEnable()
  {
    inputManager.OnThrowWind += CheckActionDirection;
  }

  void OnDisable()
  {
    inputManager.OnThrowWind -= CheckActionDirection;
  }

  void CheckActionDirection(Vector2 swipeDelta)
  {
    if (swipeDelta == Vector2.zero) return;

    shouldPass = swipeDelta == Vector2.down;

    if (shouldPass) {
      StartIgnoringCollision(3f);
      Debug.Log("Desce");
    } 
  }

  private void Update()
  {


    // Teto
    // if (playerController.isAgainstCeiling && playerController.headObject == col) {
    //     StartIgnoringCollision(delayUp);
    //     goingUp = true;
    // }

    // // Chão
    // if (shouldPass && playerController.isGrounded && playerController.footObject == col) {
    //     Debug.Log("Passou");
    //     StartIgnoringCollision(delayDown);
    // } else {
    //     shouldPass = false;
    // }
  }

  //Unity event that gets called once everytime something collides with the platform
  private void OnCollisionEnter2D(Collision2D collision)
  {
    //Checks to see if the gameobject colliding with the platform is the player
    // if (goingUp && playerController.footObject == col)
    // {
    //     //Checks to see if player is not above the platform so the player can stand on the platform while jumping and then checks to see if the platform will allow the player to jump up through it;
    //     Physics2D.IgnoreCollision(playerCollider, col, false);

    //     shouldPass = false;
    //     goingUp = false;
    //     playerController.canLedgeClimb = true;
    //     playerController.isGrounded = true;
    // } else if (playerController.footObject == col) {
    //     playerController.canGroundCheck = true;
    // }
  }

  private void OnCollisionStay2D(Collision2D other)
  {
    //Checks to see if the gameobject colliding with the platform is the player
    // if (goingUp && playerController.footObject == col)
    // {
    //     Physics2D.IgnoreCollision(playerCollider, col, false);

    //     shouldPass = false;
    //     goingUp = false;
    //     playerController.canGroundCheck = true;
    //     playerController.isGrounded = true;
    // }
  }

  private void OnCollisionExit2D(Collision2D other)
  {
    shouldPass = false;
  }

  private void StartIgnoringCollision(float delay)
  {
    Physics2D.IgnoreCollision(playerCollider, col, true);
    playerController.ToggleCollision();
    Debug.Log(col.name + " ignoring collision!");
    StartCoroutine(StopIgnoring(delay));
    // playerController.canLedgeClimb = false;
    // playerController.canGroundCheck = false;
    // playerController.isGrounded = false;
  }

  //Coroutine that toggles the collider on the platform to allow the player to collide with it again
  private IEnumerator StopIgnoring(float delay)
  {
    yield return new WaitForSeconds(delay);
    Debug.Log(col.name + " stopped ignoring collision!");

    Physics2D.IgnoreCollision(playerCollider, col, false);
    playerController.ToggleCollision();
    shouldPass = false;
    // goingUp = false;
    // playerController.canGroundCheck = true;
    // playerController.canLedgeClimb = true;
    // Debug.Log(col.name + " is checking collision again!");
  }

  void OnDrawGizmos()
  {
    float y = shouldPass ? -1f : 1f;
    Gizmos.color = Color.green;
    Gizmos.DrawCube(transform.position + new Vector3(0f, y), new Vector3(0.25f, 2f));
  }
}

