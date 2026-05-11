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
    public static SpongeUIManager Instance;
    
    [Header("증거 패널")]
    [SerializeField] private GameObject evidencePnl; // 증거 목록 전체를 감싸는 패널

    [SerializeField] private Transform evidenceBtnParent; // 증거 버튼들이 배치될 부모 오브젝트

    [SerializeField] private GameObject evidenceBtnPrefab; // 증거 버튼 프리팹

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
            case SpongeGameState.GameState.Dialogue:
            case SpongeGameState.GameState.Pressing:
            // 대사중 클릭 -> 타이핑 스킵 or 다음 대사
            case SpongeGameState.GameState.EvidenceSelect: SpongeDialogueManager.Instance.OnScreenClick();
                break;
            // 심문중 클릭 -> 다음 증언으로 이동
            case SpongeGameState.GameState.CrossExamination: SpongeCrossExaminationManager.Instance.NextLine();
                break;
        }
    }

    // ── 증거 패널 ────────────────────────────────────────────────

    /// <summary>
    /// 증거 패널 열기 / TAB 입력 시 호출 / 증거 버튼 목록을 동적으로 생성
    /// </summary>
    void TryOpenEvidencePanel()
    {
        if (evidencePnl.activeSelf)
        {
            CloseEvidencePanel();
            return;
        }

        // 증거 선택 상태로 전환
        SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.EvidenceSelect);

        // 이전에 생성된 버튼 전부 삭제 후 새로 생성
        foreach (Transform child in evidenceBtnParent)
            Destroy(child.gameObject);

        // EvidenceManager에서 전체 증거 목록 가져오기
        SpongeEvidenceData[] evidences = SpongeEvidenceManager.Instance.GetAllEvidences();

        // 증거마다 버튼 하나씩 생성
        foreach (var evidence in evidences)
        {
            // 프리팹으로 버튼 오브젝트 생성
            GameObject btn = Instantiate(evidenceBtnPrefab, evidenceBtnParent);
            // 버튼 증거이름 표시
            btn.GetComponentInChildren<TMP_Text>().text = evidence.evidenceName;
            // 버튼에 아이콘 이미지 설정
            // 프리팹 안에 아이콘용 Image 컴포넌트가 있어야함!
            Image iconImg = btn.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImg != null && evidence.icon != null)
                iconImg.sprite = evidence.icon;

            // 버튼 클릭 시 해당 증거 제시
            // 클로저 캡쳐 - 람다 안에서 evidence를 쓰면 루프 끝값으로 고정됨
            string evidenceId = evidence.id;
            btn.GetComponent<Button>().onClick.AddListener(() => SpongeEvidenceManager.Instance.PresentEvidence(evidenceId));
        }
        evidencePnl.SetActive(true);
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
