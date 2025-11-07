using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace RoombaRampage.UI
{
    /// <summary>
    /// Main skill selection UI controller.
    /// Manages the selection panel, skill cards, and keyboard input.
    /// </summary>
    public class SkillSelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private SkillCard[] skillCards;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;

        [Header("Audio (Future)")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip hoverSound;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private UnityAction<int> currentCallback;
        private bool isVisible = false;
        private float fadeTimer = 0f;
        private bool isFading = false;

        private void Awake()
        {
            // Ensure panel starts hidden
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }

            // Initialize canvas group
            if (canvasGroup == null)
            {
                canvasGroup = selectionPanel?.GetComponent<CanvasGroup>();
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        private void Update()
        {
            if (!isVisible)
                return;

            // Handle keyboard input (1, 2, 3)
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                SelectSkill(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                SelectSkill(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                SelectSkill(2);
            }

            // Handle fade animation
            if (isFading)
            {
                UpdateFade();
            }
        }

        /// <summary>
        /// Shows the skill selection UI with given offers.
        /// </summary>
        public void ShowSelection(List<Skills.SkillOffer> offers, UnityAction<int> callback)
        {
            if (offers == null || offers.Count == 0)
            {
                Debug.LogWarning("[SkillSelectionUI] No offers to display!");
                return;
            }

            currentCallback = callback;

            // Setup skill cards
            for (int i = 0; i < skillCards.Length; i++)
            {
                if (i < offers.Count)
                {
                    // Setup card with offer
                    skillCards[i].Setup(offers[i], i, SelectSkill);
                    skillCards[i].gameObject.SetActive(true);

                    if (showDebugLogs)
                        Debug.Log($"[SkillSelectionUI] Card {i}: {offers[i].skillData.skillName}");
                }
                else
                {
                    // Hide extra cards
                    skillCards[i].Reset();
                    skillCards[i].gameObject.SetActive(false);
                }
            }

            // Update title
            if (titleText != null)
            {
                titleText.text = "LEVEL UP!";
            }

            // Update instructions
            if (instructionsText != null)
            {
                instructionsText.text = "Press 1, 2, or 3 to select a skill";
            }

            // Show panel
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
            }

            // Start fade in
            isVisible = true;
            StartFade(true);

            // Ensure time scale is 0 (game paused)
            Time.timeScale = 0f;

            if (showDebugLogs)
                Debug.Log($"[SkillSelectionUI] Showing selection with {offers.Count} offers");
        }

        /// <summary>
        /// Hides the skill selection UI.
        /// </summary>
        public void HideSelection()
        {
            isVisible = false;

            // Start fade out
            StartFade(false);

            if (showDebugLogs)
                Debug.Log("[SkillSelectionUI] Hiding selection");
        }

        /// <summary>
        /// Selects a skill by index.
        /// </summary>
        private void SelectSkill(int index)
        {
            if (!isVisible)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[SkillSelectionUI] Tried to select skill while UI not visible");
                return;
            }

            if (index < 0 || index >= skillCards.Length)
            {
                if (showDebugLogs)
                    Debug.LogWarning($"[SkillSelectionUI] Invalid skill index: {index}");
                return;
            }

            if (!skillCards[index].gameObject.activeSelf)
            {
                if (showDebugLogs)
                    Debug.LogWarning($"[SkillSelectionUI] Card {index} is not active");
                return;
            }

            if (showDebugLogs)
                Debug.Log($"[SkillSelectionUI] Selected skill at index {index}");

            // Play select sound (future)
            // if (selectSound != null) AudioSource.PlayClipAtPoint(selectSound, Vector3.zero);

            // Invoke callback
            currentCallback?.Invoke(index);
        }

        /// <summary>
        /// Starts fade animation.
        /// </summary>
        private void StartFade(bool fadeIn)
        {
            isFading = true;
            fadeTimer = 0f;

            if (canvasGroup == null)
                return;

            if (fadeIn)
            {
                canvasGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// Updates fade animation.
        /// </summary>
        private void UpdateFade()
        {
            if (canvasGroup == null)
            {
                isFading = false;
                return;
            }

            fadeTimer += Time.unscaledDeltaTime;

            if (isVisible)
            {
                // Fade in
                float t = Mathf.Clamp01(fadeTimer / fadeInDuration);
                canvasGroup.alpha = t;

                if (t >= 1f)
                {
                    isFading = false;
                }
            }
            else
            {
                // Fade out
                float t = Mathf.Clamp01(fadeTimer / fadeOutDuration);
                canvasGroup.alpha = 1f - t;

                if (t >= 1f)
                {
                    isFading = false;

                    // Hide panel
                    if (selectionPanel != null)
                    {
                        selectionPanel.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Forces immediate hide without animation.
        /// </summary>
        public void ForceHide()
        {
            isVisible = false;
            isFading = false;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }
        }
    }
}
