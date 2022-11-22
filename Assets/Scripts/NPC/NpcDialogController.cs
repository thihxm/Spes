using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class NpcDialogController : MonoBehaviour
{
    public bool showText = false;
    public TextMeshProUGUI dialogTextBox;
    public CamTrigger camTrigger;
    public Image dialogBox;
    public string dialogText = "José Carlos > Faker";

    public float timer = 0.0f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerInTalk();

        if(showText)
        {
            dialogTextBox.text = dialogText;
            dialogTextBox.enabled = true;

            dialogBox.enabled = true;
        } 
        else {
            dialogTextBox.enabled = false; 
            dialogBox.enabled = false;
        }
    }

    private void OnTriggerStay(Collider2D other) {
        // if (other.tag == "Player" && time) 
    }

    void playerInTalk()
    {
        showText = camTrigger.focus;
    }
}
