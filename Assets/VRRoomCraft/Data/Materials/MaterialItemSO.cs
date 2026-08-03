using UnityEngine;

namespace VRRoomCraft.Data.Materials
{
    /// <summary>
    /// ScriptableObject data asset representing an individual surface material variant in VR RoomCraft.
    /// Encapsulates metadata, UI presentation swatches, Unity Material reference, and UV tiling/offset parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "MaterialItem_", menuName = "VR RoomCraft/Materials/Material Item", order = 1)]
    public class MaterialItemSO : ScriptableObject
    {
        [Header("Material Identification")]
        [Tooltip("Unique identifier used for lookup, matching, and Save/Load serialization.")]
        [SerializeField] private string _materialId;

        [Tooltip("Human-readable name displayed in the VR Floating Material Menu.")]
        [SerializeField] private string _displayName;

        [Tooltip("Surface target type this material is designed for (Wall, Floor, Ceiling, etc.).")]
        [SerializeField] private SurfaceType _surfaceType = SurfaceType.None;

        [Header("UI Presentation")]
        [Tooltip("Thumbnail preview swatch displayed on the VR material menu button.")]
        [SerializeField] private Sprite _icon;

        [Tooltip("Optional preview tint color for UI swatch fallback or highlight overlays.")]
        [SerializeField] private Color _previewColor = Color.white;

        [Tooltip("Brief description of the material texture, finish, or specs (optional).")]
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        [Header("Material & UV Settings")]
        [Tooltip("Target Unity Material asset applied to environmental surface renderers.")]
        [SerializeField] private Material _material;

        [Tooltip("UV texture tiling scale applied to the main texture property (default 1, 1).")]
        [SerializeField] private Vector2 _textureTiling = Vector2.one;

        [Tooltip("UV texture offset applied to align texture patterns (default 0, 0).")]
        [SerializeField] private Vector2 _textureOffset = Vector2.zero;

        // --- Public Read-Only Properties ---

        /// <summary>
        /// Gets the unique string ID for this material item.
        /// </summary>
        public string MaterialId => _materialId;

        /// <summary>
        /// Gets the user-facing display name.
        /// </summary>
        public string DisplayName => _displayName;

        /// <summary>
        /// Gets the surface type enum this material targets.
        /// </summary>
        public SurfaceType SurfaceType => _surfaceType;

        /// <summary>
        /// Gets the UI thumbnail sprite icon.
        /// </summary>
        public Sprite Icon => _icon;

        /// <summary>
        /// Gets the preview swatch tint color.
        /// </summary>
        public Color PreviewColor => _previewColor;

        /// <summary>
        /// Gets the material description.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Gets the referenced UnityEngine.Material asset.
        /// </summary>
        public Material Material => _material;

        /// <summary>
        /// Gets the UV texture tiling scale vector.
        /// </summary>
        public Vector2 TextureTiling => _textureTiling;

        /// <summary>
        /// Gets the UV texture offset vector.
        /// </summary>
        public Vector2 TextureOffset => _textureOffset;
    }
}
