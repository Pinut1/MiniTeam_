using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace MiniTeam.UI
{
    [RequireComponent(typeof(Selectable))]
    public class ButtonHoverColorInvert : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Header("Target Graphics (Auto-assigned if empty)")]
        [SerializeField] private Graphic targetBackground;
        [SerializeField] private Graphic targetText;

        private Color originalBgColor;
        private Color originalTextColor;

        private void Awake()
        {
            if (targetBackground == null) targetBackground = GetComponent<Image>();
            if (targetText == null) targetText = GetComponentInChildren<TMP_Text>();

            if (targetBackground != null) originalBgColor = targetBackground.color;
            if (targetText != null) originalTextColor = targetText.color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // If the button is not interactable, do not show hover effect
            Selectable selectable = GetComponent<Selectable>();
            if (selectable != null && !selectable.interactable) return;

            if (targetBackground != null) targetBackground.color = InvertColor(originalBgColor);
            if (targetText != null) targetText.color = InvertColor(originalTextColor);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            RestoreColors();
        }

        public void OnSelect(BaseEventData eventData)
        {
            Selectable selectable = GetComponent<Selectable>();
            if (selectable != null && !selectable.interactable) return;

            if (targetBackground != null) targetBackground.color = InvertColor(originalBgColor);
            if (targetText != null) targetText.color = InvertColor(originalTextColor);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            RestoreColors();
        }

        private void OnDisable()
        {
            // Restore colors when the object is disabled to prevent glitches when re-enabled
            RestoreColors();
        }

        private void RestoreColors()
        {
            if (targetBackground != null) targetBackground.color = originalBgColor;
            if (targetText != null) targetText.color = originalTextColor;
        }

        private Color InvertColor(Color c)
        {
            // Invert RGB values (1 - value) while keeping the original Alpha
            return new Color(1f - c.r, 1f - c.g, 1f - c.b, c.a);
        }
    }
}
