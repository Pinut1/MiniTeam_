using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위해 반드시 추가해야 합니다.

public class MainUIManager : MonoBehaviour
{
    // "Hub" 씬으로 이동하는 함수 (버튼에 연결할 것이므로 public으로 선언)
    public void GameStart()
    {
        // "Hub"라는 이름의 씬을 로드합니다.
        SceneManager.LoadScene("Hub");
    }

    // 게임을 종료하는 함수
    public void QuitGame()
    {
        // 유니티 에디터 환경에서 플레이 중일 때
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("게임 종료 (에디터)");
#else
        // 실제 빌드된 게임(exe, apk 등)에서 플레이 중일 때
        Application.Quit();
#endif
    }
}