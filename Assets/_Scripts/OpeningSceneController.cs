using MiniTeam.Pokemon;
using System.Collections;
using UnityEngine;

public class OpeningSceneController : MonoBehaviour
{
    [Header("대화 파일")]
    public string dialogueFileName = "Sugar"; // Resources/Dialogues/ 폴더 내 파일명

    [Header("배경음악 설정")]
    public AudioClip openingBGM;
    public AudioClip gameplayBGM;

    [Header("캐릭터 표정(UI)")]
    public GameObject smileImage; // Chocolate_Smile 연결
    public GameObject sadImage;   // Chocolate_Sad 연결

    [Header("플레이어 조작")]
    public MonoBehaviour playerController; // 플레이어 움직임 스크립트 연결
    public MonoBehaviour[] npcControllers; // NPC 이동 스크립트들 (배열)

    [Header("컷신 매니저")]
    public CutsceneNpcManager npcManager; // NPC 움직임이 필요하면 연결

    void Start()
    {
        // Additive 씬 로딩 시 현재 씬을 Active로 설정해야 Instantiate로 생성한 오브젝트가 Hub로 가지 않습니다.
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(gameObject.scene);
        StartCoroutine(OpeningSequence());
    }

    IEnumerator OpeningSequence()
    {
        Debug.Log("[Opening] 오프닝 시퀀스 시작!");

        // 오프닝 BGM은 BgmManager가 시작될 때(Awake) 자동으로 켜집니다.
        // 혹시 모르니 확실하게 오프닝 재생을 다시 호출합니다.
        if (BgmManager.Instance != null)
        {
            BgmManager.Instance.PlayOpeningBGM();
        }
        
        // NpcSpawner 스폰 일시정지
        NpcSpawner spawner = FindObjectOfType<NpcSpawner>();
        if (spawner != null)
        {
            spawner.isSpawningPaused = true;
            Debug.Log("[Opening] NPC 자동 생성 일시정지");
        }

        // 1. 플레이어 조작 막기
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("[Opening] 플레이어 조작 비활성화");
        }
        else
        {
            Debug.LogWarning("[Opening] playerController가 연결되지 않았습니다!");
        }

        // NPC 움직임도 막기
        if (npcControllers != null)
        {
            foreach (var npc in npcControllers)
            {
                if (npc != null) npc.enabled = false;
            }
            Debug.Log("[Opening] NPC 조작 비활성화");
        }

        // 2. 대화 로드
        DialogueDB.Instance.Load(dialogueFileName);

        // 3. 대사 순서대로 출력
        string[] dialogueKeys = {
            "scene_opening_01",
            "scene_opening_02",
            "scene_opening_03",
            "scene_opening_04",
            "scene_opening_05"
        };

        for (int i = 0; i < dialogueKeys.Length; i++)
        {
            // 1, 4, 5번째 대사(인덱스 0, 3, 4)는 웃는 표정
            if (i == 0 || i == 3 || i == 4)
            {
                if (smileImage != null) smileImage.SetActive(true);
                if (sadImage != null) sadImage.SetActive(false);
            }
            // 2, 3번째 대사(인덱스 1, 2)는 슬픈 표정
            else if (i == 1 || i == 2)
            {
                if (smileImage != null) smileImage.SetActive(false);
                if (sadImage != null) sadImage.SetActive(true);
            }

            string key = dialogueKeys[i];
            string text = DialogueDB.Instance.Get(key);
            Debug.Log($"[Opening] 대화 출력 시도: {key} -> {text}");
            
            if (Opening_Dialogue.Instance == null)
            {
                Debug.LogError("[Opening] Opening_Dialogue.Instance가 NULL입니다! UI가 씬에 없거나 Awake가 호출되지 않았습니다.");
                break;
            }
            
            bool isLast = (i == dialogueKeys.Length - 1);
            yield return StartCoroutine(Opening_Dialogue.Instance.Show(text, isLast));
            Debug.Log($"[Opening] 대화 출력 완료: {key}");
        }

        // 4. 컷신 시작
        Debug.Log("[Opening] 대화 종료. 컷신 시작!");

        // 여기에 NPC 움직임 컷씬을 실행하거나, 바로 게임 플레이로 전환
        if (npcManager != null)
        {
            Debug.Log("[Opening] npcManager.SpawnAndPlayCutscene 호출");
            npcManager.SpawnAndPlayCutscene();
        }
        else
        {
            if (playerController != null)
            {
                playerController.enabled = true;
            }
            if (npcControllers != null)
            {
                foreach (var npc in npcControllers)
                {
                    if (npc != null) npc.enabled = true;
                }
            }
        }
        
        // NpcSpawner 스폰 다시 시작
        NpcSpawner spawnerRef = FindObjectOfType<NpcSpawner>();
        if (spawnerRef != null)
        {
            spawnerRef.isSpawningPaused = false;
            Debug.Log("[Opening] NPC 자동 생성 재개");
        }

        // 5. 게임 플레이 BGM으로 전환 (자연스러운 페이드 효과 적용)
        if (BgmManager.Instance != null)
        {
            BgmManager.Instance.ChangeBGM(BgmManager.Instance.gameplayBgm);
            Debug.Log("[Opening] 게임 플레이 BGM 재생 시작");
        }
    }
}