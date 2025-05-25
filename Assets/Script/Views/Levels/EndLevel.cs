using UnityEngine;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour
{

    public static ButtonAnimation Instance { get; private set; }

    public Animator animator;
    public Button yourButton;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        yourButton.onClick.AddListener(PlayAnimation);
    }

    public void PlayAnimation()
    {
        animator.Play("starAnim");
    }
}
