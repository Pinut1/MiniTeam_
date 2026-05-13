using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniTeam.Pokemon
{
    // 배틀 UI 오버레이 패널 담당
    // Inspector에서 연결 필요: BattlePanel, TrainerImage, TrainerNameText,
    //   MessageText, FightBtn, BagBtn, PokemonBtn, RunBtn
    public class BattleUIManager : MonoBehaviour
    {
        public static BattleUIManager Instance { get; private set; }

        [Header("패널")]
        public GameObject battlePanel;

        [Header("트레이너 정보")]
        public Image           trainerImage;
        public TextMeshProUGUI trainerNameText;
        public TextMeshProUGUI trainerLevelText;

        [Header("플레이어 정보")]
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI playerLevelText;

        [Header("메시지")]
        public TextMeshProUGUI messageText;

        [Header("버튼")]
        public Button fightButton;
        public Button bagButton;
        public Button pokemonButton;
        public Button runButton;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (battlePanel != null) battlePanel.SetActive(false);

            fightButton?.onClick.AddListener(() => BattleManager.Instance?.OnFight());
            bagButton?.onClick.AddListener(() => BattleManager.Instance?.OnBag());
            pokemonButton?.onClick.AddListener(() => BattleManager.Instance?.OnPokemon());
            runButton?.onClick.AddListener(() => BattleManager.Instance?.OnRun());
        }

        public void ShowBattle(string trainerName, Sprite trainerSprite)
        {
            if (battlePanel != null) battlePanel.SetActive(true);

            if (trainerImage != null)    trainerImage.sprite  = trainerSprite;
            if (trainerNameText != null) trainerNameText.text = trainerName;
            if (trainerLevelText != null) trainerLevelText.text = "Lv. ???";
            if (playerNameText != null)  playerNameText.text  = "신태일";
            if (playerLevelText != null) playerLevelText.text = "Lv. 1";
            if (messageText != null)     messageText.text     = $"야생의 {trainerName}(이)가 나타났다!";

            SetButtonsVisible(true);
        }

        public void HideBattle()
        {
            if (battlePanel != null) battlePanel.SetActive(false);
        }

        public void ShowMessage(string message)
        {
            if (messageText != null) messageText.text = message;
            SetButtonsVisible(false);
            StartCoroutine(RestoreButtonsAfter(1.5f));
        }

        void SetButtonsVisible(bool visible)
        {
            fightButton?.gameObject.SetActive(visible);
            bagButton?.gameObject.SetActive(visible);
            pokemonButton?.gameObject.SetActive(visible);
            runButton?.gameObject.SetActive(visible);
        }

        IEnumerator RestoreButtonsAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (battlePanel != null && battlePanel.activeSelf)
                SetButtonsVisible(true);
        }
    }
}
