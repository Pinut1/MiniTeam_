using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent (typeof(RectTransform))]
public class UIShaker : MonoBehaviour
{
   private RectTransform m_RectTransform;
    private Vector2 originalAnchoredPos;
    private Coroutine shakeCoroutine;

    [Header("UI 진동 세팅")]
    public float defaultDuration = 0.5f;
    public float defaultMagnitude = 15f; // 일반 오브젝트와 달리 픽셀 단위라 10~20정도 줘야함

    private void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();
    }

    public void TriggerShake()
    {
        TriggerShake(defaultDuration, defaultMagnitude);
    }

    public void TriggerShake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            m_RectTransform.anchoredPosition = originalAnchoredPos;
        }
        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        // 진동 시작 직전의 로컬 위치를 기억 (기준점)
        originalAnchoredPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            //원래 위치 기준 좌우, 위아래로 무작위 오프셋 계산
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // 기준점에 더해 줌으로써 제자리 진동 구현
            transform.localPosition = originalAnchoredPos + new Vector2(x, y);
            elapsed += Time.deltaTime;

            yield return null;
        }
        // 진동이 완벽히 끝나면 한 치의 오차도 없이 원래 위치로 원상복구
        transform.localPosition = originalAnchoredPos;
        shakeCoroutine = null;

    }
}
