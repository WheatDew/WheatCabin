using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class LayersEditor : MonoBehaviour
    {
        [SerializeField]
        private Transform m_editorsPanel = null;
        [SerializeField]
        private GameObject m_editorPrefab = null;
        private LayersInfo m_layersInfo;
        private bool m_isDirty = false;
        private IRTE m_editor;
        
        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();

            m_layersInfo = (LayersInfo)m_editor.Selection.activeObject;

            foreach(LayersInfo.Layer layer in m_layersInfo.Layers)
            {
                GameObject editor = Instantiate(m_editorPrefab, m_editorsPanel, false);

                TextMeshProUGUI text = editor.GetComponentInChildren<TextMeshProUGUI>(true);
                if (text != null)
                {
                    text.text = layer.Index + ": ";
                }

                StringEditor stringEditor = editor.GetComponentInChildren<StringEditor>(true);
                if (stringEditor != null)
                {
                    if(layer.Index <= 5)
                    {
                        TMP_InputField inputField = stringEditor.GetComponentInChildren<TMP_InputField>(true);
                        inputField.selectionColor = new Color(0, 0, 0, 0);
                        inputField.readOnly = true;
                    }
                    
                    stringEditor.Init(layer, layer, Strong.MemberInfo((LayersInfo.Layer x) => x.Name), null, string.Empty, null, () => m_isDirty = true, null, false);
                }
            }
        }

        private void OnDestroy()
        {
            if(m_isDirty)
            {
                IRTE editor = IOC.Resolve<IRTE>();
                if(editor != null)
                {
                    EndEdit();
                }
            }
        }

        private void OnApplicationQuit()
        {
            m_isDirty = false;
        }

        private static string m_currentProject;

        private static LayersInfo m_loadedLayers;
        public static LayersInfo LoadedLayers
        {
            get { return m_loadedLayers; }
        }

        public static async void LoadLayers(Action<LayersInfo> callback)
        {
            IRTE editor = IOC.Resolve<IRTE>();
            await LoadLayersAsync(callback);
        }

        public static async void BeginEdit()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            await LoadLayersAsync(loadedLayers =>
            {
                editor.Selection.activeObject = loadedLayers;
            });
        }

        public static async Task LoadLayersAsync(Action<LayersInfo> callback)
        {
            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            using(await project.LockAsync())
            {
                if (m_loadedLayers == null || project.State.ProjectInfo.Name != m_currentProject)
                {
                    m_currentProject = project.State.ProjectInfo.Name;

                    RuntimeTextAsset layersInfo = null;
                    try
                    {
                        layersInfo = await project.GetValueAsync<RuntimeTextAsset>("Battlehub.RTEditor.LayersInfo");
                    }
                    catch(StorageException e)
                    {
                        if(e.ErrorCode != Error.E_NotFound)
                        {
                            throw;
                        }
                    }

                    if (layersInfo == null)
                    {
                        IRTE rte = IOC.Resolve<IRTE>();
                        int layersMask = rte.CameraLayerSettings.RaycastMask & ~(1 << rte.CameraLayerSettings.UIBackgroundLayer);

                        m_loadedLayers = ScriptableObject.CreateInstance<LayersInfo>();

                        m_loadedLayers.Layers = new List<LayersInfo.Layer>
                        {
                            new LayersInfo.Layer("Default", 0),
                            new LayersInfo.Layer("Transparent FX", 1),
                            new LayersInfo.Layer("Ignore Raycast", 2),
                            new LayersInfo.Layer("Water", 4),
                        };

                        for (int i = 8; i < 32; ++i)
                        {
                            if((layersMask & (1 << i)) != 0)
                            {
                                m_loadedLayers.Layers.Add(new LayersInfo.Layer(i == 10 ? "UI" : LayerMask.LayerToName(i), i));
                            }
                        }

                        RuntimeTextAsset layersTextAsset = ScriptableObject.CreateInstance<RuntimeTextAsset>();
                        layersTextAsset.Text = JsonUtility.ToJson(m_loadedLayers);

                        await project.SetValueAsync("Battlehub.RTEditor.LayersInfo", layersTextAsset);
                    }
                    else
                    {
                        m_loadedLayers = ScriptableObject.CreateInstance<LayersInfo>();
                        JsonUtility.FromJsonOverwrite(layersInfo.Text, m_loadedLayers);

                        foreach (LayersInfo.Layer layer in m_loadedLayers.Layers)
                        {
                            if (string.IsNullOrEmpty(layer.Name))
                            {
                                layer.Name = LayerMask.LayerToName(layer.Index);
                            }
                        }
                    }
                }
            }
            callback(m_loadedLayers);
        }

        private async void EndEdit()
        {
            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            if(project == null)
            {
                return;
            }

            RuntimeTextAsset layersTextAsset = ScriptableObject.CreateInstance<RuntimeTextAsset>();
            layersTextAsset.Text = JsonUtility.ToJson(m_layersInfo);
            
            await project.Safe.SetValueAsync("Battlehub.RTEditor.LayersInfo", layersTextAsset);
        }
    }
}
