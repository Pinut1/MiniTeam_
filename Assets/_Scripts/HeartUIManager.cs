using UnityEngine;
using UnityEngine.UI;

public class HeartUIManager : MonoBehaviour
{
    public static HeartUIManager instance;

    [Header("UI Settings")]
    public Image[] heartSlots;             // 10개의 UI 슬롯
    public Sprite[] possibleHeartSprites;  // 등장 가능한 하트 종류들
    public float uncollectedAlpha = 0.3f;  // 미수집 상태 투명도

    private bool[] isCollected;

    void Awake()
    {
        if (instance == null) instance = this;

        // 게임 시작 시 하트 슬롯 개수만큼 isCollected 배열 방을 무조건 생성합니다!
        if (heartSlots != null)
        {
            isCollected = new bool[heartSlots.Length];

            // 시작할 때 모든 UI 하트를 반투명하게 만듭니다.
            for (int i = 0; i < heartSlots.Length; i++)
            {
                if (heartSlots[i] != null)
                {
                    SetAlpha(heartSlots[i], uncollectedAlpha);
                    isCollected[i] = false; // 전부 수집 안 된 상태로 초기화
                }
            }
        }
    }

    public bool CollectHeart(Sprite collectedSprite)
    {
        // 주워먹은 하트 아이템에 그림이 아예 없으면 조용히 취소
        if (collectedSprite == null)
        {
            return false;
        }

        // 혹시라도 isCollected 배열이 날아갔다면 강제 재생성
        if (isCollected == null || isCollected.Length != heartSlots.Length)
        {
            isCollected = new bool[heartSlots.Length];
        }

        // 10개의 UI 보드판 칸을 하나하나 검사합니다.
        for (int i = 0; i < heartSlots.Length; i++)
        {
            // UI 슬롯 자체가 비어있거나, Source Image에 그림이 안 들어있으면 건너뜀!
            if (heartSlots[i] != null && heartSlots[i].sprite != null)
            {
                // 아직 수집 안 한 칸이고, 먹은 하트랑 그림 이름이 똑같다면!
                if (!isCollected[i] && heartSlots[i].sprite.name == collectedSprite.name)
                {
                    isCollected[i] = true;         // 수집 완료 처리
                    SetAlpha(heartSlots[i], 1f);   // 100% 불투명하게 불 켜기

                    CheckAllHeartsCollected();
                    return true; // UI 채우기 성공!
                }
            }
        }
        return false;
    }

    private void CheckAllHeartsCollected()
    {
        for (int i = 0; i < isCollected.Length; i++)
        {
            if (!isCollected[i]) return; // 하나라도 안 켜진 게 있으면 바로 종료
        }
        FindAnyObjectByType<PlayerLaser>().TriggerAllNpcsExit();
    }

    private void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}