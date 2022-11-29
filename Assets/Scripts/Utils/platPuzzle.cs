using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platPuzzle : MonoBehaviour
{
    [SerializeField] GameObject plat;

    private bool isUping = false;

    private float upLimit = 0.3f;
    private float timer = 1;

    [SerializeField] private Vector3 finalPosition;
    [SerializeField] private Vector3 initialPosition;
    private Vector3 velocity = new Vector3(0,0.1f,0);
    [SerializeField] private float closingSmoothTime = 30f;

    void Start()
    {
        initialPosition = plat.transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Wind") {
            Debug.Log("ventou");
            timer = 0;
        }  
    }

    // Update is called once per frame
    void Update()
    {   
        if (timer < upLimit) {
            timer += Time.deltaTime;
            isUping = true;
        } else {
            isUping = false;
        }

        if (isUping) {
            goUp();
        } else {
            if (plat.transform.position.y > initialPosition.y) goDown();
        }
        
    }

    void goUp() {
        var x = plat.transform.position.x;
        var y = plat.transform.position.y + 10;
        var z = plat.transform.position.z;

        var finalPosition = new Vector3(x,y,z);
        plat.transform.position = Vector3.SmoothDamp(plat.transform.position, finalPosition, ref velocity, closingSmoothTime);
    }

    void goDown() {
        // plat.transform.position = Vector3.SmoothDamp(plat.transform.position, initialPosition, ref velocity, closingSmoothTime);
        plat.transform.position = Vector3.MoveTowards(plat.transform.position, initialPosition, 3 * Time.deltaTime);
    }
}
