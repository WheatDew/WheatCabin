using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class UnityEventBaseEditor : PropertyEditor<UnityEventBase>
    {
        public class PersistentCallAccessor
        {
            private int m_eventIndex;
            private int m_callIndex;
            private UnityEventBaseEditor m_editor;

            public Type UnityEventType
            {
                get 
                {
                    object eventBase = m_editor.GetValue();
                    return eventBase.GetType();
                }
            }

            public PersistentCall Value
            {
                get { return m_editor.m_cachedCalls[m_eventIndex][m_callIndex]; }
                set 
                { 
                    for(int eventIndex = 0; eventIndex < m_editor.Targets.Length; ++eventIndex)
                    {
                        PersistentCall[] calls = m_editor.m_cachedCalls[eventIndex];
                        calls[m_callIndex] = value;

                        UnityEventBase eventBase = m_editor.GetValue(eventIndex);
                        eventBase.SetPersistentCalls(calls);
                    }
                }
            }

            public PersistentCallAccessor(UnityEventBaseEditor editor, int eventIndex, int callIndex)
            {
                m_editor = editor;
                m_eventIndex = eventIndex;
                m_callIndex = callIndex;
            }
        }

        [SerializeField]
        private ToggleGroup m_panel = null;
        [SerializeField]
        private Button m_addButton = null;
        [SerializeField]
        private Button m_removeButton = null;

        private PropertyEditor m_propertyEditorPrefab;
        private PersistentCall[][] m_cachedCalls;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();

            IEditorsMap editorsMap = IOC.Resolve<IEditorsMap>();
            GameObject propertyEditorPrefab = editorsMap.GetPropertyEditor(typeof(PersistentCall));
            if(propertyEditorPrefab != null)
            {
                m_propertyEditorPrefab = propertyEditorPrefab.GetComponent<PropertyEditor>();
            }
            
            if(m_propertyEditorPrefab == null)
            {
                Debug.LogErrorFormat("PropertyEditor for {0} was not found", typeof(PersistentCall).FullName);
            }

            UnityEventHelper.AddListener(m_addButton, btn => btn.onClick, OnAddPersistentCall);
            UnityEventHelper.AddListener(m_removeButton, btn => btn.onClick, OnRemovePersistentCall);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            UnityEventHelper.RemoveListener(m_addButton, btn => btn.onClick, OnAddPersistentCall);
            UnityEventHelper.RemoveListener(m_removeButton, btn => btn.onClick, OnRemovePersistentCall);
        }

        protected override void InitOverride(object[] target, object[] accessor, MemberInfo memberInfo, Action<object, object> eraseTargetCallback = null, string label = null)
        {
            base.InitOverride(target, accessor, memberInfo, eraseTargetCallback, label);
            BuildEditor();
        }

        private void BuildEditor()
        {
            foreach (Transform editor in m_panel.transform)
            {
                Destroy(editor.gameObject);
            }

            int targetsCount = Targets.Length;
            if (targetsCount == 0)
            {
                return;
            }

            m_cachedCalls = new PersistentCall[targetsCount][];
            PropertyInfo valueInfo = Strong.PropertyInfo((PersistentCallAccessor x) => x.Value);

            int minCount = int.MaxValue;
            for (int i = 0; i < targetsCount; ++i)
            {
                UnityEventBase eventBase = GetValue(i);
                int eventCount = eventBase.GetPersistentEventCount();
                if (eventCount < minCount)
                {
                    minCount = eventCount;
                }
                m_cachedCalls[i] = eventBase.GetPersistentCalls();
            }

            for (int j = 0; j < minCount; ++j)
            {
                PropertyEditor editor = Instantiate(m_propertyEditorPrefab);
                editor.transform.SetParent(m_panel.transform, false);
                Toggle toggle = editor.GetComponent<Toggle>();
                if(toggle == null)
                {
                    Debug.LogError("Toggle control must be added to PersistentCallEditor");
                }
                else
                {
                    toggle.group = m_panel;
                }
                
                PersistentCallAccessor[] accessors = new PersistentCallAccessor[targetsCount];
                for (int i = 0; i < targetsCount; ++i)
                {
                    accessors[i] = new PersistentCallAccessor(this, i, j);
                }

                editor.Init(accessors, valueInfo, string.Empty, false, OnBeginRecordValue, OnEndRecordValue);
            }

            if (m_removeButton != null)
            {
                m_removeButton.interactable = minCount > 0;
            }
        }

        private void OnBeginRecordValue()
        {
            BeginEdit();

            for (int i = 0; i < Targets.Length; ++i)
            {
                SetValue(Duplicate(GetValue(i)), i);
            }
        }

        private void OnEndRecordValue()
        {
            EndEdit();
        }

        private UnityEventBase Duplicate(UnityEventBase value)
        {
            if (value == null)
            {
                return null;
            }

            UnityEventBase duplicate = (UnityEventBase)Activator.CreateInstance(value.GetType());
            duplicate.SetPersistentCalls(value.GetPersistentCalls());
            return duplicate;
        }

        protected override void ReloadOverride(bool force)
        {
            if (force)
            {
                BuildEditor();
                RaiseReloadCallback();
            }
            else
            {
                UnityEventBase value = GetValue();
                if (CurrentValue != value)
                {
                    CurrentValue = value;
                    BuildEditor();
                    RaiseReloadCallback();
                }
            }
        }

        private void OnAddPersistentCall()
        {
            BeginEdit();

            int targetsCount = Targets.Length;
            for (int l = 0; l < targetsCount; ++l)
            {
                UnityEventBase unityEvent = Duplicate(GetValue(l));
                PersistentCall call = PersistentCall.CreateNew();
                unityEvent.AddPersistentCall(call);
                SetValue(unityEvent, l);
            }

            EndEdit();
            BuildEditor();
        }

        private void OnRemovePersistentCall()
        {
            int index = -1;
            foreach(Transform editor in m_panel.transform)
            {
                index++;

                Toggle toggle = editor.GetComponent<Toggle>();
                if(toggle.isOn)
                {
                    break;
                }
            }

            if (index >= 0)
            {
                BeginEdit();

                int targetsCount = Targets.Length;
                for (int l = 0; l < targetsCount; ++l)
                {
                    UnityEventBase unityEvent = Duplicate(GetValue(l));
                    unityEvent.RemovePersistentCall(index);
                    SetValue(unityEvent, l);
                }

                EndEdit();
                BuildEditor();
            }
        }
    }
}


