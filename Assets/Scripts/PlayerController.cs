using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D body;
    private BoxCollider2D collider;

    [SerializeField] private LayerMask jumpableGround;


    private float moveSpeed = 7f;
    public float jumpTakeOffSpeed = 7;
    public float jumpModifier = 1.5f;
    public float jumpDeceleration = 0.5f;

    public JumpState jumpState = JumpState.Grounded;
    public bool stopJump;

    bool jump;
    public Vector2 move;
    SpriteRenderer spriteRenderer;

    public Vector2 velocity;
    // public bool IsGrounded { get; private set; }
    
    bool isDashing = false;
    public float dashDistance = 100f;
    float lastActionTime;
    public Direction actionDirection;
    Direction lastActionDirection;
    
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate() {
      // IsGrounded = body.position.y <= 0;
    }

    // Update is called once per frame
    void Update()
    {
      // targetVelocity = Vector2.zero;
      ComputeVelocity();

      ComputeDash();
    }

    void ComputeVelocity()
    {
        if (jump && IsGrounded())
        {
          velocity.y = jumpTakeOffSpeed * jumpModifier;
          jump = false;
        }
        else if (stopJump)
        {
          stopJump = false;
          if (velocity.y > 0)
          {
            velocity.y = velocity.y * jumpDeceleration;
          }
        }

        if (move.x > 0.01f)
          spriteRenderer.flipX = false;
        else if (move.x < -0.01f)
          spriteRenderer.flipX = true;

        body.velocity = new Vector2(move.x * moveSpeed, body.velocity.y);
    }

    public void Jump() {
      // body.velocity = new Vector2(0, 7);
      body.AddForce(Vector2.up * 10f, ForceMode2D.Impulse);
    }

    private void ComputeDash() {
      Debug.Log(string.Format("Time: {0} - doubleActionTime: {1} = {2}", Time.time, lastActionTime, lastActionTime - Time.time));

      if (actionDirection == Direction.Left || actionDirection == Direction.Right) {
        bool isDoubleActionFastEnough = Time.time - lastActionTime < 0.2f;
        bool isDoubleAction = lastActionDirection == actionDirection;

        if (isDoubleAction && isDoubleActionFastEnough) {
          float directionMultiplier = actionDirection == Direction.Left ? -1 : 1;
          Debug.Log(string.Format("Starting Dash"));
          StartCoroutine(Dash(directionMultiplier));
        }

        lastActionDirection = actionDirection;
        lastActionTime = Time.time;
      }
    }

    public IEnumerator Dash(float direction) {
      isDashing = true;
      body.velocity = new Vector2(body.velocity.x, 0f);
      body.AddForce(new Vector2(dashDistance * direction, 0f), ForceMode2D.Impulse);
      // float gravity = body.gravityScale;
      // body.gravityScale = 0f;
      yield return new WaitForSeconds(0.4f);
      isDashing = false;
      // body.gravityScale = gravity;
    }
    public bool IsGrounded() {
      return Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.down, 0.1f, jumpableGround);
    }

    public enum JumpState
    {
        Grounded,
        PrepareToJump,
        Jumping,
        InFlight,
        Landed
    }

    public enum Direction {
      Left,
      Right,
      Up,
      Down,
      Stationary
    }
}
