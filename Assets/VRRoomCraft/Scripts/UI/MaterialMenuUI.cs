using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRRoomCraft.Core.Interaction;
using VRRoomCraft.Data.Materials;

namespace VRRoomCraft.UI
{
    /// <summary>
    /// UI Controller for the VR Floating Surface Material Customization Panel.
    /// Dynamically populates surface category tabs (Walls, Floors, Ceilings, Cabinets) and material swatches using MaterialDatabaseSO.
    /// Forwards material selection requests to InteractionManager without modifying renderers directly.
    /// </summary>
    [DisallowMultipleComponent]
    public class MaterialMenuUI : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Reference to the master UIManager.")]
        [SerializeField] private UIManager _uiManager;

        [Tooltip("Reference to the InteractionManager gateway.")]
        [SerializeField] private InteractionManager _interactionManager;

        [Tooltip("Master database catalog containing surface material categories and items.")]
        [SerializeField] private MaterialDatabaseSO _database;

        [Header("UI Containers")]
        [Tooltip("Parent transform for dynamically generated surface category tab buttons.")]
        [SerializeField] private Transform _categoryContainer;

        [Tooltip("Parent transform for dynamically generated material swatch buttons.")]
        [SerializeField] private Transform _materialGridContainer;

        [Header("UI Prefabs")]
        [Tooltip("Prefab instantiated for each surface category tab button.")]
        [SerializeField] private GameObject _categoryButtonPrefab;

        [Tooltip("Prefab instantiated for each material swatch tile button.")]
        [SerializeField] private GameObject _materialButtonPrefab;

        [Header("Preview Details Panel")]
        [Tooltip("UI Image component displaying selected material thumbnail or swatch tint.")]
        [SerializeField] private Image _previewImage;

        [Tooltip("UI Text component displaying selected material display name.")]
        [SerializeField] private Text _materialNameText;

        [Tooltip("UI Text component displaying selected material description.")]
        [SerializeField] private Text _materialDescriptionText;

        // --- C# Events ---

        /// <summary>
        /// Raised when a material swatch tile is selected by the user.
        /// </summary>
        public event Action<MaterialItemSO> OnMaterialSelected;

        /// <summary>
        /// Raised when the active surface category tab changes.
        /// </summary>
        public event Action<SurfaceType> OnSurfaceChanged;

        // --- Runtime State & Pools ---

        private SurfaceType _activeSurface = SurfaceType.None;
        private MaterialItemSO _selectedMaterial;

        private readonly List<GameObject> _spawnedCategoryButtons = new List<GameObject>();
        private readonly List<GameObject> _spawnedMaterialButtons = new List<GameObject>();

        // --- Public Properties ---

        public SurfaceType ActiveSurface => _activeSurface;
        public MaterialItemSO SelectedMaterial => _selectedMaterial;

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
                _interactionManager.OnMaterialSlotSelected += HandleSlotSelected;
            }

            RefreshMenu();
        }

        private void OnDisable()
        {
            if (_interactionManager != null)
            {
                _interactionManager.OnMaterialSlotSelected -= HandleSlotSelected;
            }
        }

        // --- Public Control APIs ---

        /// <summary>
        /// Re-populates and refreshes all surface category tabs and material swatches from MaterialDatabaseSO.
        /// </summary>
        public void RefreshMenu()
        {
            BuildCategoryTabs();

            // Default to first surface category if available
            if (_database != null && _database.Categories.Count > 0)
            {
                ShowSurface(_database.Categories[0].SurfaceType);
            }
        }

        /// <summary>
        /// Switches the visible material grid to display options belonging to the specified SurfaceType.
        /// </summary>
        /// <param name="surfaceType">SurfaceType enum to display.</param>
        public void ShowSurface(SurfaceType surfaceType)
        {
            _activeSurface = surfaceType;
            OnSurfaceChanged?.Invoke(_activeSurface);

            BuildMaterialGrid(_activeSurface);
        }

        /// <summary>
        /// Displays information for the currently active material in the preview details panel.
        /// </summary>
        public void ShowCurrentSelection()
        {
            if (_selectedMaterial != null)
            {
                if (_previewImage != null)
                {
                    _previewImage.sprite = _selectedMaterial.Icon;
                    _previewImage.color = _selectedMaterial.PreviewColor;
                    _previewImage.enabled = true;
                }

                if (_materialNameText != null)
                {
                    _materialNameText.text = _selectedMaterial.DisplayName;
                }

                if (_materialDescriptionText != null)
                {
                    _materialDescriptionText.text = _selectedMaterial.Description;
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
            _selectedMaterial = null;

            if (_previewImage != null)
            {
                _previewImage.sprite = null;
                _previewImage.color = Color.white;
                _previewImage.enabled = false;
            }

            if (_materialNameText != null)
            {
                _materialNameText.text = "Select a Material";
            }

            if (_materialDescriptionText != null)
            {
                _materialDescriptionText.text = string.Empty;
            }
        }

        // --- Internal UI Generation & Handling ---

        private void HandleSlotSelected(World.MaterialSlot slot)
        {
            if (slot != null)
            {
                ShowSurface(slot.SurfaceType);
                if (slot.CurrentMaterial != null)
                {
                    SelectMaterial(slot.CurrentMaterial);
                }
            }
        }

        private void BuildCategoryTabs()
        {
            ClearCategoryButtons();

            if (_database == null || _categoryContainer == null || _categoryButtonPrefab == null) return;

            IReadOnlyList<MaterialCategorySO> categories = _database.Categories;
            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i] == null) continue;

                MaterialCategorySO catSO = categories[i];
                GameObject btnObj = Instantiate(_categoryButtonPrefab, _categoryContainer);
                _spawnedCategoryButtons.Add(btnObj);

                // Configure button label
                Text label = btnObj.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = catSO.CategoryName;
                }

                // Configure button icon
                Image iconImage = btnObj.GetComponentInChildren<Image>();
                if (iconImage != null && catSO.CategoryIcon != null)
                {
                    iconImage.sprite = catSO.CategoryIcon;
                }

                // Bind UnityEvent callback
                Button button = btnObj.GetComponent<Button>();
                if (button != null)
                {
                    SurfaceType surfType = catSO.SurfaceType;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => ShowSurface(surfType));
                }
            }
        }

        private void BuildMaterialGrid(SurfaceType surfaceType)
        {
            ClearMaterialButtons();

            if (_database == null || _materialGridContainer == null || _materialButtonPrefab == null) return;

            MaterialCategorySO categorySO = _database.GetCategory(surfaceType);
            if (categorySO == null) return;

            IReadOnlyList<MaterialItemSO> materials = categorySO.Materials;
            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i] == null) continue;

                MaterialItemSO matItem = materials[i];
                GameObject btnObj = Instantiate(_materialButtonPrefab, _materialGridContainer);
                _spawnedMaterialButtons.Add(btnObj);

                // Configure button label
                Text label = btnObj.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = matItem.DisplayName;
                }

                // Configure swatch icon / tint
                Image swatchImage = btnObj.GetComponentInChildren<Image>();
                if (swatchImage != null)
                {
                    if (matItem.Icon != null)
                    {
                        swatchImage.sprite = matItem.Icon;
                    }
                    swatchImage.color = matItem.PreviewColor;
                }

                // Bind click event
                Button button = btnObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => SelectMaterial(matItem));
                }
            }
        }

        private void SelectMaterial(MaterialItemSO material)
        {
            if (material == null) return;

            _selectedMaterial = material;
            ShowCurrentSelection();
            OnMaterialSelected?.Invoke(_selectedMaterial);

            // Forward selection to InteractionManager
            if (_interactionManager != null)
            {
                _interactionManager.ApplyMaterial(_selectedMaterial);
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

        private void ClearMaterialButtons()
        {
            for (int i = 0; i < _spawnedMaterialButtons.Count; i++)
            {
                if (_spawnedMaterialButtons[i] != null)
                {
                    Destroy(_spawnedMaterialButtons[i]);
                }
            }
            _spawnedMaterialButtons.Clear();
        }
    }
}
