/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// A button that invokes click multiple times while hold down (On pointer down ONLY).
    /// </summary>
    public class RepeatButton : Button
    {
        [Tooltip("The initial delay before it starts clicking multiple times (negative values prevent repeat).")]
        [SerializeField] protected float m_InitialDelay;
        [Tooltip("The frequency of the clicks over time. The higher the frequency to more clicks per second.")]
        [SerializeField] protected AnimationCurve m_PressPerSecond = new AnimationCurve(new[] { new Keyframe(0, 5), new Keyframe(1.5f, 40) });

        public float InitialDelay { get => m_InitialDelay; set => m_InitialDelay = value; }
        public AnimationCurve PressPerSecond { get => m_PressPerSecond; set => m_PressPerSecond = value; }

        protected bool m_HoldingDown;
        protected float m_FirstPressTime;
        protected float m_LastPressTime;

        /// <summary>
        /// Check if the the button is being hold down and if it is time to press.
        /// </summary>
        private void Update()
        {
            if (m_HoldingDown == false) { return; }

            // Megative values for initial delay prevents repeating.
            if (m_InitialDelay < 0) { return; }

            // Wait for the initial delay.
            var time = Time.time - m_FirstPressTime - m_InitialDelay;
            if (time < 0) { return; }

            var frequency = m_PressPerSecond.Evaluate(time);
            // Prevent values below a certain threshold.
            frequency = Mathf.Max(frequency, 0.01f);

            var period = 1 / frequency;

            if (Time.time > period + m_LastPressTime) {
                Press();
            }
        }

        /// <summary>
        /// On Pointer down, start the hold.
        /// </summary>
        /// <param name="eventData">The pointer event data.</param>
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            // Holding down.
            if (m_HoldingDown == false) {
                // First time holding down
                m_FirstPressTime = Time.time;
                m_HoldingDown = true;
            }
        }

        /// <summary>
        /// The button is pressed.
        /// </summary>
        public virtual void Press()
        {
            if (!IsActive() || !IsInteractable()) {
                return;
            }

            m_LastPressTime = Time.time;

            UISystemProfilerApi.AddMarker("Button.onClick", this);
            onClick.Invoke();
        }

        /// <summary>
        /// On pointer click.
        /// </summary>
        /// <param name="eventData">The pointer event data.</param>
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) {
                return;
            }

            Press();
        }

        /// <summary>
        /// On submit.
        /// </summary>
        /// <param name="eventData">The pointer event data.</param>
        public override void OnSubmit(BaseEventData eventData)
        {
            Press();

            // If disabled during the press don't run the coroutine.
            if (!IsActive() || !IsInteractable()) {
                return;
            }

            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());
        }

        /// <summary>
        /// On finish submit.
        /// </summary>
        /// <returns>The IEnumerator.</returns>
        protected virtual IEnumerator OnFinishSubmit()
        {
            var fadeTime = colors.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime) {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            DoStateTransition(currentSelectionState, false);
        }

        /// <summary>
        /// On Pointer Exit.
        /// </summary>
        /// <param name="eventData">The pointer event data.</param>
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            m_HoldingDown = false;
        }

        /// <summary>
        /// On pointer up.
        /// </summary>
        /// <param name="eventData">The pointer event data.</param>
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            m_HoldingDown = false;
        }
    }
}