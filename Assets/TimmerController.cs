using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class TimmerController : MonoBehaviour
{
    public TextMeshProUGUI startText;

    public float mili = 0f;
    public float sec = 00f;
    public float min = 00f;
    public float hour = 00f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        mili += Time.deltaTime;

        if (mili > 0.9999) {
            sec += 1;            
            mili = 0;
        }
        if (sec > 59) {
            min += 1;            
            sec = 0;
        }

        if (min > 59) {
            hour += 1;            
            min = 0;
        }

        startText.text = (string.Format("{0}:{1}:{2}.{3}",
            hour.ToString("00"),
            min.ToString("00"),
            sec.ToString("00"),
            ((mili * 1000) % 100).ToString("00")));
    }

}
