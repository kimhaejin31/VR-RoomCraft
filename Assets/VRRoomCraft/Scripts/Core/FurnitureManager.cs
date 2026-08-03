using System;
using System.Collections.Generic;
using UnityEngine;
using VRRoomCraft.Data.Furniture;
using VRRoomCraft.World;

namespace VRRoomCraft.Core
{
    /// <summary>
    /// Core orchestration manager for furniture customization in VR RoomCraft.
    /// Delegates prefab spawning to FurnitureSlot components and resolves data items via FurnitureDatabaseSO.
    /// Completely decoupled from UI views, XR input, and static singletons.
    /// </summary>
    [DisallowMultipleComponent]
    public class FurnitureManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Reference to the central RoomContext scene binder.")]
        [SerializeField] private RoomContext _roomContext;

        [Tooltip("Master catalog database containing all furniture categories and items.")]
        [SerializeField] private FurnitureDatabaseSO _database;

        // --- C# Events ---

        /// <summary>
        /// Raised whenever a furniture item is changed on any slot in the room.
        /// Signature: (FurnitureSlot slot, FurnitureItemSO newItem)
        /// </summary>
        public event Action<FurnitureSlot, FurnitureItemSO> OnFurnitureChanged;

        /// <summary>
        /// Raised when all furniture slots in the apartment are reset to default settings.
        /// </summary>
        public event Action OnFurnitureReset;

        // --- Public Read-Only Properties ---

        /// <summary>
        /// Gets the bound RoomContext instance.
        /// </summary>
        public RoomContext RoomContext => _roomContext;

        /// <summary>
        /// Gets the bound FurnitureDatabaseSO instance.
        /// </summary>
        public FurnitureDatabaseSO Database => _database;

        // --- Unity Lifecycle ---

        private void Awake()
        {
            if (_roomContext == null)
            {
                _roomContext = GetComponent<RoomContext>();
            }

            if (_roomContext == null)
            {
                _roomContext = FindFirstObjectByType<RoomContext>();
            }

            if (_roomContext == null)
            {
                Debug.LogError("[FurnitureManager] Missing required RoomContext dependency!");
            }

            if (_database == null)
            {
                Debug.LogError("[FurnitureManager] Missing required FurnitureDatabaseSO dependency!");
            }
        }

        // --- Public Control APIs ---

        /// <summary>
        /// Swaps furniture on a slot using unique string IDs.
        /// </summary>
        /// <param name="slotId">Unique ID of the target FurnitureSlot.</param>
        /// <param name="furnitureId">Unique ID of the FurnitureItemSO asset.</param>
        /// <returns>True if swap succeeded, false otherwise.</returns>
        public bool SetFurniture(string slotId, string furnitureId)
        {
            if (string.IsNullOrEmpty(slotId) || string.IsNullOrEmpty(furnitureId))
            {
                Debug.LogWarning("[FurnitureManager] SetFurniture called with null or empty IDs.");
                return false;
            }

            if (_roomContext == null || _database == null)
            {
                Debug.LogError("[FurnitureManager] Dependencies unassigned.");
                return false;
            }

            FurnitureSlot slot = _roomContext.GetFurnitureSlot(slotId);
            if (slot == null)
            {
                Debug.LogWarning($"[FurnitureManager] Slot ID '{slotId}' not found in RoomContext.");
                return false;
            }

            FurnitureItemSO item = _database.GetItemById(furnitureId);
            if (item == null)
            {
                Debug.LogWarning($"[FurnitureManager] Furniture ID '{furnitureId}' not found in FurnitureDatabaseSO.");
                return false;
            }

            return SetFurniture(slot, item);
        }

        /// <summary>
        /// Swaps furniture on a direct FurnitureSlot reference with a FurnitureItemSO asset.
        /// </summary>
        /// <param name="slot">Target FurnitureSlot instance.</param>
        /// <param name="item">New FurnitureItemSO asset to spawn.</param>
        /// <returns>True if swap succeeded, false otherwise.</returns>
        public bool SetFurniture(FurnitureSlot slot, FurnitureItemSO item)
        {
            if (slot == null || item == null)
            {
                Debug.LogWarning("[FurnitureManager] SetFurniture failed: slot or item is null.");
                return false;
            }

            // Validate category compatibility
            if (slot.Category != FurnitureCategory.None && item.Category != slot.Category)
            {
                Debug.LogWarning($"[FurnitureManager] Category mismatch! Slot '{slot.SlotId}' expects {slot.Category}, but item '{item.DisplayName}' is {item.Category}.");
                return false;
            }

            // Delegate prefab spawning to FurnitureSlot
            bool success = slot.SetFurniture(item);
            if (success)
            {
                OnFurnitureChanged?.Invoke(slot, item);
            }

            return success;
        }

        /// <summary>
        /// Resets a specific slot back to its default furniture configuration.
        /// </summary>
        /// <param name="slotId">Unique ID of the slot to reset.</param>
        /// <returns>True if reset succeeded, false otherwise.</returns>
        public bool ResetSlot(string slotId)
        {
            if (_roomContext == null) return false;

            FurnitureSlot slot = _roomContext.GetFurnitureSlot(slotId);
            if (slot == null)
            {
                Debug.LogWarning($"[FurnitureManager] Cannot reset slot '{slotId}': not found.");
                return false;
            }

            FurnitureItemSO defaultItem = slot.CurrentItem;
            if (_database != null)
            {
                FurnitureItemSO dbDefault = _database.GetDefaultItemForCategory(slot.Category);
                if (dbDefault != null)
                {
                    defaultItem = dbDefault;
                }
            }

            if (defaultItem != null)
            {
                return SetFurniture(slot, defaultItem);
            }

            slot.ClearFurniture();
            OnFurnitureChanged?.Invoke(slot, null);
            return true;
        }

        /// <summary>
        /// Resets all registered furniture slots in the apartment to default items.
        /// </summary>
        public void ResetAllFurniture()
        {
            if (_roomContext == null) return;

            IReadOnlyList<FurnitureSlot> slots = _roomContext.FurnitureSlots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null) continue;

                FurnitureItemSO defaultItem = null;
                if (_database != null)
                {
                    defaultItem = _database.GetDefaultItemForCategory(slots[i].Category);
                }

                if (defaultItem != null)
                {
                    slots[i].SetFurniture(defaultItem);
                }
                else
                {
                    slots[i].ClearFurniture();
                }
            }

            OnFurnitureReset?.Invoke();
        }

        /// <summary>
        /// Gets the active FurnitureItemSO asset for a specified slot ID.
        /// </summary>
        /// <param name="slotId">Unique ID of the slot.</param>
        /// <returns>Active FurnitureItemSO or null.</returns>
        public FurnitureItemSO GetCurrentFurniture(string slotId)
        {
            if (_roomContext == null) return null;
            FurnitureSlot slot = _roomContext.GetFurnitureSlot(slotId);
            return slot != null ? slot.CurrentItem : null;
        }
    }
}
