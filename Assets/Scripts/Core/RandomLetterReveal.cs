using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro 사용 필수

public class RandomLetterReveal : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_Text textComponent;

    [Header("효과 설정")]
    [TextArea]
    public string[] targetTexts = new string[] { "HELLO", "WORLD" }; // 순서대로 띄울 단어/문장들
    public float delayBetweenWords = 1.0f; // 다음 단어로 넘어가기 전 대기 시간
    public string randomChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*"; // 섞일 랜덤 문자들들

    public float revealSpeed = 0.05f; // 한 글자가 확정되는 시간
    public int randomChangesPerLetter = 3; // 글자 하나가 확정되기 전에 랜덤 문자가 깜빡이는 횟수

    [Header("흔들림 방지 설정")]
    public bool useMonospace = true; // 강제 고정폭 사용 여부 (글자 흔들림 방지)
    public float monospaceWidth = 0.6f; // 글자간 간격 (em 단위)
    
    [Header("순서 설정")]
    public bool randomRevealOrder = false; // 앞에서부터 순서대로 할지, 무작위 순서로 할지 결정

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
        for (int t = 0; t < targetTexts.Length; t++)
        {
            string targetText = targetTexts[t];
            textComponent.text = "";
            
            // 현재 화면에 보여질 글자 배열 (처음엔 모두 빈칸)
            char[] displayChars = new char[targetText.Length];
            bool[] isRevealed = new bool[targetText.Length];

            // 확정할 순서를 담은 리스트 만들기
            System.Collections.Generic.List<int> revealOrder = new System.Collections.Generic.List<int>();
            for (int i = 0; i < targetText.Length; i++)
            {
                if (targetText[i] == ' ' || targetText[i] == '\n')
                {
                    isRevealed[i] = true; // 공백이나 줄바꿈은 미리 확정된 것으로 취급
                    displayChars[i] = targetText[i];
                }
                else
                {
                    revealOrder.Add(i);
                }
            }

            // 무작위 순서 옵션이 켜져있다면 리스트 섞기 (Fisher-Yates 셔플)
            if (randomRevealOrder)
            {
                for (int i = 0; i < revealOrder.Count; i++)
                {
                    int temp = revealOrder[i];
                    int randomIndex = Random.Range(i, revealOrder.Count);
                    revealOrder[i] = revealOrder[randomIndex];
                    revealOrder[randomIndex] = temp;
                }
            }

            // 정해진 순서대로 하나씩 글자 확정해 나가기
            foreach (int targetIndex in revealOrder)
            {
                // 글자 하나가 확정되기 전에 전체 미확정 글자들이 랜덤 문자로 깜빡이는 루프
                for (int j = 0; j < randomChangesPerLetter; j++)
                {
                    // 아직 확정되지 않은 자리들은 모두 새로운 랜덤 문자로 갱신
                    for (int k = 0; k < targetText.Length; k++)
                    {
                        if (!isRevealed[k])
                        {
                            displayChars[k] = randomChars[Random.Range(0, randomChars.Length)];
                        }
                    }

                    // 텍스트 조합 및 표시
                    string output = new string(displayChars);
                    if (useMonospace) output = $"<mspace={monospaceWidth}em>{output}</mspace>";
                    
                    textComponent.text = output;

                    // 아주 짧은 시간 대기 (타임스케일 영향 안 받게 Realtime 사용)
                    yield return new WaitForSecondsRealtime(revealSpeed / randomChangesPerLetter);
                }

                // 깜빡임이 끝나면 해당 자리를 진짜 글자로 영구 확정
                isRevealed[targetIndex] = true;
                displayChars[targetIndex] = targetText[targetIndex];
                
                string finalOutput = new string(displayChars);
                if (useMonospace) textComponent.text = $"<mspace={monospaceWidth}em>{finalOutput}</mspace>";
                else textComponent.text = finalOutput;
            }

            // 모든 글자가 다 나온 뒤 다음 단어로 넘어가기 전 대기
            if (t < targetTexts.Length - 1)
            {
                yield return new WaitForSecondsRealtime(delayBetweenWords);
            }
        }
    }
}