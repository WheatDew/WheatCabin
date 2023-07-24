/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using Opsive.Shared.Events;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The component is used to show the target being selected (usually by the CombatAimAssist ability).
    /// </summary>
    public class MultiAimTargetMonitor : CharacterMonitor
    {
        public struct TargetData
        {
            private int m_ID;
            private Image m_Image;
            private Transform m_Transform;
            private bool m_SoftAim;
            private bool m_LockedAim;
            
            public int ID => m_ID;
            public Image Image => m_Image;
            public Transform Transform => m_Transform;
            public bool SoftAim => m_SoftAim;
            public bool LockedAim => m_LockedAim;

            /// <summary>
            /// The target data constructor.
            /// </summary>
            /// <param name="id">The target id.</param>
            /// <param name="transform">The target transform.</param>
            /// <param name="softAim">Should the target be soft aimed.</param>
            /// <param name="lockedAim">Should the target be locked aimed.</param>
            /// <param name="image">The image should the lock.</param>
            public TargetData(int id,Transform transform, bool softAim, bool lockedAim, Image image)
            {
                m_ID = id;
                m_Transform = transform;
                m_SoftAim = softAim;
                m_LockedAim = lockedAim;
                m_Image = image;
            }
        }
        
        [Tooltip("The Target IDs that the Multi Aim Target will monitor.")]
        [SerializeField] protected Vector2Int m_TargetIDRange;
        [Tooltip("The aim image that will move on screen space to point at the target.")]
        [SerializeField] protected GameObject m_AimImagePrefab;
        [Tooltip("The Screen space offset for the image.")]
        [SerializeField] protected Vector2 m_ScreenSpaceOffset;
        [Tooltip("The soft aim icon.")]
        [SerializeField] protected Sprite m_SoftAimIcon;
        [Tooltip("The locked aim icon.")]
        [SerializeField] protected Sprite m_LockedAimIcon;
        [Tooltip("The default color of the soft aim icon.")]
        [SerializeField] protected Color m_SoftAimColor = Color.white;
        [Tooltip("The color of the aim icon a target locked.")]
        [SerializeField] protected Color m_LockedAimColor = Color.red;

        private Camera m_Camera;
        private RectTransform m_CanvasRectTransform;
        private Dictionary<int,TargetData> m_Targets;

        public Vector2Int TargetIDRange
        {
            get => m_TargetIDRange;
            set => m_TargetIDRange = value;
        }
        
        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_CanvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            if (m_AimImagePrefab == null) {
                Debug.Log("The Multi Aim Target Monitor is missing a prefab",gameObject);
            }

            if (m_AimImagePrefab.GetComponentInChildren<Image>() == null) {
                Debug.Log("The Multi Aim Target Monitor Image prefab is missing the Image component", gameObject);
            }
        }
        
        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<int, Transform, bool>(m_Character, "OnAimTargetChange", OnAimTargetChange);
            }

            base.OnAttachCharacter(character);

            if (m_Character == null) {
                return;
            }

            m_Camera = Shared.Camera.CameraUtility.FindCamera(m_Character);

            m_Targets = new Dictionary<int, TargetData>();
            gameObject.SetActive(CanShowUI());

            EventHandler.RegisterEvent<int, Transform, bool>(m_Character, "OnAimTargetChange", OnAimTargetChange);
        }

        /// <summary>
        /// When the aim target changes the UI should change the icon.
        /// </summary>
        /// <param name="targetID">The target ID.</param>
        /// <param name="newTarget">The new target.</param>
        /// <param name="locked">Is the target locked?</param>
        public void OnAimTargetChange(int targetID, Transform newTarget, bool locked)
        {
            var softAim = !locked;
            var lockedAim = locked;
            if (newTarget == null) {
                softAim = false;
                lockedAim = false;
            }

            Image image;
            
            if (m_Targets.TryGetValue(targetID, out var targetData)) {
                image = targetData.Image;
            } else {
                image = Instantiate(m_AimImagePrefab, transform).GetComponentInChildren<Image>();
            }

            m_Targets[targetID] = new TargetData(targetID, newTarget, softAim, lockedAim, image);
        }
        

        /// <summary>
        /// Determine the position the image should have to point at the target.
        /// </summary>
        private void LateUpdate()
        {
            foreach (var targetKeyValuePair in m_Targets) {
                var targetData = targetKeyValuePair.Value;

                var lockedAim = targetData.LockedAim;
                var softAim = targetData.SoftAim;
                var image = targetData.Image;
                var targetTransform = targetData.Transform;
                
                if (lockedAim == false && targetData.SoftAim == false) {
                    image.gameObject.SetActive(false);
                    return;
                }

                if (lockedAim) {
                    if (m_LockedAimIcon == null) {
                        image.gameObject.SetActive(false);
                        return;
                    }
                    image.gameObject.SetActive(true);
                    image.sprite = m_LockedAimIcon;
                    image.color = m_LockedAimColor;
                
                } else {
                    if (m_SoftAimIcon == null) {
                        image.gameObject.SetActive(false);
                        return;
                    }
                    image.gameObject.SetActive(true);
                    image.sprite = m_SoftAimIcon;
                    image.color = m_SoftAimColor;
                }

                var screenPoint = m_Camera.WorldToScreenPoint(targetTransform.position);
                var proportionalPosition = new Vector2(screenPoint.x, screenPoint.y) - (m_CanvasRectTransform.sizeDelta / 2f);
                image.rectTransform.anchoredPosition = proportionalPosition - m_ScreenSpaceOffset;
            }
        }
    }
}