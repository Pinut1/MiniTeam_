using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 씬 전환을 위해 반드시 추가해야 합니다.

public class MainUIManager : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("화면을 덮을 검은색 UI 이미지 패널")]
    [SerializeField] private Image fadePanel;
    [Tooltip("어두워지는 데 걸리는 시간 (초)")]
    [SerializeField] private float fadeDuration = 2.0f;

    // 중복 클릭 방지용 플래그
    private bool isTranstioning = false;

    public void GameStart()
    {
        Debug.Log("0. GameStart Gone");
        if (isTranstioning) return;
        isTranstioning = true;
        Debug.Log("1. isTranstioning true");
        //SceneManager.LoadScene("Hub");

        // 페이드 아웃이 다 끝나면, Hub씬을 로드하게 로직을 넘김
        PlayFadeOut(() => SceneManager.LoadScene("Hub"));
    }

 

    // 게임을 종료하는 함수
    public void QuitGame()
    {
        if (isTranstioning) return;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("게임 종료 (에디터)");
#else
        // 실제 빌드된 게임(exe, apk 등)에서 플레이 중일 때
        Application.Quit();
#endif
    }


    private void PlayFadeOut(Action onComplete)
    {
        StartCoroutine(FadeRoutine(0, 1, onComplete));
     
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, Action onComplete)
    {
        
       fadePanel?.gameObject.SetActive(true);
        Color color = fadePanel.color;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadePanel.color = color;

        onComplete?.Invoke();
    }
}