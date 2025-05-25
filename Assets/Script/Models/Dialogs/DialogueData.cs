using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using UnityEngine;
using System.Collections.Generic;

public class DialogueData : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        public string dialogByLevel;
        public int pose;
        public string position;

        public DialogueLine(string characterName, string dialogByLevel, int pose, string position)
        {
            this.characterName = characterName;
            this.dialogByLevel = dialogByLevel;
            this.pose = pose;
            this.position = position;
        }
    }

    List<DialogueLine> lines = new List<DialogueLine>();

    public void LoadDialogue(int levelNumber)
    {
        string filename = "dialog" + levelNumber.ToString();
        TextAsset dialogueFile = Resources.Load<TextAsset>(filename);

        if (dialogueFile != null)
        {
            string[] linesData = dialogueFile.text.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in linesData)
            {
                string[] lineData = line.Split(';');
                if (lineData.Length == 4)
                {
                    DialogueLine lineEntry = new DialogueLine(lineData[0], lineData[1], int.Parse(lineData[2]), lineData[3]);
                    lines.Add(lineEntry);
                }
            }
            
        }
    }

    public string GetPosition(int lineNumber)
    {
        if (lineNumber < lines.Count)
        {
            return lines[lineNumber].position;
        }
        return "";
    }

    public string GetName(int lineNumber)
    {
        if (lineNumber < lines.Count)
        {
            return lines[lineNumber].characterName;
        }
        return "";
    }

    public string GetContent(int lineNumber)
    {
        if (lineNumber < lines.Count)
        {
            return lines[lineNumber].dialogByLevel;
        }
        return "";
    }

    public int GetPose(int lineNumber)
    {
        if (lineNumber < lines.Count)
        {
            return lines[lineNumber].pose;
        }
        return 0;
    }
}
