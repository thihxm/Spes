using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
  public class Wind : MonoBehaviour
  {
    [SerializeField] private float speed = 7f;
    private float timeWhenShot;
    private float shootLength = 1f;
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
      if (this != null && !other.CompareTag("WindNoCollision"))
      {
        Destroy(gameObject);
      }
    }
  }
}
