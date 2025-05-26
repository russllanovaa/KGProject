using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class NextLevel : MonoBehaviour
{
    private static NextLevel _instance;

    public static NextLevel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<NextLevel>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(NextLevel).Name);
                    _instance = singletonObject.AddComponent<NextLevel>();
                }
            }
            return _instance;
        }
    }

    [SerializeField] private int _nextSceneBuildIndex;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public void UnlockNewLevel()
    {
            PlayerPrefs.SetInt("ReachedIndex", SceneManager.GetActiveScene().buildIndex + 1);
            PlayerPrefs.SetInt("UnlockedLevel", PlayerPrefs.GetInt("UnlockedLevel", 1) + 1);
            PlayerPrefs.Save();
        
    }

    // Ось цей метод додає перемикання на сцену
    public void LoadNextLevel()
    {
        if (_nextSceneBuildIndex >= 0 && _nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(_nextSceneBuildIndex);
        }
        else
        {
            Debug.LogWarning("Індекс сцени некоректний або сцена не додана до Build Settings.");
        }
    }
}
