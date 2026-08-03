using UnityEngine;

namespace VRRoomCraft.Data.Furniture
{
    /// <summary>
    /// ScriptableObject data asset representing an individual furniture item variant in VR RoomCraft.
    /// Encapsulates metadata, UI representation, 3D prefab reference, and positional/rotational offsets.
    /// </summary>
    [CreateAssetMenu(fileName = "FurnitureItem_", menuName = "VR RoomCraft/Furniture/Furniture Item", order = 1)]
    public class FurnitureItemSO : ScriptableObject
    {
        [Header("Item Identification")]
        [Tooltip("Unique identifier used for lookup, matching, and Save/Load serialization.")]
        [SerializeField] private string _itemId;

        [Tooltip("Human-readable name displayed in the VR Floating Menu.")]
        [SerializeField] private string _displayName;

        [Tooltip("Furniture category this item belongs to.")]
        [SerializeField] private FurnitureCategory _category = FurnitureCategory.None;

        [Header("UI Presentation")]
        [Tooltip("Icon thumbnail displayed on the VR menu button tile.")]
        [SerializeField] private Sprite _icon;

        [Tooltip("Brief description or specifications of the furniture item (optional).")]
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        [Header("Prefab & Placement Settings")]
        [Tooltip("3D furniture model prefab instantiated when this item is selected.")]
        [SerializeField] private GameObject _prefab;

        [Tooltip("Local position offset applied when spawning at a FurnitureSlot to compensate for model pivot discrepancies.")]
        [SerializeField] private Vector3 _spawnPositionOffset = Vector3.zero;

        [Tooltip("Local rotation offset applied when spawning at a FurnitureSlot to align orientation.")]
        [SerializeField] private Vector3 _spawnRotationOffset = Vector3.zero;

        // --- Public Read-Only Properties ---

        /// <summary>
        /// Gets the unique string ID for this furniture item.
        /// </summary>
        public string ItemId => _itemId;

        /// <summary>
        /// Gets the user-facing display name.
        /// </summary>
        public string DisplayName => _displayName;

        /// <summary>
        /// Gets the category enum this item belongs to.
        /// </summary>
        public FurnitureCategory Category => _category;

        /// <summary>
        /// Gets the UI sprite thumbnail icon.
        /// </summary>
        public Sprite Icon => _icon;

        /// <summary>
        /// Gets the item description.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Gets the 3D GameObject prefab reference.
        /// </summary>
        public GameObject Prefab => _prefab;

        /// <summary>
        /// Gets the local position offset for spawning.
        /// </summary>
        public Vector3 SpawnPositionOffset => _spawnPositionOffset;

        /// <summary>
        /// Gets the local rotation offset (Euler angles) for spawning.
        /// </summary>
        public Vector3 SpawnRotationOffset => _spawnRotationOffset;
    }
}
