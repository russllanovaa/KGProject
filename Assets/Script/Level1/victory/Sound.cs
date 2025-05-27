using UnityEngine;

public class Sound : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.Instance.PlaySound2D("victory");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
