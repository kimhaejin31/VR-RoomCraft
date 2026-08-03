using System.Collections.Generic;
using UnityEngine;

namespace VRRoomCraft.Data.Materials
{
    /// <summary>
    /// ScriptableObject container that groups multiple MaterialItemSO assets under a specific SurfaceType.
    /// Drives the VR Material Menu tab population and surface-level material queries.
    /// </summary>
    [CreateAssetMenu(fileName = "MaterialCategory_", menuName = "VR RoomCraft/Materials/Material Category", order = 2)]
    public class MaterialCategorySO : ScriptableObject
    {
        [Header("Category Data")]
        [Tooltip("Unique string identifier for this material category.")]
        [SerializeField] private string _categoryId;

        [Tooltip("Target surface type this category represents (Wall, Floor, Ceiling, etc.).")]
        [SerializeField] private SurfaceType _surfaceType = SurfaceType.None;

        [Tooltip("Human-readable category title displayed on VR UI tabs.")]
        [SerializeField] private string _categoryName;

        [Tooltip("Icon rendered on the VR UI tab button.")]
        [SerializeField] private Sprite _categoryIcon;

        [Header("Material Options")]
        [Tooltip("List of all available material options in this category.")]
        [SerializeField] private List<MaterialItemSO> _materials = new List<MaterialItemSO>();

        // --- Public Read-Only Properties ---

        /// <summary>
        /// Gets the unique category identifier.
        /// </summary>
        public string CategoryId => _categoryId;

        /// <summary>
        /// Gets the surface target enum type.
        /// </summary>
        public SurfaceType SurfaceType => _surfaceType;

        /// <summary>
        /// Gets the display name of this category.
        /// </summary>
        public string CategoryName => _categoryName;

        /// <summary>
        /// Gets the tab icon sprite for this category.
        /// </summary>
        public Sprite CategoryIcon => _categoryIcon;

        /// <summary>
        /// Gets a read-only list of all materials in this category.
        /// </summary>
        public IReadOnlyList<MaterialItemSO> Materials => _materials;

        /// <summary>
        /// Gets the default material item for this category (first item in the list if available).
        /// </summary>
        public MaterialItemSO DefaultMaterial => (_materials != null && _materials.Count > 0) ? _materials[0] : null;

        // --- Public Helper Methods ---

        /// <summary>
        /// Retrieves a MaterialItemSO by its unique material string ID within this category.
        /// </summary>
        /// <param name="materialId">Unique string ID of the material.</param>
        /// <returns>Matching MaterialItemSO or null if not found.</returns>
        public MaterialItemSO GetMaterialById(string materialId)
        {
            if (string.IsNullOrEmpty(materialId) || _materials == null) return null;

            for (int i = 0; i < _materials.Count; i++)
            {
                if (_materials[i] != null && _materials[i].MaterialId == materialId)
                {
                    return _materials[i];
                }
            }

            return null;
        }
    }
}
