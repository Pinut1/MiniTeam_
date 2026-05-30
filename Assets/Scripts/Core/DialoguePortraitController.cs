using UnityEngine;
using UnityEngine.UI;

public class DialoguePortraitController : MonoBehaviour
{
    public static DialoguePortraitController Instance { get; private set; }

    [Header("스테이지별 초상화 오브젝트")]
    [Tooltip("전체 초상화 연출을 통제할 애니메이터")]
    [SerializeField] private Animator animator;

    [Tooltip("0스테이지 튜토리얼용 초상화 (예: 주댕치)")]
    public GameObject stage0PortraitObj;
    
    [Tooltip("1스테이지 이후용 초상화")]
    public GameObject stage1PortraitObj;

    [Header("표정 제어용 이미지 컴포넌트")]
    [Tooltip("0스테이지 초상화의 Image 컴포넌트")]
    public Image stage0Image;

    [Tooltip("1스테이지 초상화의 Image 컴포넌트")]
    public Image stage1Image;

    [Header("스테이지 클리어 보상")]
    [SerializeField] private Image objectImg;
    [SerializeField] private Sprite[] stageClearSprites;

    private int currentActiveStage = -1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 시작 시 화면에 보이지 않도록 전원을 꺼둠 (캔버스 그룹 자동 부착 방지)
        if (stage0PortraitObj != null) stage0PortraitObj.SetActive(false);
        if (stage1PortraitObj != null) stage1PortraitObj.SetActive(false);
    }

    public void Show(int stage)
    {
        currentActiveStage = stage;
        
        if (stage == 0)
        {
            if (stage1PortraitObj != null) stage1PortraitObj.SetActive(false);
            if (stage0PortraitObj != null) stage0PortraitObj.SetActive(true);
            animator?.SetTrigger("ShowStage0Portrait");
        }
        else
        {
            if (stage0PortraitObj != null) stage0PortraitObj.SetActive(false);
            if (stage1PortraitObj != null) stage1PortraitObj.SetActive(true);
            animator?.SetTrigger("ShowStage1Portrait");
        }
    }

    public void Hide()
    {
        if (currentActiveStage == 0)
        {
            animator?.SetTrigger("HideStage0Portrait");
        }
        else if (currentActiveStage >= 1)
        {
            animator?.SetTrigger("HideStage1Portrait");
        }

        currentActiveStage = -1;
    }

    public void ChangeExpression(Sprite newSprite)
    {
        if (newSprite == null) return;

        if (currentActiveStage == 0 && stage0Image != null)
        {
            stage0Image.sprite = newSprite;
        }
        else if (currentActiveStage > 0 && stage1Image != null)
        {
            stage1Image.sprite = newSprite;
        }
    }

    public void PlayStageClear()
    {
        int currentStage = MiniTeam.Core.MiniGameManager.Instance != null ? MiniTeam.Core.MiniGameManager.Instance.progressData.currentStage : 0;
        int spriteIndex = currentStage - 1;
        
        if (objectImg != null && stageClearSprites != null)
        {
            if (spriteIndex >= 0 && spriteIndex < stageClearSprites.Length)
            {
                objectImg.sprite = stageClearSprites[spriteIndex];
            }
        }
        
        animator?.SetTrigger("StageClear_ObjectGet");
    }
}
