using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine;
using NUnit;

public class LevelView3 : MonoBehaviour
{
    public Text pageTextLeft;
    public Text pageTextRight;
    public Button prevButton;
    public Button nextButton;
    public Button closeInfo;
    public GameObject book;

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
                    bookRectTransform.anchoredPosition = new Vector2(0, 100);
                    Debug.Log("Book window moved up");
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

    public void SetNext(bool active) => nextButton.gameObject.SetActive(active);
    public void SetPrev(bool active) => prevButton.gameObject.SetActive(active);
    public void SetClose(bool active) => closeInfo.gameObject.SetActive(active);
}
