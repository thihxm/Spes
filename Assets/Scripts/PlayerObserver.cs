using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObserver : MonoBehaviour
{
  [SerializeField] private SplitVirtualPad splitVirtualPad;
  // Start is called before the first frame update
  void Start()
  {
    WindShooter windShooter = GetComponent<WindShooter>();
    splitVirtualPad.Attach(windShooter);
  }
}
