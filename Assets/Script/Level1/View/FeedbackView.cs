using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// --- MessageView та FeedbackView залишаються такими ж, як у попередній відповіді ---

public class FeedbackView : MonoBehaviour
{
    public Image image;
    public Animator animator;
    public float ShakeMagnitude = 0.1f; // Інтенсивність тремтіння
    public float ShakeSpeed = 20f;      // Швидкість тремтіння
    public float PulseScale = 1.1f;     // На скільки збільшується фігура при пульсації
    public float AnimationDuration = 2.0f; // Тривалість анімації (пульсація/тремтіння)

  
    public IEnumerator AnimateSuccess(GameObject figureObject, Vector3 targetCenter, float targetSize)
    {
        Debug.Log("Animation success");
        LineRenderer lr = figureObject.GetComponent<LineRenderer>();


        LockController.Instance.MessageView.ShowMessage("Успіх", Color.green);
        if (lr == null)
        {
            Destroy(figureObject);
            yield break;
        }


       
        Vector3 originalScale = figureObject.transform.localScale;
        Vector3 originalPosition = figureObject.transform.localPosition;
        if (animator != null)
        {
            animator.Play("successLock", -1, 0f); // Назва анімаційного кліпу
            yield return new WaitForSeconds(0.3f);
            image.color = new Color(1, 1, 1, 1);
        }


        float timer = 0f;
        while (timer < AnimationDuration)
        {

            figureObject.transform.localPosition = Vector3.Lerp(originalPosition, targetCenter, timer / AnimationDuration);

            float pulse = 1f + Mathf.Sin(timer * Mathf.PI / AnimationDuration * 1f) * (PulseScale - 1f); // 4 повні пульсації за час
            figureObject.transform.localScale = originalScale * pulse;

            timer += Time.deltaTime;
            yield return null;
        }

        figureObject.transform.localPosition = targetCenter;
        figureObject.transform.localScale = originalScale;


        Destroy(figureObject);
        LockController.Instance.OnSuccessAnimationComplete();
    }

    public IEnumerator AnimateFailure(GameObject figureObject, string message)
    {
        Debug.Log("Animation failure");
        Vector3 originalPosition = figureObject.transform.position;
        LockController.Instance.MessageView.ShowMessage(message, Color.red);

        float timer = 0f;
        while (timer < AnimationDuration)
        {
            // Анімація тремтіння
            float offsetX = Mathf.Sin(timer * ShakeSpeed) * ShakeMagnitude;
            float offsetY = 0;

            figureObject.transform.position = originalPosition + new Vector3(offsetX, offsetY, 0);

            timer += Time.deltaTime;
            yield return null;
        }
    }

 
}