using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveLoading : MonoBehaviour
{
    void Start()
    {
        SceneHelper.LoadScene("SideMenu", additive: true);
        Scene mainScene = SceneManager.GetSceneByName("Levels");
        SceneManager.SetActiveScene(mainScene);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
