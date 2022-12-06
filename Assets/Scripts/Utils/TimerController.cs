using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class TimerController : MonoBehaviour
{
  public List<goalSpeedRemote> speedGoals = new List<goalSpeedRemote>();
  Dictionary<string, string> compleatedGoals = new Dictionary<string, string>(); 
  public TextMeshProUGUI startText;
  public TextMeshProUGUI goalsText;

  private bool allCompleated = false;

  public float elapsedTime = 0f;
  public bool isEnabled = true;

  void Update()
  {
    if (isEnabled)
    {
      elapsedTime += Time.deltaTime;
      FormatTime(elapsedTime);
    }
    showGoals();
    checkGoals();
  }

  String FormatTime(float timeToDisplay)
  {
    float milliseconds = (timeToDisplay * 1000);
    var time = TimeSpan.FromMilliseconds(milliseconds);
    startText.text = time.ToString("mm':'ss'.'ff");
    return time.ToString("mm':'ss'.'ff");
  }

  void showGoals() {

    foreach (goalSpeedRemote item in speedGoals)
    {
      if (item.collided && !compleatedGoals.ContainsKey(item.name)) {

        compleatedGoals.Add(item.name,FormatTime(elapsedTime).ToString());
        Debug.Log("adicionado");
        goalsText.text = goalsText.text + Environment.NewLine  + item.name + " : " + compleatedGoals[item.name];
  
      }
    }
  }

  void checkGoals()
  {
    if (speedGoals.Count == compleatedGoals.Count) isEnabled = false;
  }
  
}
