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

        // 메뉴 항목 인덱스 상수
        const int MENU_POKEMON = 0;
        const int MENU_BAG     = 1;
        const int MENU_SAVE    = 2;
        const int MENU_CLOSE   = 3;

        private bool isOpen;
        private bool isBagOpen;
        private int  currentIndex;

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
            if (MapDialogueUI.Instance != null && MapDialogueUI.Instance.IsShowing) return;

            if (!isOpen)
            {
                if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
                    OpenMenu();
                return;
            }

            // 가방 열려있으면 X로만 닫기
            if (isBagOpen)
            {
                if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
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
            else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
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
            if (menuPanel != null) menuPanel.SetActive(false);
            if (bagPanel  != null) bagPanel.SetActive(true);
            RefreshBag();
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

        void RefreshBag()
        {
            var gc = PokemonGameController.Instance;
            SetSlot(slot1Text, MapItemType.PokemonBall,  "포켓몬볼    X 1", gc);
            SetSlot(slot2Text, MapItemType.StrangeCandy, "이상한사탕  X 1", gc);
            SetSlot(slot3Text, MapItemType.Digivice,     "디지바이스  X 1", gc);
        }

        void SetSlot(TextMeshProUGUI tmp, MapItemType type, string label, PokemonGameController gc)
        {
            if (tmp == null) return;
            bool has = gc != null && gc.HasCollected(type);
            tmp.gameObject.SetActive(has);
            if (has) tmp.text = label;
        }
    }
}
