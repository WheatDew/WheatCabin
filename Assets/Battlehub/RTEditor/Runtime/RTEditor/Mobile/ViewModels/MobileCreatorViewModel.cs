using UnityWeld.Binding;
using Battlehub.RTEditor.ViewModels;
using System;
using System.Collections.Generic;
using Battlehub.RTCommon;
using System.Linq;
using UnityEngine;
using Battlehub.UIControls.Binding;
using Battlehub.RTEditor.Models;
using Battlehub.RTSL.Interface;

namespace Battlehub.RTEditor.Mobile.ViewModels
{
   
    [Binding]
    public class MobileCreatorViewModel : HierarchicalDataViewModel<MobileCreatorViewModel.CreatorItemViewModel>
    {
        #region CretatorItemViewModel
        [Binding]
        public class CreatorItemViewModel
        {
            [Binding]
            public string DisplayName
            {
                get;
                private set;
            }

            public string Path
            {
                get;
                private set;
            }

            public CreatorItemViewModel Parent
            {
                get;
                set;
            }

            public List<CreatorItemViewModel> Children
            {
                get;
                private set;
            }

            public bool HasChildren
            {
                get { return Children.Count > 0; }
            }

            public CreatorItemViewModel(string path, string displayName)
            {
                Path = path;
                DisplayName = displayName;
                Children = new List<CreatorItemViewModel>();
            }
        }
        #endregion

        private CreatorItemViewModel[] m_cache;
        private Ray m_ray;
        private Plane m_dragPlane;
        private ISettingsComponent m_settings;
        private IPlacementModel m_placement;
        private IProjectAsync m_project;
        
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

        private bool m_isObjectCreated;
        [Binding]
        public bool IsObjectCreated
        {
            get { return m_isObjectCreated; }
            set
            {
                if(m_isObjectCreated != value)
                {
                    m_isObjectCreated = value;

                    RaisePropertyChanged(nameof(IsObjectCreated)); 
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            m_project = IOC.Resolve<IProjectAsync>();
            m_settings = IOC.Resolve<ISettingsComponent>();
            m_settings.KnownGameObjectsChanged += OnKnownGameObjectsChanged;

            m_placement = IOC.Resolve<IPlacementModel>();

            InitCache();

            RuntimeWindow sceneWindow = Editor.ActiveWindow;
            if(sceneWindow == null || sceneWindow.WindowType != RuntimeWindowType.Scene)
            {
                sceneWindow = Editor.GetWindow(RuntimeWindowType.Scene);
            }
            m_ray = sceneWindow.Pointer;
            m_dragPlane = m_placement.GetDragPlane(sceneWindow.Camera.transform); 
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (m_settings != null)
            {
                m_settings.KnownGameObjectsChanged -= OnKnownGameObjectsChanged;
                m_settings = null;
            }

            m_placement = null;
            m_project = null;
        }

        protected override void Start()
        {
            base.Start();
            BindData();
        }
        #region IHierarchicalData
        public override HierarchicalDataFlags GetFlags()
        {
            return HierarchicalDataFlags.None;
        }

        public override HierarchicalDataItemFlags GetItemFlags(CreatorItemViewModel item)
        {
            return HierarchicalDataItemFlags.CanSelect;
        }

        public override bool HasChildren(CreatorItemViewModel parent)
        {
            return parent.HasChildren && IsFilterTextEmpty;
        }

        public override CreatorItemViewModel GetParent(CreatorItemViewModel item)
        {
            return item.Parent;
        }
        public override IEnumerable<CreatorItemViewModel> GetChildren(CreatorItemViewModel parent)
        {
            if (IsFilterTextEmpty)
            {
                if (parent == null)
                {
                    return m_cache;
                }

                return parent.Children;
            }

            List<CreatorItemViewModel> items = new List<CreatorItemViewModel>();
            foreach (CreatorItemViewModel item in m_cache)
            {
                ApplyFilter(item, i => i.DisplayName.ToLower().Contains(m_filterText.ToLower()), items);
            }
            return items;
        }
        #endregion

        #region Bound UnityEvent Handlers
        [Binding]
        public virtual void OnCreateGameObject()
        {
            if (SelectedItem != null && !SelectedItem.HasChildren)
            {
                CreateGameObject(SelectedItem.Path);
            }
        }
        #endregion

        #region Methods
        private async void CreateGameObject(string path)
        {
            GameObject go;
            if (path.StartsWith("/Assets"))
            {
                Editor.IsBusy = true;
                GameObject prefab = await m_project.Load_Async<GameObject>(path);
                go = Instantiate(prefab);
                go.name = prefab.name;
                Editor.IsBusy = false;
            }
            else
            { 
                go = m_settings.KnownGameObjects.Instantiate(path);
            }

            if (go != null)
            {
                Vector3 point;
                if(m_placement.GetHitPoint(m_ray, out point))
                {
                    m_placement.AddGameObjectToScene(go, point);
                }
                else if(m_placement.GetPointOnDragPlane(m_dragPlane, m_ray, out point))
                {
                    m_placement.AddGameObjectToScene(go, point);
                }
                else
                {
                    m_placement.AddGameObjectToScene(go);
                }
                
                IsObjectCreated = true;
            }
        }

        protected virtual void OnKnownGameObjectsChanged(object sender, GameObjectsAsset oldValue, GameObjectsAsset newValue)
        {
            InitCache();
            BindData();
        }

        private void InitCache()
        {
            if (m_settings.KnownGameObjects == null || m_settings.KnownGameObjects.MenuPath == null)
            {
                Debug.LogWarning("m_settings.KnownGameObjects == null || m_settings.KnownGameObjects.MenuPath == null");
                return;
            }

            ILocalization localization = IOC.Resolve<ILocalization>();
            Dictionary<string, CreatorItemViewModel> items = new Dictionary<string, CreatorItemViewModel>();

            foreach (string path in m_settings.KnownGameObjects.MenuPath)
            {
                string[] parts = path.Split('/');

                string id = parts[parts.Length - 1];

                string displayName = localization.GetString(id, id);

                CreatorItemViewModel creatorItem = new CreatorItemViewModel(path, displayName);
                items.Add(path, creatorItem);

                for (int i = parts.Length - 2; i >= 0; i--)
                {
                    string parentPath = string.Join("/", parts.Take(i + 1));
                    if (!items.TryGetValue(parentPath, out CreatorItemViewModel parentItem))
                    {
                        parentItem = new CreatorItemViewModel(parentPath, localization.GetString(parts[i], parts[i]));
                        items.Add(parentPath, parentItem);
                    }

                    parentItem.Children.Add(creatorItem);
                    creatorItem.Parent = parentItem;
                    creatorItem = parentItem;
                }
            }

            ITypeMap typeMap = IOC.Resolve<ITypeMap>();
            foreach(ProjectItem item in m_project.State.RootFolder.Flatten(true))
            {
                if (typeMap.ToType(item.GetTypeGuid()) != typeof(GameObject))
                {
                    continue;
                }

                string id = item.Name;
                string displayName = localization.GetString(id, id);
                string path = $"/Assets/{item.RelativePath(false)}";

                CreatorItemViewModel creatorItem = new CreatorItemViewModel(path, displayName);
                ProjectItem parent = item.Parent;
                while(parent != null)
                {
                    string parentPath = parent.ToString();
                    if (!items.TryGetValue(parentPath, out CreatorItemViewModel parentItem))
                    {
                        parentItem = new CreatorItemViewModel(parentPath, localization.GetString(parent.Name, parent.Name));
                        items.Add(parentPath, parentItem);
                    }

                    if (!parentItem.Children.Contains(creatorItem))
                    {
                        parentItem.Children.Add(creatorItem);
                        creatorItem.Parent = parentItem;
                    }
                    creatorItem = parentItem;
                    parent = parent.Parent;
                }
            }

            m_cache = items.Values.Where(item => item.Parent == null).ToArray();
        }

        private void ApplyFilter(CreatorItemViewModel item, Func<CreatorItemViewModel, bool> filter, List<CreatorItemViewModel> result)
        {
            if (filter(item))
            {
                result.Add(item);
            }

            foreach (CreatorItemViewModel child in item.Children)
            {
                ApplyFilter(child, filter, result);
            }
        }

        #endregion
    }

}
