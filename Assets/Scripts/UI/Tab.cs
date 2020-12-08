using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace UI {
    /// <summary>
    /// A tab that holds a menu page.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class Tab : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler {
        #pragma warning disable 0649
        
        // Components
        private TabManager TabManager { get; set; }
        public Image Background { get; private set; }

        // Events.
        [SerializeField] private UnityEvent onSelected = new UnityEvent();
        /// <summary>
        /// Triggered when tab is selected.
        /// </summary>
        public UnityEvent OnSelected => onSelected;

        [SerializeField] private UnityEvent onDeselected = new UnityEvent();
        /// <summary>
        /// Triggered when tab is deselected.
        /// </summary>
        public UnityEvent OnDeselected => onDeselected;

        #pragma warning restore 0649
        
        /// <summary>
        /// Sets up the class.
        /// </summary>
        private void Awake() {
            Background = GetComponent<Image>();
            TabManager = transform.parent.GetComponent<TabManager>();
        }

        
        // Click Events.
        public void OnPointerEnter(PointerEventData eventData) {
            TabManager.OnTabEnter(this);
        }

        public void OnPointerClick(PointerEventData eventData) {
            TabManager.OnTabSelected(this);
        }

        public void OnPointerExit(PointerEventData eventData) {
            TabManager.OnTabExit(this);
        }
        
        // Events Triggers.

        /// <summary>
        /// Invokes the selected method in this class.
        /// </summary>
        public void OnSelect() => OnSelected?.Invoke();
        
        /// <summary>
        /// Invokes the deselected method in this class.
        /// </summary>
        public void OnDeselect() => OnDeselected?.Invoke();
    }
}