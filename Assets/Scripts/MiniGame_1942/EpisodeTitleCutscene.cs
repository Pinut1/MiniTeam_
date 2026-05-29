using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniTeam.Shooting1942
{
    public class EpisodeTitleCutscene : MonoBehaviour
    {
        [Header("배경")]
        public CanvasGroup backgroundGroup;
        public float bgFadeInDuration = 0.3f;

        [Header("파워퍼프걸 (Blossom / Bubbles / Buttercup 순서)")]
        public RectTransform[] girlTransforms;
        [Tooltip("화면 밖 시작 위치 (우상단)")]
        public Vector2 flyStartPos = new Vector2(1400f, 400f);
        [Tooltip("화면 밖 끝 위치 (우하단)")]
        public Vector2 flyEndPos   = new Vector2(1400f, -500f);
        [Tooltip("3명 간격 — 대각선 편대 형성")]
        public Vector2 girlOffset  = new Vector2(-120f, -80f);
        public float flyDuration   = 0.7f;
        public AnimationCurve flyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("에피소드 제목")]
        public CanvasGroup titleGroup;
        public TextMeshProUGUI titleText;
        public string episodeTitle = "모조조조를 무찌려라!";
        public float titleFadeInDuration = 0.4f;
        public float titleHoldDuration   = 2.0f;

        [Header("전체 루트 (페이드 아웃용 CanvasGroup)")]
        public CanvasGroup rootGroup;
        public float fadeOutDuration = 0.4f;

        private bool _skipped;

        public void Play(Action onComplete)
        {
            gameObject.SetActive(true);
            StartCoroutine(CutsceneRoutine(onComplete));
        }

        void Update()
        {
            if (!_skipped &&
                (Input.GetKeyDown(KeyCode.Space) ||
                 Input.GetKeyDown(KeyCode.Return) ||
                 Input.GetKeyDown(KeyCode.Escape)))
                _skipped = true;
        }

        IEnumerator CutsceneRoutine(Action onComplete)
        {
            _skipped = false;

            // 초기화
            if (titleText  != null) titleText.text  = episodeTitle;
            if (titleGroup != null) { titleGroup.alpha = 0f; }
            if (rootGroup  != null) { rootGroup.alpha  = 1f; }

            for (int i = 0; i < girlTransforms.Length; i++)
                if (girlTransforms[i] != null)
                    girlTransforms[i].anchoredPosition = flyStartPos + girlOffset * i;

            // 1. 배경 페이드인
            yield return StartCoroutine(FadeGroup(backgroundGroup, 0f, 1f, bgFadeInDuration));
            if (_skipped) { Finish(onComplete); yield break; }

            // 2. 파워퍼프걸 3명 동시 이동
            yield return StartCoroutine(FlyGirls());
            if (_skipped) { Finish(onComplete); yield break; }

            // 3. 에피소드 제목 페이드인
            yield return StartCoroutine(FadeGroup(titleGroup, 0f, 1f, titleFadeInDuration));
            if (_skipped) { Finish(onComplete); yield break; }

            // 4. 유지 (스킵 대기)
            float elapsed = 0f;
            while (elapsed < titleHoldDuration && !_skipped)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // 5. 전체 페이드 아웃
            yield return StartCoroutine(FadeGroup(rootGroup, 1f, 0f, fadeOutDuration));

            Finish(onComplete);
        }

        IEnumerator FlyGirls()
        {
            float elapsed = 0f;
            while (elapsed < flyDuration && !_skipped)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = flyCurve.Evaluate(Mathf.Clamp01(elapsed / flyDuration));

                for (int i = 0; i < girlTransforms.Length; i++)
                {
                    if (girlTransforms[i] == null) continue;
                    Vector2 start = flyStartPos + girlOffset * i;
                    Vector2 end   = flyEndPos   + girlOffset * i;
                    girlTransforms[i].anchoredPosition = Vector2.Lerp(start, end, t);
                }
                yield return null;
            }
        }

        IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
        {
            if (group == null) yield break;
            float elapsed = 0f;
            group.alpha = from;
            while (elapsed < duration && !_skipped)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            group.alpha = to;
        }

        void Finish(Action onComplete)
        {
            StopAllCoroutines();
            if (rootGroup != null) rootGroup.alpha = 0f;
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }
    }
}
