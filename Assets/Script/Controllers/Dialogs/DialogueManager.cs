using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public DialogView dialogView;
    public DialogueData dialogueData;
    public Image back;
    public int CurrentPose => pose;

    string dialogue, characterName, position;
    int pose;
    public int lineNum;
    public bool playerTalking;
    public int currentLevel;
    public int nullingLevel=0;

    public int GetInfoNulling()
    {
        return nullingLevel;
    }
    private void Awake()
    {
            instance = this;
    }

    private void Start()
    {
        nullingLevel++;

        Debug.Log("nullingLevel");
        dialogueData = new DialogueData();
        StartDialogue(PlayerData.Instance.GetCurrentLevel());
       if(PlayerData.Instance.GetCurrentLevel() == 0)
        {
            Sprite mySprite = Resources.Load<Sprite>("Images/0"); // з Assets/Resources/Images/MyImage.png
            back.sprite = mySprite;
        }
        else
        {
            Sprite mySprite = Resources.Load<Sprite>("Images/1"); // з Assets/Resources/Images/MyImage.png
            back.sprite = mySprite;
        }

        Debug.Log("CURRENT LEVEL" + PlayerData.Instance.GetCurrentLevel());
    }

    private void Update()
    {

        Debug.Log("Update");
        if (Input.GetKeyDown(KeyCode.Space) && !playerTalking)
        {
            lineNum++;
            ShowDialogue();
        }
    }

    public void StartDialogue(int level)
    {
        dialogueData.LoadDialogue(level);
        lineNum = 0;
        currentLevel = level;
        Debug.Log(currentLevel + "CURRENT");
        ShowDialogue();
    }


    void ShowDialogue()
    {
        ParseLine();
        if (dialogue == "")
        {

          GameManager.Instance.LoadSceneByIndex(currentLevel);
            return;
        }
        dialogView.UpdateDialogueUI(dialogue, characterName);
        dialogView.SetCharacterPosition(position);

        dialogView.HideAllOtherCharacters(characterName); 
    }


    void ParseLine()
    {
        playerTalking = false;
        Debug.Log("MEOW MEOW" + lineNum);
        characterName = dialogueData.GetName(lineNum);
        dialogue = dialogueData.GetContent(lineNum);
        pose = dialogueData.GetPose(lineNum);
        position = dialogueData.GetPosition(lineNum);
    }
}
