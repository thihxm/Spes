using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goalSpeedRemote : MonoBehaviour
{

    public bool collided = false;

    private SpriteRenderer sprite;

    private void Start() {
        sprite = GetComponent<SpriteRenderer>();
    }

        void OnTriggerEnter2D(Collider2D col)
    {

        if (col.gameObject.tag == "Player") {
            collided = true;
            sprite.color = new Color (0, 0, 0, 0); 
        }      
    }

}
