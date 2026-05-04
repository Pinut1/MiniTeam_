using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SpongeDialogueManager : MonoBehaviour
{
    public static SpongeDialogueManager Instance { get; private set; }

    [SerializeField] private SpongeTrialScriptSO trialScript;

    [Header("대사 UI")]
    [SerializeField] private TMP_Text speakerTxt;
    [SerializeField] private TMP_Text dialogueTxt;

    [Header("선택지 UI")]
    [SerializeField] private GameObject choicePnl;
    [SerializeField] private Button[] choiceBtns; // 버튼 2개 고정 (선택지 더 추가 될 예정X)

    [Header("배경 / 캐릭터")]
    [SerializeField] private Image backgroundImg; // 배경
    [SerializeField] private Image characterPlayer; // 플레이어
    [SerializeField] private Image characterPlankton; // 판사
    [SerializeField] private Image characterSpongeBob; // 스폰지밥
    [SerializeField] private Image characterDdungi;
    [SerializeField] private Image characterJingJingi; // 징징이
    [SerializeField] private Image characterJipgeSajang; // 집게사장

    [Header("Animater")]
    [SerializeField] private Animator characterAnim;

    private Dictionary<string, SpongeDialogueLine> lineMap;
    private Coroutine typingCoroutine;
    private bool isTyping;
    private SpongeDialogueLine currentLine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BulidLineMap();
    }

    void BulidLineMap()
    {
        lineMap = new Dictionary<string, SpongeDialogueLine>();

        foreach (var line in trialScript.openingLines)
            lineMap[line.lineId] = line;
        foreach (var line in trialScript.endingLines)
            lineMap[line.lineId] = line;

        // 추궁 대사도 lineMap에 등록 (CrossExaminationManager에서 ShowLine 호출 시 필요)
        foreach (var testimony in trialScript.testimonyLines)
        {
            // 추궁 대사 ID는 TrialSriptSO와 별도 배열로 관리
            // PressDialogueLines 배열 참고
        }
    }
    // ── 대사 표시 진입점 ─────────────────────────────────────
    public void ShowLine(string lineId)
    {
        if (!lineMap.TryGetValue(lineId, out var line))
        {
            Debug.Log($"[DialogueManager] lineId를 찾을 수 없음 : {lineId}");
            return;
        }
        currentLine = line;

        if (!string.IsNullOrEmpty(line.sceneToLoad))
        {
            StartCoroutine(LoadSceneAndShowLine(line));
            return;
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine(line));
    }

    // ── 씬 전환 후 대사 표시 ─────────────────────────────────
    /// <summary>
    /// 대사 표시 코루틴
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    IEnumerator LoadSceneAndShowLine(SpongeDialogueLine line)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(line.sceneToLoad);
        yield return op;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine(line));
    }

    // ── 타이핑 연출 ──────────────────────────────────────────
    /// <summary>
    /// 대사 타이핑 연출 코루틴
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    IEnumerator TypeLine(SpongeDialogueLine line)
    {
        // 1. 배경 교체, null이면 이전 배경 유지
        if (line.backgroundImg != null)
            backgroundImg.sprite = line.backgroundImg.sprite;

        // 2. 캐릭터 위치 활성화
        UpdateCharacter(line);

        // 3. 해당 위치 Animator에 트리거
        if (!string.IsNullOrEmpty(line.animationTrig))
        {
            TriggerAnimation(line);
            yield return new WaitForSeconds(0.5f);
        }

        isTyping = true;
        speakerTxt.text = line.speaker;
        dialogueTxt.text = "";
        choicePnl.SetActive(false);

        int i = 0;
        string fullTxt = line.txt;
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
        OnLineFinished(line);
    }

    // ── 캐릭터 위치 활성화 ───────────────────────────────────
    void UpdateCharacter(SpongeDialogueLine line)
    {
        // 전체 숨기기
        characterPlayer.gameObject.SetActive(false);
        characterSpongeBob.gameObject.SetActive(false);
        characterJingJingi.gameObject.SetActive(false);
        characterPlankton.gameObject.SetActive(false);
        characterJipgeSajang.gameObject.SetActive(false);

        if (line.characterPos == SpongeDialogueLine.CharacterPosition.None) return;
        Image target = line.characterPos switch
        {
            SpongeDialogueLine.CharacterPosition.SpongeBob => characterSpongeBob,
            SpongeDialogueLine.CharacterPosition.Player => characterPlayer,
            SpongeDialogueLine.CharacterPosition.Ddungi => characterDdungi,
            SpongeDialogueLine.CharacterPosition.JingJingi => characterJingJingi,
            SpongeDialogueLine.CharacterPosition.Plankton => characterPlankton,
            SpongeDialogueLine.CharacterPosition.JipgeSajang => characterJipgeSajang,
            _ => null
        };
        // 스프라이트 교체 없이 활성화만 - 스프라이트는 Animator가 제어
        target?.gameObject.SetActive(true);
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
    public void OnScreenClick()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
            dialogueTxt.text = currentLine.txt;
            OnLineFinished(currentLine);
            return;
        }
    }

    // ── 대사 종료 처리 ───────────────────────────────────────
    void OnLineFinished(SpongeDialogueLine line)
    {
        if (line.choices != null && line.choices.Length > 0)
        {
            ShowChoice(line);
            return;
        }
        if (string.IsNullOrEmpty(line.nextLineId))
            OnSeqeuenceEnd();
    }

    // ── 선택지 표시 ──────────────────────────────────────────
    void ShowChoice(SpongeDialogueLine line)
    {
        choicePnl.SetActive(true);

        for (int i = 0; i < choiceBtns.Length; i++)
        {
            bool active = i < line.choices.Length;
            choiceBtns[i].gameObject.SetActive(active);
            if (!active) continue;

            int idx = i;
            choiceBtns[i].GetComponentInChildren<TMP_Text>().text = line.choices[i].choiceTxt;
            choiceBtns[i].onClick.RemoveAllListeners();
            choiceBtns[i].onClick.AddListener(() => ShowLine(line.choices[idx].nextLineId));
        }
    }

    void OnSeqeuenceEnd()
    {
        switch (SpongeGameManager.Instance.CurrentState)
        {
            case SpongeGameState.GameState.Pressing:
                SpongeGameManager.Instance.ChangeState(SpongeGameState.GameState.CrossExzamination);
                break;
            case SpongeGameState.GameState.Dialogue:
                SpongeCrossExaminationManager.Instance.StartCrossExamination();
                break;
        }
    }
}
