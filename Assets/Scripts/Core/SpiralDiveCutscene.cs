using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SpiralDiveCutscene : MonoBehaviour
{
    public static SpiralDiveCutscene Instance { get; private set; }

    [Header("Cinemachine")]
    [Tooltip("컷씬 전용 VCam (Priority 0으로 시작, 재생 중 20으로 올라감)")]
    [SerializeField] private CinemachineCamera cutsceneVCam;
    [Tooltip("카메라가 바라볼 중심점 (씬 중앙 빈 오브젝트)")]
    [SerializeField] private Transform lookAtTarget;

    [Header("Spiral Path")]
    [Tooltip("6개 문이 모두 보이는 시작 높이 — 씬에서 직접 확인 후 조정")]
    [SerializeField] private float startHeight = 25f;
    [Tooltip("나선 시작 반경 (중심에서 XZ 거리)")]
    [SerializeField] private float startRadius = 10f;
    [Tooltip("줌인 후 최종 높이")]
    [SerializeField] private float endHeight = 8f;
    [Tooltip("줌인 후 최종 반경 (0에 가까울수록 정중앙)")]
    [SerializeField] private float endRadius = 0.5f;
    [Tooltip("나선 회전 횟수")]
    [SerializeField] private float spiralTurns = 1.5f;
    [SerializeField] private float duration = 4f;

    [Header("Motion Blur")]
    [SerializeField] private PostProcessVolume postProcessVolume;
    [SerializeField] private float maxShutterAngle = 270f;

    [Header("Easing")]
    [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private UnityEngine.Rendering.PostProcessing.MotionBlur motionBlur;
    private const string OPENING_PLAYED_KEY = "OpeningCutscenePlayed";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // MiniGameManager.Start() → WakeUp 콜백에서 호출
    // onComplete: 컷씬 끝나면 EnablePlayerInput 등을 실행
    public void PlayIfFirstTime(Action onComplete = null)
    {
        if (PlayerPrefs.GetInt(OPENING_PLAYED_KEY, 0) == 1)
        {
            onComplete?.Invoke();
            return;
        }
        StartCoroutine(SpiralRoutine(onComplete));
    }

    public void PlayForced(Action onComplete = null)
    {
        StartCoroutine(SpiralRoutine(onComplete));
    }

    [ContextMenu("Reset Opening Cutscene (Test)")]
    public void ResetPlayedFlag()
    {
        PlayerPrefs.DeleteKey(OPENING_PLAYED_KEY);
        PlayerPrefs.Save();
        Debug.Log("[SpiralDiveCutscene] Reset: 다음 실행 시 컷씬이 재생됩니다.");
    }

    private IEnumerator SpiralRoutine(Action onComplete)
    {
        if (postProcessVolume != null)
            postProcessVolume.profile.TryGetSettings(out motionBlur);

        cutsceneVCam.Priority = 20;
        ApplyPosition(0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 아무 키 누르면 스킵
            if (Input.anyKeyDown)
                break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            ApplyPosition(t);

            // 모션블러: 중간 지점에서 최대
            if (motionBlur != null)
                motionBlur.shutterAngle.Override(Mathf.Sin(t * Mathf.PI) * maxShutterAngle);

            yield return null;
        }

        ApplyPosition(1f);
        if (motionBlur != null) motionBlur.shutterAngle.Override(0f);

        cutsceneVCam.Priority = 0;

        PlayerPrefs.SetInt(OPENING_PLAYED_KEY, 1);
        PlayerPrefs.Save();

        onComplete?.Invoke();
    }

    private void ApplyPosition(float t)
    {
        float eased = easing.Evaluate(t);
        float angle = eased * spiralTurns * 2f * Mathf.PI;
        float radius = Mathf.Lerp(startRadius, endRadius, eased);
        float height = Mathf.Lerp(startHeight, endHeight, eased);

        Vector3 center = lookAtTarget != null ? lookAtTarget.position : Vector3.zero;
        cutsceneVCam.transform.position = new Vector3(
            center.x + Mathf.Cos(angle) * radius,
            center.y + height,
            center.z + Mathf.Sin(angle) * radius
        );
        cutsceneVCam.transform.LookAt(center);
    }
}
