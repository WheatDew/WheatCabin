using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTHandles;
using Battlehub.UIControls;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTBuilder
{
    public class ManualUVEditorView : RuntimeCameraWindow
    {
        [SerializeField]
        private Renderer m_texturePreview = null;

        [SerializeField]
        private TMP_Dropdown m_texturesDropDown = null;
        private Texture m_lastSelectedTexture;
        private Texture[] m_textures;

        private IProBuilderTool m_tool;
        private IManualUVEditor m_uvEditor;
        private IMaterialEditor m_materialEditor;
        private IMaterialPaletteManager m_paletteManager;
        private List<Transform> m_extraComponents;
 
        private IRTECamera m_rteCamera;

        protected override void AwakeOverride()
        {
            RenderTextureUsage = RenderTextureUsage.On;
            
            base.AwakeOverride();
            CanvasGroup.alpha = 0;

            m_tool = IOC.Resolve<IProBuilderTool>();
            m_tool.SelectionChanged += OnProBuilderToolSelectionChanged;
            m_uvEditor = IOC.Resolve<IManualUVEditor>();
            m_extraComponents = new List<Transform>();

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            Transform[] children = transform.OfType<Transform>().ToArray();
            for (int i = 0; i < children.Length; ++i)
            {
                Transform component = children[i];
                if (!(component is RectTransform))
                {
                    component.gameObject.SetActive(false);
                    component.transform.SetParent(wm.ComponentsRoot, false);

                    m_extraComponents.Add(component);
                }
            }
            
            m_texturesDropDown.gameObject.SetActive(false);
            UnityEventHelper.AddListener(m_texturesDropDown, dd => dd.onValueChanged, OnTextureChanged);
        }

        protected virtual void Start()
        {
            if(transform.parent != null)
            {
                RuntimeWindow parent = transform.parent.GetComponentInParent<RuntimeWindow>();
                if(parent != null)
                {
                    Depth = parent.Depth + 1;
                }
            }

            //m_texturePreview.transform.position = new Vector3(ManualUVRenderer.Scale, ManualUVRenderer.Scale, 2) * 0.5f;
            m_texturePreview.transform.rotation = Quaternion.Euler(90, 180, 0);
            m_texturePreview.transform.localScale = Vector3.one * 10;
            m_texturePreview.sharedMaterial.mainTextureScale = Vector2.one * 10;
            m_rteCamera = GetGraphicsCamera();
            m_rteCamera.RenderersCache.Add(m_texturePreview);
       
            m_materialEditor = IOC.Resolve<IMaterialEditor>();
            m_materialEditor.MaterialsApplied += OnMaterialApplied;

            m_paletteManager = IOC.Resolve<IMaterialPaletteManager>();
            m_paletteManager.PaletteChanged += OnPaletteChanged;
            m_paletteManager.MaterialAdded += OnMaterialsChanged;
            m_paletteManager.MaterialRemoved += OnMaterialsChanged;
            m_paletteManager.MaterialCreated += OnMaterialsChanged;
            m_paletteManager.MaterialReplaced += OnMaterialReplaced;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
           
            SetComponentsActive(true);

            IRuntimeSceneComponent scene = IOCContainer.Resolve<IRuntimeSceneComponent>();
            scene.CameraPosition = new Vector3(0.5f, 0.5f, -1) * ManualUVRenderer.Scale;
            scene.Pivot = new Vector3(0.5f, 0.5f, 0) * ManualUVRenderer.Scale;
            scene.IsSelectionVisible = false;
            scene.IsOrthographic = true;
            scene.CanRotate = false;
            scene.CanFreeMove = false;
            scene.CanSelect = false;
            scene.CanSelectAll = false;
           // scene.ChangeOrthographicSizeOnly = true;

            if (scene.Selection != m_uvEditor.PivotPointSelection)
            {
                scene.Selection = m_uvEditor.PivotPointSelection;
                m_uvEditor.PivotPointSelection.activeObject = null;
            }
            
            IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
            Camera.backgroundColor = appearance.Colors.Secondary;

            IRenderPipelineCameraUtility cameraUtil = IOC.Resolve<IRenderPipelineCameraUtility>();
            if (cameraUtil != null)
            {
                cameraUtil.ResetCullingMask(Camera);
            }
            else
            {
                Camera.cullingMask = 0;
            }
            
            Camera.clearFlags = CameraClearFlags.SolidColor;

            EnableRaycasts();
            Editor.StartCoroutine(CoSetAlpha());
        }

        private IEnumerator CoSetAlpha()
        {
            yield return new WaitForEndOfFrame();
            CanvasGroup.alpha = 1;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            SetComponentsActive(false);
            CanvasGroup.alpha = 0;
        }

        protected override void OnPointerEnterOverride(PointerEventData eventData)
        {
            base.OnPointerEnterOverride(eventData);

            //Box Selection disappearing fix
            IRuntimeSceneComponent scene = IOCContainer.Resolve<IRuntimeSceneComponent>();
            Canvas canvas = scene.BoxSelection.GetComponentInParent<Canvas>();
            canvas.enabled = false;
            canvas.enabled = true;

        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if(m_tool != null)
            {
                m_tool.SelectionChanged -= OnProBuilderToolSelectionChanged;
            }

            if(m_materialEditor != null)
            {
                m_materialEditor.MaterialsApplied -= OnMaterialApplied;
            }

            if(m_paletteManager != null)
            {
                m_paletteManager.PaletteChanged -= OnPaletteChanged;
                m_paletteManager.MaterialAdded -= OnMaterialsChanged;
                m_paletteManager.MaterialReplaced -= OnMaterialReplaced;
                m_paletteManager.MaterialCreated -= OnMaterialsChanged;
                m_paletteManager.MaterialRemoved -= OnMaterialsChanged;   
            }

            for (int i = 0; i < m_extraComponents.Count; ++i)
            {
                if (m_extraComponents[i] != null)
                {
                    Destroy(m_extraComponents[i].gameObject);
                }
            }

            UnityEventHelper.RemoveListener(m_texturesDropDown, dd => dd.onValueChanged, OnTextureChanged);
            if(m_rteCamera != null && m_rteCamera.RenderersCache != null)
            {
                m_rteCamera.RenderersCache.Remove(m_texturePreview);
            }
        }

        private void SetComponentsActive(bool active)
        {
            for (int i = 0; i < m_extraComponents.Count; ++i)
            {
                if (m_extraComponents[i] != null)
                {
                    m_extraComponents[i].gameObject.SetActive(active);
                }
            }
        }

        private void OnProBuilderToolSelectionChanged()
        {
            UpdateTexturesDrowDown();
        }

        private void OnTextureChanged(int index)
        {
            m_lastSelectedTexture = m_textures[index];
            m_texturePreview.material.MainTexture(m_textures[index]);
        }

        private void OnMaterialsChanged(Material obj)
        {
            UpdateTexturesDrowDown();
        }

        private void OnMaterialReplaced(Material oldMaterial, Material newMaterial)
        {
            UpdateTexturesDrowDown();
        }

        private void OnPaletteChanged(MaterialPalette obj)
        {
            UpdateTexturesDrowDown();
        }

        private void OnMaterialApplied()
        {
            UpdateTexturesDrowDown();
        }

        private void UpdateTexturesDrowDown()
        {
            //HashSet<PBMesh> selectedMeshes = new HashSet<PBMesh>();
            IMeshEditor meshEditor = m_tool.GetEditor();
            HashSet<Texture> textures = new HashSet<Texture>();
            if (meshEditor != null)
            {
                MeshSelection selection = meshEditor.GetSelection();
                if (selection != null)
                {
                    foreach (PBMesh mesh in selection.GetSelectedMeshes())
                    {
                        Renderer renderer = mesh.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            foreach (Material material in renderer.sharedMaterials)
                            {
                                if (material != null && material.MainTexture() != null && !textures.Contains(material.MainTexture()))
                                {
                                    textures.Add(material.MainTexture());
                                }
                            }
                        }
                    }
                }
            }

            m_textures = textures.ToArray();
            m_texturesDropDown.ClearOptions();

            if (m_textures.Length == 0)
            {
                m_texturesDropDown.gameObject.SetActive(false);
                m_texturePreview.material.MainTexture(null);
                return;
            }

            m_texturesDropDown.gameObject.SetActive(true);

            List<string> options = new List<string>();
            int selectedIndex = -1;
            for (int i = 0; i < m_textures.Length; ++i)
            {
                Texture texture = m_textures[i];
                if (texture == m_lastSelectedTexture)
                {
                    selectedIndex = i;
                }

                options.Add(i + ". " + (string.IsNullOrEmpty(texture.name) ? "Texture" : texture.name));
            }
            m_texturesDropDown.AddOptions(options);

            if (selectedIndex == -1)
            {
                selectedIndex = 0;
            }

            m_texturePreview.material.MainTexture(m_textures[selectedIndex]);
            m_texturesDropDown.SetValueWithoutNotify(selectedIndex);
        }
    }
}
