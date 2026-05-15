using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 모든 UI패널 표시/숨김과 키 입력을 담당
/// 1. ESC/Q/TAB 키 입력 감지 -> 현재 상태에 맞게 처리
/// 2. 증거 패널 열기/닫기
/// 3. 옵션 패널 열기/닫기
/// 4. 화면 클릭 -> DialogueManager 또는 CrossExaminationManager에 전달
/// 5. 증거 버튼 목록 생성
/// </summary>
public class SpongeUIManager : MonoBehaviour
{
    public static SpongeUIManager Instance { get; private set; }
    
    [Header("증거 패널")]
    [SerializeField] private GameObject evidencePnl; // 증거 목록 전체를 감싸는 패널
    [Header("증거 슬롯")]
    // 씬에 이미 배치된 버튼들을 직접 참조 연결
    [SerializeField] private SpongeEvidenceButtonUI[] evidenceSlots;

    [Header("증거 상세 이미지")]
    [SerializeField] private Image holderImg;

    //[Header("옵션 패널")]
    //[SerializeField] private GameObject opitionsPnl; // 메인 UI 완성시 연결 예정

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        evidencePnl.SetActive(false);
        // opitionsPnl.SetActive(true);
    }

    // ── 이벤트 구독 ──────────────────────────────────────────────
    // OnEnable : 이 오브젝트가 활성화될 때 이벤트 구독
    // 이벤트 기반으로 UI 갱신 — Update() 없이 상태가 바뀔 때만 호출됨
    private void OnEnable()
    {
        SpongeEvidenceManager.OnEvidenceSelected += HandleEvidenceSelected;
        SpongeEvidenceManager.OnEvidenceListChanged += RebuildEvidenceSlots;
    }

    private void OnDisable()
    {
        SpongeEvidenceManager.OnEvidenceSelected -= HandleEvidenceSelected;
        SpongeEvidenceManager.OnEvidenceListChanged -= RebuildEvidenceSlots;
    }

    // ── 키 입력 처리 ─────────────────────────────────────────────
    private void Update()
    {
        if (SpongeGameManager.Instance.IsInputBlocked()) return;

        //if (Input.GetKeyDown(KeyCode.Escape))
            //ToggleOptionPanel();
        if (Input.GetKeyDown(KeyCode.Tab) && SpongeGameManager.Instance.CanOpenEvidence)
            TryOpenEvidencePanel();
        if (Input.GetKeyDown(KeyCode.Q) && SpongeGameManager.Instance.CanPress())
            SpongeCrossExaminationManager.Instance.PressWitness();
        if (SpongeGameManager.Instance.CurrentState == SpongeGameState.GameState.CrossExamination)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                SpongeCrossExaminationManager.Instance.NextLine();
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                SpongeCrossExaminationManager.Instance.PrevLine();
        }
        if (Input.GetMouseButtonDown(0))
            HandleScreenClick();

    }

    // ── 화면 클릭 처리 ───────────────────────────────────────────
    /// <summary>
    /// 화면 클릭 처리 / 마우스 왼쪽 클릭시 현재 상태에 따라 다른 동작 실행
    /// </summary>
    void HandleScreenClick()
    {
        // 증거 패널이나 옵션 패널이 열려있으면 클릭 무시 / if문 괄호 안에 || optionsPnl.activeSelf 추가하기
        if (evidencePnl.activeSelf) return;

        switch (SpongeGameManager.Instance.CurrentState)
        {
            // 대사중 클릭 -> 타이핑 스킵 or 다음 대사
            case SpongeGameState.GameState.Dialogue:
            case SpongeGameState.GameState.Pressing:
            case SpongeGameState.GameState.EvidenceSelect: 
                SpongeDialogueManager.Instance.OnScreenClick();
                break;
            // 심문중 클릭 -> 다음 증언으로 이동
            case SpongeGameState.GameState.CrossExamination: 
                SpongeCrossExaminationManager.Instance.NextLine();
                break;
        }
    }

    // ── 증거 패널 ────────────────────────────────────────────────
    /// <summary>
    /// 증거 패널 열기 / TAB 입력 또는 선택지에서 호출
    /// </summary>
    public void TryOpenEvidencePanel()
    {
        if (evidencePnl.activeSelf)
        {
            CloseEvidencePanel();
            return;
        }

        // 증거 선택 상태로 전환
        SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.EvidenceSelect);

        RebuildEvidenceSlots();
        evidencePnl.SetActive(true);
    }

    /// <summary>
    /// 씬에 배치된 슬롯 10개 상태 갱신
    /// 데이터 있음 == Filled, 없으면 == Empty
    /// </summary>
    void RebuildEvidenceSlots()
    {
        SpongeEvidenceData[] evidences = SpongeEvidenceManager.Instance.GetAllEvidences();

        for (int i = 0; i < evidenceSlots.Length; i++)
        {
            SpongeEvidenceData data = i < evidences.Length ? evidences[i] : null;
            evidenceSlots[i].Setup(data);

            // 클릭 이벤트 연결
            if (data != null)
            {
                string evidenceId = data.id;
                evidenceSlots[i].GetComponent<Button>().onClick.RemoveAllListeners();
                evidenceSlots[i].GetComponent<Button>().onClick.AddListener(() => SpongeEvidenceManager.Instance.PresentEvidence(evidenceId));
            }
        }
    }


    /// <summary>
    /// 증거가 선택됐을 때 이벤트로 호출 (SpongeEvidenceManager.OnEvidenceSelected)
    /// 선택된 버튼만 Highlight 상태로 변경, 나머지는 Filled로 복귀
    /// </summary>
    /// <param name="evidenceId"></param>
    void HandleEvidenceSelected(string evidenceId)
    {
        foreach (var btn in evidenceSlots)
            btn.SetHighlight(btn.EvidenceId == evidenceId);

        SpongeEvidenceData data = SpongeEvidenceManager.Instance.GetById(evidenceId);
        if (data != null && data.icon != null)
        {
            holderImg.sprite = data.icon;
            holderImg.gameObject.SetActive(true);
        }
        else
        {
            holderImg.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 증거 패널 닫기
    /// </summary>
    public void CloseEvidencePanel()
    {
        evidencePnl.SetActive(false);
        if (SpongeGameManager.Instance.CurrentState == SpongeGameState.GameState.EvidenceSelect)
            SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.CrossExamination);
    }

    /*
    // ── 옵션 패널 ────────────────────────────────────────────────
    /// <summary>
    /// 옵션 패널 열기/닫기
    /// </summary>
    public void ToggleOptionsPanel()
    {
        optionsPnl.SetActive(!optionsPnl.activeSelf);
    }
   
    /// <summary>
    /// 옵션 패널 닫기
    /// </summary>
    public void CloseOptionsPanel()
    {
        optionsPnl.SetActive(false);
    } 
    */
}
