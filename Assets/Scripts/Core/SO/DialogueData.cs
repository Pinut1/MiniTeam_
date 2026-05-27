using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    // 한 문장마다 어떤 표정을 지을지 세트로 묶기 위한 구조체
    [System.Serializable]
    public struct SentenceData
    {
        [TextArea(2, 4)] public string text;
        public Sprite expressionSprite;
        public string animationTriggerName;
        public AudioClip voiceClip; // 해당 대사가 출력될 때 재생할 음성/효과음
    }

    [Header("대사 및 표정 데이터")]
    public SentenceData[] sentences;

    [Header("상호작용 객체 세팅 (다음 스테이지용)")]
    [Tooltip("대사가 모두 끝난 후, 허브에 띄울 상호작용 객체 (ex: 0이면 주댕치, 1이면 디지바이스)")]
    public int nextInteractableIndex = 0;

}
