using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamTrigger : MonoBehaviour
{

    public CinemachineVirtualCamera cam;
    public bool focus = false;

    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Bateu");
        if (col.gameObject.tag == "Player") {
            Debug.Log("ZoomIn");
            cam.Priority = 50;
            focus = true;
        }      
    }

    void OnTriggerExit2D (Collider2D col)
    {
        Debug.Log("Saiu");
        if (col.gameObject.tag == "Player") {
            Debug.Log("ZoomOut");
            cam.Priority = -50;
            focus = false;
        }     
    }
}
