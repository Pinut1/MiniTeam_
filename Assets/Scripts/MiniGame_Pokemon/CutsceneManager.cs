using System.Collections;
using UnityEngine;
using TMPro;

namespace MiniTeam.Pokemon
{
    // 쿠치파치 등장 컷씬 담당
    // 오박사 코스프레 NPC → "나는 포켓몬이 아니야~!" → 아이템 제공
    public class CutsceneManager : MonoBehaviour
    {
        public static CutsceneManager Instance { get; private set; }

        [Header("컷씬 패널")]
        public GameObject cutscenePanel;
        public TextMeshProUGUI dialogueText;

        [Header("쿠치파치 스프라이트")]
        public UnityEngine.UI.Image kuchipachImage;
        public Sprite kuchipachSprite;

        [Header("아이템 오브젝트 (맵에 배치된 수령 오브젝트)")]
        public GameObject itemObject;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (cutscenePanel != null) cutscenePanel.SetActive(false);
        }

        public void PlayKuchipachScene(TrainerTrigger trainer)
        {
            StartCoroutine(KuchipachRoutine(trainer));
        }

        IEnumerator KuchipachRoutine(TrainerTrigger trainer)
        {
            if (cutscenePanel != null) cutscenePanel.SetActive(true);
            if (kuchipachImage != null && kuchipachSprite != null)
                kuchipachImage.sprite = kuchipachSprite;

            yield return ShowDialogue("쿠치파치가 나타났다!");
            yield return new WaitForSeconds(1f);

            yield return ShowDialogue("쿠치파치: 오? 뭐야? 포켓몬 찾아? 난 포켓몬이 아니야~!");
            yield return new WaitForSeconds(1.5f);

            yield return ShowDialogue("쿠치파치가 도망쳤다!");
            yield return new WaitForSeconds(1f);

            // 포획 성공 처리
            PokemonGameController.Instance?.SetPokemonEventDone();
            trainer?.SetDefeated();

            yield return ShowDialogue("쿠치파치: 잠깐! 이건 너한테 줄게. 이걸 가져가렴~!");
            yield return new WaitForSeconds(1.5f);

            // 아이템 지급
            PokemonGameController.Instance?.GiveItem();
            if (itemObject != null) itemObject.SetActive(true);

            yield return ShowDialogue("특별한 오브제를 받았다!");
            yield return new WaitForSeconds(1.5f);

            if (cutscenePanel != null) cutscenePanel.SetActive(false);

            // 플레이어 조작 복구
            FindAnyObjectByType<PlayerMapController>()?.SetControllable(true);
        }

        IEnumerator ShowDialogue(string text)
        {
            if (dialogueText != null) dialogueText.text = text;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
