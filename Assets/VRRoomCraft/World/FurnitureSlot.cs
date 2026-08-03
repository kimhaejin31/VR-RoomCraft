using System;
using UnityEngine;
using VRRoomCraft.Data.Furniture;
using VRRoomCraft.Data.Save;

namespace VRRoomCraft.World
{
    /// <summary>
    /// Represents a physical furniture spawn anchor placed in the apartment scene (e.g. Master Bed Anchor, Living Room Sofa Anchor).
    /// Manages spawning, destruction, positioning, and alignment of 3D furniture prefabs for its assigned category.
    /// </summary>
    [DisallowMultipleComponent]
    public class FurnitureSlot : MonoBehaviour, ISaveable
    {
        [Header("Slot Configuration")]
        [Tooltip("Unique string identifier for this scene slot (used for Save/Load serialization).")]
        [SerializeField] private string _slotId;

        [Tooltip("Furniture category permitted in this slot.")]
        [SerializeField] private FurnitureCategory _category = FurnitureCategory.None;

        [Tooltip("Transform anchor used as origin for spawning. If unassigned, defaults to this GameObject's Transform.")]
        [SerializeField] private Transform _spawnPoint;

        [Tooltip("Optional initial furniture item asset spawned on scene start.")]
        [SerializeField] private FurnitureItemSO _defaultItem;

        [Header("Runtime State (Read-Only)")]
        [Tooltip("Currently active furniture item asset.")]
        [SerializeField] private FurnitureItemSO _currentItem;

        [Tooltip("Currently instantiated 3D furniture GameObject.")]
        [SerializeField] private GameObject _currentSpawnedInstance;

        // --- C# Events ---

        /// <summary>
        /// Raised whenever the spawned furniture in this slot changes or is cleared.
        /// Signature: (FurnitureSlot slot, FurnitureItemSO newItem)
        /// </summary>
        public event Action<FurnitureSlot, FurnitureItemSO> OnFurnitureChanged;

        // --- Public Read-Only Properties ---

        /// <summary>
        /// Gets the unique slot identifier.
        /// </summary>
        public string SlotId => _slotId;

        /// <summary>
        /// Gets the unique save identifier contract for ISaveable.
        /// </summary>
        public string SaveId => _slotId;

        /// <summary>
        /// Gets the category of furniture permitted in this slot.
        /// </summary>
        public FurnitureCategory Category => _category;

        /// <summary>
        /// Gets the currently active FurnitureItemSO asset data.
        /// </summary>
        public FurnitureItemSO CurrentItem => _currentItem;

        /// <summary>
        /// Gets the currently spawned GameObject instance.
        /// </summary>
        public GameObject CurrentSpawnedInstance => _currentSpawnedInstance;

        /// <summary>
        /// Gets the spawn point transform (falls back to transform if unassigned).
        /// </summary>
        public Transform SpawnPoint => _spawnPoint != null ? _spawnPoint : transform;

        // --- Unity Lifecycle ---

        private void Awake()
        {
            if (string.IsNullOrEmpty(_slotId))
            {
                _slotId = gameObject.name;
            }

            if (_spawnPoint == null)
            {
                _spawnPoint = transform;
            }
        }

        private void Start()
        {
            if (_defaultItem != null && _currentSpawnedInstance == null)
            {
                SetFurniture(_defaultItem);
            }
        }

        // --- Public Control APIs ---

        /// <summary>
        /// Replaces the current furniture with a new FurnitureItemSO prefab.
        /// Clears the old instance, instantiates the new prefab with offset, and raises OnFurnitureChanged.
        /// </summary>
        /// <param name="item">New FurnitureItemSO asset to spawn.</param>
        /// <returns>True if replacement succeeded, false otherwise.</returns>
        public bool SetFurniture(FurnitureItemSO item)
        {
            if (item == null)
            {
                Debug.LogWarning($"[FurnitureSlot {_slotId}] Cannot set null FurnitureItemSO. Use ClearFurniture() instead.");
                return false;
            }

            if (_category != FurnitureCategory.None && item.Category != _category)
            {
                Debug.LogWarning($"[FurnitureSlot {_slotId}] Category mismatch! Slot accepts {_category}, but item is {item.Category}.");
                return false;
            }

            if (item.Prefab == null)
            {
                Debug.LogError($"[FurnitureSlot {_slotId}] Item '{item.DisplayName}' has no assigned Prefab!");
                return false;
            }

            // Safely clear existing furniture object
            ClearFurniture();

            // Store active item reference
            _currentItem = item;

            // Calculate spawn position and rotation incorporating item-specific offsets
            Transform anchor = SpawnPoint;
            Vector3 targetPosition = anchor.position + anchor.TransformVector(item.SpawnPositionOffset);
            Quaternion targetRotation = anchor.rotation * Quaternion.Euler(item.SpawnRotationOffset);

            // Instantiate 3D model prefab under anchor transform
            _currentSpawnedInstance = Instantiate(item.Prefab, targetPosition, targetRotation, anchor);

            // Dispatch notification event
            OnFurnitureChanged?.Invoke(this, _currentItem);
            return true;
        }

        /// <summary>
        /// Destroys the currently spawned furniture GameObject and resets runtime state.
        /// </summary>
        public void ClearFurniture()
        {
            if (_currentSpawnedInstance != null)
            {
                Destroy(_currentSpawnedInstance);
                _currentSpawnedInstance = null;
            }

            _currentItem = null;
            OnFurnitureChanged?.Invoke(this, null);
        }

        /// <summary>
        /// Gets the active 3D furniture GameObject instance.
        /// </summary>
        /// <returns>Active GameObject or null.</returns>
        public GameObject GetCurrentFurniture()
        {
            return _currentSpawnedInstance;
        }

        /// <summary>
        /// Checks whether a furniture item is currently spawned in this slot.
        /// </summary>
        /// <returns>True if furniture exists, false if empty.</returns>
        public bool HasFurniture()
        {
            return _currentSpawnedInstance != null;
        }

        // --- ISaveable Contract ---

        public object GetSaveState()
        {
            return _currentItem != null ? _currentItem.ItemId : string.Empty;
        }

        public void LoadSaveState(object state)
        {
            // Handled via FurnitureManager during state load
        }
    }
}
