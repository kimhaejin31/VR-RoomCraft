using System.Collections.Generic;
using UnityEngine;

namespace VRRoomCraft.Data.Furniture
{
    /// <summary>
    /// Master database ScriptableObject catalog storing all FurnitureCategorySO assets in VR RoomCraft.
    /// Acts as the central query hub for FurnitureManager, UIManager, and future SaveManager lookup.
    /// </summary>
    [CreateAssetMenu(fileName = "FurnitureDatabase", menuName = "VR RoomCraft/Furniture/Furniture Database", order = 3)]
    public class FurnitureDatabaseSO : ScriptableObject
    {
        [Header("Master Catalog")]
        [Tooltip("List of all registered furniture categories in the project.")]
        [SerializeField] private List<FurnitureCategorySO> _categories = new List<FurnitureCategorySO>();

        // --- Public Read-Only Properties ---

        /// <summary>
        /// Gets a read-only list of all registered furniture categories.
        /// </summary>
        public IReadOnlyList<FurnitureCategorySO> Categories => _categories;

        // --- Public Query Methods ---

        /// <summary>
        /// Retrieves a FurnitureCategorySO by its enum category type.
        /// </summary>
        /// <param name="category">FurnitureCategory enum value.</param>
        /// <returns>Matching FurnitureCategorySO or null if not registered.</returns>
        public FurnitureCategorySO GetCategory(FurnitureCategory category)
        {
            if (_categories == null || category == FurnitureCategory.None) return null;

            for (int i = 0; i < _categories.Count; i++)
            {
                if (_categories[i] != null && _categories[i].Category == category)
                {
                    return _categories[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Searches all categories to find a FurnitureItemSO by its unique item string ID.
        /// Essential for Save/Load state restoration.
        /// </summary>
        /// <param name="itemId">Unique string ID of the item.</param>
        /// <returns>Matching FurnitureItemSO or null if not found.</returns>
        public FurnitureItemSO GetItemById(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || _categories == null) return null;

            for (int i = 0; i < _categories.Count; i++)
            {
                if (_categories[i] == null) continue;

                FurnitureItemSO item = _categories[i].GetItemById(itemId);
                if (item != null)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the default furniture item for a given category enum.
        /// </summary>
        /// <param name="category">FurnitureCategory enum type.</param>
        /// <returns>Default FurnitureItemSO or null.</returns>
        public FurnitureItemSO GetDefaultItemForCategory(FurnitureCategory category)
        {
            FurnitureCategorySO categorySO = GetCategory(category);
            return categorySO != null ? categorySO.DefaultItem : null;
        }
    }
}
