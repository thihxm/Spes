using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLife : MonoBehaviour
{
  private Rigidbody2D body;
  private PlayerController playerController;

  public Vector2 lastGroundPosition;

  private GameObject collisionObject;

  // Start is called before the first frame update
  void Start()
  {
    body = GetComponent<Rigidbody2D>();
    playerController = GetComponent<PlayerController>();
  }

  // Update is called once per frame
  void Update()
  {
    
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    collisionObject = collision.gameObject;

    bool isGrounded = playerController.IsGrounded();
    if (isGrounded && !collisionObject.CompareTag("Trap")) {
      lastGroundPosition = transform.position;
    }

    if (collision.gameObject.CompareTag("Trap"))
    {
      Die();
    }
  }



  private void Die() {
    body.bodyType = RigidbodyType2D.Static;
    StartCoroutine(Restart());
  }

  private void RestartLevel() {
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }

  private void Respawn() {
    body.bodyType = RigidbodyType2D.Dynamic;
    body.velocity = Vector2.zero;
    body.angularVelocity = 0;
    transform.position = lastGroundPosition;
  }

  public IEnumerator Restart() {
    yield return new WaitForSeconds(0.4f);
    Respawn();
  }
}
