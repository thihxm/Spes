using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Particles
{
  [RequireComponent(typeof(ParticleSystem))]
  public class HelixParticle : MonoBehaviour
  {
    private ParticleSystem ps;
    public int frequency = 1; // Repeat rate
    public float resolution = 20f; // Amount of keys on the created curve
    public float amplitude = 1f; // Min/max height of the curve
    public float zValue = 0f; // For spreading the curve along the Z axis

    void Start()
    {
      ps = GetComponent<ParticleSystem>();
      CreateCircle();
    }

    void CreateCircle()
    {
      var velocityOverLifetime = ps.velocityOverLifetime;
      velocityOverLifetime.enabled = true;
      velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
      var particleMain = ps.main;
      particleMain.startSpeed = 0f;

      AnimationCurve curveX = new AnimationCurve(); // Create a new curve
      for (int i = 0; i < resolution; i++)
      {
        float time = i / (resolution - 1);
        float value = amplitude * Mathf.Sin(time * Mathf.PI * 2 * frequency);
        curveX.AddKey(time, value);
      }

      AnimationCurve curveY = new AnimationCurve(); // Create a new curve
      for (int i = 0; i < resolution; i++)
      {
        float time = i / (resolution - 1);
        float value = amplitude * Mathf.Cos(time * Mathf.PI * 2 * frequency);
        curveY.AddKey(time, value);
      }

      var theCurve = new ParticleSystem.MinMaxCurve(10f, curveX);

      velocityOverLifetime.x = theCurve;
      velocityOverLifetime.y = theCurve;
      velocityOverLifetime.z = theCurve;
    }
  }
}
