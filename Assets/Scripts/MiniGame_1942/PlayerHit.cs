using System.Collections;
using UnityEngine;

namespace MiniTeam.Shooting1942
{
    public class PlayerHit : MonoBehaviour
    {
        public float invincibleTime = 2f;
        public float blinkInterval  = 0.1f;

        public bool IsGodMode = false;

        private FormationManager formation;
        private SpriteRenderer[]  renderers;
        private bool isInvincible = false;
        public bool IsInvincible => isInvincible;

        void Start()
        {
            formation = GetComponent<FormationManager>();
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        // HitboxPoint에서 호출 — 피탄점에 맞았을 때
        // ※ 플레이어 프리팹에 HitboxPoint 컴포넌트 필수 (없으면 피격 판정 없음)
        public void TakeHit()
        {
            if (isInvincible || IsGodMode) return;

            AudioManager.Instance?.PlaySFX(AudioManager.Instance.sfxPlayerHit);
            formation.TakeHit();
            StartCoroutine(InvincibleRoutine());
        }

        IEnumerator InvincibleRoutine()
        {
            isInvincible = true;

            float step    = Mathf.Max(0.01f, blinkInterval);
            float elapsed = 0f;
            while (elapsed < invincibleTime)
            {
                SetRenderersVisible(false);
                yield return new WaitForSeconds(step);
                SetRenderersVisible(true);
                yield return new WaitForSeconds(step);
                elapsed += step * 2f;
            }

            SetRenderersVisible(true);
            isInvincible = false;
        }

        void SetRenderersVisible(bool visible)
        {
            foreach (var sr in renderers)
                if (sr != null) sr.enabled = visible;
        }
    }
}
