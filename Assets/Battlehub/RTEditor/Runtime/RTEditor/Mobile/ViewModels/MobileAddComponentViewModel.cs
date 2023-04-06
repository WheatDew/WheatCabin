using UnityWeld.Binding;
using Battlehub.RTEditor.ViewModels;
using System;
using System.Collections.Generic;
using Battlehub.RTCommon;
using System.Linq;
using UnityEngine;
using Battlehub.UIControls.Binding;

namespace Battlehub.RTEditor.Mobile.ViewModels
{
    [Binding]
    public class MobileAddComponentViewModel : HierarchicalDataViewModel<MobileAddComponentViewModel.ComponentTypeViewModel>
    {
        #region ComponentTypeViewModel
        [Binding]
        public class ComponentTypeViewModel
        {
            [Binding]
            public string DisplayName
            {
                get;
                private set;
            }

            public Type Type
            {
                get;
                private set;
            }

            public ComponentTypeViewModel(Type type, string displayName)
            {
                Type = type;
                DisplayName = displayName;
            }
        }
        #endregion

        private ComponentTypeViewModel[] m_cache;

        protected bool IsFilterTextEmpty
        {
            get { return string.IsNullOrWhiteSpace(m_filterText); }
        }

        private string m_filterText;
        [Binding]
        public string FilterText
        {
            get { return m_filterText; }
            set
            {
                if (m_filterText != value)
                {
                    m_filterText = value;
                    RaisePropertyChanged(nameof(FilterText));
                    BindData();
                    SelectedItem = null;
                }
            }
        }

        #region IHierarchicalData
        protected override void OnEnable()
        {
            base.OnEnable();

            Type[] editableTypes = IOC.Resolve<IEditorsMap>().GetEditableTypes();
            m_cache = editableTypes.Where(t => t.IsSubclassOf(typeof(Component))).OrderBy(t => t.Name).Select(t => new ComponentTypeViewModel(t, Localization.GetString($"ID_RTEditor_CD_{t.Name}", t.Name))).ToArray();
        }

        protected override void Start()
        {
            base.Start();
            BindData();
        }

        public override HierarchicalDataFlags GetFlags()
        {
            return HierarchicalDataFlags.None;
        }

        public override HierarchicalDataItemFlags GetItemFlags(ComponentTypeViewModel item)
        {
            return HierarchicalDataItemFlags.CanSelect;
        }

        public override IEnumerable<ComponentTypeViewModel> GetChildren(ComponentTypeViewModel parent)
        {
            if (IsFilterTextEmpty)
            {
                return m_cache;
            }

            return m_cache.Where(t => t.DisplayName.ToLower().Contains(m_filterText.ToLower()));
            
        }
        #endregion

        #region Bound UnityEvent Handlers
        [Binding]
        public virtual void OnAddComponent()
        {
            if(SelectedItem != null)
            {
                AddComponent(SelectedItem.Type);
            }
        }
        #endregion

        #region Methods
        private void AddComponent(Type type)
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            editor.Undo.BeginRecord();

            GameObject[] gameObjects = Selection.gameObjects;
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject go = gameObjects[i];
                ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
                editor.Undo.AddComponentWithRequirements(exposeToEditor, type);
            }

            editor.Undo.EndRecord();
        }
        #endregion
    }
}
