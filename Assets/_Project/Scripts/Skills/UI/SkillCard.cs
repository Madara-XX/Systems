using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace RoombaRampage.UI
{
    /// <summary>
    /// Individual skill card UI component.
    /// Displays skill information and handles selection.
    /// </summary>
    public class SkillCard : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private GameObject newBanner;
        [SerializeField] private Image rarityBanner;
        [SerializeField] private Button selectButton;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray
        [SerializeField] private Color uncommonColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Green
        [SerializeField] private Color rareColor = new Color(0.2f, 0.5f, 1f, 1f); // Blue
        [SerializeField] private Color epicColor = new Color(0.6f, 0.2f, 0.8f, 1f); // Purple
        [SerializeField] private Color legendaryColor = new Color(1f, 0.8f, 0.2f, 1f); // Gold

        [Header("Animation")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float animationSpeed = 10f;

        private Skills.SkillOffer currentOffer;
        private UnityAction<int> onSelectedCallback;
        private int cardIndex;
        private Vector3 originalScale;
        private bool isHovered = false;

        private void Awake()
        {
            originalScale = transform.localScale;

            // Setup button if present
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(OnCardClicked);
            }
        }

        /// <summary>
        /// Sets up the card with skill offer data.
        /// </summary>
        public void Setup(Skills.SkillOffer offer, int index, UnityAction<int> callback)
        {
            currentOffer = offer;
            cardIndex = index;
            onSelectedCallback = callback;

            // Set icon
            if (iconImage != null && offer.skillData.icon != null)
            {
                iconImage.sprite = offer.skillData.icon;
                iconImage.enabled = true;
            }
            else if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            // Set name
            if (nameText != null)
            {
                nameText.text = offer.skillData.skillName;
            }

            // Set level text
            if (levelText != null)
            {
                levelText.text = offer.GetLevelText();
            }

            // Set NEW banner visibility
            if (newBanner != null)
            {
                newBanner.SetActive(offer.isNewSkill);
            }

            // Set description
            if (descriptionText != null)
            {
                descriptionText.text = offer.GetDescription();
            }

            // Set rarity colors
            Color rarityColor = GetRarityColor(offer.rarity);

            if (borderImage != null)
            {
                borderImage.color = rarityColor;
            }

            if (rarityBanner != null)
            {
                rarityBanner.color = rarityColor;
            }

            // Make card visible and interactive
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Called when card is clicked.
        /// </summary>
        public void OnCardClicked()
        {
            onSelectedCallback?.Invoke(cardIndex);
        }

        /// <summary>
        /// Gets the color for a specific rarity.
        /// </summary>
        private Color GetRarityColor(Skills.SkillRarity rarity)
        {
            switch (rarity)
            {
                case Skills.SkillRarity.Common:
                    return commonColor;
                case Skills.SkillRarity.Uncommon:
                    return uncommonColor;
                case Skills.SkillRarity.Rare:
                    return rareColor;
                case Skills.SkillRarity.Epic:
                    return epicColor;
                case Skills.SkillRarity.Legendary:
                    return legendaryColor;
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Called when pointer enters card (for hover effect).
        /// </summary>
        public void OnPointerEnter()
        {
            isHovered = true;
        }

        /// <summary>
        /// Called when pointer exits card.
        /// </summary>
        public void OnPointerExit()
        {
            isHovered = false;
        }

        private void Update()
        {
            // Smooth hover animation
            Vector3 targetScale = isHovered ? originalScale * hoverScale : originalScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
        }

        /// <summary>
        /// Resets the card to default state.
        /// </summary>
        public void ResetCard()
        {
            currentOffer = null;
            onSelectedCallback = null;
            isHovered = false;
            transform.localScale = originalScale;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Gets the keyboard shortcut number for this card (1, 2, 3).
        /// </summary>
        public string GetShortcutKey()
        {
            return (cardIndex + 1).ToString();
        }
    }
}
