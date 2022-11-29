using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;
public class NpcDialogController : MonoBehaviour
{
    public bool showText = false;
    public TextMeshProUGUI dialogTextBox;
    public Image dialogBox;
    public GameObject notification;

    public CinemachineVirtualCamera cam;

    [SerializeField] public string dialogText = "A tempos antigos, muito antes você ou mesmo de mim todos os ventos eram um só...";

    public float timer = 0.0f;

    void Start()
    {
        notification.GetComponent<Renderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (showText){
            
            timer += Time.deltaTime;
            if (timer >= 1.5) {
                cam.Priority = 50;
                activateText();
            } 
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.tag == "Player") {
            timer = 0.0f;
            showText = false;
            notification.GetComponent<Renderer>().enabled = false;
            cam.Priority = 1;
            deactivateText();
        }
    }



    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            showText = true;
            notification.GetComponent<Renderer>().enabled = true;
        } 
    } 


    private void activateText () {
        dialogTextBox.text = dialogText;
        dialogTextBox.enabled = true;

        dialogBox.enabled = true;
    } 

    private void deactivateText () {

        dialogTextBox.enabled = false; 
        dialogBox.enabled = false;
    }
}
