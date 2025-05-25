using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {

          Instance = this;
    }


    public void LoadSceneByIndex(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void ChoosingLevel(int level)
    {
        PlayerData.Instance.SetCurrentLevel(level);
    }
}


public class PlayerData : MonoBehaviour
{
    private static PlayerData instance;

    private int currentLevel;
    private int availableLevel;
    private int availableInfo;
    private int stepOfLevel = 1;
    public static PlayerData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PlayerData>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("PlayerData");
                    instance = obj.AddComponent<PlayerData>();
                }

                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }

    private PlayerData() { }

    public void SetCurrentLevel(int level)
    {
        currentLevel = level;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public void SetAvailableLevel(int level)
    {
        availableLevel = level;
    }

    public int GetAvailableLevel()
    {
        return availableLevel;
    }

    public void ChangeNumbersOfInfo(int number)
    {
        availableInfo += number;
    }

    public int GetAvailableInfo()
    {
        return availableInfo;
    }

    public int StepOfLevel()
    {
        return stepOfLevel;
    }

    public void AddStepOfLevel()
    {
         stepOfLevel+=1;
    }
}


