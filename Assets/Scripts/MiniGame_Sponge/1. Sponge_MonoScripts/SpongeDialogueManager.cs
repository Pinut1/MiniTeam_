using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// 대사 출력, 타이핑 연출, 선택지, 씬 전환을 담당
/// 1. lineId로 대사를 찾아서 타이핑 연출과 함께 출력
/// 2. 대사마다 배경/캐릭터 위치 교체
/// 3. 씬 전환이 필요하면 씬을 먼저 로드하고 대사 출력
/// 4. 선택지가 있으면 버튼 UI 표시
/// 5. 대사 시퀀스가 끝나면 다음 상태로 전환 (OnSequenceEnd)
/// </summary>
public class SpongeDialogueManager : MonoBehaviour
{
    public static SpongeDialogueManager Instance { get; private set; }

    // 재판 대본 데이터
    private SpongeTrialScriptData trialScript;

    [Header("화자 / 대사 텍스트")]
    [SerializeField] private GameObject textBoxPanel;    // 대사창 전체 패널
    [SerializeField] private Image textBoxUIImg;         // 대사창 배경 이미지 (txt null이면 비활성)
    [SerializeField] private GameObject dialogueNameImg; // 화자 이름 배경 이미지
    [SerializeField] private TMP_Text speakerTxt;        // 화자 이름 표시
    [SerializeField] private TMP_Text dialogueTxt;       // 실제 대사가 타이핑 되는 텍스트

    [Header("선택지 UI")]
    [SerializeField] private GameObject choicePnl;
    [SerializeField] private Button[] choiceBtns;    // 버튼 2개 고정 (선택지 더 추가 될 예정X)
    [SerializeField] private TMP_Text[] choiceBtnTxts; // 각 버튼의 TMP_Text — choiceBtns와 순서 맞춰 연결

    [Header("배경 / 캐릭터")]
    [SerializeField] private Image backgroundImg; // 배경
    [SerializeField] private Image deskImg;       // 책상
    [Space(10f)]
    [SerializeField] private Image characterSpongeBob;
    [SerializeField] private Image characterPlayer;
    [SerializeField] private Image characterDdungi;
    [SerializeField] private Image characterJingJingi;
    [SerializeField] private Image characterPlankton;
    [SerializeField] private Image characterJipgeSajang;

    [Header("캐릭터 위치")]
    [SerializeField] private Vector2 posLeft;
    [SerializeField] private Vector2 posCenter;
    [SerializeField] private Vector2 posRight;
    [SerializeField] private Vector2 posJudgeCenter;
    [SerializeField] private Vector2 posWitnessCenter;

    [Header("화살표")]
    [SerializeField] private Image arrowImg;                // 대사 진행 화살표
    [SerializeField] private Image arrowLeftImg;            // 심문 중 왼쪽 화살표
    [SerializeField] private Image arrowRightImg;           // 심문 중 오른쪽 화살표
    // [SerializeField] private Animator nextLineAnim;      // 애니메이션 구현 후 사용

    [Header("증언 고정 배경/캐릭터")]
    [SerializeField] private Sprite testimonyBgSprite;
    [SerializeField] private Sprite testimonyDeskSprite;
    [SerializeField] private Image testimonyCharImage;

    [Header("엔딩 연출")]
    [SerializeField] private GameObject endingTextObj;

    [Header("심문 텍스트 색상")]
    [SerializeField] private Color testimonyColor = new Color32(54, 199, 56, 255);

    [Header("캐릭터 Animater")]
    //[SerializeField] private Animator characterAnim;

    // ── 내부 변수 ────────────────────────────────────────────────
    //lineId → DialogueLine 딕셔너리
    private Dictionary<string, SpongeDialogueLine> lineMap = new Dictionary<string, SpongeDialogueLine>(); // 빠르게 대사 찾기 위해 사용

    // 타이핑 코루틴
    private Coroutine typingCoroutine;
    // 타이핑 여부 판단 - 타이핑 스킵 여부 판단용
    private bool isTyping;
    // 엔딩 시퀀스 진행 여부
    private bool isEnding = false;
    // 현재 표시 중인 대사 데이터 - 타이핑 스킵 시 전체 텍스트를 즉시 표시하기 위해 보관
    private SpongeDialogueLine currentLine;
    // 증언 낭독/심문 중 스킵 시 전체 텍스트 표시용
    private string currentTestimonyText;
    // 심문 중 스킵 시 화살표 복원용
    private bool currentTestimonyIsFirst;
    private bool currentTestimonyIsLast;

    private int selectedChoiceIndex = 0;

    public bool IsTyping => isTyping;
    public bool IsInDialogueSequence { get; private set; }
    public bool IsChoiceActive => choicePnl != null && choicePnl.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        arrowLeftImg.gameObject.SetActive(false);
        arrowRightImg.gameObject.SetActive(false);

        TextAsset jsonAsset = Resources.Load<TextAsset>("SpongeData/SpongeTrialScript");
        trialScript = JsonUtility.FromJson<SpongeTrialScriptData>(jsonAsset.text);

        // 대사 목록을 딕셔너리로 변환
        BuildLineMap();
    }

    /// <summary>
    /// TrialScriptSO의 모든 대사 배열을 딕셔너리로 변환
    /// lineId를 키로 사용해서 O(1)로 빠르게 검색 가능
    /// </summary>
    void BuildLineMap()
    {
        lineMap = new Dictionary<string, SpongeDialogueLine>();

        // 오프닝 대사 등록
        foreach (var line in trialScript.openingLines)
            lineMap[line.lineId] = line;
        // 추궁 대사 등록
        foreach (var line in trialScript.beforeRetestimonyLines)
            lineMap[line.lineId] = line;
        foreach (var line in trialScript.pressDialogueLines)
            lineMap[line.lineId] = line;
        foreach (var line in trialScript.evidenceDialogueLines)
            lineMap[line.lineId] = line;
        foreach (var line in trialScript.endingLines)
            lineMap[line.lineId] = line;
        
        /*
        foreach (var testimony in trialScript.testimonyLines)
        {
            // 추궁 대사 ID는 TrialSriptSO와 별도 배열로 관리
            // PressDialogueLines 배열 참고
        }
        */
    }
    // ── 대사 표시 진입점 ─────────────────────────────────────
    /// <summary>
    /// lined로 대사를 찾아서 출력 시작
    /// 모든 대사 출력은 이 메서드를 통해 시작
    /// </summary>
    /// <param name="lineId"></param>
    public void ShowLine(string lineId)
    {
        if (string.IsNullOrEmpty(lineId)) { OnSequenceEnd(); return; }
        if (!lineMap.TryGetValue(lineId, out var line))
        {
            Debug.LogWarning($"[DialogueManager] lineId를 찾을 수 없음 : {lineId}");
            return;
        }
        IsInDialogueSequence = true;
        currentLine = line;

        // 이전 타이핑 코루틴이 있으면 중단
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine(line));
    }

   
    /// <summary>
    /// 심문(CrossExamination) 상태에서 증언 라인을 타이핑으로 표시
    /// </summary>
    public void ShowTestimonyLine(SpongeTestimonyLine testimony, bool isFirst, bool isLast)
    {
        IsInDialogueSequence = false;
        currentTestimonyText = testimony.txt;
        currentTestimonyIsFirst = isFirst;
        currentTestimonyIsLast = isLast;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeTestimonyLineCrossExam(testimony, isFirst, isLast));
    }

    IEnumerator TypeTestimonyLineCrossExam(SpongeTestimonyLine testimony, bool isFirst, bool isLast)
    {
        ApplyTestimonyVisuals();
        isTyping = true;
        dialogueNameImg.SetActive(true);
        speakerTxt.text = "집게사장";
        dialogueTxt.text = "";
        dialogueTxt.color = testimonyColor;
        choicePnl.SetActive(false);
        arrowImg.gameObject.SetActive(false);
        arrowLeftImg.gameObject.SetActive(false);
        arrowRightImg.gameObject.SetActive(false);

        int i = 0;
        string fullTxt = testimony.txt;
        while (i < fullTxt.Length)
        {
            if (fullTxt[i] == '<')
            {
                int closeIdx = fullTxt.IndexOf('>', i);
                if (closeIdx != -1)
                {
                    dialogueTxt.text += fullTxt.Substring(i, closeIdx - i + 1);
                    i = closeIdx + 1;
                    continue;
                }
            }
            dialogueTxt.text += fullTxt[i];
            i++;
            yield return new WaitForSeconds(0.04f);
        }
        isTyping = false;
        arrowLeftImg.gameObject.SetActive(!isFirst);
        arrowRightImg.gameObject.SetActive(!isLast);
    }

    /// <summary>
    /// 심문 중 타이핑 스킵 - UIManager에서 CrossExamination 클릭 시 호출
    /// </summary>
    public void SkipCrossExamTyping()
    {
        if (!isTyping) return;
        StopCoroutine(typingCoroutine);
        isTyping = false;
        dialogueTxt.text = currentTestimonyText;
        arrowLeftImg.gameObject.SetActive(!currentTestimonyIsFirst);
        arrowRightImg.gameObject.SetActive(!currentTestimonyIsLast);
    }

    /// <summary>
    /// 증언 낭독(Testifying) 상태 진입점 - 타이핑으로 표시, 좌우 화살표 없음
    /// </summary>
    public void ShowTestimonyAsDialogue(SpongeTestimonyLine testimony)
    {
        IsInDialogueSequence = false;
        currentTestimonyText = testimony.txt;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeTestimonyLine(testimony));
    }

    IEnumerator TypeTestimonyLine(SpongeTestimonyLine testimony)
    {
        ApplyTestimonyVisuals();
        isTyping = true;
        dialogueNameImg.SetActive(true);
        speakerTxt.text = "집게사장";
        dialogueTxt.text = "";
        dialogueTxt.color = Color.white;
        choicePnl.SetActive(false);
        arrowImg.gameObject.SetActive(false);
        arrowLeftImg.gameObject.SetActive(false);
        arrowRightImg.gameObject.SetActive(false);

        int i = 0;
        string fullTxt = testimony.txt;
        while (i < fullTxt.Length)
        {
            if (fullTxt[i] == '<')
            {
                int closeIdx = fullTxt.IndexOf('>', i);
                if (closeIdx != -1)
                {
                    dialogueTxt.text += fullTxt.Substring(i, closeIdx - i + 1);
                    i = closeIdx + 1;
                    continue;
                }
            }
            dialogueTxt.text += fullTxt[i];
            i++;
            yield return new WaitForSeconds(0.04f);
        }
        isTyping = false;
        arrowRightImg.gameObject.SetActive(true);
    }


    // ── 타이핑 연출 ──────────────────────────────────────────
    /// <summary>
    /// 대사 타이핑 연출 코루틴
    /// 배경/캐릭터 교체 -> 애니메이션 -> 타이핑
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    IEnumerator TypeLine(SpongeDialogueLine line)
    {
        // 오프닝 중간 페이드 (opening_28 진입 시 한 번)
        bool needsFade = line.lineId == "opening_28" && SpongeFadeManager.Instance != null;
        if (needsFade)
        {
            if (textBoxPanel != null) textBoxPanel.SetActive(false);
            yield return StartCoroutine(SpongeFadeManager.Instance.FadeIn(1f));
        }

        // 1. 배경 교체, 비어있으면 이전 배경 유지
        if (!string.IsNullOrEmpty(line.backgroundSpr))
            backgroundImg.sprite = Resources.Load<Sprite>(line.backgroundSpr);
        if (line.deskSpr == "None")
            deskImg.gameObject.SetActive(false);
        else if (!string.IsNullOrEmpty(line.deskSpr))
        {
            deskImg.sprite = Resources.Load<Sprite>(line.deskSpr);
            deskImg.gameObject.SetActive(true);
        }

        // 2. 캐릭터 위치 활성화
        UpdateCharacter(line);

        if (needsFade)
        {
            yield return StartCoroutine(SpongeFadeManager.Instance.FadeOut(1f));
            if (textBoxPanel != null) textBoxPanel.SetActive(true);
        }

        // 3. 해당 위치 Animator에 트리거
        /*if (!string.IsNullOrEmpty(line.animationTrig))
        {
            TriggerAnimation(line);
            yield return new WaitForSeconds(1f);
        }
        */

        // txt null 또는 빈 문자열이면 대사창 이미지 비활성화하고 타이핑 생략
        bool hasTxt = !string.IsNullOrEmpty(line.txt);
        if (textBoxUIImg != null) textBoxUIImg.gameObject.SetActive(hasTxt);
        if (!hasTxt)
        {
            arrowImg.gameObject.SetActive(true);
            OnLineFinished(line);
            yield break;
        }

        // 타이핑 시작
        isTyping = true;
        bool hasSpeaker = !string.IsNullOrEmpty(line.speaker);
        dialogueNameImg.SetActive(hasSpeaker);
        speakerTxt.text = hasSpeaker ? line.speaker : "";
        dialogueTxt.color = Color.white; // 심문 색상 리셋
        if (!string.IsNullOrEmpty(line.alignment))
            dialogueTxt.alignment = line.alignment switch
            {
                "Left"   => TextAlignmentOptions.Left,
                "Center" => TextAlignmentOptions.Center,
                "Right"  => TextAlignmentOptions.Right,
                _        => dialogueTxt.alignment
            };
        dialogueTxt.text = ""; // 텍스트 초기화
        choicePnl.SetActive(false);               // 선택지 패널 숨기기
        arrowImg.gameObject.SetActive(false);     // 대사 화살표 숨기기
        arrowLeftImg.gameObject.SetActive(false); // 심문 화살표 숨기기
        arrowRightImg.gameObject.SetActive(false);

        // RichText 태그 뭐 저시기 안보이게
        int i = 0;
        string fullTxt = line.txt;
        while (i < fullTxt.Length)
        {
            // < 로 태그 시작 >로 한 번에 삽입
            if (fullTxt[i] == '<')
            {
                int closeIdx = fullTxt.IndexOf('>', i);
                if (closeIdx != -1)
                {
                    dialogueTxt.text += fullTxt.Substring(i, closeIdx - i + 1);
                    i = closeIdx + 1;
                    continue;
                }
            }
            // 일반 문자는 한 글자씩 추가
            dialogueTxt.text += fullTxt[i];
            i++;
            // 한 글자 추가 후 대기 (타이핑 속도)
            yield return new WaitForSeconds(0.04f);
        }
        isTyping = false;
        arrowImg.gameObject.SetActive(true);
        // // nextLineAnim?.Play("상태이름"); // 애니메이션 구현 후 사용

        // 타이핑 완료 -> 선택지 표시 또는 시퀀스 종료
        OnLineFinished(line);
    }

    // ── 캐릭터 위치 활성화 ───────────────────────────────────
    /// <summary>
    /// 대사의 캐릭터Pos에 따라 해당 위치 캐릭터 이미지 활성화
    /// </summary>
    /// <param name="line"></param>
    void UpdateCharacter(SpongeDialogueLine line)
    {
        characterSpongeBob.gameObject.SetActive(false);
        characterPlayer.gameObject.SetActive(false);
        characterDdungi.gameObject.SetActive(false);
        characterJingJingi.gameObject.SetActive(false);
        characterPlankton.gameObject.SetActive(false);
        characterJipgeSajang.gameObject.SetActive(false);

        if (line.characterType == SpongeDialogueLine.CharacterType.None) return;

        // 캐릭터 선택
        Image target = line.characterType switch
        {
            SpongeDialogueLine.CharacterType.SpongeBob   => characterSpongeBob,
            SpongeDialogueLine.CharacterType.Player      => characterPlayer,
            SpongeDialogueLine.CharacterType.Ddungi      => characterDdungi,
            SpongeDialogueLine.CharacterType.JingJingi   => characterJingJingi,
            SpongeDialogueLine.CharacterType.Plankton    => characterPlankton,
            SpongeDialogueLine.CharacterType.JipgeSajang => characterJipgeSajang,
            _ => null
        };

        if (target == null) return;

        // 위치 이동
        target.rectTransform.anchoredPosition = line.characterPos switch
        {
            SpongeDialogueLine.CharacterPosition.Left          => posLeft,
            SpongeDialogueLine.CharacterPosition.Center        => posCenter,
            SpongeDialogueLine.CharacterPosition.Right         => posRight,
            SpongeDialogueLine.CharacterPosition.JudgeCenter   => posJudgeCenter,
            SpongeDialogueLine.CharacterPosition.WitnessCenter => posWitnessCenter,
            _ => target.rectTransform.anchoredPosition
        };

        // // 스프라이트 (Animator 구현 전까지 사용)
        // if (line.characterSpr != null) target.sprite = line.characterSpr;

        // // 애니메이션 트리거 (Animator 구현 후 사용)
        // if (!string.IsNullOrEmpty(line.animationTrig))
        //     target.GetComponent<Animator>().SetTrigger(line.animationTrig);

        target.gameObject.SetActive(true);
    }

    // ── 증언 고정 배경/캐릭터 적용 ──────────────────────────
    public void ApplyTestimonyVisuals()
    {
        if (testimonyBgSprite != null) backgroundImg.sprite = testimonyBgSprite;

        if (testimonyDeskSprite != null)
        {
            deskImg.sprite = testimonyDeskSprite;
            deskImg.gameObject.SetActive(true);
        }
        else
            deskImg.gameObject.SetActive(false);

        characterSpongeBob.gameObject.SetActive(false);
        characterPlayer.gameObject.SetActive(false);
        characterDdungi.gameObject.SetActive(false);
        characterJingJingi.gameObject.SetActive(false);
        characterPlankton.gameObject.SetActive(false);
        characterJipgeSajang.gameObject.SetActive(false);

        if (testimonyCharImage != null) testimonyCharImage.gameObject.SetActive(true);
    }

    // ── 캐릭터 위치에 맞는 Animator에 트리거 ────────────────
    void TriggerAnimation(SpongeDialogueLine line)
    {
        /*
        Animator targer = line.characterPos switch
        {
            // 애니메이터 구현후 적을 예정인데 미리 적어두겠습니다^.^
            
            SpongeDialogueLine.CharacterPosition.SpongeBob => animSpongeBob,
            SpongeDialogueLine.CharacterPosition.Player => animPlayer,
            SpongeDialogueLine.CharacterPosition.Ddungi => animDdungi,
            SpongeDialogueLine.CharacterPosition.JingJingi => animJingJingi,
            SpongeDialogueLine.CharacterPosition.Plankton => animPlankton,
            SpongeDialogueLine.CharacterPosition.JipgeSajang => animJipgeSajang,
            _ => null
             
        };
        targer?.SetTrigger(line.animationTrigger);
        */
    }

    // ── 클릭 처리 ────────────────────────────────────────────
    /// <summary>
    /// 화면 클릭시 호출
    /// 타이핑 중이면 전체 텍스트 즉시 출력(스킵)
    /// 타이핑 완료 후면 다음 대사로 이동
    /// UIManager에서 호출
    /// </summary>
    public void OnScreenClick()
    {
        bool isTestifying = SpongeGameManager.Instance.CurrentState == SpongeGameState.GameState.Testifying;

        // 글자가 타이핑 중이라면 즉시 완성
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;

            if (isTestifying)
            {
                // 증언 낭독 스킵 - 오른쪽 화살표만 표시
                dialogueTxt.text = currentTestimonyText;
                arrowRightImg.gameObject.SetActive(true);
            }
            else
            {
                dialogueTxt.text = currentLine.txt;
                arrowImg.gameObject.SetActive(true);
                OnLineFinished(currentLine);
            }
            return;
        }

        // 선택지가 떠 있을 경우 클릭으로 넘기기 X
        if (choicePnl.activeSelf) return;

        // 증언 낭독 중 클릭 → CrossExaminationManager에서 다음 증언으로
        if (isTestifying)
        {
            SpongeCrossExaminationManager.Instance.AdvanceTestifying();
            return;
        }

        // press_02_12 이후 : 매출영수증 획득 여부에 따라 분기
        if (currentLine.lineId == "press_02_12" && !SpongeEvidenceManager.Instance.IsUnlocked("receipt"))
        {
            ShowLine("press_need_more");
            return;
        }

        // 이 라인에 증거 획득이 연결돼 있으면 클릭 시 unlock
        if (!string.IsNullOrEmpty(currentLine.grantEvidenceId))
            SpongeEvidenceManager.Instance.UnlockEvidence(currentLine.grantEvidenceId);

        // 다음 대사가 있다면 해당 대사 보여줌
        if (!string.IsNullOrEmpty(currentLine.nextLineId))
            ShowLine(currentLine.nextLineId);
        // 다음 대사가 없다면 시퀀스 종료
        else
            OnSequenceEnd();
    }

    // ── 대사 종료 처리 ───────────────────────────────────────
    /// <summary>
    /// 타이핑이 완료됐을 때
    /// 선택지 있으면 선택지 표시, 없으면 시퀀스 종료
    /// </summary>
    /// <param name="line"></param>
    void OnLineFinished(SpongeDialogueLine line)
    {
        // 선택지 있으면 선택지 UI 표시
        if (line.choices != null && line.choices.Length > 0)
        {
            ShowChoice(line);
            return;
        }
        // nextLineId 없어도 여기서 종료하지 않음 - 클릭 후 OnScreenClick()에서 OnSequenceEnd() 호출
    }

    // ── 선택지 표시 ──────────────────────────────────────────
    /// <summary>
    /// 선택지 버튼들을 활성화 -> 텍스트/이벤트 연결
    /// </summary>
    /// <param name="line"></param>
    void ShowChoice(SpongeDialogueLine line)
    {
        choicePnl.SetActive(true);
        arrowImg.gameObject.SetActive(false);

        var noNav = new Navigation { mode = Navigation.Mode.None };
        for (int i = 0; i < choiceBtns.Length; i++)
        {
            // 선택지 수보다 버튼이 많으면 나머지 버튼 숨기기
            bool active = i < line.choices.Length;
            choiceBtns[i].gameObject.SetActive(active);
            choiceBtns[i].navigation = noNav;
            if (!active) continue;

            // 클로저 캡쳐 - 람다 안에서 i를 쓰면 루프 끝난 값으로 고정되므로 idx로 복사해서 사용
            int idx = i;
            if (i < choiceBtnTxts.Length && choiceBtnTxts[i] != null)
                choiceBtnTxts[i].text = line.choices[i].choiceTxt;
            
            // 이전 이벤트 제거 -> 새 이벤트 연결
            choiceBtns[i].onClick.RemoveAllListeners();
            if (line.choices[idx].opensEvidencePanel)
            {
                choiceBtns[i].onClick.AddListener(() =>
                {
                    choicePnl.SetActive(false);
                    SpongeUIManager.Instance.TryOpenEvidencePanel();
                });
            }
            else
            {
                choiceBtns[i].onClick.AddListener(() => ShowLine(line.choices[idx].nextLineId));
            }
        }
        SelectChoice(0);
    }

    static readonly Color ChoiceNormalColor    = Color.white;
    static readonly Color ChoiceHighlightColor = new Color(1f, 0.85f, 0.3f, 1f);

    void SelectChoice(int index)
    {
        selectedChoiceIndex = index;
        for (int i = 0; i < choiceBtns.Length; i++)
        {
            if (i < choiceBtnTxts.Length && choiceBtnTxts[i] != null)
                choiceBtnTxts[i].color = (i == index) ? ChoiceHighlightColor : ChoiceNormalColor;
        }
    }

    public void NavigateChoice(int dir)
    {
        int activeCount = 0;
        for (int i = 0; i < choiceBtns.Length; i++)
            if (choiceBtns[i].gameObject.activeSelf) activeCount++;
        Debug.Log($"[Choice] NavigateChoice dir={dir} activeCount={activeCount} before={selectedChoiceIndex}");
        if (activeCount <= 1) return;
        SelectChoice(Mathf.Clamp(selectedChoiceIndex + dir, 0, activeCount - 1));
    }

    public void ConfirmChoice()
    {
        if (selectedChoiceIndex >= 0 && selectedChoiceIndex < choiceBtns.Length
            && choiceBtns[selectedChoiceIndex].gameObject.activeSelf)
            choiceBtns[selectedChoiceIndex].onClick.Invoke();
    }

    // ── 대사 시퀀스 종료 → 다음 상태로 전환 ────────────────────
    /// <summary>
    /// nextLineId가 비어있는 대사가 끝나면 호출
    /// 현재 GameState에 따라 다음 행동 결정
    /// </summary>
    void OnSequenceEnd()
    {
        arrowImg.gameObject.SetActive(false);
        switch (SpongeGameManager.Instance.CurrentState)
        {
            case SpongeGameState.GameState.Pressing:
            case SpongeGameState.GameState.EvidenceSelect:
                // evidence_02_17 끝 → 전체 조건 충족 여부에 따라 엔딩 진입 or 심문 복귀
                if (currentLine != null && currentLine.lineId == "evidence_02_17")
                {
                    if (SpongeGameManager.Instance.IsAllConditionsMet())
                        ShowLine("evidence_02_18");
                    else
                        ShowLine("need_more_evidence");
                    break;
                }
                // ConsumeConditionMet() = "방금 조건이 충족됐어?"
                if (SpongeGameManager.Instance.ConsumeConditionMet())
                {
                    // 첫번째 심문 조건 충족 — press_02 시퀀스 안에서 끝난 경우에만 재증언 전환
                    if (SpongeGameManager.Instance.CurrentRound == 1)
                    {
                        if (currentLine != null && currentLine.lineId.StartsWith("press_02_"))
                        {
                            SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.CrossExamination);
                            Instance.ShowLine("before_retestimony_01");
                        }
                        else
                        {
                            SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.CrossExamination);
                            SpongeCrossExaminationManager.Instance.NextLineOrLoop();
                        }
                    }
                    // 두번째 심문 조건 충족 -> 엔딩 대사 시작 (Dialogue 상태에서 클릭이 정상 동작하도록)
                    else
                    {
                        isEnding = true;
                        SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.Dialogue);
                        StartCoroutine(StartEndingWithFade());
                    }
                }
                else if (currentLine != null && currentLine.lineId == "press_02_14")
                {
                    // press_02_12에서 이미 press_01 여부를 체크했으므로 여기까지 왔다면 조건 충족
                    SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.CrossExamination);
                    Instance.ShowLine("before_retestimony_01");
                }
                else
                {
                    SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.CrossExamination);
                    // 증거 제시 실패 대사 끝 → 같은 증언으로 복귀
                    if (currentLine != null && currentLine.lineId.StartsWith("evidence_fail"))
                        SpongeCrossExaminationManager.Instance.ShowCurrentTestimony();
                    else
                        SpongeCrossExaminationManager.Instance.NextLineOrLoop();
                }
                break;

            case SpongeGameState.GameState.CrossExamination:
                // before_retestimony 시퀀스가 CrossExamination 상태에서 끝날 때
                SpongeCrossExaminationManager.Instance.StartRetestimony();
                break;

            case SpongeGameState.GameState.Dialogue:
                if (isEnding)
                {
                    // 엔딩 대사 시퀀스 완료 -> ENDTxt 제외 전부 비활성화
                    isEnding = false;
                    backgroundImg.gameObject.SetActive(false);
                    deskImg.gameObject.SetActive(false);
                    characterSpongeBob.gameObject.SetActive(false);
                    characterPlayer.gameObject.SetActive(false);
                    characterDdungi.gameObject.SetActive(false);
                    characterJingJingi.gameObject.SetActive(false);
                    characterPlankton.gameObject.SetActive(false);
                    characterJipgeSajang.gameObject.SetActive(false);
                    dialogueNameImg.SetActive(false);
                    dialogueTxt.gameObject.SetActive(false);
                    speakerTxt.gameObject.SetActive(false);
                    choicePnl.SetActive(false);
                    arrowLeftImg.gameObject.SetActive(false);
                    arrowRightImg.gameObject.SetActive(false);
                    if (endingTextObj != null) endingTextObj.SetActive(true);
                    SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.Resolution);
                }
                else if (SpongeGameManager.Instance.CurrentRound == 1)
                    // 오프닝 대사 끝 -> 심문 시작
                    SpongeCrossExaminationManager.Instance.StartCrossExamination();
                else
                    // 재증언 전 대사 끝 -> 재증언 시작
                    SpongeCrossExaminationManager.Instance.StartRetestimony();
                break;
        }
    }

    public void SetTextBox(bool active)
    {
        if (textBoxPanel != null) textBoxPanel.SetActive(active);
    }

    // ── 개발자 스킵 ──────────────────────────────────────────
    public void StopTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        isTyping = false;
        IsInDialogueSequence = false;
    }

    public void SkipOpening()
    {
        StopTyping();
        foreach (var line in trialScript.openingLines)
            if (!string.IsNullOrEmpty(line.grantEvidenceId))
                SpongeEvidenceManager.Instance.UnlockEvidence(line.grantEvidenceId);
        SpongeGameManager.Instance.SkipAllRequiredConditions();
        SpongeCrossExaminationManager.Instance.StartCrossExamination();
    }

    public void SkipBeforeRetestimony()
    {
        StopTyping();
        foreach (var line in trialScript.beforeRetestimonyLines)
            if (!string.IsNullOrEmpty(line.grantEvidenceId))
                SpongeEvidenceManager.Instance.UnlockEvidence(line.grantEvidenceId);
        SpongeCrossExaminationManager.Instance.StartRetestimony();
    }

    IEnumerator StartEndingWithFade()
    {
        SetTextBox(false);
        yield return StartCoroutine(SpongeFadeManager.Instance.FadeIn(1f));
        yield return StartCoroutine(SpongeFadeManager.Instance.FadeOut(1f));
        SetTextBox(true);
        Instance.ShowLine("ending_01");
    }
}
