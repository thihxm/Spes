using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
  [SerializeField] private float speed = 7f;
  private Rigidbody2D body;

  void Start()
  {
    // body = GetComponent<Rigidbody2D>();
  }

  public IEnumerator Shoot(int direction)
  {
    body = GetComponent<Rigidbody2D>();
    switch (direction)
    {
      case 0: {
        body.velocity = transform.right * -1 * speed;
        break;
      }
      case 1: {
        body.velocity = transform.right * speed;
        break;
      }
      case 2: {
        body.velocity = transform.up * speed;
        break;
      }
      case 3: {
        body.velocity = transform.up * -1 * speed;
        break;
      }
    }

    yield return new WaitForSeconds(1);
    if (this != null) {
      Destroy(gameObject);
    }
  }

  void OnTriggerEnter2D(Collider2D other)
  {
    if (other.name == "Box") {
      other.attachedRigidbody.AddForce(body.velocity, ForceMode2D.Impulse);
    }
    if (this != null) {
      Destroy(gameObject);
    }
  }
}
