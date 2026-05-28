using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniTeam.Pokemon
{
    public class BattleUIManager : MonoBehaviour
    {
        public static BattleUIManager Instance { get; private set; }

        [Header("패널")]
        public GameObject battlePanel;

        [Header("슬라이더 연결 (UIPanelSlider 붙인 패널)")]
        public UIPanelSlider statusPanelLeft;
        public UIPanelSlider statusPanelRight;
        public UIPanelSlider commandPanel;
        public UIPanelSlider enemySlider; // 트레이너 스프라이트 슬라이드 아웃용

        [Header("포켓몬 소환")]
        public RectTransform pokemonSpawnPoint;
        public Vector3       pokemonScale = new Vector3(3f, 3f, 1f);

        [Header("트레이너 정보")]
        public Image           trainerImage;
        public TextMeshProUGUI trainerNameText;
        public TextMeshProUGUI trainerLevelText;

        [Header("플레이어 정보")]
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI playerLevelText;

        [Header("메시지")]
        public TextMeshProUGUI messageText;

        [Header("커맨드 버튼 (Grid 순서대로: 싸우다/가방/포켓몬/도망치다)")]
        public RectTransform[] commandRects; // 4개, 커서 위치 기준
        public RectTransform   commandCursor;

        [Header("아이템 선택 패널")]
        public GameObject      itemPanel;
        public RectTransform[] itemRects;   // 보유 아이템 + 뒤로 버튼 순서대로
        public RectTransform   itemCursor;

        [Header("블랙아웃")]
        public GameObject blackoutPanel;

        // 배틀 활성 상태 (StartMenuUI에서 참조)
        public bool IsBattleActive { get; private set; }

        // 커맨드 상태
        private bool isCommandActive;
        private int  commandIndex;

        // 아이템 패널 상태
        private bool isItemActive;
        private int  itemIndex;
        private int  activeItemCount;

        // 메시지 입력 대기 상태
        private bool waitingConfirm;

        // 소환된 포켓몬 프리팹 인스턴스
        private GameObject spawnedPokemon;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (battlePanel   != null) battlePanel.SetActive(false);
            if (itemPanel     != null) itemPanel.SetActive(false);
            if (blackoutPanel != null) blackoutPanel.SetActive(false);
        }

        void Update()
        {
            if (waitingConfirm)
            {
                if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                    waitingConfirm = false;
                return;
            }
            if (isItemActive) { NavigateItem(); return; }
            if (isCommandActive) NavigateCommand();
        }

        // ── 커맨드 네비게이션 (2x2 그리드) ─────────────

        void NavigateCommand()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))       MoveCommandGrid(0, -1);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) MoveCommandGrid(0,  1);
            else if (Input.GetKeyDown(KeyCode.UpArrow))    MoveCommandGrid(-1, 0);
            else if (Input.GetKeyDown(KeyCode.DownArrow))  MoveCommandGrid( 1, 0);
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
                SelectCommand();
        }

        // 2x2 그리드 기반 이동 (rowDelta, colDelta)
        // 배열 순서: [0]=싸우다(0,0) [1]=가방(0,1) [2]=포켓몬(1,0) [3]=도망(1,1)
        void MoveCommandGrid(int rowDelta, int colDelta)
        {
            int row = commandIndex / 2;
            int col = commandIndex % 2;
            int newRow = row + rowDelta;
            int newCol = col + colDelta;
            if (newRow < 0 || newRow > 1 || newCol < 0 || newCol > 1) return;
            commandIndex = newRow * 2 + newCol;
            UpdateCommandCursor();
        }

        void SelectCommand()
        {
            isCommandActive = false;
            switch (commandIndex)
            {
                case 0: BattleManager.Instance?.OnFight();   break;
                case 1: BattleManager.Instance?.OnBag();     break;
                case 2: BattleManager.Instance?.OnPokemon(); break;
                case 3: BattleManager.Instance?.OnRun();     break;
            }
        }

        void UpdateCommandCursor()
        {
            if (commandCursor == null || commandRects == null) return;
            int idx = Mathf.Clamp(commandIndex, 0, commandRects.Length - 1);
            if (commandRects[idx] == null) return;

            commandCursor.position = commandRects[idx].position;
        }

        // ── 아이템 패널 네비게이션 (세로 목록) ──────────

        void NavigateItem()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))   MoveItem(-1);
            else if (Input.GetKeyDown(KeyCode.DownArrow)) MoveItem(1);
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
                SelectItem();
            else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
                HideItemPanel();
        }

        void MoveItem(int delta)
        {
            itemIndex = Mathf.Clamp(itemIndex + delta, 0, activeItemCount - 1);
            UpdateItemCursor();
        }

        void SelectItem()
        {
            isItemActive = false;

            var gc = PokemonGameController.Instance;
            // ShowItemPanel과 동일한 조건 (보유 + 미사용)으로 슬롯 매핑
            int slot = 0;
            if (gc != null && gc.HasCollected(MapItemType.PokemonBall) && !gc.HasUsed(MapItemType.PokemonBall))
            {
                if (itemIndex == slot) { BattleManager.Instance?.OnUsePokemonBall(); return; }
                slot++;
            }
            if (gc != null && gc.HasCollected(MapItemType.StrangeCandy) && !gc.HasUsed(MapItemType.StrangeCandy))
            {
                if (itemIndex == slot) { BattleManager.Instance?.OnUseStrangeCandy(); return; }
                slot++;
            }
            if (gc != null && gc.HasCollected(MapItemType.Digivice) && !gc.HasUsed(MapItemType.Digivice))
            {
                if (itemIndex == slot) { BattleManager.Instance?.OnUseDigivice(); return; }
            }
        }

        void UpdateItemCursor()
        {
            if (itemCursor == null || itemRects == null || itemRects.Length == 0) return;
            int idx = Mathf.Clamp(itemIndex, 0, itemRects.Length - 1);
            if (itemRects[idx] == null) return;
            Vector3 p = itemCursor.position;
            p.y = itemRects[idx].position.y;
            itemCursor.position = p;
        }

        // ── 배틀 표시/숨김 ────────────────────────────

        public void ShowBattle(TrainerTrigger trainer)
        {
            IsBattleActive = true;
            if (battlePanel != null) battlePanel.SetActive(true);

            // 처음엔 트레이너(테일이) 스프라이트 표시
            if (trainerImage != null)     trainerImage.sprite   = trainer.trainerBattleSprite;
            if (trainerNameText != null)  trainerNameText.text  = trainer.pokemonName;
            if (trainerLevelText != null) trainerLevelText.text = "Lv.???";
            if (playerNameText != null)   playerNameText.text   = "개발자";
            if (playerLevelText != null)  playerLevelText.text  = "Lv.1";
            if (messageText != null)      messageText.text      = $"{trainer.trainerName}이(가) 아구몬을 내보냈다!";

            // Enemy 슬라이더를 visible 위치로 즉시 리셋 (이전 배틀에서 비활성화됐을 수 있음)
            if (enemySlider != null) enemySlider.SlideIn(instant: true);

            StartCoroutine(BattleOpenRoutine(trainer));
        }

        // 하위 호환 (trainerName/Sprite만 있을 경우)
        public void ShowBattle(string trainerName, Sprite trainerSprite)
        {
            IsBattleActive = true;
            if (battlePanel != null) battlePanel.SetActive(true);
            if (trainerImage != null)     trainerImage.sprite   = trainerSprite;
            if (trainerNameText != null)  trainerNameText.text  = trainerName;
            if (trainerLevelText != null) trainerLevelText.text = "Lv. ???";
            if (playerNameText != null)   playerNameText.text   = "신태일";
            if (playerLevelText != null)  playerLevelText.text  = "Lv. 1";
            if (messageText != null)      messageText.text      = $"야생의 {trainerName}(이)가 나타났다!";
            StartCoroutine(BattleOpenRoutine(null));
        }

        IEnumerator BattleOpenRoutine(TrainerTrigger trainer)
        {
            isCommandActive = false;
            commandIndex = 0;

            if (statusPanelLeft  != null) statusPanelLeft.SlideIn();
            if (statusPanelRight != null) statusPanelRight.SlideIn();

            yield return new WaitForSeconds(0.5f);

            // 트레이너 배틀: 트레이너 스프라이트 슬라이드 아웃 → 포켓몬 프리팹 소환
            if (trainer != null && trainer.pokemonPrefab != null)
            {
                // "테일이가 아구몬을 내보냈다!" 대기
                yield return StartCoroutine(ShowMessageAndWait(messageText.text));

                // 트레이너 스프라이트 슬라이드 아웃 (비활성화 없이 off-screen 유지)
                if (enemySlider != null)
                    yield return StartCoroutine(enemySlider.SlideOutRoutine(deactivateAfter: false));

                // 포켓몬 UI 프리팹 소환 (Battle_Panel 자식으로)
                if (pokemonSpawnPoint != null)
                {
                    spawnedPokemon = Instantiate(trainer.pokemonPrefab, battlePanel.transform);
                    var rt = spawnedPokemon.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchoredPosition = pokemonSpawnPoint.anchoredPosition;
                        rt.sizeDelta        = pokemonSpawnPoint.sizeDelta;
                        rt.localScale       = pokemonScale;
                    }
                }

                if (messageText != null) messageText.text = $"상대방의 {trainer.pokemonName}!";
                yield return StartCoroutine(ShowMessageAndWait(messageText.text));
            }
            else
            {
                yield return StartCoroutine(ShowMessageAndWait(messageText.text));
            }

            if (commandPanel != null)
                yield return StartCoroutine(commandPanel.SlideInRoutine());

            isCommandActive = true;
            UpdateCommandCursor();
        }

        public void HideBattle()
        {
            IsBattleActive  = false;
            isCommandActive = false;
            isItemActive    = false;
            waitingConfirm  = false;
            if (battlePanel != null) battlePanel.SetActive(false);
            HideBlackout();
            if (spawnedPokemon != null) { Destroy(spawnedPokemon); spawnedPokemon = null; }
            var bagPanel = StartMenuUI.Instance?.bagPanel;
            if (bagPanel != null) bagPanel.SetActive(false);
        }

        // 텍스트를 세팅하고 Z/Space/Enter 입력을 기다림 (대화창은 계속 표시 상태)
        public IEnumerator ShowMessageAndWait(string message)
        {
            isCommandActive = false;
            isItemActive    = false;
            if (messageText  != null) messageText.text = message;
            if (itemPanel    != null) itemPanel.SetActive(false);
            if (commandPanel != null && commandPanel.gameObject.activeInHierarchy)
                commandPanel.SlideOut();

            yield return new WaitForSeconds(0.3f); // 입력 씹힘 방지
            waitingConfirm = true;
            yield return new WaitUntil(() => !waitingConfirm);
        }

        public void ShowMessage(string message)
        {
            isCommandActive = false;
            isItemActive    = false;
            if (messageText != null) messageText.text = message;
            if (itemPanel   != null) itemPanel.SetActive(false);
            if (commandPanel != null && commandPanel.gameObject.activeInHierarchy)
                commandPanel.SlideOut();
        }

        public void ShowCommandPanel()
        {
            commandIndex = 0;
            if (commandPanel != null)
                StartCoroutine(ShowCommandRoutine());
            else
                isCommandActive = true;
        }

        IEnumerator ShowCommandRoutine()
        {
            yield return StartCoroutine(commandPanel.SlideInRoutine());
            isCommandActive = true;
            UpdateCommandCursor();
        }

        public void ShowItemPanel()
        {
            isCommandActive = false;
            if (commandPanel != null) commandPanel.SlideOut(instant: true, deactivateAfter: false);

            // StartMenuUI의 가방 패널 재활용
            var sui = StartMenuUI.Instance;
            if (sui == null) return;

            sui.RefreshBagForBattle(); // 보유 아이템만 표시
            sui.bagPanel.SetActive(true);

            itemIndex = 0;

            // itemRects를 보유 아이템 슬롯으로 동적 설정
            var gc = PokemonGameController.Instance;
            var rects = new System.Collections.Generic.List<RectTransform>();
            if (gc != null)
            {
                // 보유하고 아직 사용 안 한 아이템만 커서 대상
                if (gc.HasCollected(MapItemType.PokemonBall)  && !gc.HasUsed(MapItemType.PokemonBall)  && sui.slot1Text != null) rects.Add(sui.slot1Text.GetComponent<RectTransform>());
                if (gc.HasCollected(MapItemType.StrangeCandy) && !gc.HasUsed(MapItemType.StrangeCandy) && sui.slot2Text != null) rects.Add(sui.slot2Text.GetComponent<RectTransform>());
                if (gc.HasCollected(MapItemType.Digivice)     && !gc.HasUsed(MapItemType.Digivice)     && sui.slot3Text != null) rects.Add(sui.slot3Text.GetComponent<RectTransform>());
            }

            // 아이템 없으면 패널 열지 않고 메시지 처리
            if (rects.Count == 0)
            {
                sui.bagPanel.SetActive(false);
                ShowMessage(DialogueDB.Instance != null ? DialogueDB.Instance.Get("menu_bag_empty") : "가방이 비어있어.");
                if (commandPanel != null) StartCoroutine(ShowCommandRoutine());
                return;
            }

            itemRects = rects.ToArray();
            activeItemCount = rects.Count;
            if (itemCursor == null)
                Debug.LogWarning("[BattleUIManager] itemCursor가 연결되지 않아 커서를 표시할 수 없습니다.");
            isItemActive = true;
            UpdateItemCursor();
        }

        public void HideItemPanel()
        {
            isItemActive = false;
            var bagPanel = StartMenuUI.Instance?.bagPanel;
            if (bagPanel != null) bagPanel.SetActive(false);
            ShowCommandPanel();
        }

        public void ShowBlackoutNow()
        {
            if (blackoutPanel == null) return;
            blackoutPanel.SetActive(true);
            // 맵 대화창(Map_Dialogue_Panel)이 blackout 위에 렌더링되도록 최상위로 이동
            if (MapDialogueUI.Instance?.panel != null)
                MapDialogueUI.Instance.panel.transform.SetAsLastSibling();
        }

        public void HideBlackout()
        {
            if (blackoutPanel != null) blackoutPanel.SetActive(false);
        }

        // 짧은 연출용 (블랙아웃만 단독 사용할 때)
        public IEnumerator ShowBlackout(float duration)
        {
            ShowBlackoutNow();
            yield return new WaitForSeconds(duration);
            HideBlackout();
        }
    }
}
