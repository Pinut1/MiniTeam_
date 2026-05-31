using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class EyeOpeningEffect : MonoBehaviour
{
    public static EyeOpeningEffect Instance { get; private set; }

    [Range(0.001f, 1.0f)] public float openAmount = 0.001f;
    [Range(0.0f, 2.0f)] public float expand = 0.0f;
    public float smoothness = 0.1f;

    private Image targetImage;
    private Material effectMaterial;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            if (Instance == null) Instance = this;
        }

        targetImage = GetComponent<Image>();
        if (targetImage != null && targetImage.material != null)
        {
            effectMaterial = new Material(targetImage.material);
            targetImage.material = effectMaterial;
        }
    }

    private void Update()
    {
        if (effectMaterial != null)
        {
            effectMaterial.SetFloat("_OpenAmount", openAmount);
            effectMaterial.SetFloat("_Expand", expand);
            effectMaterial.SetFloat("_Smoothness", smoothness);
        }
    }

    private void OnDestroy()
    {
        if (Application.isPlaying && Instance == this) Instance = null;

        if (effectMaterial != null)
        {
            if (Application.isPlaying) Destroy(effectMaterial);
            else DestroyImmediate(effectMaterial);
        }
    }

    // --- 애니메이션 연출 메서드 ---

    public void PlayWakeUpComplex(float openSpeed, Action onComplete = null)
    {
        gameObject.SetActive(true);
        enabled = true;
        StartCoroutine(WakeUpComplexRoutine(openSpeed, onComplete));
    }

    private IEnumerator WakeUpComplexRoutine(float openSpeed, Action onComplete)
    {
        openAmount = 0.001f;
        expand = 0.0f;
        float t = 0;

        while (t < 0.8f)
        {
            t += Time.deltaTime * openSpeed;
            openAmount = Mathf.Lerp(0.001f, 1.0f, t);
            yield return null;
        }

        while (t > 0.001f)
        {
            t -= Time.deltaTime * openSpeed;
            openAmount = Mathf.Lerp(0.001f, 1.0f, t);
            yield return null;
        }

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed * 2;
            openAmount = Mathf.Lerp(0.001f, 1.0f, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (openSpeed * 1.5f);
            expand = Mathf.Lerp(0.0f, 1.5f, t);
            yield return null;
        }

        gameObject.SetActive(false);
        enabled = false;
        onComplete?.Invoke();
    }

    public void PlayGoToSleep(float closeSpeed, Action onComplete = null)
    {
        gameObject.SetActive(true);
        enabled = true;
        StartCoroutine(GoToSleepRoutine(closeSpeed, onComplete));
    }

    private IEnumerator GoToSleepRoutine(float closeSpeed, Action onComplete)
    {
        openAmount = 1.0f;
        expand = 0.3f;
        float t = 1.0f;

        while (t > 0.001f)
        {
            t -= Time.deltaTime * closeSpeed;
            openAmount = t;
            yield return null;
        }

        openAmount = 0.001f;
        onComplete?.Invoke();
    }

    public void PlayWakeUpSimple(float openSpeed, Action onComplete = null)
    {
        gameObject.SetActive(true);
        enabled = true;
        StartCoroutine(WakeUpSimpleRoutine(openSpeed, onComplete));
    }

    private IEnumerator WakeUpSimpleRoutine(float openSpeed, Action onComplete)
    {
        openAmount = 0.001f;
        expand = 0.3f;
        float t = 0.0f;

        while (t < 1.0f)
        {
            t += Time.deltaTime * openSpeed;
            openAmount = t;
            yield return null;
        }

        openAmount = 1.0f;
        
        gameObject.SetActive(false);
        enabled = false;
        onComplete?.Invoke();
    }
}
