using Battlehub.RTCommon;
using Battlehub.RTHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public interface ISelectionComponentState
    {
        IRuntimeSelectionComponent Component
        {
            get;
        }

        void CanSelect(object setter, bool value);
        void CanSelectAll(object setter, bool value);
        void IsBoxSelectionEnabled(object setter, bool value);
        void IsPositionHandleEnabled(object setter, bool value);
        void IsRotationHandleEnabled(object setter, bool value);
        void IsScaleHandleEnabled(object setter, bool value);
        void IsRectToolEnabled(object setter, bool value);
        void EnableAll(object setter, bool value);
    }

    [DefaultExecutionOrder(-50)]
    public class SelectionComponentState : RTEComponent, ISelectionComponentState
    {
        private IRuntimeSelectionComponent m_component;

        public IRuntimeSelectionComponent Component
        {
            get { return m_component; }
        }

        private void SetValue(object setter, bool value, Dictionary<object, bool> d, Action<bool> setValue)
        {
            if (value)
            {
                d.Remove(setter);
            }
            else
            {
                d[setter] = value;
            }
            setValue(d.Values.All(t => t));
        }

        private Dictionary<object, bool> m_canSelect = new Dictionary<object, bool>();
        public void CanSelect(object setter, bool value)
        {
            SetValue(setter, value, m_canSelect, v => m_component.CanSelect = v);
        }

        private Dictionary<object, bool> m_canSelectAll = new Dictionary<object, bool>();
        public void CanSelectAll(object setter, bool value)
        {
            SetValue(setter, value, m_canSelectAll, v => m_component.CanSelectAll = v);
        }

        private Dictionary<object, bool> m_isBoxSelectionEnabled = new Dictionary<object, bool>();
        public void IsBoxSelectionEnabled(object setter, bool value)
        {
            SetValue(setter, value, m_isBoxSelectionEnabled, v => m_component.IsBoxSelectionEnabled = v);
        }

        private Dictionary<object, bool> m_isPositionHandleEnabled = new Dictionary<object, bool>();
        public void IsPositionHandleEnabled(object setter, bool value)
        {
            SetValue(setter, value, m_isPositionHandleEnabled, v => m_component.IsPositionHandleEnabled = v);
        }

        private Dictionary<object, bool> m_isRotationHandleEnabled = new Dictionary<object, bool>();
        public void IsRotationHandleEnabled(object setter, bool value)
        {
            SetValue(setter, value, m_isRotationHandleEnabled, v => m_component.IsRotationHandleEnabled = v);
        }

        private Dictionary<object, bool> m_isScaleHandleEnabled = new Dictionary<object, bool>();
        public void IsScaleHandleEnabled(object setter, bool value)
        {
            SetValue(setter, value, m_isScaleHandleEnabled, v => m_component.IsScaleHandleEnabled = v);
        }

        private Dictionary<object, bool> m_isRectToolEnabled = new Dictionary<object, bool>();
        public void IsRectToolEnabled(object setter, bool value)
        {
            SetValue(setter, value, m_isRectToolEnabled, v => m_component.IsRectToolEnabled = v);
        }

        public void EnableAll(object setter, bool value)
        {
            CanSelect(setter, value);
            CanSelectAll(setter, value);
            IsBoxSelectionEnabled(setter, value);
            IsPositionHandleEnabled(setter, value);
            IsRotationHandleEnabled(setter, value);
            IsScaleHandleEnabled(setter, value);
            IsRectToolEnabled(setter, value);
        }

        protected override void Awake()
        {
            base.Awake();        
            m_component = Window.IOCContainer.Resolve<IRuntimeSelectionComponent>();
            Window.IOCContainer.RegisterFallback<ISelectionComponentState>(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(Window != null)
            {
                Window.IOCContainer.UnregisterFallback<ISelectionComponentState>(this);
            }
        }
    }

}

