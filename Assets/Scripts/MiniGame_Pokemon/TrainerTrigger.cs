using UnityEngine;

namespace MiniTeam.Pokemon
{
    // 맵의 트레이너(디지몬 캐릭터) 오브젝트에 부착
    // 플레이어가 근처에 오면 자동으로 배틀 시작
    public class TrainerTrigger : MonoBehaviour
    {
        [Header("트레이너 정보")]
        public string trainerName = "아구몬";
        public Sprite trainerBattleSprite;

        private bool isDefeated = false;
        public bool IsDefeated => isDefeated;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (isDefeated) return;
            if (!other.CompareTag("Player")) return;

            BattleManager.Instance?.StartBattle(this);
        }

        public void SetDefeated()
        {
            isDefeated = true;
            gameObject.SetActive(false);
        }
    }
}
