using System.Collections.Generic;
using UnityEngine;

namespace VRRoomCraft.Data.Furniture
{
    /// <summary>
    /// ScriptableObject container that groups multiple FurnitureItemSO assets under a specific FurnitureCategory.
    /// Drives the VR UI tab population and provides category-level queries.
    /// </summary>
    [CreateAssetMenu(fileName = "FurnitureCategory_", menuName = "VR RoomCraft/Furniture/Furniture Category", order = 2)]
    public class FurnitureCategorySO : ScriptableObject
    {
        [Header("Category Data")]
        [Tooltip("Unique string identifier for this furniture category.")]
        [SerializeField] private string _categoryId;

        [Tooltip("Strongly-typed category enum.")]
        [SerializeField] private FurnitureCategory _category = FurnitureCategory.None;

        [Tooltip("Human-readable category title displayed on VR UI tabs.")]
        [SerializeField] private string _categoryName;

        [Tooltip("Icon rendered on the VR UI tab button.")]
        [SerializeField] private Sprite _categoryIcon;

        [Header("Furniture Variants")]
        [Tooltip("List of all available furniture item options in this category.")]
        [SerializeField] private List<FurnitureItemSO> _items = new List<FurnitureItemSO>();

        // --- Public Read-Only Properties ---

        /// <summary>
        /// Gets the unique category identifier.
        /// </summary>
        public string CategoryId => _categoryId;

        /// <summary>
        /// Gets the category enum type.
        /// </summary>
        public FurnitureCategory Category => _category;

        /// <summary>
        /// Gets the display name of this category.
        /// </summary>
        public string CategoryName => _categoryName;

        /// <summary>
        /// Gets the tab icon sprite for this category.
        /// </summary>
        public Sprite CategoryIcon => _categoryIcon;

        /// <summary>
        /// Gets a read-only view of the items in this category.
        /// </summary>
        public IReadOnlyList<FurnitureItemSO> Items => _items;

        /// <summary>
        /// Gets the default furniture item for this category (first item in the list if available).
        /// </summary>
        public FurnitureItemSO DefaultItem => (_items != null && _items.Count > 0) ? _items[0] : null;

        /// <summary>
        /// Retrieves a furniture item by its unique item ID within this category.
        /// </summary>
        /// <param name="itemId">Unique string ID of the item.</param>
        /// <returns>Matching FurnitureItemSO or null if not found.</returns>
        public FurnitureItemSO GetItemById(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || _items == null) return null;

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] != null && _items[i].ItemId == itemId)
                {
                    return _items[i];
                }
            }

            return null;
        }
    }
}
