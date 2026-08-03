using System.Collections.Generic;
using UnityEngine;

namespace VRRoomCraft.Data.Materials
{
    /// <summary>
    /// Master database ScriptableObject catalog storing all MaterialCategorySO assets in VR RoomCraft.
    /// Serves as the central query hub for MaterialManager, MaterialMenuUI, and SaveManager lookup.
    /// </summary>
    [CreateAssetMenu(fileName = "MaterialDatabase", menuName = "VR RoomCraft/Materials/Material Database", order = 3)]
    public class MaterialDatabaseSO : ScriptableObject
    {
        [Header("Master Catalog")]
        [Tooltip("List of all registered surface material categories in the project.")]
        [SerializeField] private List<MaterialCategorySO> _categories = new List<MaterialCategorySO>();

        // --- Public Read-Only Properties ---

        /// <summary>
        /// Gets a read-only list of all registered material categories.
        /// </summary>
        public IReadOnlyList<MaterialCategorySO> Categories => _categories;

        // --- Public Query Methods ---

        /// <summary>
        /// Retrieves a MaterialCategorySO by its targeted SurfaceType enum.
        /// </summary>
        /// <param name="surfaceType">SurfaceType enum value.</param>
        /// <returns>Matching MaterialCategorySO or null if not registered.</returns>
        public MaterialCategorySO GetCategory(SurfaceType surfaceType)
        {
            if (_categories == null || surfaceType == SurfaceType.None) return null;

            for (int i = 0; i < _categories.Count; i++)
            {
                if (_categories[i] != null && _categories[i].SurfaceType == surfaceType)
                {
                    return _categories[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Searches all material categories to find a MaterialItemSO by its unique string ID.
        /// Essential for Save/Load state restoration.
        /// </summary>
        /// <param name="materialId">Unique string ID of the material.</param>
        /// <returns>Matching MaterialItemSO or null if not found.</returns>
        public MaterialItemSO GetMaterialById(string materialId)
        {
            if (string.IsNullOrEmpty(materialId) || _categories == null) return null;

            for (int i = 0; i < _categories.Count; i++)
            {
                if (_categories[i] == null) continue;

                MaterialItemSO item = _categories[i].GetMaterialById(materialId);
                if (item != null)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the default material item for a given surface type enum.
        /// </summary>
        /// <param name="surfaceType">SurfaceType enum value.</param>
        /// <returns>Default MaterialItemSO or null.</returns>
        public MaterialItemSO GetDefaultMaterialForSurface(SurfaceType surfaceType)
        {
            MaterialCategorySO categorySO = GetCategory(surfaceType);
            return categorySO != null ? categorySO.DefaultMaterial : null;
        }
    }
}
