using UnityEngine;
using UnityEngine.UI; 

public class LevelView2 : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("��������� �� ��������� Image, ���� �� ������ ��������.")]
    [SerializeField] private Image targetImage; 

    [Header("Available Sprites")]
    [Tooltip("����� �������, � ���� ������ ������� ����������.")]
    [SerializeField] private Sprite[] levelBackgrounds; 

    public void SetLevelBackground(int spriteIndex)
    {
       

        if (spriteIndex >= 0 && spriteIndex < levelBackgrounds.Length)
        {
            // ������� ������ ���������� Image
            targetImage.sprite = levelBackgrounds[spriteIndex];

        }
        else
        {
            //Debug.LogWarning($"LevelView: Invalid sprite index {spriteIndex}. Array has {levelBackgrounds.Length} elements.", this);
        }
    }

    private void Start()
    {

         SetLevelBackground(0); 
    }

}