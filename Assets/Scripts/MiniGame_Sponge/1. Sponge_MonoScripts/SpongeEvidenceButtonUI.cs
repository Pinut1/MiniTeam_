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
    [SerializeField] private Image iconImg;
    [SerializeField] private Image borderImg;

    [Header("상태별 스프라이트")]
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite filledSprite;
    [SerializeField] private Sprite hightlightedSprite;

    private SlotState currentState = SlotState.Empty;

    private float originalBorderY;
    private bool isHighlighted = false;

    private void Awake()
    {
        if (borderImg != null)
            originalBorderY = borderImg.rectTransform.anchoredPosition.y;
    }

    public void Setup(SpongeEvidenceData data)
    {
        if (data == null)
        {
            EvidenceId = null;
            SetState(SlotState.Empty);
            return;
        }
        EvidenceId = data.id;
        if (nameTxt != null)        nameTxt.text        = data.evidenceName;
        if (descriptionTxt != null) descriptionTxt.text = data.description;
        SetState(SlotState.Filled);
    }

    public void SetHighlight(bool isSelected)
    {
        if (currentState == SlotState.Empty) return;
        SetState(isSelected ? SlotState.Highlight : SlotState.Filled);
    }

    void SetPosY(float y)
    {
        if (borderImg == null) return;
        var pos = borderImg.rectTransform.anchoredPosition;
        pos.y = y;
        borderImg.rectTransform.anchoredPosition = pos;
    }

    void ShiftChildrenY(float deltaY)
    {
        if (borderImg == null) return;
        var rt = borderImg.rectTransform;
        for (int i = 0; i < rt.childCount; i++)
        {
            var childRt = (RectTransform)rt.GetChild(i);
            var cp = childRt.anchoredPosition;
            cp.y += deltaY;
            childRt.anchoredPosition = cp;
        }
    }

    void SetState(SlotState newState)
    {
        currentState = newState;
        switch (newState)
        {
            case SlotState.Empty:
                if (borderImg != null)      { borderImg.sprite = emptySprite;       borderImg.SetNativeSize(); }
                if (iconImg != null)        iconImg.gameObject.SetActive(false);
                if (nameTxt != null)        nameTxt.gameObject.SetActive(false);
                if (descriptionTxt != null) descriptionTxt.gameObject.SetActive(false);
                if (isHighlighted) { ShiftChildrenY(12f); isHighlighted = false; }
                SetPosY(originalBorderY);
                break;
            case SlotState.Filled:
                if (borderImg != null)      { borderImg.sprite = filledSprite;       borderImg.SetNativeSize(); }
                if (iconImg != null)        iconImg.gameObject.SetActive(true);
                if (nameTxt != null)        nameTxt.gameObject.SetActive(true);
                if (descriptionTxt != null) descriptionTxt.gameObject.SetActive(true);
                if (isHighlighted) { ShiftChildrenY(12f); isHighlighted = false; }
                SetPosY(originalBorderY);
                break;
            case SlotState.Highlight:
                if (borderImg != null)      { borderImg.sprite = hightlightedSprite; borderImg.SetNativeSize(); }
                if (!isHighlighted) { ShiftChildrenY(-12f); isHighlighted = true; }
                SetPosY(-174f);
                break;
        }
    }
}
