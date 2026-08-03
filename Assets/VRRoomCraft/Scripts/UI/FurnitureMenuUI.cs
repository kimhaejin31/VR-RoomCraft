using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRRoomCraft.Core.Interaction;
using VRRoomCraft.Data.Furniture;

namespace VRRoomCraft.UI
{
    /// <summary>
    /// UI Controller for the VR Floating Furniture Customization Panel.
    /// Dynamically populates category tabs and furniture item buttons using FurnitureDatabaseSO.
    /// Forwards item selection requests to InteractionManager without modifying scene objects directly.
    /// </summary>
    [DisallowMultipleComponent]
    public class FurnitureMenuUI : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Reference to the master UIManager.")]
        [SerializeField] private UIManager _uiManager;

        [Tooltip("Reference to the InteractionManager gateway.")]
        [SerializeField] private InteractionManager _interactionManager;

        [Tooltip("Master database catalog containing furniture categories and items.")]
        [SerializeField] private FurnitureDatabaseSO _database;

        [Header("UI Containers")]
        [Tooltip("Parent transform for dynamically generated category tab buttons.")]
        [SerializeField] private Transform _categoryContainer;

        [Tooltip("Parent transform for dynamically generated furniture thumbnail buttons.")]
        [SerializeField] private Transform _furnitureGridContainer;

        [Header("UI Prefabs")]
        [Tooltip("Prefab instantiated for each category tab button.")]
        [SerializeField] private GameObject _categoryButtonPrefab;

        [Tooltip("Prefab instantiated for each furniture item tile button.")]
        [SerializeField] private GameObject _furnitureButtonPrefab;

        [Header("Preview Details Panel")]
        [Tooltip("UI Image component displaying selected furniture thumbnail.")]
        [SerializeField] private Image _previewImage;

        [Tooltip("UI Text component displaying selected furniture display name.")]
        [SerializeField] private Text _furnitureNameText;

        [Tooltip("UI Text component displaying selected furniture description.")]
        [SerializeField] private Text _furnitureDescriptionText;

        // --- C# Events ---

        /// <summary>
        /// Raised when a furniture item tile is selected by the user.
        /// </summary>
        public event Action<FurnitureItemSO> OnFurnitureSelected;

        /// <summary>
        /// Raised when the active category tab changes.
        /// </summary>
        public event Action<FurnitureCategory> OnCategoryChanged;

        // --- Runtime State & Pools ---

        private FurnitureCategory _activeCategory = FurnitureCategory.None;
        private FurnitureItemSO _selectedItem;

        private readonly List<GameObject> _spawnedCategoryButtons = new List<GameObject>();
        private readonly List<GameObject> _spawnedFurnitureButtons = new List<GameObject>();

        // --- Public Properties ---

        public FurnitureCategory ActiveCategory => _activeCategory;
        public FurnitureItemSO SelectedItem => _selectedItem;

        // --- Unity Lifecycle ---

        private void Awake()
        {
            if (_uiManager == null) _uiManager = GetComponentInParent<UIManager>();
            if (_interactionManager == null && _uiManager != null) _interactionManager = _uiManager.InteractionManager;
            if (_interactionManager == null) _interactionManager = FindFirstObjectByType<InteractionManager>();
        }

        private void OnEnable()
        {
            if (_interactionManager != null)
            {
                _interactionManager.OnFurnitureSlotSelected += HandleSlotSelected;
            }

            RefreshMenu();
        }

        private void OnDisable()
        {
            if (_interactionManager != null)
            {
                _interactionManager.OnFurnitureSlotSelected -= HandleSlotSelected;
            }
        }

        // --- Public Control APIs ---

        /// <summary>
        /// Re-populates and refreshes all category tabs and item buttons from FurnitureDatabaseSO.
        /// </summary>
        public void RefreshMenu()
        {
            BuildCategoryTabs();

            // Default to first category if available
            if (_database != null && _database.Categories.Count > 0)
            {
                ShowCategory(_database.Categories[0].Category);
            }
        }

        /// <summary>
        /// Switches the visible furniture grid to display items belonging to the specified FurnitureCategory.
        /// </summary>
        /// <param name="category">FurnitureCategory enum to display.</param>
        public void ShowCategory(FurnitureCategory category)
        {
            _activeCategory = category;
            OnCategoryChanged?.Invoke(_activeCategory);

            BuildFurnitureGrid(_activeCategory);
        }

        /// <summary>
        /// Displays information for the currently active furniture item in the preview details panel.
        /// </summary>
        public void ShowCurrentSelection()
        {
            if (_selectedItem != null)
            {
                if (_previewImage != null)
                {
                    _previewImage.sprite = _selectedItem.Icon;
                    _previewImage.enabled = _selectedItem.Icon != null;
                }

                if (_furnitureNameText != null)
                {
                    _furnitureNameText.text = _selectedItem.DisplayName;
                }

                if (_furnitureDescriptionText != null)
                {
                    _furnitureDescriptionText.text = _selectedItem.Description;
                }
            }
            else
            {
                ClearSelection();
            }
        }

        /// <summary>
        /// Clears the preview details panel.
        /// </summary>
        public void ClearSelection()
        {
            _selectedItem = null;

            if (_previewImage != null)
            {
                _previewImage.sprite = null;
                _previewImage.enabled = false;
            }

            if (_furnitureNameText != null)
            {
                _furnitureNameText.text = "Select an Item";
            }

            if (_furnitureDescriptionText != null)
            {
                _furnitureDescriptionText.text = string.Empty;
            }
        }

        // --- Internal UI Generation & Handling ---

        private void HandleSlotSelected(World.FurnitureSlot slot)
        {
            if (slot != null)
            {
                ShowCategory(slot.Category);
                if (slot.CurrentItem != null)
                {
                    SelectItem(slot.CurrentItem);
                }
            }
        }

        private void BuildCategoryTabs()
        {
            ClearCategoryButtons();

            if (_database == null || _categoryContainer == null || _categoryButtonPrefab == null) return;

            IReadOnlyList<FurnitureCategorySO> categories = _database.Categories;
            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i] == null) continue;

                FurnitureCategorySO catSO = categories[i];
                GameObject btnObj = Instantiate(_categoryButtonPrefab, _categoryContainer);
                _spawnedCategoryButtons.Add(btnObj);

                // Configure button label
                Text label = btnObj.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = catSO.CategoryName;
                }

                // Configure button image icon
                Image iconImage = btnObj.GetComponentInChildren<Image>();
                if (iconImage != null && catSO.CategoryIcon != null)
                {
                    iconImage.sprite = catSO.CategoryIcon;
                }

                // Bind UnityEvent callback
                Button button = btnObj.GetComponent<Button>();
                if (button != null)
                {
                    FurnitureCategory catType = catSO.Category;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => ShowCategory(catType));
                }
            }
        }

        private void BuildFurnitureGrid(FurnitureCategory category)
        {
            ClearFurnitureButtons();

            if (_database == null || _furnitureGridContainer == null || _furnitureButtonPrefab == null) return;

            FurnitureCategorySO categorySO = _database.GetCategory(category);
            if (categorySO == null) return;

            IReadOnlyList<FurnitureItemSO> items = categorySO.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null) continue;

                FurnitureItemSO item = items[i];
                GameObject btnObj = Instantiate(_furnitureButtonPrefab, _furnitureGridContainer);
                _spawnedFurnitureButtons.Add(btnObj);

                // Configure button label
                Text label = btnObj.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = item.DisplayName;
                }

                // Configure thumbnail icon
                Image iconImage = btnObj.GetComponentInChildren<Image>();
                if (iconImage != null && item.Icon != null)
                {
                    iconImage.sprite = item.Icon;
                }

                // Bind click event
                Button button = btnObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => SelectItem(item));
                }
            }
        }

        private void SelectItem(FurnitureItemSO item)
        {
            if (item == null) return;

            _selectedItem = item;
            ShowCurrentSelection();
            OnFurnitureSelected?.Invoke(_selectedItem);

            // Forward selection to InteractionManager
            if (_interactionManager != null)
            {
                _interactionManager.ApplyFurniture(_selectedItem);
            }
        }

        private void ClearCategoryButtons()
        {
            for (int i = 0; i < _spawnedCategoryButtons.Count; i++)
            {
                if (_spawnedCategoryButtons[i] != null)
                {
                    Destroy(_spawnedCategoryButtons[i]);
                }
            }
            _spawnedCategoryButtons.Clear();
        }

        private void ClearFurnitureButtons()
        {
            for (int i = 0; i < _spawnedFurnitureButtons.Count; i++)
            {
                if (_spawnedFurnitureButtons[i] != null)
                {
                    Destroy(_spawnedFurnitureButtons[i]);
                }
            }
            _spawnedFurnitureButtons.Clear();
        }
    }
}
