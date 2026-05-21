using UnityEngine;

public class BanillaNpcManager : MonoBehaviour
{
    public Animator playerAnim; // 플레이어 애니메이터만 할당해주세요

    private GameObject playerRedLaser;
    private GameObject banillaLaser;
    public Sprite heartSprite;

    void Start()
    {
        // [수정된 부분] 인스펙터 연결 안 해도 알아서 player 오브젝트의 애니메이터를 찾습니다.
        if (playerAnim == null)
        {
            GameObject playerObj = GameObject.Find("player");
            if (playerObj != null)
            {
                // 부모에게 있으면 가져오고, 없으면 자식(Player_walk 등)에서 찾아옵니다.
                playerAnim = playerObj.GetComponent<Animator>();
                if (playerAnim == null)
                {
                    playerAnim = playerObj.GetComponentInChildren<Animator>();
                }
            }
        }

        // 1. 바닐라 레이저 찾기
        Transform laserTransform = transform.Find("Laser_Yellow_0");
        if (laserTransform != null)
        {
            banillaLaser = laserTransform.gameObject;
            banillaLaser.SetActive(false);
        }

        // 2. 플레이어 빨간 레이저 찾기
        GameObject pObj = GameObject.Find("player");
        if (pObj != null)
        {
            Transform pLaserTransform = pObj.transform.Find("Laser");
            if (pLaserTransform != null)
            {
                playerRedLaser = pLaserTransform.gameObject;
            }
        }
    }

    private void OnMouseDown()
    {
        StartClash();
    }

    public void StartClash()
    {
        Debug.Log("플레이어 vs 바닐라 레이저 경쟁 시작!");

        if (playerAnim != null)
        {
            // 1. 기존 이동/대기 플래그를 전부 꺼서 Walk 상태로 튕기는 걸 막습니다.
            playerAnim.SetBool("isWalk", false);
            playerAnim.SetBool("isRun", false);
            playerAnim.SetBool("isIdle", false);

            // 2. 공격 상태를 켜고 트리거를 작동시킵니다.
            playerAnim.SetBool("isAttacking", true);
            playerAnim.SetTrigger("DoBackAttack");
        }

        // 플레이어 빨간 레이저 활성화
        if (playerRedLaser != null)
        {
            playerRedLaser.SetActive(true);
        }

        // 바닐라 노란 레이저 활성화
        if (banillaLaser != null)
        {
            banillaLaser.SetActive(true);
        }
    }
}