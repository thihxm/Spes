using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour
{
    Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Wind")
        {
            Debug.Log("Ventou");
            rb.AddForce(new Vector2(-3000,2500));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
