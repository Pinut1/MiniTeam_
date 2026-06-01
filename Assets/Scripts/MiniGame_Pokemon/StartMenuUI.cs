using System.Collections;
using UnityEngine;
using TMPro;

namespace MiniTeam.Pokemon
{
    public class StartMenuUI : MonoBehaviour
    {
        public static StartMenuUI Instance { get; private set; }

        [Header("START 메뉴 패널")]
        public GameObject menuPanel;

        [Header("메뉴 항목 (위→아래 순서대로 연결)")]
        public RectTransform[] menuItemRects; // 4개: 포켓몬, 가방, 저장, 닫기

        [Header("커서")]
        public RectTransform cursorIndicator; // ▶ 오브젝트

        [Header("가방 패널")]
        public GameObject bagPanel;
        public TextMeshProUGUI slot1Text; // 포켓몬볼
        public TextMeshProUGUI slot2Text; // 이상한사탕
        public TextMeshProUGUI slot3Text; // 디지바이스

        [Header("가방 커서")]
        public RectTransform bagCursor;

        // 메뉴 항목 인덱스 상수
        const int MENU_POKEMON = 0;
        const int MENU_BAG     = 1;
        const int MENU_SAVE    = 2;
        const int MENU_CLOSE   = 3;

        private bool isOpen;
        private bool isBagOpen;
        private int  currentIndex;
        private int  bagIndex;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (menuPanel != null) menuPanel.SetActive(false);
            if (bagPanel  != null) bagPanel.SetActive(false);
        }

        void Update()
        {
            // 배틀 중이거나 대화창 표시 중이면 메뉴 차단
            if (BattleUIManager.Instance != null && BattleUIManager.Instance.IsBattleActive) return;
            if (MapDialogueUI.Instance != null && MapDialogueUI.Instance.IsShowing) return;

            if (!isOpen)
            {
                if (Input.GetKeyDown(KeyCode.X))
                    OpenMenu();
                return;
            }

            // 가방 열려있으면 커서 네비게이션
            if (isBagOpen)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                    MoveBagCursor(-1);
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                    MoveBagCursor(1);
                else if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
                    SelectBagItem();
                else if (Input.GetKeyDown(KeyCode.X))
                    CloseBag();
                return;
            }

            // 메뉴 네비게이션
            if (Input.GetKeyDown(KeyCode.UpArrow))
                Navigate(-1);
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                Navigate(1);
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
                SelectCurrent();
            else if (Input.GetKeyDown(KeyCode.X))
                CloseAll();
        }

        void Navigate(int dir)
        {
            int len = menuItemRects != null ? menuItemRects.Length : 4;
            currentIndex = (currentIndex + dir + len) % len;
            UpdateCursor();
        }

        void SelectCurrent()
        {
            switch (currentIndex)
            {
                case MENU_POKEMON: StartCoroutine(ShowMenuDialogue("menu_pokemon")); break;
                case MENU_BAG:     OpenBag();                                        break;
                case MENU_SAVE:    StartCoroutine(ShowMenuDialogue("menu_save"));    break;
                case MENU_CLOSE:   CloseAll();                                       break;
            }
        }

        void UpdateCursor()
        {
            if (cursorIndicator == null || menuItemRects == null) return;
            int idx = Mathf.Clamp(currentIndex, 0, menuItemRects.Length - 1);
            if (menuItemRects[idx] == null) return;
            // 선택 항목의 Y 위치로 커서 이동 (X는 고정)
            Vector3 pos = cursorIndicator.position;
            pos.y = menuItemRects[idx].position.y;
            cursorIndicator.position = pos;
        }

        public void OpenMenu()
        {
            isOpen       = true;
            isBagOpen    = false;
            currentIndex = 0;
            if (menuPanel != null) menuPanel.SetActive(true);
            if (bagPanel  != null) bagPanel.SetActive(false);
            UpdateCursor();
            FindAnyObjectByType<PlayerMapController>()?.SetControllable(false);
        }

        public void CloseAll()
        {
            isOpen    = false;
            isBagOpen = false;
            if (menuPanel != null) menuPanel.SetActive(false);
            if (bagPanel  != null) bagPanel.SetActive(false);
            FindAnyObjectByType<PlayerMapController>()?.SetControllable(true);
        }

        void OpenBag()
        {
            isBagOpen = true;
            bagIndex  = 0;
            if (menuPanel != null) menuPanel.SetActive(false);
            if (bagPanel  != null) bagPanel.SetActive(true);
            RefreshBag();
            UpdateBagCursor();
        }

        void MoveBagCursor(int dir)
        {
            var slots = GetActiveBagSlots();
            if (slots.Count == 0) return;
            bagIndex = Mathf.Clamp(bagIndex + dir, 0, slots.Count - 1);
            UpdateBagCursor(slots);
        }

        void UpdateBagCursor(System.Collections.Generic.List<RectTransform> slots = null)
        {
            if (bagCursor == null) return;
            if (slots == null) slots = GetActiveBagSlots();
            if (slots.Count == 0) { bagCursor.gameObject.SetActive(false); return; }
            bagCursor.gameObject.SetActive(true);
            bagIndex = Mathf.Clamp(bagIndex, 0, slots.Count - 1);
            var pos = bagCursor.position;
            pos.y = slots[bagIndex].position.y;
            bagCursor.position = pos;
        }

        System.Collections.Generic.List<RectTransform> GetActiveBagSlots()
        {
            var list = new System.Collections.Generic.List<RectTransform>();
            if (slot1Text != null && slot1Text.gameObject.activeSelf) list.Add(slot1Text.rectTransform);
            if (slot2Text != null && slot2Text.gameObject.activeSelf) list.Add(slot2Text.rectTransform);
            if (slot3Text != null && slot3Text.gameObject.activeSelf) list.Add(slot3Text.rectTransform);
            return list;
        }

        void SelectBagItem()
        {
            var slots = GetActiveBagSlots();
            if (slots.Count == 0)
                StartCoroutine(ShowMenuDialogue("menu_bag_empty"));
        }

        void CloseBag()
        {
            isBagOpen = false;
            if (bagPanel  != null) bagPanel.SetActive(false);
            if (menuPanel != null) menuPanel.SetActive(true);
            UpdateCursor();
        }

        IEnumerator ShowMenuDialogue(string key)
        {
            CloseAll();
            var text = DialogueDB.Instance != null ? DialogueDB.Instance.Get(key) : $"[{key}]";
            if (MapDialogueUI.Instance != null)
                yield return StartCoroutine(MapDialogueUI.Instance.Show(text));
        }

        // 배틀에서 가방 열 때 호출 (BattleUIManager에서 사용)
        public void RefreshBagForBattle() => RefreshBag();

        void RefreshBag()
        {
            var gc = PokemonGameController.Instance;
            SetSlot(slot1Text, MapItemType.PokemonBall,  "포켓몬볼    X 1", "포켓몬볼    X 0", gc);
            SetSlot(slot2Text, MapItemType.StrangeCandy, "이상한사탕  X 1", "이상한사탕  X 0", gc);
            SetSlot(slot3Text, MapItemType.Digivice,     "디지바이스  X 1", "디지바이스  X 0", gc);
        }

        void SetSlot(TextMeshProUGUI tmp, MapItemType type, string label, string label0, PokemonGameController gc)
        {
            if (tmp == null) return;
            bool has  = gc != null && gc.HasCollected(type);
            bool used = gc != null && gc.HasUsed(type);
            tmp.gameObject.SetActive(has);
            if (has) tmp.text = used ? label0 : label;
        }
    }
}
