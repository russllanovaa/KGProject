using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // Статичне поле для зберігання єдиного екземпляра класу
    private static LevelManager _instance;

    // Публічна властивість для доступу до екземпляра
    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LevelManager>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(LevelManager).Name);
                    _instance = singletonObject.AddComponent<LevelManager>();
                }
            }
            return _instance;
        }
    }
    public LevelView view;

    private LevelData levelData;
    private string[] pages;
    private float cooldown = 60f;
    private float timer = 0f;
    private bool canUnlock = true;
    private int currentPage = 0;

    private void Start()
    {
        levelData = new LevelData();
        LoadPages();
        UpdatePage();
        view.ToggleBook(false);
        view.ToggleHint(false);
        view.ToggleEnd(false);
        view.OpenHint(false);
        unlockTheory(levelData.CurrentLevel);
    }
    private void Awake() // Додаємо Awake для логіки сінглтона
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        // DontDestroyOnLoad(gameObject); // Розкоментуйте, якщо LevelManager має існувати між сценами
    }

    public void OnHints()
    {
        view.ToggleHint(true);
    }

    public void CloseHint()
    {
        view.ToggleHint(false);
    }

    public void CloseInfo()
    {
        view.ToggleBook(false);
    }
    private void LoadPages()
    {
        //int info = PlayerData.Instance.GetAvailableInfo();
        int info = 1;
        pages = new string[info];
        for (int i = 0; i < info; i++)
        {
            pages[i] = Resources.Load<TextAsset>($"Info{i}")?.text ?? $"Missing Info{i}";
        }
    }

    public void unlockTheory(int levelId)
    {
        PlayerData.Instance.ChangeNumbersOfInfo(levelId);
    }
    public void LoadHints(int numberOfHint)
    {
        int level = levelData.CurrentLevel;
        int part = levelData.Progress;
        
        string filename = $"Hint.{level}.{part}.#{numberOfHint}";
        var textAsset = Resources.Load<TextAsset>(filename);
        if (textAsset != null)
            {
                view.SetHint(numberOfHint, textAsset.text);
                view.OpenHint(true);
            }

    }

    public void closeHintPlace()
    {
        view.OpenHint(false);
    }

    public void OnReadInformation()
    {
        UpdatePage();
        view.ToggleBook(true);
    }

    public void OnPrev()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
        }
    }

    public void OnNext()
    {
        if (currentPage < pages.Length - 2)
        {
            currentPage++;
            UpdatePage();
        }
    }

    private void UpdatePage()
    {
        string left = pages[currentPage];
        string right = (currentPage + 1 < pages.Length) ? pages[currentPage + 1] : "";
        view.ShowPage(left, right, currentPage > 0, currentPage < pages.Length - 2);
    }

    public void OnUnlockHint(int index)
    {
        if (canUnlock)
        {
            view.SetLock(index, false);
            canUnlock = false;
            timer = 0f;
        }
    }

    public void OnClickToPass()
    {
        levelData.PassLevel();
        // ButtonAnimation.Instance.PlayAnimation();
        LoadHints(0);
        view.SetLock(0, true);
        view.SetLock(2, true);
        view.SetLock(1, true);
        timer = 0f;
        view.OpenHint(false);
        canUnlock = true;
        view.SetTimerText("");
        PlayerData.Instance.AddStepOfLevel();
        if (levelData.Progress > 3)
        {
            GameManager.Instance.LoadSceneByIndex(3);
        }
    }

    private void Update()
    {
        if (!canUnlock)
        {
            timer += Time.deltaTime;
            float remain = Mathf.Clamp(cooldown - timer, 0f, cooldown);
            view.SetTimerText($"{Mathf.CeilToInt(remain)} sec");

            if (timer >= cooldown)
            {
                canUnlock = true;
                timer = 0f;
                view.SetTimerText("Get it!");
            }
        }
    }

    public void unlockLevel(int levelId)
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (levelId > unlockedLevel)
        {
            PlayerPrefs.SetInt("UnlockedLevel", levelId);
            PlayerPrefs.Save();

            PlayerData.Instance.SetAvailableLevel(levelId);

            Debug.Log($"Рівень {levelId} розблоковано");
        }
    }



}

