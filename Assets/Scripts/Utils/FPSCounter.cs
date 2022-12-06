using UnityEngine;
using TMPro;

namespace Utils
{
  public class FPSCounter : MonoBehaviour
  {
    // [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float hudRefreshRate = 1f;

    private float timer;

    private void Awake()
    {
      Application.targetFrameRate = 120;
      QualitySettings.vSyncCount = 0;
    }

    private void Update()
    {
      if (Time.unscaledTime > timer)
      {
        int fps = (int)(1f / Time.unscaledDeltaTime);
        // fpsText.text = "FPS: " + fps;
        timer = Time.unscaledTime + hudRefreshRate;
      }
    }
  }
}