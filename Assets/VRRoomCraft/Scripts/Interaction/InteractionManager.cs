using System;
using UnityEngine;
using VRRoomCraft.Data.Furniture;
using VRRoomCraft.Data.Materials;
using VRRoomCraft.World;

namespace VRRoomCraft.Core.Interaction
{
    /// <summary>
    /// Gateway manager routing user interaction requests (from XR Raycasts, UI menus, or mouse)
    /// to FurnitureManager, MaterialManager, and DoorController instances.
    /// Manages active selection states while remaining completely decoupled from UI graphics and XR toolkits.
    /// </summary>
    [DisallowMultipleComponent]
    public class InteractionManager : MonoBehaviour
    {
        [Header("Core Dependencies")]
        [Tooltip("Reference to the central RoomContext scene binder.")]
        [SerializeField] private RoomContext _roomContext;

        [Tooltip("Reference to the core FurnitureManager.")]
        [SerializeField] private FurnitureManager _furnitureManager;

        [Tooltip("Reference to the core MaterialManager.")]
        [SerializeField] private MaterialManager _materialManager;

        [Header("Active Selection State (Read-Only)")]
        [Tooltip("Currently selected FurnitureSlot in the apartment.")]
        [SerializeField] private FurnitureSlot _selectedFurnitureSlot;

        [Tooltip("Currently selected MaterialSlot in the apartment.")]
        [SerializeField] private MaterialSlot _selectedMaterialSlot;

        // --- C# Events ---

        /// <summary>
        /// Raised when a FurnitureSlot is selected by the user.
        /// </summary>
        public event Action<FurnitureSlot> OnFurnitureSlotSelected;

        /// <summary>
        /// Raised when a MaterialSlot is selected by the user.
        /// </summary>
        public event Action<MaterialSlot> OnMaterialSlotSelected;

        /// <summary>
        /// Raised when furniture is successfully applied to a slot.
        /// </summary>
        public event Action<FurnitureSlot, FurnitureItemSO> OnFurnitureApplied;

        /// <summary>
        /// Raised when a surface material is successfully applied to a slot.
        /// </summary>
        public event Action<MaterialSlot, MaterialItemSO> OnMaterialApplied;

        /// <summary>
        /// Raised when a door is toggled.
        /// </summary>
        public event Action<DoorController, bool> OnDoorToggled;

        // --- Public Read-Only Properties ---

        /// <summary>
        /// Gets the currently selected FurnitureSlot.
        /// </summary>
        public FurnitureSlot SelectedFurnitureSlot => _selectedFurnitureSlot;

        /// <summary>
        /// Gets the currently selected MaterialSlot.
        /// </summary>
        public MaterialSlot SelectedMaterialSlot => _selectedMaterialSlot;

        // --- Unity Lifecycle ---

        private void Awake()
        {
            if (_roomContext == null) _roomContext = GetComponent<RoomContext>();
            if (_furnitureManager == null) _furnitureManager = GetComponent<FurnitureManager>();
            if (_materialManager == null) _materialManager = GetComponent<MaterialManager>();

            if (_roomContext == null) Debug.LogError("[InteractionManager] Missing RoomContext dependency!");
            if (_furnitureManager == null) Debug.LogError("[InteractionManager] Missing FurnitureManager dependency!");
            if (_materialManager == null) Debug.LogError("[InteractionManager] Missing MaterialManager dependency!");
        }

        // --- Furniture Interaction Methods ---

        /// <summary>
        /// Sets the active target FurnitureSlot for customization.
        /// </summary>
        /// <param name="slot">Target FurnitureSlot instance.</param>
        public void SelectFurnitureSlot(FurnitureSlot slot)
        {
            if (slot == null)
            {
                Debug.LogWarning("[InteractionManager] SelectFurnitureSlot called with null slot.");
                return;
            }

            _selectedFurnitureSlot = slot;
            OnFurnitureSlotSelected?.Invoke(_selectedFurnitureSlot);
        }

        /// <summary>
        /// Applies a furniture item by ID to the currently selected FurnitureSlot.
        /// </summary>
        public bool ApplyFurniture(string furnitureId)
        {
            if (_selectedFurnitureSlot == null)
            {
                Debug.LogWarning("[InteractionManager] Cannot apply furniture: No FurnitureSlot selected.");
                return false;
            }

            if (_furnitureManager == null)
            {
                Debug.LogError("[InteractionManager] FurnitureManager dependency missing.");
                return false;
            }

            bool success = _furnitureManager.SetFurniture(_selectedFurnitureSlot.SlotId, furnitureId);
            if (success)
            {
                OnFurnitureApplied?.Invoke(_selectedFurnitureSlot, _selectedFurnitureSlot.CurrentItem);
            }

            return success;
        }

        /// <summary>
        /// Applies a FurnitureItemSO asset to the currently selected FurnitureSlot.
        /// </summary>
        public bool ApplyFurniture(FurnitureItemSO item)
        {
            if (_selectedFurnitureSlot == null)
            {
                Debug.LogWarning("[InteractionManager] Cannot apply furniture: No FurnitureSlot selected.");
                return false;
            }

            if (_furnitureManager == null)
            {
                Debug.LogError("[InteractionManager] FurnitureManager dependency missing.");
                return false;
            }

            bool success = _furnitureManager.SetFurniture(_selectedFurnitureSlot, item);
            if (success)
            {
                OnFurnitureApplied?.Invoke(_selectedFurnitureSlot, item);
            }

            return success;
        }

        /// <summary>
        /// Clears the active FurnitureSlot selection.
        /// </summary>
        public void ClearSelectedFurniture()
        {
            _selectedFurnitureSlot = null;
            OnFurnitureSlotSelected?.Invoke(null);
        }

        // --- Material Interaction Methods ---

        /// <summary>
        /// Sets the active target MaterialSlot for surface customization.
        /// </summary>
        /// <param name="slot">Target MaterialSlot instance.</param>
        public void SelectMaterialSlot(MaterialSlot slot)
        {
            if (slot == null)
            {
                Debug.LogWarning("[InteractionManager] SelectMaterialSlot called with null slot.");
                return;
            }

            _selectedMaterialSlot = slot;
            OnMaterialSlotSelected?.Invoke(_selectedMaterialSlot);
        }

        /// <summary>
        /// Applies a surface material by ID to the currently selected MaterialSlot.
        /// </summary>
        public bool ApplyMaterial(string materialId)
        {
            if (_selectedMaterialSlot == null)
            {
                Debug.LogWarning("[InteractionManager] Cannot apply material: No MaterialSlot selected.");
                return false;
            }

            if (_materialManager == null)
            {
                Debug.LogError("[InteractionManager] MaterialManager dependency missing.");
                return false;
            }

            bool success = _materialManager.SetMaterial(_selectedMaterialSlot.SlotId, materialId);
            if (success)
            {
                OnMaterialApplied?.Invoke(_selectedMaterialSlot, _selectedMaterialSlot.CurrentMaterial);
            }

            return success;
        }

        /// <summary>
        /// Applies a MaterialItemSO asset to the currently selected MaterialSlot.
        /// </summary>
        public bool ApplyMaterial(MaterialItemSO material)
        {
            if (_selectedMaterialSlot == null)
            {
                Debug.LogWarning("[InteractionManager] Cannot apply material: No MaterialSlot selected.");
                return false;
            }

            if (_materialManager == null)
            {
                Debug.LogError("[InteractionManager] MaterialManager dependency missing.");
                return false;
            }

            bool success = _materialManager.SetMaterial(_selectedMaterialSlot, material);
            if (success)
            {
                OnMaterialApplied?.Invoke(_selectedMaterialSlot, material);
            }

            return success;
        }

        /// <summary>
        /// Clears the active MaterialSlot selection.
        /// </summary>
        public void ClearSelectedMaterial()
        {
            _selectedMaterialSlot = null;
            OnMaterialSlotSelected?.Invoke(null);
        }

        // --- Door Interaction Methods ---

        /// <summary>
        /// Toggles open/closed state on a target DoorController.
        /// </summary>
        /// <param name="door">Target DoorController instance.</param>
        public void ToggleDoor(DoorController door)
        {
            if (door == null)
            {
                Debug.LogWarning("[InteractionManager] ToggleDoor called with null door.");
                return;
            }

            door.ToggleDoor();
            OnDoorToggled?.Invoke(door, door.IsOpenState);
        }
    }
}
