using UnityEngine;

namespace MiniTeam.Pokemon
{
    // 배틀 4가지 선택지 로직 담당
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        private TrainerTrigger currentTrainer;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartBattle(TrainerTrigger trainer)
        {
            currentTrainer = trainer;
            FindAnyObjectByType<PlayerMapController>()?.SetControllable(false);
            BattleUIManager.Instance?.ShowBattle(trainer.trainerName, trainer.trainerBattleSprite);
        }

        public void EndBattle()
        {
            BattleUIManager.Instance?.HideBattle();
            FindAnyObjectByType<PlayerMapController>()?.SetControllable(true);
            currentTrainer = null;
        }

        // ── 선택지 ────────────────────────────────

        // 싸우다: 레벨차이로 즉사 → 시작지점 리스폰
        public void OnFight()
        {
            BattleUIManager.Instance?.ShowMessage("레벨 차이가 너무 나서 쓰러졌다...");
            Invoke(nameof(RespawnAndEnd), 1.5f);
        }

        void RespawnAndEnd()
        {
            PokemonGameController.Instance?.RespawnPlayer();
            EndBattle();
        }

        // 가방: 텅 비어있음
        public void OnBag()
        {
            BattleUIManager.Instance?.ShowMessage("가방이 텅 비어있다.");
        }

        // 포켓몬: 첫 번째 = 쿠치파치 컷씬, 이후 = 빈 목록
        public void OnPokemon()
        {
            var gc = PokemonGameController.Instance;
            if (gc == null) return;

            if (!gc.IsPokemonEventDone)
            {
                var trainer = currentTrainer;
                EndBattle();
                CutsceneManager.Instance?.PlayKuchipachScene(trainer);
            }
            else
            {
                BattleUIManager.Instance?.ShowMessage("포켓몬 목록이 텅 비어있다.");
            }
        }

        // 도망치다: 배틀 종료, 맵으로 복귀
        public void OnRun()
        {
            BattleUIManager.Instance?.ShowMessage("도망쳤다!");
            Invoke(nameof(EndBattle), 0.8f);
        }
    }
}
