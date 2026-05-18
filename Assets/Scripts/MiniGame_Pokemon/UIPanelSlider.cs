using System.Collections;
using UnityEngine;

namespace MiniTeam.Pokemon
{
    // 패널 슬라이드 인/아웃 유틸리티
    // 인스펙터에서 slideDist (숨길 때 이동 거리) 와 duration 설정
    public class UIPanelSlider : MonoBehaviour
    {
        public enum SlideDir { Left, Right, Up, Down }

        [Header("슬라이드 설정")]
        public SlideDir direction = SlideDir.Right;
        public float    slideDist = 400f;   // 화면 밖으로 밀어낼 거리 (px)
        public float    duration  = 0.25f;

        private RectTransform rt;
        private Vector2 visiblePos;
        private Vector2 hiddenPos;
        private bool    posReady;
        private Coroutine current;

        void Awake() => rt = GetComponent<RectTransform>();

        // 비활성 오브젝트는 Awake가 호출되지 않을 수 있으므로 첫 사용 시점에 초기화
        void EnsurePos()
        {
            if (posReady) return;
            if (rt == null) rt = GetComponent<RectTransform>(); // 비활성 시작 대비
            posReady   = true;
            visiblePos = rt.anchoredPosition;
            hiddenPos  = visiblePos + DirVector() * slideDist;
        }

        Vector2 DirVector() => direction switch
        {
            SlideDir.Left  => Vector2.left,
            SlideDir.Right => Vector2.right,
            SlideDir.Up    => Vector2.up,
            SlideDir.Down  => Vector2.down,
            _              => Vector2.right,
        };

        public void SlideIn(bool instant = false)
        {
            EnsurePos();
            if (current != null) StopCoroutine(current);
            gameObject.SetActive(true);
            if (instant) { rt.anchoredPosition = visiblePos; return; }
            rt.anchoredPosition = hiddenPos;
            current = StartCoroutine(Animate(hiddenPos, visiblePos));
        }

        public void SlideOut(bool instant = false, bool deactivateAfter = true)
        {
            EnsurePos();
            if (current != null) StopCoroutine(current);
            if (instant)
            {
                rt.anchoredPosition = hiddenPos;
                if (deactivateAfter) gameObject.SetActive(false);
                return;
            }
            current = StartCoroutine(Animate(rt.anchoredPosition, hiddenPos, deactivateAfter));
        }

        // 코루틴 버전 — current를 통일해 중간에 SlideIn/Out 호출로도 취소 가능
        public IEnumerator SlideInRoutine()
        {
            EnsurePos();
            if (current != null) StopCoroutine(current);
            gameObject.SetActive(true);
            rt.anchoredPosition = hiddenPos;
            current = StartCoroutine(Animate(hiddenPos, visiblePos));
            yield return current;
        }

        public IEnumerator SlideOutRoutine(bool deactivateAfter = true)
        {
            EnsurePos();
            if (current != null) StopCoroutine(current);
            current = StartCoroutine(Animate(rt.anchoredPosition, hiddenPos, deactivateAfter));
            yield return current;
        }

        IEnumerator Animate(Vector2 from, Vector2 to, bool deactivateAfter = false)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                rt.anchoredPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            rt.anchoredPosition = to;
            if (deactivateAfter) gameObject.SetActive(false);
        }
    }
}
