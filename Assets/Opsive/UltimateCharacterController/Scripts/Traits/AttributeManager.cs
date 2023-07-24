/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Traits
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// An Attribute can be used to describe a set of values which change over time. Examples include health, shield, stamina, hunger, etc.
    /// </summary>
    [System.Serializable]
    public class Attribute : StateObject
    {
        /// <summary>
        /// Describes how the attribute should update the value.
        /// </summary>
        public enum AutoUpdateValue
        {
            None,       // Do not automatically update the value.
            Decrease,   // Decreases the value to the min value.
            Increase    // Increases the value to the max value.
        }

        [Tooltip("The name of the attribute.")]
        [SerializeField] protected string m_Name;
        [Tooltip("The minimum value of the attribute.")]
        [SerializeField] protected float m_MinValue;
        [Tooltip("The maximum value of the attribute.")]
        [SerializeField] protected float m_MaxValue = 100;
        [Tooltip("The current value of the attribute.")]
        [SerializeField] protected float m_Value = 100;
        [Tooltip("Describes how the attribute should update the value.")]
        [SerializeField] protected AutoUpdateValue m_AutoUpdateValueType;
        [Tooltip("Should the attribute be updated when the GameObject is inactive?")]
        [SerializeField] protected bool m_UpdateWhenInactive;
        [Tooltip("The amount of time between a value change and when the auto updater should start.")]
        [SerializeField] protected float m_AutoUpdateStartDelay = 1f;
        [Tooltip("The amount of time to wait in between auto update loops.")]
        [SerializeField] protected float m_AutoUpdateInterval = 0.1f;
        [Tooltip("The amount to change the value with each auto update.")]
        [SerializeField] protected float m_AutoUpdateAmount = 0.2f;

        [NonSerialized] public string Name { get { return m_Name; } set { m_Name = value; } }
        public float MinValue { get { return m_MinValue; } set { m_MinValue = value; } }
        public float MaxValue { get { return m_MaxValue; } set { m_MaxValue = value; } }
        [NonSerialized] public float Value { get { return m_Value; }
            set
            {
                m_Value = Mathf.Clamp(value, m_MinValue, m_MaxValue);
                EventHandler.ExecuteEvent(m_GameObject, "OnAttributeUpdateValue", this);

                ScheduleAutoUpdate(m_AutoUpdateStartDelay);
            }
        }
        public AutoUpdateValue AutoUpdateValueType { get { return m_AutoUpdateValueType; } set { m_AutoUpdateValueType = value;
                if (m_AutoUpdateStartDelay != -1) {
                    ScheduleAutoUpdate(m_AutoUpdateStartDelay);
                }
            } }
        public bool UpdateWhenInactive { get { return m_UpdateWhenInactive; } set { m_UpdateWhenInactive = value; } }
        public float AutoUpdateStartDelay { get { return m_AutoUpdateStartDelay; } set { m_AutoUpdateStartDelay = value;
                if (m_AutoUpdateStartDelay != -1) {
                    ScheduleAutoUpdate(m_AutoUpdateStartDelay);
                }
            } }
        public float AutoUpdateInterval { get { return m_AutoUpdateInterval; } set { m_AutoUpdateInterval = value; } }
        public float AutoUpdateAmount { get { return m_AutoUpdateAmount; } set { m_AutoUpdateAmount = value; } }

        [System.NonSerialized] private GameObject m_GameObject;
        private float m_StartValue;
        private ScheduledEventBase m_AutoUpdateEvent;

        private bool m_ValuesStored;
        private Attribute.AutoUpdateValue m_StoredAutoUpdateValueType;
        private float m_StoredAutoUpdateStartDelay;
        private float m_StoredAutoUpdateInterval;
        private float m_StoredAutoUpdateAmount;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Attribute() { }

        /// <summary>
        /// Two parameter constructor.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        public Attribute(string name, float value)
        {
            m_Name = name;
            m_Value = m_MaxValue = value;
        }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        /// <param name="gameObject">The GameObject this object is attached to.</param>
        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);

            m_GameObject = gameObject;
            m_StartValue = m_Value;

            ScheduleAutoUpdate(m_AutoUpdateStartDelay);
        }

        /// <summary>
        /// Schedules an auto update if the auto update value type is not set to none.
        /// </summary>
        /// <param name="delay">The amount to delay the attribute update event by.</param>
        public void ScheduleAutoUpdate(float delay)
        {
            Scheduler.Cancel(m_AutoUpdateEvent);
            if ((m_AutoUpdateValueType == AutoUpdateValue.Increase && m_Value != m_MaxValue) ||
                (m_AutoUpdateValueType == AutoUpdateValue.Decrease && m_Value != m_MinValue)) {
                m_AutoUpdateEvent = Scheduler.Schedule(delay, UpdateValue);
            }
        }

        /// <summary>
        /// Callback when the auto update event is executed.
        /// </summary>
        private void UpdateValue()
        {
            if (m_AutoUpdateValueType == AutoUpdateValue.None || (!m_UpdateWhenInactive && !m_GameObject.activeInHierarchy)) {
                return;
            }

            m_AutoUpdateEvent = null;
            if (m_AutoUpdateValueType == AutoUpdateValue.Increase) {
                m_Value = Mathf.Min(m_Value + m_AutoUpdateAmount, m_MaxValue);
                if (m_Value < m_MaxValue) {
                    m_AutoUpdateEvent = Scheduler.ScheduleFixed(m_AutoUpdateInterval, UpdateValue);
                } else {
                    EventHandler.ExecuteEvent(this, "OnAttributeReachedDestinationValue");
                }
            } else { // Decrease.
                m_Value = Mathf.Max(m_Value - m_AutoUpdateAmount, m_MinValue);
                if (m_Value > m_MinValue) {
                    m_AutoUpdateEvent = Scheduler.ScheduleFixed(m_AutoUpdateInterval, UpdateValue);
                } else {
                    EventHandler.ExecuteEvent(this, "OnAttributeReachedDestinationValue");
                }
            }
            EventHandler.ExecuteEvent(m_GameObject, "OnAttributeUpdateValue", this);
        }

        /// <summary>
        /// Check if the destination value has been reached.
        /// </summary>
        /// <returns>True if at the destination value.</returns>
        public bool IsAtDestinationValue()
        {
            if (m_AutoUpdateValueType == AutoUpdateValue.Increase) {
                return m_Value >= m_MaxValue;
            } 
            
            // Decrease.
            return m_Value <= m_MinValue;
        }

        /// <summary>
        /// Is the attribute currently valid? A valid attribute will not be at the minimum value.
        /// </summary>
        /// <param name="valueChange">The amount that the attribute is going to change values by.</param>
        /// <returns>True if the attribute value is currently valid.</returns>
        public bool IsValid(float valueChange)
        {
            return m_Value + valueChange >= m_MinValue;
        }

        /// <summary>
        /// Stores or restores the auto update values. This is used by the ability system.
        /// </summary>
        /// <param name="store">Should the values be stored?</param>
        public void StoreRestoreAutoUpdateValues(bool store)
        {
            if (m_ValuesStored == store) {
                return;
            }

            m_ValuesStored = store;
            if (store) {
                m_StoredAutoUpdateAmount = m_AutoUpdateAmount;
                m_StoredAutoUpdateInterval = m_AutoUpdateInterval;
                m_StoredAutoUpdateStartDelay = m_AutoUpdateStartDelay;
                m_StoredAutoUpdateValueType = m_AutoUpdateValueType;
            } else {
                m_AutoUpdateAmount = m_StoredAutoUpdateAmount;
                m_AutoUpdateInterval = m_StoredAutoUpdateInterval;
                m_AutoUpdateStartDelay = m_StoredAutoUpdateStartDelay;
                m_AutoUpdateValueType = m_StoredAutoUpdateValueType;
            }
        }

        /// <summary>
        /// Cancels the auto update value.
        /// </summary>
        public void CancelAutoUpdate()
        {
            Scheduler.Cancel(m_AutoUpdateEvent);
        }

        /// <summary>
        /// Resets the value to the starting value.
        /// </summary>
        public void ResetValue()
        {
            Scheduler.Cancel(m_AutoUpdateEvent);
            m_Value = m_StartValue;
            if (m_GameObject != null) {
                EventHandler.ExecuteEvent(m_GameObject, "OnAttributeUpdateValue", this);
            }
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            ScheduleAutoUpdate(m_AutoUpdateStartDelay);
        }

        /// <summary>
        /// Attribute destructor - will cancel the update event.
        /// </summary>
        ~Attribute()
        {
            Scheduler.Cancel(m_AutoUpdateEvent);
        }
    }

    /// <summary>
    /// Represents an Attribute that can have its values changed.
    /// </summary>
    [System.Serializable]
    public class AttributeModifier
    {
        [Tooltip("The name of the attribute.")]
        [SerializeField] protected string m_AttributeName;
        [Tooltip("Specifies the amount to change the attribute by when the modifier is enabled.")]
        [SerializeField] protected float m_Amount;
        [Tooltip("Should the attribute be updated every specified interval?")]
        [SerializeField] protected bool m_AutoUpdate;
        [Tooltip("The amount of time between a value change and when the auto updater should start.")]
        [SerializeField] protected float m_AutoUpdateStartDelay = 1f;
        [Tooltip("The amount of time to wait in between auto update loops.")]
        [SerializeField] protected float m_AutoUpdateInterval = 0.1f;
        [Tooltip("The duration that the auto update lasts for. Set to a positive value to enable.")]
        [SerializeField] protected float m_AutoUpdateDuration = -1;

        [NonSerialized] public string AttributeName { get { return m_AttributeName; } set { m_AttributeName = value; } }
        [NonSerialized] public float Amount { get { return m_Amount; } set { m_Amount = value; } }
        [NonSerialized] public bool AutoUpdate { get { return m_AutoUpdate; } set { m_AutoUpdate = value; } }
        [NonSerialized] public float AutoUpdateStartDelay { get { return m_AutoUpdateStartDelay; } set { m_AutoUpdateStartDelay = value; } }
        [NonSerialized] public float AutoUpdateInterval { get { return m_AutoUpdateInterval; } set { m_AutoUpdateInterval = value; } }
        [NonSerialized] public float AutoUpdateDuration { get { return m_AutoUpdateDuration; } set { m_AutoUpdateDuration = value; } }

        private Attribute m_Attribute;

        private bool m_AutoUpdating;
        private ScheduledEventBase m_DisableAutoUpdateEvent;

        public Attribute Attribute { get { return m_Attribute; } }
        public bool AutoUpdating { get { return m_AutoUpdating; } }

        /// <summary>
        /// Default AttributeModifier constructor.
        /// </summary>
        public AttributeModifier() { }

        /// <summary>
        /// AttributeModifier constructor.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="amount">Specifies the amount to change the attribute by when the modifier is enabled.</param>
        /// <param name="autoUpdateValueType">Describes how the attribute should update the value.</param>
        public AttributeModifier(string name, float amount, Attribute.AutoUpdateValue autoUpdateValueType)
        {
            m_AttributeName = name;
            m_Amount = amount;
            m_AutoUpdate = autoUpdateValueType != Attribute.AutoUpdateValue.None;
        }

        /// <summary>
        /// Initializes the AttributeModifier.
        /// </summary>
        /// <param name="gameObject">The GameObject that has the AttributeManager attached to it.</param>
        /// <returns>True if the AttributeModifier was initialized.</returns>
        public bool Initialize(GameObject gameObject)
        {
            if (string.IsNullOrEmpty(m_AttributeName)) {
                return false;
            }

            var attributeManager = gameObject.GetCachedComponent<AttributeManager>();
            if (attributeManager == null) {
                return false;
            }

            m_Attribute = attributeManager.GetAttribute(m_AttributeName);
            return m_Attribute != null;
        }

        /// <summary>
        /// Initializes the AttributeModifier from another AttributeModifier.
        /// </summary>
        /// <param name="other">The AttributeModifier to copy.</param>
        /// <param name="attributeManager">The AttributeManager that the modifier is attached to.</param>
        /// <returns>True if the AttributeModifier was initialized.</returns>
        public bool Initialize(AttributeModifier other, AttributeManager attributeManager)
        {
            if (string.IsNullOrEmpty(other.AttributeName) || attributeManager == null) {
                return false;
            }

            m_AttributeName = other.AttributeName;
            m_Amount = other.Amount;
            m_AutoUpdate = other.AutoUpdate;
            m_AutoUpdateStartDelay = other.AutoUpdateStartDelay;
            m_AutoUpdateInterval = other.m_AutoUpdateInterval;
            m_AutoUpdateDuration = other.AutoUpdateDuration;

            m_Attribute = attributeManager.GetAttribute(m_AttributeName);
            return m_Attribute != null;
        }

        /// <summary>
        /// Is the attribute currently valid? A valid attribute will not be at the minimum value.
        /// </summary>
        /// <returns>True if the attribute value is currently valid.</returns>
        public bool IsValid()
        {
            if (m_Attribute == null || string.IsNullOrEmpty(m_Attribute.Name)) {
                return true;
            }

            if (m_AutoUpdating) {
                if (m_Amount < 0) {
                    return m_Attribute.Value > m_Attribute.MinValue;
                } else {
                    return m_Attribute.Value < m_Attribute.MaxValue;
                }
            }
            
            return m_Attribute.IsValid(m_Amount);
        }

        /// <summary>
        /// Enables or disables the modifier.
        /// </summary>
        /// <param name="enable">Should the modifier be enabled?</param>
        public void EnableModifier(bool enable)
        {
            if (m_Attribute == null || string.IsNullOrEmpty(m_Attribute.Name)) {
                return;
            }
            m_DisableAutoUpdateEvent = null;

            // The attribute can be changed by a single value...
            if (enable && (!m_AutoUpdate || m_AutoUpdateStartDelay > 0)) {
                m_Attribute.Value += m_Amount;
            }

            if (!m_AutoUpdate || m_AutoUpdating == enable) {
                return;
            }

            // ...Or a change with a longer duration.
            m_AutoUpdating = enable;
            if (enable) {
                m_Attribute.StoreRestoreAutoUpdateValues(true);

                m_Attribute.AutoUpdateAmount = Mathf.Abs(m_Amount);
                m_Attribute.AutoUpdateStartDelay = -1; // Set the start delay to -1 to prevent the attribute from updating when changing the attribute properties.
                m_Attribute.AutoUpdateInterval = m_AutoUpdateInterval;
                m_Attribute.AutoUpdateValueType = m_Amount < 0 ? Attribute.AutoUpdateValue.Decrease : Attribute.AutoUpdateValue.Increase;
                m_Attribute.AutoUpdateStartDelay = m_AutoUpdateStartDelay; // Setting the actual start delay will update the value.

                if (m_AutoUpdateDuration > 0) {
                    m_DisableAutoUpdateEvent = Scheduler.Schedule(m_AutoUpdateDuration, EnableModifier, false);
                }
            } else {
                m_Attribute.StoreRestoreAutoUpdateValues(false);

                if (m_DisableAutoUpdateEvent != null) {
                    Scheduler.Cancel(m_DisableAutoUpdateEvent);
                    m_DisableAutoUpdateEvent = null;
                }

                m_Attribute.ScheduleAutoUpdate(m_Attribute.AutoUpdateStartDelay);
            }

            EventHandler.ExecuteEvent(this, "OnAttributeModifierAutoUpdateEnabled", this, enable);
        }
    }

    /// <summary>
    /// The AttributeManager will manage the array of Attributes.
    /// </summary>
    public class AttributeManager : MonoBehaviour
    {
        [Tooltip("The array of Attributes on the object.")]
        [SerializeField] protected Attribute[] m_Attributes = new Attribute[] { new Attribute("Health", 100) };

        public Attribute[] Attributes { get { return m_Attributes; } set {
                if (m_Attributes != value) {
                    m_Attributes = value;
                    if (Application.isPlaying) {
                        AwakeInternal();
                    }
                }
            }
        }

        private Dictionary<string, Attribute> m_NameAttributeMap = new Dictionary<string, Attribute>();

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected virtual void Awake()
        {
            AwakeInternal();
        }

        /// <summary>
        /// Initializes the attributes.
        /// </summary>
        protected virtual void AwakeInternal()
        {
            if (m_NameAttributeMap.Count > 0) {
                return;
            }

            for (int i = 0; i < m_Attributes.Length; ++i) {
                m_NameAttributeMap.Add(m_Attributes[i].Name, m_Attributes[i]);
                m_Attributes[i].Initialize(gameObject);
            }
        }

        /// <summary>
        /// Gets the attribute with the specified name.
        /// </summary>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <returns>The attribute with the specified name. Will return null if no attributes with the specified name exist.</returns>
        public Attribute GetAttribute(string attributeName)
        {
            if (string.IsNullOrEmpty(attributeName)) {
                return null;
            }

            if (m_Attributes.Length > 0 && m_NameAttributeMap.Count == 0) {
                AwakeInternal();
            }

            Attribute attribute;
            if (m_NameAttributeMap.TryGetValue(attributeName, out attribute)) {
                return attribute;
            }
            return null;
        }
    }
}