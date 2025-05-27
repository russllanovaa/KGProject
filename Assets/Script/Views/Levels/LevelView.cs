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
    public Button closeInfo;
    public GameObject book;
    public GameObject hint;
    public Text hints;
    public GameObject hintPlace;
    public GameObject[] locks;
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
                // Move up and set canvas order
                RectTransform bookRectTransform = book.GetComponent<RectTransform>();
                Canvas canvas = book.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 1000; // Вищий номер — вище на екрані
                }

                if (bookRectTransform != null)
                {
                    bookRectTransform.anchoredPosition = new Vector2(0, 100);

                }
                else
                {
                    //Debug.LogWarning("Book GameObject does not have a RectTransform component.");
                }
            }
        }
        else
        {
            //Debug.LogError("Book GameObject is not assigned in the Inspector.");
        }
    }

    public void ToggleHint(bool active)
    {
        if (hint != null)
        {
            hint.SetActive(active);
            if (active)
            {
                // Reset position and set canvas order
                RectTransform hintRectTransform = hint.GetComponent<RectTransform>();
                Canvas canvas = hint.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 1001; // Трохи вище, ніж книга
                }

                if (hintRectTransform != null)
                {
                    hintRectTransform.anchoredPosition = Vector2.zero;
                    hint.transform.localPosition = Vector3.zero;
                    hint.transform.position = Vector3.zero;

                }
                else
                {
                    //Debug.LogWarning("Hint GameObject does not have a RectTransform component.");
                }
            }
        }
        else
        {
            //Debug.LogError("Hint GameObject is not assigned in the Inspector.");
        }
    }

    public void SetHint(int index, string text) => hints.text = text;

    public void OpenHint(bool active) => hintPlace.SetActive(active);

    public void SetLock(int index, bool state) => locks[index].SetActive(state);
    public void SetTimerText(string text) => timerText.text = text;

    public void SetNext(bool active) => nextButton.gameObject.SetActive(active);
    public void SetPrev(bool active) => prevButton.gameObject.SetActive(active);
    public void SetClose(bool active) => closeInfo.gameObject.SetActive(active);
}
