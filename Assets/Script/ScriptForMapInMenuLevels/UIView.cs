using UnityEngine;
using System.Collections;

public class AnimatedMenuController : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] private RectTransform sideMenu;

    [Header("Buttons")]
    [SerializeField] private GameObject burgerButton;
    [SerializeField] private GameObject closeButton;

    [Header("Animation")]
    [SerializeField] private Vector2 hiddenPosition = new Vector2(-200, 0);
    [SerializeField] private Vector2 shownPosition = new Vector2(200, 0);
    [SerializeField] private float animationDuration = 0.5f;

    private Coroutine currentAnimation;

    void Start()
    {
        sideMenu.anchoredPosition = hiddenPosition;
        burgerButton.SetActive(true);
        closeButton.SetActive(false);
    }
    public void OpenMenu()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateMenu(sideMenu.anchoredPosition, shownPosition));
       
        burgerButton.SetActive(false);
        closeButton.SetActive(true);
    }

    public void CloseMenu()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateMenu(sideMenu.anchoredPosition, hiddenPosition));
        
        burgerButton.SetActive(true);
        closeButton.SetActive(false);
    }

    private IEnumerator AnimateMenu(Vector2 from, Vector2 to)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            sideMenu.anchoredPosition = Vector2.Lerp(from, to, elapsed / animationDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        sideMenu.anchoredPosition = to;
    }
}