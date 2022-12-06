using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Particles
{
  public class HelixParticleTrail : MonoBehaviour
  {
    [SerializeField] Transform trail;
    [SerializeField] float amplitude = 0.1f;
    [SerializeField] float rotationSpeed = 100f;

    void Start()
    {

    }

    void Update()
    {
      trail.transform.RotateAround(transform.position, transform.up, rotationSpeed);
      // trail.position = Vector3.Slerp(trail.transform.position, transform.position, 0.1f * Time.deltaTime);

      transform.Translate(0, amplitude * Time.deltaTime, 0);
    }
  }
}
