using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniTeam.Core
{
    public class EndingSceneManager : MonoBehaviour
    {
        [Header("이동 할 씬 이름")]
        [Tooltip("메인 메뉴 역할을 하는 씬 주소. (기본값: Hub)")]
        public string mainSceneName = "Hub";

        /// <summary>
        /// 메인 씬으로 돌아가는 메서드. 
        /// 타임라인 시그널이나 버튼의 OnClick 이벤트에서 호출하세요.
        /// </summary>
        public void GoToMainScene()
        {
            Debug.Log($"[EndingSceneManager] {mainSceneName} 로 이동합니다.");
            
            // 일시정지 상태(Time.timeScale = 0) 였다면 속도 복구
            Time.timeScale = 1f;
            
            // 통합된 씬 구조: Hub 씬으로 가되, 메인 메뉴 상태로 활성화되도록 지시
            if (MiniGameManager.Instance != null)
            {
                MiniGameManager.Instance.isMainMenuActive = true;
            }
            
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
