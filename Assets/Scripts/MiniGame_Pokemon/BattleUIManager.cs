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
        public Vector2         cursorOffset = new Vector2(-127.61f, -3.25f); // 버튼 기준 커서 오프셋

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
        private int  activeItemCount; // 현재 보유 아이템 수 (뒤로 포함)

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
            if (isItemActive)
            {
                NavigateItem();
                return;
            }
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

            // Box(commandRects 부모) + 버튼 anchored + 오프셋 = 커서 위치
            var boxRT = commandRects[idx].parent as RectTransform;
            Vector2 boxPos = boxRT != null ? boxRT.anchoredPosition : Vector2.zero;
            commandCursor.anchoredPosition = boxPos + commandRects[idx].anchoredPosition + cursorOffset;
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
            // itemRects 순서: 보유 아이템들(PokemonBall→StrangeCandy→Digivice 순) + 뒤로
            // BattleManager에서 보유 아이템 기준으로 활성화된 것들만 itemRects에 넣어야 함
            // 마지막 인덱스 = 뒤로
            if (itemIndex == activeItemCount - 1)
            {
                HideItemPanel();
                return;
            }
            isItemActive = false;

            var gc = PokemonGameController.Instance;
            // 활성화된 아이템 순서대로 매핑
            int slot = 0;
            if (gc != null && gc.HasCollected(MapItemType.PokemonBall))
            {
                if (itemIndex == slot) { BattleManager.Instance?.OnUsePokemonBall(); return; }
                slot++;
            }
            if (gc != null && gc.HasCollected(MapItemType.StrangeCandy))
            {
                if (itemIndex == slot) { BattleManager.Instance?.OnUseStrangeCandy(); return; }
                slot++;
            }
            if (gc != null && gc.HasCollected(MapItemType.Digivice))
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

            StartCoroutine(BattleOpenRoutine());
        }

        IEnumerator BattleOpenRoutine()
        {
            isCommandActive = false;
            commandIndex = 0;

            // HP바 슬라이드 인
            if (statusPanelLeft  != null) statusPanelLeft.SlideIn();
            if (statusPanelRight != null) statusPanelRight.SlideIn();

            yield return new WaitForSeconds(0.5f);

            // 배틀 시작 대사 → Z/Enter로 닫은 후 커맨드 패널 등장
            if (MapDialogueUI.Instance != null && messageText != null)
                yield return StartCoroutine(MapDialogueUI.Instance.Show(messageText.text));

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
            if (battlePanel != null) battlePanel.SetActive(false);
            // 배틀 가방 패널도 닫기
            var bagPanel = StartMenuUI.Instance?.bagPanel;
            if (bagPanel != null) bagPanel.SetActive(false);
        }

        public void ShowMessage(string message)
        {
            isCommandActive = false;
            isItemActive    = false;
            if (messageText != null) messageText.text = message;
            if (itemPanel   != null) itemPanel.SetActive(false);
            if (commandPanel != null) commandPanel.SlideOut();
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

            // 보유 아이템 수 계산 (+ 뒤로 1개)
            var gc = PokemonGameController.Instance;
            activeItemCount = 1;
            if (gc != null)
            {
                if (gc.HasCollected(MapItemType.PokemonBall))  activeItemCount++;
                if (gc.HasCollected(MapItemType.StrangeCandy)) activeItemCount++;
                if (gc.HasCollected(MapItemType.Digivice))     activeItemCount++;
            }

            // itemRects를 가방 슬롯으로 동적 설정
            var rects = new System.Collections.Generic.List<RectTransform>();
            if (gc != null)
            {
                if (gc.HasCollected(MapItemType.PokemonBall)  && sui.slot1Text != null) rects.Add(sui.slot1Text.GetComponent<RectTransform>());
                if (gc.HasCollected(MapItemType.StrangeCandy) && sui.slot2Text != null) rects.Add(sui.slot2Text.GetComponent<RectTransform>());
                if (gc.HasCollected(MapItemType.Digivice)     && sui.slot3Text != null) rects.Add(sui.slot3Text.GetComponent<RectTransform>());
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

        public IEnumerator ShowBlackout(float duration)
        {
            if (blackoutPanel != null) blackoutPanel.SetActive(true);
            yield return new WaitForSeconds(duration);
            if (blackoutPanel != null) blackoutPanel.SetActive(false);
        }
    }
}
