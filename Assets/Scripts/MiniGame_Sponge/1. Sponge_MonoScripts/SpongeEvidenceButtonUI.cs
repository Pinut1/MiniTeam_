using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpongeEvidenceButtonUI : MonoBehaviour
{
    public enum SlotState
    {
        Empty, Filled, Highlight
    }
     
    public string EvidenceId { get; private set; }

    [SerializeField] private TMP_Text nameTxt;
    [SerializeField] private TMP_Text descriptionTxt;
    [SerializeField] private Image iconImg; // 증거품 이미지
    [SerializeField] private Image borderImg; // 슬롯 전체 테두리 이미지

    [Header("상태별 스프라이트")]
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite filledSprite;
    [SerializeField] private Sprite hightlightedSprite;

    // 현재 슬롯 상태
    private SlotState currentState = SlotState.Empty;

    // 클릭 이벤트 - UIManager에서 사용
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    /// <summary>
    /// 증거 데이터로 슬롯 초기화
    /// UIManager에서 버튼 생성시 호출
    /// data가 null이면 Empty 상태
    /// </summary>
    /// <param name="data"></param>
    public void Setup(SpongeEvidenceData data)
    {
        // 데이터 없음 == Empty
        if (data == null)
        {
            SetState(SlotState.Empty);
            return;
        }
        // 데이터 있음 == Filled
        EvidenceId = data.id;
        nameTxt.text = data.evidenceName;
        descriptionTxt.text = data.description;

        SetState(SlotState.Filled);
    }

    /// <summary>
    /// Highlight 상태 전환
    /// true -> Highlight, false -> Filled
    /// Empty 상태에서 호출 무시
    /// </summary>
    /// <param name="isSelected"></param>
    public void SetHighlight(bool isSelected)
    {
        if (currentState == SlotState.Empty) return;
        SetState(isSelected ? SlotState.Highlight : SlotState.Filled);
    }

    /// <summary>
    /// 슬롯 상태 전환 - 스프라이트 / 클릭 가능 여부 갱신
    /// </summary>
    /// <param name="newState"></param>
    void SetState(SlotState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case SlotState.Empty:
                borderImg.sprite = emptySprite;
                iconImg.gameObject.SetActive(false);
                nameTxt.text = "";
                descriptionTxt.text = "";
                button.interactable = false; // 클릭 불가 처리
                break;
            case SlotState.Filled:
                borderImg.sprite = filledSprite;
                iconImg.gameObject.SetActive(true);
                button.interactable = true; // 클릭 가능
                break;
            case SlotState.Highlight:
                borderImg.sprite = hightlightedSprite;
                button.interactable = true;
                break;
            default:
                break;
        }
    }
}
