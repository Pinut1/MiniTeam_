using UnityEngine;
using TMPro;
using System.Collections;

public class MapDialogueUI : MonoBehaviour
{
    public static MapDialogueUI Instance;

    [Header("UI 연결")]
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public IEnumerator Show(string text)
    {
        // 1. 대화창 켜기
        dialoguePanel.SetActive(true);
        dialogueText.text = text;

        // 2. 클릭 쿨타임 (0.2초간 입력을 무시하여 클릭이 겹치는 것 방지)
        yield return new WaitForSecondsRealtime(0.2f);

        // 3. 클릭 대기 루프
        bool isWaiting = true;
        while (isWaiting)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
            {
                isWaiting = false;
            }
            yield return null;
        }

        // 4. 대화창 끄기
        dialoguePanel.SetActive(false);

        // 5. 다음 대사로 넘어가기 전 아주 짧은 여유시간
        yield return new WaitForSecondsRealtime(0.1f);
    }
}