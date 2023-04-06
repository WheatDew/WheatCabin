using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    public class GameObjectEditor : MonoBehaviour
    {
        [SerializeField]
        private BoolEditor IsActiveEditor = null;
        [SerializeField]
        private TMP_InputField InputName = null;
        [SerializeField]
        private GameObject LayersEditorRoot = null;
        [SerializeField]
        private OptionsEditor LayerEditor = null;
        [SerializeField]
        private Button EditLayersButton = null;
        [SerializeField]
        private Transform ComponentsPanel = null;

        [SerializeField]
        private GameObject m_addComponentRoot = null;
        [SerializeField]
        private AddComponentControl m_addComponentControl = null;

        private GameObjectEditorUtils.GameObjectWrapper[] m_selectedGameObjects;

        private IRuntimeEditor m_editor;
        private ISettingsComponent m_settingsComponent;
        private IRuntimeSelection m_selectionOverride;

        private GameObject SelectedGameObject
        {
            get { return SelectedObject as GameObject; }
        }

        private GameObject[] SelectedGameObjects
        {
            get
            {
                if (SelectedObjects == null)
                {
                    return null;
                }
                return SelectedObjects.OfType<GameObject>().ToArray();
            }
        }

        private UnityObject SelectedObject
        {
            get
            {
                if (m_selectionOverride == null || m_selectionOverride.activeObject == null)
                {
                    return m_editor.Selection.activeObject;
                }

                return m_selectionOverride.activeObject;
            }
        }

        private UnityObject[] SelectedObjects
        {
            get
            {
                if (m_selectionOverride == null || m_selectionOverride.activeObject == null)
                {
                    return m_editor.Selection.objects;
                }

                return m_selectionOverride.objects;
            }
        }

        private bool IsSelected(UnityObject obj)
        {
            if (m_selectionOverride == null || m_selectionOverride.activeObject == null)
            {
                return m_editor.Selection.IsSelected(obj);
            }

            return m_selectionOverride.IsSelected(obj);
        }

        private bool m_initOnStart = false;
        private void Awake()
        {
            m_settingsComponent = IOC.Resolve<ISettingsComponent>();
            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_editor.Object.ComponentAdded += OnComponentAdded;

            m_initOnStart = SelectedGameObjects == null;
            if(!m_initOnStart)
            {
                Init();
            }
        }

        private void Start()
        {
            RuntimeWindow window = GetComponentInParent<RuntimeWindow>();
            if(window != null)
            {
                m_selectionOverride = window.IOCContainer.Resolve<IRuntimeSelection>();
            }

            if(m_initOnStart)
            {
                Init();
            }
        }

        private void Init()
        {
            GameObject[] selectedObjects = SelectedGameObjects;
            InputName.text = GameObjectEditorUtils.GetObjectName(selectedObjects);
            InputName.onEndEdit.AddListener(OnEndEditName);

            m_selectedGameObjects = selectedObjects.Select(go => new GameObjectEditorUtils.GameObjectWrapper(go)).ToArray();
            IsActiveEditor.Init(m_selectedGameObjects, Strong.PropertyInfo((GameObjectEditorUtils.GameObjectWrapper x) => x.IsActive), string.Empty);

            m_editor.IsBusy = true;

            LayersEditor.LoadLayers(layersInfo =>
            {
                m_editor.IsBusy = false;
                if (SelectedGameObject == null)
                {
                    return;
                }

                InitLayersEditor(layersInfo);
                CreateComponentEditors(selectedObjects);
                InitAddComponentControl();
            });
        }

        private void OnDestroy()
        {
            if (InputName != null)
            {
                InputName.onEndEdit.RemoveListener(OnEndEditName);
            }

            if (m_editor != null)
            {
                if (m_editor.Object != null)
                {
                    m_editor.Object.ComponentAdded -= OnComponentAdded;
                }
            }

            UnityEventHelper.RemoveListener(EditLayersButton, btn => btn.onClick, OnEditLayersClick);

            if (m_addComponentControl != null)
            {
                m_addComponentControl.ComponentSelected -= OnAddComponent;
            }

            m_editor = null;
            m_settingsComponent = null;
            m_selectionOverride = null;
        }

        private void Update()
        {
            GameObject go = SelectedGameObject;
            if (go == null)
            {
                return;
            }

            UnityObject[] objects = SelectedObjects;
            if (objects[0] == null)
            {
                return;
            }

            if (InputName != null && !InputName.isFocused)
            {
                string objectName = GameObjectEditorUtils.GetObjectName(objects);
                if (InputName.text != objectName)
                {
                    InputName.text = objectName;
                }
            }
        }

        private void InitLayersEditor(LayersInfo layersInfo)
        {
            List<RangeOptions.Option> layers = new List<RangeOptions.Option>();

            foreach (LayersInfo.Layer layer in layersInfo.Layers)
            {
                if (!string.IsNullOrEmpty(layer.Name))
                {
                    layers.Add(new RangeOptions.Option(string.Format("{0}: {1}", layer.Index, layer.Name), layer.Index));
                }
            }

            LayerEditor.Options = layers.ToArray();
            LayerEditor.Init(SelectedGameObjects, Strong.PropertyInfo((GameObject x) => x.layer), string.Empty);
            UnityEventHelper.AddListener(EditLayersButton, btn => btn.onClick, OnEditLayersClick);

            bool showLayers = m_settingsComponent.BuiltInWindowsSettings.Inspector.GameObjectEditor.ShowLayers;
            if(LayersEditorRoot != null)
            {
                LayersEditorRoot.gameObject.SetActive(showLayers);
            }
        }

        private void CreateComponentEditors(GameObject[] selectedObjects)
        {
            List<List<Component>> groups = GameObjectEditorUtils.GetComponentGroups(selectedObjects);
            for (int i = 0; i < groups.Count; ++i)
            {
                List<Component> group = groups[i];
                GameObjectEditorUtils.CreateComponentEditor(ComponentsPanel, group);
            }
        }

        private void InitAddComponentControl()
        {
            ExposeToEditor exposeToEditor = SelectedGameObject.GetComponent<ExposeToEditor>();
            if (exposeToEditor && (m_settingsComponent == null || m_settingsComponent.BuiltInWindowsSettings.Inspector.ShowAddComponentButton))
            {
                IProjectAsync project = IOC.Resolve<IProjectAsync>();
                if (project == null || project.Utils.ToProjectItem(SelectedGameObject) == null)
                {
                    if (m_addComponentControl != null)
                    {
                        m_addComponentControl.ComponentSelected += OnAddComponent;
                    }
                }
            }
            else
            {
                if (m_addComponentRoot != null)
                {
                    m_addComponentRoot.SetActive(false);
                }
            }
        }

        [Obsolete] //13.04.2021
        public static List<List<Component>> GetComponentGroups(GameObject[] gameObjects)
        {
            return GameObjectEditorUtils.GetComponentGroups(gameObjects);
        }

        private void OnEndEditName(string name)
        {
            GameObjectEditorUtils.EndEditName(name, SelectedGameObjects);
        }

        private void OnAddComponent(Type type)
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            editor.Undo.BeginRecord();

            GameObject[] gameObjects = SelectedGameObjects;
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject go = gameObjects[i];
                ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
                editor.Undo.AddComponentWithRequirements(exposeToEditor, type);
            }

            editor.Undo.EndRecord();
        }

      
        private void OnComponentAdded(ExposeToEditor obj, Component component)
        {
            if (component == null)
            {
                IWindowManager wnd = IOC.Resolve<IWindowManager>();
                wnd.MessageBox("Unable to add component", "Component was not added");
            }
            else
            {
                if (!IsSelected(component.gameObject))
                {
                    return;
                }

                if (SelectedGameObject == null)
                {
                    return;
                }

                HashSet<Component> ignoreComponents = GameObjectEditorUtils.IgnoreComponents(obj.gameObject);
                if (!GameObjectEditorUtils.IsComponentValid(component, ignoreComponents))
                {
                    return;
                }

                GameObject[] gameObjects = SelectedGameObjects;
                if (gameObjects.Length == 1)
                {
                    GameObjectEditorUtils.CreateComponentEditor(ComponentsPanel, new List<Component> { component });
                }
                else
                {
                    if (gameObjects[gameObjects.Length - 1] != component.gameObject)
                    {
                        return;
                    }

                    List<List<Component>> groups = GameObjectEditorUtils.GetComponentGroups(gameObjects);
                    for (int i = 0; i < groups.Count; ++i)
                    {
                        List<Component> group = groups[i];

                        //This is to handle case when AddComponent called on multiple objects. 
                        //See InspectorView.cs void OnAddComponent(Type type) method for details.
                        if (group[group.Count - 1] == component)
                        {
                            GameObjectEditorUtils.CreateComponentEditor(ComponentsPanel, group);
                            break;
                        }
                    }
                }
            }
        }

        private void OnEditLayersClick()
        {
            LayersEditor.BeginEdit();
        }
    }
}

