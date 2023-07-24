/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Generic
{
    using System;
    using UnityEngine;

    /// <summary>
    /// A simple script that is used to add some visual flair by tweening transform properties with animation curves.
    /// </summary>
    public class TransformTweenLoops : MonoBehaviour
    {
        /// <summary>
        /// A stuct contains 3 animation curves.
        /// </summary>
        [Serializable]
        public struct AnimationCurve3D
        {
            [Tooltip("Active")]
            [SerializeField] private bool m_Enabled;
            [Tooltip("The Position Curve as an offset of the starting position.")]
            [SerializeField] private AnimationCurve m_X;
            [Tooltip("The Rotation Curve as an offset of the starting rotation.")]
            [SerializeField] private AnimationCurve m_Y;
            [Tooltip("The Scale Curve as an multiplier of the starting scale.")]
            [SerializeField] private AnimationCurve m_Z;

            public bool Enabled => m_Enabled;
            
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="value">The value of the animation curves.</param>
            public AnimationCurve3D(float value)
            {
                m_Enabled = false;
                m_X = new (new (0,value), new (1,value));
                m_Y = new (new (0,value), new (1,value));
                m_Z = new (new (0,value), new (1,value));
            }
            
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="startValue">The value at time 0.</param>
            /// <param name="endValue">The value at time 1.</param>
            public AnimationCurve3D(float startValue, float endValue)
            {
                m_Enabled = false;
                m_X = new (new (0,startValue), new (1,endValue));
                m_Y = new (new (0,startValue), new (1,endValue));
                m_Z = new (new (0,startValue), new (1,endValue));
            }

            /// <summary>
            /// Get the animation time by checking returning the longest one out of the curves.
            /// </summary>
            /// <returns>The animation time.</returns>
            public float GetAnimationTime()
            {
                if (m_Enabled == false) { return 0; }
                
                var maxTime = 0f;
                if (m_X.length != 0) {
                    maxTime = Mathf.Max(maxTime,m_X.keys[m_X.length - 1].time);
                }
                if (m_Y.length != 0) {
                    maxTime = Mathf.Max(maxTime,m_Y.keys[m_Y.length - 1].time);
                }
                if (m_Z.length != 0) {
                    maxTime = Mathf.Max(maxTime,m_Z.keys[m_Z.length - 1].time);
                }
                return maxTime;
            }
            
            /// <summary>
            /// Get a vector 3 evaluation at time t.
            /// </summary>
            /// <param name="t">The time at which to evaluate.</param>
            /// <returns>The resulting vector3 evaluated by all curves.</returns>
            public Vector3 Evaluate(float t)
            {
                if (m_Enabled == false) { return Vector3.zero; }
                
                return new Vector3(m_X.Evaluate(t), m_Y.Evaluate(t), m_Y.Evaluate(t));
            }
        }
        
        /// <summary>
        /// The mode in which to play the tween.
        /// </summary>
        [Serializable]
        public enum Mode
        {
            Forward,
            Loop,
            PingPong,
            PingPongLoop,
            Back
        }

        [Tooltip("The transform the tween, defaults to this transform.")]
        [SerializeField] protected Transform m_Transform;
        [Tooltip("The mode of the tween animation")]
        [SerializeField] protected Mode m_Mode;
        [Tooltip("Start the tween animation On Enable")]
        [SerializeField] protected bool m_AnimateOnEnable;
        [Tooltip("The Position Curve as an offset of the starting position.")]
        [SerializeField] protected AnimationCurve3D m_PositionOffsetCurve = new(0);
        [Tooltip("The Rotation Curve as an offset of the starting rotation.")]
        [SerializeField] protected AnimationCurve3D m_RotationOffsetCurve = new(0);
        [Tooltip("The Scale Curve as an multiplier of the starting scale.")]
        [SerializeField] protected AnimationCurve3D m_ScaleMultiplierCurve = new(1);

        private Vector3 m_StartingPosition;
        private Vector3 m_StartingRotation;
        private Vector3 m_StartingScale;
        
        protected bool m_Active;
        protected float m_TimeCount;

        /// <summary>
        /// Cache the transform starting values.
        /// </summary>
        private void Awake()
        {
            if (m_Transform == null) {
                m_Transform = transform;
            }
            
            m_StartingPosition = m_Transform.localPosition;
            m_StartingRotation = m_Transform.localRotation.eulerAngles;
            m_StartingScale = m_Transform.localScale;
        }

        /// <summary>
        /// Check if the animation should start.
        /// </summary>
        private void OnEnable()
        {
            // Make sure it is deactivated in case activated while disabled.
            m_Active = false;
            
            if (m_AnimateOnEnable) {
                StartTweenAnimation();
            }
        }

        /// <summary>
        /// Reset the values on disable.
        /// </summary>
        private void OnDisable()
        {
            m_Active = false;
            
            // Reset the transform to default.
            m_Transform.localPosition = m_StartingPosition;
            m_Transform.localRotation = Quaternion.Euler(m_StartingRotation);
            m_Transform.localScale = m_StartingScale;
        }

        /// <summary>
        /// Start the Tween animation with a specified mode.
        /// </summary>
        /// <param name="mode"></param>
        public void StartTweenAnimation(Mode mode)
        {
            m_Mode = mode;
            StartTweenAnimation();
        }

        /// <summary>
        /// Start the tween animation with the current mode.
        /// </summary>
        public void StartTweenAnimation()
        {
            m_Active = true;
            m_TimeCount = 0;

            if (m_Mode == Mode.Back) {
                Evaluate(GetAnimationTime());
            } else {
                Evaluate(0);
            }
        }

        /// <summary>
        /// Stop the tween animation.
        /// </summary>
        public void StopTweenAnimation()
        {
            m_Active = false;
        }

        /// <summary>
        /// Update the animation with the current delta time.
        /// </summary>
        private void Update()
        {
            if (m_Active == false) { return; }

            var animationTime = GetAnimationTime();
            
            
            var deltaT = m_TimeCount;
            if (m_Mode == Mode.Back) {
                deltaT = animationTime - m_TimeCount;
            }
            
            if (m_Mode == Mode.Loop) {
                deltaT = m_TimeCount % animationTime;
            }

            if (m_Mode == Mode.PingPong || m_Mode == Mode.PingPongLoop) {
                var numberOfLoops = Math.Floor(m_TimeCount / animationTime);
                var pair = ((int)numberOfLoops % 2) == 0;
                if (pair) {
                    deltaT = m_TimeCount % animationTime;
                } else {
                    deltaT = animationTime - (m_TimeCount % animationTime);
                }
            }

            Evaluate(deltaT);

            m_TimeCount += Time.deltaTime;

            if (m_Mode == Mode.Forward && m_TimeCount > animationTime) {
                Evaluate(animationTime);
                m_Active = false;
            }
            
            if (m_Mode == Mode.Back && m_TimeCount > animationTime) {
                Evaluate(0);
                m_Active = false;
            }

            if (m_Mode == Mode.PingPong && m_TimeCount > animationTime*2) {
                Evaluate(0);
                m_Active = false;
            }
        }

        /// <summary>
        /// Evaluate the transform new values at delta t.
        /// </summary>
        /// <param name="deltaT">The delta time at which to evaluate the animation curves.</param>
        public void Evaluate(float deltaT)
        {
            if (m_PositionOffsetCurve.Enabled) {
                var positionOffset = m_PositionOffsetCurve.Evaluate(deltaT);
                m_Transform.localPosition = m_StartingPosition + positionOffset;
            }

            if (m_RotationOffsetCurve.Enabled) {
                var rotationOffset = m_RotationOffsetCurve.Evaluate(deltaT);
                m_Transform.localRotation = Quaternion.Euler(m_StartingRotation + rotationOffset);
            }

            if (m_ScaleMultiplierCurve.Enabled) {
                var scaleMultiplier = m_ScaleMultiplierCurve.Evaluate(deltaT);
                m_Transform.localScale = Vector3.Scale(m_StartingScale, scaleMultiplier);
            }
        }

        /// <summary>
        /// Get the animation time by getting the longest time out of all the curves.
        /// </summary>
        /// <returns>The animation time.</returns>
        private float GetAnimationTime()
        {
            var maxTime = 0f;
            maxTime = Mathf.Max(maxTime,m_PositionOffsetCurve.GetAnimationTime());
            maxTime = Mathf.Max(maxTime,m_RotationOffsetCurve.GetAnimationTime());
            maxTime = Mathf.Max(maxTime,m_ScaleMultiplierCurve.GetAnimationTime());
            return maxTime;
        }
    }
}