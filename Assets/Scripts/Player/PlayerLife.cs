using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
  public class PlayerLife : MonoBehaviour
  {
    private Rigidbody2D body;
    private PlayerController playerController;

    public Vector2 lastGroundPosition;

    Collider2D[] objectsInsideArea;

    // Start is called before the first frame update
    void Start()
    {
      body = GetComponent<Rigidbody2D>();
      playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
      objectsInsideArea = Physics2D.OverlapCircleAll(body.position, 2f);
      // bool isGrounded = playerController.isGrounded;
      bool isTrapInsideRadius = Array.Exists(objectsInsideArea, x => x.gameObject.CompareTag("Trap"));
      // if (isGrounded && !isTrapInsideRadius)
      // {
      //   lastGroundPosition = transform.position;
      // }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
      if (collision.gameObject.CompareTag("Trap"))
      {
        Die();
      }
    }

    private void Die()
    {
      body.bodyType = RigidbodyType2D.Static;
      StartCoroutine(Restart());
    }

    private void RestartLevel()
    {
      SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Respawn()
    {
      body.bodyType = RigidbodyType2D.Dynamic;
      body.velocity = Vector2.zero;
      body.angularVelocity = 0;
      transform.position = lastGroundPosition;
    }

    public IEnumerator Restart()
    {
      yield return new WaitForSeconds(0.4f);
      Respawn();
    }
  }
}
