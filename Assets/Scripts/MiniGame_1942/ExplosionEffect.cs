using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 폭발 프리팹에 붙이는 스크립트 — 애니메이션 길이만큼 재생 후 자동 삭제
    public class ExplosionEffect : MonoBehaviour
    {
        void Start()
        {
            var anim = GetComponent<Animator>();
            if (anim != null)
            {
                float length = anim.GetCurrentAnimatorStateInfo(0).length;
                Destroy(gameObject, length);
            }
            else
            {
                Destroy(gameObject, 1f);
            }
        }
    }
}
