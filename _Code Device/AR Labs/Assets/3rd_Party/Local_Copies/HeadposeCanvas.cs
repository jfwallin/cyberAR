// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Developer Agreement, located
// here: https://auth.magicleap.com/terms/developer
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// Script used to position this Canvas object directly in front of the user by
    /// using lerp functionality to give it a smooth look. Components on the canvas
    /// should function normally.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/HeadposeCanvas")]
    [RequireComponent(typeof(Canvas))]
    public class HeadposeCanvas : MonoBehaviour
    {
        [Tooltip("The forwards distance from the camera that this object should be placed.")]
        public float CanvasDistanceForwards = 1.5f;

        [Tooltip("The upwards distance from the camera that this object should be placed.")]
        public float CanvasDistanceUpwards = 0.0f;

        [Tooltip("The speed at which this object changes its position.")]
        public float PositionLerpSpeed = 5f;

        [Tooltip("The speed at which this object changes its rotation.")]
        public float RotationLerpSpeed = 5f;

        //LOCAL EDITS TO ML SCRIPT
        [Tooltip("Whether to keep the canvas's up vector matching World Space up.")]
        public bool keepVertical = false;

        // The canvas that is attached to this object.
        private Canvas _canvas;

        // The camera this object will be in front of.
        private Camera _camera;

        /// <summary>
        /// Initializes variables and verifies that necessary components exist.
        /// </summary>
        void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _camera = _canvas.worldCamera;

            // Disable this component if
            // it failed to initialize properly.
            if (_canvas == null)
            {
                Debug.LogError("Error: HeadposeCanvas._canvas is not set, disabling script.");
                enabled = false;
                return;
            }
            if (_camera == null)
            {
                Debug.LogError("Error: HeadposeCanvas._camera is not set, disabling script.");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Update position and rotation of this canvas object to face the camera using lerp for smoothness.
        /// </summary>
        void Update()
        {
            // Move the object CanvasDistance units in front of the camera.
            float posSpeed = Time.deltaTime * PositionLerpSpeed;
            Vector3 posTo = _camera.transform.position + (_camera.transform.forward * CanvasDistanceForwards) + (_camera.transform.up * CanvasDistanceUpwards);
            transform.position = Vector3.SlerpUnclamped(transform.position, posTo, posSpeed);

            // Rotate the object to face the camera.
            float rotSpeed = Time.deltaTime * RotationLerpSpeed;

            //
            //LOCAL EDITS TO ML SCRIPT
            Vector3 lookDirection = Vector3.zero;
            //Whether to keep with the vertical or not
            if(keepVertical)
            {
                lookDirection = Vector3.ProjectOnPlane(transform.position - _camera.transform.position, Vector3.up);
            }
            else
            {
                lookDirection = transform.position - _camera.transform.position;
            }
            //
            //

            Quaternion rotTo = Quaternion.LookRotation(lookDirection);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, rotTo, rotSpeed);
        }
    }
}
