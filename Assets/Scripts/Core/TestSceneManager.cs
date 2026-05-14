using MiniTeam.Core;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TestSceneManager : MonoBehaviour
{
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text CurrentSceneText;
   
    void Start()
    {
        if (countdownText != null)
        {
            StartCoroutine(CountdownRoutine());
        }
        else
        {
            Debug.LogError("Text가 인스펙터에 연결되지 않았습니다.");
        }
        if (CurrentSceneText != null)
        {
            CurrentSceneText.text = "Current Stage\r\n" + MiniGameManager.Instance.currentStage;
        }
        else
        {
            Debug.LogError("Text가 인스펙터에 연결되지 않았습니다.");
        }
    }

    private IEnumerator CountdownRoutine()
    {
        int count = 3;
        while (count >0)
        {
            countdownText.text = count.ToString() + " 초";
            yield return new WaitForSeconds(1f);
            count--;
        }

        countdownText.text = "Clear";

        yield return new WaitForSeconds(0.5f);

        MiniGameManager.Instance.OnMiniGameClear();
    }

  
}
