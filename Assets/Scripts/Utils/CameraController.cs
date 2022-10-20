using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
  [SerializeField] private Transform testPoint;
  [SerializeField] private CinemachineVirtualCamera ccam;
  [SerializeField] private Transform player;
  [SerializeField] private UnityEngine.Camera cam;
void Start()
{
    cam = UnityEngine.Camera.main;
}

void Update()
  {
    if (isInCamera(testPoint) && isInCamera(player))
    {
        ccam.Follow = testPoint;
    }
    else
    {
      ccam.Follow = player;
    }
  }

  bool isInCamera(Transform obj) {
    Vector3 viewPos = cam.WorldToViewportPoint(obj.position);
    return (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0);
  } 
}
