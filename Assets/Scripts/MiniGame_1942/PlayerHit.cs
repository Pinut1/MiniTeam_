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

        /// <summary>
        /// Initializes component references used by the player: caches the local FormationManager and collects all child SpriteRenderer components (including inactive ones).
        /// </summary>
        void Start()
        {
            formation = GetComponent<FormationManager>();
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        /// <summary>
        /// Handles trigger collisions with enemy objects and enemy bullets, applying damage and starting temporary invincibility.
        /// </summary>
        /// <param name="other">The collider that entered this trigger.</param>
        /// <remarks>
        /// Collisions are ignored while the player is already invincible or when god mode is enabled. For enemy or enemy-bullet collisions, this method applies a hit to the player's formation and initiates the invincibility/blinking routine.
        /// </remarks>
        void OnTriggerEnter(Collider other)
        {
            if (isInvincible || IsGodMode) return;

            if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet"))
            {
                formation.TakeHit();
                StartCoroutine(InvincibleRoutine());
            }
        }

        /// <summary>
        /// Temporarily sets the player to an invincible state and blinks its sprite renderers for the configured duration.
        /// </summary>
        /// <returns>An IEnumerator to be used as a coroutine that runs the blinking invincibility sequence and restores renderer visibility when finished.</returns>
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

        /// <summary>
        /// Set the enabled state of all cached child SpriteRenderer components.
        /// </summary>
        /// <param name="visible">`true` to enable the renderers, `false` to disable them. Null entries in the cached array are ignored.</param>
        void SetRenderersVisible(bool visible)
        {
            foreach (var sr in renderers)
                if (sr != null) sr.enabled = visible;
        }
    }
}
