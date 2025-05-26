using UnityEngine;
using UnityEngine.UI;

public class ScrollViewSwitcher : MonoBehaviour
{
    public GameObject scrollView1;
    public GameObject scrollView2;
    public Button button1;
    public Button button2;

    void Start()
    {
        // Призначення методів для кнопок
        button1.onClick.AddListener(ShowScrollView1);
        button2.onClick.AddListener(ShowScrollView2);

        // Початковий стан (наприклад, показуємо перший ScrollView)
        ShowScrollView1();
    }

    void ShowScrollView1()
    {
        scrollView1.SetActive(true);
        scrollView2.SetActive(false);
    }

    void ShowScrollView2()
    {
        scrollView1.SetActive(false);
        scrollView2.SetActive(true);
    }
}
