using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class NpcDialogController : MonoBehaviour
{
    public bool showText = false;
    public TextMeshProUGUI dialogBox;

    public CamTrigger camTrigger;
    
    public string dialogText = "José Carlos > Faker";

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerInTalk();

        if(showText)
        {
            dialogBox.text = dialogText;
            dialogBox.enabled = true;
        } 
        else dialogBox.enabled = false;
       
    }

    void playerInTalk()
    {
        showText = camTrigger.focus;
    }
}
