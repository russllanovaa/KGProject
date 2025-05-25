using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine;
using NUnit;

public class LevelView : MonoBehaviour
{
    public Text pageTextLeft;
    public Text pageTextRight;
    public Button prevButton;
    public Button nextButton;
    public GameObject book;
    public GameObject hint;
    public Text hints;
    public GameObject hintPlace;
    public GameObject[] locks;
    public GameObject end;
    public Text timerText;

    public void ShowPage(string leftText, string rightText, bool canGoPrev, bool canGoNext)
    {
        pageTextLeft.text = leftText;
        pageTextRight.text = rightText;
        prevButton.interactable = canGoPrev;
        nextButton.interactable = canGoNext;
    }

    public void InitializeLevelButtons(Button[] buttons)
    {
        Debug.Log("InitializeLevelButtons");
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = (i < unlockedLevel);
        }
    }

    public void ToggleBook(bool active)
    {
        if (book != null)
        {
            book.SetActive(active);
            if (active)
            {
                RectTransform bookRectTransform = book.GetComponent<RectTransform>();
                if (bookRectTransform != null)
                {
                    bookRectTransform.anchoredPosition = Vector2.zero;
                     book.transform.localPosition = Vector3.zero; 
                     book.transform.position = Vector3.zero; 
                    Debug.Log("Book window activated and moved to (0,0)");
                }
                else
                {
                    Debug.LogWarning("Book GameObject does not have a RectTransform component.");
                }
            }
        }
        else
        {
            Debug.LogError("Book GameObject is not assigned in the Inspector.");
        }
    }

    public void ToggleHint(bool active)
    {
        if (hint != null)
        {
            hint.SetActive(active);
            if (active)
            {
                RectTransform hintRectTransform = hint.GetComponent<RectTransform>();
                if (hintRectTransform != null)
                {
                    hintRectTransform.anchoredPosition = Vector2.zero;
                     hint.transform.localPosition = Vector3.zero;
                     hint.transform.position = Vector3.zero;
                    Debug.Log("Hint window activated and moved to (0,0)");
                }
                else
                {
                    Debug.LogWarning("Hint GameObject does not have a RectTransform component.");
                }
            }
        }
        else
        {
            Debug.LogError("Hint GameObject is not assigned in the Inspector.");
        }
    }
    public void ToggleEnd(bool active) => end.SetActive(active);
    public void SetHint(int index, string text) => hints.text = text;

    public void OpenHint(bool active) => hintPlace.SetActive(active);

    public void SetLock(int index, bool state) => locks[index].SetActive(state);
    public void SetTimerText(string text) => timerText.text = text;
}
