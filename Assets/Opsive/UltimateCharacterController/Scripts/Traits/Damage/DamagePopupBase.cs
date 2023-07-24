/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Traits.Damage
{
    using Opsive.Shared.Camera;
    using Opsive.Shared.Game;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Base class for the default popup that appears when an object is damaged/healed.
    /// </summary>
    public abstract class DamagePopupBase : MonoBehaviour, IDamagePopup
    {
        [Tooltip("The lifetime before being returned to the pool.")]
        [SerializeField] protected float m_DefaultLifeTime = 1;
        [Tooltip("The ofset of the popup relative to the damage position.")]
        [SerializeField] protected Vector3 m_Offset = new Vector3(0, 1, 0);
        [Tooltip("The string format of the displayed damage amount.")]
        [SerializeField] protected string m_Format = "#.##";
        [Tooltip("A reference to the Animator which shows the popup. This can be null.")]
        [SerializeField] protected Animator m_Animator;
        [Tooltip("A reference to the Animator which shows the popup. This can be null.")]
        [SerializeField] protected string m_OpenAnimationName = "Open";

        private Transform m_Transform;
        protected Camera m_Camera;
        protected Vector3 m_Position;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
            if (m_Animator == null) {
                m_Animator = GetComponent<Animator>();
            }
        }

        /// <summary>
        /// Opens the popup.
        /// </summary>
        public void Open()
        {
            if (m_Camera == null) {
                m_Camera = Camera.main;
            }
            if (gameObject.activeInHierarchy) {
                if (m_Animator != null) {
                    StartCoroutine(AnimateOpen());
                } else {
                    StartCoroutine(PopFadeInOut());
                }
            } else {
                ObjectPool.Destroy(gameObject);
            }
        }

        /// <summary>
        /// Opens the popup at the specified position.
        /// </summary>
        /// <param name="position">The position that the popup should open at.</param>
        public void Open(Vector3 position)
        {
            m_Position = position;
            Open();
        }

        /// <summary>
        /// Opens the popup with the specified DamageData.
        /// </summary>
        /// <param name="damageData">Specifies the damage location/amount.</param>
        public void Open(DamageData damageData)
        {
            if (m_Camera == null && damageData.DamageSource != null) {
                m_Camera = CameraUtility.FindCamera(damageData.DamageSource.SourceOwner);
            }
            Open(damageData.Position, damageData.Amount.ToString(m_Format));
        }

        /// <summary>
        /// Opens the popup at the specified position with the specified amount.
        /// </summary>
        /// <param name="position">The position that the popup should open at.</param>
        /// <param name="amount">The amount of damage dealt.</param>
        public void Open(Vector3 position, float amount)
        {
            Open(position, amount.ToString(m_Format));
        }

        /// <summary>
        /// Opens the popup at the specified position with the specified text.
        /// </summary>
        /// <param name="position">The position that the popup should open at.</param>
        /// <param name="text">The text that the popup should show.</param>
        public virtual void Open(Vector3 position, string text)
        {
            SetText(text);
            Open(position);
        }

        /// <summary>
        /// Specifies the text that the popup should show.
        /// </summary>
        /// <param name="text">The text that the popup should show.</param>
        public abstract void SetText(string text);

        /// <summary>
        /// Pop animation coroutine.
        /// </summary>
        /// <returns>The IEnumerator.</returns>
        protected virtual IEnumerator AnimateOpen()
        {
            m_Animator.Play(m_OpenAnimationName);

            yield return m_Animator.GetCurrentAnimatorClipInfo(0).Length;

            ObjectPool.Destroy(gameObject);
        }

        /// <summary>
        /// Fades the popup in and out.
        /// </summary>
        /// <returns>The IEnumerator.</returns>
        protected virtual IEnumerator PopFadeInOut()
        {
            var scaleFactor = 1 / m_DefaultLifeTime;
            var thirdLifeTime = m_DefaultLifeTime / 3f;
            var time = 0f;

            m_Transform.localScale = Vector3.zero;
            while (time < thirdLifeTime) {
                time += Time.deltaTime;
                m_Transform.localScale = time * 3 * scaleFactor * Vector3.one;
                yield return null;
            }

            yield return new WaitForSeconds(thirdLifeTime);

            while (time > 0) {
                time -= Time.deltaTime;
                m_Transform.localScale = time * 3 * scaleFactor * Vector3.one;
                yield return null;
            }

            ObjectPool.Destroy(gameObject);
        }

        /// <summary>
        /// Updates the popup position.
        /// </summary>
        protected virtual void Update()
        {
            m_Transform.position = m_Camera.WorldToScreenPoint(m_Position + m_Offset);
        }
    }
}