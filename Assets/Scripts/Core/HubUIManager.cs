using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class HubUIManager : MonoBehaviour
{
    public EyeOpeningEffect eyeEffect;
    public float openSpeed = 1.5f;
   public static HubUIManager Instance { get; private set; }

    [Header("인게임 UI Panels")]
    [SerializeField] private GameObject warningUI;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    // 경고 UI 제어
    public void ToggleWarningUI(bool isActive)
    {
        if (warningUI != null)
        {
            warningUI.SetActive(isActive);
        }
    }
    
    public void WakeUp()
    {
        if (eyeEffect != null)
        {
            // 1. 잠들어 있는 효과를 깨웁니다. 
            // 이게 없으면 OnRenderImage가 호출되지 않아 화면이 계속 암전되거나 변화가 없습니다.
            eyeEffect.enabled = true;

            StartCoroutine(WakeUpRoutine());
        }
    }

    private IEnumerator WakeUpRoutine()
    {
        // 초기화
        eyeEffect.openAmount = 0.001f;
        eyeEffect.expand = 0.0f;

        float t = 0;

        // [1단계] 눈 뜨기 (가느다란 선 -> 타원형)
        // openAmount를 0.001에서 1.0까지 올립니다.
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

        while (t< 1f)
        {
            t += Time.deltaTime * openSpeed * 2;
            eyeEffect.openAmount = Mathf.Lerp(0.001f, 1.0f, t);
            yield return null;
        }

        // 잠시 초점을 맞추는 듯한 0.2초의 멈춤 (몰입감 up)
        yield return new WaitForSeconds(0.1f);

        // [2단계] 시야 확장 (타원형 -> 전체 화면)
        // expand를 0.0에서 1.5까지 올려서 화면 구석까지 밝힙니다.
        t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (openSpeed * 1.5f); // 확장은 좀 더 빠르게
            eyeEffect.expand = Mathf.Lerp(0.0f, 1.5f, t);
            yield return null;
        }

        // 연출이 끝난 후 효과 스크립트 자체를 꺼서 성능 최적화
        eyeEffect.enabled = false;
    }
}
