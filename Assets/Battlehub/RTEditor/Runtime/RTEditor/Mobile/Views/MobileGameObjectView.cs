using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Battlehub.RTEditor.Mobile.Views
{
    public class MobileGameObjectView : MonoBehaviour
    {
        public UnityEvent SelectedGameObjectsChanged;

        private GameObject[] m_selectedObjects;
        public GameObject[] SelectedGameObjects
        {
            get { return m_selectedObjects; }
            set { m_selectedObjects = value; }
        }


        [SerializeField]
        private GameObject m_layersEditorRoot;
        [SerializeField]
        private OptionsEditor m_layersEditor;

        private LayersInfo m_layers;
        public LayersInfo Layers
        {
            get { return m_layers; }
            set
            {
                if(m_layers != value)
                {
                    m_layers = value;
                    RefreshPanel();
                }
            }
        }

        [SerializeField]
        private BoolEditor m_isActiveEditor;

        private IRTE m_editor;
        private void OnEnable()
        {
            m_editor = IOC.Resolve<IRTE>();

            Layers = LayersEditor.LoadedLayers;

            RefreshPanel();

            m_editor.Selection.SelectionChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            if (m_editor.Selection != null)
            {
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;
            }
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            RefreshPanel();
        }

        private void RefreshPanel()
        {
            SelectedGameObjects = m_editor.Selection.gameObjects;
            SelectedGameObjectsChanged?.Invoke();

            if (SelectedGameObjects != null && Layers != null)
            {
                var selectedGameObjects = SelectedGameObjects.Select(go => new GameObjectEditorUtils.GameObjectWrapper(go)).ToArray();

                if(selectedGameObjects.Length > 0)
                {
                    m_isActiveEditor.Init(selectedGameObjects, Strong.PropertyInfo((GameObjectEditorUtils.GameObjectWrapper x) => x.IsActive), string.Empty);

                    InitLayersEditor();
                }
            }
        }

        private void InitLayersEditor()
        {
            List<RangeOptions.Option> layers = new List<RangeOptions.Option>();

            foreach (LayersInfo.Layer layer in Layers.Layers)
            {
                if (!string.IsNullOrEmpty(layer.Name))
                {
                    layers.Add(new RangeOptions.Option(string.Format("{0}: {1}", layer.Index, layer.Name), layer.Index));
                }
            }

            m_layersEditor.Options = layers.ToArray();
            m_layersEditor.Init(SelectedGameObjects, Strong.PropertyInfo((GameObject x) => x.layer), string.Empty);

            ISettingsComponent settingsComponent = IOC.Resolve<ISettingsComponent>();
            bool showLayers = settingsComponent.BuiltInWindowsSettings.Inspector.GameObjectEditor.ShowLayers;
            if (m_layersEditorRoot != null)
            {
                m_layersEditorRoot.gameObject.SetActive(showLayers);
            }
        }
    }
}
