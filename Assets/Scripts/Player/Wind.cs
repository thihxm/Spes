using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
  public class Wind : MonoBehaviour
  {
    [SerializeField] private float speed = 7f;
    private float timeWhenShot;
    private float shootLength = 0.5f;
    private Rigidbody2D body;

    void Start()
    {
      // body = GetComponent<Rigidbody2D>();
    }

    public void Shoot(int direction, float initialSpeed)
    {
      body = GetComponent<Rigidbody2D>();

      Vector3 windVelocity = new(initialSpeed, 0);

      switch (direction)
      {
        case 0:
          {
            windVelocity += transform.right * -1 * speed;
            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1;
            transform.localScale = currentScale;
            break;
          }
        case 1:
          {
            windVelocity += transform.right * speed;
            break;
          }
        case 2:
          {
            windVelocity = transform.up * speed;
            transform.Rotate(0, 0, 90);
            break;
          }
        case 3:
          {
            windVelocity = transform.up * -1 * speed;
            break;
          }
      }
      body.velocity = windVelocity;
      timeWhenShot = Time.time;
    }

    void Update()
    {
      if (Time.time >= timeWhenShot + shootLength && this != null)
      {
        Destroy(gameObject);
      }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
      if (other.CompareTag("Wind"))
      {
        other.attachedRigidbody.AddForce(body.velocity, ForceMode2D.Impulse);
      }

      if (
        this != null &&
        !other.CompareTag("WindNoCollision") &&
        !other.CompareTag("Player")
      )
      {
        Destroy(gameObject);
      }
    }
  }
}
