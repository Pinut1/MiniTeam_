using UnityEngine;
using UnityEngine.SceneManagement;

public class InCutScene_SceneChanger : MonoBehaviour
{
   public void GoToMainScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
