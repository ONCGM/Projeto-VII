using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Entity.Enemies;

namespace UI {
    /// <summary>
    /// Controls the enemy health bar UI.
    /// </summary>
    public class EnemyHealthBarUI : MonoBehaviour {
        #pragma warning disable 0649
        [Header("Settings")] 
        [SerializeField, Range(0.01f, 3f)] private float fadeAnimationSpeed = 0.7f;
        [SerializeField, Range(0.01f, 3f)] private float barAnimationDelay = 0.3f;
        [SerializeField, Range(0.01f, 30f)] private float barAnimationSpeed = 15f;
        [SerializeField] private float uiBarsWidth = 450;
        
        [Header("UI Components")]
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        [SerializeField] private Image healthBar, healthBackBar;
        private TMP_Text healthText;
        
        // Wait for's'.
        private WaitForSeconds waitForBarAnimationDelay;
        private WaitForFixedUpdate waitForBarAnimationFrame;
        
        // Enemy.
        private Enemy enemy;

        #pragma warning restore 0649
        
        // Unity Events.
        private void Awake() {
            canvas = GetComponent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
            healthText = GetComponentInChildren<TMP_Text>();
            waitForBarAnimationDelay = new WaitForSeconds(barAnimationDelay);
            waitForBarAnimationFrame = new WaitForFixedUpdate();
            enemy = GetComponentInParent<Enemy>();
        }

        private void Start() {
            UpdateUI();
            canvas.worldCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        }

        private void LateUpdate() {
            transform.rotation = Quaternion.Euler(60f, 0f, 0f);
        }

        /// <summary>
        /// Animates UI fading in and out.
        /// </summary>
        public void FadeOutAnimation() {
            DOTween.To(()=> canvasGroup.alpha, x=> canvasGroup.alpha = x, 0f, fadeAnimationSpeed).onComplete += () => Destroy(gameObject);
        }
        
        /// <summary>
        /// Updates all UI values.
        /// </summary>
        public void UpdateUI() {
            healthBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 
                                                              Mathf.Lerp(0f, uiBarsWidth,Mathf.InverseLerp(0f, enemy.MaxHealth, enemy.Health)));
            
            healthText.text = $"{Mathf.Clamp(enemy.Health, 0, enemy.MaxHealth)} / {enemy.MaxHealth}";
            StartCoroutine(nameof(AnimateHealthBackBar));
        }
        
        /// <summary>
        /// Animates the health back bar reducing over time after a delay.
        /// </summary>
        private IEnumerator AnimateHealthBackBar() {
            yield return waitForBarAnimationDelay;

            if(healthBar.rectTransform.sizeDelta.x > healthBackBar.rectTransform.sizeDelta.x) {
                healthBackBar.rectTransform.
                              SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 
                              Mathf.Lerp(0f, uiBarsWidth,Mathf.InverseLerp(0f,enemy.MaxHealth,enemy.Health)));
                yield break;
            }

            var width = healthBackBar.rectTransform.sizeDelta.x;
            
            while(healthBackBar.rectTransform.sizeDelta.x > healthBar.rectTransform.sizeDelta.x) {
                width -= barAnimationSpeed;
                healthBackBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                yield return waitForBarAnimationFrame;
            }
        }
    }
}