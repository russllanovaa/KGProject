using UnityEngine;
using TMPro;

public class DialogView : MonoBehaviour
{
    public TMP_Text dialogueBox;
    public TMP_Text nameBox;

    public GameObject objLeft;
    public GameObject objRight;

    public void UpdateDialogueUI(string dialogue, string characterName)
    {
        if (dialogueBox != null && nameBox != null)
        {
            dialogueBox.text = dialogue;
            nameBox.text = characterName;
        }
        
    }

    public void ResetCharacterImage(string characterName)
    {
        if (characterName != "")
        {
            GameObject character = GameObject.Find(characterName);
            SpriteRenderer currSprite = character.GetComponent<SpriteRenderer>();
            currSprite.sprite = null;
        }
    }

    public void HideAllOtherCharacters(string activeCharacterName)
    {
        Character[] allCharacters = GameObject.FindObjectsOfType<Character>();

        foreach (Character ch in allCharacters)
        {
            SpriteRenderer sr = ch.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = ch.name == activeCharacterName
                    ? ch.characterPoses[DialogueManager.instance.CurrentPose]
                    : null;
            }
        }
    }

    public void DisplayCharacterImage(string characterName, int pose)
    {
        GameObject character = GameObject.Find(characterName);
        if (character != null)
        {
            SpriteRenderer renderer = character.GetComponent<SpriteRenderer>();
            if (renderer != null)
                renderer.sprite = character.GetComponent<Character>().characterPoses[pose];
        }
    }

    public void SetCharacterPosition(string position)
    {
        position = position.Trim().ToLower();

        if (position == "left")
        {
            objLeft.SetActive(true);
            objRight.SetActive(false);
            nameBox.rectTransform.anchoredPosition = new Vector2(-450, nameBox.rectTransform.anchoredPosition.y);
        }
        else if (position == "right")
        {
            objLeft.SetActive(false);
            objRight.SetActive(true);
            nameBox.rectTransform.anchoredPosition = new Vector2(700, nameBox.rectTransform.anchoredPosition.y);
        }
    }
}
