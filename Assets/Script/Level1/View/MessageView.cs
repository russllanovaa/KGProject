using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MessageView : MonoBehaviour
{
    public Text outputText;
    public void ShowMessage(string msg, Color color) { if (outputText != null) { outputText.text = msg; outputText.color = color; } else { } }
    public void ClearMessage() { if (outputText != null) outputText.text = ""; }
}

