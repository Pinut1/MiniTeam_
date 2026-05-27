using MiniTeam.Pokemon;
using System.Collections;
using UnityEngine;

public class OpeningSceneController : MonoBehaviour
{
    [Header("시작 설정")]
    public string dialogueFileName = "Opening"; // Resources/Dialogues/ 폴더 내 파일명

    [Header("참조")]
    public CutsceneNpcManager npcManager; // NPC 움직임이 필요하면 연결

    void Start()
    {
        StartCoroutine(OpeningSequence());
    }

    IEnumerator OpeningSequence()
    {
        // 1. 씬 시작 시 조작 금지
        // 플레이어 캐릭터가 있다면 여기서 비활성화하세요.
        // 예: playerController.enabled = false;

        // 2. 대사 데이터 로드
        DialogueDB.Instance.Load(dialogueFileName);

        // 3. 대사 순서대로 출력
        string[] dialogueKeys = {
            "scene_opening_01",
            "scene_opening_02",
            "scene_opening_03",
            "scene_opening_04",
            "scene_opening_05",
            "scene_opening_06"
        };

        foreach (string key in dialogueKeys)
        {
            string text = DialogueDB.Instance.Get(key);
            yield return StartCoroutine(MapDialogueUI.Instance.Show(text));
        }

        // 4. 대사 끝난 후 로직
        Debug.Log("대사 완료! 이제 게임 시작");

        // 여기에 NPC 움직임 컷씬을 실행하거나, 바로 게임 플레이로 전환
        if (npcManager != null)
        {
            npcManager.SpawnAndPlayCutscene();
        }
        else
        {
            // NPC 컷씬 없이 바로 게임 시작 시 조작 활성화
            // 예: playerController.enabled = true;
        }
    }
}