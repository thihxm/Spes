using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
  [SerializeField] private Transform player;
  [SerializeField] private float cameraSpeed = 0.1f;

  private Vector3 m_refPos;
  // Start is called before the first frame update
  void Start()
  {
      
  }

  // Update is called once per frame
  void Update()
  {
    m_refPos *= Time.smoothDeltaTime;
    Vector3 newPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
    transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref m_refPos, cameraSpeed);
  }
}
