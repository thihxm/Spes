using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody2D body;

    public Vector2 move;
    
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Jump() {
      // body.velocity = new Vector2(0, 7);
      body.AddForce(Vector2.up * 7, ForceMode2D.Impulse);
    }

    public bool IsGrounded() {
      return body.position.y <=0;
    }
}
