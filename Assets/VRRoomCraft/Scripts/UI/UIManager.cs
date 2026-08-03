using System;
using UnityEngine;
using VRRoomCraft.Core.Interaction;

namespace VRRoomCraft.UI
{
    /// <summary>
    /// Master coordinator for VR floating menu panels (Furniture Menu, Material Menu, Settings Menu).
    /// Handles panel visibility, tab switching, and panel transitions without containing business logic.
    /// Completely decoupled from static singletons, scene searches, and XR input code.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Reference to the InteractionManager gateway.")]
        [SerializeField] private InteractionManager _interactionManager;

        [Header("UI Panel GameObjects")]
        [Tooltip("Root GameObject for the Furniture Customization Panel.")]
        [SerializeField] private GameObject _furniturePanel;

        [Tooltip("Root GameObject for the Material Customization Panel.")]
        [SerializeField] private GameObject _materialPanel;

        [Tooltip("Root GameObject for the Settings & Room Reset Panel.")]
        [SerializeField] private GameObject _settingsPanel;

        [Header("Runtime State (Read-Only)")]
        [SerializeField] private GameObject _currentActivePanel;

        // --- C# Events ---

        /// <summary>
        /// Raised when a UI panel opens.
        /// </summary>
        public event Action<GameObject> OnMenuOpened;

        /// <summary>
        /// Raised when a UI panel closes.
        /// </summary>
        public event Action<GameObject> OnMenuClosed;

        /// <summary>
        /// Raised when the active menu panel changes.
        /// Signature: (GameObject newActivePanel)
        /// </summary>
        public event Action<GameObject> OnCurrentMenuChanged;

        // --- Public Read-Only Properties ---

        /// <summary>
        /// Gets the bound InteractionManager reference.
        /// </summary>
        public InteractionManager InteractionManager => _interactionManager;

        /// <summary>
        /// Gets the currently active visible UI panel GameObject.
        /// </summary>
        public GameObject CurrentActivePanel => _currentActivePanel;

        /// <summary>
        /// Gets whether any menu panel is currently visible.
        /// </summary>
        public bool IsAnyMenuOpen => _currentActivePanel != null && _currentActivePanel.activeSelf;

        // --- Unity Lifecycle ---

        private void Awake()
        {
            if (_interactionManager == null)
            {
                _interactionManager = GetComponent<InteractionManager>();
            }

            if (_interactionManager == null)
            {
                _interactionManager = FindFirstObjectByType<InteractionManager>();
            }

            if (_interactionManager == null)
            {
                Debug.LogError("[UIManager] Missing required InteractionManager dependency!");
            }
        }

        private void Start()
        {
            // Start with all panels closed by default
            HideAllMenus();
        }

        // --- Public Panel Navigation APIs ---

        /// <summary>
        /// Opens the Furniture Customization menu panel and closes other panels.
        /// </summary>
        public void OpenFurnitureMenu()
        {
            ShowPanel(_furniturePanel);
        }

        /// <summary>
        /// Opens the Surface Material Customization menu panel and closes other panels.
        /// </summary>
        public void OpenMaterialMenu()
        {
            ShowPanel(_materialPanel);
        }

        /// <summary>
        /// Opens the Settings & Room Reset menu panel and closes other panels.
        /// </summary>
        public void OpenSettings()
        {
            ShowPanel(_settingsPanel);
        }

        /// <summary>
        /// Toggles visibility of the Furniture Menu panel.
        /// </summary>
        public void ToggleFurnitureMenu()
        {
            if (_currentActivePanel == _furniturePanel && _furniturePanel.activeSelf)
            {
                HideAllMenus();
            }
            else
            {
                OpenFurnitureMenu();
            }
        }

        /// <summary>
        /// Toggles visibility of the Material Menu panel.
        /// </summary>
        public void ToggleMaterialMenu()
        {
            if (_currentActivePanel == _materialPanel && _materialPanel.activeSelf)
            {
                HideAllMenus();
            }
            else
            {
                OpenMaterialMenu();
            }
        }

        /// <summary>
        /// Closes whichever menu panel is currently active.
        /// </summary>
        public void CloseCurrentMenu()
        {
            HideAllMenus();
        }

        /// <summary>
        /// Hides all UI menu panels.
        /// </summary>
        public void HideAllMenus()
        {
            if (_currentActivePanel != null)
            {
                GameObject previousPanel = _currentActivePanel;
                _currentActivePanel = null;

                if (_furniturePanel != null) _furniturePanel.SetActive(false);
                if (_materialPanel != null) _materialPanel.SetActive(false);
                if (_settingsPanel != null) _settingsPanel.SetActive(false);

                OnMenuClosed?.Invoke(previousPanel);
                OnCurrentMenuChanged?.Invoke(null);
            }
            else
            {
                if (_furniturePanel != null) _furniturePanel.SetActive(false);
                if (_materialPanel != null) _materialPanel.SetActive(false);
                if (_settingsPanel != null) _settingsPanel.SetActive(false);
            }
        }

        // --- Internal Panel Transition Logic ---

        private void ShowPanel(GameObject targetPanel)
        {
            if (targetPanel == null)
            {
                Debug.LogWarning("[UIManager] Cannot show null panel reference.");
                return;
            }

            if (_currentActivePanel == targetPanel && targetPanel.activeSelf)
            {
                return; // Already open
            }

            GameObject previousPanel = _currentActivePanel;

            // Deactivate all panels
            if (_furniturePanel != null) _furniturePanel.SetActive(false);
            if (_materialPanel != null) _materialPanel.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);

            if (previousPanel != null)
            {
                OnMenuClosed?.Invoke(previousPanel);
            }

            // Activate target panel
            _currentActivePanel = targetPanel;
            _currentActivePanel.SetActive(true);

            OnMenuOpened?.Invoke(_currentActivePanel);
            OnCurrentMenuChanged?.Invoke(_currentActivePanel);
        }
    }
}
