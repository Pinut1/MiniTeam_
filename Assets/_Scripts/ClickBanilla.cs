using UnityEngine;

public class ClickableBanilla : MonoBehaviour
{
    private CutsceneNpcManager npcManager;

    private void Start()
    {
        // 씬에서 매니저를 찾아둡니다.
        npcManager = FindAnyObjectByType<CutsceneNpcManager>();
    }

    private void OnMouseDown()
    {
        // 인지 로그
        Debug.Log("우는 바닐라 클릭! 대결을 시작합니다.");

        if (npcManager != null)
        {
            // 매니저에게 대결 시작 명령을 무조건 내립니다.
            npcManager.StartLaserDuel();
        }
    }
}