using System.Collections;
using UnityEngine;

namespace MiniTeam.Pokemon
{
    // TrainerTrigger를 상속하여 배틀 호환 유지
    // 좌우 패트롤 + 거리 감지로 플레이어 발견 시 대사 후 배틀 시작
    public class TrainerPatrol : TrainerTrigger
    {
        [Header("패트롤 설정")]
        public Transform leftPoint;
        public Transform rightPoint;
        public float moveSpeed = 2f;

        [Header("감지 설정")]
        public float yRange = 1.5f; // 패트롤 라인과 플레이어 y 허용 오차

        [Header("배틀 전 대사 (비어있으면 기본 생성)")]
        public string encounterDialogue = "";

        private bool isEncountered   = false;
        private bool requiresZoneExit = false; // 배틀 종료 후 구역 이탈해야 재도전 가능
        private bool movingRight      = true;
        private SpriteRenderer sr;
        private Transform playerTf;

        protected new void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTf = playerObj.transform;
        }

        void Update()
        {
            if (IsDefeated || isEncountered) return;

            bool inZone = IsPlayerInPatrolZone();

            // 배틀 후 구역을 완전히 벗어나야 재도전 허용
            if (requiresZoneExit)
            {
                if (!inZone) requiresZoneExit = false;
                Patrol();
                return;
            }

            if (inZone)
            {
                isEncountered = true;
                StartCoroutine(EncounterRoutine());
                return;
            }

            Patrol();
        }

        // TrainerTrigger의 OnTriggerEnter2D 비활성화 (패트롤 라인 감지 사용)
        new void OnTriggerEnter2D(Collider2D other) { }

        // 패배/도망 시 패트롤 재개 + 재도전 허용
        public override void OnBattleEnd()
        {
            isEncountered    = false;
            requiresZoneExit = true; // 구역 밖으로 나가야 재도전 가능
        }

        bool IsPlayerInPatrolZone()
        {
            if (playerTf == null || leftPoint == null || rightPoint == null) return false;

            float minX = Mathf.Min(leftPoint.position.x, rightPoint.position.x);
            float maxX = Mathf.Max(leftPoint.position.x, rightPoint.position.x);

            bool inX = playerTf.position.x >= minX && playerTf.position.x <= maxX;
            bool inY = Mathf.Abs(playerTf.position.y - transform.position.y) <= yRange;

            return inX && inY;
        }

        void Patrol()
        {
            if (leftPoint == null || rightPoint == null) return;

            var target = movingRight ? rightPoint.position : leftPoint.position;
            transform.position = Vector2.MoveTowards(
                transform.position, target, moveSpeed * Time.deltaTime);

            // 목표 도달 시 방향 전환
            if (Vector2.Distance(transform.position, target) < 0.05f)
            {
                movingRight = !movingRight;
                if (sr != null) sr.flipX = !movingRight;
            }
        }

        IEnumerator EncounterRoutine()
        {
            FindAnyObjectByType<PlayerMapController>()?.SetControllable(false);

            // 트레이너가 플레이어 쪽으로 스프라이트 방향 맞추기
            if (sr != null && playerTf != null)
                sr.flipX = playerTf.position.x < transform.position.x;

            string dialogue = !string.IsNullOrEmpty(encounterDialogue)
                ? encounterDialogue
                : $"{trainerName}이(가) 나타났다!";

            if (MapDialogueUI.Instance != null)
                yield return StartCoroutine(MapDialogueUI.Instance.Show(dialogue));

            // TrainerTrigger.this를 그대로 전달 (상속 관계라 호환됨)
            BattleManager.Instance?.StartBattle(this);
        }
    }
}
