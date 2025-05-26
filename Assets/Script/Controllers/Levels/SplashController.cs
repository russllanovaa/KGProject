using UnityEngine;
using System.Collections;

public class SplashController : MonoBehaviour
{

    [Header("������ ��'����")]
    [SerializeField] private GameObject splashPanel;

    [SerializeField] private CanvasGroup logoCanvasGroup;      // CanvasGroup ��������
    [SerializeField] private CanvasGroup nameImageCanvasGroup; // CanvasGroup ����� ���

    [Header("��������� ������")]
    [SerializeField] private float delayBeforeAppear = 1f;     // �������� ����� ������
    [SerializeField] private float fadeDuration = 2f;          // ��������� �����/���������
    [SerializeField] private float visibleDuration = 2f;       // ��� ��������
    [SerializeField] private float delayAfterDisappear = 1f;   // �������� ���� ���������

    private void Start()
    {
        StartCoroutine(PlaySplashSequence());
    }

    private IEnumerator PlaySplashSequence()
    {
        if (logoCanvasGroup == null || nameImageCanvasGroup == null || splashPanel == null)
        {
            Debug.LogError("�� ������� splashPanel ��� logoCanvasGroup ��� nameImageCanvasGroup � ���������.");
            yield break;
        }

        // ��������� ������ ������
        logoCanvasGroup.alpha = 0f;
        nameImageCanvasGroup.alpha = 0f;

        // �������� ����� ������
        yield return new WaitForSeconds(delayBeforeAppear);

        // ���������� ��������� ������
        yield return StartCoroutine(FadeBothCanvasGroups(0f, 1f, fadeDuration));

        // ��� ��������
        yield return new WaitForSeconds(visibleDuration);

        // ���������� ������� ������
        yield return StartCoroutine(FadeBothCanvasGroups(1f, 0f, fadeDuration));

        // �������� ����� ���������� �����
        yield return new WaitForSeconds(delayAfterDisappear);

        GameManager.Instance.LoadSceneByIndex(2);
        // �������� ������

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
