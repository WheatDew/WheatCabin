/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Monitor which shows the target being selected (usually by the Assist Aim ability).
    /// </summary>
    public class AimTargetMonitor : CharacterMonitor
    {
        [Tooltip("The Target ID.")]
        [SerializeField] protected int m_TargetID;
        [Tooltip("The aim image that will move on screen space to point at the target.")]
        [SerializeField] protected Image m_AimImage;
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
        private Transform m_Target;
        private PivotOffset m_TargetPivotOffset;
        private bool m_SoftAim;
        private bool m_LockedAim;

        public int TargetID
        {
            get => m_TargetID;
            set => m_TargetID = value;
        }
        
        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (m_AimImage == null) {
                m_AimImage = GetComponent<Image>();
            }
            m_AimImage.gameObject.SetActive(false);
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

            m_SoftAim = false;
            m_LockedAim = false;
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
            if (m_TargetID != targetID) { return; }

            OnAimTargetChange(newTarget, locked);
        }

        /// <summary>
        /// When the aim target changes the UI should change the icon.
        /// </summary>
        /// <param name="newTarget">The new target.</param>
        /// <param name="locked">Is the target locked?</param>
        public void OnAimTargetChange(Transform newTarget, bool locked)
        {
            if (newTarget == null) {
                m_LockedAim = false;
                m_SoftAim = false;
                m_Target = null;
                return;
            }

            m_Target = newTarget;
            m_TargetPivotOffset = m_Target.gameObject.GetCachedComponent<PivotOffset>();
            m_SoftAim = !locked;
            m_LockedAim = locked;
        }

        /// <summary>
        /// Determine the position the image should have to point at the target.
        /// </summary>
        private void LateUpdate()
        {
            if (!m_LockedAim && !m_SoftAim) {
                m_AimImage.gameObject.SetActive(false);
                return;
            }

            if (m_LockedAim) {
                if (m_LockedAimIcon == null) {
                    m_AimImage.gameObject.SetActive(false);
                    return;
                }
                m_AimImage.gameObject.SetActive(true);
                m_AimImage.sprite = m_LockedAimIcon;
                m_AimImage.color = m_LockedAimColor;
            } else {
                if (m_SoftAimIcon == null) {
                    m_AimImage.gameObject.SetActive(false);
                    return;
                }
                m_AimImage.gameObject.SetActive(true);
                m_AimImage.sprite = m_SoftAimIcon;
                m_AimImage.color = m_SoftAimColor;
            }

            var screenPoint = m_Camera.WorldToScreenPoint(m_Target.TransformPoint(m_TargetPivotOffset != null ? m_TargetPivotOffset.Offset : Vector3.zero));
            m_AimImage.rectTransform.anchoredPosition = transform.InverseTransformPoint(screenPoint);
        }
    }
}