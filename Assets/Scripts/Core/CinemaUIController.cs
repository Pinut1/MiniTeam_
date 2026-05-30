using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class CinemaUIController : MonoBehaviour
{
    public static CinemaUIController Instance { get; private set; }

    [Header("Letterbox Settings")]
    [SerializeField] private RectTransform topLetterbox;
    [SerializeField] private RectTransform bottomLetterbox;
    [SerializeField] private float letterboxTargetHeight = 150f;
    [SerializeField] private float letterboxDuration = 0.5f;

    [Header("WakeUp & Setup (오프닝 연출)")]
    public EyeOpeningEffect eyeEffect;
    [SerializeField] private float openSpeed = 1.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayLetterboxEnter(Action onComplete = null)
    {
        StartCoroutine(LetterboxRoutine(letterboxTargetHeight, onComplete));
    }

    public void PlayLetterboxExit(Action onComplete = null)
    {
        StartCoroutine(LetterboxRoutine(0f, onComplete));
    }

    private IEnumerator LetterboxRoutine(float targetHeight, Action onComplete)
    {
        if (topLetterbox == null || bottomLetterbox == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        float startTopHeight = topLetterbox.sizeDelta.y;
        float startBottomHeight = bottomLetterbox.sizeDelta.y;
        float elapsed = 0f;

        while (elapsed < letterboxDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / letterboxDuration;
            // 부드러운 시작과 끝을 위한 SmoothStep 보간
            t = t * t * (3f - 2f * t);

            topLetterbox.sizeDelta = new Vector2(topLetterbox.sizeDelta.x, Mathf.Lerp(startTopHeight, targetHeight, t));
            bottomLetterbox.sizeDelta = new Vector2(bottomLetterbox.sizeDelta.x, Mathf.Lerp(startBottomHeight, targetHeight, t));
            
            yield return null;
        }

        topLetterbox.sizeDelta = new Vector2(topLetterbox.sizeDelta.x, targetHeight);
        bottomLetterbox.sizeDelta = new Vector2(bottomLetterbox.sizeDelta.x, targetHeight);
        
        onComplete?.Invoke();
    }

    public void PlayWakeUp(Action onComplete = null)
    {
        if (eyeEffect != null)
        {
            eyeEffect.enabled = true;
            StartCoroutine(WakeUpRoutine(onComplete));
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    private IEnumerator WakeUpRoutine(Action onComplete)
    {
        // Start Wake Up Animation
        eyeEffect.openAmount = 0.001f;
        eyeEffect.expand = 0.0f;
        float t = 0;

        while (t < 0.8f)
        {
            t += Time.deltaTime * openSpeed;
            eyeEffect.openAmount = Mathf.Lerp(0.001f, 1.0f, t);
            yield return null;
        }

        while (t > 0.001f)
        {
            t -= Time.deltaTime * openSpeed;
            eyeEffect.openAmount = Mathf.Lerp(0.001f, 1.0f, t);
            yield return null;
        }

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed * 2;
            eyeEffect.openAmount = Mathf.Lerp(0.001f, 1.0f, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (openSpeed * 1.5f);
            eyeEffect.expand = Mathf.Lerp(0.0f, 1.5f, t);
            yield return null;
        }

        eyeEffect.enabled = false;
        
        onComplete?.Invoke();
    }
}
