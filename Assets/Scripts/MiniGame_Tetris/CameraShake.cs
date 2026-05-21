using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    // 어디서든 쉽게 부를 수 있게 싱글톤 패턴 적용
    public static CameraShake Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    /// <summary>
    /// 카메라 진동을 발생시킵니다.
    /// </summary>
    /// <param name="duration">진동이 지속되는 시간 (초)</param>
    /// <param name="magnitude">진동의 세기 (흔들리는 범위)</param>
    public void Shake(float duration, float magnitude)
    {
        // 이미 흔들리고 있을 수 있으니 모든 코루틴을 멈추고 새로 시작
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        // 원래 카메라 위치 저장 (진동이 끝나면 돌아와야 함)
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // 랜덤한 X, Y 위치 생성
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // 카메라 위치 흔들기
            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;

            yield return null; // 다음 프레임까지 대기
        }

        // 진동이 끝나면 원래 위치로 완벽하게 복구
        transform.localPosition = originalPos;
    }
}