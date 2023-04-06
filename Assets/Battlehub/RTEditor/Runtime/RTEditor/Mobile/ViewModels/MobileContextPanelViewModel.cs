using Battlehub.RTCommon;
using Battlehub.RTEditor.Mobile.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Mobile.ViewModels
{
    [Binding]
    public class MobileContextPanelViewModel : MonoBehaviour, System.ComponentModel.INotifyPropertyChanged
    {
        private WaitForSeconds m_cleanupInterval = new WaitForSeconds(0.25f);
        private IEnumerator m_coCleanup;
        private List<List<Component>> m_groups = new List<List<Component>>();
        
        private ObservableList<MobileComponentViewModel> m_components = new ObservableList<MobileComponentViewModel>();
        [Binding]
        public ObservableList<MobileComponentViewModel> Components
        {
            get { return m_components; }
            set
            {
                if(m_components != value)
                {
                    m_components = value;
                    RaisePropertyChanged(nameof(Components));
                }
            }
        }

        [Binding]
        public string GameObjectName
        {
            get
            {
                if(m_rte == null)
                {
                    return string.Empty;
                }

                GameObject[] gameObjects = m_rte.Selection.gameObjects;
                int count = gameObjects != null ? gameObjects.Where(go => go != null).Count() : 0;
                if (count == 0)
                {
                    return string.Empty;
                }
                
                if(count > 1)
                {
                    return $"{count} objects";
                }

                return gameObjects.Where(go => go != null).First().name;
            }
        }

        private bool m_isInspectorVisible;

        [Binding]
        public bool IsInspectorVisible
        {
            get { return m_isInspectorVisible; }
            private set
            {
                if(m_isInspectorVisible != value)
                {
                    m_isInspectorVisible = value;
                    RaisePropertyChanged(nameof(IsInspectorVisible));
                }
            }
        }

        private IRTE m_rte;
        private IEditorsMap m_editorsMap;
        private IMobileEditorModel m_mobileEditorModel;

        private void Start()
        {
            m_rte = IOC.Resolve<IRTE>();
            m_rte.Selection.SelectionChanged += OnEditorSelectionChanged;
            m_rte.Object.NameChanged += OnObjectNameChanged;
            m_rte.Object.ComponentAdded += OnComponentAdded;
            m_rte.Object.ComponentDestroyed += OnComponentDestroyed;

            m_editorsMap = IOC.Resolve<IEditorsMap>();
            m_mobileEditorModel = IOC.Resolve<IMobileEditorModel>();
            if(m_mobileEditorModel != null)
            {
                IsInspectorVisible = m_mobileEditorModel.IsInspectorOpened;

                m_mobileEditorModel.IsInspectorOpenedChanged += OnIsInspectorVisibleChanged;
                m_mobileEditorModel = null;
            }

            Components = CreateComponentViewModels();
            RaisePropertyChanged(nameof(GameObjectName));
        }

        private void OnDestroy()
        {
            if(m_rte != null)
            {
                if(m_rte.Selection != null)
                {
                    m_rte.Selection.SelectionChanged -= OnEditorSelectionChanged;
                }
                
                if(m_rte.Object != null)
                {
                    m_rte.Object.NameChanged -= OnObjectNameChanged;
                    m_rte.Object.ComponentAdded -= OnComponentAdded;
                    m_rte.Object.ComponentDestroyed -= OnComponentDestroyed;
                }
                
                m_rte = null;
            }

            m_editorsMap = null;

            if(m_mobileEditorModel != null)
            {
                m_mobileEditorModel.IsInspectorOpenedChanged -= OnIsInspectorVisibleChanged;
                m_mobileEditorModel = null;
            }
        }

        private void OnEnable()
        {
            StartCoroutine(m_coCleanup = CoCleanup());
        }

        private void OnDisable()
        {
            if (m_coCleanup != null)
            {
                StopCoroutine(m_coCleanup);
                m_coCleanup = null;
            }
        }

        private IEnumerator CoCleanup()
        {
            while(true)
            {
                for(int i = 0; i < m_groups.Count; ++i)
                {
                    IList<Component> group = m_groups[i];
                    for(int j = group.Count - 1; j >= 0; j--)
                    {
                        yield return null;

                        Component component = group[j];
                        if(component == null)
                        {
                            i = m_groups.Count;

                            CreateComponentViewModels();
                            RemoveComponent(component);

                            break;
                        }
                    }
                }

                yield return m_cleanupInterval;
            }
        }

        private void OnIsInspectorVisibleChanged(object sender, ValueChangedArgs<bool> e)
        {
            IsInspectorVisible = e.NewValue;
        }

        private void OnComponentAdded(ExposeToEditor obj, Component component)
        {
            if(component == null)
            {
                return;
            }

            Components = CreateComponentViewModels();
        }

        private void OnComponentDestroyed(ExposeToEditor obj, Component component)
        {
            CreateComponentViewModels();
            RemoveComponent(component);
        }

        private void RemoveComponent(Component component)
        {
            MobileComponentViewModel componentViewModel = Components.Where(c => c.Components.Contains(component)).FirstOrDefault();
            if (componentViewModel != null)
            {
                Components.Remove(componentViewModel);
            }
        }

        private void OnObjectNameChanged(ExposeToEditor obj)
        {
            RaisePropertyChanged(nameof(GameObjectName));
        }

        private void OnEditorSelectionChanged(Object[] unselectedObjects)
        {
            Components = CreateComponentViewModels();
            RaisePropertyChanged(nameof(GameObjectName));
        }

        private ObservableList<MobileComponentViewModel> CreateComponentViewModels()
        {
            ObservableList<MobileComponentViewModel> list = new ObservableList<MobileComponentViewModel>();
            if(m_rte.Selection.activeGameObject != null)
            {
                m_groups = GameObjectEditorUtils.GetComponentGroups(m_rte.Selection.gameObjects);

                foreach (var group in m_groups)
                {
                    Component component = group.First();
                    if (m_editorsMap.IsObjectEditorEnabled(component.GetType()))
                    {
                        list.Add(new MobileComponentViewModel(group));
                    }
                }
            }
            else
            {
                m_groups.Clear();
            }
            return list;
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

    }

}
