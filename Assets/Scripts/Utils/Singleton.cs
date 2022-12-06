using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
  private static T _instance;
  public static T Instance {
    get {
      if (_instance == null) {
        _instance = FindObjectOfType<T>();
        if (_instance == null) {
          GameObject obj = new GameObject();
          obj.name = typeof(T).Name;
          obj.hideFlags = HideFlags.HideAndDontSave;
          _instance = obj.AddComponent<T>();
        }
      }
      return _instance;
    }
  }

  private void OnDestroy() {
    if (_instance == this) {
      _instance = null;
    }
  }
}
