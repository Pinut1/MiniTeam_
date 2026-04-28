using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 역할: 피격 감지 → FormationManager 호출
    public class PlayerHit : MonoBehaviour
    {
        public float invincibleTime = 2f;

        private FormationManager formation;
        private bool isInvincible = false;

        void Start()
        {
            formation = GetComponent<FormationManager>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (isInvincible) return;

            if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet"))
            {
                formation.TakeHit();
                StartCoroutine(InvincibleRoutine());
            }
        }

        System.Collections.IEnumerator InvincibleRoutine()
        {
            isInvincible = true;
            yield return new WaitForSeconds(invincibleTime);
            isInvincible = false;
        }
    }
}
