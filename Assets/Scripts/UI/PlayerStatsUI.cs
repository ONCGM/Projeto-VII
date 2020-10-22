using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Entity.Player;
using Game;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    /// <summary>
    /// Controls the UI that displays player health and level.
    /// </summary>
    public class PlayerStatsUI : MonoBehaviour {
    #pragma warning disable 0649
        [Header("Settings")] 
        [SerializeField, Range(0.01f, 3f)] private float fadeAnimationSpeed = 0.7f;
        [SerializeField, Range(0.01f, 3f)] private float barAnimationDelay = 0.3f;
        [SerializeField, Range(0.01f, 30f)] private float barAnimationSpeed = 15f;
        [SerializeField] private float uiBarsWidth = 450;

        [Header("UI Components")] 
        [SerializeField] private Image timeNeedlePointer;
        [SerializeField] private TMP_Text healthText,
                                          staminaText,
                                          levelText,
                                          coinsText,
                                          dayText,
                                          timeOfDayText;
        [SerializeField] private Image healthBar, staminaBar, levelBar;
        [SerializeField] private Image healthBackBar, staminaBackBar;
        
        // Components.
        private PlayerController player;
        private CanvasGroup childCanvasGroup;
        private CanvasGroup parentCanvasGroup;
        
        // Events.
        /// <summary>
        /// Updates the Ui values being displayed. So it updates on demand.
        /// </summary>
        public static Action UpdateUiValues;
        
        // Constant values and variables.
        private static readonly List<Vector3> ClockRotations = new List<Vector3> {
            Vector3.zero, new Vector3(0f, 0f, 120f), new Vector3(0f, 0f, 240f)
        };
        private static readonly List<string> TimeOfDayLocalizationKeys = new List<string> {
            "CONTEXT_TIME_NIGHT","CONTEXT_TIME_MORNING","CONTEXT_TIME_AFTERNOON"
        };
        private const string upgradeSettingsPath = "Scriptables/Player/Player_Upgrade_Settings";
        private PlayerUpgradeSettings upgradeSettings;
        private WaitForSeconds waitForBarAnimationDelay;
        private WaitForFixedUpdate waitForBarAnimationFrame;
        private static readonly int HitEffectBlend = Shader.PropertyToID("_HitEffectBlend");

        #pragma warning restore 0649
        
        // Set up and events.
        private void Awake() {
            player = FindObjectOfType<PlayerController>();
            childCanvasGroup = GetComponentInChildren<CanvasGroup>();
            parentCanvasGroup = GetComponent<CanvasGroup>();
            upgradeSettings = Resources.Load<PlayerUpgradeSettings>(upgradeSettingsPath);
            waitForBarAnimationDelay = new WaitForSeconds(barAnimationDelay);
            waitForBarAnimationFrame = new WaitForFixedUpdate();
            GameMaster.OnGameMenuUpdated += FadeAnimation;
            GameMaster.OnGameTimeOfDayUpdated += UpdateUiValues;
            GameMaster.OnGameDayUpdate += UpdateUiValues;
            GameMaster.OnPlayerStatsUpdated += UpdateUiValues;
            UpdateUiValues += UpdateUi;
            UpdateUiValues?.Invoke();
        }

        private void OnDestroy() {
            GameMaster.OnGameMenuUpdated -= FadeAnimation;
            GameMaster.OnGameTimeOfDayUpdated -= UpdateUiValues;
            GameMaster.OnPlayerStatsUpdated -= UpdateUiValues;
            GameMaster.OnGameDayUpdate -= UpdateUiValues;
            UpdateUiValues = null;
        }

        /// <summary>
        /// Animates UI fading in and out.
        /// </summary>
        private void FadeAnimation() {
            DOTween.To(()=> childCanvasGroup.alpha, x=> childCanvasGroup.alpha = x, (GameMaster.Instance.GameMenuIsOpen ? 0f : 1f), fadeAnimationSpeed);
        }

        /// <summary>
        /// Toggles the visibility of the entire player canvas.
        /// </summary>
        /// <param name="state"> Set true to enable the canvas. </param>
        public void ShowHideCanvas(bool state) {
            DOTween.To(()=> parentCanvasGroup.alpha, x=> parentCanvasGroup.alpha = x, (state ? 1f : 0f), fadeAnimationSpeed);
        }

        /// <summary>
        /// Updates all UI values.
        /// </summary>
        private void UpdateUi() {
            timeNeedlePointer.transform.rotation = 
                Quaternion.Euler(ClockRotations[(int) GameMaster.Instance.CurrentTimeOfDay]);
            
            healthBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 
                Mathf.Lerp(0f, uiBarsWidth,Mathf.InverseLerp(0f, player.MaxHealth, player.Health)));
            
            staminaBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 
                Mathf.Lerp(0f, uiBarsWidth,Mathf.InverseLerp(0f, player.MaxStamina, player.Stamina)));
            
            levelBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 
                Mathf.Lerp(0f, uiBarsWidth,Mathf.InverseLerp(0f, upgradeSettings.GetExperienceNeededForLevelUp(player.Level), player.Experience)));

            healthText.text = $"{player.Health} / {player.MaxHealth}";
            staminaText.text = $"{player.Stamina} / {player.MaxStamina}";
            levelText.text = (player.Level >= player.MaxLevel ? LocalizationSystem.GetLocalizedValue("CONTEXT_GAMBLING_MAX") : player.Level.ToString());
            coinsText.text = $"{LocalizationSystem.GetLocalizedValue("CONTEXT_METAL_COIN")} {player.Coins}";
            var dayLocalized = LocalizationSystem.GetLocalizedValue("CONTEXT_TIME_DAY");
            var currentDay = GameMaster.Instance.CurrentGameDay;
            dayText.text = LocalizationSystem.CurrentLanguage == LocalizationSystem.Language.Japanese ? $"{currentDay} {dayLocalized}" : $"{dayLocalized} {currentDay}";
            timeOfDayText.text = LocalizationSystem.GetLocalizedValue(TimeOfDayLocalizationKeys[(int) GameMaster.Instance.CurrentTimeOfDay]);

            staminaBackBar.material.SetFloat(HitEffectBlend, 1f);
            StartCoroutine(nameof(AnimateHealthBackBar));
            StartCoroutine(nameof(AnimateStaminaBackBar));
        }

        /// <summary>
        /// Animates the health back bar reducing over time after a delay.
        /// </summary>
        private IEnumerator AnimateHealthBackBar() {
            yield return waitForBarAnimationDelay;

            if(healthBar.rectTransform.sizeDelta.x > healthBackBar.rectTransform.sizeDelta.x) {
                healthBackBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                         Mathf.Lerp(0f, uiBarsWidth,Mathf.InverseLerp(0f,player.MaxHealth,player.Health)));
                yield break;
            }

            var width = healthBackBar.rectTransform.sizeDelta.x;
            
            while(healthBackBar.rectTransform.sizeDelta.x > healthBar.rectTransform.sizeDelta.x) {
                width -= barAnimationSpeed;
                healthBackBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                yield return waitForBarAnimationFrame;
            }
        }
        
        /// <summary>
        /// Animates the stamina back bar reducing over time after a delay.
        /// </summary>
        private IEnumerator AnimateStaminaBackBar() {
            yield return waitForBarAnimationDelay;
            var staminaBarMaterial = staminaBackBar.material;
            staminaBarMaterial.SetFloat(HitEffectBlend, 1f);
            
            if(staminaBar.rectTransform.sizeDelta.x > staminaBackBar.rectTransform.sizeDelta.x) {
                staminaBackBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                                                                       Mathf.Lerp(0f, uiBarsWidth,Mathf.InverseLerp(0f,player.MaxStamina,player.Stamina)));
                yield break;
            }

            var width = staminaBackBar.rectTransform.sizeDelta.x;
            DOTween.To(() => staminaBarMaterial.GetFloat(HitEffectBlend), 
                       x => staminaBarMaterial.SetFloat(HitEffectBlend, x), 0f, 0.15f);

            while(staminaBackBar.rectTransform.sizeDelta.x > staminaBar.rectTransform.sizeDelta.x) {
                width -= barAnimationSpeed;
                staminaBackBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                yield return waitForBarAnimationFrame;
            }
            
            
            staminaBarMaterial.SetFloat(HitEffectBlend, 1f);
        }
    }
}