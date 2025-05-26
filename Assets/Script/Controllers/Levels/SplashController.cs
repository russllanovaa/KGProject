using UnityEngine;
using System.Collections;

public class SplashController : MonoBehaviour
{

    [Header("Основні об'єкти")]
    [SerializeField] private GameObject splashPanel;

    [SerializeField] private CanvasGroup logoCanvasGroup;      // CanvasGroup логотипу
    [SerializeField] private CanvasGroup nameImageCanvasGroup; // CanvasGroup назви гри

    [Header("Тривалість ефектів")]
    [SerializeField] private float delayBeforeAppear = 1f;     // Затримка перед появою
    [SerializeField] private float fadeDuration = 2f;          // Тривалість появи/зникнення
    [SerializeField] private float visibleDuration = 2f;       // Час видимості
    [SerializeField] private float delayAfterDisappear = 1f;   // Затримка після зникнення

    private void Start()
    {
        StartCoroutine(PlaySplashSequence());
    }

    private IEnumerator PlaySplashSequence()
    {
        if (logoCanvasGroup == null || nameImageCanvasGroup == null || splashPanel == null)
        {
            Debug.LogError("Не вказано splashPanel або logoCanvasGroup або nameImageCanvasGroup у інспекторі.");
            yield break;
        }

        // Початково прозорі обидва
        logoCanvasGroup.alpha = 0f;
        nameImageCanvasGroup.alpha = 0f;

        // Затримка перед появою
        yield return new WaitForSeconds(delayBeforeAppear);

        // Паралельно появляємо обидва
        yield return StartCoroutine(FadeBothCanvasGroups(0f, 1f, fadeDuration));

        // Час видимості
        yield return new WaitForSeconds(visibleDuration);

        // Паралельно зникаємо обидва
        yield return StartCoroutine(FadeBothCanvasGroups(1f, 0f, fadeDuration));

        // Затримка перед зникненням панелі
        yield return new WaitForSeconds(delayAfterDisappear);

        GameManager.Instance.LoadSceneByIndex(2);
        // Вимикаємо панель

    }

    private IEnumerator FadeBothCanvasGroups(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            logoCanvasGroup.alpha = alpha;
            nameImageCanvasGroup.alpha = alpha;

            elapsed += Time.deltaTime;
            yield return null;
        }

        logoCanvasGroup.alpha = endAlpha;
        nameImageCanvasGroup.alpha = endAlpha;
    }
}
