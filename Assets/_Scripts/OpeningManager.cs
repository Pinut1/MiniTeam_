using UnityEngine;
using System.Collections;
using MiniTeam.Pokemon;

public class OpeningManager : MonoBehaviour
{
    [Header("플레이어 및 NPC 스크립트 연결")]
    public MonoBehaviour playerController; // 플레이어 이동 스크립트
    public MonoBehaviour[] npcControllers; // NPC 이동 스크립트들 (배열)

    void Start()
    {
        // OpeningSceneController와 중복 실행되므로 구버전 스크립트인 이 클래스는 작동하지 않도록 방어
        return;
    }

    IEnumerator RunSequence()
    {
        // 대사 진행 중에는 플레이어와 NPC 움직임 스크립트를 끔
        if (playerController != null) playerController.enabled = false;
        foreach (var npc in npcControllers)
        {
            if (npc != null) npc.enabled = false;
        }

        // 2. JSON 파일에 실제로 있는 키(scene_opening_01 ~ 05)를 사용합니다.
        string[] dialogueKeys = {
            "scene_opening_01",
            "scene_opening_02",
            "scene_opening_03",
            "scene_opening_04",
            "scene_opening_05"
        };

        for (int i = 0; i < dialogueKeys.Length; i++)
        {
            string key = dialogueKeys[i];
            bool isLast = (i == dialogueKeys.Length - 1);
            
            // DialogueDB에서 텍스트를 가져와서 UI에 출력
            yield return StartCoroutine(Opening_Dialogue.Instance.Show(DialogueDB.Instance.Get(key), isLast));
        }

        // 대사가 모두 끝나고 패널이 내려가면 움직임 스크립트 다시 켬
        if (playerController != null) playerController.enabled = true;
        foreach (var npc in npcControllers)
        {
            if (npc != null) npc.enabled = true;
        }

        // 게임 플레이 BGM으로 즉시 교체
        if (BgmManager.Instance != null)
        {
            BgmManager.Instance.ChangeBGM(BgmManager.Instance.gameplayBgm);
        }
    }
}