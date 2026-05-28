using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro 사용 필수

public class RandomLetterReveal : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_Text textComponent;

    [Header("효과 설정")]
    [TextArea]
    public string targetText = "SYSTEM INITIALIZED..."; // 최종적으로 보여질 글자
    public string randomChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*"; // 섞일 랜덤 문자들

    public float revealSpeed = 0.05f; // 한 글자가 확정되는 시간
    public int randomChangesPerLetter = 3; // 글자 하나가 확정되기 전에 랜덤 문자가 깜빡이는 횟수

    [Header("흔들림 방지 설정")]
    public bool useMonospace = true; // 강제 고정폭 사용 여부 (글자 흔들림 방지)
    public float monospaceWidth = 0.6f; // 글자간 간격 (em 단위)

    // ⭐️ 오브젝트가 켜질 때(활성화될 때) 자동으로 효과가 시작되도록 OnEnable 사용
    private void OnEnable()
    {
        if (textComponent != null)
        {
            StartCoroutine(RevealRoutine());
        }
    }

    private IEnumerator RevealRoutine()
    {
        textComponent.text = "";
        string currentRevealed = ""; // 지금까지 확정된 올바른 글자들

        for (int i = 0; i < targetText.Length; i++)
        {
            // 띄어쓰기나 줄바꿈은 효과 없이 그냥 넘김
            if (targetText[i] == ' ' || targetText[i] == '\n')
            {
                currentRevealed += targetText[i];
                continue;
            }

            // 진짜 글자가 확정되기 전에 가짜(랜덤) 글자들을 타다닥! 보여줌
            for (int j = 0; j < randomChangesPerLetter; j++)
            {
                char randomChar = randomChars[Random.Range(0, randomChars.Length)];

                // 만약 뒤에 남은 빈자리도 전부 무작위 문자로 채우고 싶다면 아래 로직을 씁니다.
                string suffix = "";
                for (int k = i + 1; k < targetText.Length; k++)
                {
                    suffix += targetText[k] == ' ' ? " " : randomChars[Random.Range(0, randomChars.Length)].ToString();
                }

                // 확정된 글자 + 현재 깜빡이는 랜덤 글자 + 뒤에 남은 랜덤 글자들
                string output = currentRevealed + randomChar + suffix;
                if (useMonospace) output = $"<mspace={monospaceWidth}em>{output}</mspace>";
                
                textComponent.text = output;

                // 아주 짧은 시간 대기 (타임스케일 영향 안 받게 Realtime 사용)
                yield return new WaitForSecondsRealtime(revealSpeed / randomChangesPerLetter);
            }

            // 시간 경과 후, 진짜 글자로 확정
            currentRevealed += targetText[i];
            
            if (useMonospace) textComponent.text = $"<mspace={monospaceWidth}em>{currentRevealed}</mspace>";
            else textComponent.text = currentRevealed;
        }
    }
}