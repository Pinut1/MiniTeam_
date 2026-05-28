using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Opening_Dialogue : MonoBehaviour
{
    public static Opening_Dialogue Instance;

    [Header("UI 연결")]
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator Show(string text, bool isLast = false)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueText != null) dialogueText.text = text;

        yield return new WaitForSecondsRealtime(0.3f); // 입력 씹힘 방지

        bool isWaiting = true;
        while (isWaiting)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z))
            {
                isWaiting = false;
            }
            yield return null;
        }

        if (isLast && dialoguePanel != null) 
        {
            dialoguePanel.SetActive(false);
        }
    }
}