

using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class LevelsMenu : MonoBehaviour
{
    public static LevelsMenu Instance { get; private set; }
    public Button[] buttons;
    public GameObject[] girls;
    public GameObject complateLevel;

    private void Start()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if(unlockedLevel <= 1)
        {
            complateLevel.SetActive(false);
        }
    }
    private void Awake()
    {
        Instance = this;
        //PlayerPrefs.SetInt("UnlockedLevel", 1);

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 0; i < girls.Length; i++)
        {
            girls[i].SetActive(false);
        }

        if (unlockedLevel == 2)
        {
            girls[1].SetActive(true);
        }

        else
        {
            girls[0].SetActive(true);
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }
        for (int i = 0; i < unlockedLevel; i++)
        {
            buttons[i].interactable = true;
        }

        //Debug.Log(unlockedLevel);
    }
    public void OpenLevel()
    {
       // string levelName = "Level" + levelId;
      // SceneManager.LoadScene(levelName);

        complateLevel.SetActive(true);
    }
}

