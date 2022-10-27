using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class TimerController : MonoBehaviour
{
  public TextMeshProUGUI startText;

  public float elapsedTime = 0f;
  public bool isEnabled = true;

  void Update()
  {
    if (isEnabled)
    {
      elapsedTime += Time.deltaTime;
      FormatTime(elapsedTime);
    }
  }

  void FormatTime(float timeToDisplay)
  {
    float milliseconds = (timeToDisplay * 1000);
    var time = TimeSpan.FromMilliseconds(milliseconds);
    startText.text = time.ToString("mm':'ss'.'ff");
  }

}
