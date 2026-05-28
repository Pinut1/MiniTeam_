using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectShaker : MonoBehaviour
{
    private Vector3 originalLocalPos;
    private Coroutine shakeCoroutine;

    [Header("기본 진동 세팅 (인스펙터용)")]
    public float defaultDuration = 0.2f;
    public float defaultMagnitude = 0.1f;
    

    /// <summary>
    ///  인스펙터에 설정된 기본값으로 진동을 시작
    /// </summary>
    public void TriggerShake()
    {
        TriggerShake(defaultDuration, defaultMagnitude);
    }


    /// <summary>
    /// 외부 코드에서 원하는 시간과 세기를 주어 진동을 시작
    /// </summary>
    /// <param name="defaultDuration"></param>
    /// <param name="defaultMagnitude"></param>
    public void TriggerShake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.localPosition= originalLocalPos;  
        }

        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        // 진동 시작 직전의 로컬 위치를 기억 (기준점)
        originalLocalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            //원래 위치 기준 좌우, 위아래로 무작위 오프셋 계산
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // 기준점에 더해 줌으로써 제자리 진동 구현
            transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;

            yield return null;
        }
        // 진동이 완벽히 끝나면 한 치의 오차도 없이 원래 위치로 원상복구
        transform.localPosition= originalLocalPos;
        shakeCoroutine = null;
    }
}
