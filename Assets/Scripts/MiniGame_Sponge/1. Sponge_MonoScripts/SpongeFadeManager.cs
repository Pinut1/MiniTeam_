using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전체 화면 페이드 인/아웃 담당
/// FadeIn  : 화면을 검정으로 (alpha 0→1)
/// FadeOut : 검정 화면을 걷어냄 (alpha 1→0)
/// IsFading : 페이드 중 입력 차단용 플래그
/// </summary>
public class SpongeFadeManager : MonoBehaviour
{
    public static SpongeFadeManager Instance { get; private set; }

    [SerializeField] private Image overlay; // 전체화면 검정 Image (Canvas 최상단 배치)

    public bool IsFading { get; private set; }

    private void Awake()
    {
        Instance = this;
        overlay.color = new Color(0f, 0f, 0f, 0f);
        overlay.gameObject.SetActive(false);
    }

    public IEnumerator FadeIn(float duration)
    {
        IsFading = true;
        overlay.gameObject.SetActive(true);
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            overlay.color = new Color(0f, 0f, 0f, Mathf.Clamp01(t / duration));
            yield return null;
        }
        overlay.color = new Color(0f, 0f, 0f, 1f);
    }

    public IEnumerator FadeOut(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            overlay.color = new Color(0f, 0f, 0f, Mathf.Clamp01(1f - t / duration));
            yield return null;
        }
        overlay.color = new Color(0f, 0f, 0f, 0f);
        overlay.gameObject.SetActive(false);
        IsFading = false;
    }
}
