/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Effect
{
    using Opsive.Shared.Game;
    using System;
    using UnityEngine;

    /// <summary>
    /// Invoke a Unity Event.
    /// </summary>
    [Serializable]
    public class EnableDisableObject : ItemEffect
    {
        [Tooltip("The Prefab to spawn.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<GameObject> m_Object;
        [Tooltip("Enable the Object or disable it?")]
        [SerializeField] protected bool m_EnableObject;
        [Tooltip("return the object to its previous active state after some time. (Ignored if 0 or less).")]
        [SerializeField] protected float m_RevertTime = -1;
        
        public ItemPerspectiveIDObjectProperty<GameObject> Object { get => m_Object; set => m_Object = value; }
        public float LifeTime { get => m_RevertTime; set => m_RevertTime = value; }

        protected Action<bool, bool> m_DoEnableDisable;
        protected ScheduledEventBase m_EnableDisableScheduledEvent;

        /// <summary>
        /// Initialize the effect.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            m_Object.Initialize(m_CharacterItemAction);
            m_DoEnableDisable = DoEnableDisable;
        }

        /// <summary>
        /// Invoke the effect.
        /// </summary>
        protected override void InvokeEffectInternal()
        {
            base.InvokeEffectInternal();

            DoEnableDisable(m_EnableObject, true);
        }

        /// <summary>
        /// Spawn the prefab gameobject.
        /// </summary>
        private void DoEnableDisable(bool enable, bool revert)
        {
            var obj = m_Object.GetValue();
            if(obj == null){ return; }

            if (m_EnableDisableScheduledEvent != null) {
                Scheduler.Cancel(m_EnableDisableScheduledEvent);
                m_EnableDisableScheduledEvent = null;
            }
            
            var previousActiveState = !enable;
            obj.SetActive(enable);
            if (revert && m_RevertTime > 0) {
                m_EnableDisableScheduledEvent = Scheduler.Schedule(m_RevertTime, m_DoEnableDisable, previousActiveState, false);
            }
        }
    }
}