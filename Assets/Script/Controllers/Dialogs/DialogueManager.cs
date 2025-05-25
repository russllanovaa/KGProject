using Unity.VisualScripting;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public DialogView dialogView;
    public DialogueData dialogueData;

    public int CurrentPose => pose;

    string dialogue, characterName, position;
    int pose;
    public int lineNum;
    public bool playerTalking;
    public int currentLevel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        dialogueData = new DialogueData();
        StartDialogue(PlayerData.Instance.GetCurrentLevel());
    }

    private void Update()
    {
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
        characterName = dialogueData.GetName(lineNum);
        dialogue = dialogueData.GetContent(lineNum);
        pose = dialogueData.GetPose(lineNum);
        position = dialogueData.GetPosition(lineNum);
    }
}
