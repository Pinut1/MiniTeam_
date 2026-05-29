using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniTeam.Core
{
    public class EndingSceneManager : MonoBehaviour
    {
        [Header("이동할 씬 이름")]
        [Tooltip("엔딩이 끝나고 돌아갈 씬의 이름을 적어주세요. (기본값: Main)")]
        public string mainSceneName = "Main";

        /// <summary>
        /// 메인 씬으로 돌아가는 메서드. 
        /// 타임라인 시그널이나 버튼의 OnClick 이벤트에서 호출하세요.
        /// </summary>
        public void GoToMainScene()
        {
            Debug.Log($"[EndingSceneManager] {mainSceneName} 씬으로 이동합니다.");
            
            // 만약 컷씬 도중 게임이 일시정지(Time.timeScale = 0) 되어 있었다면 정상 속도로 복구
            Time.timeScale = 1f;
            
            SceneManager.LoadScene(mainSceneName);
        }

        /// <summary>
        /// (선택 사항) 아예 게임을 종료하고 싶을 때 사용하는 메서드
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[EndingSceneManager] 게임을 종료합니다.");
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
