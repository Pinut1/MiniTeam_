using System;
using System.Collections;
using UnityEngine;

public class LetterBoxManager : MonoBehaviour
{
    public static LetterBoxManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;

    [Header("Settings")]
    [SerializeField] private float targetHeight = 150f;
    [SerializeField] private float animationDuration = 0.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public void ShowBars(Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateBars(targetHeight, onComplete));
    }

    public void HideBars(Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateBars(0f, onComplete));
    }

    private IEnumerator AnimateBars(float targetHeight, Action onComplete)
    {
        float startHeight = topBar.sizeDelta.y;
        float t = 0f;
        while (t < animationDuration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.SmoothStep(0f, 1f, t / animationDuration);

            float currentHeight = Mathf.Lerp(startHeight, targetHeight, lerpT);

            // X(너비)는 그대로 두고 Y(높이)만 변경
            topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, currentHeight);
            bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, currentHeight);

            yield return null;
        }

        // 미세한 오차 보정
        topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, targetHeight);
        bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, targetHeight);

        onComplete?.Invoke();
    }
}
