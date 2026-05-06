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

        void Start()
        {
            formation = GetComponent<FormationManager>();
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        void OnTriggerEnter(Collider other)
        {
            if (isInvincible || IsGodMode) return;

            if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet"))
            {
                formation.TakeHit();
                StartCoroutine(InvincibleRoutine());
            }
        }

        IEnumerator InvincibleRoutine()
        {
            isInvincible = true;

            float elapsed = 0f;
            while (elapsed < invincibleTime)
            {
                SetRenderersVisible(false);
                yield return new WaitForSeconds(blinkInterval);
                SetRenderersVisible(true);
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval * 2f;
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
