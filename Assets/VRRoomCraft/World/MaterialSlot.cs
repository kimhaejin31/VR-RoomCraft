using System;
using UnityEngine;
using VRRoomCraft.Data.Materials;
using VRRoomCraft.Data.Save;

namespace VRRoomCraft.World
{
    /// <summary>
    /// Represents a customizable surface renderer target in the apartment scene (e.g., Living Room Wall, Kitchen Floor, Cabinet Front).
    /// Handles applying Unity Materials, UV tiling, texture offsets, and raising surface change events.
    /// </summary>
    [DisallowMultipleComponent]
    public class MaterialSlot : MonoBehaviour, ISaveable
    {
        [Header("Slot Configuration")]
        [Tooltip("Unique string identifier for this surface slot (used for Save/Load serialization).")]
        [SerializeField] private string _slotId;

        [Tooltip("Surface target type allowed on this renderer slot.")]
        [SerializeField] private SurfaceType _surfaceType = SurfaceType.None;

        [Tooltip("Target Renderer component. If unassigned, automatically fetches Renderer from this GameObject.")]
        [SerializeField] private Renderer _targetRenderer;

        [Tooltip("Index of the material in multi-material renderers (default 0).")]
        [SerializeField] private int _materialIndex = 0;

        [Tooltip("Optional initial material asset applied on scene start.")]
        [SerializeField] private MaterialItemSO _defaultMaterial;

        [Header("Runtime State (Read-Only)")]
        [Tooltip("Currently active material item asset.")]
        [SerializeField] private MaterialItemSO _currentMaterial;

        // --- C# Events ---

        /// <summary>
        /// Raised whenever the material on this surface slot changes or is reset.
        /// Signature: (MaterialSlot slot, MaterialItemSO newMaterial)
        /// </summary>
        public event Action<MaterialSlot, MaterialItemSO> OnMaterialChanged;

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
        /// Gets the surface type enum for this slot.
        /// </summary>
        public SurfaceType SurfaceType => _surfaceType;

        /// <summary>
        /// Gets the targeted Renderer component.
        /// </summary>
        public Renderer TargetRenderer => _targetRenderer;

        /// <summary>
        /// Gets the material array index for multi-material sub-meshes.
        /// </summary>
        public int MaterialIndex => _materialIndex;

        /// <summary>
        /// Gets the default material asset assigned to this slot.
        /// </summary>
        public MaterialItemSO DefaultMaterial => _defaultMaterial;

        /// <summary>
        /// Gets the currently active MaterialItemSO asset.
        /// </summary>
        public MaterialItemSO CurrentMaterial => _currentMaterial;

        // --- Unity Lifecycle ---

        private void Awake()
        {
            if (string.IsNullOrEmpty(_slotId))
            {
                _slotId = gameObject.name;
            }

            if (_targetRenderer == null)
            {
                _targetRenderer = GetComponent<Renderer>();
            }

            if (_targetRenderer == null)
            {
                Debug.LogError($"[MaterialSlot {_slotId}] No Renderer component found on {gameObject.name}!");
            }
        }

        private void Start()
        {
            if (_defaultMaterial != null && _currentMaterial == null)
            {
                SetMaterial(_defaultMaterial);
            }
        }

        // --- Public Control APIs ---

        /// <summary>
        /// Applies a new MaterialItemSO to the target Renderer.
        /// Sets material, updates texture tiling & UV offsets, and dispatches change event.
        /// </summary>
        /// <param name="item">MaterialItemSO asset to apply.</param>
        /// <returns>True if material swap succeeded, false otherwise.</returns>
        public bool SetMaterial(MaterialItemSO item)
        {
            if (item == null)
            {
                Debug.LogWarning($"[MaterialSlot {_slotId}] Cannot set null MaterialItemSO.");
                return false;
            }

            if (_surfaceType != SurfaceType.None && item.SurfaceType != _surfaceType)
            {
                Debug.LogWarning($"[MaterialSlot {_slotId}] SurfaceType mismatch! Slot expects {_surfaceType}, but item is {item.SurfaceType}.");
                return false;
            }

            if (item.Material == null)
            {
                Debug.LogError($"[MaterialSlot {_slotId}] MaterialItem '{item.DisplayName}' has no assigned UnityEngine.Material!");
                return false;
            }

            if (_targetRenderer == null)
            {
                Debug.LogError($"[MaterialSlot {_slotId}] Cannot apply material because TargetRenderer is missing!");
                return false;
            }

            // Fetch materials array from renderer
            Material[] sharedMats = _targetRenderer.sharedMaterials;
            if (_materialIndex < 0 || _materialIndex >= sharedMats.Length)
            {
                Debug.LogError($"[MaterialSlot {_slotId}] MaterialIndex {_materialIndex} is out of bounds for Renderer with {sharedMats.Length} materials.");
                return false;
            }

            // Assign new material asset to target index
            sharedMats[_materialIndex] = item.Material;
            _targetRenderer.sharedMaterials = sharedMats;

            // Apply UV Tiling and Offset parameters
            Material activeMat = _targetRenderer.materials[_materialIndex];
            if (activeMat.HasProperty("_MainTex"))
            {
                activeMat.SetTextureScale("_MainTex", item.TextureTiling);
                activeMat.SetTextureOffset("_MainTex", item.TextureOffset);
            }
            else if (activeMat.HasProperty("_BaseMap")) // Unity URP / HDRP Standard Shader Property
            {
                activeMat.SetTextureScale("_BaseMap", item.TextureTiling);
                activeMat.SetTextureOffset("_BaseMap", item.TextureOffset);
            }

            // Update state & dispatch event
            _currentMaterial = item;
            OnMaterialChanged?.Invoke(this, _currentMaterial);
            return true;
        }

        /// <summary>
        /// Resets the slot to its default configured material.
        /// </summary>
        public void ResetToDefault()
        {
            if (_defaultMaterial != null)
            {
                SetMaterial(_defaultMaterial);
            }
        }

        /// <summary>
        /// Gets the active MaterialItemSO asset applied to this surface.
        /// </summary>
        /// <returns>Active MaterialItemSO or null.</returns>
        public MaterialItemSO GetCurrentMaterial()
        {
            return _currentMaterial;
        }

        /// <summary>
        /// Checks whether a material is actively applied to this surface slot.
        /// </summary>
        /// <returns>True if material exists, false otherwise.</returns>
        public bool HasMaterial()
        {
            return _currentMaterial != null;
        }

        // --- ISaveable Contract ---

        public object GetSaveState()
        {
            return _currentMaterial != null ? _currentMaterial.MaterialId : string.Empty;
        }

        public void LoadSaveState(object state)
        {
            // Handled via MaterialManager during state load
        }
    }
}
