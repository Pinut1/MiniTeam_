using System.Collections;
using UnityEngine;

namespace MiniTeam.Pokemon
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        private TrainerTrigger currentTrainer;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        string L(string key) =>
            DialogueDB.Instance != null ? DialogueDB.Instance.Get(key) : $"[{key}]";

        // ── 배틀 시작/종료 ──────────────────────────

        public void StartBattle(TrainerTrigger trainer)
        {
            currentTrainer = trainer;
            FindAnyObjectByType<PlayerMapController>()?.SetControllable(false);
            BattleUIManager.Instance?.ShowBattle(trainer);
        }

        public void EndBattle()
        {
            // 승리가 아닌 경우(패배/도망) 트레이너 상태 리셋 → 재도전 가능
            if (currentTrainer != null && !currentTrainer.IsDefeated)
                currentTrainer.OnBattleEnd();

            BattleUIManager.Instance?.HideBattle();
            FindAnyObjectByType<PlayerMapController>()?.SetControllable(true);
            currentTrainer = null;
        }

        // ── 메인 선택지 ──────────────────────────────

        // 1. 싸운다 → 포켓몬 없음 → 패배
        public void OnFight()
        {
            BattleUIManager.Instance?.ShowMessage(L("battle_fight"));
            StartCoroutine(DefeatRoutine());
        }

        // 2. 가방 → 아이템 서브패널
        public void OnBag()
        {
            BattleUIManager.Instance?.ShowItemPanel();
        }

        // 6. 포켓몬 → 교체 불가 → 패배
        public void OnPokemon()
        {
            BattleUIManager.Instance?.ShowMessage(L("battle_pokemon"));
            StartCoroutine(DefeatRoutine());
        }

        // 5. 도망친다
        public void OnRun()
        {
            BattleUIManager.Instance?.ShowMessage(L("battle_run"));
            StartCoroutine(RunRoutine());
        }

        // ── 가방 아이템 선택 ─────────────────────────

        // 2-1. 몬스터볼 → 사용 불가 → 패배
        public void OnUsePokemonBall()
        {
            StartCoroutine(PokemonBallRoutine());
        }

        // 2-2. 이상한사탕 → 아구몬이 다가옴
        public void OnUseStrangeCandy()
        {
            StartCoroutine(StrangeCandyRoutine());
        }

        // 2-3. 디지바이스 → 아구몬 포획 → 승리
        public void OnUseDigivice()
        {
            StartCoroutine(DigiviceRoutine());
        }

        // ── 결과 코루틴 ──────────────────────────────

        // 패배: 블랙아웃 → Map_Dialogue_Panel이 최상위로 올라와 "눈앞이 깜깜해졌다" 표시 → 리스폰
        IEnumerator DefeatRoutine()
        {
            yield return new WaitForSeconds(1.2f);
            BattleUIManager.Instance?.ShowBlackoutNow();
            if (MapDialogueUI.Instance != null)
                yield return StartCoroutine(MapDialogueUI.Instance.Show(L("battle_defeat")));
            EndBattle();
            PokemonGameController.Instance?.RespawnPlayer();
        }

        IEnumerator RunRoutine()
        {
            yield return new WaitForSeconds(0.8f);
            EndBattle();
        }

        IEnumerator PokemonBallRoutine()
        {
            BattleUIManager.Instance?.ShowMessage(L("battle_pokemonball_1"));
            yield return new WaitForSeconds(1.2f);
            BattleUIManager.Instance?.ShowMessage(L("battle_pokemonball_2"));
            yield return new WaitForSeconds(1.2f);
            PokemonGameController.Instance?.UseItem(MapItemType.PokemonBall);
            BattleUIManager.Instance?.ShowCommandPanel();
        }

        IEnumerator StrangeCandyRoutine()
        {
            BattleUIManager.Instance?.ShowMessage(L("battle_strangecandy_1"));
            yield return new WaitForSeconds(1.5f);
            BattleUIManager.Instance?.ShowMessage(L("battle_strangecandy_2"));
            yield return new WaitForSeconds(1.5f);
            PokemonGameController.Instance?.UseItem(MapItemType.StrangeCandy);
            BattleUIManager.Instance?.ShowCommandPanel();
        }

        IEnumerator DigiviceRoutine()
        {
            BattleUIManager.Instance?.ShowMessage(L("battle_digivice_1"));
            yield return new WaitForSeconds(1.2f);
            BattleUIManager.Instance?.ShowMessage(L("battle_digivice_2"));
            yield return new WaitForSeconds(1.5f);
            PokemonGameController.Instance?.UseItem(MapItemType.Digivice);
            PokemonGameController.Instance?.SetPokemonEventDone();
            currentTrainer?.SetDefeated();
            EndBattle();
            PokemonGameController.Instance?.OnGameClear();
        }
    }
}
