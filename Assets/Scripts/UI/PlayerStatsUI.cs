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
        [SerializeField] private CanvasGroup infoBarCanvasGroup;
        [SerializeField] private CanvasGroup parentCanvasGroup,
                                             childCanvasGroup,
                                             minimapCanvasGroup;
        
        // Components.
        private PlayerController player;

        // Events.
        /// <summary>
        /// Updates the Ui values being displayed. So it updates on demand.
        /// </summary>
        public static Action UpdateUiValues;
        
        // Constant values and variables.
        private static readonly List<Vector3> ClockRotations = new List<Vector3> {
            new Vector3(0f, 0f, 120f), new Vector3(0f, 0f, 240f), Vector3.zero
        };
        private static readonly List<string> TimeOfDayLocalizationKeys = new List<string> {
            "CONTEXT_TIME_MORNING","CONTEXT_TIME_AFTERNOON", "CONTEXT_TIME_NIGHT"
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
            parentCanvasGroup = GetComponent<CanvasGroup>();
            upgradeSettings = Resources.Load<PlayerUpgradeSettings>(upgradeSettingsPath);
            waitForBarAnimationDelay = new WaitForSeconds(barAnimationDelay);
            waitForBarAnimationFrame = new WaitForFixedUpdate();
            GameMaster.OnGameMenuUpdated += FadeAnimation;
            GameMaster.OnTimeOfDayUpdated += UpdateUiValues;
            GameMaster.OnGameDayUpdate += UpdateUiValues;
            GameMaster.OnPlayerStatsUpdated += UpdateUiValues;
            UpdateUiValues += UpdateUi;
            UpdateUiValues?.Invoke();
            
            InvokeRepeating(nameof(UpdateUi), 1f, 1f);
        }

        private void OnDestroy() {
            GameMaster.OnGameMenuUpdated -= FadeAnimation;
            GameMaster.OnTimeOfDayUpdated -= UpdateUiValues;
            GameMaster.OnPlayerStatsUpdated -= UpdateUiValues;
            GameMaster.OnGameDayUpdate -= UpdateUiValues;
            StopAllCoroutines();
            CancelInvoke(nameof(UpdateUi));
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
            DOTween.To(()=> infoBarCanvasGroup.alpha, x=> infoBarCanvasGroup.alpha = x, (state ? 1f : 0f), fadeAnimationSpeed);
            DOTween.To(()=> childCanvasGroup.alpha, x=> childCanvasGroup.alpha = x, (state ? 1f : 0f), fadeAnimationSpeed);
            DOTween.To(()=> minimapCanvasGroup.alpha, x=> minimapCanvasGroup.alpha = x, (state ? 1f : 0f), fadeAnimationSpeed);
        }

        /// <summary>
        /// Toggles the visibility of the entire player canvas
        /// except for the clock.
        /// </summary>
        /// <param name="state"> Set true to enable the canvas. </param>
        public void ShowHideCanvasKeepClock(bool state) {
            DOTween.To(()=> parentCanvasGroup.alpha, x=> parentCanvasGroup.alpha = x, 1f, fadeAnimationSpeed);
            DOTween.To(()=> infoBarCanvasGroup.alpha, x=> infoBarCanvasGroup.alpha = x, (state ? 1f : 0f), fadeAnimationSpeed);
            DOTween.To(()=> childCanvasGroup.alpha, x=> childCanvasGroup.alpha = x, (state ? 1f : 0f), fadeAnimationSpeed);
            DOTween.To(()=> minimapCanvasGroup.alpha, x=> minimapCanvasGroup.alpha = x, (state ? 1f : 0f), fadeAnimationSpeed);
        }

        /// <summary>
        /// Updates all UI values.
        /// </summary>
        private void UpdateUi() {
            if(timeNeedlePointer != null && timeNeedlePointer.transform.rotation !=
               Quaternion.Euler(ClockRotations[(int) GameMaster.Instance.CurrentTimeOfDay])) {
                timeNeedlePointer.transform.DORotate(Quaternion
                                                     .Euler(ClockRotations[(int) GameMaster.Instance.CurrentTimeOfDay])
                                                     .eulerAngles,
                                                     fadeAnimationSpeed * 2f);
            }

            var dayLocalized = LocalizationSystem.GetLocalizedValue("CONTEXT_TIME_DAY");
            var currentDay = GameMaster.Instance.CurrentGameDay;
            if(dayText != null) dayText.text = LocalizationSystem.CurrentLanguage == LocalizationSystem.Language.Japanese ? $"{currentDay} {dayLocalized}" : $"{dayLocalized} {currentDay}";
            if(timeOfDayText != null) timeOfDayText.text = LocalizationSystem.GetLocalizedValue(TimeOfDayLocalizationKeys[(int) GameMaster.Instance.CurrentTimeOfDay]);
            
            if(player == null) {
                player = FindObjectOfType<PlayerController>();
                return;
            }
            
            if(healthBar != null) healthBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 
                Mathf.Lerp(0f, uiBarsWidth,Mathf.InverseLerp(0f, player.MaxHealth, player.Health)));
            
            if(staminaBar != null) staminaBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 
                Mathf.Lerp(0f, uiBarsWidth,Mathf.InverseLerp(0f, player.MaxStamina, player.Stamina)));
            
            if(levelBar != null) levelBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 
                Mathf.Lerp(0f, uiBarsWidth,Mathf.InverseLerp(0f, upgradeSettings.GetExperienceNeededForLevelUp(player.Level), player.Experience)));

            if(healthText != null) healthText.text = $"{player.Health} / {player.MaxHealth}";
            if(staminaText != null) staminaText.text = $"{player.Stamina} / {player.MaxStamina}";
            if(levelText != null) levelText.text = (player.Level >= player.MaxLevel ? LocalizationSystem.GetLocalizedValue("CONTEXT_GAMBLING_MAX") : player.Level.ToString());
            
            DOTween.To(x => coinsText.text = $"{Mathf.RoundToInt(x).ToString()}", int.Parse(coinsText.text), player.Coins, fadeAnimationSpeed * 0.5f);

            if(staminaBackBar != null) staminaBackBar.material.SetFloat(HitEffectBlend, 1f);
            
            
            if(this != null) StartCoroutine(nameof(AnimateHealthBackBar));
            if(this != null) StartCoroutine(nameof(AnimateStaminaBackBar));
        }

        /// <summary>
        /// Animates the health back bar reducing over time after a delay.
        /// </summary>
        private IEnumerator AnimateHealthBackBar() {
            yield return waitForBarAnimationDelay;

            if(healthBar != null && healthBackBar != null) {
                if(healthBar.rectTransform.sizeDelta.x > healthBackBar.rectTransform.sizeDelta.x) {
                    healthBackBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                                                                          Mathf.Lerp(0f, uiBarsWidth,
                                                                              Mathf.InverseLerp(0f,
                                                                                  player.MaxHealth,
                                                                                  player.Health)));
                    yield break;
                }
            }

            var width = healthBackBar.rectTransform.sizeDelta.x;

            if(healthBar == null || healthBackBar == null) yield break;
            while(healthBackBar.rectTransform.sizeDelta.x > healthBar.rectTransform.sizeDelta.x) {
                width -= barAnimationSpeed;
                if(healthBackBar == null) yield break;
                healthBackBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                yield return waitForBarAnimationFrame;
            }
        }

        /// <summary>
        /// Animates the stamina back bar reducing over time after a delay.
        /// </summary>
        private IEnumerator AnimateStaminaBackBar() {
            yield return waitForBarAnimationDelay;
            if(staminaBackBar == null) yield break;
            
            var staminaBarMaterial = staminaBackBar.material;
            staminaBarMaterial.SetFloat(HitEffectBlend, 1f);

            if(staminaBar != null && staminaBackBar != null) {
                if(staminaBar.rectTransform.sizeDelta.x > staminaBackBar.rectTransform.sizeDelta.x) {
                    staminaBackBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                                                                           Mathf.Lerp(0f, uiBarsWidth,
                                                                               Mathf.InverseLerp(0f,
                                                                                   player.MaxStamina,
                                                                                   player.Stamina)));
                    yield break;
                }
            }

            var width = staminaBackBar.rectTransform.sizeDelta.x;
            DOTween.To(() => staminaBarMaterial.GetFloat(HitEffectBlend), 
                       x => staminaBarMaterial.SetFloat(HitEffectBlend, x), 0f, 0.15f);

            while(staminaBackBar.rectTransform.sizeDelta.x > staminaBar.rectTransform.sizeDelta.x) {
                width -= barAnimationSpeed;
                if(staminaBackBar == null) yield break;
                staminaBackBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                yield return waitForBarAnimationFrame;
            }
            
            
            staminaBarMaterial.SetFloat(HitEffectBlend, 1f);
        }
    }
}