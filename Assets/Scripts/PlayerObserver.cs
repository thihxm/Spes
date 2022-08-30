using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObserver : MonoBehaviour
{
  [SerializeField] private SplitVirtualPad splitVirtualPad;
  // Start is called before the first frame update
  void Awake()
  {
    WindShooter windShooter = GetComponent<WindShooter>();
    PlayerController playerController = GetComponent<PlayerController>();
    splitVirtualPad.Attach(windShooter);
    splitVirtualPad.Attach(playerController);
  }
}
