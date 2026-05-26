using UnityEngine;
using UnityEngine.UI;

public class PlayerUIScene : MonoBehaviour
{
    public static PlayerUIScene instance;

    [Header("UI 컴포넌트")]
    public Image playerUIImage;       // 배경 이미지
    public Image uiHeartImage;        // 하트 이미지
    public Image playerPortraitImage; // 쇼콜라 초상화 이미지

    [Header("교체할 배경 스프라이트들")]
    public Sprite bgNormal;
    public Sprite bgBanilla;
    public Sprite bgExtra;

    [Header("쇼콜라 초상화 스프라이트들")]
    public Sprite portraitIdle;      // UI_IDLE
    public Sprite portraitChecking;  // UI_Chacking
    public Sprite portraitKnockback; // UI_2

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        HideUIHeart();
        SetIdlePortrait(); // 시작할 때는 기본 표정으로 설정
    }

    // ==========================================
    // ★ 배경 관리 함수
    // ==========================================
    public void SetNormalBG() { if (playerUIImage != null && bgNormal != null) playerUIImage.sprite = bgNormal; }
    public void SetBanillaBG() { if (playerUIImage != null && bgBanilla != null) playerUIImage.sprite = bgBanilla; }
    public void SetExtraBG() { if (playerUIImage != null && bgExtra != null) playerUIImage.sprite = bgExtra; }

    // ==========================================
    // ★ 하트 관리 함수
    // ==========================================
    public void SetUIHeart(Sprite heartSprite)
    {
        if (uiHeartImage != null && heartSprite != null)
        {
            uiHeartImage.sprite = heartSprite;
            uiHeartImage.gameObject.SetActive(true);
        }
    }

    public void HideUIHeart()
    {
        if (uiHeartImage != null)
        {
            uiHeartImage.gameObject.SetActive(false);
        }
    }

    // ==========================================
    // ★ 초상화 관리 함수
    // ==========================================
    public void SetIdlePortrait()
    {
        if (playerPortraitImage != null && portraitIdle != null)
            playerPortraitImage.sprite = portraitIdle;
    }

    public void SetCheckingPortrait()
    {
        if (playerPortraitImage != null && portraitChecking != null)
            playerPortraitImage.sprite = portraitChecking;
    }

    public void SetKnockbackPortrait()
    {
        if (playerPortraitImage != null && portraitKnockback != null)
            playerPortraitImage.sprite = portraitKnockback;
    }
}