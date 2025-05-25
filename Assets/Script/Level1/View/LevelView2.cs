using UnityEngine;
using UnityEngine.UI; 

public class LevelView2 : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Посилання на компонент Image, який ми будемо змінювати.")]
    [SerializeField] private Image targetImage; 

    [Header("Available Sprites")]
    [Tooltip("Масив спрайтів, з яких будемо обирати зображення.")]
    [SerializeField] private Sprite[] levelBackgrounds; 

    public void SetLevelBackground(int spriteIndex)
    {
       

        if (spriteIndex >= 0 && spriteIndex < levelBackgrounds.Length)
        {
            // Змінюємо спрайт компонента Image
            targetImage.sprite = levelBackgrounds[spriteIndex];
            Debug.Log($"LevelView: Background changed to sprite at index {spriteIndex}.");
        }
        else
        {
            Debug.LogWarning($"LevelView: Invalid sprite index {spriteIndex}. Array has {levelBackgrounds.Length} elements.", this);
        }
    }

    private void Start()
    {

         SetLevelBackground(0); 
    }

}