using System.Collections.Generic;
using UnityEngine;
using VRRoomCraft.Data.Furniture;
using VRRoomCraft.Data.Materials;
using VRRoomCraft.World;

namespace VRRoomCraft.Core
{
    /// <summary>
    /// Central scene registry and binder for the VR RoomCraft apartment.
    /// Discovers, validates, and stores references to all FurnitureSlot, MaterialSlot, and DoorController components in the scene.
    /// Provides fast query lookups for Managers and Save/Load serialization without hardcoded singletons.
    /// </summary>
    [DisallowMultipleComponent]
    public class RoomContext : MonoBehaviour
    {
        [Header("Scene Reference Discovery")]
        [Tooltip("If true, automatically scans scene for all Slots and Doors on Awake if lists are empty.")]
        [SerializeField] private bool _autoScanOnAwake = true;

        [Header("Registered World Elements")]
        [Tooltip("All registered FurnitureSlot components in the apartment scene.")]
        [SerializeField] private List<FurnitureSlot> _furnitureSlots = new List<FurnitureSlot>();

        [Tooltip("All registered MaterialSlot components in the apartment scene.")]
        [SerializeField] private List<MaterialSlot> _materialSlots = new List<MaterialSlot>();

        [Tooltip("All registered DoorController components in the apartment scene.")]
        [SerializeField] private List<DoorController> _doorControllers = new List<DoorController>();

        // Fast lookup dictionaries
        private readonly Dictionary<string, FurnitureSlot> _furnitureSlotMap = new Dictionary<string, FurnitureSlot>();
        private readonly Dictionary<string, MaterialSlot> _materialSlotMap = new Dictionary<string, MaterialSlot>();
        private readonly Dictionary<string, DoorController> _doorMap = new Dictionary<string, DoorController>();

        // --- Public Read-Only Properties ---

        /// <summary>
        /// Gets a read-only list of all registered FurnitureSlot instances in the room.
        /// </summary>
        public IReadOnlyList<FurnitureSlot> FurnitureSlots => _furnitureSlots;

        /// <summary>
        /// Gets a read-only list of all registered MaterialSlot instances in the room.
        /// </summary>
        public IReadOnlyList<MaterialSlot> MaterialSlots => _materialSlots;

        /// <summary>
        /// Gets a read-only list of all registered DoorController instances in the room.
        /// </summary>
        public IReadOnlyList<DoorController> DoorControllers => _doorControllers;

        // --- Unity Lifecycle ---

        private void Awake()
        {
            if (_autoScanOnAwake)
            {
                ScanSceneForReferences();
            }

            BuildLookupDictionaries();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ValidateUniqueIds();
        }
#endif

        // --- Scene Scanning & Validation ---

        /// <summary>
        /// Scans the scene to automatically discover and populate unassigned scene references.
        /// </summary>
        [ContextMenu("Scan Scene For References")]
        public void ScanSceneForReferences()
        {
            if (_furnitureSlots.Count == 0)
            {
                _furnitureSlots = new List<FurnitureSlot>(FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None));
            }

            if (_materialSlots.Count == 0)
            {
                _materialSlots = new List<MaterialSlot>(FindObjectsByType<MaterialSlot>(FindObjectsSortMode.None));
            }

            if (_doorControllers.Count == 0)
            {
                _doorControllers = new List<DoorController>(FindObjectsByType<DoorController>(FindObjectsSortMode.None));
            }
        }

        private void BuildLookupDictionaries()
        {
            _furnitureSlotMap.Clear();
            for (int i = 0; i < _furnitureSlots.Count; i++)
            {
                if (_furnitureSlots[i] == null) continue;
                string id = _furnitureSlots[i].SlotId;
                if (!_furnitureSlotMap.ContainsKey(id))
                {
                    _furnitureSlotMap.Add(id, _furnitureSlots[i]);
                }
                else
                {
                    Debug.LogWarning($"[RoomContext] Duplicate FurnitureSlot ID detected: '{id}' on GameObject '{_furnitureSlots[i].gameObject.name}'!");
                }
            }

            _materialSlotMap.Clear();
            for (int i = 0; i < _materialSlots.Count; i++)
            {
                if (_materialSlots[i] == null) continue;
                string id = _materialSlots[i].SlotId;
                if (!_materialSlotMap.ContainsKey(id))
                {
                    _materialSlotMap.Add(id, _materialSlots[i]);
                }
                else
                {
                    Debug.LogWarning($"[RoomContext] Duplicate MaterialSlot ID detected: '{id}' on GameObject '{_materialSlots[i].gameObject.name}'!");
                }
            }

            _doorMap.Clear();
            for (int i = 0; i < _doorControllers.Count; i++)
            {
                if (_doorControllers[i] == null) continue;
                string id = _doorControllers[i].DoorId;
                if (!_doorMap.ContainsKey(id))
                {
                    _doorMap.Add(id, _doorControllers[i]);
                }
                else
                {
                    Debug.LogWarning($"[RoomContext] Duplicate Door ID detected: '{id}' on GameObject '{_doorControllers[i].gameObject.name}'!");
                }
            }
        }

        private void ValidateUniqueIds()
        {
            HashSet<string> seenIds = new HashSet<string>();

            for (int i = 0; i < _furnitureSlots.Count; i++)
            {
                if (_furnitureSlots[i] == null) continue;
                if (!seenIds.Add(_furnitureSlots[i].SlotId))
                {
                    Debug.LogWarning($"[RoomContext Validation] Duplicate FurnitureSlot ID: '{_furnitureSlots[i].SlotId}'");
                }
            }

            seenIds.Clear();
            for (int i = 0; i < _materialSlots.Count; i++)
            {
                if (_materialSlots[i] == null) continue;
                if (!seenIds.Add(_materialSlots[i].SlotId))
                {
                    Debug.LogWarning($"[RoomContext Validation] Duplicate MaterialSlot ID: '{_materialSlots[i].SlotId}'");
                }
            }

            seenIds.Clear();
            for (int i = 0; i < _doorControllers.Count; i++)
            {
                if (_doorControllers[i] == null) continue;
                if (!seenIds.Add(_doorControllers[i].DoorId))
                {
                    Debug.LogWarning($"[RoomContext Validation] Duplicate Door ID: '{_doorControllers[i].DoorId}'");
                }
            }
        }

        // --- Query Helper Methods ---

        /// <summary>
        /// Retrieves a FurnitureSlot by its unique slot ID.
        /// </summary>
        public FurnitureSlot GetFurnitureSlot(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _furnitureSlotMap.TryGetValue(id, out FurnitureSlot slot);
            return slot;
        }

        /// <summary>
        /// Retrieves a MaterialSlot by its unique slot ID.
        /// </summary>
        public MaterialSlot GetMaterialSlot(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _materialSlotMap.TryGetValue(id, out MaterialSlot slot);
            return slot;
        }

        /// <summary>
        /// Retrieves a DoorController by its unique door ID.
        /// </summary>
        public DoorController GetDoor(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _doorMap.TryGetValue(id, out DoorController door);
            return door;
        }

        /// <summary>
        /// Gets all FurnitureSlots matching a specified FurnitureCategory enum.
        /// </summary>
        public List<FurnitureSlot> GetFurnitureSlotsByCategory(FurnitureCategory category)
        {
            List<FurnitureSlot> result = new List<FurnitureSlot>();
            for (int i = 0; i < _furnitureSlots.Count; i++)
            {
                if (_furnitureSlots[i] != null && _furnitureSlots[i].Category == category)
                {
                    result.Add(_furnitureSlots[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets all MaterialSlots matching a specified SurfaceType enum.
        /// </summary>
        public List<MaterialSlot> GetMaterialSlotsBySurface(SurfaceType surfaceType)
        {
            List<MaterialSlot> result = new List<MaterialSlot>();
            for (int i = 0; i < _materialSlots.Count; i++)
            {
                if (_materialSlots[i] != null && _materialSlots[i].SurfaceType == surfaceType)
                {
                    result.Add(_materialSlots[i]);
                }
            }
            return result;
        }
    }
}
