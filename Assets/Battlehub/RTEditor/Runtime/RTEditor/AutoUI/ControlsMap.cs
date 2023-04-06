using Battlehub.RTCommon;
using Battlehub.UIControls;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor.UI
{
    public interface IControlsMap
    {
        T GetControl<T>() where T : Component;
    }

    [DefaultExecutionOrder(-90)]
    public class ControlsMap : MonoBehaviour, IControlsMap
    {
        [SerializeField]
        private TextMeshProUGUI m_labelPrefab = null;

        [SerializeField]
        private Button m_buttonPrefab = null;

        [SerializeField]
        private TMP_Dropdown m_dropdownPrefab = null;

        [SerializeField]
        private Toggle m_togglePrefab = null;

        [SerializeField]
        private VirtualizingTreeView m_virtualizingTreeViewPrefab = null;

        [SerializeField]
        private VirtualizingTreeViewItem m_virtualizingTreeViewItemPrefab = null;

        public T GetControl<T>() where T : Component
        {
            if (m_typeToComponent.TryGetValue(typeof(T), out Component component))
            {
                return (T)component;
            }
            return null;
        }

        private Dictionary<Type, Component> m_typeToComponent;

        private void Awake()
        {
            m_typeToComponent = new Dictionary<Type, Component>
            {
                { typeof(TextMeshProUGUI), m_labelPrefab },
                { typeof(Button), m_buttonPrefab },
                { typeof(TMP_Dropdown), m_dropdownPrefab },
                { typeof(Toggle), m_togglePrefab },
                { typeof(VirtualizingTreeView), m_virtualizingTreeViewPrefab },
                { typeof(VirtualizingTreeViewItem), m_virtualizingTreeViewItemPrefab },
            };

            IOC.RegisterFallback<IControlsMap>(this);
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<IControlsMap>(this);
        }
    }

}
