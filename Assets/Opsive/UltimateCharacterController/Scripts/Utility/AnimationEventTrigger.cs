/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Utility
{
    using Opsive.Shared.Game;
    using UnityEngine;
    using System;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// Storage class for determining if an event is triggered based on an animation event or time.
    /// </summary>
    [System.Serializable]
    public class AnimationEventTrigger : ICloneable
    {
        [Tooltip("Is the event triggered with a Unity animation event?")]
        [SerializeField] protected bool m_WaitForAnimationEvent;
        [Tooltip("The amount of time it takes to trigger the event if not using an animation event.")]
        [SerializeField] protected float m_Duration;

        public bool WaitForAnimationEvent
        {
            get => m_WaitForAnimationEvent; 
            set {
                if (m_WaitForAnimationEvent == value) {
                    return;
                }
                m_WaitForAnimationEvent = value;
                // Schedule the event again in case wait for animation event changes.
                if (m_IsWaiting && m_ScheduledEvent == null && !m_WaitForAnimationEvent) {
                    m_ScheduledEvent = Scheduler.ScheduleFixed(m_Duration, InvokeScheduledEvent);
                }
            }
        }
        public float Duration { get => m_Duration; set => m_Duration = value; }

        protected ScheduledEventBase m_ScheduledEvent;
        protected bool m_IsWaiting;
        protected event Action OnEvent;
        protected event Action OnScheduledEvent;
        protected event Action OnAnimationEvent;

        public bool IsWaiting => m_IsWaiting;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AnimationEventTrigger() { }

        /// <summary>
        /// Two parameter constructor for AnimationEventTrigger.
        /// </summary>
        /// <param name="waitForAnimationEvent">Is the event triggered with a Unity animation event?</param>
        /// <param name="duration">The amount of time it takes to trigger the event if not using an animation event.</param>
        public AnimationEventTrigger(bool waitForAnimationEvent, float duration)
        {
            m_WaitForAnimationEvent = waitForAnimationEvent;
            m_Duration = duration;
        }

        /// <summary>
        /// Registers or unregisteres for an animation event update.
        /// </summary>
        /// <param name="register">Should the event be registered?</param>
        /// <param name="target">The object that is registering or unregistering the for the event.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="action">The action that should be performed when the event is executed.</param>
        public virtual void RegisterUnregisterAnimationEvent(bool register, object target, string eventName, Action action)
        {
            if (register) {
                RegisterAnimationEvent(target, eventName);
                OnEvent += action;
            } else {
                UnregisterAnimationEvent(target, eventName);
                OnEvent -= action;
            }
        }

        /// <summary>
        /// Registeres for the animation event on the target.
        /// </summary>
        /// <param name="target">The object that is registering the for the event.</param>
        /// <param name="eventName">The name of the event.</param>
        public virtual void RegisterAnimationEvent(object target, string eventName)
        {
            if (target == null) {
                EventHandler.RegisterEvent(eventName, InvokeAnimationEvent);
            } else {
                EventHandler.RegisterEvent(target, eventName, InvokeAnimationEvent);
            }
        }

        /// <summary>
        /// Unregisteres for the animation event on the target.
        /// </summary>
        /// <param name="target">The object that is unregistering the for the event.</param>
        /// <param name="eventName">The name of the event.</param>
        public virtual void UnregisterAnimationEvent(object target, string eventName)
        {
            if (target == null) {
                EventHandler.UnregisterEvent(eventName, InvokeAnimationEvent);
            } else {
                EventHandler.UnregisterEvent(target, eventName, InvokeAnimationEvent);
            }
        }

        /// <summary>
        /// Invokes the animation event.
        /// </summary>
        protected virtual void InvokeAnimationEvent()
        {
            if (!m_WaitForAnimationEvent) {
                return;
            }
            OnAnimationEvent?.Invoke();
            NotifyEventListeners();
        }

        /// <summary>
        /// Starts to wait for the event. This can be through the ScheduledEvent or the animation event.
        /// </summary>
        /// <param name="reset">Should the event be reset?</param>
        /// <returns>The resulting scheduled event.</returns>
        public virtual ScheduledEventBase WaitForEvent(bool reset = false)
        {
            // Return the current event if the event is active.
            if (!reset && m_IsWaiting) {
                return m_ScheduledEvent;
            }

            m_IsWaiting = true;
            if (m_ScheduledEvent != null) {
                Scheduler.Cancel(m_ScheduledEvent);
                m_ScheduledEvent = null;
            }
            m_ScheduledEvent = Scheduler.ScheduleFixed(m_Duration, InvokeScheduledEvent);

            return m_ScheduledEvent;
        }

        /// <summary>
        /// Invokes the scheduled event.
        /// </summary>
        public virtual void InvokeScheduledEvent()
        {
            m_ScheduledEvent = null;
            if (m_WaitForAnimationEvent) {
                return;
            }

            OnScheduledEvent?.Invoke();
            NotifyEventListeners();
        }

        /// <summary>
        /// Sends the event notification.
        /// </summary>
        protected virtual void NotifyEventListeners()
        {
            if (!m_IsWaiting) {
                return;
            }

            m_IsWaiting = false;
            OnEvent?.Invoke();
        }

        /// <summary>
        /// Stops waiting for the event.
        /// </summary>
        /// <returns>The ScheduledEvent.</returns>
        public virtual ScheduledEventBase CancelWaitForEvent()
        {
            m_IsWaiting = false;
            var scheduledEvent = m_ScheduledEvent;
            if (m_ScheduledEvent != null) {
                Scheduler.Cancel(m_ScheduledEvent);
                m_ScheduledEvent = null;
            }

            return scheduledEvent;
        }

        /// <summary>
        /// Copies the settings from the other trigger.
        /// </summary>
        /// <param name="other">The settings that should be copied.</param>
        public void CopyFrom(AnimationEventTrigger other)
        {
            m_WaitForAnimationEvent = other.WaitForAnimationEvent;
            m_Duration = other.Duration;
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>The cloned object.</returns>
        public virtual object Clone()
        {
            var clone = new AnimationEventTrigger();
            clone.WaitForAnimationEvent = m_WaitForAnimationEvent;
            clone.Duration = m_Duration;
            return clone;
        }
    }

    /// <summary>
    /// Determines if an animation event should be triggered for a specified slot.
    /// </summary>
    [System.Serializable]
    public class AnimationSlotEventTrigger : AnimationEventTrigger
    {
        [Tooltip("Specifies if the item should wait for the specific slot animation event.")]
        [SerializeField] private bool m_WaitForSlotEvent;

        public bool WaitForSlotEvent { get { return m_WaitForSlotEvent; } set { m_WaitForSlotEvent = value; } }

        private int m_SlotID = -1;
        public event Action<int> OnSlotEvent;
        public event Action<int> OnEventIndexed;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AnimationSlotEventTrigger() { }

        /// <summary>
        /// Two parameter constructor for AnimationSlotEventTrigger.
        /// </summary>
        /// <param name="waitForAnimationEvent">Is the event triggered with a Unity animation event?</param>
        /// <param name="duration">The amount of time it takes to trigger the event if not using an animation event.</param>
        public AnimationSlotEventTrigger(bool waitForAnimationEvent, float duration) : base(waitForAnimationEvent, duration) { }

        /// <summary>
        /// Registers or unregistered for the animation and slot event.
        /// </summary>
        /// <param name="register">Should the event be registered?</param>
        /// <param name="target">The object that is registering or unregistering the for the event.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="slotID">The ID of the slot that is being registered or unregistered.</param>
        /// <param name="action">The action that should be performed when the event is executed.</param>
        public virtual void RegisterUnregisterEvent(bool register, object target, string eventName, int slotID, Action action)
        {
            m_SlotID = slotID;
            if (register) {
                RegisterAnimationAndSlotEvent(target, eventName, slotID);
                OnEvent += action;
            } else {
                UnregisterAnimationAndSlotEvent(target, eventName, slotID);
                OnEvent -= action;
            }
        }
        
        /// <summary>
        /// Registers or unregistered for the animation and slot event.
        /// </summary>
        /// <param name="register">Should the event be registered?</param>
        /// <param name="target">The object that is registering or unregistering the for the event.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="slotID">The ID of the slot that is being registered or unregistered.</param>
        /// <param name="action">The action that should be performed when the event is executed.</param>
        public virtual void RegisterUnregisterEvent(bool register, object target, string eventName, int slotID, Action<int> action)
        {
            m_SlotID = slotID;
            if (register) {
                RegisterAnimationAndSlotEvent(target, eventName, slotID);
                OnEventIndexed += action;
            } else {
                UnregisterAnimationAndSlotEvent(target, eventName, slotID);
                OnEventIndexed -= action;
            }
        }

        /// <summary>
        /// Registers for the animation and slot event.
        /// </summary>
        /// <param name="target">The object that is registering for the event.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="slotID">The ID of the slot that is being registered.</param>
        public void RegisterAnimationAndSlotEvent(object target, string eventName, int slotID)
        {
            RegisterAnimationEvent(target, eventName);
            RegisterSlotEvent(target, string.Format("{0}Slot{1}", eventName, slotID));
        }

        /// <summary>
        /// Unregisters for the animation and slot event.
        /// </summary>
        /// <param name="target">The object that is unregistering for the event.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="slotID">The ID of the slot that is being unregistered.</param>
        public void UnregisterAnimationAndSlotEvent(object target, string eventName, int slotID)
        {
            UnregisterAnimationEvent(target, eventName);
            UnregisterSlotEvent(target, string.Format("{0}Slot{1}", eventName, slotID));
        }

        /// <summary>
        /// Registers for the slot event.
        /// </summary>
        /// <param name="target">The object that is registering for the event.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="slotID">The ID of the slot that is being registered.</param>
        private void RegisterSlotEvent(object target, string eventName)
        {
            if (target == null) {
                EventHandler.RegisterEvent(eventName, InvokeSlotEvent);
            } else {
                EventHandler.RegisterEvent(target, eventName, InvokeSlotEvent);
            }
        }

        /// <summary>
        /// Unregisters for the slot event.
        /// </summary>
        /// <param name="target">The object that is unregistering for the event.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="slotID">The ID of the slot that is being unregistered.</param>
        private void UnregisterSlotEvent(object target, string eventName)
        {
            if (target == null) {
                EventHandler.UnregisterEvent(eventName, InvokeSlotEvent);
            } else {
                EventHandler.UnregisterEvent(target, eventName, InvokeSlotEvent);
            }
        }

        /// <summary>
        /// Invokes the slot event.
        /// </summary>
        public virtual void InvokeSlotEvent()
        {
            // Don't invoke the event if it has already been invoked.
            if (m_WaitForSlotEvent == false) {
                return;
            }
            OnSlotEvent?.Invoke(m_SlotID);
            NotifyEventListeners();
        }
        
        /// <summary>
        /// Sends the event notification.
        /// </summary>
        protected override void NotifyEventListeners()
        {
            if (!m_IsWaiting) {
                return;
            }
            
            OnEventIndexed?.Invoke(m_SlotID);
            base.NotifyEventListeners();
        }

        /// <summary>
        /// Copies the settings from the other trigger.
        /// </summary>
        /// <param name="other">The settings that should be copied.</param>
        public void CopyFrom(AnimationSlotEventTrigger other)
        {
            base.CopyFrom(other);

            m_WaitForSlotEvent = other.WaitForSlotEvent;
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>The cloned object.</returns>
        public override object Clone()
        {
            var clone = new AnimationSlotEventTrigger();
            clone.WaitForAnimationEvent = m_WaitForAnimationEvent;
            clone.Duration = m_Duration;
            clone.m_SlotID = m_SlotID;
            return clone;
        }
    }
}