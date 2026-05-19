using System.Collections;
using UnityEngine;

namespace MiniTeam.Pokemon
{
    // 맵의 트레이너(디지몬 캐릭터) 오브젝트에 부착
    // 플레이어가 근처에 오면 자동으로 배틀 시작
    public class TrainerTrigger : MonoBehaviour
    {
        [Header("트레이너 정보")]
        public string trainerName = "테일이";
        public Sprite trainerBattleSprite;

        [Header("배틀 포켓몬")]
        public string     pokemonName   = "아구몬";
        public GameObject pokemonPrefab; // 애니메이션 프리팹

        [Header("발견 대사 (비어있으면 즉시 배틀)")]
        public string encounterDialogueKey = ""; // DialogueDB 키

        private bool isDefeated = false;
        public bool IsDefeated => isDefeated;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (isDefeated) return;
            if (!other.CompareTag("Player")) return;

            if (!string.IsNullOrEmpty(encounterDialogueKey))
                StartCoroutine(EncounterWithDialogue());
            else
                BattleManager.Instance?.StartBattle(this);
        }

        IEnumerator EncounterWithDialogue()
        {
            FindAnyObjectByType<PlayerMapController>()?.SetControllable(false);
            if (MapDialogueUI.Instance != null)
            {
                string text = DialogueDB.Instance != null
                    ? DialogueDB.Instance.Get(encounterDialogueKey)
                    : encounterDialogueKey;
                yield return StartCoroutine(MapDialogueUI.Instance.Show(text));
            }
            BattleManager.Instance?.StartBattle(this);
        }

        public void SetDefeated()
        {
            isDefeated = true;
            gameObject.SetActive(false);
        }

        // 배틀이 패배/도망으로 끝났을 때 (승리 시엔 SetDefeated 호출)
        public virtual void OnBattleEnd() { }
    }
}
