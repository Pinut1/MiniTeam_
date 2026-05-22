using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField] private GameObject evidencePnl;
    [SerializeField] private TMP_Text evidenceNameTxt;
    [SerializeField] private TMP_Text evidenceDescriptionTxt;
    [Header("증거 슬롯")]
    // 씬에 이미 배치된 버튼들을 직접 참조 연결
    [SerializeField] private SpongeEvidenceButtonUI[] evidenceSlots;

    [Header("증거 상세 이미지")]
    [SerializeField] private GameObject holderBurger;
    [SerializeField] private GameObject holderBread;
    [SerializeField] private GameObject holderRecorder;
    [SerializeField] private GameObject holderPoster;
    [SerializeField] private GameObject holderReceipt;
    [SerializeField] private GameObject holderStatement;

    private Dictionary<string, GameObject> holderMap;
    private int selectedSlotIndex = 0;
    private SpongeGameState.GameState stateBeforeEvidence;

    [Header("메뉴 이미지")]
    [SerializeField] private GameObject menuDefault;
    [SerializeField] private GameObject menuCrossExam;

    [Header("추궁 힌트 패널")]
    [SerializeField] private GameObject pressStartPnlLeft;
    [SerializeField] private GameObject pressStartPnlRight;
    [SerializeField] private Animator pressStartAnimLeft;
    [SerializeField] private Animator pressStartAnimRight;

    [Header("심문 중 표시")]
    [SerializeField] private GameObject pressingImgObj;
    [SerializeField] private Animator pressingImgAnim;

    [Header("추궁 연출")]
    [SerializeField] private GameObject whitePnl;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private GameObject holditObj;
    [SerializeField] private Animator holditAnim;

    [Header("증거 제시 연출")]
    [SerializeField] private GameObject objectionObj;
    [SerializeField] private Animator objectionAnim;

    [Header("레코드 패널 증거 제시 연출")]
    [SerializeField] private GameObject takeThatObj;
    [SerializeField] private Animator takeThatAnim;

    [Header("심문 시작 패널")]
    [SerializeField] private GameObject questionPnlLeft;
    [SerializeField] private GameObject questionPnlRight;
    [SerializeField] private Animator questionAnimLeft;
    [SerializeField] private Animator questionAnimRight;

    [Header("레코드 패널 (대사 직접 증거 선택)")]
    [SerializeField] private GameObject recordPnl;

    private bool questionPnlShown = false;
    private bool isRecordPanelMode = false;
    private SpongeDialogueLine recordPanelSourceLine;
    public bool IsPlayingHoldit { get; private set; }
    public bool IsPlayingObjection { get; private set; }
    public bool IsPlayingTakeThat { get; private set; }

    //[Header("옵션 패널")]
    //[SerializeField] private GameObject opitionsPnl; // 메인 UI 완성시 연결 예정

    private void Awake()
    {
        Instance = this;
        holderMap = new Dictionary<string, GameObject>
        {
            { "burger",    holderBurger    },
            { "bread",     holderBread     },
            { "recorder",  holderRecorder  },
            { "poster",    holderPoster    },
            { "receipt",   holderReceipt   },
            { "bankstatement", holderStatement }
        };
    }

    private void Start()
    {
        evidencePnl.SetActive(false);
        if (recordPnl != null) recordPnl.SetActive(false);
        // opitionsPnl.SetActive(true);
    }

    // ── 이벤트 구독 ──────────────────────────────────────────────
    // OnEnable : 이 오브젝트가 활성화될 때 이벤트 구독
    // 이벤트 기반으로 UI 갱신 — Update() 없이 상태가 바뀔 때만 호출됨
    private void OnEnable()
    {
        SpongeEvidenceManager.OnEvidenceSelected += HandleEvidenceSelected;
        SpongeEvidenceManager.OnEvidenceListChanged += RebuildEvidenceSlots;
        SpongeGameManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        SpongeEvidenceManager.OnEvidenceSelected -= HandleEvidenceSelected;
        SpongeEvidenceManager.OnEvidenceListChanged -= RebuildEvidenceSlots;
        SpongeGameManager.OnStateChanged -= HandleStateChanged;
    }

    // ── 상태 변화 처리 ───────────────────────────────────────────
    void HandleStateChanged(SpongeGameState.GameState newState)
    {
        if (newState == SpongeGameState.GameState.Resolution)
        {
            if (menuDefault != null)  menuDefault.SetActive(false);
            if (menuCrossExam != null) menuCrossExam.SetActive(false);
            return;
        }
        if (newState == SpongeGameState.GameState.EvidenceSelect) return;
        bool isCrossExam = newState == SpongeGameState.GameState.CrossExamination;
        if (menuDefault != null)  menuDefault.SetActive(!isCrossExam);
        if (menuCrossExam != null) menuCrossExam.SetActive(isCrossExam);

        if (newState == SpongeGameState.GameState.Testifying)
        {
            questionPnlShown = false;
            if (pressStartPnlLeft != null)
            {
                pressStartPnlLeft.SetActive(true);
                pressStartAnimLeft?.Play("UI_PressStartPnlLeft");
            }
            if (pressStartPnlRight != null)
            {
                pressStartPnlRight.SetActive(true);
                pressStartAnimRight?.Play("UI_PressStartPnlRight");
            }
            if (pressingImgObj != null)
            {
                pressingImgObj.SetActive(true);
                pressingImgAnim?.Play("PressingLoof");
            }
        }

        if (newState == SpongeGameState.GameState.CrossExamination)
        {
            pressingImgObj?.SetActive(false);

            if (!questionPnlShown)
            {
                questionPnlShown = true;
                if (questionPnlLeft != null)
                {
                    questionPnlLeft.SetActive(true);
                    questionAnimLeft?.Play("UI_QuestionStartPnlLeft");
                }
                if (questionPnlRight != null)
                {
                    questionPnlRight.SetActive(true);
                    questionAnimRight?.Play("UI_QuestionStartPnlRight");
                }
            }
        }
    }

    // ── 추궁 연출 ────────────────────────────────────────────────
    public IEnumerator PlayHolditAnim()
    {
        IsPlayingHoldit = true;
        whitePnl.SetActive(true);
        yield return new WaitForSeconds(flashDuration);
        whitePnl.SetActive(false);
        holditObj.SetActive(true);
        holditAnim.Play("HoldItAnim");
        yield return null;
        yield return new WaitForSeconds(holditAnim.GetCurrentAnimatorStateInfo(0).length);
        holditObj.SetActive(false);
        IsPlayingHoldit = false;
    }

    public IEnumerator PlayObjectionAnim()
    {
        IsPlayingObjection = true;
        whitePnl.SetActive(true);
        yield return new WaitForSeconds(flashDuration);
        whitePnl.SetActive(false);
        objectionObj.SetActive(true);
        objectionAnim.Play("HoldItAnim");
        yield return null;
        yield return new WaitForSeconds(objectionAnim.GetCurrentAnimatorStateInfo(0).length);
        objectionObj.SetActive(false);
        IsPlayingObjection = false;
    }

    public IEnumerator PlayTakeThatAnim()
    {
        IsPlayingTakeThat = true;
        whitePnl.SetActive(true);
        yield return new WaitForSeconds(flashDuration);
        whitePnl.SetActive(false);
        takeThatObj.SetActive(true);
        takeThatAnim.Play("TakeThatAnim");
        yield return null;
        yield return new WaitForSeconds(takeThatAnim.GetCurrentAnimatorStateInfo(0).length);
        takeThatObj.SetActive(false);
        IsPlayingTakeThat = false;
    }

    IEnumerator PresentEvidenceSequence(string evidenceId)
    {
        CloseEvidencePanel();
        yield return StartCoroutine(PlayObjectionAnim());
        SpongeEvidenceManager.Instance.PresentEvidence(evidenceId);
    }

    // ── 키 입력 처리 ─────────────────────────────────────────────
    private void Update()
    {
        if (SpongeGameManager.Instance.IsInputBlocked()) return;

        if (SpongeDialogueManager.Instance.IsChoiceActive)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))   SpongeDialogueManager.Instance.NavigateChoice(-1);
            if (Input.GetKeyDown(KeyCode.DownArrow)) SpongeDialogueManager.Instance.NavigateChoice(1);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                SpongeDialogueManager.Instance.ConfirmChoice();
            return;
        }

        if (Input.GetKeyDown(KeyCode.S))
            TryDevSkip();

        if (Input.GetKeyDown(KeyCode.Tab) && SpongeGameManager.Instance.CurrentState != SpongeGameState.GameState.Resolution)
            TryOpenEvidencePanel();

        // 증거 패널 또는 레코드 패널이 열려 있을 때는 방향키/Enter를 슬롯 조작에만 사용
        if (evidencePnl.activeSelf || isRecordPanelMode)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) MoveEvidenceSelection(1);
            if (Input.GetKeyDown(KeyCode.LeftArrow))  MoveEvidenceSelection(-1);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                TryPresentSelectedEvidence();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q) && SpongeGameManager.Instance.CanPress())
            SpongeCrossExaminationManager.Instance.PressWitness();
        if (SpongeGameManager.Instance.CurrentState == SpongeGameState.GameState.Testifying)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
                HandleScreenClick();
        }
        if (SpongeGameManager.Instance.CurrentState == SpongeGameState.GameState.CrossExamination)
        {
            if (SpongeDialogueManager.Instance.IsInDialogueSequence)
            {
                // before_retestimony 같은 일반 대사가 재생 중일 때는 OnScreenClick()으로 진행
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
                    SpongeDialogueManager.Instance.OnScreenClick();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if (SpongeDialogueManager.Instance.IsTyping)
                        SpongeDialogueManager.Instance.SkipCrossExamTyping();
                    else
                        SpongeCrossExaminationManager.Instance.NextLine();
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (SpongeDialogueManager.Instance.IsTyping)
                        SpongeDialogueManager.Instance.SkipCrossExamTyping();
                    else
                        SpongeCrossExaminationManager.Instance.PrevLine();
                }
            }
        }
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            HandleScreenClick();
    }

    // ── 화면 클릭 처리 ───────────────────────────────────────────
    /// <summary>
    /// 화면 클릭 처리 / 마우스 왼쪽 클릭시 현재 상태에 따라 다른 동작 실행
    /// </summary>
    void HandleScreenClick()
    {
        // 증거 패널이나 레코드 패널이 열려있으면 클릭 무시
        if (evidencePnl.activeSelf || isRecordPanelMode) return;

        switch (SpongeGameManager.Instance.CurrentState)
        {
            // 대사중 클릭 -> 타이핑 스킵 or 다음 대사
            case SpongeGameState.GameState.Dialogue:
            case SpongeGameState.GameState.Pressing:
            case SpongeGameState.GameState.EvidenceSelect:
            // 증언 낭독 중 클릭 -> 타이핑 스킵 or 다음 증언
            case SpongeGameState.GameState.Testifying:
                SpongeDialogueManager.Instance.OnScreenClick();
                break;
            // 심문중 클릭 -> 일반 대사 중이면 OnScreenClick(), 아니면 다음 증언으로 이동
            case SpongeGameState.GameState.CrossExamination:
                if (SpongeDialogueManager.Instance.IsInDialogueSequence)
                    SpongeDialogueManager.Instance.OnScreenClick();
                else if (SpongeDialogueManager.Instance.IsTyping)
                    SpongeDialogueManager.Instance.SkipCrossExamTyping();
                else
                    SpongeCrossExaminationManager.Instance.NextLine();
                break;
        }
    }

    // ── 레코드 패널 ──────────────────────────────────────────────
    /// <summary>
    /// 특정 대사 라인(opensRecordPanel=true) 클릭 후 DialogueManager에서 호출
    /// </summary>
    public void OpenRecordPanel(SpongeDialogueLine sourceLine)
    {
        isRecordPanelMode = true;
        recordPanelSourceLine = sourceLine;
        stateBeforeEvidence = SpongeGameManager.Instance.CurrentState;
        SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.EvidenceSelect);
        recordPnl.transform.SetAsLastSibling();
        recordPnl.SetActive(true);
        Time.timeScale = 0f;
        RebuildEvidenceSlots();
        SelectFirstFilledSlot();
    }

    void CloseRecordPanel()
    {
        isRecordPanelMode = false;
        recordPanelSourceLine = null;
        Time.timeScale = 1f;
        recordPnl.SetActive(false);
        SpongeEvidenceManager.Instance.ClearSelection();
        if (SpongeGameManager.Instance.CurrentState == SpongeGameState.GameState.EvidenceSelect)
            SpongeGameManager.Instance.ChangeState(stateBeforeEvidence);
    }

    IEnumerator RecordPanelPresentSequence(string evidenceId)
    {
        var sourceLine = recordPanelSourceLine;
        CloseRecordPanel();
        yield return StartCoroutine(PlayTakeThatAnim());
        bool isCorrect = sourceLine.validRecordEvidenceIds != null
                      && sourceLine.validRecordEvidenceIds.Length > 0
                      && System.Array.Exists(sourceLine.validRecordEvidenceIds, id => id == evidenceId);
        if (isCorrect)
            SpongeDialogueManager.Instance.ShowLine(sourceLine.nextLineId);
        else
            SpongeDialogueManager.Instance.ShowLine("evidence_fail_default_01");
    }

    // ── 증거 패널 ────────────────────────────────────────────────
    /// <summary>
    /// 증거 패널 열기 / TAB 입력 또는 선택지에서 호출
    /// </summary>
    public void TryOpenEvidencePanel()
    {
        // 레코드 패널 모드일 때는 TAB 무시
        if (isRecordPanelMode) return;

        if (evidencePnl.activeSelf)
        {
            CloseEvidencePanel();
            return;
        }

        // 패널 열기 전 상태 저장 후 EvidenceSelect로 전환
        stateBeforeEvidence = SpongeGameManager.Instance.CurrentState;
        SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.EvidenceSelect);

        evidencePnl.transform.SetAsLastSibling();
        evidencePnl.SetActive(true);
        Time.timeScale = 0f;
        RebuildEvidenceSlots();
        SelectFirstFilledSlot();
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
            bool unlocked = data != null && SpongeEvidenceManager.Instance.IsUnlocked(data.id);
            evidenceSlots[i].Setup(data, unlocked);

            // 클릭 이벤트 연결 — EventTrigger 사용 (첫 클릭: 선택, 두 번째 클릭: 제시)
            var trigger = evidenceSlots[i].GetComponent<EventTrigger>()
                       ?? evidenceSlots[i].gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            if (data != null)
            {
                string evidenceId = data.id;
                var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                entry.callback.AddListener(_ =>
                {
                    if (SpongeEvidenceManager.Instance.SelectedEvidenceId == evidenceId)
                    {
                        if (isRecordPanelMode)
                            StartCoroutine(RecordPanelPresentSequence(evidenceId));
                        else if (stateBeforeEvidence == SpongeGameState.GameState.CrossExamination)
                            StartCoroutine(PresentEvidenceSequence(evidenceId));
                    }
                    else
                        SpongeEvidenceManager.Instance.SelectEvidence(evidenceId);
                });
                trigger.triggers.Add(entry);
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
        for (int i = 0; i < evidenceSlots.Length; i++)
        {
            bool match = evidenceSlots[i].EvidenceId == evidenceId;
            evidenceSlots[i].SetHighlight(match);
            if (match) selectedSlotIndex = i;
        }

        foreach (var holder in holderMap.Values)
            if (holder != null) holder.SetActive(false);

        if (holderMap.TryGetValue(evidenceId, out var target) && target != null)
            target.SetActive(true);

        var data = SpongeEvidenceManager.Instance.GetById(evidenceId);
        if (data != null)
        {
            if (evidenceNameTxt != null)        evidenceNameTxt.text        = data.evidenceName;
            if (evidenceDescriptionTxt != null) evidenceDescriptionTxt.text = data.description;
        }
    }

    void SelectFirstFilledSlot()
    {
        for (int i = 0; i < evidenceSlots.Length; i++)
        {
            if (!string.IsNullOrEmpty(evidenceSlots[i].EvidenceId))
            {
                SpongeEvidenceManager.Instance.SelectEvidence(evidenceSlots[i].EvidenceId);
                return;
            }
        }
    }

    void TryPresentSelectedEvidence()
    {
        string id = SpongeEvidenceManager.Instance.SelectedEvidenceId;
        if (string.IsNullOrEmpty(id)) return;
        if (isRecordPanelMode)
            StartCoroutine(RecordPanelPresentSequence(id));
        else if (stateBeforeEvidence == SpongeGameState.GameState.CrossExamination)
            StartCoroutine(PresentEvidenceSequence(id));
    }

    void MoveEvidenceSelection(int dir)
    {
        int next = selectedSlotIndex + dir;
        while (next >= 0 && next < evidenceSlots.Length)
        {
            if (!string.IsNullOrEmpty(evidenceSlots[next].EvidenceId))
            {
                SpongeEvidenceManager.Instance.SelectEvidence(evidenceSlots[next].EvidenceId);
                return;
            }
            next += dir;
        }
    }

    // ── 개발자 스킵 (S키) ────────────────────────────────────────
    void TryDevSkip()
    {
        var state = SpongeGameManager.Instance.CurrentState;

        // 오프닝 스킵
        if (state == SpongeGameState.GameState.Dialogue && SpongeGameManager.Instance.CurrentRound == 1)
            SpongeDialogueManager.Instance.SkipOpening();
        // 증언 낭독 스킵
        else if (state == SpongeGameState.GameState.Testifying)
            SpongeCrossExaminationManager.Instance.SkipTestifying();
        // 재증언 전 대사 스킵
        else if (state == SpongeGameState.GameState.CrossExamination && SpongeDialogueManager.Instance.IsInDialogueSequence)
            SpongeDialogueManager.Instance.SkipBeforeRetestimony();
    }

    /// <summary>
    /// 증거 패널 닫기
    /// </summary>
    public void CloseEvidencePanel()
    {
        Time.timeScale = 1f;
        evidencePnl.SetActive(false);
        SpongeEvidenceManager.Instance.ClearSelection();
        if (SpongeGameManager.Instance.CurrentState == SpongeGameState.GameState.EvidenceSelect)
            SpongeGameManager.Instance.ChangeState(stateBeforeEvidence);
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
