using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    /// <summary>
    /// Controls a group of tabs.
    /// </summary>
    public class TabManager : MonoBehaviour {
        #pragma warning disable 0649
        /// <summary>
        /// List of all tabs subscribed to this manager.
        /// </summary>
        private List<Tab> Tabs { get; set; } = new List<Tab>();

        private Tab activeTab;

        // Sprite States.
        [Header("Sprites")]
        [SerializeField] private Sprite idle, hover, active;
        
        // Toggleable objects.
        /// <summary>
        /// Objects that will be toggled by the manager when changing tabs.
        /// </summary>
        [Header("Objects to toggle. Must be on same order as tabs.")]
        public List<GameObject> tabObjectsToToggle;
        
        #pragma warning restore 0649

        /// <summary>
        /// Selects the first tab.
        /// </summary>
        private void Start() {
            for(var i = 0; i < transform.childCount; i++) {
                SubscribeToManager(transform.GetChild(i)?.GetComponent<Tab>());
            }
            
            OnTabSelected(Tabs[0]);
        }
        
        /// <summary>
        /// Subscribe a tab to this manager.
        /// </summary>
        /// <param name="tab"> Tab to subscribe.</param>
        public void SubscribeToManager(Tab tab) {
            if(Tabs == null) Tabs = new List<Tab>();
            
            Tabs.Add(tab);
        }
        
        // Event methods.
        /// <summary>
        /// Called by the tabs so that the manager can change the current active one.
        /// </summary>
        /// <param name="tab"> Tab to change.</param>
        public void OnTabEnter(Tab tab) {
            ResetTabs();
            
            if(activeTab == null || tab != activeTab) {
                tab.Background.sprite = hover;
            }
        }
        
        /// <summary>
        /// Called by the tabs so that the manager can change the current active one.
        /// </summary>
        /// <param name="tab"> Tab to change.</param>
        public void OnTabExit(Tab tab) {
            ResetTabs();
        }

        /// <summary>
        /// Called by the tabs so that the manager can change the current active one.
        /// </summary>
        /// <param name="tab"> Tab to change.</param>
        public void OnTabSelected(Tab tab) {
            if(activeTab != null) activeTab.OnDeselect();

            activeTab = tab;

            activeTab.OnSelect();
            
            ResetTabs();
            tab.Background.sprite = active;
            
            var index = tab.transform.GetSiblingIndex();
            
            for(var i = 0; i < tabObjectsToToggle.Count; i++) {
                if(i == index) {
                    tabObjectsToToggle[i].SetActive(true);    
                    continue;
                }
                
                tabObjectsToToggle[i].SetActive(false);
            }
        }

        /// <summary>
        /// Reset all tabs to the idle sprite.
        /// </summary>
        private void ResetTabs() {
            foreach(var tab in Tabs) {
                if(activeTab != null && tab == activeTab) continue;
                tab.Background.sprite = idle;
            }
        }
    }
}