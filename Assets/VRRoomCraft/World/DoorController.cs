using System;
using System.Collections;
using UnityEngine;
using VRRoomCraft.Data.Save;

namespace VRRoomCraft.World
{
    /// <summary>
    /// Controls interactive door and cabinet animations in the VR RoomCraft apartment.
    /// Handles smooth hinge rotation, state tracking, optional auto-closing, and raises interaction events.
    /// </summary>
    [DisallowMultipleComponent]
    public class DoorController : MonoBehaviour, ISaveable
    {
        [Header("Door Identification")]
        [Tooltip("Unique string identifier for this door (used for Save/Load serialization).")]
        [SerializeField] private string _doorId;

        [Header("Hinge & Motion Settings")]
        [Tooltip("Transform pivot where rotation occurs. If unassigned, defaults to this GameObject's Transform.")]
        [SerializeField] private Transform _pivotTransform;

        [Tooltip("Opening rotation angle in degrees around the Y axis (default 90).")]
        [SerializeField] private float _openAngle = 90f;

        [Tooltip("Rotation animation speed modifier.")]
        [SerializeField] private float _openSpeed = 4f;

        [Header("Door Behavior")]
        [Tooltip("Initial state when scene loads.")]
        [SerializeField] private bool _startOpen = false;

        [Tooltip("Should the door automatically close after a delay?")]
        [SerializeField] private bool _autoClose = false;

        [Tooltip("Delay in seconds before auto-closing if autoClose is enabled.")]
        [SerializeField] private float _autoCloseDelay = 5f;

        [Header("Runtime State (Read-Only)")]
        [SerializeField] private bool _isOpen = false;
        [SerializeField] private bool _isAnimating = false;

        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private Coroutine _animationCoroutine;
        private Coroutine _autoCloseCoroutine;

        // --- C# Events ---

        /// <summary>
        /// Raised when the door fully opens.
        /// </summary>
        public event Action<DoorController> OnDoorOpened;

        /// <summary>
        /// Raised when the door fully closes.
        /// </summary>
        public event Action<DoorController> OnDoorClosed;

        /// <summary>
        /// Raised whenever the door state changes (opened or closed).
        /// Signature: (DoorController door, bool isOpen)
        /// </summary>
        public event Action<DoorController, bool> OnDoorStateChanged;

        // --- Public Read-Only Properties ---

        /// <summary>
        /// Gets the unique door identifier.
        /// </summary>
        public string DoorId => _doorId;

        /// <summary>
        /// Gets the SaveId contract for ISaveable.
        /// </summary>
        public string SaveId => _doorId;

        /// <summary>
        /// Gets whether the door is currently open.
        /// </summary>
        public bool IsOpenState => _isOpen;

        /// <summary>
        /// Gets whether the door is currently animating.
        /// </summary>
        public bool IsAnimating => _isAnimating;

        /// <summary>
        /// Gets the target hinge pivot transform.
        /// </summary>
        public Transform Pivot => _pivotTransform != null ? _pivotTransform : transform;

        // --- Unity Lifecycle ---

        private void Awake()
        {
            if (string.IsNullOrEmpty(_doorId))
            {
                _doorId = gameObject.name;
            }

            if (_pivotTransform == null)
            {
                _pivotTransform = transform;
            }

            // Cache closed and open local rotations based on pivot initial rotation
            _closedRotation = _pivotTransform.localRotation;
            _openRotation = _closedRotation * Quaternion.Euler(0f, _openAngle, 0f);
        }

        private void Start()
        {
            if (_startOpen)
            {
                _isOpen = true;
                _pivotTransform.localRotation = _openRotation;
            }
        }

        // --- Public Control APIs ---

        /// <summary>
        /// Toggles the door state (opens if closed, closes if open).
        /// </summary>
        public void ToggleDoor()
        {
            if (_isOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }

        /// <summary>
        /// Smoothly opens the door.
        /// </summary>
        public void OpenDoor()
        {
            if (_isOpen && !_isAnimating) return;

            StopExistingCoroutines();
            _isOpen = true;
            _animationCoroutine = StartCoroutine(AnimateDoorRotation(_openRotation, OnOpenedComplete));
        }

        /// <summary>
        /// Smoothly closes the door.
        /// </summary>
        public void CloseDoor()
        {
            if (!_isOpen && !_isAnimating) return;

            StopExistingCoroutines();
            _isOpen = false;
            _animationCoroutine = StartCoroutine(AnimateDoorRotation(_closedRotation, OnClosedComplete));
        }

        /// <summary>
        /// Checks whether the door is currently in the open state.
        /// </summary>
        public bool IsOpen()
        {
            return _isOpen;
        }

        // --- Coroutine Animation Logic ---

        private IEnumerator AnimateDoorRotation(Quaternion targetRotation, Action onComplete)
        {
            _isAnimating = true;

            while (Quaternion.Angle(_pivotTransform.localRotation, targetRotation) > 0.1f)
            {
                _pivotTransform.localRotation = Quaternion.RotateTowards(
                    _pivotTransform.localRotation,
                    targetRotation,
                    _openSpeed * 100f * Time.deltaTime
                );
                yield return null;
            }

            _pivotTransform.localRotation = targetRotation;
            _isAnimating = false;
            onComplete?.Invoke();
        }

        private void OnOpenedComplete()
        {
            OnDoorOpened?.Invoke(this);
            OnDoorStateChanged?.Invoke(this, true);

            if (_autoClose)
            {
                _autoCloseCoroutine = StartCoroutine(AutoCloseRoutine());
            }
        }

        private void OnClosedComplete()
        {
            OnDoorClosed?.Invoke(this);
            OnDoorStateChanged?.Invoke(this, false);
        }

        private IEnumerator AutoCloseRoutine()
        {
            yield return new WaitForSeconds(_autoCloseDelay);
            CloseDoor();
        }

        private void StopExistingCoroutines()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            if (_autoCloseCoroutine != null)
            {
                StopCoroutine(_autoCloseCoroutine);
                _autoCloseCoroutine = null;
            }
        }

        // --- ISaveable Contract ---

        public object GetSaveState()
        {
            return _isOpen;
        }

        public void LoadSaveState(object state)
        {
            if (state is bool isOpenSaved)
            {
                _isOpen = isOpenSaved;
                _pivotTransform.localRotation = _isOpen ? _openRotation : _closedRotation;
            }
        }
    }
}
