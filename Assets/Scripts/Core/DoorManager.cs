using UnityEngine;

namespace MiniTeam.Core
{
    public class DoorManager : MonoBehaviour
    {
        public static DoorManager Instance { get; private set; }

        [Header("Door Management")]
        [Tooltip("스테이지 순서대로 문(Stage Door)을 할당. (Stage 1 = Index 0)")]
        public StageDoor[] stageDoors;

        [Header("Warning Messages")]
        [Tooltip("아직 비활성화된 스테이지 문에 접근했을 때의 경고 메시지")]
        public string inactiveDoorWarning = "현재 지정된 경로에 접근할 수 없습니다. 계속하려면 다른 문을 이용하십시오.";
        
        [Tooltip("활성화된 문이지만 주댕치와의 대화 전일 때의 경고 메시지")]
        public string cutscenePendingWarning = "작업이 중단되었습니다. 계속 실행하려면 [주댕치]의 이야기를 입력 하십시오.";

        [Tooltip("모든 스테이지를 클리어했을 때의 경고 메시지")]
        public string allClearWarning = "더 이상 실행할 작업이 없습니다.";
        
        [Tooltip("이미 클리어한 스테이지 문에 접근했을 때의 경고 메시지")]
        public string alreadyClearedWarning = "해당 구역은 이미 탐색이 완료되었습니다.";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // 허브 씬 전용이므로 DontDestroyOnLoad는 사용하지 않습니다.
        }

        public bool IsDoorActive(StageDoor door)
        {
            if (MiniGameManager.Instance == null) return false;

            // currentStage는 1부터 시작하고 배열 인덱스는 0부터 시작하므로 수 맞춤
            int stageIndex = MiniGameManager.Instance.progressData.currentStage;

            if (stageDoors == null || stageDoors.Length == 0) return false;
            if (stageIndex < 0 || stageIndex >= stageDoors.Length) return false;

            return stageDoors[stageIndex] == door;
        }

        public int GetDoorIndex(StageDoor door)
        {
            if (stageDoors == null) return -1;
            return System.Array.IndexOf(stageDoors, door);
        }

        public void TryInteractWithDoor(StageDoor door, bool isInputPressed = false)
        {
            if (MiniGameManager.Instance == null) return;
            
            if (IsDoorActive(door))
            {
                if (MiniGameManager.Instance.progressData.isCutscenePlayed)
                {
                    if (isInputPressed)
                    {
                        door.ToggleDoor();
                    }
                }
                else
                {
                    ShowWarning(cutscenePendingWarning);
                }
            }
            else
            {
                if (!isInputPressed)
                {
                    ShowInactiveWarning(door);
                }
            }
        }

        public void HideWarning()
        {
            HubUIManager.Instance?.ToggleWarningUI(false);
        }

        private void ShowWarning(string message)
        {
            Debug.Log($"[DoorManager Warning] {message}");
            HubUIManager.Instance?.ToggleWarningUI(true, message);
        }

        private void ShowInactiveWarning(StageDoor door)
        {
            var manager = MiniGameManager.Instance;
            if (manager == null || stageDoors == null)
            {
                ShowWarning(inactiveDoorWarning);
                return;
            }

            if (manager.progressData.currentStage >= 5)
            {
                ShowWarning(allClearWarning);
                return;
            }

            int myIndex = GetDoorIndex(door);
            if (myIndex != -1)
            {
                if (myIndex < manager.progressData.currentStage)
                {
                    ShowWarning(alreadyClearedWarning);
                    return;
                }
            }

            ShowWarning(inactiveDoorWarning);
        }
    }
}
