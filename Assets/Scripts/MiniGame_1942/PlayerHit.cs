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

        /// <summary>
        /// Handles trigger collisions: if the player is neither invincible nor in god mode, processes hits from objects tagged "Enemy" or "EnemyBullet".
        /// Plays the player hit SFX, notifies the formation of the hit, and starts temporary invincibility with blinking.
        /// </summary>
        /// <param name="other">The collider that entered the trigger.</param>
        void OnTriggerEnter(Collider other)
        {
            if (isInvincible || IsGodMode) return;

            if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet"))
            {
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.sfxPlayerHit);
                formation.TakeHit();
                StartCoroutine(InvincibleRoutine());
            }
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
