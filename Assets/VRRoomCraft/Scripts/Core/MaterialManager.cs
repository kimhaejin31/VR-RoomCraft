using System;
using System.Collections.Generic;
using UnityEngine;
using VRRoomCraft.Data.Materials;
using VRRoomCraft.World;

namespace VRRoomCraft.Core
{
    /// <summary>
    /// Core orchestration manager for architectural surface material customization in VR RoomCraft.
    /// Delegates material rendering application to MaterialSlot components and resolves data items via MaterialDatabaseSO.
    /// Completely decoupled from UI views, XR input, and static singletons.
    /// </summary>
    [DisallowMultipleComponent]
    public class MaterialManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Reference to the central RoomContext scene binder.")]
        [SerializeField] private RoomContext _roomContext;

        [Tooltip("Master catalog database containing all surface material categories and items.")]
        [SerializeField] private MaterialDatabaseSO _database;

        // --- C# Events ---

        /// <summary>
        /// Raised whenever a surface material is changed on any slot in the room.
        /// Signature: (MaterialSlot slot, MaterialItemSO newMaterial)
        /// </summary>
        public event Action<MaterialSlot, MaterialItemSO> OnMaterialChanged;

        /// <summary>
        /// Raised when all surface material slots in the apartment are reset to default settings.
        /// </summary>
        public event Action OnMaterialReset;

        // --- Public Read-Only Properties ---

        /// <summary>
        /// Gets the bound RoomContext instance.
        /// </summary>
        public RoomContext RoomContext => _roomContext;

        /// <summary>
        /// Gets the bound MaterialDatabaseSO instance.
        /// </summary>
        public MaterialDatabaseSO Database => _database;

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
                Debug.LogError("[MaterialManager] Missing required RoomContext dependency!");
            }

            if (_database == null)
            {
                Debug.LogError("[MaterialManager] Missing required MaterialDatabaseSO dependency!");
            }
        }

        // --- Public Control APIs ---

        /// <summary>
        /// Applies a surface material using unique string IDs.
        /// </summary>
        /// <param name="slotId">Unique ID of the target MaterialSlot.</param>
        /// <param name="materialId">Unique ID of the MaterialItemSO asset.</param>
        /// <returns>True if swap succeeded, false otherwise.</returns>
        public bool SetMaterial(string slotId, string materialId)
        {
            if (string.IsNullOrEmpty(slotId) || string.IsNullOrEmpty(materialId))
            {
                Debug.LogWarning("[MaterialManager] SetMaterial called with null or empty IDs.");
                return false;
            }

            if (_roomContext == null || _database == null)
            {
                Debug.LogError("[MaterialManager] Dependencies unassigned.");
                return false;
            }

            MaterialSlot slot = _roomContext.GetMaterialSlot(slotId);
            if (slot == null)
            {
                Debug.LogWarning($"[MaterialManager] MaterialSlot ID '{slotId}' not found in RoomContext.");
                return false;
            }

            MaterialItemSO item = _database.GetMaterialById(materialId);
            if (item == null)
            {
                Debug.LogWarning($"[MaterialManager] Material ID '{materialId}' not found in MaterialDatabaseSO.");
                return false;
            }

            return SetMaterial(slot, item);
        }

        /// <summary>
        /// Applies a surface material on a direct MaterialSlot reference with a MaterialItemSO asset.
        /// </summary>
        /// <param name="slot">Target MaterialSlot instance.</param>
        /// <param name="material">New MaterialItemSO asset to apply.</param>
        /// <returns>True if swap succeeded, false otherwise.</returns>
        public bool SetMaterial(MaterialSlot slot, MaterialItemSO material)
        {
            if (slot == null || material == null)
            {
                Debug.LogWarning("[MaterialManager] SetMaterial failed: slot or material is null.");
                return false;
            }

            // Validate SurfaceType compatibility
            if (slot.SurfaceType != SurfaceType.None && material.SurfaceType != slot.SurfaceType)
            {
                Debug.LogWarning($"[MaterialManager] SurfaceType mismatch! Slot '{slot.SlotId}' expects {slot.SurfaceType}, but material '{material.DisplayName}' is {material.SurfaceType}.");
                return false;
            }

            // Delegate material application to MaterialSlot
            bool success = slot.SetMaterial(material);
            if (success)
            {
                OnMaterialChanged?.Invoke(slot, material);
            }

            return success;
        }

        /// <summary>
        /// Resets a specific material slot back to its default material configuration.
        /// </summary>
        /// <param name="slotId">Unique ID of the slot to reset.</param>
        /// <returns>True if reset succeeded, false otherwise.</returns>
        public bool ResetSlot(string slotId)
        {
            if (_roomContext == null) return false;

            MaterialSlot slot = _roomContext.GetMaterialSlot(slotId);
            if (slot == null)
            {
                Debug.LogWarning($"[MaterialManager] Cannot reset slot '{slotId}': not found.");
                return false;
            }

            slot.ResetToDefault();
            OnMaterialChanged?.Invoke(slot, slot.CurrentMaterial);
            return true;
        }

        /// <summary>
        /// Resets all registered surface material slots in the apartment to default materials.
        /// </summary>
        public void ResetAllMaterials()
        {
            if (_roomContext == null) return;

            IReadOnlyList<MaterialSlot> slots = _roomContext.MaterialSlots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null) continue;
                slots[i].ResetToDefault();
            }

            OnMaterialReset?.Invoke();
        }

        /// <summary>
        /// Gets the active MaterialItemSO asset for a specified slot ID.
        /// </summary>
        /// <param name="slotId">Unique ID of the slot.</param>
        /// <returns>Active MaterialItemSO or null.</returns>
        public MaterialItemSO GetCurrentMaterial(string slotId)
        {
            if (_roomContext == null) return null;
            MaterialSlot slot = _roomContext.GetMaterialSlot(slotId);
            return slot != null ? slot.CurrentMaterial : null;
        }
    }
}
