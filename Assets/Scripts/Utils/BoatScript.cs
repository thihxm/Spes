using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BoatScript : MonoBehaviour
{
    public Rigidbody2D rb;

    private void Start() {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Wind") {
            Debug.Log("Ventou");
            rb.AddForce(new Vector2 (-1000f, 1000f));
        }
    }
}
