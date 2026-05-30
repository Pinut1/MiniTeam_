using MiniTeam.Core;
using System.Collections;
using UnityEngine;

public class HubCutsceneDirector : MonoBehaviour
{
    public static HubCutsceneDirector Instance { get; private set; }





    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayWakeUpSequence(System.Action onComplete = null)
    {
        StartCoroutine(WakeUpRoutine(onComplete));
    }

    public void PlaySequenceForFirstStage()
    {
        StartCoroutine(FirstSequenceRoutine());
    }

    public void PlaySequenceForCurrentStage()
    {
        StartCoroutine(NomalSequenceRoutine());
    }

    public void PlaySequenceForGameClear()
    {
        StartCoroutine(GameClearSequenceRoutine());
    }



    private IEnumerator WakeUpRoutine(System.Action onComplete)
    {
        MiniGameManager.Instance.ChangeState(MiniGameManager.State.Hub_Cutscene);
        
        bool isWakeUpDone = false;
        CinemaUIController.Instance?.PlayWakeUp(() => isWakeUpDone = true);
        
        yield return new WaitUntil(() => isWakeUpDone);
        
        if (SoundManager.Instance != null && AudioManager.Instance != null)
        {
            SoundManager.Instance.SetBGMPitch(0.7f);
            AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmHub);
        }
        
        BottomUIController.Instance?.Show(MiniGameManager.Instance.progressData.currentStage);
        
        MiniGameManager.Instance.ChangeState(MiniGameManager.State.Hub_Idle);
        onComplete?.Invoke();
    }

    private IEnumerator FirstSequenceRoutine()
    {
        // 0. 상태 변경 (마우스 클릭 등 조작 제한)
        MiniGameManager.Instance.ChangeState(MiniGameManager.State.Hub_Cutscene);

        // 1. [입장] 주댕치 버튼 클릭 애니메이션 발사 ("Click" 트리거)
        BottomUIController.Instance?.PlayClickAnimation(0);
        yield return new WaitForSeconds(1.5f); // 1.5초 대기
        
        // 2. [입장] 레터박스 위아래로 닫힘
        bool isLetterboxDone = false;
        CinemaUIController.Instance?.PlayLetterboxEnter(() => isLetterboxDone = true);
        yield return new WaitUntil(() => isLetterboxDone);
        yield return new WaitForSeconds(0.2f); // 레터박스 닫힌 후 잠깐 여운 대기

        // 3. [입장] 주댕치 초상화 튀어나옴 ("Show" 트리거)
        DialoguePortraitController.Instance?.Show(0);
        yield return new WaitForSeconds(1.1f); // 초상화 애니메이션(1.1초) 완전히 끝날 때까지 대기

        // 4. [진행] 대사창 켜고 대사 시작. 클릭하며 끝날 때까지 대기
        bool isDialogueDone = false;
        JudangChiDialogueManager.Instance.StartDialogue("hub_stage" + MiniGameManager.Instance.progressData.currentStage, () => isDialogueDone = true);
        yield return new WaitUntil(() => isDialogueDone);

        // 5. [퇴장] 주댕치 초상화 쏙 숨음 ("Hide" 트리거)
        DialoguePortraitController.Instance?.Hide();
        yield return new WaitForSeconds(1.1f); // 퇴장 애니메이션도 1.1초 걸리므로 동일하게 대기

        // 6. [퇴장] 레터박스 다시 열림
        CinemaUIController.Instance?.PlayLetterboxExit();
        yield return new WaitForSeconds(0.5f);

        // 7. [종료] 주댕치 버튼 다시 위로 올라오게 함 ("Show" 트리거 및 interactable = true)
        BottomUIController.Instance?.Show(MiniGameManager.Instance.progressData.currentStage);
        yield return new WaitForSeconds(0.5f);

        // 8. 컷신 종료 플래그 켜고 조작 권한 회복
        MiniGameManager.Instance.SetCutscenePlayed(true);
        MiniGameManager.Instance.ChangeState(MiniGameManager.State.Hub_Idle);
    }

    private IEnumerator NomalSequenceRoutine()
    {
        // 0. 상태 변경 (조작 제한)
        MiniGameManager.Instance.ChangeState(MiniGameManager.State.Hub_Cutscene);

        // 1. [입장] 디지바이스 클릭 연출 발사 ("Click" 트리거)
        BottomUIController.Instance?.PlayClickAnimation(MiniGameManager.Instance.progressData.currentStage);
        yield return new WaitForSeconds(1.5f); // 1.5초 대기
        
        // 2. [입장] 레터박스 닫힘
        bool isLetterboxDone = false;
        CinemaUIController.Instance?.PlayLetterboxEnter(() => isLetterboxDone = true);
        yield return new WaitUntil(() => isLetterboxDone);
        yield return new WaitForSeconds(0.2f); // 레터박스 닫힌 후 잠깐 대기

        // 3. [입장] 디지바이스 대화 초상화 튀어나옴 ("Show" 트리거)
        DialoguePortraitController.Instance?.Show(MiniGameManager.Instance.progressData.currentStage);
        yield return new WaitForSeconds(1.1f); // 초상화 애니메이션(1.1초) 대기

        // 4. [진행] 대사창 켜고 대사 재생
        bool isDialogueDone = false;
        JudangChiDialogueManager.Instance.StartDialogue("hub_stage" + MiniGameManager.Instance.progressData.currentStage, () => isDialogueDone = true);
        yield return new WaitUntil(() => isDialogueDone);

        // --- 여기서부터 퇴장 시퀀스 ---
        bool isFinalStage = MiniGameManager.Instance.progressData.currentStage == 5;
        
        if (isFinalStage)
        {
            // 5-A. [퇴장] 엔딩 직전 컷신일 경우: 초상화와 버튼 동시 퇴장 ("Hide" 트리거)
            DialoguePortraitController.Instance?.Hide();
            BottomUIController.Instance?.PlayHideAnimation(MiniGameManager.Instance.progressData.currentStage);
            yield return new WaitForSeconds(2.0f); // 2초간 동시 퇴장 감상
            
            // 6-A. 레터박스 열림
            CinemaUIController.Instance?.PlayLetterboxExit();
            yield return new WaitForSeconds(1.0f);
            
            // 엔딩 씬으로 바로 넘어감
            yield return StartCoroutine(PlayEndingSequence());
        }
        else
        {
            // 5-B. [퇴장] 일반 스테이지일 경우: 초상화와 버튼 동시 퇴장 ("Hide" 트리거)
            DialoguePortraitController.Instance?.Hide();
            BottomUIController.Instance?.PlayHideAnimation(MiniGameManager.Instance.progressData.currentStage);
            yield return new WaitForSeconds(2.0f); // 2초간 동시 퇴장 감상

            // 6-B. 레터박스 열림
            CinemaUIController.Instance?.PlayLetterboxExit();
            yield return new WaitForSeconds(0.5f);

            // 7. [종료] 버튼 다시 활성화 (클릭 가능해짐)
            BottomUIController.Instance?.Show(MiniGameManager.Instance.progressData.currentStage);
            yield return new WaitForSeconds(0.5f);
            
            // 8. 조작 권한 회복
            MiniGameManager.Instance.ChangeState(MiniGameManager.State.Hub_Idle);
        }

        MiniGameManager.Instance.SetCutscenePlayed(true);
    }

    private IEnumerator GameClearSequenceRoutine()
    {
        MiniGameManager.Instance.ChangeState(MiniGameManager.State.Hub_Cutscene);
        
        bool isWakeUpDone = false;
        CinemaUIController.Instance?.PlayWakeUp(() => isWakeUpDone = true);
        yield return new WaitUntil(() => isWakeUpDone);

        yield return new WaitForSeconds(0.5f);

        DialoguePortraitController.Instance?.PlayStageClear();
        
        yield return new WaitForSeconds(2.5f);
        
        BottomUIController.Instance?.Show(MiniGameManager.Instance.progressData.currentStage);
        MiniGameManager.Instance.ChangeState(MiniGameManager.State.Hub_Idle);
    }

    private IEnumerator PlayEndingSequence()
    {
        MiniTeam.Core.AudioManager.Instance?.StopBGM();
        Debug.Log("모든 스테이지 클리어! 엔딩 씬으로 진입합니다.");
        yield return new WaitForSeconds(1f);
        MiniGameManager.Instance.LoadEndingScene();
    }
}
