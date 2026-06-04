using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SpiralDiveCutscene : MonoBehaviour
{
    public static SpiralDiveCutscene Instance { get; private set; }

    [Header("카메라")]
    [Tooltip("비워두면 Camera.main 자동 사용")]
    [SerializeField] private Camera targetCamera;
    [Tooltip("카메라가 바라볼 중심점 (씬 중앙 빈 오브젝트)")]
    [SerializeField] private Transform lookAtTarget;

    [Header("Spiral Path")]
    [SerializeField] private float startHeight  = 25f;
    [SerializeField] private float startRadius  = 10f;
    [SerializeField] private float endHeight    = 8f;
    [SerializeField] private float endRadius    = 0.5f;
    [SerializeField] private float spiralTurns  = 1.5f;
    [SerializeField] private float duration     = 4f;

    [Header("Motion Blur")]
    [SerializeField] private PostProcessVolume postProcessVolume;
    [SerializeField] private float maxShutterAngle = 270f;

    [Header("Easing")]
    [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio")]
    [Tooltip("컷씬 전용 BGM (없으면 재생 안 함)")]
    [SerializeField] private AudioClip cutsceneBGM;

    private MotionBlur motionBlur;
    private const string OPENING_PLAYED_KEY = "OpeningCutscenePlayed";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null) { onComplete?.Invoke(); yield break; }

        if (cutsceneBGM != null && MiniTeam.Core.AudioManager.Instance != null)
        {
            MiniTeam.Core.AudioManager.Instance.PlayBGM(cutsceneBGM);
        }

        if (postProcessVolume != null)
            postProcessVolume.profile.TryGetSettings(out motionBlur);

        // 원래 카메라 상태 저장
        Vector3 originalPos = cam.transform.position;
        Quaternion originalRot = cam.transform.rotation;

        ApplyPosition(cam, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (Input.anyKeyDown) break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            ApplyPosition(cam, t);

            if (motionBlur != null)
                motionBlur.shutterAngle.Override(Mathf.Sin(t * Mathf.PI) * maxShutterAngle);

            yield return null;
        }

        // 컷씬 종료 후 카메라 원위치 복구
        cam.transform.position = originalPos;
        cam.transform.rotation = originalRot;
        if (motionBlur != null) motionBlur.shutterAngle.Override(0f);

        PlayerPrefs.SetInt(OPENING_PLAYED_KEY, 1);
        PlayerPrefs.Save();

        onComplete?.Invoke();
    }

    private void ApplyPosition(Camera cam, float t)
    {
        float eased = easing.Evaluate(t);
        float angle  = eased * spiralTurns * 2f * Mathf.PI;
        float radius = Mathf.Lerp(startRadius, endRadius, eased);
        float height = Mathf.Lerp(startHeight, endHeight, eased);

        Vector3 center = lookAtTarget != null ? lookAtTarget.position : Vector3.zero;
        cam.transform.position = new Vector3(
            center.x + Mathf.Cos(angle) * radius,
            center.y + height,
            center.z + Mathf.Sin(angle) * radius
        );
        cam.transform.LookAt(center);
    }
}
