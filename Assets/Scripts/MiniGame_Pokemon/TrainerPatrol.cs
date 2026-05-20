using System.Collections;
using UnityEngine;

namespace MiniTeam.Pokemon
{
    // 출구 앞 대기 → 직선 시야 감지 → 플레이어에게 직선 접근 → 배틀
    public class TrainerPatrol : TrainerTrigger
    {
        [Header("시야 설정")]
        public float detectionRange  = 5f;   // 감지 최대 거리
        public float sightTolerance  = 0.5f; // 직선 허용 오차 (수평/수직)

        [Header("이동 설정")]
        public float moveSpeed       = 3f;
        public float battleDistance  = 0.8f; // 이 거리 이내면 배틀 시작

        [Header("배틀 전 대사 (비어있으면 encounterDialogueKey 사용)")]
        public string encounterDialogue = "";

        private enum State { Idle, Chasing, Battling }
        private State state = State.Idle;

        private Vector3    startPosition;
        private SpriteRenderer sr;
        private Transform  playerTf;

        protected new void Start()
        {
            startPosition = transform.position;
            sr = GetComponent<SpriteRenderer>();
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTf = playerObj.transform;
        }

        void Update()
        {
            if (IsDefeated || state == State.Battling) return;

            if (state == State.Chasing)
            {
                ChasePlayer();
                return;
            }

            if (HasLineOfSight())
            {
                state = State.Chasing;
                FindAnyObjectByType<PlayerMapController>()?.SetControllable(false);
            }
        }

        // 부모 OnTriggerEnter2D 무력화 (직선 시야 감지로 대체)
        new void OnTriggerEnter2D(Collider2D other) { }

        bool HasLineOfSight()
        {
            if (playerTf == null) return false;
            Vector2 diff = (Vector2)(playerTf.position - transform.position);
            if (diff.magnitude > detectionRange) return false;

            bool horizontal = Mathf.Abs(diff.y) <= sightTolerance;
            bool vertical   = Mathf.Abs(diff.x) <= sightTolerance;
            return horizontal || vertical;
        }

        void ChasePlayer()
        {
            if (playerTf == null) return;

            Vector2 to = (Vector2)(playerTf.position - transform.position);
            if (sr != null) sr.flipX = to.x < 0;

            transform.position = Vector2.MoveTowards(
                transform.position, playerTf.position, moveSpeed * Time.deltaTime);

            if (to.magnitude <= battleDistance)
            {
                state = State.Battling;
                StartCoroutine(EncounterRoutine());
            }
        }

        // 도망/패배 후 원래 위치로 복귀, 플레이어를 감지 범위 아래로 밀어냄
        public override void OnBattleEnd()
        {
            state = State.Idle;
            transform.position = startPosition;
            if (playerTf != null)
                playerTf.position = startPosition + Vector3.down * (detectionRange + 1f);
        }

        IEnumerator EncounterRoutine()
        {
            if (sr != null && playerTf != null)
                sr.flipX = playerTf.position.x < transform.position.x;

            string dialogue;
            if (!string.IsNullOrEmpty(encounterDialogueKey) && DialogueDB.Instance != null)
                dialogue = DialogueDB.Instance.Get(encounterDialogueKey);
            else if (!string.IsNullOrEmpty(encounterDialogue))
                dialogue = encounterDialogue;
            else
                dialogue = $"{trainerName}이(가) 나타났다!";

            if (MapDialogueUI.Instance != null)
                yield return StartCoroutine(MapDialogueUI.Instance.Show(dialogue));

            BattleManager.Instance?.StartBattle(this);
        }
    }
}
