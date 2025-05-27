using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

public class LevelManager2 : MonoBehaviour
{
    // Статичне поле для зберігання єдиного екземпляра класу
    private static LevelManager2 _instance;
    public Animator animator;
    public Image image;
    // Публічна властивість для доступу до екземпляра
    public static LevelManager2 Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LevelManager2>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(LevelManager2).Name);
                    _instance = singletonObject.AddComponent<LevelManager2>();
                }
            }
            return _instance;
        }
    }
    public LevelView3 view;

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
        //UpdatePage();
        view.ToggleBook(false);
        unlockTheory(levelData.CurrentLevel);
        view.SetNext(false);
        view.SetPrev(false);
        view.SetClose(false);
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

    public void CloseInfo()
    {
        SoundManager.Instance.PlaySound2D("page_turn");
        string left = "";
        string right = "";
        view.SetNext(false);
        view.SetPrev(false);
        view.SetClose(false);
        view.ShowPage(left, right, currentPage > 0, currentPage < pages.Length - 2);

        if (animator != null)
        {
            animator.Play("CloseAnim", -1, 0f);
            image.color = new Color(1, 1, 1, 1);
            StartCoroutine(WaitForAnimationThenUpdateClose(true));


        }
        else
        {
            view.ToggleBook(false);
        }
    }
    private void LoadPages()
    {
        //int info = PlayerData.Instance.GetAvailableInfo();
        int info = 4;
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

    public void OnReadInformation()
    {
        SoundManager.Instance.PlaySound2D("page_turn");
        string left = "";
        string right = "";
        view.SetNext(false);
        view.SetPrev(false);
        view.SetClose(false);
        view.ShowPage(left, right, currentPage > 0, currentPage < pages.Length - 2);
        view.ToggleBook(true);

        if (animator != null)
        {
            animator.Play("OpenAnim", -1, 0f); 
            image.color = new Color(1, 1, 1, 1);
            StartCoroutine(WaitForAnimationThenUpdate(true)); 

           
        }
        else
        {
            UpdatePageImmediately();
        }
        
    }

    public void OnPrev()
    {
        SoundManager.Instance.PlaySound2D("page_turn");
        string left = "";
        string right = "";
        view.SetNext(false);
        view.SetPrev(false);
        view.SetClose(false);
        view.ShowPage(left, right, currentPage > 0, currentPage < pages.Length - 2);

        if (animator != null)
        {
            animator.Play("ChangeLeftAnim", -1, 0f); 
            image.color = new Color(1, 1, 1, 1);
            StartCoroutine(WaitForAnimationThenUpdateLeft(false)); 
        }
        else if (currentPage > 0)
        {
            currentPage--;
            UpdatePageImmediately();
        }
    }

    public void OnNext()
    {
        SoundManager.Instance.PlaySound2D("page_turn");
        string left = "";
        string right = "";
        view.SetNext(false);
        view.SetPrev(false);
        view.SetClose(false);
        view.ShowPage(left, right, currentPage > 0, currentPage < pages.Length - 2);

        if (animator != null)
        {
            animator.Play("ChangeAnim", -1, 0f); 
            image.color = new Color(1, 1, 1, 1);
            StartCoroutine(WaitForAnimationThenUpdate(false)); 
        }
        else if (currentPage < pages.Length - 2)
        {
            currentPage++;
            UpdatePageImmediately();
        }
    }

    private void UpdatePageImmediately()
    {
        string left = pages[currentPage];
        string right = (currentPage + 1 < pages.Length) ? pages[currentPage + 1] : "";
        view.ShowPage(left, right, currentPage > 0, currentPage < pages.Length - 2);

        view.SetNext(true);
        view.SetPrev(true);
        view.SetClose(true);
    }

    private IEnumerator WaitForAnimationThenUpdate(bool isOpening)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;
        yield return new WaitForSeconds(animationLength);

        if (!isOpening && currentPage < pages.Length - 2)
        {
            currentPage+=2;
        }

        UpdatePageImmediately();
        
    }

    private IEnumerator WaitForAnimationThenUpdateLeft(bool isOpening)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;
        yield return new WaitForSeconds(animationLength);

        if (currentPage > 0)
        {
            currentPage-=2;
        }

        UpdatePageImmediately();

    }

    private IEnumerator WaitForAnimationThenUpdateClose(bool isOpening)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;
        yield return new WaitForSeconds(animationLength);

        view.ToggleBook(false);
        // UpdatePageImmediately();

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

